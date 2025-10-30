// Unified toast notifications - global usage across the app
// Provides: window.toastSuccess, window.toastError, window.toastInfo, window.toastWarning

(function () {
	function ensureToastrDefaults() {
		if (typeof toastr === 'undefined') return;
		toastr.options = Object.assign({
			closeButton: true,
			progressBar: true,
			newestOnTop: true,
			positionClass: 'toast-top-right',
			timeOut: 4000,
			extendedTimeOut: 2000,
			preventDuplicates: true
		}, toastr.options || {});
	}

	function fallbackAlert(prefix, message) {
		try { alert((prefix ? prefix + ' ' : '') + message); } catch (_) {}
	}

	window.toastSuccess = function (message) {
		ensureToastrDefaults();
		if (typeof toastr !== 'undefined') toastr.success(message, 'موفقیت');
		else fallbackAlert('موفق:', message);
	};

	window.toastError = function (message) {
		ensureToastrDefaults();
		if (typeof toastr !== 'undefined') toastr.error(message, 'خطا');
		else fallbackAlert('خطا:', message);
	};

	window.toastInfo = function (message) {
		ensureToastrDefaults();
		if (typeof toastr !== 'undefined') toastr.info(message, 'اطلاع');
		else fallbackAlert('', message);
	};

	window.toastWarning = function (message) {
		ensureToastrDefaults();
		if (typeof toastr !== 'undefined') toastr.warning(message, 'هشدار');
		else fallbackAlert('هشدار:', message);
	};
})();


