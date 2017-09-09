function RecommendationCategoryManageViewModel(recommendationCategoryViewModel) {
	var self = this;

	self.RecommendationCategoryViewModel = recommendationCategoryViewModel;

	self.Categories = ko.observableArray(null);
	self.ErrorVisible = ko.observable(false);
	self.Executed = ko.observable(false);
	self.Executing = ko.observable(false);

	self.ErrorMessageViewModel = new ErrorMessageViewModel(ConnectCMS.Globalization.AnErrorHasOccured, self.ErrorVisible);
	self.NoDataVisible = ko.computed(function () {
		var result = true;

		if (!ConnectCMS.IsNullOrEmpty(self.Categories))
			result = false;

		return result;
	});
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.FetchingCategories, self.Executing);
	self.ToolBarEnabled = ko.computed(function () {
		return !self.Executing();
	});

	self.NoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoCategoriesWereFound, null, self.NoDataVisible);

	self.ClickBack = function () {
		self.OnClickBack(true);
	};
	self.ClickUpdate = function () {
		var categories = self.Categories();

		if (!ConnectCMS.IsNullOrEmpty(categories)) {
			ConnectCMS.ShowPleaseWait();

			var addCategoryIds;
			var categoryId;
			var removeCategoryIds;

			ko.utils.arrayForEach(categories, function (categoryManageViewModel) {
				if (categoryManageViewModel.HasSelfChanged()) {
					categoryManageViewModel.commitData(true);

					categoryId = categoryManageViewModel.PKID;

					if (categoryManageViewModel.OnDevice()) {
						if (addCategoryIds == null)
							addCategoryIds = new Array();

						addCategoryIds.push(categoryId);
					}
					else {
						if (removeCategoryIds == null)
							removeCategoryIds = new Array();

						removeCategoryIds.push(categoryId);
					}
				}
			});

			if (addCategoryIds != null || removeCategoryIds != null) {
				var data = {
					addCategoryIds: addCategoryIds,
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
					removeCategoryIds: removeCategoryIds
				};

				$.ajax({
					url: "/ConnectCMS/Category/UpdateCategoriesManage",
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
			} else {
				ConnectCMS.HidePleaseWait();
				self.OnClickBack(false);
			}
		} else
			self.OnClickBack(false);
	};
	self.GetCategories = function () {
		self.Executed(false);
		self.Executing(true);

		$.ajax({
			url: "/ConnectCMS/Category/GetCategories",
			data: {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
			},
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				self.Categories.removeAll();

				var categories = $.map(result, function (categoryModel) {
					return new CategoryManageViewModel(new CategoryViewModel(categoryModel));
				});

				if (!ConnectCMS.IsNullOrEmpty(categories))
					categories = categories.sort(SortCategoryByName);

				self.Categories(categories);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				self.Executed(true);
				self.Executing(false);
			}
		});
	};
	self.OnClickBack = function (revertData) {
		NavigateDispose(self.RecommendationCategoryViewModel.RecommendationViewModel.SelectedContentIndex, self.RecommendationCategoryViewModel.RecommendationViewModel.NavigationElement);
		self.ErrorVisible(false);

		if (revertData != null && revertData == true) {
			var categories = self.Categories();

			if (!ConnectCMS.IsNullOrEmpty(categories))
				ko.utils.arrayForEach(categories, function (categoryManageViewModel) {
					if (categoryManageViewModel.HasSelfChanged())
						categoryManageViewModel.revertData(false);
				});
		}
	};

	self.GetCategories();
};