/**
 * Ordering Block Handler
 * Handles ordering question blocks
 */

class OrderingHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
    }

    canHandle(blockType) {
        return blockType === 'ordering';
    }

    render(block) {
        const blockTemplatesContainer = document.getElementById('blockTemplatesContainer');
        if (!blockTemplatesContainer) {
            console.error('OrderingHandler: blockTemplatesContainer not found');
            return null;
        }

        const templatesContainer = blockTemplatesContainer.querySelector('#questionTypeBlockTemplates');
        if (!templatesContainer) {
            console.error('OrderingHandler: questionTypeBlockTemplates not found');
            return null;
        }

        const template = templatesContainer.querySelector('[data-type="ordering"]');
        if (!template) {
            console.error('OrderingHandler: Ordering template not found');
            return null;
        }

        const blockElement = template.cloneNode(true);
        // Remove template class that hides the element
        blockElement.classList.remove('content-block-template');
        blockElement.classList.add('content-block', 'question-type-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data || {});
        blockElement.style.display = ''; // Ensure it's visible

        return blockElement;
    }

    initialize(blockElement, block) {
        // Initialize ordering items if they exist
        if (block.data && block.data.items) {
            this.renderItems(blockElement, block);
        }

        // Setup event listeners
        const addItemBtn = blockElement.querySelector('[data-action="add-ordering-item"]');
        if (addItemBtn) {
            addItemBtn.addEventListener('click', () => this.addItem(blockElement, block));
        }

        const saveOrderBtn = blockElement.querySelector('[data-action="save-correct-order"]');
        if (saveOrderBtn) {
            saveOrderBtn.addEventListener('click', () => this.saveCorrectOrder(blockElement, block));
        }

        // Initialize drag & drop if enabled
        if (block.data && block.data.allowDragDrop) {
            this.initializeDragDrop(blockElement);
        }
    }

    addItem(blockElement, block) {
        // Add new ordering item
        // Implementation to be completed
    }

    renderItems(blockElement, block) {
        // Render ordering items
        // Implementation to be completed
    }

    saveCorrectOrder(blockElement, block) {
        // Save correct order
        // Implementation to be completed
    }

    initializeDragDrop(blockElement) {
        // Initialize drag and drop functionality
        // Implementation to be completed (can use SortableJS or native HTML5)
    }

    collectData(blockElement, block) {
        // Collect ordering data
        // Implementation to be completed
        return block.data;
    }
}

if (typeof window !== 'undefined') {
    window.OrderingHandler = OrderingHandler;
}
