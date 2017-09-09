function HelpMessageViewModel(label, visibleSelector, contactUserSettingKey, clickMethod) {
	var self = this;

	self.ClickMethod = clickMethod;
	self.ContactUserSettingKey = contactUserSettingKey;
	self.Label = label;
	self.VisibleSelector = visibleSelector;

	self.Click = function () {
		if (self.VisibleSelector != null)
			self.VisibleSelector(false);

		try {
			$.ajax({
				url: "/ConnectCMS/Utility/InsertOrUpdateContactUserSetting",
				data: {
					key: self.ContactUserSettingKey,
					value: false,
				},
				type: "POST",
				error: function (jqXHR, textStatus, errorThrown) {
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});
		} catch (errorThrown) {
			ConnectCMS.LogError(eerrorThrownrror);
		}

		if (self.ClickMethod != null)
			self.ClickMethod();
	};
};