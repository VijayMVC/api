ko.bindingHandlers.isHover = {
	init: function (element, valueAccessor) {
		var value = valueAccessor();

		var elm = $(element);

		if (elm.length > 0) {
			ko.utils.registerEventHandler(elm, "mouseover", function () {
				value(true);
			});

			ko.utils.registerEventHandler(elm, "mouseout", function () {
				value(false);
			});
		}
	}
};