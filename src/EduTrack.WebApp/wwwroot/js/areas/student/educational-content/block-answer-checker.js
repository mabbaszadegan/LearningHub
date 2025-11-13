/**
 * Generic Block Answer Checker
 * Handles answer submission and result display for all schedule item types
 */

(function() {
    'use strict';

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function() {
        initializeBlockAnswerChecker();
    });

    function initializeBlockAnswerChecker() {
        // Attach click handlers to all check answer buttons (both old and new minimal style)
        const checkButtons = document.querySelectorAll('.btn-check-answer, .btn-check-answer-minimal');
        checkButtons.forEach(function(button) {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                handleCheckAnswer(button);
            });
        });
        
        // Load answer history for all blocks on page load
        loadAnswerHistory();
    }

    function handleCheckAnswer(button) {
        const scheduleItemId = button.getAttribute('data-schedule-item-id');
        const scheduleItemType = button.getAttribute('data-schedule-item-type');
        let blockId = button.getAttribute('data-block-id') || '';
        const blockOrder = button.getAttribute('data-block-order');

        if (!blockId || blockId.trim() === '') {
            const contextCard = button.closest('[data-block-id]');
            if (contextCard && contextCard.getAttribute('data-block-id')) {
                blockId = contextCard.getAttribute('data-block-id');
            }
        }

        if ((!blockId || blockId.trim() === '') && scheduleItemType) {
            const inferredContext = getBlockContext(button, scheduleItemType);
            if (inferredContext === 'multipleChoice') {
                blockId = 'main';
            } else if (inferredContext === 'ordering') {
                blockId = 'main';
            }
        }

        if (!scheduleItemId || !blockId) {
            console.error('Missing required data attributes');
            return;
        }

        // Disable button during submission
        button.disabled = true;
        const isMinimal = button.classList.contains('btn-check-answer-minimal');
        const originalContent = button.innerHTML;
        
        if (isMinimal) {
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
        } else {
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>در حال بررسی...</span>';
        }

        // Determine block context (ordering, multiple choice, etc.)
        const blockContext = getBlockContext(button, scheduleItemType);
        const isMatchingBlock = blockContext === 'matching';

        if (!blockContext) {
            console.error('Unsupported or unknown block context for check answer button.', { scheduleItemType, blockId });
            showError(button, 'نوع این بلاک پشتیبانی نمی‌شود.');
            button.disabled = false;
            button.innerHTML = originalContent;
            return;
        }

        // Get submitted answer based on block context
        const submittedAnswer = getSubmittedAnswer(button, blockContext, blockId);

        if (!submittedAnswer) {
            let emptyMessage = 'لطفاً ابتدا پاسخ خود را تکمیل کنید.';
            if (blockContext === 'multipleChoice') {
                emptyMessage = 'لطفاً ابتدا یک گزینه را انتخاب کنید.';
            } else if (blockContext === 'ordering') {
                emptyMessage = 'لطفاً ابتدا پاسخ را وارد کنید. حداقل یک آیتم باید در جایگاه قرار گیرد.';
            } else if (blockContext === 'matching') {
                emptyMessage = 'لطفاً برای تمام آیتم‌ها تطبیق مناسب را انتخاب کنید.';
            } else if (blockContext === 'gapFill') {
                emptyMessage = 'لطفاً تمام جای خالی‌ها را تکمیل کنید.';
            }

            showError(button, emptyMessage);
            button.disabled = false;
            button.innerHTML = originalContent;
            if (isMatchingBlock) {
                dispatchMatchingChecked(button, { reason: 'incomplete' });
            }
            return;
        }

        // Validate that submitted answer has content
        if (blockContext === 'ordering') {
            if (!submittedAnswer.order || submittedAnswer.order.length === 0) {
                showError(button, 'لطفاً ابتدا پاسخ را وارد کنید. حداقل یک آیتم باید در جایگاه قرار گیرد.');
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }
        }
        
        if (blockContext === 'multipleChoice') {
            if (!submittedAnswer.selectedOptions || submittedAnswer.selectedOptions.length === 0) {
                showError(button, 'لطفاً ابتدا پاسخ را انتخاب کنید.');
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }
        }

        if (blockContext === 'matching') {
            if (!submittedAnswer.matches || submittedAnswer.matches.length === 0) {
                showError(button, 'لطفاً برای تمام آیتم‌ها تطبیق مناسب را انتخاب کنید.');
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }

            const hasIncomplete = submittedAnswer.matches.some(match =>
                !match ||
                !match.leftItemId ||
                !match.selectedPairId ||
                !match.selectedPairId.trim()
            );

            if (hasIncomplete) {
                showError(button, 'برای هر مورد ستون چپ یک گزینه معتبر انتخاب کنید.');
                button.disabled = false;
                button.innerHTML = originalContent;
                dispatchMatchingChecked(button, { reason: 'incomplete' });
                return;
            }
        }

        if (blockContext === 'gapFill') {
            if (!submittedAnswer.blanks || !Array.isArray(submittedAnswer.blanks) || submittedAnswer.blanks.length === 0) {
                showError(button, 'لطفاً تمام جای خالی‌ها را تکمیل کنید.');
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }
        }

        // Submit answer
        fetch('/Student/ScheduleItem/SubmitBlockAnswer', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({
                scheduleItemId: parseInt(scheduleItemId),
                blockId: blockId,
                submittedAnswer: submittedAnswer
            })
        })
        .then(function(response) {
            if (!response.ok) {
                return response.json().then(function(data) {
                    throw new Error(data.error || 'خطا در بررسی پاسخ');
                });
            }
            return response.json();
        })
        .then(function(result) {
            showResult(button, result, blockId);
            button.disabled = false;
            updateButtonIcon(button, result.isCorrect);
            // Reload history after answer check
            loadAnswerHistoryForBlock(scheduleItemId, blockId);
            button.innerHTML = originalContent;
            if (isMatchingBlock) {
                dispatchMatchingChecked(button, { isCorrect: !!result && !!result.isCorrect, reason: 'checked' });
            }
        })
        .catch(function(error) {
            console.error('Error checking answer:', error);
            showError(button, error.message || 'خطا در بررسی پاسخ');
            button.disabled = false;
            button.innerHTML = originalContent;
            if (isMatchingBlock) {
                dispatchMatchingChecked(button, { error: true, reason: 'error' });
            }
        });
    }

    function getSubmittedAnswer(button, blockContext, blockId) {
        // Ordering block
        if (blockContext === 'ordering') {
            return getOrderingAnswer(blockId, button);
        }
        
        // MultipleChoice block
        if (blockContext === 'multipleChoice') {
            return getMultipleChoiceAnswer(blockId, button);
        }

        // Matching block
        if (blockContext === 'matching') {
            return getMatchingAnswer(blockId, button);
        }

        if (blockContext === 'gapFill') {
            return getGapFillAnswer(blockId, button);
        }

        // Add other types as needed
        // For now, return null for unsupported types
        return null;
    }

    function getBlockContext(button, scheduleItemType) {
        if (isOrderingBlock(button, scheduleItemType)) {
            return 'ordering';
        }

        if (isMultipleChoiceBlock(button, scheduleItemType)) {
            return 'multipleChoice';
        }

        if (isMatchingBlock(button, scheduleItemType)) {
            return 'matching';
        }

        if (isGapFillBlock(button, scheduleItemType)) {
            return 'gapFill';
        }

        return null;
    }

    function getOrderingAnswer(blockId, triggerButton) {
        // Find the sorting block for this blockId
        let blockCard = triggerButton ? triggerButton.closest('.ordering-block-card') : null;
        
        if (!blockCard) {
            // Try to find by block order
            const blockCards = document.querySelectorAll('.ordering-block-card');
            const blockOrder = parseInt(blockId) || 0;
            if (blockCards[blockOrder]) {
                blockCard = blockCards[blockOrder];
            } else {
                // Last resort: find first block card
                blockCard = blockCards[0];
            }
        }
        
        if (!blockCard) {
            return null;
        }

        const slots = blockCard.querySelector('.sorting-slots');
        if (!slots) return null;

        const dropzones = slots.querySelectorAll('.slot-dropzone');
        const order = [];

        // Get order from dropzones - only include items that are actually in slots
        // This ensures we capture the exact order as displayed
        dropzones.forEach(function(dropzone) {
            const item = dropzone.querySelector('.sorting-item');
            if (item) {
                const itemId = item.getAttribute('data-id');
                if (itemId && itemId.trim() !== '') {
                    order.push(itemId.trim());
                }
            }
        });

        // Validate that we have at least one item
        if (order.length === 0) {
            console.warn('No items found in slots for block:', blockId);
            return null; // Return null to indicate no answer provided
        }

        return {
            order: order
        };
    }

    function getMultipleChoiceAnswer(blockId, triggerButton) {
        // Find the multiple choice block card
        let blockCard = triggerButton ? triggerButton.closest('.mcq-block-card') : null;
        
        if (!blockCard) {
            // Try to find by block order or first block
            const blockCards = document.querySelectorAll('.mcq-block-card');
            if (blockCards.length > 0) {
                blockCard = blockCards[0];
            }
        }
        
        if (!blockCard) {
            // Fallback: try to find by data-block-id attribute
            blockCard = document.querySelector(`.mcq-block-card[data-block-id="${blockId}"]`);
        }
        
        if (!blockCard) {
            return null;
        }

        // Find the options container for this specific block
        const optionsContainer = blockCard.querySelector(`.mcq-options[data-block-id="${blockId}"]`);
        if (!optionsContainer) {
            return null;
        }

        const selectedOptions = [];
        const checkboxes = optionsContainer.querySelectorAll('input[type="checkbox"]:checked');
        const radios = optionsContainer.querySelectorAll('input[type="radio"]:checked');

        if (checkboxes.length > 0) {
            checkboxes.forEach(function(checkbox) {
                const optionIndex = parseInt(checkbox.value);
                if (!isNaN(optionIndex)) {
                    selectedOptions.push(optionIndex);
                }
            });
        } else if (radios.length > 0) {
            radios.forEach(function(radio) {
                const optionIndex = parseInt(radio.value);
                if (!isNaN(optionIndex)) {
                    selectedOptions.push(optionIndex);
                }
            });
        }

        // Validate that we have at least one selected option
        if (selectedOptions.length === 0) {
            return null; // Return null to indicate no answer provided
        }

        return {
            selectedOptions: selectedOptions
        };
    }

    function getMatchingAnswer(blockId, triggerButton) {
        let blockCard = triggerButton ? triggerButton.closest('.matching-block-card') : null;

        if (!blockCard) {
            blockCard = document.querySelector(`.matching-block-card[data-block-id="${blockId}"]`);
        }

        if (!blockCard) {
            const cards = document.querySelectorAll('.matching-block-card');
            if (cards.length > 0) {
                blockCard = cards[0];
            }
        }

        if (!blockCard) {
            return null;
        }

        const selects = blockCard.querySelectorAll('.matching-select-input');
        if (!selects.length) {
            return null;
        }

        const matches = Array.from(selects).map((select, index) => {
            const leftItemId = select.getAttribute('data-left-id') || '';
            const selectedPairId = select.value || '';
            return {
                leftItemId: leftItemId,
                selectedPairId: selectedPairId.trim(),
                orderIndex: index
            };
        });

        const hasAnySelection = matches.some(match => match.selectedPairId.length > 0);
        if (!hasAnySelection) {
            return null;
        }

        return {
            matches: matches
        };
    }

    function getGapFillAnswer(blockId, triggerButton) {
        let blockCard = triggerButton ? triggerButton.closest('.gapfill-block-card') : null;

        if (!blockCard) {
            blockCard = document.querySelector(`.gapfill-block-card[data-block-id="${blockId}"]`);
        }

        if (!blockCard) {
            return null;
        }

        const blanks = Array.from(blockCard.querySelectorAll('.gapfill-blank'));
        if (!blanks.length) {
            return null;
        }

        const answers = [];
        let hasIncomplete = false;

        blanks.forEach(function(blank) {
            blank.classList.remove('is-incomplete');

            const input = blank.querySelector('.gapfill-input');
            const value = input ? input.value.trim() : '';
            const optionId = (blank.dataset.selectedOptionId || '').trim();
            const blankIndex = parseInt(blank.dataset.blankIndex || '', 10);
            const blankId = blank.dataset.blankId || (Number.isFinite(blankIndex) ? `blank${blankIndex}` : '');
            const answered = value.length > 0 || optionId.length > 0;

            if (!answered) {
                hasIncomplete = true;
                blank.classList.add('is-incomplete');
            }

            answers.push({
                blankId: blankId,
                index: Number.isFinite(blankIndex) ? blankIndex : undefined,
                value: value,
                optionId: optionId
            });
        });

        if (hasIncomplete) {
            return null;
        }

        return {
            blanks: answers
        };
    }

    function isOrderingBlock(button, scheduleItemType) {
        if (scheduleItemType === 'Ordering' || scheduleItemType === '8') {
            return true;
        }

        if (button && button.closest('.ordering-block-card')) {
            return true;
        }

        const blockWrapper = button ? button.closest('[data-block-type]') : null;
        if (blockWrapper) {
            const typeString = (blockWrapper.getAttribute('data-block-type') || '').toLowerCase();
            if (typeString === 'ordering') {
                return true;
            }
        }

        const legacyWrapper = button ? button.closest('.content-block-ordering, .ordering-block') : null;
        if (legacyWrapper) {
            return true;
        }

        return false;
    }

    function isMultipleChoiceBlock(button, scheduleItemType) {
        if (scheduleItemType === 'MultipleChoice' || scheduleItemType === '4') {
            return true;
        }

        if (button && button.closest('.mcq-block-card')) {
            return true;
        }

        const blockWrapper = button ? button.closest('[data-block-type]') : null;
        if (blockWrapper) {
            const typeString = (blockWrapper.getAttribute('data-block-type') || '').toLowerCase();
            if (typeString === 'multiplechoice' || typeString === 'questionmultiplechoice') {
                return true;
            }
        }

        const legacyWrapper = button ? button.closest('.content-block-multiplechoice, .multiple-choice-block') : null;
        if (legacyWrapper) {
            return true;
        }

        return false;
    }

    function isMatchingBlock(button, scheduleItemType) {
        if (scheduleItemType === 'Match' || scheduleItemType === '5') {
            return true;
        }

        if (button && button.closest('.matching-block-card')) {
            return true;
        }

        const blockWrapper = button ? button.closest('[data-block-type]') : null;
        if (blockWrapper) {
            const typeString = (blockWrapper.getAttribute('data-block-type') || '').toLowerCase();
            if (typeString === 'matching') {
                return true;
            }
        }

        return false;
    }

    function isGapFillBlock(button, scheduleItemType) {
        if (scheduleItemType === 'GapFill' || scheduleItemType === '3') {
            return true;
        }

        if (button && button.closest('.gapfill-block-card')) {
            return true;
        }

        const blockWrapper = button ? button.closest('[data-block-type]') : null;
        if (blockWrapper) {
            const typeString = (blockWrapper.getAttribute('data-block-type') || '').toLowerCase();
            if (typeString === 'gapfill') {
                return true;
            }
        }

        const legacyWrapper = button ? button.closest('.gap-fill-content-container, .gap-fill-text') : null;
        if (legacyWrapper) {
            return true;
        }

        return false;
    }

    function showResult(button, result, blockId) {
        // For minimal button, we just update the icon - no need for result container
        const isMinimal = button.classList.contains('btn-check-answer-minimal');
        applyGapFillFeedback(button, result);
        
        if (!isMinimal) {
            // Find result container for old style buttons
            const blockCard = button.closest('.ordering-block-card');
            if (!blockCard) return;

            const resultContainer = blockCard.querySelector(`.block-result[data-block-id="${blockId}"]`);
            if (!resultContainer) return;

            const resultContent = resultContainer.querySelector('.result-content');
            if (!resultContent) return;

            // Clear previous result
            resultContent.innerHTML = '';

            // Add result class
            resultContainer.classList.remove('correct', 'incorrect');
            resultContainer.classList.add(result.isCorrect ? 'correct' : 'incorrect');

            // Build result HTML
            const icon = result.isCorrect 
                ? '<i class="fas fa-check-circle result-icon correct"></i>'
                : '<i class="fas fa-times-circle result-icon incorrect"></i>';

            const message = result.isCorrect 
                ? 'عالی! پاسخ شما صحیح است.'
                : (result.feedback || 'متأسفانه پاسخ شما صحیح نیست. لطفاً دوباره تلاش کنید.');

            const points = `امتیاز: ${result.pointsEarned}/${result.maxPoints}`;

            resultContent.innerHTML = `
                ${icon}
                <div class="result-message">${message}</div>
                <div class="result-points">${points}</div>
            `;

            // Show result
            resultContainer.style.display = 'block';

            // Scroll to result if needed
            resultContainer.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    }

    function applyGapFillFeedback(button, result) {
        if (!button || !result) {
            return false;
        }

        const card = button.closest('.gapfill-block-card');
        if (!card) {
            return false;
        }

        const blanks = Array.from(card.querySelectorAll('.gapfill-blank'));
        if (blanks.length === 0) {
            return false;
        }

        blanks.forEach(blank => {
            blank.classList.remove('is-correct', 'is-incorrect');
        });

        card.querySelectorAll('.gapfill-option').forEach(option => {
            option.classList.remove('is-crossed');
        });

        const detailed = result.detailedFeedback || result.DetailedFeedback;
        const perBlank = detailed && (detailed.blanks || detailed.Blanks);
        if (!Array.isArray(perBlank)) {
            return true;
        }

        perBlank.forEach(entry => {
            if (!entry) {
                return;
            }

            const blankIdRaw = entry.blankId || entry.BlankId || '';
            const index = entry.index ?? entry.Index;
            const selectorId = (blankIdRaw && blankIdRaw.toString().trim().length > 0)
                ? blankIdRaw.toString().trim()
                : (Number.isFinite(index) ? `blank${index}` : '');

            const blank = selectorId
                ? card.querySelector(`.gapfill-blank[data-blank-id="${escapeSelector(selectorId)}"]`)
                : null;

            if (!blank) {
                return;
            }

            const isCorrect = !!(entry.isCorrect ?? entry.IsCorrect);
            blank.classList.toggle('is-correct', isCorrect);
            blank.classList.toggle('is-incorrect', !isCorrect);

            if (!isCorrect) {
                const optionId = entry.submittedOptionId || entry.SubmittedOptionId || '';
                if (optionId) {
                    const optionSelector = `.gapfill-option[data-option-id="${escapeSelector(optionId)}"]`;
                    const optionButton = blank.querySelector(optionSelector) ||
                        card.querySelector(`.gapfill-global-options ${optionSelector}`);
                    if (optionButton) {
                        optionButton.classList.add('is-crossed');
                    }
                }
            }
        });

        return true;
    }

    function escapeSelector(value) {
        if (typeof value !== 'string') {
            value = `${value ?? ''}`;
        }
        if (typeof CSS !== 'undefined' && CSS.escape) {
            return CSS.escape(value);
        }
        return value.replace(/([!"#$%&'()*+,.\/:;<=>?@\[\\\]^`{|}~\s])/g, '\\$1');
    }
    
    function updateButtonIcon(button, isCorrect) {
        const isMinimal = button.classList.contains('btn-check-answer-minimal');
        
        if (isMinimal) {
            // Remove previous state classes
            button.classList.remove('correct', 'incorrect');
            
            // Add new state class
            button.classList.add(isCorrect ? 'correct' : 'incorrect');
            
            // Update icon
            const icon = button.querySelector('i');
            if (icon) {
                if (isCorrect) {
                    icon.className = 'fas fa-check';
                } else {
                    icon.className = 'fas fa-times';
                }
            }
        }
    }

    function showError(button, message) {
        // Find result container - try both ordering and MCQ block cards
        const blockCard = button.closest('.ordering-block-card') ||
            button.closest('.mcq-block-card') ||
            button.closest('.matching-block-card');
        if (!blockCard) {
            // For minimal buttons, just show a simple alert
            alert(message);
            return;
        }

        const blockId = button.getAttribute('data-block-id');
        const resultContainer = blockCard.querySelector(`.block-result[data-block-id="${blockId}"]`);
        if (!resultContainer) {
            // For minimal buttons without result container, show alert
            alert(message);
            return;
        }

        const resultContent = resultContainer.querySelector('.result-content');
        if (!resultContent) {
            alert(message);
            return;
        }

        resultContainer.classList.remove('correct', 'incorrect');
        resultContainer.classList.add('incorrect');

        resultContent.innerHTML = `
            <i class="fas fa-exclamation-triangle result-icon incorrect"></i>
            <div class="result-message">${message}</div>
        `;

        resultContainer.style.display = 'block';
    }

    function getAntiForgeryToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }
    
    function loadAnswerHistory() {
        // Get all ordering blocks, MCQ blocks, and gap fill blocks on the page
        const orderingBlocks = document.querySelectorAll('.ordering-block-card');
        const mcqBlocks = document.querySelectorAll('.mcq-block-card');
        const gapfillBlocks = document.querySelectorAll('.gapfill-block-card');
        
        if (orderingBlocks.length === 0 && mcqBlocks.length === 0 && gapfillBlocks.length === 0) return;
        
        // Get schedule item ID from first button
        const firstButton = document.querySelector('.btn-check-answer-minimal, .btn-check-answer');
        if (!firstButton) return;
        
        const scheduleItemId = firstButton.getAttribute('data-schedule-item-id');
        if (!scheduleItemId) return;
        
        const loadHistoryForCard = function(blockCard) {
            const button = blockCard.querySelector('.btn-check-answer-minimal, .btn-check-answer');
            if (!button) return;

            const blockId = button.getAttribute('data-block-id');
            if (!blockId) return;

            loadAnswerHistoryForBlock(scheduleItemId, blockId);
        };

        // Load history for all ordering blocks
        orderingBlocks.forEach(loadHistoryForCard);

        // Load history for all MCQ blocks
        mcqBlocks.forEach(loadHistoryForCard);

        // Load history for all gap fill blocks
        gapfillBlocks.forEach(loadHistoryForCard);
    }
    
    function loadAnswerHistoryForBlock(scheduleItemId, blockId) {
        fetch(`/Student/ScheduleItem/GetBlockStatistics?scheduleItemId=${scheduleItemId}&blockId=${blockId}`, {
            method: 'GET',
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            }
        })
        .then(function(response) {
            if (!response.ok) {
                return response.json().then(function(data) {
                    throw new Error(data.error || 'خطا در دریافت تاریخچه');
                });
            }
            return response.json();
        })
        .then(function(data) {
            if (!data) {
                return;
            }

            // Handle new profile-aware response wrapper
            if (typeof data === 'object' && !Array.isArray(data)) {
                if (data.requiresProfile) {
                    // Profile not selected; skip updating history silently
                    return;
                }

                if (data.success && Array.isArray(data.data)) {
                    data = data.data;
                } else if (data.success && data.value && Array.isArray(data.value)) {
                    data = data.value;
                }
            }

            if (Array.isArray(data) && data.length > 0) {
                const stats = data[0];
                updateAnswerHistory(blockId, stats.correctAttempts || 0, stats.incorrectAttempts || 0);
            }
        })
        .catch(function(error) {
            console.error('Error loading answer history:', error);
            // Silently fail - history is optional
        });
    }
    
    function updateAnswerHistory(blockId, correctCount, incorrectCount) {
        const historyElement = document.querySelector(`.block-answer-history[data-block-id="${blockId}"]`);
        if (!historyElement) return;
        
        const correctSpan = historyElement.querySelector('.history-correct');
        const incorrectSpan = historyElement.querySelector('.history-incorrect');
        
        if (correctSpan) {
            correctSpan.textContent = correctCount;
            correctSpan.setAttribute('data-count', correctCount);
        }
        
        if (incorrectSpan) {
            incorrectSpan.textContent = incorrectCount;
            incorrectSpan.setAttribute('data-count', incorrectCount);
        }
    }

    function dispatchMatchingChecked(button, detail) {
        if (!button) {
            return;
        }

        const card = button.closest('.matching-block-card');
        if (!card) {
            return;
        }

        const eventDetail = detail || {};
        card.dispatchEvent(new CustomEvent('matching:block:checked', {
            bubbles: true,
            detail: eventDetail
        }));
    }

    // Export for use in other scripts if needed
    window.BlockAnswerChecker = {
        initialize: initializeBlockAnswerChecker,
        checkAnswer: handleCheckAnswer,
        loadHistory: loadAnswerHistory
    };
})();

