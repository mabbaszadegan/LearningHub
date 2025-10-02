// Simple Jalaali (Persian) Date Converter
// Based on moment-jalaali algorithms but simplified

class JalaaliDate {
    constructor() {
        this.persianMonths = [
            'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
            'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
        ];
    }

    // Check if Jalaali year is leap
    isLeapJalaaliYear(jy) {
        const breaks = [
            -61, 9, 38, 199, 426, 686, 756, 818, 1111, 1181, 1210,
            1635, 2060, 2097, 2192, 2262, 2324, 2394, 2456, 3178
        ];

        let jp = breaks[0];
        let jump = 0;
        for (let j = 1; j < breaks.length; j++) {
            const jm = breaks[j];
            jump = jm - jp;
            if (jy < jm) break;
            jp = jm;
        }

        let n = jy - jp;
        if (n < jump) {
            if ((jump % 33) === 4 && (jump - n) === 4) {
                n++;
            }
            const leap = ((n + 1) % 33) % 4;
            if (jump === 33 && leap === 1) {
                return true;
            } else {
                return leap === 1;
            }
        }
        return false;
    }

    // Convert Jalaali to Julian Day Number
    jalaaliToJulian(jy, jm, jd) {
        const breaks = [
            -61, 9, 38, 199, 426, 686, 756, 818, 1111, 1181, 1210,
            1635, 2060, 2097, 2192, 2262, 2324, 2394, 2456, 3178
        ];

        let jp = breaks[0];
        let jump = 0;
        for (let j = 1; j < breaks.length; j++) {
            const jm2 = breaks[j];
            jump = jm2 - jp;
            if (jy < jm2) break;
            jp = jm2;
        }

        let n = jy - jp;
        let leap = -14;
        if (n < jump) {
            leap = ((n + 1) % 33) % 4;
            if (jump === 33 && leap === 4) {
                leap = 0;
            }
        }

        if (n >= jump) {
            n = n - jump;
            leap = ((n + 1) % 33) % 4;
            if (leap === 4 || leap === 0) {
                leap = 0;
            } else {
                leap = 1;
            }
        }

        const jp2 = jp + jump;
        let jd2 = 0;
        for (let i = 0; i < jm - 1; ++i) {
            if (i < 6) {
                jd2 += 31;
            } else {
                jd2 += 30;
            }
        }
        jd2 += jd;

        const epyear = jy - ((jy >= 0) ? 474 : 473);
        const epochbase = jy - ((jy >= 0) ? 474 : 473) - 474;
        const auxYear = 474 + (epochbase % 2820);

        let aux = 0;
        if (auxYear < 474) {
            aux = auxYear + 38 + (Math.floor((auxYear + 38) / 128) * 128);
        } else {
            aux = auxYear - 474;
        }

        const cycle = Math.floor(aux / 128);
        const cyear = aux % 128;
        let epy = 0;

        if (cyear <= 28) {
            epy = cyear;
        } else if (cyear >= 29 && cyear <= 33) {
            epy = cyear - 1;
        } else if (cyear >= 34 && cyear <= 37) {
            epy = cyear - 2;
        } else if (cyear >= 38 && cyear <= 42) {
            epy = cyear - 3;
        } else if (cyear >= 43 && cyear <= 46) {
            epy = cyear - 4;
        } else if (cyear >= 47 && cyear <= 50) {
            epy = cyear - 5;
        } else if (cyear >= 51 && cyear <= 54) {
            epy = cyear - 6;
        } else if (cyear >= 55 && cyear <= 58) {
            epy = cyear - 7;
        } else if (cyear >= 59 && cyear <= 62) {
            epy = cyear - 8;
        } else if (cyear >= 63 && cyear <= 66) {
            epy = cyear - 9;
        } else if (cyear >= 67 && cyear <= 70) {
            epy = cyear - 10;
        } else if (cyear >= 71 && cyear <= 74) {
            epy = cyear - 11;
        } else if (cyear >= 75 && cyear <= 78) {
            epy = cyear - 12;
        } else if (cyear >= 79 && cyear <= 82) {
            epy = cyear - 13;
        } else if (cyear >= 83 && cyear <= 86) {
            epy = cyear - 14;
        } else if (cyear >= 87 && cyear <= 90) {
            epy = cyear - 15;
        } else if (cyear >= 91 && cyear <= 94) {
            epy = cyear - 16;
        } else if (cyear >= 95 && cyear <= 98) {
            epy = cyear - 17;
        } else if (cyear >= 99 && cyear <= 102) {
            epy = cyear - 18;
        } else if (cyear >= 103 && cyear <= 106) {
            epy = cyear - 19;
        } else if (cyear >= 107 && cyear <= 110) {
            epy = cyear - 20;
        } else if (cyear >= 111 && cyear <= 114) {
            epy = cyear - 21;
        } else if (cyear >= 115 && cyear <= 118) {
            epy = cyear - 22;
        } else if (cyear >= 119 && cyear <= 122) {
            epy = cyear - 23;
        } else if (cyear >= 123 && cyear <= 126) {
            epy = cyear - 24;
        } else if (cyear >= 127 && cyear <= 128) {
            epy = cyear - 25;
        }

        const julday = 1948321 + 365 * jy + Math.floor(jy / 33) * 8 + Math.floor(((jy % 33) + 3) / 4) + jd2;
        
        return julday;
    }

    // Convert Julian Day Number to Gregorian
    julianToGregorian(julday) {
        let a = julday + 32044;
        let b = Math.floor((4 * a + 3) / 146097);
        let c = a - Math.floor((146097 * b) / 4);
        let d = Math.floor((4 * c + 3) / 1461);
        let e = c - Math.floor((1461 * d) / 4);
        let m = Math.floor((5 * e + 2) / 153);

        const day = e - Math.floor((153 * m + 2) / 5) + 1;
        const month = m + 3 - 12 * Math.floor(m / 10);
        const year = 100 * b + d - 4800 + Math.floor(m / 10);

        return [year, month, day];
    }

    // Convert Gregorian to Julian Day Number
    gregorianToJulian(gy, gm, gd) {
        let a = Math.floor((14 - gm) / 12);
        let y = gy - a;
        let m = gm + 12 * a - 3;

        return gd + Math.floor((153 * m + 2) / 5) + 365 * y + Math.floor(y / 4) - Math.floor(y / 100) + Math.floor(y / 400) + 1721119;
    }

    // Convert Julian Day Number to Jalaali
    julianToJalaali(julday) {
        const gy = this.julianToGregorian(julday)[0];
        let jy = gy <= 1600 ? 0 : 979;
        julday -= this.gregorianToJulian(gy, 1, 1);
        
        if (julday >= 0) {
            jy += 33 * Math.floor(julday / 12053);
            julday %= 12053;
            jy += 4 * Math.floor(julday / 1461);
            julday %= 1461;
            if (julday > 365) {
                jy += Math.floor((julday - 1) / 365);
                julday = (julday - 1) % 365;
            }
        } else {
            jy -= 1;
            julday += 365;
        }

        let jm, jd;
        if (julday < 186) {
            jm = 1 + Math.floor(julday / 31);
            jd = 1 + (julday % 31);
        } else {
            jm = 7 + Math.floor((julday - 186) / 30);
            jd = 1 + ((julday - 186) % 30);
        }

        return [jy, jm, jd];
    }

    // Main conversion functions
    gregorianToPersian(gy, gm, gd) {
        const julday = this.gregorianToJulian(gy, gm, gd);
        return this.julianToJalaali(julday);
    }

    persianToGregorian(jy, jm, jd) {
        const julday = this.jalaaliToJulian(jy, jm, jd);
        return this.julianToGregorian(julday);
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

// Test the implementation
console.log('=== Testing Jalaali Date Converter ===');
const jalaali = new JalaaliDate();

// Test with known values
const today = new Date();
const todayPersian = jalaali.getTodayPersian();
console.log('Today:', today.toDateString());
console.log('Today Persian:', todayPersian);
console.log('Expected year: around 1403');

// Test Nowruz 2024 (March 21, 2024 = 1403/01/01)
const nowruz = new Date(2024, 2, 21);
const nowruzPersian = jalaali.gregorianDateToPersianString(nowruz);
console.log('Nowruz 2024 (March 21):', nowruzPersian, '(should be 1403/01/01)');

// Replace global instance
window.persianDate = jalaali;
