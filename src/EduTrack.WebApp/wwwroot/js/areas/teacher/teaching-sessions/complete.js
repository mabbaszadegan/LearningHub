// Teaching Sessions JavaScript

// Safe JSON parsing utility
function safeJsonParse(data, fallback = []) {
    if (!data || data === 'null' || data === 'undefined' || data === '' || data === '[]' || data === 'null') {
        return fallback;
    }
    
    try {
        return JSON.parse(data);
    } catch (error) {
        console.warn('JSON parse error:', error, 'Data:', data);
        return fallback;
    }
}

// Session Completion Manager
class SessionCompletionManager {
    constructor(options = {}) {
        this.hasPlan = options.hasPlan || false;
        this.groups = options.groups || [];
        this.availableSubTopics = options.availableSubTopics || [];
        this.availableLessons = options.availableLessons || [];
        this.plannedItems = options.plannedItems || [];
    }
    
    init() {
        this.setupEventListeners();
        this.initializeRatingSliders();
        this.setupGroupSelection();
        this.setupTopicCoverage();
    }
    
    setupEventListeners() {
        // Group selection
        $('.group-selector').on('change', (e) => {
            const groupId = $(e.target).val();
            this.toggleGroupSections(groupId);
        });
        
        // Form submission
        $('#completionForm').on('submit', (e) => {
            this.validateForm(e);
        });
        
        // Rating sliders
        $('.rating-slider').on('input', (e) => {
            this.updateRatingDisplay(e.target);
        });
        
        // Coverage percentage sliders
        $('.coverage-percentage').on('input', (e) => {
            this.updateCoverageDisplay(e.target);
        });
    }
    
    initializeRatingSliders() {
        $('.rating-slider').each(function() {
            const slider = $(this);
            const value = slider.val();
            const display = slider.siblings('.rating-display');
            display.text(value);
        });
    }
    
    setupGroupSelection() {
        // Show first group by default
        if (this.groups.length > 0) {
            this.toggleGroupSections(this.groups[0].id);
        }
    }
    
    setupTopicCoverage() {
        // Initialize topic coverage based on planned items
        if (this.hasPlan && this.plannedItems.length > 0) {
            this.populatePlannedTopics();
        }
    }
    
    toggleGroupSections(groupId) {
        $('.group-section').removeClass('active').hide();
        $(`.group-section[data-group-id="${groupId}"]`).addClass('active').show();
    }
    
    populatePlannedTopics() {
        // Populate planned topics for each group
        this.plannedItems.forEach(item => {
            const groupSection = $(`.group-section[data-group-id="${item.studentGroupId}"]`);
            
            // Add planned subtopics
            if (item.plannedSubTopics && item.plannedSubTopics.length > 0) {
                item.plannedSubTopics.forEach(subTopicId => {
                    const subTopic = this.availableSubTopics.find(st => st.id === subTopicId);
                    if (subTopic) {
                        this.addTopicToCoverage(groupSection, 'SubTopic', subTopicId, subTopic.title, true);
                    }
                });
            }
            
            // Add planned lessons
            if (item.plannedLessons && item.plannedLessons.length > 0) {
                item.plannedLessons.forEach(lessonId => {
                    const lesson = this.availableLessons.find(l => l.id === lessonId);
                    if (lesson) {
                        this.addTopicToCoverage(groupSection, 'Lesson', lessonId, lesson.title, true);
                    }
                });
            }
        });
    }
    
    addTopicToCoverage(groupSection, type, id, title, wasPlanned = false) {
        const coverageContainer = groupSection.find('.topic-coverage-container');
        const topicHtml = `
            <div class="topic-coverage-item" data-topic-type="${type}" data-topic-id="${id}">
                <div class="topic-info">
                    <span class="topic-title">${title}</span>
                    ${wasPlanned ? '<span class="planned-badge">برنامه‌ریزی شده</span>' : ''}
                </div>
                <div class="coverage-controls">
                    <label class="coverage-checkbox">
                        <input type="checkbox" name="TopicCoverages[${type}][${id}][WasCovered]" class="coverage-check">
                        <span>پوشش داده شد</span>
                    </label>
                    <input type="range" name="TopicCoverages[${type}][${id}][CoveragePercentage]" 
                           class="coverage-percentage" min="0" max="100" value="0">
                    <span class="coverage-display">0%</span>
                </div>
            </div>
        `;
        coverageContainer.append(topicHtml);
        
        // Setup event listener for the new coverage percentage slider
        coverageContainer.find('.coverage-percentage').last().on('input', (e) => {
            this.updateCoverageDisplay(e.target);
        });
    }
    
    updateRatingDisplay(slider) {
        const value = $(slider).val();
        const display = $(slider).siblings('.rating-display');
        display.text(value);
    }
    
    updateCoverageDisplay(slider) {
        const value = $(slider).val();
        const display = $(slider).siblings('.coverage-display');
        display.text(value + '%');
    }
    
    validateForm(e) {
        let isValid = true;
        
        // Check if at least one group is selected
        const selectedGroups = $('.group-selector:checked');
        if (selectedGroups.length === 0) {
            alert('لطفاً حداقل یک گروه را انتخاب کنید.');
            isValid = false;
        }
        
        // Check if at least one topic is covered
        const coveredTopics = $('.coverage-check:checked');
        if (coveredTopics.length === 0) {
            alert('لطفاً حداقل یک موضوع را به عنوان پوشش داده شده علامت بزنید.');
            isValid = false;
        }
        
        if (!isValid) {
            e.preventDefault();
        }
    }
}

// Initialize when document is ready
$(document).ready(function() {
    // Check if we're on the completion page
    if ($('#completionForm').length > 0) {
        // Get data from the page with safe parsing
        const hasPlan = $('#completionForm').data('has-plan') === 'True';
        
        // Safe JSON parsing with debugging
        const groupsData = $('#completionForm').data('groups');
        console.log('Groups data:', groupsData);
        
        const subTopicsData = $('#completionForm').data('available-subtopics');
        console.log('SubTopics data:', subTopicsData);
        
        const lessonsData = $('#completionForm').data('available-lessons');
        console.log('Lessons data:', lessonsData);
        
        const plannedItemsData = $('#completionForm').data('planned-items');
        console.log('PlannedItems data:', plannedItemsData);
        
        // Initialize completion form manager
        const completionManager = new SessionCompletionManager({
            hasPlan: hasPlan,
            groups: groupsData,
            availableSubTopics: subTopicsData,
            availableLessons: lessonsData,
            plannedItems: plannedItemsData
        });
        
        completionManager.init();
    }
});
