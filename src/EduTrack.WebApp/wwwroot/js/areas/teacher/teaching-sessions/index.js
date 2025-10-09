$(document).ready(function () {
    // Filter functionality
    $('.filter-btn-minimal').on('click', function () {
        const filter = $(this).data('filter');

        // Update active button
        $('.filter-btn-minimal').removeClass('active');
        $(this).addClass('active');

        // Filter sessions
        $('.session-card').each(function () {
            const sessionDate = new Date($(this).data('session-date'));
            const today = new Date();
            const startOfWeek = new Date(today.setDate(today.getDate() - today.getDay()));

            if (filter === 'all') {
                $(this).show();
            } else if (filter === 'today') {
                const isToday = sessionDate.toDateString() === new Date().toDateString();
                $(this).toggle(isToday);
            } else if (filter === 'this-week') {
                const isThisWeek = sessionDate >= startOfWeek;
                $(this).toggle(isThisWeek);
            }
        });
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        var alerts = document.querySelectorAll('.alert');
        alerts.forEach(function (alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});
