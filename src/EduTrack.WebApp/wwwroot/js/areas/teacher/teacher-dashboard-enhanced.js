// Enhanced Teacher Dashboard JavaScript

$(document).ready(function() {
    // Update time display
    function updateTime() {
        const now = new Date();
        const timeString = now.toLocaleTimeString('fa-IR', { 
            hour: '2-digit', 
            minute: '2-digit'
        });
        $('#currentTime').text(timeString);
    }
    
    // Initialize time display
    updateTime();
    setInterval(updateTime, 1000);

    // Add smooth scroll behavior for quick links
    $('.quick-link').on('click', function(e) {
        const href = $(this).attr('href');
        if (href && href.startsWith('#')) {
            e.preventDefault();
            const target = $(href);
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 100
                }, 500);
            }
        }
    });

    // Add ripple effect to interactive elements
    $('.quick-action-card, .course-card.enhanced, .stats-card-enhanced').on('click', function(e) {
        const $this = $(this);
        const ripple = $('<span class="ripple-effect"></span>');
        
        const rect = this.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = e.clientX - rect.left - size / 2;
        const y = e.clientY - rect.top - size / 2;
        
        ripple.css({
            width: size,
            height: size,
            left: x,
            top: y
        });
        
        $this.append(ripple);
        
        setTimeout(() => {
            ripple.remove();
        }, 600);
    });

    // Add loading state to buttons
    $('.btn').on('click', function() {
        const $btn = $(this);
        if (!$btn.hasClass('btn-loading')) {
            $btn.addClass('btn-loading');
            setTimeout(() => {
                $btn.removeClass('btn-loading');
            }, 2000);
        }
    });

    // Initialize tooltips for stats trends
    $('.stats-trend').each(function() {
        const $this = $(this);
        const trend = $this.hasClass('positive') ? 'افزایش' : 
                     $this.hasClass('negative') ? 'کاهش' : 'بدون تغییر';
        
        $this.attr('title', `روند: ${trend}`);
    });

    // Add animation classes on scroll
    function animateOnScroll() {
        $('.stats-card-enhanced, .dashboard-panel').each(function() {
            const elementTop = $(this).offset().top;
            const elementBottom = elementTop + $(this).outerHeight();
            const viewportTop = $(window).scrollTop();
            const viewportBottom = viewportTop + $(window).height();

            if (elementBottom > viewportTop && elementTop < viewportBottom) {
                $(this).addClass('animate-fade-in-up');
            }
        });
    }

    // Run animation check on scroll
    $(window).on('scroll', animateOnScroll);
    animateOnScroll(); // Run once on load

    // Add hover effects for course cards
    $('.course-card.enhanced').hover(
        function() {
            $(this).find('.course-card-image').css('transform', 'scale(1.05)');
        },
        function() {
            $(this).find('.course-card-image').css('transform', 'scale(1)');
        }
    );

    // Add keyboard navigation support
    $('.quick-action-card, .quick-link').on('keydown', function(e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            $(this).click();
        }
    });

    // Add focus management for accessibility
    $('.quick-action-card, .quick-link').attr('tabindex', '0');

    // Initialize activity timeline animations
    $('.activity-item').each(function(index) {
        $(this).css('animation-delay', `${index * 0.1}s`);
        $(this).addClass('animate-slide-in-right');
    });

    // Add click tracking for analytics (if needed)
    $('.quick-action-card').on('click', function() {
        const action = $(this).find('.quick-action-text').text();
        console.log(`Quick action clicked: ${action}`);
        // Add analytics tracking here if needed
    });

    // Add error handling for failed requests
    $(document).ajaxError(function(event, xhr, settings, thrownError) {
        console.error('AJAX Error:', thrownError);
        // Show user-friendly error message
        if (xhr.status === 0) {
            console.log('Network error - please check your connection');
        }
    });

    // Add performance monitoring
    const startTime = performance.now();
    $(window).on('load', function() {
        const loadTime = performance.now() - startTime;
        console.log(`Dashboard loaded in ${loadTime.toFixed(2)}ms`);
    });
});
