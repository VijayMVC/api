function ExtendPopupViewModel(model) {
	model.prototype = Object.create(DataModel.prototype);
	model.prototype.constructor = model;
};