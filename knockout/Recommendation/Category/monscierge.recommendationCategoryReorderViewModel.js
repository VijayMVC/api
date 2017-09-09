function RecommendationCategoryReorderViewModel(recommendationCategoryViewModel) {
	var self = this;

	self.RecommendationCategoryViewModel = recommendationCategoryViewModel;

	self.Categories = ko.observableArray(null);
	self.ErrorVisible = ko.observable(false);

	self.CategoriesOnDevice = ko.computed(function () {
		var categoriesOnDevice = self.RecommendationCategoryViewModel.RecommendationViewModel.CategoriesOnDevice();

		if (ConnectCMS.IsNullOrEmpty(self.Categories) && !ConnectCMS.IsNullOrEmpty(categoriesOnDevice)) {
			var categories;

			ko.utils.arrayForEach(categoriesOnDevice, function (categoryViewModel) {
				if (categories == null)
					categories = new Array();

				categories.push(new CategoryReorderViewModel(categoryViewModel, self.Categories));
			});

			if (!ConnectCMS.IsNullOrEmpty(categories))
				categories.sort(SortCategoryByOrder);

			self.Categories(categories);
		}
	});
	self.ErrorMessageViewModel = new ErrorMessageViewModel(ConnectCMS.Globalization.AnErrorHasOccured, self.ErrorVisible);
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.FetchingCategories, self.RecommendationCategoryViewModel.RecommendationViewModel.CategoriesExecuting);
	self.ToolBarEnabled = ko.computed(function () {
		return !self.RecommendationCategoryViewModel.RecommendationViewModel.CategoriesExecuting();
	});

	self.ClickBack = function () {
		self.OnClickBack(true);
	};
	self.ClickUpdate = function () {
		var categories = self.Categories();

		if (!ConnectCMS.IsNullOrEmpty(categories)) {
			ConnectCMS.ShowPleaseWait();

			var categoriesReorder;

			ko.utils.arrayForEach(categories, function (categoryReorderViewModel) {
				if (categoryReorderViewModel.HasSelfChanged()) {
					categoryReorderViewModel.commitData(false);

					if (categoriesReorder == null)
						categoriesReorder = new Array();

					categoriesReorder.push(categoryReorderViewModel.getData());
				}
			});

			if (!ConnectCMS.IsNullOrEmpty(categoriesReorder)) {
				var data = {
					categories: categoriesReorder,
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
				};

				$.ajax({
					url: "/ConnectCMS/Category/UpdateCategoriesReorder",
					type: "POST",
					contentType: 'application/json; charset=utf-8;',
					dataType: 'json',
					data: JSON.stringify(data),
					success: function (result, textStatus, jqXHR) {
						var recommendationViewModel = self.RecommendationCategoryViewModel.RecommendationViewModel;

						if (recommendationViewModel != null) {
							recommendationViewModel.CategoriesExecuted(false);
							recommendationViewModel.GetCategoriesOnDevice();
						}

						self.OnClickBack(false);
					},
					error: function (jqXHR, textStatus, errorThrown) {
						self.ErrorVisible(true);
						ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
					},
					complete: function (jqXHR, textStatus) {
						ConnectCMS.HidePleaseWait();
					}
				});
			}
			else
				ConnectCMS.HidePleaseWait();
		}
	};
	self.OnClickBack = function (revertData) {
		NavigateDispose(self.RecommendationCategoryViewModel.RecommendationViewModel.SelectedContentIndex, self.RecommendationCategoryViewModel.RecommendationViewModel.NavigationElement);
		self.ErrorVisible(false);

		if (revertData != null && revertData == true) {
			var categories = self.Categories();

			if (!ConnectCMS.IsNullOrEmpty(categories))
				ko.utils.arrayForEach(categories, function (categoryReorderViewModel) {
					categoryReorderViewModel.revertData(false);
				});
		}
	};
	self.OnSortUpdate = function (event, ui, viewModel, index) {
		if (index != null && viewModel != null) {
			var order = viewModel.Order();

			if (order != null) {
				index++;

				var position = index - order;

				viewModel.OnMove(index, position);
			}
		}
	};

	self.SortableViewModel = new ConnectCMS.Widgets.Sortable.SortableViewModel(self.Categories, false, null, self.OnSortUpdate);
};