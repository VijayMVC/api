ko.bindingHandlers.helpMessage = {
	'init': function (element, valueAccessor) {
		var helpMessageViewModel = ko.utils.unwrapObservable(valueAccessor());

		var divElements = "";

		divElements += "<div class='message help' data-bind='visible: VisibleSelector'>";
		divElements += "<span class='flexSpacer' data-bind='text: Label'></span>";
		divElements += "<img src='/ConnectCMS/Content/monscierge/images/message-close.png' data-bind='click: Click' />";
		divElements += "</div>";

		$(element).html(divElements);

		ko.cleanNode(element.firstElementChild);
		ko.applyBindings(helpMessageViewModel, element.firstElementChild);

		return {
			'controlsDescendantBindings': true
		};
	}
};