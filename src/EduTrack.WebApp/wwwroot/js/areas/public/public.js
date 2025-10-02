// Public Area JavaScript

// Public Landing Page functionality
class PublicLanding {
    constructor() {
        this.initializeHeroAnimations();
        this.initializeFeatureCards();
        this.initializeCourseCarousel();
        this.initializeScrollEffects();
    }

    initializeHeroAnimations() {
        // Animate hero elements on load
        $('.public-hero-title').addClass('animate__animated animate__fadeInUp');
        $('.public-hero-subtitle').addClass('animate__animated animate__fadeInUp animate__delay-1s');
        $('.public-hero-cta').addClass('animate__animated animate__fadeInUp animate__delay-2s');
    }

    initializeFeatureCards() {
        // Add hover effects and animations to feature cards
        $('.public-feature-card').on('mouseenter', function() {
            $(this).addClass('animate__animated animate__pulse');
        }).on('mouseleave', function() {
            $(this).removeClass('animate__animated animate__pulse');
        });

        // Intersection Observer for scroll animations
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    $(entry.target).addClass('animate__animated animate__fadeInUp');
                }
            });
        }, { threshold: 0.1 });

        $('.public-feature-card').each(function() {
            observer.observe(this);
        });
    }

    initializeCourseCarousel() {
        // Initialize course carousel if exists
        const carousel = $('.public-course-carousel');
        if (carousel.length) {
            carousel.owlCarousel({
                loop: true,
                margin: 30,
                nav: true,
                dots: true,
                autoplay: true,
                autoplayTimeout: 5000,
                responsive: {
                    0: { items: 1 },
                    768: { items: 2 },
                    1024: { items: 3 }
                }
            });
        }
    }

    initializeScrollEffects() {
        // Smooth scrolling for anchor links
        $('a[href^="#"]').on('click', function(e) {
            e.preventDefault();
            const target = $($(this).attr('href'));
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 80
                }, 800);
            }
        });

        // Navbar background change on scroll
        $(window).on('scroll', function() {
            const scrollTop = $(this).scrollTop();
            const navbar = $('.public-navbar');
            
            if (scrollTop > 100) {
                navbar.addClass('scrolled');
            } else {
                navbar.removeClass('scrolled');
            }
        });
    }
}

// Public Authentication functionality
class PublicAuth {
    constructor() {
        this.initializeLoginForm();
        this.initializeRegisterForm();
        this.initializePasswordToggle();
        this.initializeFormValidation();
    }

    initializeLoginForm() {
        $('#loginForm').on('submit', function(e) {
            if (!PublicAuth.validateLoginForm()) {
                e.preventDefault();
                return false;
            }
            
            // Show loading state
            const submitBtn = $(this).find('button[type="submit"]');
            submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> در حال ورود...');
        });
    }

    initializeRegisterForm() {
        $('#registerForm').on('submit', function(e) {
            if (!PublicAuth.validateRegisterForm()) {
                e.preventDefault();
                return false;
            }
            
            // Show loading state
            const submitBtn = $(this).find('button[type="submit"]');
            submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> در حال ثبت نام...');
        });

        // Real-time password strength indicator
        $('#Password').on('input', function() {
            const password = $(this).val();
            PublicAuth.updatePasswordStrength(password);
        });

        // Confirm password validation
        $('#ConfirmPassword').on('input', function() {
            const password = $('#Password').val();
            const confirmPassword = $(this).val();
            
            if (password !== confirmPassword) {
                $(this).addClass('is-invalid');
                $('.confirm-password-feedback').text('رمز عبور و تکرار آن یکسان نیستند');
            } else {
                $(this).removeClass('is-invalid').addClass('is-valid');
                $('.confirm-password-feedback').text('');
            }
        });
    }

    initializePasswordToggle() {
        $('.password-toggle').on('click', function() {
            const input = $(this).siblings('input');
            const icon = $(this).find('i');
            
            if (input.attr('type') === 'password') {
                input.attr('type', 'text');
                icon.removeClass('fa-eye').addClass('fa-eye-slash');
            } else {
                input.attr('type', 'password');
                icon.removeClass('fa-eye-slash').addClass('fa-eye');
            }
        });
    }

    initializeFormValidation() {
        // Real-time validation for all form inputs
        $('.public-auth-control').on('blur', function() {
            PublicAuth.validateField($(this));
        });

        $('.public-auth-control').on('input', function() {
            if ($(this).hasClass('is-invalid')) {
                PublicAuth.validateField($(this));
            }
        });
    }

    static validateLoginForm() {
        let isValid = true;
        const email = $('#Email');
        const password = $('#Password');

        // Email validation
        if (!email.val() || !PublicAuth.isValidEmail(email.val())) {
            email.addClass('is-invalid');
            isValid = false;
        } else {
            email.removeClass('is-invalid').addClass('is-valid');
        }

        // Password validation
        if (!password.val() || password.val().length < 6) {
            password.addClass('is-invalid');
            isValid = false;
        } else {
            password.removeClass('is-invalid').addClass('is-valid');
        }

        return isValid;
    }

    static validateRegisterForm() {
        let isValid = true;
        const fields = ['FirstName', 'LastName', 'Email', 'Password', 'ConfirmPassword'];

        fields.forEach(fieldName => {
            const field = $(`#${fieldName}`);
            if (!PublicAuth.validateField(field)) {
                isValid = false;
            }
        });

        return isValid;
    }

    static validateField(field) {
        const fieldName = field.attr('id');
        const value = field.val();
        let isValid = true;
        let errorMessage = '';

        switch (fieldName) {
            case 'FirstName':
            case 'LastName':
                if (!value || value.length < 2) {
                    isValid = false;
                    errorMessage = 'حداقل 2 کاراکتر وارد کنید';
                }
                break;

            case 'Email':
                if (!value || !PublicAuth.isValidEmail(value)) {
                    isValid = false;
                    errorMessage = 'ایمیل معتبر وارد کنید';
                }
                break;

            case 'Password':
                if (!value || value.length < 6) {
                    isValid = false;
                    errorMessage = 'رمز عبور باید حداقل 6 کاراکتر باشد';
                }
                break;

            case 'ConfirmPassword':
                const password = $('#Password').val();
                if (!value || value !== password) {
                    isValid = false;
                    errorMessage = 'رمز عبور و تکرار آن یکسان نیستند';
                }
                break;
        }

        if (isValid) {
            field.removeClass('is-invalid').addClass('is-valid');
            field.siblings('.invalid-feedback').text('');
        } else {
            field.removeClass('is-valid').addClass('is-invalid');
            field.siblings('.invalid-feedback').text(errorMessage);
        }

        return isValid;
    }

    static updatePasswordStrength(password) {
        const strengthIndicator = $('.password-strength');
        const strengthText = $('.password-strength-text');
        
        if (!password) {
            strengthIndicator.removeClass().addClass('password-strength');
            strengthText.text('');
            return;
        }

        let strength = 0;
        let strengthLabel = '';

        // Length check
        if (password.length >= 8) strength++;
        
        // Uppercase check
        if (/[A-Z]/.test(password)) strength++;
        
        // Lowercase check
        if (/[a-z]/.test(password)) strength++;
        
        // Number check
        if (/\d/.test(password)) strength++;
        
        // Special character check
        if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength++;

        switch (strength) {
            case 0:
            case 1:
                strengthIndicator.removeClass().addClass('password-strength weak');
                strengthLabel = 'ضعیف';
                break;
            case 2:
            case 3:
                strengthIndicator.removeClass().addClass('password-strength medium');
                strengthLabel = 'متوسط';
                break;
            case 4:
            case 5:
                strengthIndicator.removeClass().addClass('password-strength strong');
                strengthLabel = 'قوی';
                break;
        }

        strengthText.text(`قدرت رمز عبور: ${strengthLabel}`);
    }

    static isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
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

// Public Course Catalog functionality
class PublicCatalog {
    constructor() {
        this.initializeFilters();
        this.initializeSearch();
        this.initializePagination();
        this.initializeCourseCards();
    }

    initializeFilters() {
        $('.catalog-filter').on('change', function() {
            PublicCatalog.applyFilters();
        });

        $('.filter-clear').on('click', function() {
            $('.catalog-filter').val('');
            PublicCatalog.applyFilters();
        });
    }

    initializeSearch() {
        let searchTimeout;
        $('#catalogSearch').on('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                PublicCatalog.applyFilters();
            }, 500);
        });
    }

    initializePagination() {
        $('.pagination a').on('click', function(e) {
            e.preventDefault();
            const page = $(this).data('page');
            if (page) {
                PublicCatalog.loadPage(page);
            }
        });
    }

    initializeCourseCards() {
        $('.public-course-card').on('click', '.btn-view-course', function() {
            const courseId = $(this).data('course-id');
            window.location.href = `/Public/Catalog/Course/${courseId}`;
        });

        $('.public-course-card').on('click', '.btn-enroll-preview', function() {
            const courseId = $(this).data('course-id');
            PublicCatalog.showEnrollmentPreview(courseId);
        });
    }

    static applyFilters() {
        const filters = {
            search: $('#catalogSearch').val(),
            category: $('#categoryFilter').val(),
            level: $('#levelFilter').val(),
            duration: $('#durationFilter').val()
        };

        const queryString = $.param(filters);
        window.location.href = `/Public/Catalog?${queryString}`;
    }

    static loadPage(page) {
        const currentUrl = new URL(window.location);
        currentUrl.searchParams.set('page', page);
        window.location.href = currentUrl.toString();
    }

    static showEnrollmentPreview(courseId) {
        if (!$('#enrollmentPreviewModal').length) {
            $('body').append(`
                <div class="modal fade" id="enrollmentPreviewModal" tabindex="-1">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">پیش‌نمایش ثبت نام</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body"></div>
                        </div>
                    </div>
                </div>
            `);
        }

        $.ajax({
            url: `/Public/Catalog/EnrollmentPreview/${courseId}`,
            type: 'GET',
            success: function(data) {
                $('#enrollmentPreviewModal .modal-body').html(data);
                $('#enrollmentPreviewModal').modal('show');
            },
            error: function() {
                PublicAuth.showToast('error', 'خطا در بارگذاری اطلاعات دوره');
            }
        });
    }
}

// Public Newsletter Subscription
class PublicNewsletter {
    constructor() {
        this.initializeSubscription();
    }

    initializeSubscription() {
        $('#newsletterForm').on('submit', function(e) {
            e.preventDefault();
            const email = $('#newsletterEmail').val();
            
            if (!PublicAuth.isValidEmail(email)) {
                PublicAuth.showToast('error', 'ایمیل معتبر وارد کنید');
                return;
            }

            PublicNewsletter.subscribe(email);
        });
    }

    static subscribe(email) {
        $.ajax({
            url: '/Public/Newsletter/Subscribe',
            type: 'POST',
            data: {
                email: email,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    PublicAuth.showToast('success', 'با موفقیت در خبرنامه عضو شدید');
                    $('#newsletterForm')[0].reset();
                } else {
                    PublicAuth.showToast('error', response.message);
                }
            },
            error: function() {
                PublicAuth.showToast('error', 'خطا در عضویت در خبرنامه');
            }
        });
    }
}

// Initialize public functionality when document is ready
$(document).ready(function() {
    // Create toast container if it doesn't exist
    if ($('#toast-container').length === 0) {
        $('body').append('<div id="toast-container" class="toast-container position-fixed top-0 end-0 p-3"></div>');
    }

    // Initialize public modules based on current page
    const currentPath = window.location.pathname;
    
    if (currentPath === '/' || currentPath.includes('/Public/Home')) {
        new PublicLanding();
    }
    
    if (currentPath.includes('/Public/Account')) {
        new PublicAuth();
    }
    
    if (currentPath.includes('/Public/Catalog')) {
        new PublicCatalog();
    }

    // Initialize newsletter on all public pages
    new PublicNewsletter();

    // Global public functionality
    $('.btn-scroll-to-top').on('click', function() {
        $('html, body').animate({ scrollTop: 0 }, 800);
    });

    // Show/hide scroll to top button
    $(window).on('scroll', function() {
        const scrollTop = $(this).scrollTop();
        const scrollBtn = $('.btn-scroll-to-top');
        
        if (scrollTop > 300) {
            scrollBtn.fadeIn();
        } else {
            scrollBtn.fadeOut();
        }
    });

    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();

    // Auto-hide alerts after 5 seconds
    $('.alert').delay(5000).fadeOut();

    // Initialize AOS (Animate On Scroll) if available
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 800,
            once: true
        });
    }
});

// Export functions for global use
window.PublicLanding = PublicLanding;
window.PublicAuth = PublicAuth;
window.PublicCatalog = PublicCatalog;
window.PublicNewsletter = PublicNewsletter;
