(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        const cards = document.querySelectorAll('.gapfill-block-card');
        cards.forEach(setupGapFillBlock);
    });

    function setupGapFillBlock(card) {
        if (!card || card.dataset.gapfillInitialized === 'true') {
            return;
        }

        card.dataset.gapfillInitialized = 'true';

        let activeBlank = null;
        const blanks = Array.from(card.querySelectorAll('.gapfill-blank'));
        const globalOptionButtons = Array.from(card.querySelectorAll('.gapfill-global-options .gapfill-option'));
        const restartButton = card.querySelector('.btn-restart-gapfill');

        blanks.forEach(blank => {
            const input = blank.querySelector('.gapfill-input');
            const clearButton = blank.querySelector('[data-action="clear-blank"]');
            const blankOptionButtons = Array.from(blank.querySelectorAll('.gapfill-blank-options .gapfill-option'));
            const allowManual = blank.dataset.allowManual === 'true';

            if (input) {
                if (!allowManual) {
                    input.setAttribute('readonly', 'readonly');
                    input.setAttribute('tabindex', '-1');
                }

                input.addEventListener('focus', () => {
                    activeBlank = blank;
                    blank.classList.add('is-active');
                });

                input.addEventListener('blur', () => {
                    blank.classList.remove('is-active');
                });

                input.addEventListener('input', () => {
                    blank.dataset.selectedOptionId = '';
                    blank.dataset.selectedOptionSource = '';
                    blank.classList.toggle('has-value', input.value.trim().length > 0);
                    blank.classList.remove('is-incomplete');
                    clearOptionSelections(blank);
                });

                blank.addEventListener('click', () => {
                    activeBlank = blank;
                });
            }

            if (clearButton) {
                clearButton.addEventListener('click', (event) => {
                    event.preventDefault();
                    event.stopPropagation();
                    clearBlank(blank);
                });
            }

            blankOptionButtons.forEach(optionButton => {
                optionButton.addEventListener('click', (event) => {
                    event.preventDefault();
                    selectOption(blank, optionButton, 'blank');
                    activeBlank = blank;
                });
            });
        });

        globalOptionButtons.forEach(optionButton => {
            optionButton.addEventListener('click', (event) => {
                event.preventDefault();
                const targetBlank = findTargetBlank(blanks, activeBlank);
                if (targetBlank) {
                    selectOption(targetBlank, optionButton, 'global');
                    activeBlank = targetBlank;
                }
            });
        });

        if (restartButton) {
            restartButton.addEventListener('click', (event) => {
                event.preventDefault();
                blanks.forEach(blank => clearBlank(blank));
                restartButton.blur();
            });
        }

        function selectOption(blank, button, source) {
            const input = blank.querySelector('.gapfill-input');
            if (!input) {
                return;
            }

            const allowManual = blank.dataset.allowManual === 'true';
            const allowGlobal = blank.dataset.allowGlobalOptions === 'true';
            const allowBlankOptions = blank.dataset.allowBlankOptions === 'true';

            if (source === 'global' && !allowGlobal) {
                return;
            }

            if (source === 'blank' && !allowBlankOptions) {
                return;
            }

            const optionId = button.dataset.optionId || '';
            const optionValue = button.dataset.optionValue || button.textContent.trim();

            if (!allowManual) {
                input.setAttribute('readonly', 'readonly');
            }

            input.value = optionValue;
            input.dispatchEvent(new Event('input', { bubbles: true }));

            blank.dataset.selectedOptionId = optionId;
            blank.dataset.selectedOptionSource = source;
            blank.classList.add('has-value');
            blank.classList.remove('is-incomplete');

            setSelectedOption(button);
            updateBlankOptionSelections(blank, optionId);
        }

        function clearBlank(blank) {
            const input = blank.querySelector('.gapfill-input');
            if (input) {
                input.value = '';
                input.dispatchEvent(new Event('input', { bubbles: true }));
            }
            blank.dataset.selectedOptionId = '';
            blank.dataset.selectedOptionSource = '';
            blank.classList.remove('has-value');
            blank.classList.remove('is-incomplete');
            clearOptionSelections(blank);
        }

        function findTargetBlank(allBlanks, preferredBlank) {
            if (preferredBlank) {
                const allowManual = preferredBlank.dataset.allowManual === 'true';
                const allowGlobal = preferredBlank.dataset.allowGlobalOptions === 'true';
                if (allowManual || allowGlobal) {
                    return preferredBlank;
                }
            }

            const emptyBlank = allBlanks.find(blank => {
                const allowGlobal = blank.dataset.allowGlobalOptions === 'true';
                const input = blank.querySelector('.gapfill-input');
                const hasValue = input && input.value.trim().length > 0;
                return allowGlobal && !hasValue;
            });

            if (emptyBlank) {
                return emptyBlank;
            }

            return allBlanks.find(blank => blank.dataset.allowGlobalOptions === 'true') || preferredBlank || allBlanks[0] || null;
        }

        function updateBlankOptionSelections(blank, selectedOptionId) {
            const optionButtons = blank.querySelectorAll('.gapfill-blank-options .gapfill-option');
            optionButtons.forEach(option => {
                option.classList.toggle('is-selected', option.dataset.optionId === selectedOptionId);
            });
        }

        function setSelectedOption(button) {
            if (!button) {
                return;
            }

            const container = button.closest('.gapfill-global-options-list');
            if (!container) {
                return;
            }

            container.querySelectorAll('.gapfill-option').forEach(option => {
                option.classList.toggle('is-selected', option === button);
            });
        }

        function clearOptionSelections(blank) {
            updateBlankOptionSelections(blank, '');
        }
    }
})();

