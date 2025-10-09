$(document).ready(function () {
    // Tab functionality
    $('.tab-btn').on('click', function () {
        const tabId = $(this).data('tab');

        // Update active tab button
        $('.tab-btn').removeClass('active');
        $(this).addClass('active');

        // Show corresponding tab panel
        $('.tab-panel').removeClass('active');
        $(`#${tabId}`).addClass('active');
    });
});
