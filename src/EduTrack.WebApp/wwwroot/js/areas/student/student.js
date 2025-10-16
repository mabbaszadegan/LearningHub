// Student Area JavaScript

// Student Dashboard functionality
class StudentDashboard {
    constructor() {
        this.initializeProgressCharts();
        this.initializeClassCards();
        this.initializeLearningPath();
        this.initializeAchievements();
    }

    initializeProgressCharts() {
        // Overall Progress Chart
        const progressCtx = document.getElementById('overallProgressChart');
        if (progressCtx) {
            new Chart(progressCtx, {
                type: 'doughnut',
                data: {
                    labels: ['تکمیل شده', 'در حال انجام', 'باقی‌مانده'],
                    datasets: [{
                        data: window.progressData || [0, 0, 100],
                        backgroundColor: [
                            '#51cf66',
                            '#667eea',
                            '#e9ecef'
                        ],
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        }

        // Weekly Progress Chart
        const weeklyCtx = document.getElementById('weeklyProgressChart');
        if (weeklyCtx) {
            new Chart(weeklyCtx, {
                type: 'line',
                data: {
                    labels: window.weeklyProgressData?.labels || [],
                    datasets: [{
                        label: 'پیشرفت هفتگی',
                        data: window.weeklyProgressData?.data || [],
                        borderColor: '#667eea',
                        backgroundColor: 'rgba(102, 126, 234, 0.1)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            max: 100
                        }
                    }
                }
            });
        }
    }

    initializeClassCards() {
        $('.student-class-card').on('click', '.btn-continue-learning', function() {
            const classId = $(this).data('class-id');
            window.location.href = `/Student/Classes/Details/${classId}`;
        });

        $('.student-class-card').on('click', '.btn-view-progress', function() {
            const classId = $(this).data('class-id');
            window.location.href = `/Student/Classes/Progress/${classId}`;
        });
    }

    initializeLearningPath() {
        $('.student-path-step').on('click', function() {
            const lessonId = $(this).data('lesson-id');
            const isCompleted = $(this).hasClass('student-path-completed');
            
            if (!isCompleted) {
                StudentDashboard.startLesson(lessonId);
            }
        });
    }

    initializeAchievements() {
        $('.student-achievement').on('click', function() {
            const achievementId = $(this).data('achievement-id');
            StudentDashboard.showAchievementDetails(achievementId);
        });
    }

    static startLesson(lessonId) {
        window.location.href = `/Student/Lessons/Start/${lessonId}`;
    }

    static showAchievementDetails(achievementId) {
        $.ajax({
            url: `/Student/Achievements/Details/${achievementId}`,
            type: 'GET',
            success: function(data) {
                $('#achievementModal .modal-body').html(data);
                $('#achievementModal').modal('show');
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در بارگذاری جزئیات دستاورد');
            }
        });
    }

    static showToast(type, message) {
        const toast = $(`
            <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `);
        
        $('#toast-container').append(toast);
        toast.toast('show');
        
        setTimeout(() => toast.remove(), 5000);
    }
}

// Student Class Management
class StudentClassManagement {
    constructor() {
        this.initializeEnrollment();
        this.initializeProgressTracking();
        this.initializeLessonViewer();
    }

    initializeEnrollment() {
        $('.btn-enroll').on('click', function() {
            const classId = $(this).data('class-id');
            const className = $(this).data('class-name');
            
            if (confirm(`آیا می‌خواهید در کلاس "${className}" ثبت نام کنید؟`)) {
                StudentClassManagement.enrollInClass(classId);
            }
        });

        $('.btn-unenroll').on('click', function() {
            const classId = $(this).data('class-id');
            const className = $(this).data('class-name');
            
            if (confirm(`آیا می‌خواهید از کلاس "${className}" خارج شوید؟`)) {
                StudentClassManagement.unenrollFromClass(classId);
            }
        });
    }

    initializeProgressTracking() {
        $('.lesson-complete-btn').on('click', function() {
            const lessonId = $(this).data('lesson-id');
            const classId = $(this).data('class-id');
            
            StudentClassManagement.markLessonComplete(lessonId, classId);
        });

        $('.quiz-submit-btn').on('click', function() {
            const quizId = $(this).data('quiz-id');
            const answers = StudentClassManagement.collectQuizAnswers();
            
            StudentClassManagement.submitQuiz(quizId, answers);
        });
    }

    initializeLessonViewer() {
        // Video player initialization
        $('.lesson-video').each(function() {
            const video = this;
            
            video.addEventListener('ended', function() {
                const lessonId = $(this).data('lesson-id');
                const classId = $(this).data('class-id');
                
                StudentClassManagement.markLessonComplete(lessonId, classId);
            });
            
            // Track video progress
            video.addEventListener('timeupdate', function() {
                const progress = (this.currentTime / this.duration) * 100;
                const lessonId = $(this).data('lesson-id');
                
                StudentClassManagement.updateLessonProgress(lessonId, progress);
            });
        });

        // PDF viewer initialization
        $('.lesson-pdf').each(function() {
            const pdfUrl = $(this).data('pdf-url');
            const container = this;
            
            pdfjsLib.getDocument(pdfUrl).promise.then(function(pdf) {
                // Render first page
                pdf.getPage(1).then(function(page) {
                    const scale = 1.5;
                    const viewport = page.getViewport({ scale: scale });
                    
                    const canvas = document.createElement('canvas');
                    const context = canvas.getContext('2d');
                    canvas.height = viewport.height;
                    canvas.width = viewport.width;
                    
                    container.appendChild(canvas);
                    
                    const renderContext = {
                        canvasContext: context,
                        viewport: viewport
                    };
                    
                    page.render(renderContext);
                });
            });
        });
    }

    static enrollInClass(classId) {
        $.ajax({
            url: '/Student/Classes/Enroll',
            type: 'POST',
            data: {
                classId: classId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    StudentDashboard.showToast('success', response.message);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    StudentDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در ثبت نام');
            }
        });
    }

    static unenrollFromClass(classId) {
        $.ajax({
            url: '/Student/Classes/Unenroll',
            type: 'POST',
            data: {
                classId: classId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    StudentDashboard.showToast('success', response.message);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    StudentDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در لغو ثبت نام');
            }
        });
    }

    static markLessonComplete(lessonId, classId) {
        $.ajax({
            url: '/Student/Progress/MarkLessonComplete',
            type: 'POST',
            data: {
                lessonId: lessonId,
                classId: classId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    StudentDashboard.showToast('success', 'درس با موفقیت تکمیل شد');
                    // Update UI to show completion
                    $(`.lesson-item[data-lesson-id="${lessonId}"]`).addClass('completed');
                } else {
                    StudentDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در ثبت تکمیل درس');
            }
        });
    }

    static updateLessonProgress(lessonId, progress) {
        // Throttle progress updates to avoid too many requests
        if (!StudentClassManagement.progressUpdateTimeout) {
            StudentClassManagement.progressUpdateTimeout = setTimeout(() => {
                $.ajax({
                    url: '/Student/Progress/UpdateLessonProgress',
                    type: 'POST',
                    data: {
                        lessonId: lessonId,
                        progress: progress,
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                    },
                    complete: function() {
                        StudentClassManagement.progressUpdateTimeout = null;
                    }
                });
            }, 5000); // Update every 5 seconds
        }
    }

    static collectQuizAnswers() {
        const answers = {};
        $('.quiz-question').each(function() {
            const questionId = $(this).data('question-id');
            const questionType = $(this).data('question-type');
            
            if (questionType === 'multiple-choice') {
                answers[questionId] = $(this).find('input[type="radio"]:checked').val();
            } else if (questionType === 'multiple-select') {
                answers[questionId] = $(this).find('input[type="checkbox"]:checked').map(function() {
                    return $(this).val();
                }).get();
            } else if (questionType === 'text') {
                answers[questionId] = $(this).find('textarea').val();
            }
        });
        
        return answers;
    }

    static submitQuiz(quizId, answers) {
        $.ajax({
            url: '/Student/Quiz/Submit',
            type: 'POST',
            data: {
                quizId: quizId,
                answers: JSON.stringify(answers),
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    StudentDashboard.showToast('success', 'آزمون با موفقیت ارسال شد');
                    // Show results
                    $('#quizResults').html(response.results).show();
                    $('.quiz-submit-btn').prop('disabled', true);
                } else {
                    StudentDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در ارسال آزمون');
            }
        });
    }
}

// Student Learning Tools
class StudentLearningTools {
    constructor() {
        this.initializeNotesTaking();
        this.initializeBookmarks();
        this.initializeDiscussion();
    }

    initializeNotesTaking() {
        $('.btn-add-note').on('click', function() {
            const lessonId = $(this).data('lesson-id');
            StudentLearningTools.showNotesModal(lessonId);
        });

        $('#notesForm').on('submit', function(e) {
            e.preventDefault();
            const formData = $(this).serialize();
            StudentLearningTools.saveNote(formData);
        });
    }

    initializeBookmarks() {
        $('.btn-bookmark').on('click', function() {
            const lessonId = $(this).data('lesson-id');
            const isBookmarked = $(this).hasClass('bookmarked');
            
            if (isBookmarked) {
                StudentLearningTools.removeBookmark(lessonId);
            } else {
                StudentLearningTools.addBookmark(lessonId);
            }
        });
    }

    initializeDiscussion() {
        $('.btn-ask-question').on('click', function() {
            const lessonId = $(this).data('lesson-id');
            StudentLearningTools.showQuestionModal(lessonId);
        });

        $('#questionForm').on('submit', function(e) {
            e.preventDefault();
            const formData = $(this).serialize();
            StudentLearningTools.submitQuestion(formData);
        });
    }

    static showNotesModal(lessonId) {
        $('#notesLessonId').val(lessonId);
        $('#notesModal').modal('show');
    }

    static saveNote(formData) {
        $.ajax({
            url: '/Student/Notes/Save',
            type: 'POST',
            data: formData,
            success: function(response) {
                if (response.success) {
                    StudentDashboard.showToast('success', 'یادداشت ذخیره شد');
                    $('#notesModal').modal('hide');
                } else {
                    StudentDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در ذخیره یادداشت');
            }
        });
    }

    static addBookmark(lessonId) {
        $.ajax({
            url: '/Student/Bookmarks/Add',
            type: 'POST',
            data: {
                lessonId: lessonId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    $(`.btn-bookmark[data-lesson-id="${lessonId}"]`).addClass('bookmarked');
                    StudentDashboard.showToast('success', 'نشانک اضافه شد');
                } else {
                    StudentDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در افزودن نشانک');
            }
        });
    }

    static removeBookmark(lessonId) {
        $.ajax({
            url: '/Student/Bookmarks/Remove',
            type: 'POST',
            data: {
                lessonId: lessonId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    $(`.btn-bookmark[data-lesson-id="${lessonId}"]`).removeClass('bookmarked');
                    StudentDashboard.showToast('success', 'نشانک حذف شد');
                } else {
                    StudentDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در حذف نشانک');
            }
        });
    }

    static showQuestionModal(lessonId) {
        $('#questionLessonId').val(lessonId);
        $('#questionModal').modal('show');
    }

    static submitQuestion(formData) {
        $.ajax({
            url: '/Student/Discussion/AskQuestion',
            type: 'POST',
            data: formData,
            success: function(response) {
                if (response.success) {
                    StudentDashboard.showToast('success', 'سوال ارسال شد');
                    $('#questionModal').modal('hide');
                } else {
                    StudentDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                StudentDashboard.showToast('error', 'خطا در ارسال سوال');
            }
        });
    }
}

// Initialize student functionality when document is ready
$(document).ready(function() {
    // Create toast container if it doesn't exist
    if ($('#toast-container').length === 0) {
        $('body').append('<div id="toast-container" class="toast-container position-fixed top-0 end-0 p-3"></div>');
    }

    // Initialize student modules based on current page
    const currentPath = window.location.pathname;
    
    if (currentPath.includes('/Student/Home') || currentPath === '/Student') {
        new StudentDashboard();
    }
    
    if (currentPath.includes('/Student/Classes')) {
        new StudentClassManagement();
    }
    
    // Initialize learning tools on all student pages
    new StudentLearningTools();

    // Global student functionality
    $('.btn-confirm-action').on('click', function() {
        const message = $(this).data('confirm-message') || 'آیا مطمئن هستید؟';
        return confirm(message);
    });

    // Auto-hide alerts after 5 seconds
    $('.alert').delay(5000).fadeOut();

    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();

    // Initialize progress bars animation
    $('.student-progress-fill').each(function() {
        const progress = $(this).data('progress') || 0;
        $(this).animate({ width: progress + '%' }, 1000);
    });
});

// Student Authentication Functions
class StudentAuth {
    static logout() {
        if (confirm('آیا مطمئن هستید که می‌خواهید از سیستم خارج شوید؟')) {
            // Create a form dynamically and submit it
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '/Public/Account/Logout';
            
            // Add anti-forgery token if available
            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                const tokenInput = document.createElement('input');
                tokenInput.type = 'hidden';
                tokenInput.name = '__RequestVerificationToken';
                tokenInput.value = token.value;
                form.appendChild(tokenInput);
            }
            
            // Submit the form
            document.body.appendChild(form);
            form.submit();
        }
    }
}

// Export functions for global use
window.StudentDashboard = StudentDashboard;
window.StudentClassManagement = StudentClassManagement;
window.StudentLearningTools = StudentLearningTools;
window.StudentAuth = StudentAuth;
