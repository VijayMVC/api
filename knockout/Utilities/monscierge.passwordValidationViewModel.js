/// <reference path="monscierge.passwordValidationViewModel.js" />
/// <reference path="~/Scripts/knockout-3.2.0.debug.js" />
function PasswordValidationViewModel(inputSelector) {
	var self = this;
	var inputElement = $(inputSelector);
	var input = ko.observable(inputElement.val());

	self.Visible = ko.observable(false);

	inputElement.off('.PasswordValidation');
	inputElement.on('input.PasswordValidation', function (event) {
		input($(event.target).val());
	});

	self.PasswordReqs = ko.observableArray(
        [
            new InputValidationViewModel({ Text: ConnectCMS.Globalization.PasswordRuleCharacterCount, IsValid: ko.computed(function () { return input().length >= 8; }) }),
            new InputValidationViewModel({ Text: ConnectCMS.Globalization.PasswordRuleUpperLowerCase, IsValid: ko.computed(function () { return input().match(/[A-Z]/) && input().match(/[a-z]/); }) }),
            new InputValidationViewModel({ Text: ConnectCMS.Globalization.PasswordRuleNumberCount, IsValid: ko.computed(function () { return input().match(/[0-9]/); }) }),
            new InputValidationViewModel({ Text: ConnectCMS.Globalization.PasswordRuleSymbolCount, IsValid: ko.computed(function () { return input().match(/[\`\~\!\@\#\$\%\^\&\*\(\)\_\+\-\=\{\}\|\[\]\:\"\;\'\<\>\?\,\.\/\\]/); }) })
        ]);

	self.IsValid = ko.computed(function () {
		var isValid = true;
		ko.utils.arrayForEach(self.PasswordReqs(), function (item) {
			if (!item.IsValid()) isValid = false;
		});
		return isValid;
	});
}

function InputMultiValidationViewModel(options) {
	var self = this;
	self.Reqs = ko.observableArray();
};

function InputValidationViewModel(options) {
	var self = this;
	self.Text = options.Text;
	self.IsValid = options.IsValid;
};