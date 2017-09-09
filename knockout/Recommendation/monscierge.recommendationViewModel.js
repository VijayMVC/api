function RecommendationViewModel() {
	var self = this;

	self.BingMapCredentials = ko.observable(null);
	self.BlacklistTabIndex = 2;
	self.CategoriesExecuted = ko.observable(false);
	self.CategoriesExecuting = ko.observable(false);
	self.CategoriesOnDevice = ko.observableArray(null);
	self.CategoryTabIndex = 4;
	self.ClickedBlacklistedTab = ko.observable(false);
	self.ClickedCategoriesTab = ko.observable(false);
	self.ClickedEnterprise = ko.observable(false);
	self.ClickedRecommendedTab = ko.observable(false);
	self.ClickedSearchTab = ko.observable(false);
	self.ErrorVisible = ko.observable(false);
	self.Hotel = ko.observable(null);
	self.LoggedInUserId = ko.observable(null);
	self.NavigationElement = "#recommendationNavigationDiv";
	self.RecommendTabIndex = 1;
	self.SearchTabIndex = 0;
	self.SelectedContentIndex = ko.observable(0);
	self.SelectedTabIndex = ko.observable(0);
	self.TipTabIndex = 3;

	self.ErrorMessageViewModel = new ErrorMessageViewModel(ConnectCMS.Globalization.AnErrorHasOccured, self.ErrorVisible);

	var recommendationBlacklistViewModel = ko.observable(null);
	self.RecommendationBlacklistViewModel = ko.computed(function () {
		if (recommendationBlacklistViewModel() == null && self.ClickedBlacklistedTab())
			recommendationBlacklistViewModel(new RecommendationBlacklistViewModel(self));

		return recommendationBlacklistViewModel();
	});
	var recommendationCategoryViewModel = ko.observable(null);
	self.RecommendationCategoryViewModel = ko.computed(function () {
		if (recommendationCategoryViewModel() == null && self.ClickedCategoriesTab())
			recommendationCategoryViewModel(new RecommendationCategoryViewModel(self));

		return recommendationCategoryViewModel();
	});
	var recommendationEnterpriseViewModel = ko.observable(null);
	self.RecommendationEnterpriseViewModel = ko.computed(function () {
		if (recommendationEnterpriseViewModel() == null && self.ClickedEnterprise())
			recommendationEnterpriseViewModel(new RecommendationEnterpriseViewModel(self));

		return recommendationEnterpriseViewModel();
	});
	var recommendationRecommendViewModel = ko.observable(null);
	self.RecommendationRecommendViewModel = ko.computed(function () {
		if (recommendationRecommendViewModel() == null && self.ClickedRecommendedTab())
			recommendationRecommendViewModel(new RecommendationRecommendViewModel(self));

		return recommendationRecommendViewModel();
	});
	var recommendationSearchViewModel = ko.observable(null);
	self.RecommendationSearchViewModel = ko.computed(function () {
		if (recommendationSearchViewModel() == null && self.ClickedSearchTab())
			recommendationSearchViewModel(new RecommendationSearchViewModel(self));

		return recommendationSearchViewModel();
	});
	self.RecommendationTipViewModel = ko.observable(null);

	self.ClickBlacklistedTab = function () {
		self.SelectedTabIndex(self.BlacklistTabIndex);
		self.ClickedBlacklistedTab(true);

		var recommendationBlacklistViewModel = self.RecommendationBlacklistViewModel();

		if (recommendationBlacklistViewModel != null) {
			recommendationBlacklistViewModel.Enterprises(null);
			recommendationBlacklistViewModel.Executed(false);
		}

		return true;
	};
	self.ClickCategoriesTab = function () {
		self.SelectedTabIndex(self.CategoryTabIndex);
		self.ClickedCategoriesTab(true);

		return true;
	};
	self.ClickRecommendedTab = function () {
		self.SelectedTabIndex(self.RecommendTabIndex);
		self.ClickedRecommendedTab(true);

		var recommendationRecommendViewModel = self.RecommendationRecommendViewModel();

		if (recommendationRecommendViewModel != null) {
			recommendationRecommendViewModel.Enterprises(null);
			recommendationRecommendViewModel.Executed(false);
		}

		return true;
	};
	self.ClickSearchTab = function () {
		self.SelectedTabIndex(self.SearchTabIndex);
		self.ClickedSearchTab(true);

		var recommendationSearchViewModel = self.RecommendationSearchViewModel();

		if (recommendationSearchViewModel != null) {
			recommendationSearchViewModel.Enterprises([]);
			recommendationSearchViewModel.Executed(false);
		}

		return true;
	};
	self.GetCategoriesOnDevice = function () {
		self.CategoriesExecuted(false);
		self.CategoriesExecuting(true);

		$.ajax({
			url: "/ConnectCMS/Category/GetCategoriesOnDevice",
			data: {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
			},
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				var categoriesOnDevice = $.map(result, function (categoryModel) {
					return new CategoryViewModel(categoryModel);
				});

				if (!ConnectCMS.IsNullOrEmpty(categoriesOnDevice)) {
					ConnectCMS.Order.SetOrder(categoriesOnDevice, SortCategoryByOrder, function (categoryViewModel, order) {
						categoryViewModel.Order(order);
					});

					ko.utils.arrayForEach(categoriesOnDevice, function (categoryViewModel) {
						categoryViewModel.commitData(false);
					});

					categoriesOnDevice = categoriesOnDevice.sort(SortCategoryByName);
				}

				self.CategoriesOnDevice(categoriesOnDevice);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				self.CategoriesExecuted(true);
				self.CategoriesExecuting(false);
			}
		});
	};
	self.GetHotelFromDevice = function () {
		$.ajax({
			url: "/ConnectCMS/Utility/GetHotelFromDevice",
			data: {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
			},
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				self.Hotel(new ConnectCMS.Hotels.HotelViewModel(result));
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			}
		});
	};
	self.GetLoggedInUserId = function () {
		$.ajax({
			url: "/ConnectCMS/Utility/GetLoggedInUserId",
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				self.LoggedInUserId(result);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			}
		});
	};
	self.GetRecommendationContactUserSettings = function () {
		$.ajax({
			url: "/ConnectCMS/Recommendation/GetRecommendationContactUserSettings",
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				if (result == null)
					return;

				ko.utils.arrayForEach(result, function (contactUserSetting) {
					switch (contactUserSetting.Key) {
						case self.RecommendedHelpReorderVisibleSettingKey:
							self.RecommendedHelpReorderVisible(contactUserSetting.Value);

							break;
						case self.SearchHelpAddVisibleSettingKey:
							self.SearchHelpAddVisible(contactUserSetting.Value);

							break;
						case self.SearchHelpSearchVisibleSettingKey:
							self.SearchHelpSearchVisible(contactUserSetting.Value);

							break;
					}
				});
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			}
		});
	};
	self.GetMapText = function (selector) {
		if (ko.utils.unwrapObservable(selector))
			return ConnectCMS.Globalization.Hide;
		else
			return ConnectCMS.Globalization.Map;
	};
	self.GetMapViewModel = function (visibilitySelector, resultsSelector, bingResultsSelector) {
		var result = new MapViewModel(self.BingMapCredentials);

		result.HotelPinClass = "bingMapHotelPin";
		result.HoverClass = "hover";
		result.LocationPinClass = "bingMapPin";
		result.SelectedClass = "selected";

		var hotelViewModel = self.Hotel();

		if (hotelViewModel != null) {
			result.AddHotelPin(hotelViewModel);
			result.SetCenterRadius(hotelViewModel.HotelDetail().RadiusInKilometers());

			var locationViewModel = hotelViewModel.HotelDetail().Location();

			if (locationViewModel != null) {
				result.SetCenter(hotelViewModel.HotelDetail().Location().Latitude(), hotelViewModel.HotelDetail().Location().Longitude());
			}
		}

		if (resultsSelector != null) {
			var results = ko.utils.unwrapObservable(resultsSelector);

			if (results != null)
				ko.utils.arrayForEach(results, function (enterpriseViewModel) {
					ko.utils.arrayForEach(enterpriseViewModel.EnterpriseLocations(), function (enterpriseLocationViewModel) {
						result.AddLocationPin(enterpriseLocationViewModel);

						if (enterpriseLocationViewModel.Selected())
							enterpriseLocationViewModel.CenterMap();
					});
				});
		}

		if (bingResultsSelector != null) {
			var bingResults = bingResultsSelector();

			if (bingResults != null)
				ko.utils.arrayForEach(bingResults, function (enterpriseViewModel) {
					ko.utils.arrayForEach(enterpriseViewModel.EnterpriseLocations(), function (enterpriseLocationViewModel) {
						result.AddLocationPin(enterpriseLocationViewModel);

						if (enterpriseLocationViewModel.Selected())
							enterpriseLocationViewModel.CenterMap();
					});
				});
		}

		return result;
	};
	self.OnNavigateEnterprise = function (enterpriseViewModel, enterpriseLocationViewModel, enterpriseNavigationType) {
		self.ClickedEnterprise(true);

		var recommendationEnterpriseViewModel = self.RecommendationEnterpriseViewModel();

		if (recommendationEnterpriseViewModel != null) {
			recommendationEnterpriseViewModel.SetEnterpriseForEdit(enterpriseViewModel, enterpriseLocationViewModel, enterpriseNavigationType);

			$.ajax({
				url: "/ConnectCMS/Recommendation/Enterprise",
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					NavigateOpen(recommendationEnterpriseViewModel, self.SelectedContentIndex, self.NavigationElement, result);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					self.ErrorVisible(true);
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});
		}
	};
	self.SetMapIds = function (enterpriseViewModels, startingMapId) {
		if (!ConnectCMS.IsNullOrEmpty(enterpriseViewModels)) {
			if (startingMapId == null)
				startingMapId = 1;

			ko.utils.arrayForEach(enterpriseViewModels, function (enterpriseViewModel) {
				var enterpriseLocationViewModels = enterpriseViewModel.EnterpriseLocations();

				if (!ConnectCMS.IsNullOrEmpty(enterpriseLocationViewModels))
					ko.utils.arrayForEach(enterpriseLocationViewModels, function (enterpriseLocationViewModel) {
						enterpriseLocationViewModel.MapId(startingMapId);

						startingMapId++;
					});
			});
		}
	};

	self.Tabs = [
		new TabViewModel(ConnectCMS.Globalization.Search, true, true, self.ClickSearchTab, self.SearchTabIndex, self.SelectedTabIndex),
		new TabViewModel(ConnectCMS.Globalization.Recommended, true, true, self.ClickRecommendedTab, self.RecommendTabIndex, self.SelectedTabIndex),
		new TabViewModel(ConnectCMS.Globalization.Blacklisted, true, true, self.ClickBlacklistedTab, self.BlacklistTabIndex, self.SelectedTabIndex),
		new TabViewModel(ConnectCMS.Globalization.Tips, true, true, null, self.TipTabIndex, self.SelectedTabIndex),
		new TabViewModel(ConnectCMS.Globalization.Categories, true, true, self.ClickCategoriesTab, self.CategoryTabIndex, self.SelectedTabIndex)
	];

	ConnectCMS.GetBingMapCredentials(self.BingMapCredentials);
	self.ClickSearchTab();
	self.GetRecommendationContactUserSettings();
	self.GetCategoriesOnDevice();
	self.GetHotelFromDevice();
	self.GetLoggedInUserId();
	self.RecommendationTipViewModel(new RecommendationTipViewModel(self));
};