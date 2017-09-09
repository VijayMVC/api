function RecommendationEnterpriseViewModel(recommendationViewModel) {
	var self = this;

	self.RecommendationViewModel = recommendationViewModel;

	self.ClickedEnterpriseTab = ko.observable(false);
	self.ClickedLocationTab = ko.observable(false);
	self.ClickedRecommendTab = ko.observable(false);
	self.ClickedTipTab = ko.observable(false);
	self.Enterprise = ko.validatedObservable(null);
	self.EnterpriseTabIndex = 0;
	self.ErrorVisible = ko.observable(false);
	self.Executing = ko.observable(false);
	self.ImageViewViewModel = ko.observable(null);
	self.LocationTabIndex = 1;
	self.NavigationElement = "#enterpriseNavigationDiv";
	self.NavigationForEnterpriseLocation = ko.observable(false);
	self.NavigationType = ko.observable(null);
	self.RecommendTabIndex = 2;
	self.SelectedContentIndex = ko.observable(0);
	self.SelectedTabIndex = ko.observable(0);
	self.TipTabIndex = 3;
	self.Verified = ko.observable(false);

	self.EnterpriseTabLabel = ko.computed(function () {
		var result = null;

		var navigationType = self.NavigationType();

		if (navigationType != null)
			switch (navigationType) {
				case RecommendationEnterpriseNavigationType.ADD:
				case RecommendationEnterpriseNavigationType.TIP:
					result = ConnectCMS.Globalization.Merchant;

					break;
				case RecommendationEnterpriseNavigationType.RECOMMEND:
					result = ConnectCMS.Globalization.Verify;

					break;
			}

		return result;
	});
	self.ErrorMessageViewModel = new ErrorMessageViewModel(ConnectCMS.Globalization.AnErrorHasOccured, self.ErrorVisible);
	var headerEnterprise = ko.observable(null);
	self.HeaderEnterprise = ko.computed({
		read: function () {
			return headerEnterprise();
		},
		write: function (value) {
			headerEnterprise(value);

			var headerEnterpriseViewModel = self.HeaderEnterprise();

			if (headerEnterpriseViewModel != null)
				headerEnterpriseViewModel.ClickBack = self.ClickBack;
		}
	});
	self.LocationsTabVisible = ko.computed(function () {
		var result = false;

		var enterprise = self.Enterprise();

		if (enterprise != null && enterprise.EnterpriseLocationsLength() > 1)
			result = true;

		return result;
	});

	var recommendationEnterpriseEditViewModel = ko.observable(null);
	self.RecommendationEnterpriseEditViewModel = ko.computed(function () {
		var navigationType = self.NavigationType();

		if (recommendationEnterpriseEditViewModel() == null && navigationType != null && self.ClickedEnterpriseTab() && self.Enterprise() != null) {
			recommendationEnterpriseEditViewModel(new RecommendationEnterpriseEditViewModel(self));

			if (navigationType == RecommendationEnterpriseNavigationType.ADD)
				recommendationEnterpriseEditViewModel().Edit(true);
		}

		return recommendationEnterpriseEditViewModel();
	});
	var recommendationEnterpriseLocationViewModel = ko.observable(null);
	self.RecommendationEnterpriseLocationViewModel = ko.computed(function () {
		if (recommendationEnterpriseLocationViewModel() == null && self.ClickedLocationTab() && self.Enterprise() != null)
			recommendationEnterpriseLocationViewModel(new RecommendationEnterpriseLocationViewModel(self));

		return recommendationEnterpriseLocationViewModel();
	});
	var recommendationEnterpriseRecommendViewModel = ko.observable(null);
	self.RecommendationEnterpriseRecommendViewModel = ko.computed(function () {
		if (recommendationEnterpriseRecommendViewModel() == null && self.ClickedRecommendTab() && self.Enterprise() != null)
			recommendationEnterpriseRecommendViewModel(new RecommendationEnterpriseRecommendViewModel(self));

		return recommendationEnterpriseRecommendViewModel();
	});
	var recommendationEnterpriseTipViewModel = ko.observable(null);
	self.RecommendationEnterpriseTipViewModel = ko.computed(function () {
		if (recommendationEnterpriseTipViewModel() == null && self.ClickedTipTab() && self.Enterprise() != null)
			recommendationEnterpriseTipViewModel(new RecommendationEnterpriseTipViewModel(self));

		return recommendationEnterpriseTipViewModel();
	});

	self.ClickBack = function () {
		NavigateDispose(self.RecommendationViewModel.SelectedContentIndex, self.RecommendationViewModel.NavigationElement);

		var recommendationTipViewModel = self.RecommendationViewModel.RecommendationTipViewModel();

		if (recommendationTipViewModel != null)
			recommendationTipViewModel.GetEnterprisesWithTipsForHotel();

		self.ClickedEnterpriseTab(false);
		self.ClickedLocationTab(false);
		self.ClickedRecommendTab(false);
		self.ClickedTipTab(false);
		self.Enterprise(null);
		self.ErrorVisible(false);
		self.HeaderEnterprise(null);
		self.Verified(false);

		recommendationEnterpriseEditViewModel(null);
		recommendationEnterpriseLocationViewModel(null);
		recommendationEnterpriseRecommendViewModel(null);
		recommendationEnterpriseTipViewModel(null);

		var enterpriseViewModel = self.Enterprise();

		if (enterpriseViewModel)
			enterpriseViewModel.revertData(true);
	};
	self.ClickEnterpriseTab = function () {
		self.SelectedTabIndex(self.EnterpriseTabIndex);
		self.ClickedEnterpriseTab(true);

		return true;
	};
	self.ClickLocationTab = function () {
		self.SelectedTabIndex(self.LocationTabIndex);
		self.ClickedLocationTab(true);

		return true;
	};
	self.ClickRecommendTab = function () {
		self.SelectedTabIndex(self.RecommendTabIndex);
		self.ClickedRecommendTab(true);

		return true;
	};
	self.ClickTipTab = function () {
		self.SelectedTabIndex(self.TipTabIndex);
		self.ClickedTipTab(true);

		return true;
	};
	self.GetEnterpriseForEdit = function (enterpriseId, enterpriseLocationId) {
		ConnectCMS.ShowPleaseWait();
		self.Executing(true);

		$.ajax({
			url: "/ConnectCMS/Enterprise/GetEnterpriseForEdit",
			data: {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
				enterpriseId: enterpriseId,
				enterpriseLocationId: enterpriseLocationId,
			},
			type: "POST",
			success: function (result) {
				self.SetEnterprise(new ConnectCMS.Enterprises.EnterpriseViewModel(result, null));
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				self.Executing(false);
				ConnectCMS.HidePleaseWait();
			}
		});
	};
	self.OnClickImageBack = function () {
		NavigateDispose(self.SelectedContentIndex, self.NavigationElement);
	};
	self.OnClickImageView = function (imageEnterpriseEditViewModel) {
		if (imageEnterpriseEditViewModel != null) {
			ConnectCMS.ShowPleaseWait();

			self.ImageViewViewModel(new ImageViewViewModel(imageEnterpriseEditViewModel, self.OnClickImageBack));

			$.ajax({
				url: "/ConnectCMS/Image/Preview",
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					NavigateOpen(self.ImageViewViewModel, self.SelectedContentIndex, self.NavigationElement, result);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					self.ErrorVisible(true);
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				},
				complete: function (jqXHR, textStatus) {
					ConnectCMS.HidePleaseWait();
				}
			});
		}
	};
	self.SetEnterprise = function (enterpriseViewModel) {
		self.Enterprise(ConnectCMS.Framework.ExtendObject(enterpriseViewModel, ConnectCMS.Enterprises.EnterpriseEditViewModel, { onClickImageView: self.OnClickImageView }));
		self.HeaderEnterprise(ConnectCMS.Framework.ExtendObject(enterpriseViewModel, ConnectCMS.Enterprises.EnterpriseHeaderViewModel, { onClickBack: self.ClickBack }));
		self.SetTab();
	};
	self.SetEnterpriseForEdit = function (enterpriseViewModel, enterpriseLocationViewModel, navigationType) {
		self.NavigationType(navigationType);

		if (enterpriseLocationViewModel == null)
			self.NavigationForEnterpriseLocation(false)
		else
			self.NavigationForEnterpriseLocation(true);

		if (enterpriseViewModel != null) {
			var enterpriseId = enterpriseViewModel.PKID;

			if (enterpriseId != null) {
				var enterpriseLocationId = null;

				if (enterpriseLocationViewModel != null)
					enterpriseLocationId = enterpriseLocationViewModel.PKID;

				self.GetEnterpriseForEdit(enterpriseId, enterpriseLocationId);
			} else
				self.SetEnterprise(enterpriseViewModel);
		}
		else if (enterpriseLocationViewModel == null) {
			self.Enterprise(new ConnectCMS.Enterprises.EnterpriseEditViewModel(null, self.OnClickImageView));
			self.HeaderEnterprise(null);
			self.SetTab();
		}
	};
	self.SetTab = function () {
		var enterprise = self.Enterprise();
		var navigationType = self.NavigationType();

		if (enterprise != null && navigationType != null)
			switch (navigationType) {
				case RecommendationEnterpriseNavigationType.ADD:
					self.ClickEnterpriseTab();

					break;
				case RecommendationEnterpriseNavigationType.RECOMMEND:
					var recommended = enterprise.Recommended();

					if (!recommended)
						self.ClickEnterpriseTab();
					else {
						self.Verified(true);
						self.ClickRecommendTab();
					}

					break;
				case RecommendationEnterpriseNavigationType.TIP:
					self.ClickTipTab();

					break;
			}
	};

	self.Tabs = [
		new TabViewModel(self.EnterpriseTabLabel, true, true, self.ClickEnterpriseTab, self.EnterpriseTabIndex, self.SelectedTabIndex),
		new TabViewModel(ConnectCMS.Globalization.Locations, self.LocationsTabVisible, self.Verified, self.ClickLocationTab, self.LocationTabIndex, self.SelectedTabIndex),
		new TabViewModel(ConnectCMS.Globalization.Recommend, true, self.Verified, self.ClickRecommendTab, self.RecommendTabIndex, self.SelectedTabIndex),
		new TabViewModel(ConnectCMS.Globalization.Tips, true, self.Verified, self.ClickTipTab, self.TipTabIndex, self.SelectedTabIndex)
	];
};