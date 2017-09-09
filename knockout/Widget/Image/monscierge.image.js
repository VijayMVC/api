ko.bindingHandlers.image = {
    init: function (element, valueAccessor, allBindings, viewModel) {
        var value = ko.unwrap(valueAccessor());

        if (value != null)
            $(element).html('<img style="max-height: 300px; max-width: 300px; padding: 10px; border: 1px solid #e3e3e3;" data-bind="attr :{ src: ImageUrl }" alt="" />');
        else
            $(element).html();
    },
    update: function (element, valueAccessor) {
        var value = ko.unwrap(valueAccessor());

        if (value != null)
            $(element).html('<img style="max-height: 300px; max-width: 300px; padding: 10px; border: 1px solid #e3e3e3;" data-bind="attr :{ src: ImageUrl }" alt="" />');
        else
            $(element).html();
    }
};