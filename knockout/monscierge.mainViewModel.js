function MainViewModel() {
	var self = this;

	self.ImageGalleryOptions = [];
	self.NavigationItems = ko.observableArray([]);
	self.NavigationLoading = ko.observable(false);
	self.PageLoading = ko.observable(false);
	// TODO: FH: Remove default once Recommendations is in proper container.
	self.SelectedDevice = ko.observable();
	self.SelectedPage = ko.observable();
	self.ShowImageGallery = ko.observable(false);
	self.ShowOverlay = ko.observable(false);
	self.ShowPleaseWait = ko.observable(false);
	self.ShowMvc = ko.observable(false);
	self.ShowUnsupportedBrowser = ko.observable(false);
	self.ShowAlreadySignedIn = ko.observable(false);

	self.CheckNearbyUsersTimeout = null;
	self.AvailableBeacons = ko.observableArray([]);
	self.SelectedBeacon = ko.observable();
	self.SelectedBeacon.subscribe(function (newValue) {
		var expire = new Date();
		expire.setDate(expire.getDate() + (365 * 10));
		document.cookie = "DashboardBeacon=" + (self.SelectedBeacon() == null ? "" : self.SelectedBeacon()) + "; expires=" + expire.toUTCString();
		if (self.CheckNearbyUsersTimeout != null)
			clearTimeout(self.CheckNearbyUsersTimeout);

		if (self.SelectedBeacon() != null) {
			self.CheckNearbyUsers();
		} else {
			self.NearbyGuests([]);
		}
	});
	self.CheckSelectedBeacon = function () {
		var value = getCookie("DashboardBeacon");
		var sel = ko.utils.arrayFirst(self.AvailableBeacons(), function (item) { return item.PKID == value; });
		if (sel != null)
			self.SelectedBeacon(sel.PKID);
		else
			self.SelectedBeacon(null);
	};
	self.NearbyGuests = ko.observableArray([]);
	var dismissedOn;
	var show = false;
	self.CheckNearbyUsers = function () {
		self.CheckNearbyUsersTimeout = window.setTimeout(function () {
			$.ajax({
				url: "/ConnectCMS/Dashboard/GetBeaconNearbyUsers",
				data: { beaconId: self.SelectedBeacon() },
				type: "GET",
				success: function (result, textStatus, jqXHR) {
					if (result != null && result.length > 0) {
						self.NearbyGuests($.map(result, function (item) {
							var backgroundColor = item.User.Title == 'Platinum Elite' ? 'aliceblue' : (item.User.Title == 'Gold Elite' ? 'gold' : 'rgba(0, 0, 0, 0.8)');
							var color = item.User.Title == 'Platinum Elite' ? '#222' : (item.User.Title == 'Gold Elite' ? '#222' : 'rgb(153, 153, 153)');
							return { PKID: item.User.PKID, Name: item.User.Name, Title: item.User.Title, ImageUrl: item.User.ImageUrl, EnteredOn: moment.utc(item.EnteredOn), BackgroundColor: backgroundColor, Color: color };
						}));
						if (dismissedOn == null || self.NearbyGuests().some(function (item) { return item.EnteredOn > dismissedOn; })) {
							dismissedOn = null;
							$('#helpPanel').animate({ 'right': '-2px' });
						}
					} else {
						self.NearbyGuests([]);
						if (!show)
							$('#helpPanel').animate({ 'right': '-327px' });
					}
					if (self.CheckNearbyUsersTimeout != null)
						clearTimeout(self.CheckNearbyUsersTimeout);
					self.CheckNearbyUsers();
				},
				error: function (jqXHR, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});
		}, 2000);
	}

	self.ShowNearbyUsers = function () {
		if ($('#helpPanel').css('right') == '-327px') {
			show = true;
			$('#helpPanel').animate({ 'right': '-2px' });
		} else {
			show = false;
			dismissedOn = moment.utc();
			$('#helpPanel').animate({ 'right': '-327px' });
		}
	};

	self.ValidationTimeout = null;
	self.AuthMax = 432000000; //2147483647
	self.AuthWarning = 300000;
	self.AuthExpiration = moment.utc();
	self.AuthRemaining = function () {
		var now = moment.utc();
		return self.AuthExpiration.diff(now);
	};

	self.CurrentMVCPage = ko.observable();

	var imageGalleryViewModel = ko.observable();
	self.ImageGalleryViewModel = ko.computed({
		read: function () {
			return imageGalleryViewModel();
		},
		write: function (value) {
			if (imageGalleryViewModel() == value)
				return;

			if (imageGalleryViewModel() != null && imageGalleryViewModel() != value)
				imageGalleryViewModel().Clean();

			imageGalleryViewModel(value);
		}
	});
	var popupViewModel = ko.observable();
	self.PopupViewModel = ko.computed({
		read: function () {
			return popupViewModel();
		},
		write: function (value) {
			if (popupViewModel() == value)
				return;

			if (popupViewModel() != null && popupViewModel() != value)
				popupViewModel().Dispose();

			popupViewModel(value);
		}
	});

	self.Logout = function () {
		$('#logoutForm')[0].submit();
	};

	self.Navigate = function (page, useDeviceId, useCustomNavigation, customNavigationSelector) {
		ConnectCMS.MainViewModel.ShowMvc(false);
		SilverlightShow();
		SilverlightNavigate(page, useDeviceId, useCustomNavigation, customNavigationSelector);
	};

	self.UpdateCurrentDevice = function (device) {
		self.NavigationLoading(true);
		self.SelectedDevice(null);
		self.NavigationItems.removeAll();

		if (device == null) {
			self.NavigationLoading(false);
			$.ajax({
				url: "/ConnectCMS/Home/LoadNavigationItems",
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					var mappedNavItems = $.map(result, function (item) { return new NavigationItemViewModel(item); });
					self.NavigationItems(mappedNavItems);
					self.NavigationLoading(false);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});
			return;
		}

		self.SelectedDevice(device.PKID == 0 ? null : new DeviceViewModel(device));

		var placeHolder = $('#DevicePlaceholder.leftnav-deviceplaceholder');
		//var hover = $('.leftnav-devicenetwork-hover');
		var nav = $('.leftnav-navigation.container');

		if (self.SelectedDevice() == null) {
			self.NavigationLoading(false);
			$.ajax({
				url: "/ConnectCMS/Home/LoadNavigationItems",
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					var mappedNavItems = $.map(result, function (item) { return new NavigationItemViewModel(item); });
					self.NavigationItems(mappedNavItems);
					self.NavigationLoading(false);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});
		} else {
			//hover.outerHeight(placeHolder[0].clientHeight);
			nav.css('height', 'calc(100% - ' + (placeHolder[0].clientHeight + 5) + 'px)');

			$.ajax({
				url: "/ConnectCMS/Home/LoadNavigationItems",
				data: { deviceId: self.SelectedDevice().PKID() },
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					var mappedNavItems = $.map(result, function (item) { return new NavigationItemViewModel(item); });
					self.NavigationItems(mappedNavItems);
					self.NavigationLoading(false);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});

			$.ajax({
				url: "/ConnectCMS/Utility/GetDeviceTimeZone",
				data: { deviceId: self.SelectedDevice().PKID() },
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					ConnectCMS.DeviceTimeZone(result);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});

			$.ajax({
				url: "/ConnectCMS/Dashboard/GetBeacons",
				data: { deviceId: self.SelectedDevice().PKID() },
				type: "POST",
				success: function (result, textStatus, jqXHR) {
					self.AvailableBeacons($.map(result, function (item) { return { PKID: item.PKID, Name: item.Name }; }));
					self.CheckSelectedBeacon();
				},
				error: function (jqXHR, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});
		}
	};
	self.ValidateAuth = function () {
		$.ajax({
			url: '/ConnectCMS/Account/VerifyAuthentication',
			type: "POST",
			headers: {
				"IgnoreSlide": "true"
			},
			success: function (result) {
				self.AuthExpiration = moment.utc().add(result, "ms");
				var remaining = self.AuthRemaining();
				if (remaining > self.AuthMax) return;
				if (remaining == null || remaining <= 0) {
					if (self.ValidationTimeout != null) {
						window.clearTimeout(self.ValidationTimeout);
						self.ValidationTimeOut = null;
					}

					if (window.location.pathname.toLowerCase().indexOf("/connectcms/account") != 0) {
						window.location.reload(true);
					}
					return;
				} else if (remaining > self.AuthWarning) {
					if (self.ValidationTimeout != null) {
						window.clearTimeout(self.ValidationTimeout);
						self.ValidationTimeOut = null;
					}

					if (self.InactiveDialog != null) {
						self.InactiveDialog.Close();
					}

					self.ValidationTimeout = window.setTimeout(function () {
						self.ValidateAuth();
					}, remaining - self.AuthWarning);
				} else {
					if (self.ValidationTimeout != null) {
						window.clearTimeout(self.ValidationTimeout);
						self.ValidationTimeOut = null;
					}

					if (self.InactiveDialog != null) {
						self.InactiveDialog.Close();
					}

					self.ShowInactiveDialog();
					self.ValidationTimeout = window.setTimeout(function () {
						self.ValidateAuth();
					}, self.AuthWarning);
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (self.InactiveDialog != null) {
					self.InactiveDialog.Close();
				}
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function () {
			}
		});
	};

	self.ShowAboutCMS = function () {
		var dialog = new PopupViewModel({
			contentUrl: "/ConnectCMS/Popup/AboutCMS",
			title: ConnectCMS.Globalization.AboutConnectCMS + ' ( ' + ConnectCMS.Version + ' )'
		});

		dialog.Open();
	}
	self.ShowFeedback = function () {
		var dialog = new PopupViewModel({
			contentUrl: "/ConnectCMS/Popup/Feedback",
			title: ConnectCMS.Globalization.SendFeedback,
			contentViewModel: new FeedbackPopupViewModel()
		});

		dialog.Open();
	}
	self.ShowLanguages = function () {
		var dialog = new PopupViewModel({
			contentUrl: "/ConnectCMS/Popup/Languages",
			title: ConnectCMS.Globalization.SelectLanguage,
			contentViewModel: new LanguagePopupViewModel()
		});

		dialog.Open();
	};
	self.ShowInactiveDialog = function () {
		self.InactiveDialog = new PopupViewModel({
			contentUrl: "/ConnectCMS/Popup/InactiveDialog",
			headers: { "IgnoreSlide": "true" },
			title: ConnectCMS.Globalization.InactivityWarning,
			onOk: function () {
				$.ajax({
					url: '/ConnectCMS/Account/VerifyAuthentication',
					type: "POST",
					success: function (result) {
						self.AuthExpiration = moment.utc().add(result, "ms");
						var remaining = self.AuthRemaining();
						if (remaining == null || remaining <= 0) {
							if (self.ValidationTimeout != null) {
								window.clearTimeout(self.ValidationTimeout);
								self.ValidationTimeOut = null;
							}

							if (window.location.pathname.toLowerCase().indexOf("/connectcms/account") != 0) {
								window.location.reload(true);
							}
							return;
						} else if (remaining > self.AuthWarning) {
							if (self.ValidationTimeout != null) {
								window.clearTimeout(self.ValidationTimeout);
								self.ValidationTimeOut = null;
							}

							self.ValidationTimeout = window.setTimeout(function () {
								self.ValidateAuth();
							}, remaining - self.AuthWarning);
						} else {
							if (self.ValidationTimeout != null) {
								window.clearTimeout(self.ValidationTimeout);
								self.ValidationTimeOut = null;
							}

							self.ValidationTimeout = window.setTimeout(function () {
								self.ValidateAuth();
							}, self.AuthWarning);
						}
					},
					error: function (jqXHR, textStatus, errorThrown) {
						if (self.InactiveDialog != null) {
							self.InactiveDialog.Close();
						}
						ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
					},
					complete: function () {
					}
				});
				return true;
			}
		});

		self.InactiveDialog.Open();
	}

	//Until DeviceManager is in MVC
	//$('.content.container').css('overflow', 'hidden');
	self.ValidateAuth();
	self.UpdateCurrentDevice();
};

$(document).ready(function () {
	ConnectCMS.MainViewModel = new MainViewModel();

	ko.validation.init({
		insertMessages: true,
		decorateElement: true,
		parseInputAttributes: true,
		messagesOnModified: true,
		grouping: { deep: true, observable: true, live: true },
		errorClass: 'Error'
	});

	$.extend(ko.validation.rules.required, {
		message: 'Required',
	});

	$.extend(ko.validation, {
		insertValidationMessage: function (element) {
			var span = document.createElement('Label');
			span.className = this.utils.getConfigOptions(element).errorMessageClass;
			$(span).attr('for', element.id);
			this.utils.insertAfter(element, span);

			return span;
		},
	});

	ko.applyBindings(ConnectCMS.MainViewModel, $('html')[0]);

	if (ConnectCMS.Browser != null) {
		switch (ConnectCMS.Browser) {
			case 'InternetExplorer':
				if (ConnectCMS.BrowserVersion < 10)
					ConnectCMS.MainViewModel.ShowUnsupportedBrowser(true);
				break;
			case 'Chrome':
				if (ConnectCMS.BrowserVersion < 36)
					ConnectCMS.MainViewModel.ShowUnsupportedBrowser(true);
				break;
			case 'Safari':
				if (ConnectCMS.BrowserVersion < 7)
					ConnectCMS.MainViewModel.ShowUnsupportedBrowser(true);
				break;
			case 'Firefox':
				if (ConnectCMS.BrowserVersion < 31)
					ConnectCMS.MainViewModel.ShowUnsupportedBrowser(true);
				break;
			case 'Opera':
				if (ConnectCMS.BrowserVersion < 23)
					ConnectCMS.MainViewModel.ShowUnsupportedBrowser(true);
				break;
			default:
				ConnectCMS.MainViewModel.ShowUnsupportedBrowser(true);
				break;
		}
	}
});

function getCookie(cname) {
	var name = cname + "=";
	var ca = document.cookie.split(';');
	for (var i = 0; i < ca.length; i++) {
		var c = ca[i];
		while (c.charAt(0) == ' ') c = c.substring(1);
		if (c.indexOf(name) != -1) return c.substring(name.length, c.length);
	}
	return "";
}