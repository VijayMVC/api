function RecommendationEnterpriseTipViewModel(recommendationEnterpriseViewModel) {
	var self = this;

	self.RecommendationEnterpriseViewModel = recommendationEnterpriseViewModel;

	self.Enterprise = ko.observable(null);
	self.Executed = ko.observable(false);
	self.Executing = ko.observable(false);
	self.RecommendationEnterpriseTipAddViewModel = ko.observable(new RecommendationEnterpriseTipAddViewModel(self));

	var filter = ko.observable("1");
	self.Filter = ko.computed({
		read: function () {
			return filter();
		},
		write: function (value) {
			filter(value);

			ConnectCMS.ShowPleaseWait();
			self.OnFilter();
			ConnectCMS.HidePleaseWait();
		}
	});
	self.NoDataVisible = ko.computed(function () {
		var result = true;

		var enterprise = self.Enterprise();

		if (enterprise != null) {
			var allTips = enterprise.AllTips();

			if (!ConnectCMS.IsNullOrEmpty(allTips))
				result = false;
		}

		return result;
	});
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.FetchingTips, self.Executing);
	var sort = ko.observable("1");
	self.Sort = ko.computed({
		read: function () {
			return sort();
		},
		write: function (value) {
			sort(value);

			ConnectCMS.ShowPleaseWait();
			self.OnSort();
			ConnectCMS.HidePleaseWait();
		}
	});
	self.ToolBarEnabled = ko.computed(function () {
		return !self.Executing();
	});

	self.NoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoTipsWereFound, null, self.NoDataVisible);

	self.ClickAdd = function () {
		ConnectCMS.ShowPleaseWait();

		$.ajax({
			url: "/ConnectCMS/Tip/Add",
			type: "POST",
			success: function (result) {
				NavigateOpen(self.RecommendationEnterpriseTipAddViewModel, self.RecommendationEnterpriseViewModel.SelectedContentIndex, self.RecommendationEnterpriseViewModel.NavigationElement, result);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.RecommendationEnterpriseViewModel.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				ConnectCMS.HidePleaseWait();
			}
		});
	};
	self.GetEnterpriseTipForEdit = function () {
		var enterprise = self.RecommendationEnterpriseViewModel.Enterprise();

		if (enterprise != null) {
			self.Executed(false);
			self.Executing(true);

			var enterpriseLocationId = null;

			if (self.RecommendationEnterpriseViewModel.NavigationForEnterpriseLocation() && enterprise.EnterpriseLocationsLength() == 1)
				enterpriseLocationId = enterprise.FirstEnterpriseLocation().PKID();

			$.ajax({
				url: "/ConnectCMS/Enterprise/GetEnterpriseTipForEdit",
				data: {
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
					enterpriseId: enterprise.PKID(),
					enterpriseLocationId: enterpriseLocationId
				},
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					self.Enterprise(new ConnectCMS.Enterprises.EnterpriseTipViewModel(new ConnectCMS.Enterprises.EnterpriseViewModel(result, null)));

					self.OnFilter();
					self.OnSort();
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
	self.OnFilter = function () {
		var filter = Number(self.Filter());
		var loggedInUserId = self.RecommendationEnterpriseViewModel.RecommendationViewModel.LoggedInUserId();
		var predicate;

		switch (filter) {
			case 1:
				predicate = function (tipViewModel) {
					return true;
				};

				break;
			case 2:
				predicate = function (tipViewModel) {
					return tipViewModel.ContactUser().PKID == loggedInUserId;
				};

				break;
			case 3:
				predicate = function (tipViewModel) {
					return tipViewModel.ContactUser().PKID != loggedInUserId;
				};

				break;
		}

		var enterpriseTipViewModel = self.Enterprise();

		if (enterpriseTipViewModel != null)
			enterpriseTipViewModel.FilterPredicate(predicate);
	};
	self.OnSort = function () {
		var comparefn;
		var sort = Number(self.Sort());

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

		var enterpriseTipViewModel = self.Enterprise();

		if (enterpriseTipViewModel != null)
			enterpriseTipViewModel.SortPredicate(comparefn);
	};

	self.GetEnterpriseTipForEdit();
};