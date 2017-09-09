var ConnectCMS = ConnectCMS || {};
ConnectCMS.Utilities = ConnectCMS.Utilities || {};

ConnectCMS.Utilities.PNotify = function () {
	PNotify.prototype.options.styling = 'jqueryui';

	var success = function (title, message) {
		var p = new PNotify({
			title: title,
			text: message,
			animation: 'none',
			buttons: {
				closer: true,
				closer_hover: false,
				sticker: false
			}
		});
	};

	var error = function (title, message) {
		var p = new PNotify({
			title: title,
			text: message,
			type: 'error',
			animation: 'none',
			buttons: {
				closer: true,
				closer_hover: false,
				sticker: false
			}
		});
	};

	var desktopNotification = function (title, message) {
		PNotify.desktop.permission();

		var p = new PNotify({
			title: title,
			text: message,
			type: 'info',
			desktop: {
				desktop: true
			},
			hide: false,
			buttons: {
				closer: true,
				closer_hover: false,
				sticker: false
			}
		});

		return p;
	};

	var updateDesktopNotification = function (notification, title, message) {
		PNotify.desktop.permission();

		notification.update({
			title: title,
			text: message,
		});
	};

	return {
		success: success,
		error: error,
		desktopNotification: desktopNotification,
		updateDesktopNotification: updateDesktopNotification
	}
}();