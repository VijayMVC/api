function RecommendationEnterpriseEditViewModel(recommendationEnterpriseViewModel) {
	var self = this;

	self.RecommendationEnterpriseViewModel = recommendationEnterpriseViewModel;

	self.CountriesWithPhoneNumber = ko.observableArray(null);
	self.Edit = ko.observable(false);
	self.LocateExecuting = ko.observable(false);
	self.MapVisible = ko.observable(false);
	self.SelectedPhoneCountry = ko.observable(null);
	self.States = ko.observableArray(null);

	self.CancelVisible = ko.computed(function () {
		var result = true;

		var navigationType = self.RecommendationEnterpriseViewModel.NavigationType();

		if (navigationType != null && navigationType == RecommendationEnterpriseNavigationType.ADD)
			result = false;

		return result;
	});
	self.EditMapVisible = ko.computed(function () {
		var result = false;

		var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

		if (enterpriseViewModel != null) {
			var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

			if (firstEnterpriseLocationViewModel != null) {
				var locationViewModel = firstEnterpriseLocationViewModel.Location();

				if (locationViewModel != null) {
					if (locationViewModel.Latitude() != null && locationViewModel.Longitude() != null)
						result = true;
				}
			}
		}

		return result;
	});
	self.ImageNoDataVisible = ko.computed(function () {
		var result = true;

		var enterprise = self.RecommendationEnterpriseViewModel.Enterprise();

		if (enterprise != null && !ConnectCMS.IsNullOrEmpty(enterprise.Images))
			result = false;

		return result;
	});
	self.LocateEnabled = ko.computed(function () {
		var result = false;

		var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

		if (enterpriseViewModel != null) {
			var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

			if (firstEnterpriseLocationViewModel != null) {
				var locationViewModel = firstEnterpriseLocationViewModel.Location;

				if (locationViewModel() != null)
					result = !self.LocateExecuting() && locationViewModel.IsValid();
			}
		}

		return result;
	});
	self.MapText = ko.computed(function () {
		var result = ConnectCMS.Globalization.Locate;

		if (self.LocateExecuting())
			result = ConnectCMS.Globalization.Locating;
		else {
			var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

			if (enterpriseViewModel != null) {
				var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

				if (firstEnterpriseLocationViewModel != null) {
					var locationViewModel = firstEnterpriseLocationViewModel.Location();

					if (locationViewModel != null) {
						if (locationViewModel.Latitude() != null && locationViewModel.Longitude() != null)
							result = ConnectCMS.Globalization.Relocate;
					}
				}
			}
		}

		return result;
	});
	self.NoEditMapVisible = ko.computed(function () {
		var result = false;

		var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

		if (enterpriseViewModel != null) {
			var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

			if (firstEnterpriseLocationViewModel != null) {
				var locationViewModel = firstEnterpriseLocationViewModel.Location();

				if (locationViewModel != null) {
					if (locationViewModel.Latitude() != null && locationViewModel.Longitude() != null)
						result = true;
				}
			}
		}

		return result;
	});
	self.SelectedCountryId = ko.computed({
		read: function () {
			var result = null;

			var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

			if (enterpriseViewModel != null) {
				var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

				if (firstEnterpriseLocationViewModel != null) {
					var locationViewModel = firstEnterpriseLocationViewModel.Location();

					if (locationViewModel != null)
						result = locationViewModel.FKCountry();
				}
			}

			return result;
		},
		write: function (value) {
			var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

			if (enterpriseViewModel != null) {
				var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

				if (firstEnterpriseLocationViewModel != null) {
					var locationViewModel = firstEnterpriseLocationViewModel.Location();

					if (locationViewModel != null) {
						locationViewModel.FKCountry(value);

						var country = locationViewModel.Country();
						var phoneISOCountryCode = firstEnterpriseLocationViewModel.PhoneISOCountryCode;

						if (phoneISOCountryCode() == null && country != null && value != null)
							phoneISOCountryCode(country.ISOCountryCode);
					}
				}
			}
		}
	}).extend({
		required: true
	});

	self.ImageNoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoImagesWereFound, null, self.ImageNoDataVisible);

	self.ClickAddImage = function () {
		ConnectCMS.ShowImageGallery({
			imageConstraints: {
				allowedFormats: ['jpg', 'png'],
				minHeight: 150,
				minWidth: 150
			},
			selection: 'single',
			insert: self.OnInsertImage
		});
	};
	self.ClickCancel = function () {
		self.Edit(false);

		var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

		//if (enterpriseViewModel)
		//enterpriseViewModel.revertData(true);
	};
	self.ClickEdit = function () {
		self.Edit(true);
	};
	self.ClickLocate = function () {
		if (!self.LocateEnabled())
			return;

		var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

		if (enterpriseViewModel != null) {
			var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

			if (firstEnterpriseLocationViewModel != null) {
				var locationViewModel = firstEnterpriseLocationViewModel.Location();

				if (locationViewModel != null) {
					self.LocateExecuting(true);

					var country = locationViewModel.Country();
					var state = locationViewModel.State();

					$.ajax({
						url: "/ConnectCMS/Bing/GetBingAddressGeocode",
						data: {
							address1: locationViewModel.Address1(),
							city: locationViewModel.City(),
							state: state != null ? state.ISOStateCode : null,
							postalCode: locationViewModel.PostalCode(),
							country: country != null ? country.ISOCountryCode : null
						},
						type: "POST",
						success: function (result, textStatus, jqXHR) {
							locationViewModel.Latitude(result.Latitude);
							locationViewModel.Longitude(result.Longitude);
						},
						error: function (jqXHR, textStatus, errorThrown) {
							self.RecommendationEnterpriseViewModel.ErrorVisible(true);
							ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
						},
						complete: function (jqXHR, textStatus) {
							self.LocateExecuting(false);
						}
					});
				}
			}
		}
	};
	self.ClickReorderImage = function () {
		ConnectCMS.ShowPleaseWait();

		$.ajax({
			url: "/ConnectCMS/Image/Reorder",
			type: "POST",
			success: function (result) {
				NavigateOpen(self.RecommendationEnterpriseTipAddViewModel, self.RecommendationEnterpriseViewModel.SelectedContentIndex, self.RecommendationEnterpriseViewModel.NavigationElement, result);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.RecommendationEnterpriseViewModel.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				ConnectCMS.HidePleaseWait();
			}
		});
	};
	self.ClickUpdate = function () {
		var recommendationEnterpriseViewModel = self.RecommendationEnterpriseViewModel;

		if (recommendationEnterpriseViewModel != null) {
			var enterpriseViewModel = recommendationEnterpriseViewModel.Enterprise;

			if (enterpriseViewModel != null && enterpriseViewModel.isValid()) {
				ConnectCMS.ShowPleaseWait();

				enterpriseViewModel = enterpriseViewModel();

				var enterpriseLocationViewModel;

				if (enterpriseViewModel.EnterpriseLocationsLength() == 1)
					enterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

				if (enterpriseViewModel.HasChanges()) {
					var data = {
						deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
						enterprise: ConnectCMS.Framework.toJS(enterpriseViewModel),
						enterpriseLocation: enterpriseLocationViewModel == null ? null : ConnectCMS.Framework.toJS(enterpriseLocationViewModel)
					};

					$.ajax({
						url: "/ConnectCMS/Enterprise/UpdateEnterprise",
						type: "POST",
						contentType: 'application/json; charset=utf-8;',
						dataType: 'json',
						data: JSON.stringify(data),
						success: function (result, textStatus, jqXHR) {
							self.ClickVerify();
						},
						error: function (jqXHR, textStatus, errorThrown) {
							self.RecommendationEnterpriseViewModel.ErrorVisible(true);
							ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
						},
						complete: function (jqXHR, textStatus) {
							ConnectCMS.HidePleaseWait();
						}
					});
				} else
					ConnectCMS.HidePleaseWait();
			}
		}
	};
	self.ClickVerify = function () {
		self.RecommendationEnterpriseViewModel.Verified(true);
	};
	self.GetCountriesWithPhoneNumber = function () {
		$.ajax({
			url: "/ConnectCMS/Utility/GetCountriesWithPhoneNumber",
			data: {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
			},
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				self.CountriesWithPhoneNumber($.map(result, function (countryModel) {
					return new ConnectCMS.Locations.CountryViewModel(countryModel);
				}));
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.RecommendationEnterpriseViewModel.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			}
		});
	};
	self.GetMapViewModel = function (mapViewModel) {
		if (mapViewModel != null) {
			var hotelViewModel = self.RecommendationEnterpriseViewModel.RecommendationViewModel.Hotel();
			var locationViewModel;

			if (hotelViewModel != null)
				mapViewModel.AddHotelPin(hotelViewModel);

			var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

			if (enterpriseViewModel != null) {
				var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

				if (firstEnterpriseLocationViewModel != null) {
					mapViewModel.AddLocationPin(firstEnterpriseLocationViewModel);

					locationViewModel = firstEnterpriseLocationViewModel.Location();

					if (locationViewModel != null)
						mapViewModel.SetCenter(locationViewModel.Latitude(), locationViewModel.Longitude());
				}
			}
		}

		return mapViewModel;
	};
	self.OnGetPostalCodeSuccess = function (result, textStatus, jqXHR) {
		var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();
		var postalCode = new PostalCodeViewModel(result);

		if (enterpriseViewModel != null && postalCode != null) {
			var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

			if (firstEnterpriseLocationViewModel != null) {
				var locationViewModel = firstEnterpriseLocationViewModel.Location();

				if (locationViewModel != null && locationViewModel.State() == null) {
					var state = postalCode.State;

					if (state != null)
						state = ko.utils.arrayFirst(locationViewModel.AvailableStates(), function (stateViewModel) {
							return state.PKID == stateViewModel.PKID;
						});

					if (state != null)
						locationViewModel.State(state);
				}
			}
		}
	};
	self.OnInsertImage = function (image) {
		// TODO: FH: Insert Image Click, 720 for Recommended Gallery
		//		if (image.length > 0)
		//			self.Logo(new ImageViewModel(image[0]._Data));
	};

	self.EditMap = ko.computed(function () {
		var result = new MapViewModel(self.RecommendationEnterpriseViewModel.RecommendationViewModel.BingMapCredentials);

		result.HotelPinClass = "bingMapHotelPin";
		result.LocationPinClass = "bingMapPin enterprise";
		result.LocationPinDraggable = true;

		result = self.GetMapViewModel(result);

		return result;
	});
	self.NoEditMap = ko.computed(function () {
		var result = new MapViewModel(self.RecommendationEnterpriseViewModel.RecommendationViewModel.BingMapCredentials);

		result.DisablePanning = true;
		result.HotelPinClass = "bingMapHotelPin";
		result.LocationPinClass = "bingMapPin enterprise";
		result.ShowDashboard = false;

		result = self.GetMapViewModel(result);

		return result;
	});

	self.GetCountriesWithPhoneNumber();

	var enterpriseViewModel = self.RecommendationEnterpriseViewModel.Enterprise();

	if (enterpriseViewModel != null) {
		var firstEnterpriseLocationViewModel = enterpriseViewModel.FirstEnterpriseLocation();

		if (firstEnterpriseLocationViewModel != null) {
			var locationViewModel = firstEnterpriseLocationViewModel.Location();

			if (locationViewModel != null) {
				var countryId;
				var countryViewModel = locationViewModel.Country();

				if (countryViewModel != null && countryViewModel.PKID() != null)
					countryId = countryViewModel.PKID();
				else {
					var availableCountries = locationViewModel.AvailableCountries();

					if (!ConnectCMS.IsNullOrEmpty(availableCountries)) {
						var firstCountryViewModel = ko.utils.arrayFirst(availableCountries, function (countryViewModel) {
							return true;
						});

						if (firstCountryViewModel != null)
							countryId = firstCountryViewModel.PKID();
					}
				}

				if (countryId != null)
					self.SelectedCountryId(countryId);

				locationViewModel.OnGetPostalCodeSuccess = self.OnGetPostalCodeSuccess;
			}
		}
	}
};