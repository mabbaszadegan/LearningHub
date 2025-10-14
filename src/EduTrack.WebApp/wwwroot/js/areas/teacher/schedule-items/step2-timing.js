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
        const startDatePresets = document.querySelectorAll('.date-preset-btn[data-preset]');
        
        startDatePresets.forEach(btn => {
            btn.addEventListener('click', () => {
                const preset = btn.dataset.preset;
                const targetDate = this.calculateStartDate(preset);
                
                if (targetDate) {
                    this.setStartDate(targetDate);
                    this.updatePresetButtonState(btn, startDatePresets);
                }
            });
        });
    }

    setupDueDatePresets() {
        const dueDatePresets = document.querySelectorAll('.date-preset-btn[data-preset]');
        
        dueDatePresets.forEach(btn => {
            btn.addEventListener('click', () => {
                const preset = btn.dataset.preset;
                const targetDate = this.calculateDueDate(preset);
                
                if (targetDate) {
                    this.setDueDate(targetDate);
                    this.updatePresetButtonState(btn, dueDatePresets);
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
        const startDate = startDateInput ? new Date(startDateInput.value) : new Date();
        
        switch (preset) {
            case '1day':
                return new Date(startDate.getTime() + (1 * 24 * 60 * 60 * 1000));
            case '2days':
                return new Date(startDate.getTime() + (2 * 24 * 60 * 60 * 1000));
            case '3days':
                return new Date(startDate.getTime() + (3 * 24 * 60 * 60 * 1000));
            case '1week':
                return new Date(startDate.getTime() + (7 * 24 * 60 * 60 * 1000));
            case '2weeks':
                return new Date(startDate.getTime() + (14 * 24 * 60 * 60 * 1000));
            case '1month':
                return new Date(startDate.getTime() + (30 * 24 * 60 * 60 * 1000));
            default:
                return null;
        }
    }

    setStartDate(date) {
        const persianStartDateInput = document.getElementById('PersianStartDate');
        const startDateInput = document.getElementById('StartDate');
        const startTimeInput = document.getElementById('StartTime');
        
        if (persianStartDateInput && startDateInput) {
            // Convert to Persian date
            const persianDate = this.convertToPersianDate(date);
            persianStartDateInput.value = persianDate;
            
            // Set hidden field with date
            startDateInput.value = date.toISOString();
            
            // Set time to current time
            if (startTimeInput) {
                const timeString = `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                startTimeInput.value = timeString;
            }
            
            // Trigger datepicker update
            this.updateDatePicker(persianStartDateInput, persianDate);
            
            // Update duration calculator
            this.updateDurationCalculator();
        }
    }

    setDueDate(date) {
        const persianDueDateInput = document.getElementById('PersianDueDate');
        const dueDateInput = document.getElementById('DueDate');
        const dueTimeInput = document.getElementById('DueTime');
        
        if (persianDueDateInput && dueDateInput) {
            // Convert to Persian date
            const persianDate = this.convertToPersianDate(date);
            persianDueDateInput.value = persianDate;
            
            // Set hidden field with date
            dueDateInput.value = date.toISOString();
            
            // Set time to current time
            if (dueTimeInput) {
                const timeString = `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                dueTimeInput.value = timeString;
            }
            
            // Trigger datepicker update
            this.updateDatePicker(persianDueDateInput, persianDate);
            
            // Update duration calculator
            this.updateDurationCalculator();
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

        if (startTimeInput && startDateInput) {
            startTimeInput.addEventListener('change', () => {
                this.updateDateTimeField(startDateInput, startTimeInput);
            });
        }

        if (dueTimeInput && dueDateInput) {
            dueTimeInput.addEventListener('change', () => {
                this.updateDateTimeField(dueDateInput, dueTimeInput);
            });
        }
    }

    updateDateTimeField(dateInput, timeInput) {
        if (dateInput.value && timeInput.value) {
            const date = new Date(dateInput.value);
            const [hours, minutes] = timeInput.value.split(':');
            date.setHours(parseInt(hours), parseInt(minutes));
            dateInput.value = date.toISOString();
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
            const year = date.getFullYear() - 621;
            const month = date.getMonth() + 1;
            const day = date.getDate();
            return `${year}/${month.toString().padStart(2, '0')}/${day.toString().padStart(2, '0')}`;
            
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

    updateDatePicker(inputElement, persianDate) {
        try {
            // Try to update the datepicker if it exists
            if (inputElement._persianDatePicker && typeof inputElement._persianDatePicker.setDate === 'function') {
                inputElement._persianDatePicker.setDate(persianDate);
            } else {
                // Trigger change event to update the calendar
                const changeEvent = new Event('change', { bubbles: true });
                inputElement.dispatchEvent(changeEvent);
                
                // Also trigger input event
                const inputEvent = new Event('input', { bubbles: true });
                inputElement.dispatchEvent(inputEvent);
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
            if (persianStartDateInput.datePicker) {
                persianStartDateInput.datePicker.setDate(persianStartDateInput.value);
            } else {
                console.warn('step2-timing: PersianStartDate datepicker not found');
            }
        }
        
        // Update PersianDueDate datepicker
        const persianDueDateInput = document.getElementById('PersianDueDate');
        if (persianDueDateInput && persianDueDateInput.value) {
            console.log('step2-timing: Updating PersianDueDate datepicker', persianDueDateInput.value);
            if (persianDueDateInput.datePicker) {
                persianDueDateInput.datePicker.setDate(persianDueDateInput.value);
            } else {
                console.warn('step2-timing: PersianDueDate datepicker not found');
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
        if (isoDate) {
            const date = new Date(isoDate);
            const timeString = `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
            const timeInput = document.getElementById(inputId);
            if (timeInput) {
                timeInput.value = timeString;
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
        if (data.persianStartDate) {
            console.log('step2-timing: Setting PersianStartDate', data.persianStartDate);
            const persianStartDate = document.getElementById('PersianStartDate');
            if (persianStartDate) {
                persianStartDate.value = data.persianStartDate;
            }
            const startTime = document.getElementById('StartTime');
            if (startTime) {
                this.updateTimeInput('StartTime', data.startDate);
            }
        }
        if (data.persianDueDate) {
            console.log('step2-timing: Setting PersianDueDate', data.persianDueDate);
            const persianDueDate = document.getElementById('PersianDueDate');
            if (persianDueDate) {
                persianDueDate.value = data.persianDueDate;
            }
            const dueTime = document.getElementById('DueTime');
            if (dueTime) {
                this.updateTimeInput('DueTime', data.dueDate);
            }
        }
        if (data.maxScore) {
            const maxScore = document.getElementById('maxScore');
            if (maxScore) {
                maxScore.value = data.maxScore;
            }
        }
        if (data.isMandatory) {
            const isMandatory = document.getElementById('isMandatory');
            if (isMandatory) {
                isMandatory.checked = data.isMandatory;
            }
        }

        // Update datepickers and UI after form is populated
        setTimeout(() => {
            this.updateDatePickers();
            this.updateDurationCalculator();
            this.toggleScoreDisplay(); // Update score display based on item type
        }, 200);
    }
}

window.Step2TimingManager = Step2TimingManager;
