var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};
ConnectCMS.Globalization = ConnectCMS.Globalization || {};

ConnectCMS.Requests.OptionFieldType = {
	NUMBER: 1,
	DATETIME: 2,
	TEXT: 3
};

ConnectCMS.Requests.RequestOptionModel = function (data) {
	var self = this;

	self.id = data.Id || 0;
	self.name = data.Name || '';
	self.fieldType = data.FieldType || 0;
	self.required = data.Required || false;
	self.defaultValue = data.DefaultValue;
	self.minValue = data.MinValue || null;
	self.maxValue = data.MaxValue || null;
	self.ordinal = data.Ordinal;

	self.minAllowableDateTime = ko.computed(function () {
		var increment = 15;
		var now = moment();

		var currentMinutes = now.minute();
		var modulus = currentMinutes % increment;
		if (modulus === 0) {
			return now.set('second', 0).set('millisecond', 0).toDate();
		}

		var offset = increment - modulus;
		return now.add(offset, 'm').set('second', 0).set('millisecond', 0).toDate();
	});

	self.validationError = ko.observable('');
	self.enableValidation = ko.observable(false);

	self.fieldTypeIsNumber = ko.computed(function () {
		return self.fieldType === ConnectCMS.Requests.OptionFieldType.NUMBER;
	});

	self.fieldTypeIsDateTime = ko.computed(function () {
		return self.fieldType === ConnectCMS.Requests.OptionFieldType.DATETIME;
	});

	self.fieldTypeIsText = ko.computed(function () {
		return self.fieldType === ConnectCMS.Requests.OptionFieldType.TEXT;
	});

	self.optionValue = ko.observable(self.defaultValue || '');

	self.isValid = ko.computed(function () {
		if (self.enableValidation() === false) {
			return true;
		} else {
			self.validationError('');

			var value = self.optionValue();
			var isPopulated = (value !== null && value != '');

			// Is the field required, but not populated?
			if (self.required && !isPopulated) {
				self.validationError(ConnectCMS.Globalization.ValidationFieldRequired);
				return false;
			}

			// If the field is populated, evaluate other aspects of the value.
			if (isPopulated) {
				// Numeric field
				if (self.fieldTypeIsNumber()) {
					// Is it a number?
					if (isNaN(value)) {
						self.validationError(ConnectCMS.Globalization.ValidationNotANumber);
						return false;
					}

					var numericValue = value * 1;

					// Is it within the allowable range (if specified)
					if (self.minValue && !isNaN(self.minValue)) {
						var minValue = (self.minValue * 1);
						if (numericValue < minValue) {
							self.validationError(ConnectCMS.Globalization.ValidationValueMustBeGreaterThan + ' ' + self.minValue);
							return false;
						}
					}

					if (self.maxValue && !isNaN(self.maxValue)) {
						var maxValue = (self.maxValue * 1);
						if (numericValue > maxValue) {
							self.validationError(ConnectCMS.Globalization.ValidationValueMustBeLessThan + ' ' + self.maxValue);
							return false;
						}
					}
				} else if (self.fieldTypeIsDateTime()) {
					if (!moment(value).isValid()) {
						self.validationError(ConnectCMS.Globalization.ValidationInvalidDateTime);
						return false;
					}
				}

				// Text field
				// Nothing else to do here.
			}

			return true;
		}
	});
};