function CategoryCategoryViewModel(categoryViewModel, array, onClickEdit, onDeleteSuccess) {
	BaseViewModel.call(this, categoryViewModel, true);

	var self = this;

	self.Array = ko.isObservable(array) ? array : ko.observableArray(array);
	self.CategoryViewModel = categoryViewModel;
	self.OnClickEdit = onClickEdit;
	self.OnDeleteSuccess = onDeleteSuccess;

	self.ClickEdit = function () {
		if (self.OnClickEdit != null)
			self.OnClickEdit(self);
	};
	self.ClickDelete = function () {
		var dialog = new PopupViewModel({
			header: ConnectCMS.Globalization.RemoveCategoryQuestionMark,
			body: ConnectCMS.Globalization.AreYouCertainYouWantToRemoveThisCategoryThisWillNoLongerBeDisplayedByYourProperty,
			okButton: ConnectCMS.Globalization.Remove,
			cancelButton: ConnectCMS.Globalization.Cancel,
			onOk: function () {
				self.OnDelete();

				return true;
			}
		});

		dialog.Open();
	};
	self.OnDelete = function () {
		ConnectCMS.ShowPleaseWait();

		$.ajax({
			url: "/ConnectCMS/Category/DeleteCategory",
			data: {
				categoryId: self.PKID,
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
			},
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				var array = self.Array();

				if (!ConnectCMS.IsNullOrEmpty(array))
					ko.utils.arrayRemoveItem(array, self);

				if (self.OnDeleteSuccess != null)
					self.OnDeleteSuccess();
			},
			error: function (jqXHR, textStatus, errorThrown) {
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				ConnectCMS.HidePleaseWait();
			}
		});
	};

	self.ListButtonViewModel = ko.computed(function () {
		var listButtonOptions = new Array();

		listButtonOptions.push(listButtonOption = {
			clickMethod: self.ClickEdit,
			defaultable: true,
			label: ConnectCMS.Globalization.Edit
		});
		listButtonOptions.push(listButtonOption = {
			clickMethod: self.ClickDelete,
			defaultable: true,
			label: ConnectCMS.Globalization.Remove
		});

		return new ListButtonViewModel(null, listButtonOptions);
	});
};