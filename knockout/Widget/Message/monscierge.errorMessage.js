ko.bindingHandlers.errorMessage = {
	'init': function (element, valueAccessor) {
		var errorMessageViewModel = ko.utils.unwrapObservable(valueAccessor());

		var divElements = "";

		divElements += "<div class='message error' data-bind='visible: VisibleSelector'>";
		divElements += "<span class='flexSpacer' data-bind='text: Label'></span>";
		divElements += "<img src='/ConnectCMS/Content/monscierge/images/message-close.png' data-bind='click: Click' />";
		divElements += "</div>";

		$(element).html(divElements);

		ko.cleanNode(element.firstElementChild);
		ko.applyBindings(errorMessageViewModel, element.firstElementChild);

		return {
			'controlsDescendantBindings': true
		};
	}
};