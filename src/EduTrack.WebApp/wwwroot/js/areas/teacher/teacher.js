// Teacher Area JavaScript

// Teacher Dashboard functionality
class TeacherDashboard {
    constructor() {
        this.initializeClassCards();
        this.initializeStudentManagement();
        this.initializeProgressTracking();
        this.initializeCalendar();
    }

    initializeClassCards() {
        $('.teacher-class-card').on('click', '.btn-view-class', function() {
            const classId = $(this).data('class-id');
            window.location.href = `/Teacher/Classes/Details/${classId}`;
        });

        $('.teacher-class-card').on('click', '.btn-edit-class', function() {
            const classId = $(this).data('class-id');
            window.location.href = `/Teacher/Classes/Edit/${classId}`;
        });
    }

    initializeStudentManagement() {
        $('.btn-view-student-progress').on('click', function() {
            const studentId = $(this).data('student-id');
            const classId = $(this).data('class-id');
            TeacherDashboard.showStudentProgress(studentId, classId);
        });

        $('.btn-send-message').on('click', function() {
            const studentId = $(this).data('student-id');
            TeacherDashboard.showMessageModal(studentId);
        });
    }

    initializeProgressTracking() {
        $('.progress-update-form').on('submit', function(e) {
            e.preventDefault();
            const formData = $(this).serialize();
            TeacherDashboard.updateStudentProgress(formData);
        });
    }

    initializeCalendar() {
        const calendarEl = document.getElementById('teacher-calendar');
        if (calendarEl) {
            const calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                locale: 'fa',
                direction: 'rtl',
                events: '/Teacher/Classes/GetCalendarEvents',
                eventClick: function(info) {
                    TeacherDashboard.showClassDetails(info.event.id);
                },
                headerToolbar: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'dayGridMonth,timeGridWeek,timeGridDay'
                }
            });
            calendar.render();
        }
    }

    static showStudentProgress(studentId, classId) {
        $.ajax({
            url: `/Teacher/Classes/GetStudentProgress/${studentId}/${classId}`,
            type: 'GET',
            success: function(data) {
                $('#studentProgressModal .modal-body').html(data);
                $('#studentProgressModal').modal('show');
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در بارگذاری اطلاعات پیشرفت دانش‌آموز');
            }
        });
    }

    static showMessageModal(studentId) {
        $('#messageStudentId').val(studentId);
        $('#messageModal').modal('show');
    }

    static updateStudentProgress(formData) {
        $.ajax({
            url: '/Teacher/Progress/Update',
            type: 'POST',
            data: formData,
            success: function(response) {
                if (response.success) {
                    TeacherDashboard.showToast('success', 'پیشرفت دانش‌آموز با موفقیت بروزرسانی شد');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    TeacherDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در بروزرسانی پیشرفت');
            }
        });
    }

    static showClassDetails(classId) {
        window.location.href = `/Teacher/Classes/Details/${classId}`;
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

// Teacher Class Management
class TeacherClassManagement {
    constructor() {
        this.initializeClassForm();
        this.initializeEnrollmentManagement();
        this.initializeAttendanceTracking();
    }

    initializeClassForm() {
        $('#classForm').on('submit', function(e) {
            if (!TeacherClassManagement.validateClassForm()) {
                e.preventDefault();
                return false;
            }
        });

        // Date picker initialization
        $('.date-picker').each(function() {
            $(this).persianDatepicker({
                format: 'YYYY/MM/DD',
                calendar: {
                    persian: {
                        locale: 'fa'
                    }
                }
            });
        });

        // Course selection change handler
        $('#CourseId').on('change', function() {
            const courseId = $(this).val();
            if (courseId) {
                TeacherClassManagement.loadCourseModules(courseId);
            }
        });
    }

    initializeEnrollmentManagement() {
        $('.btn-approve-enrollment').on('click', function() {
            const enrollmentId = $(this).data('enrollment-id');
            TeacherClassManagement.approveEnrollment(enrollmentId);
        });

        $('.btn-reject-enrollment').on('click', function() {
            const enrollmentId = $(this).data('enrollment-id');
            if (confirm('آیا مطمئن هستید که می‌خواهید این ثبت نام را رد کنید؟')) {
                TeacherClassManagement.rejectEnrollment(enrollmentId);
            }
        });
    }

    initializeAttendanceTracking() {
        $('.attendance-checkbox').on('change', function() {
            const studentId = $(this).data('student-id');
            const classId = $(this).data('class-id');
            const isPresent = $(this).is(':checked');
            
            TeacherClassManagement.updateAttendance(studentId, classId, isPresent);
        });

        $('.btn-mark-all-present').on('click', function() {
            $('.attendance-checkbox').prop('checked', true).trigger('change');
        });

        $('.btn-mark-all-absent').on('click', function() {
            $('.attendance-checkbox').prop('checked', false).trigger('change');
        });
    }

    static validateClassForm() {
        let isValid = true;
        const requiredFields = ['Name', 'CourseId', 'StartDate', 'EndDate', 'MaxStudents'];
        
        requiredFields.forEach(field => {
            const element = $(`#${field}`);
            if (!element.val()) {
                element.addClass('is-invalid');
                isValid = false;
            } else {
                element.removeClass('is-invalid');
            }
        });

        // Validate date range
        const startDate = new Date($('#StartDate').val());
        const endDate = new Date($('#EndDate').val());
        
        if (startDate >= endDate) {
            $('#EndDate').addClass('is-invalid');
            TeacherDashboard.showToast('error', 'تاریخ پایان باید بعد از تاریخ شروع باشد');
            isValid = false;
        }

        return isValid;
    }

    static loadCourseModules(courseId) {
        $.ajax({
            url: `/Teacher/Courses/GetModules/${courseId}`,
            type: 'GET',
            success: function(modules) {
                const moduleSelect = $('#ModuleId');
                moduleSelect.empty().append('<option value="">انتخاب ماژول</option>');
                
                modules.forEach(module => {
                    moduleSelect.append(`<option value="${module.id}">${module.title}</option>`);
                });
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در بارگذاری ماژول‌ها');
            }
        });
    }

    static approveEnrollment(enrollmentId) {
        $.ajax({
            url: '/Teacher/Enrollments/Approve',
            type: 'POST',
            data: {
                enrollmentId: enrollmentId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    TeacherDashboard.showToast('success', 'ثبت نام تایید شد');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    TeacherDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در تایید ثبت نام');
            }
        });
    }

    static rejectEnrollment(enrollmentId) {
        $.ajax({
            url: '/Teacher/Enrollments/Reject',
            type: 'POST',
            data: {
                enrollmentId: enrollmentId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    TeacherDashboard.showToast('success', 'ثبت نام رد شد');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    TeacherDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در رد ثبت نام');
            }
        });
    }

    static updateAttendance(studentId, classId, isPresent) {
        $.ajax({
            url: '/Teacher/Attendance/Update',
            type: 'POST',
            data: {
                studentId: studentId,
                classId: classId,
                isPresent: isPresent,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (!response.success) {
                    TeacherDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در ثبت حضور و غیاب');
            }
        });
    }
}

// Teacher Course Management
class TeacherCourseManagement {
    constructor() {
        this.initializeCourseActions();
        this.initializeModuleManagement();
        this.initializeLessonManagement();
    }

    initializeCourseActions() {
        $('.btn-add-module').on('click', function() {
            const courseId = $(this).data('course-id');
            TeacherCourseManagement.showAddModuleModal(courseId);
        });

        $('.btn-edit-module').on('click', function() {
            const moduleId = $(this).data('module-id');
            TeacherCourseManagement.showEditModuleModal(moduleId);
        });
    }

    initializeModuleManagement() {
        $('#moduleForm').on('submit', function(e) {
            e.preventDefault();
            const formData = $(this).serialize();
            TeacherCourseManagement.saveModule(formData);
        });
    }

    initializeLessonManagement() {
        $('.btn-add-lesson').on('click', function() {
            const moduleId = $(this).data('module-id');
            TeacherCourseManagement.showAddLessonModal(moduleId);
        });

        $('#lessonForm').on('submit', function(e) {
            e.preventDefault();
            const formData = $(this).serialize();
            TeacherCourseManagement.saveLesson(formData);
        });
    }

    static showAddModuleModal(courseId) {
        $('#moduleForm')[0].reset();
        $('#ModuleCourseId').val(courseId);
        $('#moduleModal .modal-title').text('افزودن ماژول جدید');
        $('#moduleModal').modal('show');
    }

    static showEditModuleModal(moduleId) {
        $.ajax({
            url: `/Teacher/Modules/GetModule/${moduleId}`,
            type: 'GET',
            success: function(module) {
                $('#ModuleId').val(module.id);
                $('#ModuleTitle').val(module.title);
                $('#ModuleDescription').val(module.description);
                $('#ModuleOrder').val(module.order);
                $('#moduleModal .modal-title').text('ویرایش ماژول');
                $('#moduleModal').modal('show');
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در بارگذاری اطلاعات ماژول');
            }
        });
    }

    static saveModule(formData) {
        $.ajax({
            url: '/Teacher/Modules/Save',
            type: 'POST',
            data: formData,
            success: function(response) {
                if (response.success) {
                    TeacherDashboard.showToast('success', 'ماژول با موفقیت ذخیره شد');
                    $('#moduleModal').modal('hide');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    TeacherDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در ذخیره ماژول');
            }
        });
    }

    static showAddLessonModal(moduleId) {
        $('#lessonForm')[0].reset();
        $('#LessonModuleId').val(moduleId);
        $('#lessonModal .modal-title').text('افزودن درس جدید');
        $('#lessonModal').modal('show');
    }

    static saveLesson(formData) {
        $.ajax({
            url: '/Teacher/Lessons/Save',
            type: 'POST',
            data: formData,
            success: function(response) {
                if (response.success) {
                    TeacherDashboard.showToast('success', 'درس با موفقیت ذخیره شد');
                    $('#lessonModal').modal('hide');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    TeacherDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                TeacherDashboard.showToast('error', 'خطا در ذخیره درس');
            }
        });
    }
}

// Initialize teacher functionality when document is ready
$(document).ready(function() {
    // Create toast container if it doesn't exist
    if ($('#toast-container').length === 0) {
        $('body').append('<div id="toast-container" class="toast-container position-fixed top-0 end-0 p-3"></div>');
    }

    // Initialize teacher modules based on current page
    const currentPath = window.location.pathname;
    
    if (currentPath.includes('/Teacher/Home') || currentPath === '/Teacher') {
        new TeacherDashboard();
    }
    
    if (currentPath.includes('/Teacher/Classes')) {
        new TeacherClassManagement();
    }
    
    if (currentPath.includes('/Teacher/Courses')) {
        new TeacherCourseManagement();
    }

    // Global teacher functionality
    $('.btn-confirm-action').on('click', function() {
        const message = $(this).data('confirm-message') || 'آیا مطمئن هستید؟';
        return confirm(message);
    });

    // Auto-hide alerts after 5 seconds
    $('.alert').delay(5000).fadeOut();

    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
});

// Export functions for global use
window.TeacherDashboard = TeacherDashboard;
window.TeacherClassManagement = TeacherClassManagement;
window.TeacherCourseManagement = TeacherCourseManagement;
