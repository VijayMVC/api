function RecommendationBlacklistRecommendPopupViewModel(options) {
	PopupViewModel.call(this, options);

	var self = this;

	self.RecommendedVisible = ko.observable(true);

	self.RecommendedInformationMessageViewModel = new InformationMessageViewModel(ConnectCMS.Globalization.ThisIsCurrentlyRecommendedMakingChangesToTheBlacklistingWillRemoveTheRecommendation, self.RecommendedVisible());
};

ExtendPopupViewModel(RecommendationBlacklistRecommendPopupViewModel);