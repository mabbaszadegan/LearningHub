/**
 * Step 2 Timing Manager
 * Handles date/time selection, Persian date picker, score management and duration calculation
 */

class Step2TimingManager {
    constructor(formManager) {
        this.formManager = formManager;
        this.init();
    }

    init() {
        this.setupDatetimeHelpers();
        this.setupPersianDatePicker();
    }

    setupDatetimeHelpers() {
        // Setup start date preset buttons
        this.setupStartDatePresets();
        
        // Setup due date preset buttons
        this.setupDueDatePresets();
        
        // Setup conditional score display
        this.setupConditionalScoreDisplay();
    }

    setupStartDatePresets() {
        // فقط دکمه‌های تاریخ شروع را انتخاب کن (دکمه‌هایی که در بخش تاریخ شروع هستند)
        const startDateSection = document.querySelector('.required-item:has(#PersianStartDate)');
        if (!startDateSection) {
            console.warn('Start date section not found');
            return;
        }
        
        const startDatePresets = startDateSection.querySelectorAll('.date-preset-btn[data-preset]');
        console.log('Found start date presets:', startDatePresets.length);
        
        startDatePresets.forEach(btn => {
            btn.addEventListener('click', () => {
                const preset = btn.dataset.preset;
                console.log('Start date preset clicked:', preset);
                const targetDate = this.calculateStartDate(preset);
                
                if (targetDate) {
                    this.setStartDate(targetDate);
                    this.updatePresetButtonState(btn, startDatePresets);
                    
                    // Dispatch timing changed event
                    document.dispatchEvent(new CustomEvent('timingChanged', {
                        detail: { type: 'startDatePreset', preset: preset, date: targetDate }
                    }));
                }
            });
        });
    }

    setupDueDatePresets() {
        // فقط دکمه‌های تاریخ مهلت را انتخاب کن (دکمه‌هایی که در بخش تاریخ مهلت هستند)
        const dueDateSection = document.querySelector('.required-item:has(#PersianDueDate)');
        if (!dueDateSection) {
            console.warn('Due date section not found');
            return;
        }
        
        const dueDatePresets = dueDateSection.querySelectorAll('.date-preset-btn[data-preset]');
        console.log('Found due date presets:', dueDatePresets.length);
        
        dueDatePresets.forEach(btn => {
            btn.addEventListener('click', () => {
                const preset = btn.dataset.preset;
                console.log('Due date preset clicked:', preset);
                const targetDate = this.calculateDueDate(preset);
                
                if (targetDate) {
                    this.setDueDate(targetDate);
                    this.updatePresetButtonState(btn, dueDatePresets);
                    
                    // Dispatch timing changed event
                    document.dispatchEvent(new CustomEvent('timingChanged', {
                        detail: { type: 'dueDatePreset', preset: preset, date: targetDate }
                    }));
                }
            });
        });
    }

    calculateStartDate(preset) {
        const now = new Date();
        
        switch (preset) {
            case 'now':
                return now;
            case 'tomorrow':
                return new Date(now.getTime() + (1 * 24 * 60 * 60 * 1000));
            case '2days':
                return new Date(now.getTime() + (2 * 24 * 60 * 60 * 1000));
            case 'nextweek':
                return new Date(now.getTime() + (7 * 24 * 60 * 60 * 1000));
            case '2weeks':
                return new Date(now.getTime() + (14 * 24 * 60 * 60 * 1000));
            case 'nextmonth':
                return new Date(now.getTime() + (30 * 24 * 60 * 60 * 1000));
            case '2months':
                return new Date(now.getTime() + (60 * 24 * 60 * 60 * 1000));
            default:
                return null;
        }
    }

    calculateDueDate(preset) {
        const startDateInput = document.getElementById('StartDate');
        const persianStartDateInput = document.getElementById('PersianStartDate');
        
        let startDate;
        
        // Try to get start date from hidden field first
        if (startDateInput && startDateInput.value) {
            startDate = new Date(startDateInput.value);
            console.log('Using start date from hidden field:', startDate);
            console.log('Hidden field value:', startDateInput.value);
        } else if (persianStartDateInput && persianStartDateInput.value) {
            // Convert Persian date to Gregorian
            startDate = this.convertPersianToGregorian(persianStartDateInput.value);
            console.log('Using start date from Persian field:', startDate);
            console.log('Persian field value:', persianStartDateInput.value);
        } else {
            // Use current date as fallback
            startDate = new Date();
            console.log('Using current date as fallback:', startDate);
        }
        
        // Validate the date
        if (!startDate || isNaN(startDate.getTime())) {
            console.error('Invalid start date, using current date');
            startDate = new Date();
        }
        
        console.log('Calculating due date from start date:', startDate);
        
        let dueDate;
        switch (preset) {
            case '1day':
                dueDate = new Date(startDate.getTime() + (1 * 24 * 60 * 60 * 1000));
                break;
            case '2days':
                dueDate = new Date(startDate.getTime() + (2 * 24 * 60 * 60 * 1000));
                break;
            case '3days':
                dueDate = new Date(startDate.getTime() + (3 * 24 * 60 * 60 * 1000));
                break;
            case '1week':
                dueDate = new Date(startDate.getTime() + (7 * 24 * 60 * 60 * 1000));
                break;
            case '2weeks':
                dueDate = new Date(startDate.getTime() + (14 * 24 * 60 * 60 * 1000));
                break;
            case '1month':
                dueDate = new Date(startDate.getTime() + (30 * 24 * 60 * 60 * 1000));
                break;
            default:
                return null;
        }
        
        console.log('Calculated due date:', dueDate);
        return dueDate;
    }

    setStartDate(date) {
        const persianStartDateInput = document.getElementById('PersianStartDate');
        const startDateInput = document.getElementById('StartDate');
        const startTimeInput = document.getElementById('StartTime');
        
        if (persianStartDateInput && startDateInput) {
            console.log('Setting start date:', date);
            
            // Validate the date
            if (!date || isNaN(date.getTime())) {
                console.error('Invalid start date provided');
                return;
            }
            
            // Convert to Persian date
            const persianDate = this.convertToPersianDate(date);
            console.log('Converted to Persian:', persianDate);
            
            if (persianDate && persianDate !== '0000/00/00') {
                persianStartDateInput.value = persianDate;
                
                // Set hidden field with date
                startDateInput.value = date.toISOString();
                console.log('Updated startDateInput with:', startDateInput.value);
                
                // Set time to current time
                if (startTimeInput) {
                    const timeString = `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                    startTimeInput.value = timeString;
                }
                
                // Trigger datepicker update
                this.updateDatePicker(persianStartDateInput, persianDate);
                
                // Update duration calculator
                this.updateDurationCalculator();
            } else {
                console.error('Failed to convert start date to Persian');
            }
        }
    }

    setDueDate(date) {
        const persianDueDateInput = document.getElementById('PersianDueDate');
        const dueDateInput = document.getElementById('DueDate');
        const dueTimeInput = document.getElementById('DueTime');
        const startTimeInput = document.getElementById('StartTime');
        
        if (persianDueDateInput && dueDateInput) {
            console.log('Setting due date:', date);
            
            // Validate the date
            if (!date || isNaN(date.getTime())) {
                console.error('Invalid due date provided');
                return;
            }
            
            // Convert to Persian date
            const persianDate = this.convertToPersianDate(date);
            console.log('Converted to Persian:', persianDate);
            
            if (persianDate && persianDate !== '0000/00/00') {
                persianDueDateInput.value = persianDate;
                
                // Set hidden field with date
                dueDateInput.value = date.toISOString();
                
                // Set time to start time if available, otherwise current time
                if (dueTimeInput) {
                    let timeString;
                    if (startTimeInput && startTimeInput.value) {
                        // Use start time
                        timeString = startTimeInput.value;
                        console.log('Using start time:', timeString);
                    } else {
                        // Use current time
                        timeString = `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                        console.log('Using current time:', timeString);
                    }
                    dueTimeInput.value = timeString;
                }
                
                // Trigger datepicker update
                this.updateDatePicker(persianDueDateInput, persianDate);
                
                // Update duration calculator
                this.updateDurationCalculator();
            } else {
                console.error('Failed to convert due date to Persian');
            }
        }
    }

    updatePresetButtonState(activeBtn, allButtons) {
        // Remove active class from all buttons in the same group
        allButtons.forEach(btn => {
            if (btn.closest('.date-presets') === activeBtn.closest('.date-presets')) {
                btn.classList.remove('active');
            }
        });
        
        // Add active class to clicked button
        activeBtn.classList.add('active');
    }

    setupConditionalScoreDisplay() {
        // Check if we're in step 2 and hide score for reminder items
        const itemTypeSelect = document.getElementById('itemType');
        const maxScoreSection = document.getElementById('maxScoreSection');
        
        if (itemTypeSelect && maxScoreSection) {
            // Initial check
            this.toggleScoreDisplay();
            
            // Listen for type changes
            itemTypeSelect.addEventListener('change', () => {
                this.toggleScoreDisplay();
            });
        }
    }

    toggleScoreDisplay() {
        const itemTypeSelect = document.getElementById('itemType');
        const maxScoreSection = document.getElementById('maxScoreSection');
        
        if (itemTypeSelect && maxScoreSection) {
            const selectedType = parseInt(itemTypeSelect.value);
            
            // Hide score section for reminder items (Reminder = 0 in ScheduleItemType enum)
            const isReminderType = selectedType === 0;
            
            if (isReminderType) {
                maxScoreSection.style.display = 'none';
            } else {
                maxScoreSection.style.display = 'block';
            }
        }
    }

    setupScorePresets() {
        document.querySelectorAll('.score-preset').forEach(preset => {
            preset.addEventListener('click', (e) => {
                const score = e.currentTarget.dataset.score;
                const maxScoreInput = document.getElementById('maxScore');
                if (maxScoreInput) {
                    maxScoreInput.value = score;
                    this.updateScorePresets(score);
                }
            });
        });

        const maxScoreInput = document.getElementById('maxScore');
        if (maxScoreInput) {
            maxScoreInput.addEventListener('input', (e) => {
                this.updateScorePresets(e.target.value);
            });
        }
    }

    updateDurationCalculator() {
        const startDateInput = document.getElementById('StartDate');
        const dueDateInput = document.getElementById('DueDate');
        const calculator = document.getElementById('durationCalculator');
        const durationValue = document.getElementById('durationValue');

        if (!startDateInput || !dueDateInput || !calculator || !durationValue) return;

        const startDate = new Date(startDateInput.value);
        const dueDate = new Date(dueDateInput.value);

        if (startDateInput.value && dueDateInput.value && dueDate > startDate) {
            const diffMs = dueDate - startDate;
            const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
            const diffHours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

            durationValue.textContent = `${diffDays} روز و ${diffHours} ساعت`;
            calculator.style.display = 'block';
        } else {
            calculator.style.display = 'none';
        }
    }

    updateScorePresets(score) {
        document.querySelectorAll('.score-preset').forEach(preset => {
            preset.classList.toggle('active', preset.dataset.score === score);
        });
    }

    // Persian Date Picker Setup
    setupPersianDatePicker() {
        // Wait for DOM to be ready
        setTimeout(() => {
            // Initialize Persian date pickers
            if (typeof initializeAllDatePickers === 'function') {
                initializeAllDatePickers();
            }

            // Setup custom datepicker events
            this.setupDatePickerEvents();

            // Setup time inputs
            this.setupTimeInputs();
        }, 100);
    }

    setupDatePickerEvents() {
        // Setup events for Persian date pickers
        const persianDateInputs = document.querySelectorAll('.persian-datepicker');
        
        persianDateInputs.forEach(input => {
            input.addEventListener('click', () => {
                // Ensure datepicker is properly initialized
                if (!input._persianDatePicker) {
                    setTimeout(() => {
                        if (typeof initializeAllDatePickers === 'function') {
                            initializeAllDatePickers();
                        }
                    }, 50);
                }
            });
        });
    }

    setupTimeInputs() {
        const startTimeInput = document.getElementById('StartTime');
        const dueTimeInput = document.getElementById('DueTime');
        const startDateInput = document.getElementById('StartDate');
        const dueDateInput = document.getElementById('DueDate');
        const persianStartDateInput = document.getElementById('PersianStartDate');

        if (startTimeInput && startDateInput) {
            startTimeInput.addEventListener('change', () => {
                this.updateDateTimeField(startDateInput, startTimeInput);
                this.updateDurationCalculator();
                
                // Dispatch timing changed event
                document.dispatchEvent(new CustomEvent('timingChanged', {
                    detail: { type: 'startTime', value: startTimeInput.value }
                }));
            });
        }

        if (dueTimeInput && dueDateInput) {
            dueTimeInput.addEventListener('change', () => {
                this.updateDateTimeField(dueDateInput, dueTimeInput);
                this.updateDurationCalculator();
                
                // Dispatch timing changed event
                document.dispatchEvent(new CustomEvent('timingChanged', {
                    detail: { type: 'dueTime', value: dueTimeInput.value }
                }));
            });
        }

        // Add event listener for start date changes to update due date presets
        if (persianStartDateInput) {
            persianStartDateInput.addEventListener('change', () => {
                // Update the hidden StartDate field when Persian date changes
                if (persianStartDateInput.value) {
                    try {
                        // Convert Persian date to Gregorian date
                        const persianDate = persianStartDateInput.value;
                        const gregorianDate = this.convertPersianToGregorian(persianDate);
                        if (gregorianDate && startDateInput) {
                            startDateInput.value = gregorianDate.toISOString();
                            console.log('Updated startDateInput with:', startDateInput.value);
                        }
                    } catch (error) {
                        console.error('Error converting Persian date to Gregorian:', error);
                    }
                }
                this.updateDurationCalculator();
                
                // Dispatch timing changed event
                document.dispatchEvent(new CustomEvent('timingChanged', {
                    detail: { type: 'startDate', value: persianStartDateInput.value }
                }));
            });
        }

        if (startDateInput) {
            startDateInput.addEventListener('change', () => {
                this.updateDurationCalculator();
            });
        }

        // Add event listener for due date changes
        const persianDueDateInput = document.getElementById('PersianDueDate');
        if (persianDueDateInput) {
            persianDueDateInput.addEventListener('change', () => {
                // Update the hidden DueDate field when Persian date changes
                if (persianDueDateInput.value) {
                    try {
                        // Convert Persian date to Gregorian date
                        const persianDate = persianDueDateInput.value;
                        const gregorianDate = this.convertPersianToGregorian(persianDate);
                        if (gregorianDate && dueDateInput) {
                            dueDateInput.value = gregorianDate.toISOString();
                        }
                    } catch (error) {
                        console.error('Error converting Persian date to Gregorian:', error);
                    }
                }
                this.updateDurationCalculator();
                
                // Dispatch timing changed event
                document.dispatchEvent(new CustomEvent('timingChanged', {
                    detail: { type: 'dueDate', value: persianDueDateInput.value }
                }));
            });
        }
    }

    updateDateTimeField(dateInput, timeInput) {
        if (dateInput.value && timeInput.value) {
            const date = new Date(dateInput.value);
            const [hours, minutes] = timeInput.value.split(':');
            date.setHours(parseInt(hours), parseInt(minutes));
            dateInput.value = date.toISOString();
            console.log('Updated dateTimeField:', dateInput.id, 'with value:', dateInput.value);
        }
    }

    convertToPersianDate(date) {
        try {
            // Validate input
            if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
                console.warn('Invalid date provided to convertToPersianDate');
                return '0000/00/00';
            }

            // Use official jalaali-js library if available
            if (typeof window.jalaali !== 'undefined' && window.jalaali.toJalaali) {
                try {
                    const jalaali = window.jalaali.toJalaali(date);
                    return `${jalaali.jy}/${jalaali.jm.toString().padStart(2, '0')}/${jalaali.jd.toString().padStart(2, '0')}`;
                } catch (jalaaliError) {
                    console.warn('jalaali-js library error:', jalaaliError);
                    // Fall through to next method
                }
            }
            
            // Fallback to old PersianDate library if available
            if (typeof PersianDate !== 'undefined') {
                try {
                    const persianDate = PersianDate.fromDate(date);
                    return persianDate.format('YYYY/MM/DD');
                } catch (persianError) {
                    console.warn('PersianDate library error:', persianError);
                    // Fall through to next method
                }
            }
            
            // Fallback to persianDateUtils if available
            if (typeof window.persianDateUtils !== 'undefined' && window.persianDateUtils.gregorianToPersian) {
                try {
                    return window.persianDateUtils.gregorianToPersian(date);
                } catch (utilsError) {
                    console.warn('persianDateUtils error:', utilsError);
                    // Fall through to next method
                }
            }
            
            // Final fallback: simple approximation
            console.warn('Using simple Persian date approximation');
            const gregorianYear = date.getFullYear();
            const gregorianMonth = date.getMonth() + 1;
            const gregorianDay = date.getDate();
            
            // More accurate Gregorian to Persian conversion
            let persianYear = gregorianYear - 621;
            let persianMonth = gregorianMonth;
            let persianDay = gregorianDay;
            
            // Adjust for Persian calendar differences
            if (gregorianMonth >= 3 && gregorianMonth <= 5) {
                persianMonth = gregorianMonth - 2;
            } else if (gregorianMonth >= 6 && gregorianMonth <= 8) {
                persianMonth = gregorianMonth - 2;
            } else if (gregorianMonth >= 9 && gregorianMonth <= 11) {
                persianMonth = gregorianMonth - 2;
            } else if (gregorianMonth >= 12 || gregorianMonth <= 2) {
                persianMonth = gregorianMonth + 10;
                if (gregorianMonth <= 2) {
                    persianYear -= 1;
                }
            }
            
            // Handle month boundaries
            if (persianMonth > 12) {
                persianMonth -= 12;
                persianYear += 1;
            }
            
            console.log(`Converting ${gregorianYear}/${gregorianMonth}/${gregorianDay} to ${persianYear}/${persianMonth}/${persianDay}`);
            return `${persianYear}/${persianMonth.toString().padStart(2, '0')}/${persianDay.toString().padStart(2, '0')}`;
            
        } catch (error) {
            console.error('Error converting to Persian date:', error);
            
            // Return current date as fallback
            const now = new Date();
            const year = now.getFullYear() - 621;
            const month = now.getMonth() + 1;
            const day = now.getDate();
            return `${year}/${month.toString().padStart(2, '0')}/${day.toString().padStart(2, '0')}`;
        }
    }

    convertPersianToGregorian(persianDateString) {
        try {
            if (!persianDateString || persianDateString === '0000/00/00') {
                return null;
            }

            // Use official jalaali-js library if available
            if (typeof window.jalaali !== 'undefined' && window.jalaali.toGregorian) {
                try {
                    const parts = persianDateString.split('/');
                    if (parts.length === 3) {
                        const jy = parseInt(parts[0]);
                        const jm = parseInt(parts[1]);
                        const jd = parseInt(parts[2]);
                        const gregorian = window.jalaali.toGregorian(jy, jm, jd);
                        return new Date(gregorian.gy, gregorian.gm - 1, gregorian.gd);
                    }
                } catch (jalaaliError) {
                    console.warn('jalaali-js library error:', jalaaliError);
                    // Fall through to next method
                }
            }
            
            // Fallback to old PersianDate library if available
            if (typeof PersianDate !== 'undefined') {
                try {
                    const persianDate = PersianDate.parse(persianDateString);
                    return persianDate.toDate();
                } catch (persianError) {
                    console.warn('PersianDate library error:', persianError);
                    // Fall through to next method
                }
            }
            
            // Fallback to persianDateUtils if available
            if (typeof window.persianDateUtils !== 'undefined' && window.persianDateUtils.persianToGregorian) {
                try {
                    return window.persianDateUtils.persianToGregorian(persianDateString);
                } catch (utilsError) {
                    console.warn('persianDateUtils error:', utilsError);
                    // Fall through to next method
                }
            }
            
            // Final fallback: simple approximation
            console.warn('Using simple Gregorian date approximation');
            const parts = persianDateString.split('/');
            if (parts.length === 3) {
                const persianYear = parseInt(parts[0]);
                const persianMonth = parseInt(parts[1]);
                const persianDay = parseInt(parts[2]);
                
                // More accurate Persian to Gregorian conversion
                let gregorianYear = persianYear + 621;
                let gregorianMonth = persianMonth;
                let gregorianDay = persianDay;
                
                // Adjust for Persian calendar differences
                if (persianMonth <= 6) {
                    gregorianMonth = persianMonth + 3;
                } else {
                    gregorianMonth = persianMonth - 9;
                    gregorianYear += 1;
                }
                
                // Handle month boundaries
                if (gregorianMonth > 12) {
                    gregorianMonth -= 12;
                    gregorianYear += 1;
                }
                
                console.log(`Converting ${persianDateString} to ${gregorianYear}/${gregorianMonth}/${gregorianDay}`);
                return new Date(gregorianYear, gregorianMonth - 1, gregorianDay);
            }
            
            return null;
            
        } catch (error) {
            console.error('Error converting Persian date to Gregorian:', error);
            return null;
        }
    }

    updateDatePicker(inputElement, persianDate) {
        try {
            console.log('Updating datepicker for:', inputElement.id, 'with date:', persianDate);
            
            // Try to update the datepicker if it exists
            if (inputElement._persianDatePicker && typeof inputElement._persianDatePicker.setDate === 'function') {
                console.log('Using _persianDatePicker.setDate');
                inputElement._persianDatePicker.setDate(persianDate);
            } else if (inputElement.datePicker && typeof inputElement.datePicker.setDate === 'function') {
                console.log('Using datePicker.setDate');
                inputElement.datePicker.setDate(persianDate);
            } else {
                console.log('Triggering events to update calendar');
                // Trigger change event to update the calendar
                const changeEvent = new Event('change', { bubbles: true });
                inputElement.dispatchEvent(changeEvent);
                
                // Also trigger input event
                const inputEvent = new Event('input', { bubbles: true });
                inputElement.dispatchEvent(inputEvent);
                
                // Try to trigger focus and blur to refresh the datepicker
                inputElement.focus();
                setTimeout(() => {
                    inputElement.blur();
                }, 100);
            }
        } catch (error) {
            console.error('Error updating datepicker:', error);
        }
    }

    updateDatePickers() {
        console.log('step2-timing: Updating datepickers');
        
        // Update PersianStartDate datepicker
        const persianStartDateInput = document.getElementById('PersianStartDate');
        if (persianStartDateInput && persianStartDateInput.value) {
            console.log('step2-timing: Updating PersianStartDate datepicker', persianStartDateInput.value);
            // Try different possible datepicker property names
            const datePicker = persianStartDateInput.datePicker || persianStartDateInput._persianDatePicker;
            if (datePicker && typeof datePicker.setDate === 'function') {
                datePicker.setDate(persianStartDateInput.value);
            } else {
                console.warn('step2-timing: PersianStartDate datepicker not found or setDate method not available');
                // Try to trigger change event
                const changeEvent = new Event('change', { bubbles: true });
                persianStartDateInput.dispatchEvent(changeEvent);
            }
        }
        
        // Update PersianDueDate datepicker
        const persianDueDateInput = document.getElementById('PersianDueDate');
        if (persianDueDateInput && persianDueDateInput.value) {
            console.log('step2-timing: Updating PersianDueDate datepicker', persianDueDateInput.value);
            // Try different possible datepicker property names
            const datePicker = persianDueDateInput.datePicker || persianDueDateInput._persianDatePicker;
            if (datePicker && typeof datePicker.setDate === 'function') {
                datePicker.setDate(persianDueDateInput.value);
            } else {
                console.warn('step2-timing: PersianDueDate datepicker not found or setDate method not available');
                // Try to trigger change event
                const changeEvent = new Event('change', { bubbles: true });
                persianDueDateInput.dispatchEvent(changeEvent);
            }
        }
    }

    updatePersianDateDisplay(inputId, isoDate) {
        if (isoDate) {
            const date = new Date(isoDate);
            const persianDateString = this.convertToPersianDate(date);
            const inputElement = document.getElementById(inputId);
            if (inputElement && persianDateString !== '0000/00/00') {
                inputElement.value = persianDateString;
                // Update the datepicker if it exists
                this.updateDatePicker(inputElement, persianDateString);
            }
        }
    }

    updateTimeInput(inputId, isoDate) {
        console.log(`step2-timing: updateTimeInput called for ${inputId} with date:`, isoDate);
        if (isoDate) {
            try {
                const date = new Date(isoDate);
                if (!isNaN(date.getTime())) {
                    const timeString = `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                    const timeInput = document.getElementById(inputId);
                    if (timeInput) {
                        timeInput.value = timeString;
                        console.log(`step2-timing: Set ${inputId} to ${timeString}`);
                    } else {
                        console.warn(`step2-timing: Time input ${inputId} not found`);
                    }
                } else {
                    console.warn(`step2-timing: Invalid date format: ${isoDate}`);
                }
            } catch (error) {
                console.error(`step2-timing: Error updating time input ${inputId}:`, error);
            }
        }
    }

    // Data collection for step 2
    collectStep2Data() {
        return {
            PersianStartDate: document.getElementById('PersianStartDate')?.value,
            PersianDueDate: document.getElementById('PersianDueDate')?.value,
            StartTime: document.getElementById('StartTime')?.value,
            DueTime: document.getElementById('DueTime')?.value,
            MaxScore: parseFloat(document.getElementById('maxScore')?.value) || null,
            IsMandatory: document.getElementById('isMandatory')?.checked || false
        };
    }

    // Load step 2 data from existing item
    async loadStepData() {
        if (this.formManager && typeof this.formManager.getExistingItemData === 'function') {
            const existingData = this.formManager.getExistingItemData();
            if (existingData) {
                console.log('Loading Step 2 data:', existingData);
                this.populateStep2Data(existingData);
                console.log('Step 2 data loaded from existing item');
            } else {
                console.log('No existing data found for Step 2');
            }
        } else {
            console.log('FormManager or getExistingItemData not available');
        }
    }

    // Populate step 2 with existing data
    populateStep2Data(data) {
        console.log('step2-timing: populateStep2Data called with data:', data);
        
        // Handle start date and time separately
        let startDateField = data.persianStartDate || data.PersianStartDate;
        
        // If Persian date is not available, convert from Gregorian date
        if (!startDateField) {
            const startDateTime = data.startDate || data.StartDate || data.startDateTime;
            if (startDateTime) {
                console.log('step2-timing: Converting start date from Gregorian to Persian:', startDateTime);
                const gregorianDate = new Date(startDateTime);
                if (!isNaN(gregorianDate.getTime())) {
                    startDateField = this.convertToPersianDate(gregorianDate);
                    console.log('step2-timing: Converted Persian start date:', startDateField);
                }
            }
        }
        
        if (startDateField) {
            console.log('step2-timing: Setting PersianStartDate', startDateField);
            const persianStartDate = document.getElementById('PersianStartDate');
            const startDateInput = document.getElementById('StartDate');
            if (persianStartDate) {
                persianStartDate.value = startDateField;
                
                // Update hidden field
                if (startDateInput) {
                    const gregorianDate = this.convertPersianToGregorian(startDateField);
                    if (gregorianDate) {
                        startDateInput.value = gregorianDate.toISOString();
                        console.log('Updated startDateInput with:', startDateInput.value);
                    }
                }
            }
        }
        
        // Handle start time separately
        const startDateTime = data.startDate || data.StartDate || data.startDateTime;
        if (startDateTime) {
            console.log('step2-timing: Setting StartTime from', startDateTime);
            this.updateTimeInput('StartTime', startDateTime);
        }
        
        // Handle due date and time separately
        let dueDateField = data.persianDueDate || data.PersianDueDate;
        
        // If Persian date is not available, convert from Gregorian date
        if (!dueDateField) {
            const dueDateTime = data.dueDate || data.DueDate || data.dueDateTime;
            if (dueDateTime) {
                console.log('step2-timing: Converting due date from Gregorian to Persian:', dueDateTime);
                const gregorianDate = new Date(dueDateTime);
                if (!isNaN(gregorianDate.getTime())) {
                    dueDateField = this.convertToPersianDate(gregorianDate);
                    console.log('step2-timing: Converted Persian due date:', dueDateField);
                }
            }
        }
        
        if (dueDateField) {
            console.log('step2-timing: Setting PersianDueDate', dueDateField);
            const persianDueDate = document.getElementById('PersianDueDate');
            const dueDateInput = document.getElementById('DueDate');
            if (persianDueDate) {
                persianDueDate.value = dueDateField;
                
                // Update hidden field
                if (dueDateInput) {
                    const gregorianDate = this.convertPersianToGregorian(dueDateField);
                    if (gregorianDate) {
                        dueDateInput.value = gregorianDate.toISOString();
                        console.log('Updated dueDateInput with:', dueDateInput.value);
                    }
                }
            }
        }
        
        // Handle due time separately
        const dueDateTime = data.dueDate || data.DueDate || data.dueDateTime;
        if (dueDateTime) {
            console.log('step2-timing: Setting DueTime from', dueDateTime);
            this.updateTimeInput('DueTime', dueDateTime);
        }
        
        // Try different possible field names for max score
        const maxScoreField = data.maxScore || data.MaxScore;
        if (maxScoreField) {
            console.log('step2-timing: Setting MaxScore', maxScoreField);
            const maxScore = document.getElementById('maxScore');
            if (maxScore) {
                maxScore.value = maxScoreField;
            }
        }
        
        // Try different possible field names for mandatory
        const mandatoryField = data.isMandatory || data.IsMandatory;
        if (mandatoryField !== undefined) {
            console.log('step2-timing: Setting IsMandatory', mandatoryField);
            const isMandatory = document.getElementById('isMandatory');
            if (isMandatory) {
                isMandatory.checked = mandatoryField;
            }
        }

        // Update datepickers and UI after form is populated
        setTimeout(() => {
            console.log('step2-timing: Updating datepickers and UI...');
            this.updateDatePickers();
            this.updateDurationCalculator();
            this.toggleScoreDisplay(); // Update score display based on item type
            console.log('step2-timing: UI updates completed');
        }, 200);
    }
}

window.Step2TimingManager = Step2TimingManager;
