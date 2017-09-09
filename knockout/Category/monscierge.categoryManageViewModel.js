function CategoryManageViewModel(categoryViewModel) {
	BaseViewModel.call(this, categoryViewModel, true);

	var self = this;

	self.CategoryViewModel = categoryViewModel;

	self.ClickToggle = function () {
		self.OnDevice(!self.OnDevice());
	};
};