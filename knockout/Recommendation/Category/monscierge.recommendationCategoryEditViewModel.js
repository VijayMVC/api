function RecommendationCategoryEditViewModel(recommendationCategoryViewModel) {
	var self = this;

	self.RecommendationCategoryViewModel = recommendationCategoryViewModel;

	self.ErrorVisible = ko.observable(false);

	var categoryEditViewModel = ko.observable(null);
	self.CategoryEditViewModel = ko.computed(function () {
		var selectedCategoryViewModel = self.RecommendationCategoryViewModel.SelectedCategoryViewModel();

		if (categoryEditViewModel() == null && selectedCategoryViewModel != null)
			categoryEditViewModel(new CategoryEditViewModel(selectedCategoryViewModel));

		return categoryEditViewModel();
	});
	self.ErrorMessageViewModel = new ErrorMessageViewModel(ConnectCMS.Globalization.AnErrorHasOccured, self.ErrorVisible);

	self.ImageNoDataVisible = ko.computed(function () {
		var result = true;

		var categoryEditViewModel = self.CategoryEditViewModel();

		if (categoryEditViewModel != null && categoryEditViewModel.ImagesDiffer())
			result = false;

		return result;
	});

	self.ImageNoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoImageWasFound, null, self.ImageNoDataVisible);

	self.ClickAddImage = function () {
		var categoryEditViewModel = self.CategoryEditViewModel();

		if (categoryEditViewModel != null)
			ConnectCMS.ShowImageGallery({
				imageConstraints: {
					allowedFormats: ['jpg', 'png'],
					minHeight: 500,
					minWidth: 500
				},
				selection: 'single',
				currentImage: categoryEditViewModel.Image(),
				insert: self.OnInsertImage
			});
	};
	self.ClickDeleteImage = function () {
		var categoryEditViewModel = self.CategoryEditViewModel();

		if (categoryEditViewModel != null)
			categoryEditViewModel.Image(categoryEditViewModel.DefaultImage());
	};
	self.ClickUpdate = function () {
		var categoryEditViewModel = self.CategoryEditViewModel();

		if (categoryEditViewModel != null && categoryEditViewModel.HasSelfChanged()) {
			ConnectCMS.ShowPleaseWait();
			categoryEditViewModel.commitData(true);

			var data = {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
				category: categoryEditViewModel.getData()
			};

			$.ajax({
				url: "/ConnectCMS/Category/UpdateCategoryEdit",
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
		} else
			self.OnClickBack();
	};
	self.OnClickBack = function (revertData) {
		NavigateDispose(self.RecommendationCategoryViewModel.RecommendationViewModel.SelectedContentIndex, self.RecommendationCategoryViewModel.RecommendationViewModel.NavigationElement);
		self.ErrorVisible(false);
		self.RecommendationCategoryViewModel.SelectedCategoryViewModel(null);

		if (revertData != null && revertData == true) {
			var categoryEditViewModel = self.CategoryEditViewModel();

			if (categoryEditViewModel != null)
				categoryEditViewModel.revertData(false);
		}
	};
	self.OnInsertImage = function (image) {
		if (image.length > 0) {
			var categoryEditViewModel = self.CategoryEditViewModel();

			if (categoryEditViewModel != null)
				categoryEditViewModel.Image(new ImageViewModel(image[0]._Data));
		}
	};

	self.HeaderCategoryViewModel = ko.computed(function () {
		var result = null;

		var selectedCategoryViewModel = self.RecommendationCategoryViewModel.SelectedCategoryViewModel();

		if (selectedCategoryViewModel != null)
			result = new CategoryHeaderViewModel(selectedCategoryViewModel, self.OnClickBack);

		return result;
	});
};