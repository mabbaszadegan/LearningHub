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
        const blockId = button.getAttribute('data-block-id');
        const blockOrder = button.getAttribute('data-block-order');

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

        // Get submitted answer based on schedule item type
        const submittedAnswer = getSubmittedAnswer(scheduleItemType, blockId);

        if (!submittedAnswer) {
            showError(button, 'لطفاً ابتدا پاسخ را وارد کنید. حداقل یک آیتم باید در جایگاه قرار گیرد.');
            button.disabled = false;
            button.innerHTML = originalContent;
            return;
        }

        // Validate that submitted answer has content
        if (scheduleItemType === 'Ordering' || scheduleItemType === '8') {
            if (!submittedAnswer.order || submittedAnswer.order.length === 0) {
                showError(button, 'لطفاً ابتدا پاسخ را وارد کنید. حداقل یک آیتم باید در جایگاه قرار گیرد.');
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }
        }
        
        if (scheduleItemType === 'MultipleChoice' || scheduleItemType === '4') {
            if (!submittedAnswer.selectedOptions || submittedAnswer.selectedOptions.length === 0) {
                showError(button, 'لطفاً ابتدا پاسخ را انتخاب کنید.');
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
        })
        .catch(function(error) {
            console.error('Error checking answer:', error);
            showError(button, error.message || 'خطا در بررسی پاسخ');
            button.disabled = false;
            button.innerHTML = originalContent;
        });
    }

    function getSubmittedAnswer(scheduleItemType, blockId) {
        // Ordering block
        if (scheduleItemType === 'Ordering' || scheduleItemType === '8') {
            return getOrderingAnswer(blockId);
        }
        
        // MultipleChoice block
        if (scheduleItemType === 'MultipleChoice' || scheduleItemType === '4') {
            return getMultipleChoiceAnswer(blockId);
        }

        // Add other types as needed
        // For now, return null for unsupported types
        return null;
    }

    function getOrderingAnswer(blockId) {
        // Find the sorting block for this blockId
        // First, try to find button with this blockId and get its parent card
        const button = document.querySelector(`.btn-check-answer-minimal[data-block-id="${blockId}"]`);
        let blockCard = button ? button.closest('.ordering-block-card') : null;
        
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

    function getMultipleChoiceAnswer(blockId) {
        // Find the multiple choice block card
        const button = document.querySelector(`.btn-check-answer-minimal[data-block-id="${blockId}"]`);
        let blockCard = button ? button.closest('.mcq-block-card') : null;
        
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

    function showResult(button, result, blockId) {
        // For minimal button, we just update the icon - no need for result container
        const isMinimal = button.classList.contains('btn-check-answer-minimal');
        
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
        const blockCard = button.closest('.ordering-block-card') || button.closest('.mcq-block-card');
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
        // Get all ordering blocks and MCQ blocks on the page
        const orderingBlocks = document.querySelectorAll('.ordering-block-card');
        const mcqBlocks = document.querySelectorAll('.mcq-block-card');
        
        if (orderingBlocks.length === 0 && mcqBlocks.length === 0) return;
        
        // Get schedule item ID from first button
        const firstButton = document.querySelector('.btn-check-answer-minimal, .btn-check-answer');
        if (!firstButton) return;
        
        const scheduleItemId = firstButton.getAttribute('data-schedule-item-id');
        if (!scheduleItemId) return;
        
        // Load history for all ordering blocks
        orderingBlocks.forEach(function(blockCard) {
            const button = blockCard.querySelector('.btn-check-answer-minimal, .btn-check-answer');
            if (!button) return;
            
            const blockId = button.getAttribute('data-block-id');
            if (!blockId) return;
            
            loadAnswerHistoryForBlock(scheduleItemId, blockId);
        });
        
        // Load history for all MCQ blocks
        mcqBlocks.forEach(function(blockCard) {
            const button = blockCard.querySelector('.btn-check-answer-minimal, .btn-check-answer');
            if (!button) return;
            
            const blockId = button.getAttribute('data-block-id');
            if (!blockId) return;
            
            loadAnswerHistoryForBlock(scheduleItemId, blockId);
        });
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
            // Handle both wrapped Result format and direct array format
            let statsList = null;
            if (data && data.success && data.value) {
                statsList = data.value;
            } else if (Array.isArray(data)) {
                statsList = data;
            }
            
            if (statsList && statsList.length > 0) {
                const stats = statsList[0];
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

    // Export for use in other scripts if needed
    window.BlockAnswerChecker = {
        initialize: initializeBlockAnswerChecker,
        checkAnswer: handleCheckAnswer,
        loadHistory: loadAnswerHistory
    };
})();

