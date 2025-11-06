/**
 * Block Type Configuration
 * Defines which block types are available for each schedule item type
 */

const BlockTypeConfig = {
    'reminder': {
        regularBlocks: ['text', 'image', 'video', 'audio', 'code'],
        questionBlocks: ['questionText', 'questionImage', 'questionVideo', 'questionAudio'],
        questionTypeBlocks: ['multipleChoice', 'gapFill', 'ordering', 'matching', 'errorFinding']
    },
    'writing': {
        regularBlocks: [],
        questionBlocks: ['questionText', 'questionImage', 'questionVideo', 'questionAudio'],
        questionTypeBlocks: ['multipleChoice']
    },
    'audio': {
        regularBlocks: [],
        questionBlocks: ['questionText', 'questionImage', 'questionVideo', 'questionAudio'],
        questionTypeBlocks: ['multipleChoice']
    },
    'multiplechoice': {
        regularBlocks: [],
        questionBlocks: [],
        questionTypeBlocks: ['multipleChoice']
    },
    'gapfill': {
        regularBlocks: [],
        questionBlocks: [],
        questionTypeBlocks: ['gapFill']
    },
    'ordering': {
        regularBlocks: [],
        questionBlocks: [],
        questionTypeBlocks: ['ordering']
    },
    'match': {
        regularBlocks: [],
        questionBlocks: [],
        questionTypeBlocks: ['matching']
    },
    'errorfinding': {
        regularBlocks: [],
        questionBlocks: [],
        questionTypeBlocks: ['errorFinding']
    },
    'codeexercise': {
        regularBlocks: [],
        questionBlocks: ['questionText', 'questionImage', 'questionVideo', 'questionAudio'],
        questionTypeBlocks: []
    },
    'quiz': {
        regularBlocks: ['text', 'image', 'video', 'audio', 'code'],
        questionBlocks: ['questionText', 'questionImage', 'questionVideo', 'questionAudio'],
        questionTypeBlocks: ['multipleChoice', 'gapFill', 'ordering', 'matching', 'errorFinding']
    }
};

// Mapping from enum integer values to string keys
const ScheduleItemTypeMapping = {
    0: 'reminder',      // Reminder
    1: 'writing',       // Writing
    2: 'audio',         // Audio
    3: 'gapfill',       // GapFill
    4: 'multiplechoice', // MultipleChoice
    5: 'match',         // Match
    6: 'errorfinding',  // ErrorFinding
    7: 'codeexercise',  // CodeExercise
    8: 'quiz',          // Quiz
    9: 'ordering'       // Ordering
};

// Helper function to convert enum integer to string key
function getItemTypeString(itemType) {
    // First, try to get from select element's data attribute (most reliable)
    if (typeof itemType === 'string' || typeof itemType === 'number') {
        const selectElement = document.getElementById('itemType');
        if (selectElement) {
            const selectedOption = selectElement.options[selectElement.selectedIndex];
            if (selectedOption && selectedOption.dataset.typeString) {
                return selectedOption.dataset.typeString;
            }
            
            // Try to find option by value
            const valueToCheck = typeof itemType === 'string' && /^\d+$/.test(itemType) 
                ? parseInt(itemType, 10) 
                : itemType;
            const option = Array.from(selectElement.options).find(opt => 
                opt.value == valueToCheck || parseInt(opt.value, 10) == valueToCheck
            );
            if (option && option.dataset.typeString) {
                return option.dataset.typeString;
            }
        }
    }
    
    // Fallback: direct conversion
    // If already a string and not a number, return it
    if (typeof itemType === 'string' && !/^\d+$/.test(itemType)) {
        return itemType.toLowerCase();
    }
    
    // If it's a number (enum value) or string number, convert to string
    const numValue = typeof itemType === 'number' 
        ? itemType 
        : (typeof itemType === 'string' && /^\d+$/.test(itemType) ? parseInt(itemType, 10) : null);
    
    if (numValue !== null && numValue !== undefined) {
        return ScheduleItemTypeMapping[numValue] || 'reminder';
    }
    
    // Default fallback
    return 'reminder';
}

// Helper function to get config for item type
function getBlockTypeConfig(itemType) {
    const typeString = getItemTypeString(itemType);
    return BlockTypeConfig[typeString] || BlockTypeConfig['reminder'];
}

// Export for use in other files
if (typeof window !== 'undefined') {
    window.BlockTypeConfig = BlockTypeConfig;
    window.ScheduleItemTypeMapping = ScheduleItemTypeMapping;
    window.getItemTypeString = getItemTypeString;
    window.getBlockTypeConfig = getBlockTypeConfig;
}
