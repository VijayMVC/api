function CategoryReorderViewModel(categoryViewModel, array) {
	BaseViewModel.call(this, categoryViewModel, true);

	var self = this;

	self.Array = ko.isObservable(array) ? array : ko.computed(array);
	self.CategoryViewModel = categoryViewModel;

	self.DownVisible = ko.computed(function () {
		var result = false;

		var array = self.Array();
		var order = self.Order();

		if (!ConnectCMS.IsNullOrEmpty(array) && order != null && array.length != order)
			result = true;

		return result;
	});
	self.UpVisible = ko.computed(function () {
		var result = false;

		var order = self.Order();

		if (order != null && order != 1)
			result = true;

		return result;
	});

	self.ClickDownOrUp = function (position) {
		var order = self.Order();

		if (order != null) {
			self.OnMove(order + position, position);
			self.SortArray();
		}
	};
	self.OnMove = function (orderValue, position) {
		if (index != null && position != null && position != 0) {
			var array = self.Array();
			var order = self.Order();

			if (!ConnectCMS.IsNullOrEmpty(array) && order != null) {
				var arrayViewModelOrder;
				var maximumOrder;
				var minimumOrder;

				if (position > 0) {
					maximumOrder = order + position;
					minimumOrder = order;
				}
				else if (position < 0) {
					maximumOrder = order;
					minimumOrder = order + position;
				}

				ko.utils.arrayForEach(array, function (arrayViewModel) {
					if (self.PKID != arrayViewModel.PKID) {
						arrayViewModelOrder = arrayViewModel.Order();

						if (arrayViewModelOrder != null && arrayViewModelOrder >= minimumOrder && arrayViewModelOrder <= maximumOrder)
							if (position > 0)
								arrayViewModel.Order(arrayViewModelOrder - 1);
							else if (position < 0)
								arrayViewModel.Order(arrayViewModelOrder + 1);
					}
				});

				self.Order(orderValue);
			}
		}
	};
	self.SortArray = function () {
		var array = self.Array();

		if (!ConnectCMS.IsNullOrEmpty(array)) {
			array = array.sort(SortCategoryByOrder);

			self.Array(array);
		}
	};

	self.ClickDown = function () {
		var position = 1;

		self.ClickDownOrUp(position);
	};
	self.ClickUp = function () {
		var position = -1;

		self.ClickDownOrUp(position);
	};
};