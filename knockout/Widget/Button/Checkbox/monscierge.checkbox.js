ko.bindingHandlers.checkbox = {
	init: function (element, valueAccessor, allBindings, viewModel) {
		var options = ko.utils.unwrapObservable(valueAccessor());
		var vm = new CheckboxViewModel(options.text, options.checked, options.indeterminate);

		var container = $('<div class="check-innerContainer" data-bind="click: Click, css: { \'checked\' : IsChecked, \'indeterminate\' : IsIndeterminate }"></div>');
		var div = $('<div class="check-replacement"></div>');
		var label = $('<label class="check-label" data-bind="text: Text"></label>');
		$(element).addClass('check-container');
		container.append(div);
		container.append(label);
		$(element).append(container);
		ko.applyBindings(vm, container[0]);

		return {
			'controlsDescendantBindings': true
		};
	},
};