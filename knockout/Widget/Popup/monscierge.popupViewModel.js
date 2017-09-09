function PopupViewModel(options) {
	var self = this;
	var headers = options != null ? options.headers : null;
	var body = options != null ? options.body : null;
	var cancelButton = options != null ? options.cancelButton : null;
	var contentData = options != null ? options.contentData : null;
	var contentUrl = options != null ? options.contentUrl : null;
	var contentViewModel = options != null ? options.contentViewModel : null;
	var header = options != null ? options.header : null;
	var okButton = options != null ? options.okButton : null;
	var onClose = options != null ? options.onClose : null;
	var onOk = options != null ? options.onOk : null;
	var onOpen = options != null ? options.onOpen : null;
	var title = options != null ? options.title : null;
	var stackTrace = options != null ? options.stackTrace : null;
	var content = options != null ? options.content : null;

	self.Content = content;
	self.ContentData = contentData == null ? (cancelButton == null ? {
		header: header,
		body: body,
		stackTrace: stackTrace,
		button: okButton
	} : {
		header: header,
		body: body,
		okButton: okButton,
		cancelButton: cancelButton
	}) : contentData;
	self.Body = body;
	self.Header = header;
	self.Button = okButton;
	self.ContentUrl = contentUrl == null ? (cancelButton == null ? '/ConnectCMS/Popup/Alert' : '/ConnectCMS/Popup/Dialog') : contentUrl;
	self.ContentViewModel = contentViewModel == null ? self : contentViewModel;
	self.OnClose = onClose;
	self.OnOk = onOk;
	self.OnOpen = onOpen;
	self.Title = title;

	self.Result = false;

	self.Close = function () {
		if (self.OnClose != null)
			if (!self.OnClose())
				return;

		ConnectCMS.MainViewModel.ShowOverlay(false);
	};
	self.Dispose = function () {
		var node = $('#popup-content > .innerContent');
		if (node.length == 0) return;
		ko.cleanNode(node[0]);
		$('#popup-content').html('');
	};
	self.Ok = function () {
		if (self.OnOk != null)
			if (!self.OnOk())
				return;

		self.Result = true;
		ConnectCMS.MainViewModel.ShowOverlay(false);
	};
	self.Open = function (popupType) {
		if (self.OnOpen != null)
			if (!self.OnOpen())
				return;

		if (ConnectCMS.MainViewModel.PopupViewModel() != self) {
			if (content != null) {
				ConnectCMS.MainViewModel.PopupViewModel(self);
				if (popupType == null || popupType == '' || popupType == 'overlay') {
					$('#popup-window > .popup-content').html(content);
					ko.applyBindings(self.ContentViewModel, $('#popup-window > .popup-content')[0].firstElementChild);
					ConnectCMS.MainViewModel.ShowOverlay(true);
				} else if (popupType == 'cover') {
					$('#cover-window > .popup-content').html(content);
					ko.applyBindings(self.ContentViewModel, $('#cover-window > .popup-content')[0].firstElementChild);
					ConnectCMS.MainViewModel.ShowCoverOverlay(true);
				}
			} else {
				$.ajax({
					url: self.ContentUrl,
					data: JSON.stringify(self.ContentData),
					headers: headers,
					contentType: 'application/json',
					type: "POST",
					success: function (result, textStatus, jqXHR) {
						ConnectCMS.MainViewModel.PopupViewModel(self);
						if (popupType == null || popupType == '' || popupType == 'overlay') {
							$('#popup-window > .popup-content').html(result);
							ko.applyBindings(self.ContentViewModel, $('#popup-window > .popup-content')[0].firstElementChild);
							ConnectCMS.MainViewModel.ShowOverlay(true);
						} else if (popupType == 'cover') {
							$('#cover-window > .popup-content').html(result);
							ko.applyBindings(self.ContentViewModel, $('#cover-window > .popup-content')[0].firstElementChild);
							ConnectCMS.MainViewModel.ShowCoverOverlay(true);
						}
					},
					error: function (jqXHR, textStatus, errorThrown) {
						alert(self.ContentData.body);
					}
				});
			}
		} else {
			if (popupType == null || popupType == '' || popupType == 'overlay') {
				ConnectCMS.MainViewModel.ShowOverlay(true);
			} else if (popupType == 'cover') {
				ConnectCMS.MainViewModel.ShowCoverOverlay(true);
			}
		}
	};
};