function RecommendationBlacklistViewModel(recommendationViewModel) {
	var self = this;

	self.RecommendationViewModel = recommendationViewModel;

	self.Enterprises = ko.observableArray([]);
	self.Executed = ko.observable(false);
	self.Executing = ko.observable(false);
	self.MapVisible = ko.observable(false);
	self.SelectedCategory = ko.observable(null);
	self.Text = ko.observable("");

	self.Map = ko.computed(function () {
		return self.RecommendationViewModel.GetMapViewModel(self.MapVisible, self.Enterprises, null);
	});
	var mapElementId;
	self.MapElementId = ko.computed(function () {
		if (ConnectCMS.Strings.IsNullOrWhitespace(mapElementId))
			mapElementId = $("#blacklistBingMap")[0]

		return mapElementId;
	});
	self.MapText = ko.computed(function () {
		return self.RecommendationViewModel.GetMapText(self.MapVisible());
	});
	self.NoDataVisible = ko.computed(function () {
		var result = true;

		if (!ConnectCMS.IsNullOrEmpty(self.Enterprises))
			result = false;

		return result;
	});
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.SearchingBlacklistings, self.Executing);
	self.SearchEnabled = ko.computed(function () {
		return !self.Executing();
	});
	var sort = ko.observable("1");
	self.Sort = ko.computed({
		read: function () {
			return sort();
		},
		write: function (value) {
			sort(value);

			self.OnSort();
		}
	});

	self.NoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoBlacklistingsWereFound, ConnectCMS.Globalization.CheckYourSpellingOrTryBroadeningYourSearch, self.NoDataVisible);

	self.ClickMap = function () {
		self.MapVisible(!self.MapVisible());
	};
	self.ClickSearch = function () {
		if (self.SearchEnabled()) {
			self.Executed(false);
			self.Executing(true);

			var SelectedCategory = self.SelectedCategory();

			var categoryId = SelectedCategory != null ? SelectedCategory.PKID : null;

			$.ajax({
				url: "/ConnectCMS/Recommendation/GetSearchBlacklistedEnterprisesOnDevice",
				data: {
					categoryId: categoryId,
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
					sort: self.Sort(),
					text: self.Text()
				},
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					var enterprises = $.map(result, function (enterpriseModel) {
						return new ConnectCMS.Enterprises.EnterpriseBlacklistViewModel(enterpriseModel, { array: self.Enterprises, mapElementId: self.MapElementId, onNavigateEnterprise: self.RecommendationViewModel.OnNavigateEnterprise });
					});

					if (!ConnectCMS.IsNullOrEmpty(enterprises)) {
						self.RecommendationViewModel.SetMapIds(enterprises);
						self.OnSort();
					}

					self.Enterprises(enterprises);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					self.RecommendationViewModel.ErrorVisible(true);
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				},
				complete: function (jqXHR, textStatus) {
					self.Executed(true);
					self.Executing(false);
				}
			});
		}
	};
	self.OnSort = function () {
		var comparefn;
		var sort = Number(self.Sort());

		switch (sort) {
			case 1:
				comparefn = ConnectCMS.Enterprises.SortEnterpriseByNearest;

				break;
			case 2:
				comparefn = ConnectCMS.Enterprises.SortEnterpriseByName;

				break;
		}

		var enterprises = self.Enterprises();

		if (!ConnectCMS.IsNullOrEmpty(enterprises)) {
			enterprises = enterprises.sort(comparefn);

			self.RecommendationViewModel.SetMapIds(enterprises, 1);
			self.Enterprises(enterprises);
		}
	};
};