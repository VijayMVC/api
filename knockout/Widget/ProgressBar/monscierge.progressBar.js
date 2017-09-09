ko.bindingHandlers.progressBar = {
	'init': function (element, valueAccessor) {
		var progressBarViewModel = ko.utils.unwrapObservable(valueAccessor());

		var divElements = "";

		divElements += "<div class='progressBar' data-bind='text: Text, visible: Visible'>";
		divElements += "</div>";

		$(element).html(divElements);

		ko.cleanNode(element.firstElementChild);
		ko.applyBindings(progressBarViewModel, element.firstElementChild);

		return {
			'controlsDescendantBindings': true
		};
	}
};