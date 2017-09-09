function GetListButton(element, valueAccessor, alt) {
	var listButtonViewModel = ko.utils.unwrapObservable(valueAccessor());

	var divElements = "";

	if (alt) {
		var guid = ConnectCMS.Guids.CreateGuid();
		divElements += "<div class='btn-group'>";
		divElements += "<button class='btn btn-primary-light btn-sm' type'button' data-bind='text: DefaultLabel, click: ClickDefault'></button>";
		divElements += "<button id='listButton_"+guid + "' class='btn btn-primary-light btn-sm dropdown-toggle' aria-expanded='true' type='button' data-toggle='dropdown' data-bind='click: ClickOptions, visible: Options() != null && Options().length > 1'>";
		divElements += "<span class='caret'></span>";
		divElements += "</button>";
		divElements += "<div class='dropdownContainer' style='display: none' data-bind='visible: OptionsVisible'>";
		divElements += "<ul class='dropdown-menu visible' data-bind='foreach: NonDefaultOptions'>";
		divElements += "<li><a href='#' data-bind='text: Label, click: ClickOption, attr: AnchorOptions'></a></li>";
		divElements += "</ul>";
		divElements += "</div>";
		divElements += "</div>";

		var jEl = $(element);
		jEl.attr('data-guid', guid);
		jEl.html(divElements);
		var jElListDropDown = jEl.find(".dropdownContainer");
		jElListDropDown.attr('data-guid', guid);

		ko.cleanNode(element.firstElementChild);
		ko.applyBindings(listButtonViewModel, element.firstElementChild);

		var escape = null;
		jEl.mouseleave(function (e) {
			if (listButtonViewModel.OptionsVisible())
				window.setTimeout(function () {
					if (escape) {
						escape = false;
						return;
					}
					listButtonViewModel.OptionsVisible(false);
					jEl.find('.btn-group').removeClass('open');
					var t = document.getElementById('listButton_' + $(e.currentTarget).attr('data-guid'));
					if (t != null)
						t.blur();
				}, 200);
		});
		jElListDropDown.mouseleave(function (e) {
			if (listButtonViewModel.OptionsVisible())
				window.setTimeout(function () {
					if (escape) {
						escape = false;
						return;
					}
					listButtonViewModel.OptionsVisible(false);
					jEl.find('.btn-group').removeClass('open');
					var t = document.getElementById('listButton_' + $(e.currentTarget).attr('data-guid'));
					if (t != null)
						t.blur();
				}, 200);
		});
		jEl.mouseenter(function () {
			if (listButtonViewModel.OptionsVisible()) {
				escape = true;
			}
		});
		jElListDropDown.mouseenter(function () {
			if (listButtonViewModel.OptionsVisible()) {
				escape = true;
			}
		});
	} else {
		divElements += "<div class='listButtonInnerContainer'>";

		divElements += "<div class='listButton'>";
		divElements += "<a class='listButton' data-bind='text: DefaultLabel, click: ClickDefault, style: { minWidth : MinWidth() + \"px\" }'></a>";
		divElements += "<a class='listButtonArrow' data-bind='click: ClickOptions, visible: Options() != null && Options().length > 1' />";
		divElements += "</div>";

		divElements += "<div class='listButtonOptionsContainer'>";
		divElements += "<div class='listButtonOptions' data-bind='foreach: NonDefaultOptions, visible: OptionsVisible()'>";
		divElements += "<a class='listButtonOption' data-bind='text: Label, click: ClickOption'></a>";
		divElements += "</div>";
		divElements += "</div>";

		divElements += "</div>";

		var jElement = $(element);

		jElement.addClass("listButtonContainer");
		jElement.html(divElements);

		ko.cleanNode(element.firstElementChild);
		ko.applyBindings(listButtonViewModel, element.firstElementChild);

		var jElementListButtonOptions = jElement.find(".listButtonOptions");
		var minWidth = jElement.find(".listButton").outerWidth();

		var currentWidth = jElementListButtonOptions.outerWidth();

		if (currentWidth < minWidth)
			jElementListButtonOptions.outerWidth(minWidth);

		var jElementListButtonInnerContainer = jElement.find(".listButtonInnerContainer");

		jElementListButtonInnerContainer.mouseleave(function () {
			listButtonViewModel.OptionsVisible(false);
		});
	}
};

ko.bindingHandlers.listButton = {
	init: function (element, valueAccessor, allBindings) {
		var alt = allBindings().listButtonAlt != null;
		GetListButton(element, valueAccessor, alt);

		return {
			'controlsDescendantBindings': true
		};
	},
	update: function (element, valueAccessor, allBindings) {
		var alt = allBindings().listButtonAlt != null;
		GetListButton(element, valueAccessor, alt);
	}
};