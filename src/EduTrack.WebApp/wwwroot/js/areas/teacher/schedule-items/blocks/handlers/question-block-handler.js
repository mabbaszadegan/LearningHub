/**
 * Question Block Handler
 * Handles question blocks (questionText, questionImage, questionVideo, questionAudio)
 */

class QuestionBlockHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
    }

    canHandle(blockType) {
        return ['questionText', 'questionImage', 'questionVideo', 'questionAudio'].includes(blockType);
    }

    render(block) {
        // For question blocks, use base class renderBlock directly to avoid infinite loop
        // This method should not be called for question blocks, but if it is, use base class
        if (this.contentManager && typeof this.contentManager.constructor.prototype.renderBlock === 'function') {
            // Get the base class method
            const baseClass = Object.getPrototypeOf(Object.getPrototypeOf(this.contentManager));
            if (baseClass && baseClass.renderBlock) {
                return baseClass.renderBlock.call(this.contentManager, block);
            }
        }
        // Fallback: should not reach here for question blocks
        console.warn('QuestionBlockHandler.render called - this should use base class renderBlock');
        return null;
    }

    collectData(blockElement, block) {
        // Collect question-specific data
        if (typeof QuestionBlockBase !== 'undefined') {
            QuestionBlockBase.collectQuestionFields(blockElement, block);
            QuestionBlockBase.collectQuestionTextContent(blockElement, block);
        }
        return block.data;
    }
}

if (typeof window !== 'undefined') {
    window.QuestionBlockHandler = QuestionBlockHandler;
}
