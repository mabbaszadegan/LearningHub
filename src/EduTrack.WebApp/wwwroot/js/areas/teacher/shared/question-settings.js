/* Question Settings Enhancer: points slider + difficulty slider */
(function () {
    const ENHANCED_ATTR = 'data-qsettings-enhanced';

    // Make enhanceContainer globally accessible
    window.enhanceQuestionSettings = function(container) {
        if (!container || container.hasAttribute(ENHANCED_ATTR)) return;

        enhancePoints(container);
        enhanceDifficulty(container);

        container.setAttribute(ENHANCED_ATTR, 'true');
    };

    function enhanceAll(root = document) {
        const containers = root.querySelectorAll('.question-settings:not([' + ENHANCED_ATTR + '])');
        containers.forEach(window.enhanceQuestionSettings);
    }

    function enhancePoints(container) {
        const number = container.querySelector('input[type="number"][data-setting="points"]');
        if (!number) return;

        // Points range: 1 to 20
        const min = 1;
        const max = 20;
        const step = 1;
        
        // Ensure value is valid
        let currentValue = parseInt(number.value || number.defaultValue || '1', 10);
        currentValue = isNaN(currentValue) ? 1 : currentValue;
        const value = clamp(currentValue, min, max);
        
        // Update number input with clamped value
        number.value = String(value);

        // Find the label and setting item
        const settingItem = number.closest('.setting-item');
        const label = settingItem ? settingItem.querySelector('label') : null;
        
        // Create value display span next to label
        let valueDisplay = settingItem ? settingItem.querySelector('.setting-value-display') : null;
        if (!valueDisplay && label) {
            valueDisplay = document.createElement('span');
            valueDisplay.className = 'setting-value-display';
            valueDisplay.textContent = value;
            // Insert after label text
            label.appendChild(document.createTextNode(' '));
            label.appendChild(valueDisplay);
        } else if (valueDisplay) {
            valueDisplay.textContent = value;
        }

        const wrapper = document.createElement('div');
        wrapper.className = 'points-slider';

        const range = document.createElement('input');
        range.type = 'range';
        range.min = String(min);
        range.max = String(max);
        range.step = String(step);
        range.value = String(value);
        range.className = 'points-range';

        // NO BUBBLE - removed as requested
        wrapper.appendChild(range);

        if (settingItem) {
            number.classList.add('qsetting-hidden-input');
            settingItem.appendChild(wrapper);
        }

        // Sync range -> number -> label display
        range.addEventListener('input', () => {
            const currentValue = range.value;
            number.value = currentValue;
            // Trigger both input and change events for content-builder
            number.dispatchEvent(new Event('input', { bubbles: true }));
            number.dispatchEvent(new Event('change', { bubbles: true }));
            // Update label display
            if (valueDisplay) {
                valueDisplay.textContent = currentValue;
            }
            // Update track fill
            updateTrackFill(range);
        });

        // Sync number -> range (in case code changes original input)
        number.addEventListener('input', () => {
            const nv = clamp(parseInt(number.value || '1', 10), min, max);
            if (String(nv) !== range.value) {
                range.value = String(nv);
                // Update label display
                if (valueDisplay) {
                    valueDisplay.textContent = String(nv);
                }
                // Update track fill
                updateTrackFill(range);
            }
        });

        // Also update on change event
        range.addEventListener('change', () => {
            const currentValue = range.value;
            number.value = currentValue;
            number.dispatchEvent(new Event('change', { bubbles: true }));
            if (valueDisplay) {
                valueDisplay.textContent = currentValue;
            }
            updateTrackFill(range);
        });

        // Initial track fill update
        setTimeout(() => {
            updateTrackFill(range);
        }, 50);
    }

    function enhanceDifficulty(container) {
        const select = container.querySelector('select[data-setting="difficulty"]');
        if (!select) return;

        const options = [
            { value: 'easy', label: 'آسان', index: 0 },
            { value: 'medium', label: 'متوسط', index: 1 },
            { value: 'hard', label: 'سخت', index: 2 }
        ];

        // Get current value
        let currentVal = select.value || (select.options[select.selectedIndex]?.value || 'medium');
        if (!['easy', 'medium', 'hard'].includes(currentVal)) {
            currentVal = 'medium';
        }
        
        const currentOption = options.find(opt => opt.value === currentVal) || options[1];
        const currentIndex = currentOption.index;
        
        // Find the label and setting item
        const settingItem = select.closest('.setting-item');
        const label = settingItem ? settingItem.querySelector('label') : null;
        
        // Create value display span next to label
        let valueDisplay = settingItem ? settingItem.querySelector('.difficulty-value-display') : null;
        if (!valueDisplay && label) {
            valueDisplay = document.createElement('span');
            valueDisplay.className = 'difficulty-value-display';
            valueDisplay.textContent = currentOption.label;
            // Insert after label text
            label.appendChild(document.createTextNode(' '));
            label.appendChild(valueDisplay);
        } else if (valueDisplay) {
            valueDisplay.textContent = currentOption.label;
        }

        // Create slider wrapper
        const wrapper = document.createElement('div');
        wrapper.className = 'difficulty-slider';

        // Difficulty range: 0 (easy) to 2 (hard)
        const min = 0;
        const max = 2;
        const step = 1;

        const range = document.createElement('input');
        range.type = 'range';
        range.min = String(min);
        range.max = String(max);
        range.step = String(step);
        range.value = String(currentIndex);
        range.className = 'difficulty-range';

        // NO BUBBLE - removed as requested
        wrapper.appendChild(range);

        if (settingItem) {
            // Hide original select but keep it accessible
            select.classList.add('qsetting-hidden-input');
            settingItem.appendChild(wrapper);
        }

        // Sync range -> select -> label display
        range.addEventListener('input', () => {
            const index = parseInt(range.value, 10);
            const selectedOption = options[index] || options[1];
            
            select.value = selectedOption.value;
            select.selectedIndex = Array.from(select.options).findIndex(o => o.value === selectedOption.value);
            // Trigger change event for content-builder
            select.dispatchEvent(new Event('change', { bubbles: true }));
            
            // Update label display
            if (valueDisplay) {
                valueDisplay.textContent = selectedOption.label;
            }
            // Update track fill
            updateTrackFill(range);
        });

        // Sync select -> range (in case code changes original select)
        select.addEventListener('change', () => {
            const selectedVal = select.value;
            const selectedOption = options.find(opt => opt.value === selectedVal) || options[1];
            const newIndex = selectedOption.index;
            
            if (String(newIndex) !== range.value) {
                range.value = String(newIndex);
                if (valueDisplay) {
                    valueDisplay.textContent = selectedOption.label;
                }
                updateTrackFill(range);
            }
        });

        // Also update on change event
        range.addEventListener('change', () => {
            const index = parseInt(range.value, 10);
            const selectedOption = options[index] || options[1];
            
            select.value = selectedOption.value;
            select.selectedIndex = Array.from(select.options).findIndex(o => o.value === selectedOption.value);
            select.dispatchEvent(new Event('change', { bubbles: true }));
            
            if (valueDisplay) {
                valueDisplay.textContent = selectedOption.label;
            }
            updateTrackFill(range);
        });

        // Initial track fill update
        setTimeout(() => {
            updateTrackFill(range);
        }, 50);
    }

    function updateTrackFill(range) {
        if (!range) return;
        
        const val = Number(range.value);
        if (isNaN(val)) return;
        
        const min = Number(range.min) || 0;
        const max = Number(range.max) || 100;
        const percent = (max - min > 0) ? (val - min) / (max - min) : 0;
        
        // Update track fill via CSS variable
        const pct = percent * 100;
        range.style.setProperty('--fill', `${pct}%`);
    }

    function clamp(v, min, max) { 
        return Math.min(Math.max(min, v), max); 
    }

    // Initial run
    document.addEventListener('DOMContentLoaded', () => enhanceAll());

    // Observe dynamic inserts
    const mo = new MutationObserver(mutations => {
        mutations.forEach(m => {
            if (!m.addedNodes) return;
            m.addedNodes.forEach(n => {
                if (!(n instanceof HTMLElement)) return;
                // Check if the added node itself is question-settings
                if (n.matches && n.matches('.question-settings')) {
                    window.enhanceQuestionSettings(n);
                }
                // Check for nested question-settings
                const nested = n.querySelectorAll ? n.querySelectorAll('.question-settings:not([' + ENHANCED_ATTR + '])') : [];
                if (nested && nested.length) {
                    nested.forEach(window.enhanceQuestionSettings);
                }
            });
        });
    });
    mo.observe(document.documentElement, { childList: true, subtree: true });
})();