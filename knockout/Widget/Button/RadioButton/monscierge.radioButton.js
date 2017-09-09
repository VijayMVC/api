ko.bindingHandlers.radioButton = {
	init: function (element, valueAccessor, allBindings, viewModel) {
		var options = ko.utils.unwrapObservable(valueAccessor());
		var vm = new RadioButtonViewModel(options.text, options.selected, options.value);

		var container = $('<div class="radio-innerContainer" data-bind="click: Click, css: IsSelected() ? \'checked\':\'\'"></div>');
		var div = $('<div class="radio-replacement"></div>');
		var label = $('<label class="radio-label" data-bind="text: Text"></label>');
		$(element).addClass('radio-container');
		container.addClass('');
		container.append(div);
		container.append(label);
		$(element).append(container);
		ko.applyBindings(vm, container[0]);

		return {
			'controlsDescendantBindings': true
		};
	}
};