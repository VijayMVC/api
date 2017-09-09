ConnectCMS.BingMap = {
	CenterMap: function (map, latitude, longitude) {
		if (map != null) {
			if (latitude != null && longitude != null) {
				var location = new Microsoft.Maps.Location(latitude, longitude);

				var viewOptions = {
					center: location
				};

				map.setView(viewOptions);
			}
		}
	},
	ClickCircle: function (enterpriseId, enterpriseLocationId) {
		var jElementCircle = ConnectCMS.BingMap.GetEnterpriseCircleElement(enterpriseId, enterpriseLocationId);

		var jElementSelectedCircle = $(".bingMapPin.selected, .listItemMapPinCircle.selected");

		jElementSelectedCircle.removeClass("selected");

		jElementCircle.removeClass("hover");
		jElementCircle.addClass("selected");
	},
	ClickMapCircle: function (e) {
		var ids = ConnectCMS.BingMap.ParseTypeName(e);

		if (ids != null) {
			var location = e.target.getLocation();

			if (location != null) {
				var map = ConnectCMS.BingMap.GetMapElementFromEnterprisePinIds(ids);

				ConnectCMS.BingMap.CenterMap(map, location.latitude, location.longitude);
			}

			ConnectCMS.BingMap.ClickCircle(ids.enterpriseId, ids.enterpriseLocationId);
		}
	},
	DrawCiricle: function (map, latitude, longitude, radius) {
		if (map == null)
			return;

		// FH: Earth's mean radius in km
		var averageRadius = 6371;

		// FH: d = angular distance covered on earth's surface
		var circleDistance = parseFloat(radius) / averageRadius;
		var circleLatitude = (latitude * Math.PI) / 180;
		var circleLongitude = (longitude * Math.PI) / 180;
		var circlePoints = new Array();
		var degree;
		var location;

		for (var index = 0; index <= 360; index++) {
			degree = index * Math.PI / 180;
			location = new Microsoft.Maps.Location(0, 0);

			location.latitude = Math.asin(Math.sin(circleLatitude) * Math.cos(circleDistance) + Math.cos(circleLatitude) * Math.sin(circleDistance) * Math.cos(degree));
			location.longitude = ((circleLongitude + Math.atan2(Math.sin(degree) * Math.sin(circleDistance) * Math.cos(circleLatitude), Math.cos(circleDistance) - Math.sin(circleLatitude) * Math.sin(location.latitude))) * 180) / Math.PI;
			location.latitude = (location.latitude * 180) / Math.PI;

			circlePoints.push(location);
		}

		var fillColor = new Microsoft.Maps.Color(30, 178, 234, 249);
		var strokeColor = new Microsoft.Maps.Color(30, 0, 156, 216);

		var polygonOptions = {
			fillColor: fillColor,
			strokeColor: strokeColor
		};

		var polygon = new Microsoft.Maps.Polygon(circlePoints, polygonOptions);

		map.entities.push(polygon);
	},
	GetEnterpriseCircleElement: function (enterpriseId, enterpriseLocationId) {
		var jElementCircle;

		if (enterpriseLocationId == 0)
			jElementCircle = $(".enterprise" + enterpriseId);
		else
			jElementCircle = $(".enterpriseLocation" + enterpriseLocationId);

		return jElementCircle;
	},
	GetMapElementFromElement: function (element) {
		var map = ko.utils.domData.get(element, element.id);

		return map;
	},
	GetMapElementFromEnterprisePinIds: function (ids) {
		var element = $(".bingMapPin, .enterprise" + ids.enterpriseId + ", .enterpriseLocation" + ids.enterpriseLocationId);

		var parent = element.parents(".bingMap:first")[0]

		var map = ConnectCMS.BingMap.GetMapElementFromElement(parent);

		return map;
	},
	HoverCircle: function (enterpriseId, enterpriseLocationId) {
		var jElementCircle = ConnectCMS.BingMap.GetEnterpriseCircleElement(enterpriseId, 0);

		if (!jElementCircle.hasClass("selected") && !jElementCircle.hasClass(".enterpriseLocation" + enterpriseLocationId))
			jElementCircle.addClass("groupHover");

		var jElementCircle = ConnectCMS.BingMap.GetEnterpriseCircleElement(enterpriseId, enterpriseLocationId);

		if (!jElementCircle.hasClass("selected"))
			jElementCircle.addClass("hover");
	},
	MouseEnterMapCircle: function (e) {
		var ids = ConnectCMS.BingMap.ParseTypeName(e);

		if (ids != null)
			ConnectCMS.BingMap.HoverCircle(ids.enterpriseId, ids.enterpriseLocationId);
	},
	MouseLeaveMapCircle: function (e) {
		var ids = ConnectCMS.BingMap.ParseTypeName(e);

		if (ids != null)
			ConnectCMS.BingMap.NormalCircle(ids.enterpriseId, ids.enterpriseLocationId);
	},
	NormalCircle: function (enterpriseId, enterpriseLocationId) {
		var jElementCircle = ConnectCMS.BingMap.GetEnterpriseCircleElement(enterpriseId, 0);

		if (!jElementCircle.hasClass("selected"))
			jElementCircle.removeClass("groupHover");

		var jElementCircle = ConnectCMS.BingMap.GetEnterpriseCircleElement(enterpriseId, enterpriseLocationId);

		if (!jElementCircle.hasClass("selected"))
			jElementCircle.removeClass("hover");
	},
	ParseTypeName: function (e) {
		var result = null;

		if (e.target != undefined && e.target.getTypeName() != undefined) {
			var typeName = e.target.getTypeName();

			typeName = typeName.substr(typeName.indexOf(" ") + 1);

			var spaceIndex = typeName.indexOf(" ");

			var enterpriseClassName = typeName.substr(0, spaceIndex);
			var enterpriseLocationClassName = typeName.substr(spaceIndex + 2);

			var enterpriseId = enterpriseClassName.replace("enterprise", "");
			var enterpriseLocationId = enterpriseLocationClassName.replace("enterpriseLocation", "");

			result = {
				enterpriseId: enterpriseId,
				enterpriseLocationId: enterpriseLocationId
			}
		}

		return result;
	}
};

ko.bindingHandlers.bingMap = {
	update: function (element, valueAccessor) {
		var mapViewModel = ko.utils.unwrapObservable(valueAccessor());

		if (mapViewModel == null || mapViewModel.Credentials() == null)
			return;

		var map = ConnectCMS.BingMap.GetMapElementFromElement(element);

		if (map == null) {
			var mapOptions = {
				credentials: mapViewModel.Credentials(),
				disablePanning: mapViewModel.DisablePanning,
				disableZooming: mapViewModel.DisableZooming,
				enableSearchLogo: false,
				mapTypeId: Microsoft.Maps.MapTypeId.road,
				showCopyright: false,
				showDashboard: mapViewModel.ShowDashboard,
				zoom: 13
			}

			var map = new Microsoft.Maps.Map(element, mapOptions);

			ko.utils.domData.set(element, element.id, map);
		} else {
			var mapElement = $(element);
			var mapOptions = map.getOptions();

			mapOptions.height = mapElement.height();
			mapOptions.width = mapElement.width();

			map.setOptions(mapOptions);
		}

		map.entities.clear();

		var centerLatitude = mapViewModel.CenterLatitude;
		var centerLongitude = mapViewModel.CenterLongitude;

		if (centerLatitude != null && centerLongitude != null) {
			ConnectCMS.BingMap.CenterMap(map, centerLatitude, centerLongitude);
		}

		var location;
		var pushpin;
		var pushpinOptions = {
			height: 40,
			icon: "",
			typeName: "bingMapHotelPin",
			width: 40
		};

		if (mapViewModel.CenterRadius != null)
			ConnectCMS.BingMap.DrawCiricle(map, mapViewModel.CenterLatitude, mapViewModel.CenterLongitude, mapViewModel.CenterRadius);

		var locationViewModel;

		ko.utils.arrayForEach(mapViewModel.HotelPins(), function (hotelViewModel) {
			locationViewModel = hotelViewModel.HotelDetail().Location();

			if (locationViewModel != null) {
				location = new Microsoft.Maps.Location(hotelViewModel.HotelDetail().Location().Latitude(), hotelViewModel.HotelDetail().Location().Longitude());

				pushpin = new Microsoft.Maps.Pushpin(location, pushpinOptions);

				map.entities.push(pushpin);
			}
		});

		var enterpriseClass;
		var enterpriseLocationClass;
		var hoverClass = mapViewModel.HoverClass;
		var locationPinClass = mapViewModel.LocationPinClass;
		var mapId;
		var selectedClass = mapViewModel.SelectedClass;
		var typeName;

		if (!ConnectCMS.Strings.IsNullOrWhitespace(locationPinClass))
			pushpinOptions = {
				height: 30,
				icon: "",
				textOffset: new Microsoft.Maps.Point(-1, 5),
				typeName: "",
				width: 30
			};
		else
			pushpinOptions = {
			};

		ko.utils.arrayForEach(mapViewModel.LocationPins(), function (enterpriseLocationViewModel) {
			locationViewModel = enterpriseLocationViewModel.Location();

			if (locationViewModel != null) {
				if (enterpriseLocationViewModel.MapId != null)
					mapId = enterpriseLocationViewModel.MapId();

				typeName = "";

				if (!ConnectCMS.Strings.IsNullOrWhitespace(locationPinClass)) {
					if (!ConnectCMS.Strings.IsNullOrWhitespace(typeName))
						typeName += " ";

					typeName += locationPinClass;

					enterpriseClass = "";
					enterpriseLocationClass = "enterpriseLocation";

					if (enterpriseLocationViewModel.FKEnterprise() != null && enterpriseLocationViewModel.FKEnterprise() != 0) {
						enterpriseClass = "enterprise" + enterpriseLocationViewModel.FKEnterprise();
						enterpriseLocationClass += enterpriseLocationViewModel.PKID();
					} else
						enterpriseLocationClass += enterpriseLocationViewModel.BingId();

					if (!ConnectCMS.Strings.IsNullOrWhitespace(enterpriseClass)) {
						if (!ConnectCMS.Strings.IsNullOrWhitespace(typeName))
							typeName += " ";

						typeName += enterpriseClass;
					}

					if (!ConnectCMS.Strings.IsNullOrWhitespace(typeName))
						typeName += " ";

					typeName += enterpriseLocationClass;

					if (enterpriseLocationViewModel.Hover != null && enterpriseLocationViewModel.Hover()) {
						if (!ConnectCMS.Strings.IsNullOrWhitespace(typeName))
							typeName += " ";

						typeName += "hover";
					}

					if (enterpriseLocationViewModel.Selected != null && enterpriseLocationViewModel.Selected()) {
						if (!ConnectCMS.Strings.IsNullOrWhitespace(typeName))
							typeName += " ";

						typeName += "selected";
					}
				}

				location = new Microsoft.Maps.Location(locationViewModel.Latitude(), locationViewModel.Longitude());

				pushpinOptions.draggable = mapViewModel.LocationPinDraggable;

				if (!ConnectCMS.Strings.IsNullOrWhitespace(mapId))
					pushpinOptions.text = mapId.toString();

				if (!ConnectCMS.Strings.IsNullOrWhitespace(typeName))
					pushpinOptions.typeName = typeName;

				pushpin = new Microsoft.Maps.Pushpin(location, pushpinOptions);

				Microsoft.Maps.Events.addHandler(pushpin, 'click', ConnectCMS.BingMap.ClickMapCircle);
				Microsoft.Maps.Events.addHandler(pushpin, 'mouseout', ConnectCMS.BingMap.MouseLeaveMapCircle);
				Microsoft.Maps.Events.addHandler(pushpin, 'mouseover', ConnectCMS.BingMap.MouseEnterMapCircle);

				map.entities.push(pushpin);
			}
		});
	}
};