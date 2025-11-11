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

        if (!leftButtons.length || !optionButtons.length) {
            setupAudioControls(card);
            return;
        }

        leftButtons.forEach(button => {
            const selectId = button.getAttribute('data-select-id');
            if (!selectId) {
                return;
            }

            const select = card.querySelector(`#${CSS.escape(selectId)}`);
            if (select) {
                selectMap.set(button, select);
            }
        });

        let activeButton = null;

        leftButtons.forEach(button => {
            button.addEventListener('click', function () {
                if (activeButton === button) {
                    button.classList.remove('is-active');
                    activeButton = null;
                    return;
                }

                if (activeButton) {
                    activeButton.classList.remove('is-active');
                }

                activeButton = button;
                button.classList.add('is-active');
            });

            const select = selectMap.get(button);
            if (select) {
                const value = select.value;
                if (value) {
                    const optionButton = optionButtons.find(opt => opt.dataset.optionId === value);
                    const letter = optionButton ? optionButton.dataset.optionLetter || '' : '';
                    button.dataset.selectedOption = value;
                    updateLeftDisplay(button, letter);
                } else {
                    updateLeftDisplay(button, null);
                }
            } else {
                updateLeftDisplay(button, null);
            }
        });

        optionButtons.forEach(optionButton => {
            optionButton.addEventListener('click', function () {
                handleOptionSelection(optionButton);
            });
        });

        function handleOptionSelection(optionButton) {
            const optionId = optionButton.dataset.optionId || '';
            const optionLetter = optionButton.dataset.optionLetter || '';

            if (!activeButton) {
                const assignedButton = leftButtons.find(btn => btn.dataset.selectedOption === optionId);
                if (assignedButton) {
                    assignedButton.focus();
                    leftButtons.forEach(btn => btn.classList.remove('is-active'));
                    assignedButton.classList.add('is-active');
                    activeButton = assignedButton;
                }
                return;
            }

            const select = selectMap.get(activeButton);
            if (!select) {
                return;
            }

            const currentValue = activeButton.dataset.selectedOption || '';
            if (currentValue === optionId) {
                setSelection(activeButton, select, null, '');
                activeButton.classList.remove('is-active');
                activeButton = null;
                refreshOptionStates();
                return;
            }

            const previousButton = leftButtons.find(btn => btn.dataset.selectedOption === optionId && btn !== activeButton);
            if (previousButton) {
                const previousSelect = selectMap.get(previousButton);
                if (previousSelect) {
                    setSelection(previousButton, previousSelect, null, '');
                }
            }

            setSelection(activeButton, select, optionId, optionLetter);
            activeButton.classList.remove('is-active');
            activeButton = null;
            refreshOptionStates();
        }

        function setSelection(button, select, optionId, letter) {
            select.value = optionId || '';
            button.dataset.selectedOption = optionId || '';
            updateLeftDisplay(button, letter);
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

        refreshOptionStates();
        setupAudioControls(card);
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

