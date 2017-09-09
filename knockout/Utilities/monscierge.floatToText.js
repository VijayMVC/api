ko.bindingHandlers.floatToText = {
	init: function (element, valueAccessor) {
		// Prevent binding on the dynamically-injected text node (as developers are unlikely to expect that, and it has security implications).
		// It should also make things faster, as we no longer have to consider whether the text node might be bindable.
		return {
			controlsDescendantBindings: true
		};
	},
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());
		var text = ko.utils.unwrapObservable(value.text);
		var decimals = ko.utils.unwrapObservable(value.decimals);
		var unit = ko.utils.unwrapObservable(value.unit);

		if ((text === null) || (text === undefined))
			text = '0';

		if ((decimals === null) || (decimals === undefined))
			decimals = 0;

		if (unit === null || unit === undefined)
			unit = '';

		var f = parseFloat(text);

		var val = f.toFixed(decimals); //Math.round(f * Math.pow(10, decimals)) / Math.pow(10, decimals);

		// We need there to be exactly one child: a text node.
		// If there are no children, more than one, or if it's not a text node,
		// we'll clear everything and create a single text node.
		var innerTextNode = ko.virtualElements.firstChild(element);
		if (!innerTextNode || innerTextNode.nodeType != 3 || ko.virtualElements.nextSibling(innerTextNode)) {
			ko.virtualElements.setDomNodeChildren(element, [document.createTextNode(val + unit)]);
		} else {
			innerTextNode.data = value;
		}

		ko.utils.forceRefresh(element);
	}
};