var ConnectCMS = ConnectCMS || {};
ConnectCMS.Dashboard = ConnectCMS.Dashboard || {};
ConnectCMS.Globalization = ConnectCMS.Globalization || {};

ConnectCMS.Dashboard.Api = function () {
	var scheme = window.location.protocol,
		host = window.location.host,

		createApiUrl = function (apiMethod) {
			var urlParts = new Array(host, 'ConnectCMS', apiMethod);
			return scheme + '//' + urlParts.join('/');
		},

		getJsonData = function (apiMethod, params, callback) {
			var apiUrl = createApiUrl(apiMethod);
			$.ajax({
				url: apiUrl,
				type: 'GET',
				data: params,
				contentType: 'application/json',
				dataType: 'json',
				success: function (result) {
					if (callback) {
						callback(result);
					}
				},
				error: function (jqXhr, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXhr, textStatus, errorThrown);
				}
			});
		};

	return {
		getJsonData: getJsonData
	};
};

ConnectCMS.Dashboard.Stat = function (title, range, isOptional, dataUrl, dataQueryParams) {
	var self = this;
	self.title = title || '';
	self.range = range || '';
	self.value = ko.observable(0);
	self.isOptional = isOptional || false;
	self.dataUrlField = dataUrl;
	self.dataQueryParams = dataQueryParams;

	self.statClass = ko.computed(function () {
		return (this.isOptional ? 'optional-stat' : 'standard-stat');
	}, self);

	self.loadData = function () {
		if (dataUrl) {
			var api = new ConnectCMS.Dashboard.Api();
			api.getJsonData(dataUrl, dataQueryParams, function (json) {
				if (json) {
					self.value(json.count || 0);
				}
			});
		}
	};

	self.loadData();
};

ConnectCMS.Dashboard.Event = function (id, title, location, startDateTime) {
	var self = this;
	self.id = id || 0;
	self.title = title || '';
	self.location = location || '';
	self.startDateTime = startDateTime || '';

	self.navigateUrl = 'Events/Index/' + self.id;
	self.isSilverlight = false;

	self.startTimeText = ko.computed(function () {
		if (self.startDateTime === '') return '';

		return moment(startDateTime).format('LT');
	});
};

ConnectCMS.Dashboard.QuickLink = function (title, navigateUrl, isSilverlight, iconClass) {
	var self = this;
	self.title = title || '';
	self.navigateUrl = navigateUrl;
	self.isSilverlight = isSilverlight || false;
	self.iconClass = iconClass;
};

ConnectCMS.Dashboard.DashboardViewModel = function (deviceId) {
	var self = this;
	self.deviceId = deviceId;

	self.stats = ko.observableArray([
		new ConnectCMS.Dashboard.Stat('Guest Interactions', 'Today', false, '/Dashboard/GetGuestInteractionTodayCount', { deviceId: self.deviceId }),
		new ConnectCMS.Dashboard.Stat('Guest Interactions', '10-Day Avg', false, '/Dashboard/GetGuestInteraction10DayAvgCount', { deviceId: self.deviceId }),
		new ConnectCMS.Dashboard.Stat('Recommendations', 'Current', false, '/Dashboard/GetRecommendationCount', { deviceId: self.deviceId }),
		new ConnectCMS.Dashboard.Stat('Requests', 'Today', true, '/Dashboard/GetTodaysRequestCount', { deviceId: self.deviceId }),
		new ConnectCMS.Dashboard.Stat('In-Room Dining', 'Today', true, '/Dashboard/GetTodaysInRoomDiningCount', { deviceId: self.deviceId }),
		new ConnectCMS.Dashboard.Stat('Postcards', 'Today', true, '/Dashboard/GetTodaysPostcardCount', { deviceId: self.deviceId }),
		new ConnectCMS.Dashboard.Stat('Events', 'Tomorrow', true, '/Dashboard/GetTomorrowsEventCount', { deviceId: self.deviceId })
	]);

	self.events = ko.observableArray([]);

	self.quickLinks = ko.observableArray([
		new ConnectCMS.Dashboard.QuickLink('Manage or Add Amenities', 'Amenities', true, 'icon-add_amenity'),
		new ConnectCMS.Dashboard.QuickLink('Make a Recommendation', 'Recommendation/Index', false, 'icon-make_recommendation'),
		new ConnectCMS.Dashboard.QuickLink('Add a New Event', 'Events/Index', false, 'icon-new_event'),
		new ConnectCMS.Dashboard.QuickLink('Manage Requests', 'Request/Index', false, 'icon-manage_requests')
	]);

	self.hasEvents = ko.computed(function () {
		return self.events().length > 0;
	});

	self.navigate = function (link) {
		var pageNav = new ConnectCMS.Utilities.PageNavigation();
		pageNav.navigate(link.navigateUrl, link.isSilverlight);
	};

	self.init = function () {
		// load events
		var api = new ConnectCMS.Dashboard.Api();
		api.getJsonData('/Dashboard/GetTodaysDashboardEvents', { deviceId: self.deviceId }, function (json) {
			if (json) {
				var mappedEvents = ko.utils.arrayMap(json, function (event) {
					return new ConnectCMS.Dashboard.Event(event.id, event.name, event.location, event.startDateTime);
				});

				self.events(mappedEvents);
			}
		});
	};

	self.init();
};