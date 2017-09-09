//Merge me with something better!

ko.bindingHandlers.visibleAnimated = {
	init: function(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
		var value = ko.utils.unwrapObservable(valueAccessor());

		var $element = $(element);

		if (value)
			$element.show();
		else
			$element.hide();
	},
	update: function(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
		var value = ko.utils.unwrapObservable(valueAccessor());

		var $element = $(element);


		var allBindings = allBindingsAccessor();

		// Grab data from binding property
		var duration = allBindings.duration || 500;
		var toggle = allBindings.toggle || null;
		var isCurrentlyVisible = !(element.style.display == "none");

		if (toggle != null && ((value && !isCurrentlyVisible) || ((!value) && isCurrentlyVisible))) {
			switch(toggle) {
				case 'slide':
					$element.slideToggle();
					break;
				case 'slideLeft':
					if (value) {
						$element.show("slide", { direction: "left" }, 1000);
					} else {
						$element.hide("slide", { direction: "right" }, 1000);
					}
					break;
			}
		} else {
			if (value && !isCurrentlyVisible)
				$element.show(duration);
			else if ((!value) && isCurrentlyVisible)
				$element.hide(duration);
		}
	}
};


ko.bindingHandlers.prop = {
	init: function (element, valueAccessor, allBindings, viewModel) {
		var value = ko.unwrap(valueAccessor());
		for (var prop in value) {
			if (value.hasOwnProperty(prop)) {
				$(element).prop(prop, value[prop]);
			}
		}
	},
	update: function (element, valueAccessor, allBindings, viewModel) {
		var value = ko.unwrap(valueAccessor());
		for (var prop in value) {
			if (value.hasOwnProperty(prop)) {
				$(element).prop(prop, value[prop]);
			}
		}
	}
}

ko.bindingHandlers.monsciergeMenu = {
	init: function (element, valueAccessor) {
		var value = ko.unwrap(valueAccessor());
		var menuSelector = ko.unwrap(value.menuSelector);
		var options = { menuSelector: menuSelector };
		var minWidth = ko.unwrap(value.minWidth);
		if (minWidth != null) options.minWidth = minWidth;
		$(element).monscierge_menu(options); // Use "unwrapObservable" so we can handle values that may or may not be observable
	}
};

ko.bindingHandlers.monsciergeHoverOverlay = {
	init: function (element, valueAccessor, allBindings, viewModel) {
		var value = ko.unwrap(valueAccessor());
		var overlaySelector = ko.unwrap(value.overlaySelector);
		var options = { overlaySelector: overlaySelector };

		var sensor = new ResizeSensor(element, function () {
			var plugin = $(element).data('monsciergeHoverOverlay');
			if (plugin != null) plugin.refresh();
		});

		//var minWidth = ko.unwrap(value.minWidth);
		//if (minWidth != null) options.minWidth = minWidth;
		$(element).monscierge_hoverOverlay(options); // Use "unwrapObservable" so we can handle values that may or may not be observable
	},
	update: function (element, valueAccessor) {
		var value = ko.unwrap(valueAccessor());
		var overlaySelector = ko.unwrap(value.overlaySelector);
		var options = { overlaySelector: overlaySelector };
		var plugin = $(element).data('monsciergeHoverOverlay');
		if (plugin != null) plugin.refresh();
	}
};

ko.bindingHandlers.monsciergeTooltip = {
	init: function (element, valueAccessor) {
		var value = ko.unwrap(valueAccessor());
		if (value == true || value == false) {
			$(element).tooltip();
			return;
		}

		var options = {};

		var title = ko.unwrap(value.title);
		if (title != null) options.title = title;

		var placement = ko.unwrap(value.placement);
		if (placement != null) options.placement = placement;

		$(element).tooltip(options);
	}
};

// Note: this binding has a dependency on moment.js for date/time manipulations.
ko.bindingHandlers.timeAgo = {
	update: function (element, valueAccessor, allBindings) {
		var val = ko.unwrap(valueAccessor()),
			dateAsMoment = moment(val);

		var hideAgoText = allBindings.get('hideTimeAgoText') || false;

		var timeAgo = dateAsMoment.from(moment(), hideAgoText);

		return ko.bindingHandlers.html.update(element, function () {
			return '<time datetime="' + encodeURIComponent(dateAsMoment.toDate()) + '">' + timeAgo + '</time>';
		});
	}
};

ko.bindingHandlers.hideTimeAgoText = {
	init: function (element, valueAccessor) {
		return ko.unwrap(valueAccessor()) || false;
	}
}

ko.bindingHandlers['readonly'] = {
	'update': function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());
		if (value && element.readonly)
			element.removeAttribute("readonly");
		else if ((!value) && (!element.readonly))
			element.readonly = true;
	}
};