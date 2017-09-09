function NavigationItemViewModel(data) {
	var self = this;

	self.Page = data.Page;
	self.WithDeviceId = data.WithDeviceId || false;
	self.Text = ko.observable(data.Text);
	self.Icon = ko.observable(data.Icon);
	self.IsSilverlight = data.IsSilverlight || false;
	self.IsDefault = data.IsDefault || false;
	self.UseCustomNavigation = data.UseCustomNavigation || true;
	self.CustomNavigationSelector = data.CustomNavigationSelector;

	self.url = data.Url;
	self.Url = ko.computed(function () {
		return self.url;
	});

	self.SubItems = ko.observableArray(data.SubMenuItems == null ? [] : $.map(data.SubMenuItems, function (item) {
		return new NavigationItemViewModel(item);
	}));

	self.IsSelected = ko.computed(function () {
		return ConnectCMS.MainViewModel.SelectedPage() == self;
	});

	self.IsExpanded = ko.computed(function () {
		return self.IsSelected() || ko.utils.arrayFirst(self.SubItems(), function (item) { return item.IsExpanded(); }) != null;
	});

	self.Navigate = function () {
		//if (self.IsSelected())
		//	return;
		ConnectCMS.MainViewModel.SelectedPage(self);
		if(self.IsSilverlight || !ConnectCMS.Strings.IsNullOrWhitespace(self.Url()))
			self.loadPage();
	};

	self.loadPage = function () {
		ConnectCMS.MainViewModel.CurrentMVCPage(self.Url());
		$('#mvcContainer').empty();

		if (self.IsSilverlight) {
			ConnectCMS.MainViewModel.ShowMvc(false);
			SilverlightShow();
			SilverlightNavigate(self.Page, self.WithDeviceId, self.UseCustomNavigation, self.CustomNavigationSelector);
		} else {
			ConnectCMS.MainViewModel.ShowMvc(true);
			SilverlightHide();

			$.ajax({
				url: self.Url(),
				type: "POST",
				data: ConnectCMS.MainViewModel.SelectedDevice() != null ? {
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
				} : null,
				success: function (result, textStatus, jqXhr) {
					if(ConnectCMS.MainViewModel.CurrentMVCPage() == self.Url())
						$('#mvcContainer').html(result);
				},
				error: function (jqXhr, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXhr, textStatus, errorThrown);
				}
			});
		}
	};

	self.init = function () {
		if (self.IsDefault) {
			self.loadPage();
		}
	};

	self.init();

	return {
		Page: self.Page,
		WithDeviceId: self.WithDeviceId,
		Text: self.Text,
		Icon: self.Icon,
		IsSilverlight: self.IsSilverlight,
		IsDefault: self.IsDefault,
		UseCustomNavigation: self.UseCustomNavigation,
		CustomNavigationSelector: self.CustomNavigationSelector,
		Url: self.Url,
		SubItems: self.SubItems,
		IsSelected: self.IsSelected,
		IsExpanded: self.IsExpanded,
		Navigate: self.Navigate
	}
};