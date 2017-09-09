ko.bindingHandlers.predictiveInput = {
	'init': function (element, valueAccessor) {
		var predictiveInputViewModel = ko.utils.unwrapObservable(valueAccessor());

		var html = "";
		var inputId = predictiveInputViewModel.InputId;
		var size = predictiveInputViewModel.Size;

		html += "<div>";
		html += "    <label class='vertical paddingLeftInput' for='" + inputId + "' data-bind='text: Label'></label>";
		html += "    <input class='predictive' id='" + inputId + "' type='text' data-bind='value: Text, event: { keydown: KeyDown }, css: InputClass + (GetItemsExecuting() ? \"executing\" : \"\"), enable: Enabled, hasFocus: Focused, valueUpdate: \"input\"'>";
		html += "    <img class='predictExecuting' id='" + inputId + "Img' src='Content/monscierge/images/predictive-input-executing.gif' alt='ConnectCMS.Globalization.Loading' data-bind='visible: GetItemsExecuting' />";
		html += "    <div class='predictedItems' id='" + inputId + "Div' data-bind='foreach: FilteredItems, isHover: Hovered, visible: PredictedItemsVisible'>";
		html += "        <a class='predictedItem' data-bind='text: Label, click: Click, event: { mouseover: MouseOver }, attr: { id: ElementId }, css: PredictiveInputViewModel.SelectedItem() == $data ? \"selected\" : \"\"'></a>";
		html += "    </div>";
		html += "</div>";

		$(element).html(html);

		ko.cleanNode(element.firstChild);
		ko.applyBindings(predictiveInputViewModel, element.firstChild);

		var jElementDiv = $("#" + inputId + "Div");
		var jElementImg = $("#" + inputId + "Img");
		var jElementInput = $("#" + inputId);

		var minWidth = jElementInput.outerWidth(true);
		var currentWidth = jElementDiv.outerWidth();

		jElementImg.css("left", minWidth - 40);

		if (currentWidth < minWidth)
			jElementDiv.outerWidth(minWidth);

		return {
			'controlsDescendantBindings': true
		};
	}
};