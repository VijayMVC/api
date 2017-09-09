function PredictiveInputViewModel(inputId, label, enabled, inputClass, predictionStartLength, getItems, getItemsExecuting, items) {
	var self = this;

	self.Enabled = enabled;
	self.Focused = ko.observable(false);
	self.GetItems = getItems;
	self.GetItemsExecuting = getItemsExecuting;
	self.Hovered = ko.observable(false);
	self.InputClass = inputClass != null ? inputClass : "";
	self.InputId = inputId;
	self.Items = items;
	self.Label = label;
	self.PredictionStartLength = predictionStartLength;
	self.PredictiveStart = "";
	self.SelectedItem = ko.observable(null);

	var text = ko.observable("");
	self.Text = ko.computed({
		read: function () {
			return text();
		},
		write: function (value) {
			text(value);

			//var selectedItem = self.SelectedItem;

			//selectedItem(null);

			var predictionStartLength = self.PredictionStartLength;
			var predictiveStart = self.PredictiveStart;

			if (text().length >= predictionStartLength) {
				var valueTextSubstring = text().substring(0, predictionStartLength);

				if (ConnectCMS.Strings.IsNullOrWhitespace(predictiveStart) || predictiveStart != valueTextSubstring) {
					predictiveStart = valueTextSubstring;

					if (self.GetItems != null)
						self.GetItems();
				}
			}
			else
				predictiveStart = "";

			//if (selectedItem() == null) {
			//	var firstItem = ko.utils.arrayFirst(self.Items(), function (item) {
			//		return text() == item.Label;
			//	});

			//	if (firstItem != null) {
			//		text(firstItem.Label());
			//		selectedItem(firstItem);
			//	}
			//}
		}
	});

	self.FilteredItems = ko.computed(function () {
		var result = self.Items();

		if (result != null && result.length > 0) {
			var formattedText = self.Text().toLowerCase();

			if (formattedText.length >= self.PredictionStartLength) {
				if (!ConnectCMS.Strings.IsNullOrWhitespace(formattedText))
					result = ko.utils.arrayFilter(result, function (predictiveItemViewModel) {
						return predictiveItemViewModel.Label.toLowerCase().indexOf(formattedText) != -1;
					});

				self.SelectedItem(ko.utils.arrayFirst(result, function (predictiveItemViewModel) {
					return true;
				}));
			}
		}

		return result;
	});
	self.PredictedItemsVisible = ko.computed(function () {
		return (self.Focused() || self.Hovered()) && self.Text().length >= self.PredictionStartLength;
	});
	self.ScrollToItem = function (elementId) {
		if (!ConnectCMS.Strings.IsNullOrWhitespace(elementId)) {
			var element = $(elementId).first();

			if (element != null) {
				var parent = element.parent();

				if (parent != null)
					parent.scrollTop(parent.scrollTop() + element.position().top - (parent.height() / 2) + (element.height() / 2));
			}
		}
	};

	self.KeyDown = function (data, event) {
		var filteredItems = self.FilteredItems();
		var predictedItemsVisible = self.PredictedItemsVisible();
		var selectedItem = self.SelectedItem;

		if (predictedItemsVisible && selectedItem() != null) {
			if (event.keyCode == ConnectCMS.KeyCodes.TAB || event.keyCode == ConnectCMS.KeyCodes.ENTER) {
				self.Text(selectedItem().Label);
				self.Focused(false);
				self.Hovered(false);

				return false;
			}
			else if (event.keyCode == ConnectCMS.KeyCodes.UP || event.keyCode == ConnectCMS.KeyCodes.DOWN) {
				var index = filteredItems.indexOf(selectedItem());

				if (event.keyCode == ConnectCMS.KeyCodes.UP) {
					if (index == 0)
						return false;

					index--;
				}
				else if (event.keyCode == ConnectCMS.KeyCodes.DOWN) {
					if (index == filteredItems.length - 1)
						return false;

					index++;
				}

				selectedItem(filteredItems[index]);
				self.ScrollToItem("#" + selectedItem().ElementId());

				return false;
			}
			else
				return true;
		}
		else
			return true;
	};
};

//self.PredictHotels = ko.computed(function () {
//	// FH: Hack to get computed to re-evaluate.
//	var predictedHotels = self.PredictedHotels();

//	if (!self.HotelFocused() && !self.HotelHovered() && self.SelectedHotel() != null)
//		return null;

//	return self.Predict(self.Hotel, self.HotelPredictiveLength, self.PredictedHotels, self.SelectedPredictedHotel);
//});

//self.Predict = function (value, valuePredictiveLength, predictedValues, selectedPreictedValue) {
//	if (value().length < valuePredictiveLength)
//		return null;

//	var result = ko.utils.arrayFilter(predictedValues(), function (item) {
//		return item.Name().toLowerCase().indexOf(value().toLowerCase()) != -1;
//	});

//	// FH: Check if selected predicted value is in filtered result, if no, then select the first.
//	//if (selectedPreictedValue() == null || ko.utils.arrayIndexOf(result, selectedPreictedValue()) == -1) {
//	selectedPreictedValue(ko.utils.arrayFirst(result, function () {
//		return true;
//	}));
//	//}

//	return result;
//};
//self.ScrollToItem = function (item) {
//	var parent = $(item).parent();

//	parent.scrollTop(parent.scrollTop() + $(item).position().top - (parent.height() / 2) + ($(item).height() / 2));
//};