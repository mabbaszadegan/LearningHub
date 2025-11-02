/**
 * Regular Block Handler
 * Handles regular content blocks (text, image, video, audio, code)
 */

class RegularBlockHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
    }

    canHandle(blockType) {
        return ['text', 'image', 'video', 'audio', 'code'].includes(blockType);
    }

    render(block) {
        // For regular blocks, use base class renderBlock directly to avoid infinite loop
        // This method should not be called for regular blocks, but if it is, use base class
        if (this.contentManager && typeof this.contentManager.constructor.prototype.renderBlock === 'function') {
            // Get the base class method
            const baseClass = Object.getPrototypeOf(Object.getPrototypeOf(this.contentManager));
            if (baseClass && baseClass.renderBlock) {
                return baseClass.renderBlock.call(this.contentManager, block);
            }
        }
        // Fallback: should not reach here for regular blocks
        console.warn('RegularBlockHandler.render called - this should use base class renderBlock');
        return null;
    }

    collectData(blockElement, block) {
        // Collect data from DOM
        // Implementation depends on block type
        return block.data;
    }
}

if (typeof window !== 'undefined') {
    window.RegularBlockHandler = RegularBlockHandler;
}
