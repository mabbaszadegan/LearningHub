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
        // Attach click handlers to all check answer buttons
        const checkButtons = document.querySelectorAll('.btn-check-answer');
        checkButtons.forEach(function(button) {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                handleCheckAnswer(button);
            });
        });
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
        const originalText = button.innerHTML;
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>در حال بررسی...</span>';

        // Get submitted answer based on schedule item type
        const submittedAnswer = getSubmittedAnswer(scheduleItemType, blockId);

        if (!submittedAnswer) {
            showError(button, 'لطفاً ابتدا پاسخ را وارد کنید. حداقل یک آیتم باید در جایگاه قرار گیرد.');
            button.disabled = false;
            button.innerHTML = originalText;
            return;
        }

        // Validate that submitted answer has content
        if (scheduleItemType === 'Ordering' || scheduleItemType === '8') {
            if (!submittedAnswer.order || submittedAnswer.order.length === 0) {
                showError(button, 'لطفاً ابتدا پاسخ را وارد کنید. حداقل یک آیتم باید در جایگاه قرار گیرد.');
                button.disabled = false;
                button.innerHTML = originalText;
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
            button.innerHTML = originalText;
        })
        .catch(function(error) {
            console.error('Error checking answer:', error);
            showError(button, error.message || 'خطا در بررسی پاسخ');
            button.disabled = false;
            button.innerHTML = originalText;
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
        const button = document.querySelector(`.btn-check-answer[data-block-id="${blockId}"]`);
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
        // Find the multiple choice block
        const block = document.querySelector(`[data-block-id="${blockId}"]`) || 
                     document.querySelector('.multiple-choice-block');
        if (!block) return null;

        const selectedOptions = [];
        const checkboxes = block.querySelectorAll('input[type="checkbox"]:checked');
        const radios = block.querySelectorAll('input[type="radio"]:checked');

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

        return {
            selectedOptions: selectedOptions
        };
    }

    function showResult(button, result, blockId) {
        // Find result container
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

    function showError(button, message) {
        // Find result container
        const blockCard = button.closest('.ordering-block-card');
        if (!blockCard) return;

        const blockId = button.getAttribute('data-block-id');
        const resultContainer = blockCard.querySelector(`.block-result[data-block-id="${blockId}"]`);
        if (!resultContainer) return;

        const resultContent = resultContainer.querySelector('.result-content');
        if (!resultContent) return;

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

    // Export for use in other scripts if needed
    window.BlockAnswerChecker = {
        initialize: initializeBlockAnswerChecker,
        checkAnswer: handleCheckAnswer
    };
})();

