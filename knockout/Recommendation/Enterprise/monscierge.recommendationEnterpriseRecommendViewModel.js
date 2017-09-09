function RecommendationEnterpriseRecommendViewModel(recommendationEnterpriseViewModel) {
	var self = this;

	self.RecommendationEnterpriseViewModel = recommendationEnterpriseViewModel;

	self.BlacklistedVisible = ko.observable(true);
	self.Categories = ko.observableArray(null);
	self.Enterprise = ko.observable(null);
	self.Executed = ko.observable(false);
	self.Executing = ko.observable(false);

	self.BlacklistedInformationMessageViewModel = new InformationMessageViewModel(ConnectCMS.Globalization.ThisIsCurrentlyBlacklistedMakingChangesToTheRecommendationWillRemoveTheBlacklisting, self.BlacklistedVisible);
	self.CategoriesOnDevice = ko.computed(function () {
		var categoriesOnDevice = self.RecommendationEnterpriseViewModel.RecommendationViewModel.CategoriesOnDevice();

		if (ConnectCMS.IsNullOrEmpty(self.Categories) && !ConnectCMS.IsNullOrEmpty(categoriesOnDevice)) {
			self.Categories.removeAll();

			ko.utils.arrayForEach(categoriesOnDevice, function (categoryViewModel) {
				self.Categories.push(new CategoryEnterpriseViewModel(categoryViewModel));
			});
		}
	});
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.FetchingRecommendations, self.Executing);
	self.ToolBarEnabled = ko.computed(function () {
		return !self.Executing();
	});

	self.ClickUpdate = function () {
		var recommendationEnterpriseViewModel = self.RecommendationEnterpriseViewModel;

		if (recommendationEnterpriseViewModel != null) {
			var categories = self.Categories();
			var enterpriseViewModel = recommendationEnterpriseViewModel.Enterprise();

			if (!ConnectCMS.IsNullOrEmpty(categories) && enterpriseViewModel != null) {
				ConnectCMS.ShowPleaseWait();

				var addedCategoryIds = new Array();
				var removedCategoryIds = new Array();

				ko.utils.arrayForEach(categories, function (categoryViewModel) {
					if (categoryViewModel.HasSelfChanged()) {
						categoryViewModel.commitData(false);

						categoryId = categoryViewModel.PKID;

						if (categoryViewModel.OnDevice())
							addedCategoryIds.push(categoryId);
						else
							removedCategoryIds.push(categoryId);
					}
				});

				if (!ConnectCMS.IsNullOrEmpty(addedCategoryIds) || !ConnectCMS.IsNullOrEmpty(removedCategoryIds)) {
					var enterpriseLocationId = null;

					if (recommendationEnterpriseViewModel.NavigationForEnterpriseLocation()) {
						var enterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

						if (enterpriseLocationViewModel != null)
							enterpriseLocationId = enterpriseLocationViewModel.PKID;
					}

					var data = {
						addedCategoryIds: addedCategoryIds,
						deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
						enterpriseId: enterpriseViewModel.PKID(),
						enterpriseLocationId: enterpriseLocationId,
						removedCategoryIds: removedCategoryIds
					};

					$.ajax({
						url: "/ConnectCMS/Enterprise/UpdateEnterpriseRecommendation",
						type: "POST",
						contentType: 'application/json; charset=utf-8;',
						dataType: 'json',
						data: JSON.stringify(data),
						success: function (result, textStatus, jqXHR) {
							self.BlacklistedVisible(false);

							var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

							if (enterpriseViewModel != null)
								enterpriseViewModel.Blacklisted(false);
						},
						error: function (jqXHR, textStatus, errorThrown) {
							self.RecommendationEnterpriseViewModel.ErrorVisible(true);
							ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
						},
						complete: function (jqXHR, textStatus) {
							ConnectCMS.HidePleaseWait();
						}
					});
				} else
					ConnectCMS.HidePleaseWait();
			}
		}
	};
	self.GetEnterpriseRecommendationForEdit = function () {
		var enterprise = self.RecommendationEnterpriseViewModel.Enterprise();

		if (enterprise != null) {
			self.Executed(false);
			self.Executing(true);

			var enterpriseLocationId = null;

			if (self.RecommendationEnterpriseViewModel.NavigationForEnterpriseLocation() && enterprise.EnterpriseLocationsLength() == 1)
				enterpriseLocationId = enterprise.FirstEnterpriseLocation().PKID;

			$.ajax({
				url: "/ConnectCMS/Enterprise/GetEnterpriseRecommendationForEdit",
				data: {
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
					enterpriseId: enterprise.PKID,
					enterpriseLocationId: enterpriseLocationId
				},
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					var categories = self.Categories();
					var enterprise1 = new ConnectCMS.Enterprises.EnterpriseViewModel(result, null);

					if (!ConnectCMS.IsNullOrEmpty(categories)) {
						var recommendedCategories;

						ko.utils.arrayForEach(categories, function (categoryEnterpriseViewModel) {
							categoryEnterpriseViewModel.OnDevice(false);

							recommendedCategories = enterprise1.RecommendedCategories();

							if (!ConnectCMS.IsNullOrEmpty(recommendedCategories))
								ko.utils.arrayForEach(recommendedCategories, function (orderModel) {
									if (categoryEnterpriseViewModel.PKID == orderModel.Key)
										categoryEnterpriseViewModel.OnDevice(true);
								});
						});
					}
				},
				error: function (jqXHR, textStatus, errorThrown) {
					self.RecommendationEnterpriseViewModel.ErrorVisible(true);
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				},
				complete: function (jqXHR, textStatus) {
					self.Executed(true);
					self.Executing(false);
				}
			});
		}
	};

	var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

	if (enterpriseViewModel != null)
		self.BlacklistedVisible(enterpriseViewModel.Blacklisted());

	self.GetEnterpriseRecommendationForEdit();
};