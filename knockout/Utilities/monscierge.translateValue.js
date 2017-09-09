function TranslateValue(translatedValues) {
	return ko.computed({
		read: function () {
			if (ko.utils.unwrapObservable(translatedValues) == null)
				return null;

			var result = null;

			var selectedLanguage = ConnectCMS.SelectedLanguage();

			if (selectedLanguage != null) {
				var key = selectedLanguage.Key;

				var keyValuePairModel = ko.utils.arrayFirst(ko.utils.unwrapObservable(translatedValues) , function (keyValuePairModel) {
					return keyValuePairModel.Key == key;
				});

				if (keyValuePairModel != null)
					result = keyValuePairModel.Value();
			}

			return result;
		},
		write: function (value) {
			var selectedLanguage = ConnectCMS.SelectedLanguage();

			if (selectedLanguage != null) {
				var key = selectedLanguage.Key;

				var keyValuePairModel = ko.utils.arrayFirst(ko.utils.unwrapObservable(translatedValues), function (keyValuePairModel) {
					return keyValuePairModel.Key == key;
				});

				if (keyValuePairModel != null)
					keyValuePairModel.Value(value);
			}
		}
	});
};