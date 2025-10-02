// Persian Date Utility Functions
class PersianDate {
    constructor() {
        this.persianMonths = [
            'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
            'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
        ];
        
        this.persianDays = [
            'یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'
        ];
    }

    // Convert Gregorian to Persian - Improved Algorithm
    gregorianToPersian(gYear, gMonth, gDay) {
        const g_d_m = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
        
        let jy, jm, jd;
        
        if (gMonth > 2) {
            const gy2 = gYear + 1;
            const days = 365 * gYear + Math.floor((gy2 + 3) / 4) - Math.floor((gy2 + 99) / 100) + 
                        Math.floor((gy2 + 399) / 400) - 80 + gDay + g_d_m[gMonth - 1];
            jy = -1029 + 33 * Math.floor(days / 12053);
            const days2 = days % 12053;
            jy += 4 * Math.floor(days2 / 1461);
            const days3 = days2 % 1461;
            
            if (days3 > 365) {
                jy += Math.floor((days3 - 1) / 365);
                const days4 = (days3 - 1) % 365;
                
                if (days4 < 186) {
                    jm = 1 + Math.floor(days4 / 31);
                    jd = 1 + (days4 % 31);
                } else {
                    jm = 7 + Math.floor((days4 - 186) / 30);
                    jd = 1 + ((days4 - 186) % 30);
                }
            } else {
                if (days3 < 186) {
                    jm = 1 + Math.floor(days3 / 31);
                    jd = 1 + (days3 % 31);
                } else {
                    jm = 7 + Math.floor((days3 - 186) / 30);
                    jd = 1 + ((days3 - 186) % 30);
                }
            }
        } else {
            const gy2 = gYear;
            const days = 365 * gYear + Math.floor((gy2 + 3) / 4) - Math.floor((gy2 + 99) / 100) + 
                        Math.floor((gy2 + 399) / 400) - 80 + gDay + g_d_m[gMonth - 1];
            jy = -1029 + 33 * Math.floor(days / 12053);
            const days2 = days % 12053;
            jy += 4 * Math.floor(days2 / 1461);
            const days3 = days2 % 1461;
            
            if (days3 >= 366) {
                jy += Math.floor((days3 - 1) / 365);
                const days4 = (days3 - 1) % 365;
                
                if (days4 < 186) {
                    jm = 1 + Math.floor(days4 / 31);
                    jd = 1 + (days4 % 31);
                } else {
                    jm = 7 + Math.floor((days4 - 186) / 30);
                    jd = 1 + ((days4 - 186) % 30);
                }
            } else {
                if (days3 < 186) {
                    jm = 1 + Math.floor(days3 / 31);
                    jd = 1 + (days3 % 31);
                } else {
                    jm = 7 + Math.floor((days3 - 186) / 30);
                    jd = 1 + ((days3 - 186) % 30);
                }
            }
        }
        
        return [jy, jm, jd];
    }

    // Convert Persian to Gregorian
    persianToGregorian(jYear, jMonth, jDay) {
        const jy = jYear - 979;
        const jp = jy * 365 + Math.floor(jy / 33) * 8 + Math.floor(((jy % 33) + 3) / 4);
        let jd;
        
        if (jMonth < 7) {
            jd = jp + (jMonth - 1) * 31 + jDay;
        } else {
            jd = jp + (jMonth - 7) * 30 + 186 + jDay;
        }
        
        const gy = 1600 + 400 * Math.floor(jd / 146097);
        let gd = jd % 146097;
        let leap = true;
        
        if (gd >= 36525) {
            gd--;
            const gy2 = gy + 100 * Math.floor(gd / 36524);
            gd = gd % 36524;
            
            if (gd >= 365) {
                gd++;
            }
            
            const gy3 = gy2 + 4 * Math.floor(gd / 1461);
            gd %= 1461;
            
            if (gd >= 366) {
                leap = false;
                gd--;
                const gy4 = gy3 + Math.floor(gd / 365);
                gd = gd % 365;
                return this.calculateGregorianMonth(gy4, gd, leap);
            } else {
                return this.calculateGregorianMonth(gy3, gd, leap);
            }
        } else {
            const gy2 = gy + 4 * Math.floor(gd / 1461);
            gd %= 1461;
            
            if (gd >= 366) {
                leap = false;
                gd--;
                const gy3 = gy2 + Math.floor(gd / 365);
                gd = gd % 365;
                return this.calculateGregorianMonth(gy3, gd, leap);
            } else {
                return this.calculateGregorianMonth(gy2, gd, leap);
            }
        }
    }

    calculateGregorianMonth(gy, gd, leap) {
        const sal_a = [0, 31, (leap ? 29 : 28), 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        let gm;
        
        for (gm = 0; gm < 13; gm++) {
            const v = sal_a[gm];
            if (gd <= v) break;
            gd -= v;
        }
        
        return [gy, gm, gd];
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

// Initialize Persian date utility
window.persianDate = new PersianDate();
