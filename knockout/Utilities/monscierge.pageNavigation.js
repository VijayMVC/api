var ConnectCMS = ConnectCMS || {};
ConnectCMS.Utilities = ConnectCMS.Utilities || {};

ConnectCMS.Utilities.PageNavigation = function () {
	var self = this;

	self.getNavigationItem = function (navItems, urlToFind, isSilverlightPage) {
		var found = null;
		for (var i = 0; i < navItems.length; i++) {
			var navItem = navItems[i];

			if (isSilverlightPage && navItem.CustomNavigationSelector === urlToFind) {
				found = navItem;
				break;
			} else if (navItem.Url() === urlToFind) {
				found = navItem;
				break;
			}

			// Recurse sub-items if necessary
			if (navItem.SubItems().length > 0) {
				// Prepend the parent path to the urlToFind if the item we're searching for is a Silverlight page.
				var subItemUrlToFind = (isSilverlightPage) ? navItem.CustomNavigationSelector + '/' + urlToFind : urlToFind;
				found = self.getNavigationItem(navItem.SubItems(), subItemUrlToFind, isSilverlightPage);
				if (found) {
					break;
				}
			}
		}
		return found;
	};

	// The navigateUrl can be either an MVC url or the Silverlight CustomNavigationSelector property.
	self.navigate = function (navigateUrl, isSilverlightPage) {
		// IF the page was not loaded via Ajax, just go to the url.
		var marker = document.getElementById('non-ajax-request');
		if (marker) {
			window.location = navigateUrl;
		} else {
			// Find the link in the menu items. If it's there, set it as the selected page
			var navItems = ConnectCMS.MainViewModel.NavigationItems();
			var selectedNavItem = self.getNavigationItem(navItems, navigateUrl, isSilverlightPage);

			if (selectedNavItem) {
				selectedNavItem.Navigate();
			} else {
				// Didn't find the link in the menu.
				// It could be a page-to-page link so create a new NavigationItem and navigate to it.
				var navItemView = new NavigationItemViewModel({
					Url: navigateUrl,
					CustomNavigationSelector: navigateUrl,
					Page: null,
					IsSilverlight: isSilverlightPage || false,
					WithDeviceId: true,
					UseCustomNavigation: true,
					IsDefault: false
				});

				navItemView.Navigate();
			}
		}
	};

	return {
		navigate: self.navigate
	}
};