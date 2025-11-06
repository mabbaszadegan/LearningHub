/**
 * Content Migrator
 * Converts old content structure to new unified structure
 */

const ContentMigrator = {
    /**
     * Migrate old content structure to new unified structure
     * @param {Object} oldContent - Old content object
     * @param {string} itemType - Schedule item type
     * @returns {Object} New unified content structure
     */
    migrate(oldContent, itemType) {
        if (!oldContent || typeof oldContent !== 'object') {
            return { itemType, blocks: [] };
        }

        // Check if already in new format
        if (oldContent.blocks && Array.isArray(oldContent.blocks)) {
            return oldContent;
        }

        // Handle different old formats
        let blocks = [];

        // Old reminder format
        if (oldContent.blocks && Array.isArray(oldContent.blocks)) {
            blocks = oldContent.blocks;
        }
        // Old gapfill format
        else if (oldContent.type === 'gapfill' && oldContent.blocks) {
            blocks = oldContent.blocks;
        }
        // Old multiple choice format
        else if (oldContent.type === 'multiplechoice' && oldContent.blocks) {
            blocks = oldContent.blocks;
        }
        // Old written format
        else if (oldContent.questionBlocks && Array.isArray(oldContent.questionBlocks)) {
            blocks = oldContent.questionBlocks.map((qb, index) => ({
                id: qb.id || `block-${index + 1}`,
                type: qb.questionType === 'Text' ? 'questionText' : 
                      qb.questionType === 'Image' ? 'questionImage' :
                      qb.questionType === 'Video' ? 'questionVideo' :
                      qb.questionType === 'Audio' ? 'questionAudio' : 'questionText',
                order: qb.order || index,
                data: {
                    ...qb.questionData,
                    points: qb.points || 1,
                    isRequired: qb.isRequired !== false,
                    hint: qb.hint || ''
                }
            }));
        }
        // Old reminder format with blocks
        else if (oldContent.blocks && Array.isArray(oldContent.blocks)) {
            blocks = oldContent.blocks;
        }

        return {
            itemType: itemType || 'reminder',
            blocks: blocks,
            settings: {}
        };
    }
};

if (typeof window !== 'undefined') {
    window.ContentMigrator = ContentMigrator;
}


