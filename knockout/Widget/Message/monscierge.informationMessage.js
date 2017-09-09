ko.bindingHandlers.informationMessage = {
	'init': function (element, valueAccessor) {
		var informationMessageViewModel = ko.utils.unwrapObservable(valueAccessor());

		var divElements = "";

		divElements += "<div class='message information' data-bind='visible: VisibleSelector'>";
		divElements += "<span class='flexSpacer' data-bind='text: Label'></span>";
		divElements += "<img src='/ConnectCMS/Content/monscierge/images/message-close.png' data-bind='click: Click' />";
		divElements += "</div>";

		$(element).html(divElements);

		ko.cleanNode(element.firstElementChild);
		ko.applyBindings(informationMessageViewModel, element.firstElementChild);

		return {
			'controlsDescendantBindings': true
		};
	}
};