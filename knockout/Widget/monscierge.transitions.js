function NavigateDispose(selectedIndex, selector) {
	var navigationDiv = $(selector);

	if (navigationDiv != null) {
		var firstNavigationDiv = navigationDiv[0];

		if (firstNavigationDiv != null) {
			var firstElementChild = firstNavigationDiv.firstElementChild;

			if (firstElementChild != null)
				ko.cleanNode(firstElementChild);
		}

		navigationDiv.html("");
	}

	selectedIndex(0);
};

function NavigateOpen(viewModel, selectedIndex, divId, html) {
	var navigationDiv = $(divId);

	navigationDiv.html(html);
	if (viewModel != null) {
		var element = navigationDiv[0].firstElementChild;
		var elementData = ko.dataFor(element);
		if (elementData == null)
			ko.applyBindings(viewModel, element);
		else {
			var elements = $(element).find('.bindingPoint');
			element = elements.length > 0 ? elements[0] : element;
			if (element != null) {
				ko.applyBindings(viewModel, element);
			}
		}
	}

	selectedIndex(1);
};

var transitionPanelGroups = [];
ko.bindingHandlers.transitionPanel = {
	init: function (element, valueAccessor) {
		var options = ko.utils.unwrapObservable(valueAccessor());
		var panelId = ko.utils.unwrapObservable(options.panelId);
		var selectedIndex = ko.utils.unwrapObservable(options.selectedIndex);
		var selector = ko.utils.unwrapObservable(options.selector);

		var panel;
		var exists = -1;

		for (var i = transitionPanelGroups.length - 1; i >= 0; i--)
			if (transitionPanelGroups[i].key == panelId) {
				exists = i;

				break;
			}

		if (exists == -1) {
			panel = {
				key: panelId,
				panels: []
			};

			transitionPanelGroups[transitionPanelGroups.length] = panel;
		} else
			panel = transitionPanelGroups[exists];

		$(element).css('position', 'relative');

		$(element).find(selector).each(function (item) {
			$($(element).find(selector)[item]).toggle(item == selectedIndex);

			panel.panels[item] = {
				element: $(element).find(selector)[item],
				isSelected: item == selectedIndex
			};
		});
	},
	update: function (element, valueAccessor) {
		var options = ko.utils.unwrapObservable(valueAccessor());
		var panelId = ko.utils.unwrapObservable(options.panelId);
		var selectedIndex = ko.utils.unwrapObservable(options.selectedIndex);

		var panel;
		var oldSelected;

		for (var i = transitionPanelGroups.length - 1; i >= 0; i--)
			if (transitionPanelGroups[i].key == panelId) {
				panel = transitionPanelGroups[i];

				break;
			}

		if (panel == null)
			return;

		for (var i = panel.panels.length - 1; i >= 0; i--)
			if (panel.panels[i].isSelected) {
				oldSelected = panel.panels[i];

				break;
			}

		var newSelected = panel.panels[selectedIndex];

		if (oldSelected != null && oldSelected == newSelected)
			return;

		if (oldSelected != null) {
			oldSelected.isSelected = false;
			$(oldSelected.element).toggle(false);
		}

		if (newSelected != null) {
			newSelected.isSelected = true;
			$(newSelected.element).toggle(true);
		}
	}
};