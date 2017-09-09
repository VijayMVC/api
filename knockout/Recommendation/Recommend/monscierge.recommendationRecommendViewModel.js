function RecommendationRecommendViewModel(recommendationViewModel) {
	var self = this;

	self.RecommendationViewModel = recommendationViewModel;

	self.Enterprises = ko.observableArray(null);
	self.Executed = ko.observable(false);
	self.Executing = ko.observable(false);
	self.MapVisible = ko.observable(false);
	self.RecommendationRecommendReorderViewModel = ko.observable(new RecommendationRecommendReorderViewModel(self));
	self.SelectedCategory = ko.observable(null);
	self.Text = ko.observable("");

	var mapElementId;
	self.MapElementId = ko.computed(function () {
		if (ConnectCMS.Strings.IsNullOrWhitespace(mapElementId))
			mapElementId = $("#recommendBingMap")[0]

		return mapElementId;
	});
	self.Map = ko.computed(function () {
		return self.RecommendationViewModel.GetMapViewModel(self.MapVisible, self.Enterprises, null);
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
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.SearchingRecommendations, self.Executing);
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

	self.NoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoRecommendationsWereFound, ConnectCMS.Globalization.CheckYourSpellingOrTryBroadeningYourSearch, self.NoDataVisible);

	self.ClickMap = function () {
		self.MapVisible(!self.MapVisible());
	};
	self.ClickReorder = function () {
		ConnectCMS.ShowPleaseWait();

		$.ajax({
			url: "/ConnectCMS/Recommendation/RecommendReorder",
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				NavigateOpen(self.RecommendationRecommendReorderViewModel, self.RecommendationViewModel.SelectedContentIndex, self.RecommendationViewModel.NavigationElement, result);
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
	self.ClickSearch = function () {
		if (self.SearchEnabled()) {
			self.Executed(false);
			self.Executing(true);

			var selectedCategory = self.SelectedCategory();

			var categoryId = selectedCategory != null ? selectedCategory.PKID : null;

			$.ajax({
				url: "/ConnectCMS/Recommendation/GetSearchRecommendedEnterprisesOnDevice",
				data: {
					categoryId: categoryId,
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
					sort: self.Sort(),
					text: self.Text()
				},
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					var enterprises = $.map(result, function (enterpriseModel) {
						return new ConnectCMS.Enterprises.EnterpriseRecommendViewModel(enterpriseModel, { array: self.Enterprises, mapElementId: self.MapElementId, onNavigateEnterprise: self.RecommendationViewModel.OnNavigateEnterprise });
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
			case 3:
				comparefn = ConnectCMS.Enterprises.SortEnterpriseByRecommendedCategoriesOrder;

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