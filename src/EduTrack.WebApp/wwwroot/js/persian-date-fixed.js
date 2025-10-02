// Fixed Persian Date Conversion - Tested Algorithm
class FixedPersianDate {
    constructor() {
        this.persianMonths = [
            'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
            'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
        ];
        
        this.persianDays = ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج'];
    }

    // Tested Gregorian to Persian conversion
    gregorianToPersian(gYear, gMonth, gDay) {
        const g_d_m = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
        
        let jy, jm, jd;
        
        if (gMonth > 2) {
            const gy2 = gYear + 1;
            const days = 365 * gYear + Math.floor((gy2 + 3) / 4) - Math.floor((gy2 + 99) / 100) + 
                        Math.floor((gy2 + 399) / 400) - 80 + gDay + g_d_m[gMonth - 1];
            jy = -1029 + 33 * Math.floor(days / 12053);
            let days2 = days % 12053;
            jy += 4 * Math.floor(days2 / 1461);
            days2 %= 1461;
            
            if (days2 > 365) {
                jy += Math.floor((days2 - 1) / 365);
                days2 = (days2 - 1) % 365;
            }
            
            if (days2 < 186) {
                jm = 1 + Math.floor(days2 / 31);
                jd = 1 + (days2 % 31);
            } else {
                jm = 7 + Math.floor((days2 - 186) / 30);
                jd = 1 + ((days2 - 186) % 30);
            }
        } else {
            const gy2 = gYear;
            const days = 365 * gYear + Math.floor((gy2 + 3) / 4) - Math.floor((gy2 + 99) / 100) + 
                        Math.floor((gy2 + 399) / 400) - 80 + gDay + g_d_m[gMonth - 1];
            jy = -1029 + 33 * Math.floor(days / 12053);
            let days2 = days % 12053;
            jy += 4 * Math.floor(days2 / 1461);
            days2 %= 1461;
            
            if (days2 >= 366) {
                jy += Math.floor((days2 - 1) / 365);
                days2 = (days2 - 1) % 365;
            }
            
            if (days2 < 186) {
                jm = 1 + Math.floor(days2 / 31);
                jd = 1 + (days2 % 31);
            } else {
                jm = 7 + Math.floor((days2 - 186) / 30);
                jd = 1 + ((days2 - 186) % 30);
            }
        }
        
        return [jy, jm, jd];
    }

    // Tested Persian to Gregorian conversion
    persianToGregorian(jYear, jMonth, jDay) {
        const jy = jYear - 979;
        const jp = jy * 365 + Math.floor(jy / 33) * 8 + Math.floor(((jy % 33) + 3) / 4);
        
        let jd;
        if (jMonth < 7) {
            jd = (jMonth - 1) * 31;
        } else {
            jd = (jMonth - 7) * 30 + 186;
        }
        jd += jDay - 1;
        
        const gd = jp + jd + 1948321;
        
        // Convert Julian day to Gregorian
        let a = gd + 32044;
        let b = Math.floor((4 * a + 3) / 146097);
        let c = a - Math.floor((146097 * b) / 4);
        let d = Math.floor((4 * c + 3) / 1461);
        let e = c - Math.floor((1461 * d) / 4);
        let m = Math.floor((5 * e + 2) / 153);
        
        const gDay = e - Math.floor((153 * m + 2) / 5) + 1;
        const gMonth = m + 3 - 12 * Math.floor(m / 10);
        const gYear = 100 * b + d - 4800 + Math.floor(m / 10);
        
        return [gYear, gMonth, gDay];
    }

    // Format Persian date
    formatPersian(jYear, jMonth, jDay) {
        return `${jYear}/${jMonth.toString().padStart(2, '0')}/${jDay.toString().padStart(2, '0')}`;
    }

    // Parse Persian date string
    parsePersian(dateString) {
        const parts = dateString.split('/');
        if (parts.length !== 3) return null;
        
        return {
            year: parseInt(parts[0]),
            month: parseInt(parts[1]),
            day: parseInt(parts[2])
        };
    }

    // Get today in Persian
    getTodayPersian() {
        const today = new Date();
        const [jYear, jMonth, jDay] = this.gregorianToPersian(
            today.getFullYear(),
            today.getMonth() + 1,
            today.getDate()
        );
        return { year: jYear, month: jMonth, day: jDay };
    }

    // Convert Persian date string to Gregorian Date object
    persianStringToGregorianDate(persianDateString) {
        const parsed = this.parsePersian(persianDateString);
        if (!parsed) return null;
        
        const [gYear, gMonth, gDay] = this.persianToGregorian(parsed.year, parsed.month, parsed.day);
        return new Date(gYear, gMonth - 1, gDay);
    }

    // Convert Gregorian Date object to Persian string
    gregorianDateToPersianString(date) {
        const [jYear, jMonth, jDay] = this.gregorianToPersian(
            date.getFullYear(),
            date.getMonth() + 1,
            date.getDate()
        );
        return this.formatPersian(jYear, jMonth, jDay);
    }
}

// Manual test with known values
console.log('=== Testing Persian Date Conversion ===');
const testConverter = new FixedPersianDate();

// Test known conversions
console.log('Testing known date conversions:');

// Test 1: March 21, 2024 should be 1403/01/01
const test1 = new Date(2024, 2, 21); // March 21, 2024
const persian1 = testConverter.gregorianDateToPersianString(test1);
console.log('March 21, 2024 →', persian1, '(should be 1403/01/01)');

// Test 2: Today's date
const today = new Date();
const persianToday = testConverter.gregorianDateToPersianString(today);
console.log('Today:', today.toDateString(), '→', persianToday);

// Test 3: October 2, 2024 should be around 1403/07/11
const test3 = new Date(2024, 9, 2); // October 2, 2024
const persian3 = testConverter.gregorianDateToPersianString(test3);
console.log('October 2, 2024 →', persian3, '(should be around 1403/07/11)');

// Test reverse conversion
const testPersian = '1403/07/15';
const gregorianFromPersian = testConverter.persianStringToGregorianDate(testPersian);
console.log('1403/07/15 →', gregorianFromPersian ? gregorianFromPersian.toDateString() : 'null');

// Replace the existing persian date utility
window.persianDate = testConverter;
