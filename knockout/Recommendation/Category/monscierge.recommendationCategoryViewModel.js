function RecommendationCategoryViewModel(recommendationViewModel) {
	var self = this;

	self.RecommendationViewModel = recommendationViewModel;

	self.Categories = ko.observableArray(null);
	self.RecommendationCategoryManageViewModel = ko.observable(new RecommendationCategoryManageViewModel(self));
	self.RecommendationCategoryReorderViewModel = ko.observable(new RecommendationCategoryReorderViewModel(self));
	self.SelectedCategoryViewModel = ko.observable(null);

	self.NoDataVisible = ko.computed(function () {
		var result = true;

		if (!ConnectCMS.IsNullOrEmpty(self.RecommendationViewModel.CategoriesOnDevice))
			result = false;

		return result;
	});
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.FetchingCategories, self.RecommendationViewModel.CategoriesExecuting);
	self.RecommendationCategoryEditViewModel = ko.computed(function () {
		var result = null;

		if (self.SelectedCategoryViewModel() != null)
			result = new RecommendationCategoryEditViewModel(self);

		return result;
	});
	self.NoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoCategoriesWereFound, null, self.NoDataVisible);

	self.ClickManage = function () {
		ConnectCMS.ShowPleaseWait();

		$.ajax({
			url: "/ConnectCMS/Category/Manage",
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				NavigateOpen(self.RecommendationCategoryManageViewModel, self.RecommendationViewModel.SelectedContentIndex, self.RecommendationViewModel.NavigationElement, result);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.RecommendationViewModel.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				ConnectCMS.HidePleaseWait();
			}
		});
	};
	self.ClickReorder = function () {
		ConnectCMS.ShowPleaseWait();

		$.ajax({
			url: "/ConnectCMS/Category/Reorder",
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				NavigateOpen(self.RecommendationCategoryReorderViewModel, self.RecommendationViewModel.SelectedContentIndex, self.RecommendationViewModel.NavigationElement, result);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.RecommendationViewModel.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				ConnectCMS.HidePleaseWait();
			}
		});
	};
	self.OnClickEdit = function (categoryCategoryViewModel) {
		if (categoryCategoryViewModel != null) {
			ConnectCMS.ShowPleaseWait();

			self.SelectedCategoryViewModel(categoryCategoryViewModel.CategoryViewModel);

			$.ajax({
				url: "/ConnectCMS/Category/Edit",
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					NavigateOpen(self.RecommendationCategoryEditViewModel, self.RecommendationViewModel.SelectedContentIndex, self.RecommendationViewModel.NavigationElement, result);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					self.RecommendationViewModel.ErrorVisible(true);
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				},
				complete: function (jqXHR, textStatus) {
					ConnectCMS.HidePleaseWait();
				}
			});
		}
	};
	self.OnDeleteSuccess = function () {
		self.RecommendationViewModel.GetCategoriesOnDevice();
	};

	self.CategoriesOnDevice = ko.computed(function () {
		var categoriesOnDevice = self.RecommendationViewModel.CategoriesOnDevice();

		if (!ConnectCMS.IsNullOrEmpty(categoriesOnDevice)) {
			var categories;

			ko.utils.arrayForEach(categoriesOnDevice, function (categoryViewModel) {
				if (categories == null)
					categories = new Array();

				categories.push(new CategoryCategoryViewModel(categoryViewModel, self.Categories, self.OnClickEdit, self.OnDeleteSuccess));
			});

			if (!ConnectCMS.IsNullOrEmpty(categories))
				categories.sort(SortCategoryByOrder);

			self.Categories(categories);
		}
	});
};