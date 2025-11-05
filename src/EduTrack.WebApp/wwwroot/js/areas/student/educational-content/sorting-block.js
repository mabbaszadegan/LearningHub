(function () {
    function qs(selector, root) { return (root || document).querySelector(selector); }
    function qsa(selector, root) { return Array.prototype.slice.call((root || document).querySelectorAll(selector)); }

    function initSortingBlock(container) {
        if (!container) return;

        var allowDnd = (container.getAttribute('data-allow-dnd') || 'true') === 'true';
        if (!allowDnd) return;

        // Support both ID selector and class selector for pool and slots
        var pool = qs('#sortingPool', container) || qs('.sorting-pool', container);
        var slots = qs('#sortingSlots', container) || qs('.sorting-slots', container);
        if (!pool || !slots) return;

        var draggedEl = null;

        function makeDraggable(item) {
            item.setAttribute('draggable', 'true');
            item.addEventListener('dragstart', function (e) {
                draggedEl = item;
                item.classList.add('dragging');
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData('text/plain', item.getAttribute('data-id') || '');
            });
            item.addEventListener('dragend', function () {
                item.classList.remove('dragging');
            });
        }

        function setupDropzone(zone) {
            zone.addEventListener('dragover', function (e) {
                if (!draggedEl) return;
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                zone.classList.add('over');
            });
            zone.addEventListener('dragleave', function () {
                zone.classList.remove('over');
            });
            zone.addEventListener('drop', function (e) {
                e.preventDefault();
                zone.classList.remove('over');
                if (!draggedEl) return;

                // If zone already has an item, move it back to pool before placing
                var existing = qs('.sorting-item', zone);
                if (existing) {
                    pool.appendChild(existing);
                }
                zone.appendChild(draggedEl);
                zone.classList.add('filled');
            });
        }

        // Allow dragging items back to pool
        pool.addEventListener('dragover', function (e) {
            if (!draggedEl) return;
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
        });
        pool.addEventListener('drop', function (e) {
            e.preventDefault();
            if (!draggedEl) return;
            pool.appendChild(draggedEl);
        });

        // Init all pool items
        qsa('.sorting-item', pool).forEach(makeDraggable);
        // Init all slots' dropzones
        qsa('.slot-dropzone', slots).forEach(setupDropzone);

        // Keyboard support (basic): Enter to move focused item into first empty slot
        pool.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            var focused = document.activeElement;
            if (!focused || !focused.classList.contains('sorting-item')) return;
            var firstEmpty = qsa('.slot-dropzone', slots).find(function (z) { return !qs('.sorting-item', z); });
            if (firstEmpty) {
                firstEmpty.appendChild(focused);
                firstEmpty.classList.add('filled');
            }
        });

        slots.addEventListener('keydown', function (e) {
            if (e.key !== 'Delete' && e.key !== 'Backspace') return;
            var focused = document.activeElement;
            if (!focused) return;
            var item = focused.classList.contains('sorting-item') ? focused : focused.closest('.sorting-item');
            if (item) {
                pool.appendChild(item);
            }
        });
    }

    function initAllSortingBlocks() {
        // Initialize all sorting blocks on the page
        var containers = qsa('.sorting-block');
        containers.forEach(function(container) {
            initSortingBlock(container);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAllSortingBlocks);
    } else {
        initAllSortingBlocks();
    }
})();


