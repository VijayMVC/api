function RecommendationEnterpriseTipAddViewModel(recommentationEnterpriseTipViewModel) {
	var self = this;

	self.RecommendationEnterpriseTipViewModel = recommentationEnterpriseTipViewModel;

	self.ErrorVisible = ko.observable(false);
	self.Tip = ko.observable("").extend({
		required: true
	});

	self.ErrorMessageViewModel = new ErrorMessageViewModel(ConnectCMS.Globalization.AnErrorHasOccured, self.ErrorVisible);

	self.ClickBack = function () {
		NavigateDispose(self.RecommendationEnterpriseTipViewModel.RecommendationEnterpriseViewModel.SelectedContentIndex, self.RecommendationEnterpriseTipViewModel.RecommendationEnterpriseViewModel.NavigationElement);
		self.ErrorVisible(false);
	};
	self.ClickUpdate = function () {
		ConnectCMS.ShowPleaseWait();

		var enterpriseId = self.RecommendationEnterpriseTipViewModel.RecommendationEnterpriseViewModel.Enterprise().PKID();
		var enterpriseLocationId = self.RecommendationEnterpriseTipViewModel.RecommendationEnterpriseViewModel.Enterprise().FirstEnterpriseLocation().PKID();

		$.ajax({
			url: "/ConnectCMS/Tip/InsertTip",
			data: {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
				enterpriseId: enterpriseId,
				enterpriseLocationId: enterpriseLocationId,
				tip: self.Tip()
			},
			type: "POST",
			success: function (result) {
				self.RecommendationEnterpriseTipViewModel.GetEnterpriseTipForEdit();
				self.ClickBack();
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				ConnectCMS.HidePleaseWait();
			}
		});
	};
};