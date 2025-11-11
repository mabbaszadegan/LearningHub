(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        const cards = document.querySelectorAll('.matching-block-card');
        cards.forEach(setupMatchingCard);
    });

    function setupMatchingCard(card) {
        const leftButtons = Array.from(card.querySelectorAll('.matching-item-button'));
        const optionButtons = Array.from(card.querySelectorAll('.matching-option-button'));
        const selectMap = new Map();
        const optionById = new Map();

        if (!leftButtons.length || !optionButtons.length) {
            setupAudioControls(card);
            return;
        }

        optionButtons.forEach(optionButton => {
            const optionId = optionButton.dataset.optionId;
            if (optionId) {
                optionById.set(optionId, optionButton);
            }
            optionButton.addEventListener('click', function () {
                handleRightClick(optionButton);
            });
        });

        leftButtons.forEach(button => {
            const selectId = button.getAttribute('data-select-id');
            if (!selectId) {
                selectMap.set(button, null);
                return;
            }

            const select = card.querySelector(`#${CSS.escape(selectId)}`);
            selectMap.set(button, select || null);
        });

        let activeLeft = null;
        let activeRight = null;

        leftButtons.forEach(button => {
            button.addEventListener('click', function () {
                handleLeftClick(button);
            });
        });

        initializeSelections();
        attachControls();
        refreshOptionStates();
        setupAudioControls(card);

        function initializeSelections() {
            leftButtons.forEach(button => {
                const select = selectMap.get(button);
                if (!select) {
                    button.dataset.selectedOption = '';
                    updateLeftDisplay(button, null);
                    return;
                }

                const storedValue = select.value;
                if (storedValue) {
                    button.dataset.selectedOption = storedValue;
                    button.dataset.pairedOption = storedValue;
                    const optionButton = optionById.get(storedValue) || null;
                    const letter = optionButton ? optionButton.dataset.optionLetter || '' : '';
                    if (optionButton) {
                        optionButton.dataset.pairedLeft = button.dataset.leftId || '';
                    }
                    updateLeftDisplay(button, letter);
                    lockPair(button, optionButton);
                } else {
                    button.dataset.selectedOption = '';
                    delete button.dataset.pairedOption;
                    updateLeftDisplay(button, null);
                }
            });
        }

        function attachControls() {
            const restartButton = card.querySelector('.btn-restart-matching');
            if (restartButton) {
                restartButton.addEventListener('click', function () {
                    unlockAllPairs(true);
                    restartButton.blur();
                });
            }

            card.addEventListener('matching:block:checked', function (event) {
                if (event && event.detail && event.detail.reason === 'incomplete') {
                    return;
                }

                unlockAllPairs(false);
            });
        }

        function handleLeftClick(button) {
            if (!button || button.disabled) {
                return;
            }

            if (activeRight && !activeRight.disabled) {
                pairItems(button, activeRight);
                return;
            }

            if (activeLeft === button) {
                button.classList.remove('is-active');
                activeLeft = null;
                return;
            }

            if (activeLeft) {
                activeLeft.classList.remove('is-active');
            }

            activeLeft = button;
            button.classList.add('is-active');
        }

        function handleRightClick(optionButton) {
            if (!optionButton || optionButton.disabled) {
                return;
            }

            if (activeLeft && !activeLeft.disabled) {
                pairItems(activeLeft, optionButton);
                return;
            }

            if (activeRight === optionButton) {
                optionButton.classList.remove('is-active');
                activeRight = null;
                return;
            }

            if (activeRight) {
                activeRight.classList.remove('is-active');
            }

            activeRight = optionButton;
            optionButton.classList.add('is-active');
        }

        function pairItems(leftButton, optionButton) {
            if (!leftButton || !optionButton) {
                return;
            }

            const select = selectMap.get(leftButton);
            if (!select) {
                return;
            }

            const optionId = optionButton.dataset.optionId || '';
            const letter = optionButton.dataset.optionLetter || '';

            select.value = optionId;
            leftButton.dataset.selectedOption = optionId;
            leftButton.dataset.pairedOption = optionId;
            optionButton.dataset.pairedLeft = leftButton.dataset.leftId || '';

            updateLeftDisplay(leftButton, letter);
            lockPair(leftButton, optionButton);
            refreshOptionStates();
            clearActiveStates();
        }

        function clearActiveStates() {
            if (activeLeft) {
                activeLeft.classList.remove('is-active');
            }

            if (activeRight) {
                activeRight.classList.remove('is-active');
            }

            activeLeft = null;
            activeRight = null;
        }

        function lockPair(leftButton, optionButton) {
            if (!leftButton) {
                return;
            }

            leftButton.classList.remove('is-active');
            leftButton.classList.add('is-locked');
            leftButton.disabled = true;
            leftButton.setAttribute('aria-disabled', 'true');

            if (optionButton) {
                optionButton.classList.remove('is-active');
                optionButton.classList.add('is-picked');
                optionButton.classList.add('is-locked');
                optionButton.disabled = true;
                optionButton.setAttribute('aria-disabled', 'true');
                optionButton.dataset.pairedLeft = leftButton.dataset.leftId || '';
            }
        }

        function unlockAllPairs(resetSelections) {
            clearActiveStates();

            leftButtons.forEach(button => {
                button.classList.remove('is-locked');
                button.disabled = false;
                button.removeAttribute('aria-disabled');

                const select = selectMap.get(button);
                if (resetSelections) {
                    if (select) {
                        select.value = '';
                    }
                    button.dataset.selectedOption = '';
                    delete button.dataset.pairedOption;
                    updateLeftDisplay(button, null);
                } else {
                    const optionId = button.dataset.selectedOption || '';
                    if (optionId) {
                        button.dataset.pairedOption = optionId;
                    } else {
                        delete button.dataset.pairedOption;
                    }
                    const letter = getLetterForOptionId(optionId);
                    updateLeftDisplay(button, letter);
                }
            });

            optionButtons.forEach(option => {
                option.classList.remove('is-locked');
                option.classList.remove('is-active');
                option.disabled = false;
                option.removeAttribute('aria-disabled');

                if (resetSelections) {
                    delete option.dataset.pairedLeft;
                } else {
                    option.dataset.pairedLeft = '';
                }
            });

            if (!resetSelections) {
                leftButtons.forEach(button => {
                    const optionId = button.dataset.selectedOption || '';
                    if (!optionId) {
                        return;
                    }

                    const optionButton = optionById.get(optionId);
                    if (optionButton) {
                        optionButton.dataset.pairedLeft = button.dataset.leftId || '';
                    }
                });
            }

            refreshOptionStates();
        }

        function getLetterForOptionId(optionId) {
            if (!optionId) {
                return null;
            }

            const optionButton = optionById.get(optionId);
            return optionButton ? optionButton.dataset.optionLetter || '' : null;
        }

        function updateLeftDisplay(button, letter) {
            const display = button.querySelector('.matching-item-current');
            if (!display) {
                return;
            }

            const emptyLabel = display.getAttribute('data-empty-label') || 'انتخاب نشده';

            if (letter) {
                display.textContent = letter;
                button.classList.add('has-selection');
            } else {
                display.textContent = emptyLabel;
                button.classList.remove('has-selection');
            }
        }

        function refreshOptionStates() {
            optionButtons.forEach(option => {
                const optionId = option.dataset.optionId || '';
                const assigned = leftButtons.some(btn => btn.dataset.selectedOption === optionId);
                option.classList.toggle('is-picked', assigned);
            });
        }
    }

    function setupAudioControls(root) {
        const audioButtons = root.querySelectorAll('.matching-audio-button');
        if (!audioButtons.length) {
            return;
        }

        let activeButton = null;

        audioButtons.forEach(button => {
            const audio = button.querySelector('.matching-audio-element');
            if (!audio) {
                return;
            }

            audio.preload = 'none';

            audio.addEventListener('ended', function () {
                stop(button, audio, false);
            });

            button.addEventListener('click', function () {
                if (button === activeButton) {
                    stop(button, audio, true);
                    activeButton = null;
                    return;
                }

                if (activeButton) {
                    const previousAudio = activeButton.querySelector('.matching-audio-element');
                    if (previousAudio) {
                        stop(activeButton, previousAudio, true);
                    }
                }

                play(button, audio);
                activeButton = button;
            });
        });

        function play(button, audio) {
            button.classList.add('is-playing');
            button.setAttribute('aria-pressed', 'true');
            const promise = audio.play();
            if (promise && typeof promise.catch === 'function') {
                promise.catch(function () {
                    stop(button, audio, false);
                });
            }
        }

        function stop(button, audio, reset) {
            button.classList.remove('is-playing');
            button.setAttribute('aria-pressed', 'false');
            audio.pause();
            if (reset) {
                audio.currentTime = 0;
            }
        }
    }
})();

