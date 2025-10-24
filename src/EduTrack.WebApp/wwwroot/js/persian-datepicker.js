// Persian DatePicker Component
class PersianDatePicker {
    constructor(inputElement, options = {}) {
        this.input = inputElement;
        this.options = {
            placeholder: 'انتخاب تاریخ',
            format: 'YYYY/MM/DD',
            minDate: null,
            maxDate: null,
            initialDate: null,
            onSelect: null,
            ...options
        };
        
        this.persianMonths = [
            'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
            'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
        ];
        
        this.persianDays = ['شنبه', 'یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه'];
        
        this.currentDate = new Date();
        this.selectedDate = null;
        
        // Set viewDate and selectedDate with proper fallback
        if (this.options.initialDate) {
            const parsedDate = this.parseDate(this.options.initialDate);
            if (parsedDate) {
                this.selectedDate = { ...parsedDate };
                this.viewDate = { ...parsedDate };
            } else {
                this.viewDate = this.getTodayPersian();
            }
        } else {
            this.viewDate = this.getTodayPersian();
        }
        
        this.init();
    }
    
    init() {
        this.setupInput();
        this.createCalendar();
        this.bindEvents();
        
        // Check if input has a value and no initialDate is provided
        if (this.options.initialDate) {
            this.setDate(this.options.initialDate);
        } else if (this.input.value && this.input.value.trim() !== '') {
            this.setDate(this.input.value);
        } else {
        }
    }
    
    setupInput() {
        this.input.setAttribute('readonly', 'readonly');
        this.input.setAttribute('placeholder', this.options.placeholder);
        this.input.classList.add('persian-datepicker-input');
        this.input.setAttribute('autocomplete', 'off');
        
        // Create wrapper
        this.wrapper = document.createElement('div');
        this.wrapper.className = 'persian-datepicker-wrapper';
        this.input.parentNode.insertBefore(this.wrapper, this.input);
        this.wrapper.appendChild(this.input);
        
        // Add calendar icon
        this.calendarIcon = document.createElement('i');
        this.calendarIcon.className = 'fas fa-calendar-alt persian-datepicker-icon';
        this.wrapper.appendChild(this.calendarIcon);
        
        // Store reference to datepicker instance
        this.input.datePicker = this;
        this.wrapper.datePicker = this;
    }
    
    createCalendar() {
        // Create modal overlay
        this.modal = document.createElement('div');
        this.modal.className = 'persian-datepicker-modal';
        
        // Create calendar container
        this.calendar = document.createElement('div');
        this.calendar.className = 'persian-datepicker-calendar';
        
        // Create modal header
        this.createModalHeader();
        
        // Header
        this.createHeader();
        
        // Days of week
        this.createDaysHeader();
        
        // Days grid
        this.createDaysGrid();
        
        // Footer
        this.createFooter();
        
        // Append calendar to modal
        this.modal.appendChild(this.calendar);
        document.body.appendChild(this.modal);
    }
    
    createModalHeader() {
        const modalHeader = document.createElement('div');
        modalHeader.className = 'datepicker-modal-header';
        
        const title = document.createElement('h3');
        title.className = 'datepicker-modal-title';
        title.textContent = 'انتخاب تاریخ';
        
        const closeBtn = document.createElement('button');
        closeBtn.className = 'datepicker-modal-close';
        closeBtn.innerHTML = '×';
        closeBtn.onclick = () => this.hide();
        
        modalHeader.appendChild(title);
        modalHeader.appendChild(closeBtn);
        this.calendar.appendChild(modalHeader);
    }
    
    createHeader() {
        const header = document.createElement('div');
        header.className = 'datepicker-header';
        
        // Previous year button
        const prevYear = document.createElement('button');
        prevYear.type = 'button';
        prevYear.className = 'datepicker-nav-btn';
        prevYear.innerHTML = '<i class="fas fa-angle-double-right"></i>';
        prevYear.onclick = () => this.changeYear(-1);
        
        // Previous month button
        const prevMonth = document.createElement('button');
        prevMonth.type = 'button';
        prevMonth.className = 'datepicker-nav-btn';
        prevMonth.innerHTML = '<i class="fas fa-angle-right"></i>';
        prevMonth.onclick = () => this.changeMonth(-1);
        
        // Month/Year display
        this.monthYearDisplay = document.createElement('div');
        this.monthYearDisplay.className = 'datepicker-month-year';
        
        // Next month button
        const nextMonth = document.createElement('button');
        nextMonth.type = 'button';
        nextMonth.className = 'datepicker-nav-btn';
        nextMonth.innerHTML = '<i class="fas fa-angle-left"></i>';
        nextMonth.onclick = () => this.changeMonth(1);
        
        // Next year button
        const nextYear = document.createElement('button');
        nextYear.type = 'button';
        nextYear.className = 'datepicker-nav-btn';
        nextYear.innerHTML = '<i class="fas fa-angle-double-left"></i>';
        nextYear.onclick = () => this.changeYear(1);
        
        header.appendChild(prevYear);
        header.appendChild(prevMonth);
        header.appendChild(this.monthYearDisplay);
        header.appendChild(nextMonth);
        header.appendChild(nextYear);
        
        this.calendar.appendChild(header);
    }
    
    createDaysHeader() {
        const daysHeader = document.createElement('div');
        daysHeader.className = 'datepicker-days-header';
        
        this.persianDays.forEach(day => {
            const dayElement = document.createElement('div');
            dayElement.className = 'datepicker-day-header';
            dayElement.textContent = day;
            daysHeader.appendChild(dayElement);
        });
        
        this.calendar.appendChild(daysHeader);
    }
    
    createDaysGrid() {
        this.daysGrid = document.createElement('div');
        this.daysGrid.className = 'datepicker-days-grid';
        this.calendar.appendChild(this.daysGrid);
    }
    
    createFooter() {
        const footer = document.createElement('div');
        footer.className = 'datepicker-footer';
        
        const todayBtn = document.createElement('button');
        todayBtn.type = 'button';
        todayBtn.className = 'datepicker-today-btn';
        todayBtn.innerHTML = '<i class="fas fa-calendar-day"></i> امروز';
        todayBtn.onclick = () => this.selectToday();
        
        const clearBtn = document.createElement('button');
        clearBtn.type = 'button';
        clearBtn.className = 'datepicker-clear-btn';
        clearBtn.innerHTML = '<i class="fas fa-times"></i> پاک کردن';
        clearBtn.onclick = () => this.clearDate();
        
        // Add quick date buttons
        const quickDatesContainer = document.createElement('div');
        quickDatesContainer.className = 'datepicker-quick-dates';
        
        const quickDates = [
            { label: 'فردا', days: 1 },
            { label: 'هفته آینده', days: 7 },
            { label: 'ماه آینده', days: 30 }
        ];
        
        quickDates.forEach(({ label, days }) => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'datepicker-quick-btn';
            btn.textContent = label;
            btn.onclick = () => this.selectQuickDate(days);
            quickDatesContainer.appendChild(btn);
        });
        
        const footerActions = document.createElement('div');
        footerActions.className = 'datepicker-footer-actions';
        footerActions.appendChild(todayBtn);
        footerActions.appendChild(clearBtn);
        
        footer.appendChild(quickDatesContainer);
        footer.appendChild(footerActions);
        
        this.calendar.appendChild(footer);
    }
    
    bindEvents() {
        // Toggle calendar on input click
        this.input.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.show();
        });
        
        // Toggle calendar on icon click
        this.calendarIcon.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.show();
        });
        
        // Close modal when clicking on overlay
        this.modal.addEventListener('click', (e) => {
            if (e.target === this.modal) {
                this.hide();
            }
        });
        
        // Prevent modal from closing when clicking inside calendar
        this.calendar.addEventListener('click', (e) => {
            e.stopPropagation();
        });
        
        // Handle keyboard navigation
        document.addEventListener('keydown', (e) => {
            if (this.modal.classList.contains('show')) {
                switch (e.key) {
                    case 'Escape':
                        this.hide();
                        break;
                    case 'Enter':
                        if (this.selectedDate) {
                            this.hide();
                        }
                        break;
                    case 'ArrowUp':
                        e.preventDefault();
                        this.navigateDate(-7);
                        break;
                    case 'ArrowDown':
                        e.preventDefault();
                        this.navigateDate(7);
                        break;
                    case 'ArrowLeft':
                        e.preventDefault();
                        this.navigateDate(1);
                        break;
                    case 'ArrowRight':
                        e.preventDefault();
                        this.navigateDate(-1);
                        break;
                }
            }
        });
        
        // Reposition on window resize (throttled)
        window.addEventListener('resize', this.throttle(() => {
            if (this.calendar.style.display === 'block') {
                this.positionCalendar();
            }
        }, 100));
        
        // Prevent repositioning on mouse events that might cause flicker
        this.calendar.addEventListener('mouseenter', (e) => {
            e.stopPropagation();
        });
        
        this.calendar.addEventListener('mouseleave', (e) => {
            e.stopPropagation();
        });
        
        // No need for scroll repositioning in modal mode
    }
    
    trapFocus() {
        // Get all focusable elements in the modal
        const focusableElements = this.calendar.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        
        if (focusableElements.length === 0) return;
        
        this.firstFocusableElement = focusableElements[0];
        this.lastFocusableElement = focusableElements[focusableElements.length - 1];
        
        // Focus the first element
        this.firstFocusableElement.focus();
        
        // Add tab trap
        this.tabTrapHandler = (e) => {
            if (e.key === 'Tab') {
                if (e.shiftKey) {
                    if (document.activeElement === this.firstFocusableElement) {
                        e.preventDefault();
                        this.lastFocusableElement.focus();
                    }
                } else {
                    if (document.activeElement === this.lastFocusableElement) {
                        e.preventDefault();
                        this.firstFocusableElement.focus();
                    }
                }
            }
        };
        
        document.addEventListener('keydown', this.tabTrapHandler);
    }
    
    removeFocusTrap() {
        if (this.tabTrapHandler) {
            document.removeEventListener('keydown', this.tabTrapHandler);
            this.tabTrapHandler = null;
        }
        
        // Return focus to input
        this.input.focus();
    }
    
    toggle() {
        if (this.modal.classList.contains('show')) {
            this.hide();
        } else {
            this.show();
        }
    }
    
    show() {
        // Prevent multiple rapid calls
        if (this.isAnimating) return;
        
        // Close any other open calendars
        this.closeOtherCalendars();
        
        // Show modal
        this.modal.classList.add('show');
        this.updateCalendar();
        this.input.classList.add('datepicker-active');
        
        // Focus trap for accessibility
        this.trapFocus();
        
        this.isAnimating = true;
        setTimeout(() => {
            this.isAnimating = false;
        }, 200);
    }
    
    hide() {
        if (this.isAnimating) return;
        
        this.modal.classList.remove('show');
        this.input.classList.remove('datepicker-active');
        
        // Remove focus trap
        this.removeFocusTrap();
        
        this.isAnimating = true;
        setTimeout(() => {
            this.isAnimating = false;
        }, 200);
    }
    
    closeOtherCalendars() {
        // Close any other open datepicker modals
        document.querySelectorAll('.persian-datepicker-modal.show').forEach(modal => {
            if (modal !== this.modal) {
                modal.classList.remove('show');
                // Find the associated datepicker instance and remove active class
                const calendar = modal.querySelector('.persian-datepicker-calendar');
                if (calendar) {
                    // Find the input associated with this calendar
                    document.querySelectorAll('.persian-datepicker-input').forEach(input => {
                        if (input.datePicker && input.datePicker.modal === modal) {
                            input.classList.remove('datepicker-active');
                        }
                    });
                }
            }
        });
    }
    
    // No positioning needed for modal - it's centered automatically
    
    // No viewport adjustment needed for modal
    
    isRTL() {
        return document.dir === 'rtl' || 
               document.documentElement.dir === 'rtl' || 
               document.body.classList.contains('rtl') ||
               getComputedStyle(document.body).direction === 'rtl' ||
               getComputedStyle(document.documentElement).direction === 'rtl';
    }
    
    updateCalendar() {
        this.updateHeader();
        this.updateDays();
    }
    
    updateHeader() {
        const monthName = this.persianMonths[this.viewDate.month - 1];
        this.monthYearDisplay.textContent = `${monthName} ${this.viewDate.year}`;
    }
    
    updateDays() {
        this.daysGrid.innerHTML = '';
        
        const firstDayOfMonth = this.getFirstDayOfMonth(this.viewDate.year, this.viewDate.month);
        const daysInMonth = this.getDaysInMonth(this.viewDate.year, this.viewDate.month);
        const today = this.getTodayPersian();
        
        // Add empty cells for days before the first day of month
        for (let i = 0; i < firstDayOfMonth; i++) {
            const emptyDay = document.createElement('div');
            emptyDay.className = 'datepicker-day empty';
            this.daysGrid.appendChild(emptyDay);
        }
        
        // Add days of the month
        for (let day = 1; day <= daysInMonth; day++) {
            const dayElement = document.createElement('div');
            dayElement.className = 'datepicker-day';
            dayElement.textContent = day;
            
            const currentDate = {
                year: this.viewDate.year,
                month: this.viewDate.month,
                day: day
            };
            
            // Check if it's today
            if (this.isSameDate(currentDate, today)) {
                dayElement.classList.add('today');
            }
            
            // Check if it's selected
            if (this.selectedDate && this.isSameDate(currentDate, this.selectedDate)) {
                dayElement.classList.add('selected');
            }
            
            // Check if it's disabled
            if (this.isDateDisabled(currentDate)) {
                dayElement.classList.add('disabled');
            } else {
                dayElement.addEventListener('click', () => {
                    this.selectDate(currentDate);
                });
            }
            
            this.daysGrid.appendChild(dayElement);
        }
    }
    
    changeMonth(delta) {
        this.viewDate.month += delta;
        if (this.viewDate.month > 12) {
            this.viewDate.month = 1;
            this.viewDate.year++;
        } else if (this.viewDate.month < 1) {
            this.viewDate.month = 12;
            this.viewDate.year--;
        }
        this.updateCalendar();
    }
    
    changeYear(delta) {
        this.viewDate.year += delta;
        this.updateCalendar();
    }
    
    selectDate(date) {
        if (this.isDateDisabled(date)) return;
        
        this.selectedDate = { ...date };
        const dateString = this.formatDate(date);
        this.input.value = dateString;
        
        // Trigger change event
        const changeEvent = new Event('change', { bubbles: true });
        this.input.dispatchEvent(changeEvent);
        
        // Call callback if provided
        if (this.options.onSelect) {
            this.options.onSelect(date, dateString);
        }
        
        this.hide();
    }
    
    selectToday() {
        const today = this.getTodayPersian();
        this.viewDate = { ...today };
        this.selectDate(today);
    }
    
    selectQuickDate(daysFromToday) {
        const today = new Date();
        const targetDate = new Date(today.getTime() + (daysFromToday * 24 * 60 * 60 * 1000));
        
        // Use jalaali-js for accurate conversion
        let jYear, jMonth, jDay;
        if (typeof window.jalaali !== 'undefined') {
            const jalaali = window.jalaali.toJalaali(targetDate);
            jYear = jalaali.jy;
            jMonth = jalaali.jm;
            jDay = jalaali.jd;
        } else {
            // Fallback to old method
            [jYear, jMonth, jDay] = window.persianDate.gregorianToPersian(
                targetDate.getFullYear(),
                targetDate.getMonth() + 1,
                targetDate.getDate()
            );
        }
        
        const persianDate = { year: jYear, month: jMonth, day: jDay };
        this.viewDate = { ...persianDate };
        this.selectDate(persianDate);
    }
    
    navigateDate(daysDelta) {
        if (!this.selectedDate) {
            // If no date selected, start with today
            this.selectedDate = this.getTodayPersian();
        }
        
        // Convert to Gregorian, add days, convert back
        let gregorianDate;
        if (typeof window.jalaali !== 'undefined') {
            // Use jalaali-js for accurate conversion
            const gregorian = window.jalaali.toGregorian(this.selectedDate.year, this.selectedDate.month, this.selectedDate.day);
            gregorianDate = new Date(gregorian.gy, gregorian.gm - 1, gregorian.gd);
        } else {
            // Fallback to old method
            gregorianDate = window.persianDate.persianStringToGregorianDate(
                this.formatDate(this.selectedDate)
            );
        }
        
        if (gregorianDate) {
            const newDate = new Date(gregorianDate.getTime() + (daysDelta * 24 * 60 * 60 * 1000));
            
            let jYear, jMonth, jDay;
            if (typeof window.jalaali !== 'undefined') {
                const jalaali = window.jalaali.toJalaali(newDate);
                jYear = jalaali.jy;
                jMonth = jalaali.jm;
                jDay = jalaali.jd;
            } else {
                [jYear, jMonth, jDay] = window.persianDate.gregorianToPersian(
                    newDate.getFullYear(),
                    newDate.getMonth() + 1,
                    newDate.getDate()
                );
            }
            
            const newPersianDate = { year: jYear, month: jMonth, day: jDay };
            
            // Update view if month changed
            if (newPersianDate.month !== this.viewDate.month || newPersianDate.year !== this.viewDate.year) {
                this.viewDate = { ...newPersianDate };
            }
            
            this.selectedDate = newPersianDate;
            this.updateCalendar();
        }
    }
    
    clearDate() {
        this.selectedDate = null;
        this.input.value = '';
        
        // Trigger change event
        const changeEvent = new Event('change', { bubbles: true });
        this.input.dispatchEvent(changeEvent);
        
        this.updateCalendar();
        this.hide();
    }
    
    setDate(dateString) {
        const date = this.parseDate(dateString);
        if (date) {
            this.selectedDate = date;
            this.viewDate = { ...date };
            this.input.value = dateString;
            this.updateCalendar();
        } else {
            console.warn('PersianDatePicker setDate failed to parse:', dateString);
        }
    }
    
    isDateDisabled(date) {
        if (this.options.minDate) {
            const minDate = this.parseDate(this.options.minDate);
            if (minDate && this.compareDates(date, minDate) < 0) {
                return true;
            }
        }
        
        if (this.options.maxDate) {
            const maxDate = this.parseDate(this.options.maxDate);
            if (maxDate && this.compareDates(date, maxDate) > 0) {
                return true;
            }
        }
        
        return false;
    }
    
    // Utility methods
    throttle(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
    
    getTodayPersian() {
        const today = new Date();
        
        // Use jalaali-js for accurate conversion
        if (typeof window.jalaali !== 'undefined') {
            const jalaali = window.jalaali.toJalaali(today);
            return { year: jalaali.jy, month: jalaali.jm, day: jalaali.jd };
        }
        
        // Fallback to old method
        const gYear = today.getFullYear();
        const gMonth = today.getMonth() + 1;
        const gDay = today.getDate();
        
        // Accurate Persian date conversion
        let jYear, jMonth, jDay;
        
        // Persian calendar starts on March 21st (Farvardin 1st)
        if (gMonth > 3 || (gMonth === 3 && gDay >= 21)) {
            jYear = gYear - 621;
        } else {
            jYear = gYear - 622;
        }
        
        // Calculate Persian month and day based on Gregorian date
        const persianNewYear = new Date(gYear, 2, 21); // March 21st
        const daysSinceNewYear = Math.floor((today - persianNewYear) / (1000 * 60 * 60 * 24));
        
        if (daysSinceNewYear >= 0) {
            // After Persian New Year
            if (daysSinceNewYear < 31) {
                jMonth = 1; // Farvardin
                jDay = daysSinceNewYear + 1;
            } else if (daysSinceNewYear < 62) {
                jMonth = 2; // Ordibehesht
                jDay = daysSinceNewYear - 30;
            } else if (daysSinceNewYear < 93) {
                jMonth = 3; // Khordad
                jDay = daysSinceNewYear - 61;
            } else if (daysSinceNewYear < 124) {
                jMonth = 4; // Tir
                jDay = daysSinceNewYear - 92;
            } else if (daysSinceNewYear < 155) {
                jMonth = 5; // Mordad
                jDay = daysSinceNewYear - 123;
            } else if (daysSinceNewYear < 186) {
                jMonth = 6; // Shahrivar
                jDay = daysSinceNewYear - 154;
            } else if (daysSinceNewYear < 216) {
                jMonth = 7; // Mehr
                jDay = daysSinceNewYear - 185;
            } else if (daysSinceNewYear < 246) {
                jMonth = 8; // Aban
                jDay = daysSinceNewYear - 215;
            } else if (daysSinceNewYear < 276) {
                jMonth = 9; // Azar
                jDay = daysSinceNewYear - 245;
            } else if (daysSinceNewYear < 306) {
                jMonth = 10; // Dey
                jDay = daysSinceNewYear - 275;
            } else if (daysSinceNewYear < 336) {
                jMonth = 11; // Bahman
                jDay = daysSinceNewYear - 305;
            } else {
                jMonth = 12; // Esfand
                jDay = daysSinceNewYear - 335;
            }
        } else {
            // Before Persian New Year (previous year)
            jYear = jYear - 1;
            const daysInPreviousYear = 365 + (this.isLeapYear(jYear) ? 1 : 0);
            const daysFromEnd = daysInPreviousYear + daysSinceNewYear;
            
            if (daysFromEnd < 31) {
                jMonth = 1; // Farvardin
                jDay = daysFromEnd + 1;
            } else if (daysFromEnd < 62) {
                jMonth = 2; // Ordibehesht
                jDay = daysFromEnd - 30;
            } else if (daysFromEnd < 93) {
                jMonth = 3; // Khordad
                jDay = daysFromEnd - 61;
            } else if (daysFromEnd < 124) {
                jMonth = 4; // Tir
                jDay = daysFromEnd - 92;
            } else if (daysFromEnd < 155) {
                jMonth = 5; // Mordad
                jDay = daysFromEnd - 123;
            } else if (daysFromEnd < 186) {
                jMonth = 6; // Shahrivar
                jDay = daysFromEnd - 154;
            } else if (daysFromEnd < 216) {
                jMonth = 7; // Mehr
                jDay = daysFromEnd - 185;
            } else if (daysFromEnd < 246) {
                jMonth = 8; // Aban
                jDay = daysFromEnd - 215;
            } else if (daysFromEnd < 276) {
                jMonth = 9; // Azar
                jDay = daysFromEnd - 245;
            } else if (daysFromEnd < 306) {
                jMonth = 10; // Dey
                jDay = daysFromEnd - 275;
            } else if (daysFromEnd < 336) {
                jMonth = 11; // Bahman
                jDay = daysFromEnd - 305;
            } else {
                jMonth = 12; // Esfand
                jDay = daysFromEnd - 335;
            }
        }
        
        // Ensure reasonable values
        if (jMonth < 1) jMonth = 1;
        if (jMonth > 12) jMonth = 12;
        if (jDay < 1) jDay = 1;
        if (jDay > 31) jDay = 31;
        
        return { year: jYear, month: jMonth, day: jDay };
    }
    
    isLeapYear(persianYear) {
        // Persian leap year calculation
        const breaks = [-61, 9, 38, 199, 426, 686, 756, 818, 1111, 1181, 1210, 1635, 2060, 2097, 2192, 2262, 2324, 2394, 2456, 3178];
        let jp = persianYear;
        let j = 1;
        let j1 = j;
        let jump = 0;
        
        for (let i = 1; i < breaks.length; i++) {
            const jm = breaks[i];
            jump = jm - j1;
            if (jp < jm) break;
            j = j1;
            j1 = jm;
        }
        
        const n = jp - j;
        if (n - jump < 0) return false;
        return ((n - jump) % 33) % 4 === 1;
    }
    
    parseDate(dateString) {
        if (!dateString) return null;
        
        const parts = dateString.split('/');
        if (parts.length === 3) {
            const year = parseInt(parts[0]);
            const month = parseInt(parts[1]);
            const day = parseInt(parts[2]);
            
            // Validate the parsed values
            if (isNaN(year) || isNaN(month) || isNaN(day)) {
                console.warn('Invalid date parts:', { year, month, day });
                return null;
            }
            
        // Ensure reasonable year range (1400-1450 for current Persian calendar)
        if (year < 1400 || year > 1450) {
            console.warn('Year out of reasonable range:', year);
            return null;
        }
            
            return { year, month, day };
        }
        return null;
    }
    
    formatDate(date) {
        return `${date.year}/${date.month.toString().padStart(2, '0')}/${date.day.toString().padStart(2, '0')}`;
    }
    
    isSameDate(date1, date2) {
        return date1.year === date2.year && 
               date1.month === date2.month && 
               date1.day === date2.day;
    }
    
    compareDates(date1, date2) {
        if (date1.year !== date2.year) return date1.year - date2.year;
        if (date1.month !== date2.month) return date1.month - date2.month;
        return date1.day - date2.day;
    }
    
    getFirstDayOfMonth(year, month) {
        // Use jalaali-js for accurate conversion
        if (typeof window.jalaali !== 'undefined') {
            const gregorian = window.jalaali.toGregorian(year, month, 1);
            const firstDay = new Date(gregorian.gy, gregorian.gm - 1, gregorian.gd);
            // Convert to Persian week (Saturday = 0, Sunday = 1, ..., Friday = 6)
            return (firstDay.getDay() + 1) % 7;
        }
        
        // Fallback to old method
        const [gYear, gMonth, gDay] = window.persianDate.persianToGregorian(year, month, 1);
        const firstDay = new Date(gYear, gMonth - 1, gDay);
        return (firstDay.getDay() + 1) % 7; // Convert to Persian week (Saturday = 0)
    }
    
    getDaysInMonth(year, month) {
        if (month <= 6) return 31;
        if (month <= 11) return 30;
        return this.isLeapYear(year) ? 30 : 29;
    }
    
    isLeapYear(year) {
        // Use jalaali-js for accurate leap year calculation
        if (typeof window.jalaali !== 'undefined') {
            return window.jalaali.isLeapJalaaliYear(year);
        }
        
        // Fallback to old method
        const breaks = [
            -61, 9, 38, 199, 426, 686, 756, 818, 1111, 1181, 1210,
            1635, 2060, 2097, 2192, 2262, 2324, 2394, 2456, 3178
        ];
        
        const gy = year + 1029;
        let leap = -14;
        let jp = breaks[0];
        
        let jump = 0;
        for (let j = 1; j <= 19; j++) {
            const jm = breaks[j];
            jump = jm - jp;
            if (year < jm) break;
            leap += Math.floor(jump / 33) * 8 + Math.floor(((jump % 33) + 3) / 4);
            jp = jm;
        }
        
        let n = year - jp;
        if (n < jump) {
            leap += Math.floor(n / 33) * 8 + Math.floor(((n % 33) + 3) / 4);
            if ((jump % 33) === 4 && (jump - n) === 4) leap++;
        }
        
        return (leap + 4) % 1029 % 33 % 4 === 1;
    }
}

// Auto-initialize datepickers
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all elements with persian-datepicker class
    document.querySelectorAll('.persian-datepicker').forEach(input => {
        new PersianDatePicker(input, {
            minDate: input.dataset.minDate,
            maxDate: input.dataset.maxDate,
            initialDate: input.value
        });
    });
});
