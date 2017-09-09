function CategoryHeaderViewModel(categoryViewModel, onClickBack) {
	BaseViewModel.call(this, categoryViewModel, true);

	var self = this;

	self.CategoryViewModel = categoryViewModel;
	self.OnClickBack = onClickBack;

	self.ClickBack = function () {
		if (self.OnClickBack != null)
			self.OnClickBack();
	};
};