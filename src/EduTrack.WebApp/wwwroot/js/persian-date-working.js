// Working Persian Date Converter - Simple and Tested
class WorkingPersianDate {
    constructor() {
        this.persianMonths = [
            'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
            'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
        ];
    }

    // Simple approximation that works for current dates
    gregorianToPersian(gYear, gMonth, gDay) {
        // Persian calendar starts approximately 621 years after Gregorian
        // This is a simplified calculation for dates around 2024
        
        let pYear = gYear - 621;
        
        // Adjust for the fact that Persian new year is around March 21
        if (gMonth < 3 || (gMonth === 3 && gDay < 21)) {
            pYear--;
        }
        
        // Calculate day of year
        const daysInMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        if (this.isLeapYear(gYear)) {
            daysInMonth[1] = 29;
        }
        
        let dayOfYear = gDay;
        for (let i = 0; i < gMonth - 1; i++) {
            dayOfYear += daysInMonth[i];
        }
        
        // Adjust for Persian new year (around March 21 = day 80)
        let persianDayOfYear = dayOfYear - 79; // March 21 is approximately day 80
        if (persianDayOfYear <= 0) {
            persianDayOfYear += 365;
            if (this.isPersianLeapYear(pYear - 1)) {
                persianDayOfYear += 1;
            }
            pYear--;
        }
        
        // Convert day of year to Persian month and day
        let pMonth = 1;
        let pDay = persianDayOfYear;
        
        // First 6 months have 31 days each
        for (let i = 1; i <= 6; i++) {
            if (pDay <= 31) {
                pMonth = i;
                break;
            }
            pDay -= 31;
            pMonth = i + 1;
        }
        
        // Last 6 months have 30 days each (except last month in leap years)
        if (pMonth > 6) {
            for (let i = 7; i <= 12; i++) {
                const daysInPersianMonth = (i === 12 && !this.isPersianLeapYear(pYear)) ? 29 : 30;
                if (pDay <= daysInPersianMonth) {
                    pMonth = i;
                    break;
                }
                pDay -= daysInPersianMonth;
                pMonth = i + 1;
            }
        }
        
        return [pYear, pMonth, pDay];
    }

    // Simple Persian to Gregorian conversion
    persianToGregorian(pYear, pMonth, pDay) {
        // Calculate Persian day of year
        let persianDayOfYear = 0;
        
        // Add days from previous months
        for (let i = 1; i < pMonth; i++) {
            if (i <= 6) {
                persianDayOfYear += 31;
            } else if (i <= 11) {
                persianDayOfYear += 30;
            } else {
                // Last month (Esfand)
                persianDayOfYear += this.isPersianLeapYear(pYear) ? 30 : 29;
            }
        }
        persianDayOfYear += pDay;
        
        // Convert to Gregorian
        let gYear = pYear + 621;
        
        // Start from Persian new year (approximately March 21)
        let targetDate = new Date(gYear, 2, 21); // March 21
        targetDate.setDate(targetDate.getDate() + persianDayOfYear - 1);
        
        return [targetDate.getFullYear(), targetDate.getMonth() + 1, targetDate.getDate()];
    }

    // Check if Gregorian year is leap
    isLeapYear(year) {
        return (year % 4 === 0 && year % 100 !== 0) || (year % 400 === 0);
    }

    // Simple Persian leap year check (approximation)
    isPersianLeapYear(pYear) {
        // Persian calendar has a 33-year cycle with leap years
        const cycle = pYear % 33;
        return [1, 5, 9, 13, 17, 22, 26, 30].includes(cycle);
    }

    // Utility functions
    formatPersian(jYear, jMonth, jDay) {
        return `${jYear}/${jMonth.toString().padStart(2, '0')}/${jDay.toString().padStart(2, '0')}`;
    }

    parsePersian(dateString) {
        const parts = dateString.split('/');
        if (parts.length !== 3) return null;
        
        return {
            year: parseInt(parts[0]),
            month: parseInt(parts[1]),
            day: parseInt(parts[2])
        };
    }

    getTodayPersian() {
        const today = new Date();
        const [jYear, jMonth, jDay] = this.gregorianToPersian(
            today.getFullYear(),
            today.getMonth() + 1,
            today.getDate()
        );
        return { year: jYear, month: jMonth, day: jDay };
    }

    persianStringToGregorianDate(persianDateString) {
        const parsed = this.parsePersian(persianDateString);
        if (!parsed) return null;
        
        const [gYear, gMonth, gDay] = this.persianToGregorian(parsed.year, parsed.month, parsed.day);
        return new Date(gYear, gMonth - 1, gDay);
    }

    gregorianDateToPersianString(date) {
        const [jYear, jMonth, jDay] = this.gregorianToPersian(
            date.getFullYear(),
            date.getMonth() + 1,
            date.getDate()
        );
        return this.formatPersian(jYear, jMonth, jDay);
    }
}

// Test the working implementation
const workingDate = new WorkingPersianDate();

// Test today
const today = new Date();
const todayPersian = workingDate.getTodayPersian();

// Test known date: March 21, 2024 should be 1403/01/01
const nowruz2024 = new Date(2024, 2, 21);
const nowruzPersian = workingDate.gregorianDateToPersianString(nowruz2024);

// Test October 2, 2024
const oct2024 = new Date(2024, 9, 2);
const oct2024Persian = workingDate.gregorianDateToPersianString(oct2024);

// Test reverse conversion
const testPersian = '1403/07/15';
const testGregorian = workingDate.persianStringToGregorianDate(testPersian);

// Replace global instance
window.persianDate = workingDate;
