(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initializeMatchingBlocks();
    });

    function initializeMatchingBlocks() {
        const cards = document.querySelectorAll('.matching-block-card');
        if (!cards.length) {
            return;
        }

        cards.forEach(card => {
            const selects = card.querySelectorAll('.matching-select-input');
            selects.forEach(select => {
                select.addEventListener('change', function () {
                    updateCardSelections(card);
                });
            });

            updateCardSelections(card);
        });
    }

    function updateCardSelections(card) {
        const selects = Array.from(card.querySelectorAll('.matching-select-input'));

        const selectedValues = selects
            .map(select => select.value)
            .filter(value => value && value.trim().length > 0);

        const previews = card.querySelectorAll('.matching-option-preview');
        previews.forEach(preview => {
            const optionId = preview.getAttribute('data-option-id');
            if (!optionId) {
                preview.classList.remove('is-selected');
                return;
            }

            const isActive = selectedValues.includes(optionId);
            preview.classList.toggle('is-selected', isActive);
        });

        const duplicates = getDuplicateSelections(selectedValues);
        selects.forEach(select => {
            if (select.value && duplicates.has(select.value)) {
                select.classList.add('matching-select-duplicate');
            } else {
                select.classList.remove('matching-select-duplicate');
            }
        });
    }

    function getDuplicateSelections(values) {
        const duplicates = new Set();
        const seen = new Set();

        values.forEach(value => {
            if (seen.has(value)) {
                duplicates.add(value);
            } else {
                seen.add(value);
            }
        });

        return duplicates;
    }
})();

