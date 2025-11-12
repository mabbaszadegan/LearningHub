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
        const globalOptionLookup = new Map();
        const MIN_CHAR_LENGTH = 4;
        const globalOptionMaxCharLength = computeOptionLengthFromButtons(globalOptionButtons);

        globalOptionButtons.forEach((button, index) => {
            const optionId = (button.dataset.optionId || '').trim();
            const fallbackKey = (button.dataset.optionValue || button.textContent || '').trim();
            const key = optionId.length > 0 ? optionId : `${fallbackKey}-${index}`;
            button.dataset.optionKey = key;
            if (optionId.length > 0) {
                globalOptionLookup.set(optionId, button);
            }
            globalOptionLookup.set(key, button);
        });

        blanks.forEach(blank => {
            const input = blank.querySelector('.gapfill-input');
            const clearButton = blank.querySelector('[data-action="clear-blank"]');
            const blankOptionButtons = Array.from(blank.querySelectorAll('.gapfill-blank-options .gapfill-option'));
            const allowManual = blank.dataset.allowManual === 'true';
            const allowGlobalOptions = blank.dataset.allowGlobalOptions === 'true';
            const blankOptionMaxCharLength = computeOptionLengthFromButtons(blankOptionButtons);
            const preferredByOptions = Math.max(
                blankOptionMaxCharLength,
                allowGlobalOptions ? globalOptionMaxCharLength : 0
            );
            if (preferredByOptions > 0) {
                blank.dataset.preferredCharLength = String(preferredByOptions);
            } else {
                delete blank.dataset.preferredCharLength;
            }

            if (input) {
                blank.dataset.selectedOptionId = blank.dataset.selectedOptionId || '';
                blank.dataset.selectedOptionSource = blank.dataset.selectedOptionSource || '';

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

                input.addEventListener('input', (event) => {
                    if (!event.isTrusted && blank.dataset.suppressInputHandler === 'true') {
                        blank.dataset.suppressInputHandler = '';
                        return;
                    }

                    releaseGlobalOption(blank);
                    blank.dataset.selectedOptionId = '';
                    blank.dataset.selectedOptionSource = allowManual ? 'manual' : '';
                    blank.dataset.selectedOptionKey = '';
                    blank.classList.toggle('has-value', input.value.trim().length > 0);
                    blank.classList.remove('is-incomplete');
                    blank.classList.remove('is-correct', 'is-incorrect');
                    clearOptionSelections(blank);
                    applyInputWidth(blank);
                });

                blank.addEventListener('pointerdown', (event) => {
                    if (event.target.closest('[data-action="clear-blank"]')) {
                        return;
                    }
                    activeBlank = blank;
                });

                blank.addEventListener('click', (event) => {
                    if (event.target.closest('[data-action="clear-blank"]')) {
                        return;
                    }

                    const selectedSource = blank.dataset.selectedOptionSource || '';
                    const selectedOptionId = blank.dataset.selectedOptionId || '';

                    if (selectedSource === 'global' && selectedOptionId) {
                        event.preventDefault();
                        clearBlank(blank);
                        if (allowManual) {
                            input.removeAttribute('readonly');
                            input.removeAttribute('tabindex');
                            input.focus();
                        }
                    }
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

            applyInputWidth(blank);
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
            const optionKey = button.dataset.optionKey || optionId || '';
            const optionValue = button.dataset.optionValue || button.textContent.trim();

            releaseGlobalOption(blank);

            if (!allowManual) {
                input.setAttribute('readonly', 'readonly');
            }

            blank.dataset.suppressInputHandler = 'true';
            input.value = optionValue;
            blank.classList.add('has-value');
            blank.classList.remove('is-incomplete');
            blank.classList.remove('is-correct', 'is-incorrect');

            blank.dataset.selectedOptionId = optionId;
            blank.dataset.selectedOptionSource = source;
            blank.dataset.selectedOptionKey = optionKey;

            setSelectedOption(button, source);
            updateBlankOptionSelections(blank, optionId);
            assignGlobalOption(blank, button, source);
            applyInputWidth(blank);
            input.focus();
        }

        function clearBlank(blank) {
            releaseGlobalOption(blank);

            const input = blank.querySelector('.gapfill-input');
            if (input) {
                input.value = '';
                input.dispatchEvent(new Event('input', { bubbles: true }));
            }
            blank.dataset.selectedOptionId = '';
            blank.dataset.selectedOptionSource = '';
            blank.dataset.selectedOptionKey = '';
            blank.classList.remove('has-value');
            blank.classList.remove('is-incomplete');
            blank.classList.remove('is-correct', 'is-incorrect');
            clearOptionSelections(blank);
            applyInputWidth(blank);
        }

        function findTargetBlank(allBlanks, preferredBlank) {
            if (preferredBlank) {
                const allowGlobal = preferredBlank.dataset.allowGlobalOptions === 'true';
                if (allowGlobal) {
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

            return allBlanks.find(blank => blank.dataset.allowGlobalOptions === 'true') || null;
        }

        function updateBlankOptionSelections(blank, selectedOptionId) {
            const optionButtons = blank.querySelectorAll('.gapfill-blank-options .gapfill-option');
            optionButtons.forEach(option => {
                option.classList.toggle('is-selected', option.dataset.optionId === selectedOptionId);
                if (!selectedOptionId || option.dataset.optionId !== selectedOptionId) {
                    option.classList.remove('is-crossed');
                }
            });
        }

        function setSelectedOption(button, source) {
            if (!button || source !== 'global') {
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

        function assignGlobalOption(blank, button, source) {
            if (source !== 'global') {
                return;
            }

            const optionId = blank.dataset.selectedOptionId || '';
            if (!optionId) {
                return;
            }

            const key = button.dataset.optionKey || optionId;
            const targetButton = globalOptionLookup.get(key) || button;

            if (targetButton) {
                targetButton.classList.add('is-assigned');
                targetButton.classList.remove('is-selected');
                targetButton.dataset.assignedBlank = blank.dataset.blankId || '';
                targetButton.setAttribute('aria-hidden', 'true');
                targetButton.setAttribute('tabindex', '-1');
            }
        }

        function releaseGlobalOption(blank) {
            const selectedSource = blank.dataset.selectedOptionSource || '';
            const optionId = blank.dataset.selectedOptionId || blank.dataset.selectedOptionKey || '';
            if (selectedSource !== 'global' || !optionId) {
                return;
            }

            let button = globalOptionLookup.get(optionId);
            if (!button) {
                button = Array.from(globalOptionLookup.values()).find(opt =>
                    opt.dataset.optionId === optionId || opt.dataset.optionKey === optionId);
            }

            if (button) {
                button.classList.remove('is-assigned', 'is-selected', 'is-crossed');
                button.removeAttribute('data-assigned-blank');
                button.removeAttribute('aria-hidden');
                button.removeAttribute('tabindex');
            }
        }

        function computePreferredCharLength(blank) {
            const input = blank.querySelector('.gapfill-input');
            if (!input) {
                return MIN_CHAR_LENGTH;
            }

            const currentValue = input.value.trim();
            const preferredFromOptions = parseInt(blank.dataset.preferredCharLength || '0', 10);
            if (currentValue.length > 0) {
                return Math.max(1, currentValue.length, preferredFromOptions, MIN_CHAR_LENGTH);
            }

            return Math.max(MIN_CHAR_LENGTH, preferredFromOptions);
        }

        function applyInputWidth(blank) {
            const input = blank.querySelector('.gapfill-input');
            if (!input) {
                return;
            }

            const charLength = computePreferredCharLength(blank);
            input.style.width = `${charLength + 1}ch`;
        }

        function computeOptionLengthFromButtons(buttons) {
            if (!buttons || buttons.length === 0) {
                return 0;
            }

            return buttons.reduce((max, button) => {
                if (!button) {
                    return max;
                }

                const optionText = (button.dataset.optionValue || button.textContent || '').trim();
                return Math.max(max, optionText.length);
            }, 0);
        }
    }
})();

