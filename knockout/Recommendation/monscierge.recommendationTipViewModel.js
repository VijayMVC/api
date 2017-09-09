function RecommendationTipViewModel(recommendationViewModel) {
	var self = this;

	self.RecommendationViewModel = recommendationViewModel;

	self.Enterprises = ko.observableArray([]);
	self.Executed = ko.observable(false);
	self.Executing = ko.observable(false);

	self.NoDataVisible = ko.computed(function () {
		var result = true;

		var enterprises = self.Enterprises();

		if (!ConnectCMS.IsNullOrEmpty(enterprises))
			result = false;

		return result;
	});

	var enterpriseSort = ko.observable("1");
	self.EnterpriseSort = ko.computed({
		read: function () {
			return enterpriseSort();
		},
		write: function (value) {
			enterpriseSort(value);

			ConnectCMS.ShowPleaseWait();
			self.OnEnterpriseSort();
			ConnectCMS.HidePleaseWait();
		}
	});
	self.NoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoTipsWereFound, null, self.NoDataVisible);
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.FetchingTips, self.Executing);
	var tipSort = ko.observable("1");
	self.TipSort = ko.computed({
		read: function () {
			return tipSort();
		},
		write: function (value) {
			tipSort(value);

			ConnectCMS.ShowPleaseWait();
			self.OnTipSort();
			ConnectCMS.HidePleaseWait();
		}
	});
	self.ToolBarEnabled = ko.computed(function () {
		return !self.Executing();
	});

	self.GetEnterprisesWithTipsForHotel = function () {
		self.Executed(false);
		self.Executing(true);

		$.ajax({
			url: "/ConnectCMS/Recommendation/GetEnterprisesWithTipsForHotel",
			data: {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
			},
			type: "POST",
			success: function (result) {
				self.Enterprises($.map(result, function (enterpriseModel) {
					return new ConnectCMS.Enterprises.EnterpriseTipViewModel(enterpriseModel, { array: self.Enterprises });
				}));

				self.OnEnterpriseSort();
				self.OnTipSort();
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
	};
	self.OnEnterpriseSort = function () {
		var comparefn;
		var sort = Number(self.EnterpriseSort());

		switch (sort) {
			case 1:
				comparefn = ConnectCMS.Enterprises.SortEnterpriseByNearest;

				break;
			case 2:
				comparefn = ConnectCMS.Enterprises.SortEnterpriseByName;

				break;
		}

		var enterprises = self.Enterprises();

		if (!ConnectCMS.IsNullOrEmpty(enterprises))
			self.Enterprises(enterprises.sort(comparefn));
	};
	self.OnTipSort = function () {
		var comparefn;
		var sort = Number(self.TipSort());

		switch (sort) {
			case 1:
				comparefn = ConnectCMS.Enterprises.SortTipByLatestLastModifiedMilliseconds;

				break;
			case 2:
				comparefn = ConnectCMS.Enterprises.SortTipByEarliestLastModifiedMilliseconds;

				break;
			case 3:
				comparefn = ConnectCMS.Enterprises.SortTipByContactUserName;

				break;
			case 4:
				comparefn = ConnectCMS.Enterprises.SortTipByLocationFormattedAddress;

				break;
		}

		var enterprises = self.Enterprises();

		if (!ConnectCMS.IsNullOrEmpty(enterprises))
			ko.utils.arrayForEach(enterprises, function (enterpriseTipViewModel) {
				enterpriseTipViewModel.SortPredicate(comparefn);
			});
	};

	self.GetEnterprisesWithTipsForHotel();
};