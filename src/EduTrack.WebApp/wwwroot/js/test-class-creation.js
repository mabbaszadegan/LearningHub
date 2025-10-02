// Test script for class creation functionality
$(document).ready(function() {
    console.log('Class creation test script loaded');
    
    // Test Persian date conversion
    if (window.persianDate) {
        console.log('Persian date utility loaded successfully');
        
        // Test conversion
        const today = window.persianDate.getTodayPersian();
        console.log('Today in Persian:', today);
        
        const testDate = '1403/01/15';
        const gregorianDate = window.persianDate.persianStringToGregorianDate(testDate);
        console.log('Persian date', testDate, 'converted to Gregorian:', gregorianDate);
    } else {
        console.error('Persian date utility not loaded');
    }
    
    // Test form validation
    if (window.formValidator) {
        console.log('Form validator loaded successfully');
    } else {
        console.error('Form validator not loaded');
    }
    
    // Check if teachers are loaded from database
    const teacherSelect = document.querySelector('select[name="TeacherId"]');
    if (teacherSelect) {
        const options = teacherSelect.querySelectorAll('option');
        console.log('Teacher options loaded:', options.length - 1); // -1 for placeholder
        
        options.forEach((option, index) => {
            if (index > 0) { // Skip placeholder
                console.log('Teacher option:', option.value, option.text);
            }
        });
    }
    
    // Check if courses are loaded
    const courseSelect = document.querySelector('select[name="CourseId"]');
    if (courseSelect) {
        const options = courseSelect.querySelectorAll('option');
        console.log('Course options loaded:', options.length - 1); // -1 for placeholder
        
        options.forEach((option, index) => {
            if (index > 0) { // Skip placeholder
                console.log('Course option:', option.value, option.text);
            }
        });
    }
});
