// Initializes study page configuration without inline scripts in views
(function () {
	function initConfigFromDataset() {
		var container = document.getElementById('schedule-item-content');
		if (!container) return;

		var scheduleItemId = parseInt(container.getAttribute('data-item-id') || '0');
		var contentType = container.getAttribute('data-type') || '';
		var courseId = parseInt(container.getAttribute('data-course-id') || '0');
		var activeSessionId = parseInt(container.getAttribute('data-active-session-id') || '0');
		var currentUserId = container.getAttribute('data-current-user-id') || '';

		window.studyContentConfig = {
			scheduleItemId: scheduleItemId,
			contentType: contentType,
			activeSessionId: activeSessionId,
			currentUserId: currentUserId,
			courseId: courseId
		};
	}

	function onReady(cb) {
		if (document.readyState === 'loading') {
			document.addEventListener('DOMContentLoaded', cb);
		} else {
			cb();
		}
	}

	onReady(function () {
		initConfigFromDataset();
		// Hide app chrome to make a book-like minimal view
		document.body.classList.add('study-content-page-active');
		if (window.studySession && typeof window.studySession.init === 'function') {
			window.studySession.init();
		}
	});

	window.addEventListener('beforeunload', function () {
		document.body.classList.remove('study-content-page-active');
	});
})();


