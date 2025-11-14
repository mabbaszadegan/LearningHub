class StudentStatisticsPage {
    constructor(rootElement) {
        this.root = rootElement;
        this.chartElement = document.getElementById('study-time-chart');
        this.chartInstance = null;
        this.learningStats = this.parseDataset();
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
}

document.addEventListener('DOMContentLoaded', () => {
    const rootElement = document.getElementById('student-statistics-root');
    if (!rootElement) {
        return;
    }

    new StudentStatisticsPage(rootElement);
});

