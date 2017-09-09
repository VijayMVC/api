(function ($) {
	$.textLines = function (element, options) {
		this.options = {};

		element.data('textLines', this);

		this.init = function (initElement, initOptions) {
			this.options = $.extend({}, $.textLines.defaultOptions, initOptions);

			//Call private function
			setup(element[0], this.options);
		};

		//Public function
		this.refresh = function () {
			var elem = element[0];
			var bg = null;
			var p = elem;
			while (bg == null && p != null) {
				var temp = $(p).css('background-color');
				if (temp != 'transparent') bg = temp;
				p = p.parentElement;
			}

			element.css("height", "auto");
			var endHeight = element.height();

			var lHeight = $(element).css('line-height');
			var lHeightValue = lHeight.substr(0, lHeight.length - 2);
			var lHeightUnit = lHeight.substr(lHeight.length - 2, lHeight.length - 1);
			var height = (lHeightValue) * options.lines;

			if (endHeight <= height) {
				element.find('.ellipsisDiv').hide();
			} else {
				if (element.find('.ellipsisDiv').length === 0) {
					$('<div/>', {
						'class': 'ellipsisDiv',
						text: ' ...',
						style: 'background-Image: linear-gradient(to right, rgba(0,0,0,0) 0%, ' + bg + ' 25%, ' + bg + ' 100%); font-weight: bold; position: absolute; bottom: 0;right: 0; padding: 0 5px 5px 20px;',
					}).appendTo(element);
				} else {
					element.find('.ellipsisDiv').css('background-Image', 'linear-gradient(to right, rgba(0,0,0,0) 0%, ' + bg + ' 25%, ' + bg + ' 100%)');
				}
				element.find('.ellipsisDiv').show();
				element.css('height', height + lHeightUnit);
			}
		};

		this.init(element, options);
	};

	$.fn.textLines = function (options) {
		return this.each(function () {
			return new $.textLines($(this), options);
		});
	};

	var timeout_id;

	//Private function
	function setup(element) {
		$(element).addClass('textLines');
		var bg = null;
		var p = element;
		while (bg == null && p != null) {
			var temp = $(p).css('background-color');
			if (temp != 'transparent') bg = temp;
			p = p.parentElement;
		}

		var elem = $(element);
		elem.data('textLines-Initials', {
			w: elem.width(),
			h: elem.height(),
			bg: bg
		});

		if ($('.textLines').length === 1) {
			startLoop();
		}

		$(element).css('overflow', 'hidden');
		$(element).css('padding-bottom', '1px');
	};

	function startLoop() {
		timeout_id = window['setTimeout'](function () {
			if ($('.textLines').length === 0) {
				clearTimeout(timeout_id);
			}
			// Iterate over all elements to which the 'resize' event is bound.
			$('.textLines').each(function () {
				var elem = $(this),
					width = elem.width(),
					height = elem.height(),
					data = $.data(this, 'textLines-Initials');

				var bg = null;
				var p = this;
				while (bg == null && p != null) {
					var temp = $(p).css('background-color');
					if (temp != 'transparent') bg = temp;
					p = p.parentElement;
				}

				// If element size has changed since the last time, update the element
				// data store and trigger the 'resize' event.
				if (width !== data.w || height !== data.h || bg != data.bg) {
					elem.data('textLines-Initials').w = width;
					elem.data('textLines-Initials').h = height;
					elem.data('textLines-Initials').bg = bg;
					elem.data('textLines').refresh();
				}
			});
			startLoop();
		}, 250);
	};

	$.textLines.defaultOptions = {
		lines: 0,
		showTooltip: false
	};
})(jQuery);

ko.bindingHandlers.textLines = {
	'init': function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());

		var lines;
		var showToolTip = false;

		if ($.isNumeric(value)) {
			lines = value;
		} else {
			lines = ko.utils.unwrapObservable(value.lines);
			showToolTip = ko.utils.unwrapObservable(value.showToolTip);
		}

		$(element).textLines({ lines: lines, showTooltip: showToolTip });
	},
	'update': function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());
		var text;

		if ($.isNumeric(value)) {
			return;
		} else {
			text = ko.utils.unwrapObservable(value.text);
		}

		if (!((text === null) || (text === undefined))) {
			// We need there to be exactly one child: a text node.
			// If there are no children, more than one, or if it's not a text node,
			// we'll clear everything and create a single text node.
			var innerTextNode = ko.virtualElements.firstChild(element);
			if (!innerTextNode || innerTextNode.nodeType != 3 || ko.virtualElements.nextSibling(innerTextNode)) {
				ko.virtualElements.setDomNodeChildren(element, [document.createTextNode(text)]);
			} else {
				innerTextNode.data = text;
			}

			ko.utils.forceRefresh(element);
		}

		$(element).data('textLines').refresh();
	}
};