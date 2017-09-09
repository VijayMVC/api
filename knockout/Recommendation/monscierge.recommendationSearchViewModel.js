function RecommendationSearchViewModel(recommendationViewModel) {
	var self = this;

	self.RecommendationViewModel = recommendationViewModel;

	self.AddVisible = ko.observable(false);
	self.BingChanged = ko.observable(false);
	self.BingExecuted = ko.observable(false);
	self.BingExecuting = ko.observable(false);
	self.BingEnterprises = ko.observableArray();
	self.BingVisible = ko.observable(false);
	self.Enterprises = ko.observableArray();
	self.Executed = ko.observable(false);
	self.Executing = ko.observable(false);
	self.HelpAddVisible = ko.observable(true);
	self.HelpAddVisibleSettingKey = 2;
	self.HelpBingVisible = ko.observable(true);
	self.HelpBingVisibleSettingKey = 4;
	self.HelpNoBingVisible = ko.observable(true);
	self.HelpNoBingVisibleSettingKey = 7;
	self.HelpVisible = ko.observable(true);
	self.HelpVisibleSettingKey = 3;
	self.MapVisible = ko.observable(false);

	self.AddHelpMessageViewModel = new HelpMessageViewModel(ConnectCMS.Globalization.DontSeeTheResultYoureLookingForAboveYouCanCreateANewListing, self.HelpAddVisible, self.HelpAddVisibleSettingKey, null);
	self.AllEnterprises = ko.computed(function () {
		var result = null;

		var enterprises = self.Enterprises();

		if (enterprises != null) {
			if (result == null)
				result = new Array();

			ko.utils.arrayPushAll(result, enterprises);
		}

		var bingEnterprises = self.BingEnterprises();

		if (bingEnterprises != null) {
			if (result == null)
				result = new Array();

			ko.utils.arrayPushAll(result, bingEnterprises);
		}

		return result;
	});
	self.BingHelpMessageViewModel = new HelpMessageViewModel(ConnectCMS.Globalization.DontSeeTheResultYoureLookingForAboveTakeALookAt20MoreResultsFromTheWeb, self.HelpBingVisible, self.HelpBingVisibleSettingKey, null);
	self.BingSearchNoDataVisible = ko.computed(function () {
		var result = true;

		var bingEnterprises = self.BingEnterprises();

		if (bingEnterprises != null && bingEnterprises.length > 0)
			result = false;

		return result;
	});
	self.BingSearchProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.SearchingBing, self.BingExecuting);
	self.BingStartingMapId = ko.computed(function () {
		var result = 0;

		ko.utils.arrayForEach(self.Enterprises(), function (enterpriseViewModel) {
			ko.utils.arrayForEach(enterpriseViewModel.EnterpriseLocations(), function (enterpriseLocationViewModel) {
				if (enterpriseLocationViewModel.MapId() > result)
					result = enterpriseLocationViewModel.MapId();
			});
		});

		result++;

		return result;
	});
	self.Map = ko.computed(function () {
		return self.RecommendationViewModel.GetMapViewModel(self.MapVisible, self.Enterprises, self.BingEnterprises);
	});
	var mapElementId;
	self.MapElementId = ko.computed(function () {
		if (ConnectCMS.Strings.IsNullOrWhitespace(mapElementId))
			mapElementId = $("#searchBingMap")[0]

		return mapElementId;
	});
	self.MapText = ko.computed(function () {
		return self.RecommendationViewModel.GetMapText(self.MapVisible());
	});
	self.NoBingHelpMessageViewModel = new HelpMessageViewModel(ConnectCMS.Globalization.BingResultsAreOnlyAvailableOnTextSearches, self.HelpNoBingVisible, self.HelpNoBingVisibleSettingKey, null);
	self.SearchHelpMessageViewModel = new HelpMessageViewModel(ConnectCMS.Globalization.SearchForBusinessesAttractionsOrOtherLocalMerchantsToRecommend, self.HelpVisible, self.HelpVisibleSettingKey, null);
	self.SearchNoDataVisible = ko.computed(function () {
		var result = true;

		var enterprises = self.Enterprises();

		if (enterprises != null && enterprises.length > 0)
			result = false;

		return result;
	});
	self.SearchProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.Searching, self.Executing);
	self.SelectedCategory = ko.observable(null);
	var sort = ko.observable("1");
	self.Sort = ko.computed({
		read: function () {
			return sort();
		},
		write: function (value) {
			sort(value);

			self.BingChanged(true);
		}
	});
	var text = ko.observable("");
	//var text = ko.observable("AT&T Wireless");
	self.Text = ko.computed({
		read: function () {
			return text();
		},
		write: function (value) {
			text(value);

			self.BingChanged(true);
		}
	});

	self.BingSearchEnabled = ko.computed(function () {
		return !self.Executing() && !self.BingChanged() && !ConnectCMS.Strings.IsNullOrWhitespace(self.Text);
	});
	self.BingSearchNoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoResultsWereFound, ConnectCMS.Globalization.CheckYourSpellingOrTryBroadeningYourSearch, self.BingSearchNoDataVisible);
	self.SearchEnabled = ko.computed(function () {
		return !self.Executing();
	});
	self.CanSearch = ko.computed(function () {
		return !self.Executing() && (self.SelectedCategory() != null || !ConnectCMS.Strings.IsNullOrWhitespace(self.Text));
	});
	self.SearchNoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoResultsWereFound, ConnectCMS.Globalization.CheckYourSpellingOrTryBroadeningYourSearch, self.SearchNoDataVisible);

	self.ClickAdd = function () {
		self.RecommendationViewModel.OnNavigateEnterprise(null, null, RecommendationEnterpriseNavigationType.ADD);
	};
	self.ClickBingSearch = function () {
		if (self.BingSearchEnabled()) {
			self.BingExecuting(true);

			var enterprises = self.Enterprises();
			var exlucdeBingIds = null;

			if (!ConnectCMS.IsNullOrEmpty(enterprises)) {
				var bingId;
				var enterpriseLocationViewModels;

				ko.utils.arrayForEach(enterprises, function (enterpriseViewModel) {
					enterpriseLocationViewModels = enterpriseViewModel.EnterpriseLocations();

					if (!ConnectCMS.IsNullOrEmpty(enterpriseLocationViewModels))
						ko.utils.arrayForEach(enterpriseLocationViewModels, function (enterpriseLocationViewModel) {
							bingId = enterpriseLocationViewModel.BingId;

							if (!ConnectCMS.Strings.IsNullOrWhitespace(bingId)) {
								if (exlucdeBingIds == null)
									exlucdeBingIds = new Array();

								exlucdeBingIds.push(bingId);
							}
						});
				});
			}

			var data = {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
				excludeBingIds: exlucdeBingIds,
				sort: self.Sort(),
				text: self.Text()
			};

			$.ajax({
				url: "/ConnectCMS/Bing/GetSearchBingEnterprises",
				type: "POST",
				contentType: 'application/json',
				data: JSON.stringify(data),
				success: function (result, textStatus, jqXHR) {
					var bingEnterprises = $.map(result, function (enterpriseModel) {
						return new EnterpriseSearchBingViewModel(new ConnectCMS.Enterprises.EnterpriseMapViewModel(new ConnectCMS.Enterprises.EnterpriseViewModel(enterpriseModel, self.AllEnterprises), { mapElementId: self.MapElementId, onNavigateEnterprise: self.RecommendationViewModel.OnNavigateEnterprise }));
					});

					if (!ConnectCMS.IsNullOrEmpty(bingEnterprises))
						self.RecommendationViewModel.SetMapIds(bingEnterprises, self.BingStartingMapId());

					self.AddVisible(true);
					self.BingEnterprises(bingEnterprises);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					self.RecommendationViewModel.ErrorVisible(true);
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				},
				complete: function (jqXHR, textStatus) {
					self.BingExecuted(true);
					self.BingExecuting(false);
				}
			});
		}
	};
	self.ClickMap = function () {
		self.MapVisible(!self.MapVisible());

		self.OnClickMap(self.BingEnterprises);
		self.OnClickMap(self.Enterprises);
	};
	self.ClickSearch = function () {
		if (self.SearchEnabled()) {
			self.AddVisible(false);
			self.BingEnterprises(null);
			self.BingExecuted(false);
			self.BingVisible(false);
			self.Executed(false);
			self.Executing(true);

			var SelectedCategory = self.SelectedCategory();

			var categoryId = SelectedCategory != null ? SelectedCategory.PKID : null;
			var text = self.Text();

			$.ajax({
				url: "/ConnectCMS/Enterprise/GetSearchEnterprises",
				data: {
					categoryId: categoryId,
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
					sort: self.Sort(),
					text: text
				},
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					var enterprises = $.map(result, function (enterpriseModel) {
						return new ConnectCMS.Enterprises.EnterpriseSearchViewModel(enterpriseModel, { array: self.AllEnterprises, mapElementId: self.MapElementId, onNavigateEnterprise: self.RecommendationViewModel.OnNavigateEnterprise });
					});

					if (!ConnectCMS.IsNullOrEmpty(enterprises))
						self.RecommendationViewModel.SetMapIds(enterprises);

					if (ConnectCMS.Strings.IsNullOrWhitespace(text))
						self.AddVisible(true);
					else
						self.BingVisible(true);

					self.Enterprises(enterprises);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					self.RecommendationViewModel.ErrorVisible(true);
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				},
				complete: function (jqXHR, textStatus) {
					self.BingChanged(false);
					self.Executed(true);
					self.Executing(false);
				}
			});
		}
	};
	self.OnClickMap = function (enterprises) {
		enterprises = ko.utils.unwrapObservable(enterprises);

		if (!ConnectCMS.IsNullOrEmpty(enterprises)) {
			var enterpriseLocations;

			ko.utils.arrayForEach(enterprises, function (enterpriseViewModel) {
				enterpriseLocations = enterpriseViewModel.EnterpriseLocations();

				if (!ConnectCMS.IsNullOrEmpty(enterpriseLocations))
					ko.utils.arrayForEach(enterpriseLocations, function (enterpriseLocationViewModel) {
						if (enterpriseLocationViewModel.Selected()) {
							enterpriseLocationViewModel.Selected(false);
							enterpriseLocationViewModel.Selected(true);

							locationViewModel = enterpriseLocationViewModel.Location();

							if (locationViewModel != null)
								enterpriseLocationViewModel.CenterMap(locationViewModel.Latitude(), locationViewModel.Longitude());
						}
					});
			});
		}
	};
};