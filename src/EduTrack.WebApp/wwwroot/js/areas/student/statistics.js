class StudentStatisticsPage {
    constructor(rootElement) {
        this.root = rootElement;
        this.chartElement = document.getElementById('study-time-chart');
        this.chartInstance = null;
        this.learningStats = this.parseDataset();
        this.questionStats = this.learningStats?.questionPerformance ?? null;
        this.questionAttempts = this.questionStats?.attemptSummaries ?? [];
        this.init();
    }

    parseDataset() {
        if (!this.root) {
            return null;
        }

        try {
            const raw = this.root.dataset.learningStats;
            return raw ? JSON.parse(raw) : null;
        } catch (error) {
            console.error('Failed to parse learning statistics dataset', error);
            return null;
        }
    }

    init() {
        this.setActiveNavigation();
        this.animateProgressCircle();
        this.initializeChart();
        this.bindRangeToggle();
        this.bindQuestionStatCards();
    }

    setActiveNavigation() {
        if (window.bottomNavigation) {
            window.bottomNavigation.setActivePage('statistics');
        }
    }

    animateProgressCircle() {
        const circleWrapper = document.querySelector('.progress-circle');
        if (!circleWrapper) {
            return;
        }

        const percentage = parseFloat(circleWrapper.dataset.percentage) || 0;
        const circle = circleWrapper.querySelector('.progress-ring-circle');
        if (!circle) {
            return;
        }

        const radius = circle.r.baseVal.value;
        const circumference = radius * 2 * Math.PI;
        circle.style.strokeDasharray = `${circumference} ${circumference}`;
        circle.style.strokeDashoffset = circumference;

        setTimeout(() => {
            const offset = circumference - (percentage / 100) * circumference;
            circle.style.transition = 'stroke-dashoffset 1s ease-in-out';
            circle.style.strokeDashoffset = offset;
        }, 150);
    }

    initializeChart() {
        if (!this.chartElement || !window.Chart || !this.learningStats) {
            return;
        }

        this.renderChart('week');
    }

    renderChart(rangeKey) {
        const chartData = this.getChartData(rangeKey);
        if (!chartData) {
            return;
        }

        const config = {
            type: 'line',
            data: {
                labels: chartData.labels,
                datasets: [{
                    label: chartData.title,
                    data: chartData.values,
                    borderColor: '#667eea',
                    backgroundColor: 'rgba(102, 126, 234, 0.12)',
                    pointBackgroundColor: '#667eea',
                    tension: 0.4,
                    borderWidth: 3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: context => `${context.parsed.y} دقیقه`
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: value => `${value} دقیقه`
                        },
                        grid: {
                            color: '#f1f3f5'
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        };

        if (this.chartInstance) {
            this.chartInstance.destroy();
        }

        this.chartInstance = new Chart(this.chartElement, config);
    }

    getChartData(rangeKey) {
        if (!this.learningStats) {
            return null;
        }

        const chart = rangeKey === 'month' ? this.learningStats.monthlyChart : this.learningStats.weeklyChart;
        const labels = chart?.points?.map(point => point.label) || [];
        const values = chart?.points?.map(point => point.minutes) || [];

        return {
            labels,
            values,
            title: chart?.rangeTitle || ''
        };
    }

    bindRangeToggle() {
        const toggleButtons = document.querySelectorAll('.chart-range-toggle button');
        if (!toggleButtons.length) {
            return;
        }

        toggleButtons.forEach(button => {
            button.addEventListener('click', () => {
                if (button.classList.contains('active')) {
                    return;
                }

                toggleButtons.forEach(btn => btn.classList.remove('active'));
                button.classList.add('active');
                const rangeKey = button.dataset.chartRange || 'week';
                this.renderChart(rangeKey);
            });
        });
    }

    bindQuestionStatCards() {
        const cards = document.querySelectorAll('.question-stat-card');
        const modalElement = document.getElementById('questionAttemptModal');
        const modalList = document.getElementById('questionAttemptModalList');
        const modalTitle = document.getElementById('questionAttemptModalTitle');
        const modalDescription = document.getElementById('questionAttemptModalDescription');

        if (!cards.length || !modalElement || !modalList || !modalTitle || !this.questionStats) {
            return;
        }

        this.ensureModalPlacement(modalElement);
        const bootstrapModal = window.bootstrap?.Modal
            ? window.bootstrap.Modal.getOrCreateInstance(modalElement)
            : null;

        cards.forEach(card => {
            card.addEventListener('click', () => {
                const filterKey = card.dataset.filter || 'all';
                const attempts = this.getFilteredAttempts(filterKey);
                modalTitle.textContent = card.dataset.modalTitle || 'خلاصه تلاش‌ها';
                modalDescription.textContent = card.dataset.modalDescription || '';
                modalList.innerHTML = attempts.length
                    ? attempts.map(attempt => this.buildAttemptItem(attempt)).join('')
                    : this.renderEmptyAttemptState();

                if (bootstrapModal) {
                    bootstrapModal.show();
                } else {
                    modalElement.classList.add('show');
                    modalElement.style.display = 'block';
                }
            });
        });

        modalElement.addEventListener('hidden.bs.modal', () => {
            modalList.innerHTML = '';
        });
    }

    ensureModalPlacement(modalElement) {
        if (modalElement.parentElement !== document.body) {
            document.body.appendChild(modalElement);
        }
    }

    getFilteredAttempts(filterKey) {
        if (!Array.isArray(this.questionAttempts)) {
            return [];
        }

        if (filterKey === 'correct') {
            return this.questionAttempts.filter(attempt => attempt.isCorrect);
        }

        if (filterKey === 'incorrect') {
            return this.questionAttempts.filter(attempt => attempt.isCorrect === false);
        }

        return this.questionAttempts;
    }

    buildAttemptItem(attempt) {
        const questionLabel = this.escapeHtml(this.toPlainText(attempt.questionLabel || attempt.scheduleItemTitle || 'سوال'));
        const scheduleTitle = this.escapeHtml(this.toPlainText(attempt.scheduleItemTitle || ''));
        const orderLabel = typeof attempt.blockOrder === 'number' ? `سوال ${attempt.blockOrder}` : '';
        const dateLabel = this.formatPersianDateTime(attempt.attemptedAt);
        const statusClass = attempt.isCorrect ? 'success' : 'danger';
        const statusLabel = attempt.isCorrect ? 'درست' : 'غلط';
        const points = typeof attempt.pointsEarned === 'number' && typeof attempt.maxPoints === 'number'
            ? `${attempt.pointsEarned}/${attempt.maxPoints}`
            : '';

        return `
            <div class="attempt-item">
                <div class="attempt-info">
                    <div class="attempt-question">${questionLabel}</div>
                    <div class="attempt-meta">
                        ${orderLabel ? `${orderLabel} · ` : ''}${scheduleTitle}
                        ${points ? ` · ${points} امتیاز` : ''}
                        <span> · ${dateLabel}</span>
                    </div>
                </div>
                <span class="attempt-status ${statusClass}">${statusLabel}</span>
            </div>
        `;
    }

    renderEmptyAttemptState() {
        return `
            <div class="question-attempt-empty">
                هنوز داده‌ای در این دسته وجود ندارد.
            </div>
        `;
    }

    formatPersianDateTime(value) {
        try {
            const options = {
                day: '2-digit',
                month: 'long',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
                hour12: false,
                timeZone: 'Asia/Tehran'
            };

            const formatter = new Intl.DateTimeFormat('fa-IR-u-ca-persian', options);
            return formatter.format(new Date(value));
        } catch (error) {
            return value;
        }
    }

    escapeHtml(value) {
        if (typeof value !== 'string') {
            return value || '';
        }

        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };

        return value.replace(/[&<>"']/g, char => map[char]);
    }

    toPlainText(value) {
        if (typeof DOMParser === 'undefined' || typeof value !== 'string') {
            return value || '';
        }

        const parser = new DOMParser();
        const doc = parser.parseFromString(value, 'text/html');
        return doc.body.textContent?.trim() || '';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const rootElement = document.getElementById('student-statistics-root');
    if (!rootElement) {
        return;
    }

    new StudentStatisticsPage(rootElement);
});

