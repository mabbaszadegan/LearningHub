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
        var draggedFromZone = null;

        function updateDropzoneState(zone) {
            var hasItem = qs('.sorting-item', zone) !== null;
            if (hasItem) {
                zone.classList.add('filled');
            } else {
                zone.classList.remove('filled');
            }
        }

        function makeDraggable(item) {
            item.setAttribute('draggable', 'true');
            item.addEventListener('dragstart', function (e) {
                draggedEl = item;
                draggedFromZone = item.closest('.slot-dropzone');
                item.classList.add('dragging');
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData('text/plain', item.getAttribute('data-id') || '');
            });
            item.addEventListener('dragend', function () {
                item.classList.remove('dragging');
                // Update dropzone state if item was moved from a slot
                if (draggedFromZone) {
                    updateDropzoneState(draggedFromZone);
                    draggedFromZone = null;
                }
                draggedEl = null;
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
                    updateDropzoneState(zone);
                }
                zone.appendChild(draggedEl);
                updateDropzoneState(zone);
                draggedFromZone = null;
                draggedEl = null;
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
            if (draggedFromZone) {
                updateDropzoneState(draggedFromZone);
            }
            draggedFromZone = null;
            draggedEl = null;
        });

        // Helper function to check if mobile
        function isMobile() {
            return window.innerWidth <= 768;
        }
        
        // Helper function to handle mobile click
        function handleMobileClick(item, e) {
            if (!isMobile()) return;
            e.preventDefault();
            e.stopPropagation();
            
            // Check if item is in pool (not in a slot)
            if (item.parentElement === pool) {
                var firstEmpty = qsa('.slot-dropzone', slots).find(function (z) { return !qs('.sorting-item', z); });
                if (firstEmpty) {
                    firstEmpty.appendChild(item);
                    updateDropzoneState(firstEmpty);
                }
            } else if (item.parentElement && item.parentElement.classList.contains('slot-dropzone')) {
                // Item is in a slot - return to pool
                pool.appendChild(item);
                updateDropzoneState(item.parentElement);
            }
        }
        
        // Init all pool items
        qsa('.sorting-item', pool).forEach(function(item) {
            makeDraggable(item);
            // Mobile: click to move to first empty slot
            item.addEventListener('click', function(e) {
                handleMobileClick(item, e);
            });
        });
        
        // Init all slots' dropzones
        qsa('.slot-dropzone', slots).forEach(function(zone) {
            setupDropzone(zone);
            updateDropzoneState(zone); // Initialize state for items already in slots
            
            // Mobile: click on item in slot to return to pool
            var itemInZone = qs('.sorting-item', zone);
            if (itemInZone) {
                itemInZone.addEventListener('click', function(e) {
                    handleMobileClick(itemInZone, e);
                });
            }
        });
        
        // Observe slot changes for mobile click handling
        var observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                mutation.addedNodes.forEach(function(node) {
                    if (node.nodeType === 1 && node.classList.contains('sorting-item')) {
                        // New item added to a slot - add click handler
                        if (node.parentElement && node.parentElement.classList.contains('slot-dropzone')) {
                            node.addEventListener('click', function(e) {
                                handleMobileClick(node, e);
                            });
                        }
                    }
                });
            });
        });
        
        observer.observe(slots, { childList: true, subtree: true });

        // Keyboard support (basic): Enter to move focused item into first empty slot
        pool.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            var focused = document.activeElement;
            if (!focused || !focused.classList.contains('sorting-item')) return;
            var firstEmpty = qsa('.slot-dropzone', slots).find(function (z) { return !qs('.sorting-item', z); });
            if (firstEmpty) {
                firstEmpty.appendChild(focused);
                updateDropzoneState(firstEmpty);
            }
        });

        slots.addEventListener('keydown', function (e) {
            if (e.key !== 'Delete' && e.key !== 'Backspace') return;
            var focused = document.activeElement;
            if (!focused) return;
            var item = focused.classList.contains('sorting-item') ? focused : focused.closest('.sorting-item');
            if (item) {
                var previousZone = item.closest('.slot-dropzone');
                pool.appendChild(item);
                if (previousZone) {
                    updateDropzoneState(previousZone);
                }
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


