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
            var grip = qs('.sorting-grip', item);
            var isMobileDevice = isMobile();
            
            if (grip && !isMobileDevice) {
                // Desktop: Only make draggable from grip handle
                grip.setAttribute('draggable', 'true');
                grip.style.cursor = 'grab';
                
                // Prevent drag from starting on the item itself when grip exists (desktop only)
                item.setAttribute('draggable', 'false');
                item.style.cursor = 'pointer';
                
                grip.addEventListener('dragstart', function (e) {
                    // Start dragging the parent item
                    draggedEl = item;
                    draggedFromZone = item.closest('.slot-dropzone');
                    item.classList.add('dragging');
                    e.dataTransfer.effectAllowed = 'move';
                    e.dataTransfer.setData('text/plain', item.getAttribute('data-id') || '');
                    e.stopPropagation(); // Prevent bubbling to item
                });
                
                grip.addEventListener('dragend', function () {
                    item.classList.remove('dragging');
                    // Update dropzone state if item was moved from a slot
                    if (draggedFromZone) {
                        updateDropzoneState(draggedFromZone);
                        draggedFromZone = null;
                    }
                    draggedEl = null;
                });
            } else {
                // Mobile or no grip - make whole item draggable (for mobile drag & drop if needed)
                // In mobile, we'll use touch events instead, but keep draggable for compatibility
                if (!isMobileDevice) {
                    item.setAttribute('draggable', 'true');
                    item.style.cursor = 'grab';
                } else {
                    // In mobile, disable drag and use touch events instead
                    item.setAttribute('draggable', 'false');
                    item.style.cursor = 'pointer';
                }
                
                item.addEventListener('dragstart', function (e) {
                    // Only allow drag in desktop mode
                    if (isMobile()) {
                        e.preventDefault();
                        return false;
                    }
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
        }
        
        // Handle click on item in desktop to move to first empty slot
        function handleItemClick(item, e) {
            // Only in desktop mode, not mobile
            if (isMobile()) return;
            
            // Don't handle if clicking on grip (that's for dragging)
            var grip = qs('.sorting-grip', item);
            if (grip && (e.target === grip || grip.contains(e.target))) {
                return;
            }
            
            // Don't handle if clicking on audio controls
            if (e.target.tagName === 'AUDIO' || e.target.closest('audio')) {
                return;
            }
            
            e.preventDefault();
            e.stopPropagation();
            
            // Check if item is in pool (not in a slot)
            if (item.parentElement === pool) {
                // Item is in pool - move to first empty slot
                var firstEmpty = qsa('.slot-dropzone', slots).find(function (z) { return !qs('.sorting-item', z); });
                if (firstEmpty) {
                    firstEmpty.appendChild(item);
                    updateDropzoneState(firstEmpty);
                }
            } else if (item.parentElement && item.parentElement.classList.contains('slot-dropzone')) {
                // Item is in a slot - return to pool
                var previousZone = item.parentElement;
                pool.appendChild(item);
                updateDropzoneState(previousZone);
            }
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
        
        // Helper function to handle mobile tap/click
        function handleMobileTap(item, e) {
            // Double check mobile - don't interfere with desktop drag & drop
            if (!isMobile()) return;
            
            // Only handle touch events on mobile, not mouse events
            if (e.type === 'pointerdown' && e.pointerType !== 'touch') return;
            
            e.preventDefault();
            e.stopPropagation();
            
            // Check if item is in pool (not in a slot)
            if (item.parentElement === pool) {
                // Item is in pool - move to first empty slot
                var firstEmpty = qsa('.slot-dropzone', slots).find(function (z) { return !qs('.sorting-item', z); });
                if (firstEmpty) {
                    firstEmpty.appendChild(item);
                    updateDropzoneState(firstEmpty);
                    // Remove focus if any
                    if (document.activeElement === item) {
                        item.blur();
                    }
                }
            } else if (item.parentElement && item.parentElement.classList.contains('slot-dropzone')) {
                // Item is in a slot - return to pool
                var previousZone = item.parentElement; // Capture before moving
                pool.appendChild(item);
                updateDropzoneState(previousZone); // Update the zone that was just emptied
                // Remove focus if any
                if (document.activeElement === item) {
                    item.blur();
                }
            }
        }
        
        // Track items that already have mobile handlers to avoid duplicates
        var itemsWithMobileHandlers = new WeakSet();
        
        // Helper to attach handlers to an item (mobile and desktop)
        function attachMobileHandlers(item) {
            if (!item) return item;
            
            // Skip if already has handlers
            if (itemsWithMobileHandlers.has(item)) {
                return item;
            }
            
            // Mark as having handlers
            itemsWithMobileHandlers.add(item);
            
            // Ensure draggable is set (from grip handle in desktop, disabled in mobile)
            makeDraggable(item);
            
            if (isMobile()) {
                // Mobile: Use touch events for better responsiveness
                var touchStartTime = 0;
                var touchTarget = null;
                
                // Use touchstart for immediate response
                item.addEventListener('touchstart', function(e) {
                    touchStartTime = Date.now();
                    touchTarget = e.target;
                    
                    // Don't handle if touching grip (if user wants to drag)
                    var grip = qs('.sorting-grip', item);
                    if (grip && (e.target === grip || grip.contains(e.target))) {
                        return; // Let default behavior handle grip
                    }
                    
                    // Don't handle if touching audio controls
                    if (e.target.tagName === 'AUDIO' || e.target.closest('audio')) {
                        return;
                    }
                    
                    // Handle tap immediately
                    handleMobileTap(item, e);
                }, { passive: false });
                
                // Also add touchend to prevent click from firing
                item.addEventListener('touchend', function(e) {
                    e.preventDefault(); // Prevent click event from firing
                }, { passive: false });
                
                // Click fallback (shouldn't fire due to preventDefault, but just in case)
                item.addEventListener('click', function(e) {
                    // Only handle if it was a quick tap
                    if (Date.now() - touchStartTime < 500) {
                        var grip = qs('.sorting-grip', item);
                        if (grip && (e.target === grip || grip.contains(e.target))) {
                            return;
                        }
                        if (e.target.tagName === 'AUDIO' || e.target.closest('audio')) {
                            return;
                        }
                        // Don't handle if already handled by touchstart
                        if (touchTarget === e.target) {
                            return;
                        }
                        handleMobileTap(item, e);
                    }
                }, { passive: false });
            } else {
                // Desktop: Add click handler to move item to slot
                item.addEventListener('click', function(e) {
                    handleItemClick(item, e);
                });
            }
            
            return item;
        }
        
        // Init all pool items
        qsa('.sorting-item', pool).forEach(function(item) {
            // Always make draggable, attach mobile handlers if needed
            attachMobileHandlers(item);
        });
        
        // Init all slots' dropzones
        qsa('.slot-dropzone', slots).forEach(function(zone) {
            setupDropzone(zone);
            updateDropzoneState(zone); // Initialize state for items already in slots
            
            // Attach handlers to item in slot (works for both mobile and desktop)
            var itemInZone = qs('.sorting-item', zone);
            if (itemInZone) {
                attachMobileHandlers(itemInZone);
            }
        });
        
        // Observe slot changes for mobile tap handling
        var observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                mutation.addedNodes.forEach(function(node) {
                    if (node.nodeType === 1 && node.classList.contains('sorting-item')) {
                        // New item added to a slot - attach handlers
                        if (node.parentElement && node.parentElement.classList.contains('slot-dropzone')) {
                            attachMobileHandlers(node);
                        }
                    }
                });
            });
        });
        
        observer.observe(slots, { childList: true, subtree: true });
        
        // Also observe pool for items being added back from slots
        // Note: Using WeakSet to track handlers prevents infinite loops
        var poolObserver = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                mutation.addedNodes.forEach(function(node) {
                    if (node.nodeType === 1 && node.classList.contains('sorting-item')) {
                        // Item added back to pool - ensure it has handlers
                        if (node.parentElement === pool) {
                            attachMobileHandlers(node);
                        }
                    }
                });
            });
        });
        
        poolObserver.observe(pool, { childList: true });

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


