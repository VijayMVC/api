var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};

ConnectCMS.Requests.ActionModel = function (data) {
	var self = this;

	self.actionTime = ConnectCMS.GetDateFromUTCMilliseconds(data.ActionTimeMilliseconds || 0);
	self.guestUserRoomNumber = data.GuestUerRoomNumber || '';
	self.actualMessage = data.Message || '';
	self.messageIsPrivate = data.MessagePrivate || false;
	self.requestActionId = data.RequestActionId || -1;
	self.requestUserName = data.RequestUserName || '';
	self.isStaffAction = data.StaffAction || false;
	self.userId = data.UserId || -1;
	self.newEtaMilliseconds = data.NewETAMilliseconds;
	self.addedUserName = data.AddedUserName;
	self.removedUserName = data.RemovedUserName;
	self.newRequestGroupName = data.NewRequestGroupName;
	self.newStatus = data.NewStatus;

	self.hasMessage = ko.computed(function () {
		return self.message && self.message.length > 0;
	});

	self.actionMessages = ko.computed(function () {
		// Check various properties on the action to see if it needs to have a specific
		// action message.

		var messages = [];

		// Changed to Pending Status
		if (self.newStatus !== null && self.newStatus === ConnectCMS.Requests.RequestStatus.PENDING) {
			messages.push({ message: 'Guest Approved', css: 'action-approved' });
		}

		// Changed to Accepted Status
		if (self.newStatus && self.newStatus === ConnectCMS.Requests.RequestStatus.ACCEPTED) {
			messages.push({ message: 'Accepted', css: 'action-accepted' });
		}

		// Changed to Completed Status
		if (self.newStatus && self.newStatus === ConnectCMS.Requests.RequestStatus.CLOSED) {
			messages.push({ message: 'Completed', css: 'action-completed' });
		}

		// Changed ETA
		if (self.newEtaMilliseconds) {
			var newEta = ConnectCMS.GetDateFromUTCMilliseconds(self.newEtaMilliseconds);
			messages.push({ message: 'ETA Updated: ' + moment(newEta).format('llll'), css: 'action-eta-update' });
		}

		// Forwarded to a new group
		if (self.newRequestGroupName) {
			messages.push({ message: 'Forwarded to ' + self.newRequestGroupName, css: 'action-forward' });
		}

		// Added a user
		if (self.addedUserName) {
			messages.push({ message: 'Added ' + self.addedUserName, css: 'action-add-user' });
		}

		// Removed a user / Unfollowed
		if (self.removedUserName) {
			messages.push({ message: 'Unfollowed by ' + self.removedUserName, css: 'action-remove-user' });
		}
		return messages;
	});

	self.message = ko.computed(function () {
		if (this.actualMessage.length > 0 || this.actionMessages().length > 0) {
			return this.actualMessage;
		} else {
			'No message';
		}
	}, self);

	self.actionTimeDisplay = ko.computed(function () {
		if (self.actionTime) {
			return moment(self.actionTime).format('LT');
		} else {
			return '';
		}
	});

	self.actionDateDisplay = ko.computed(function () {
		if (self.actionTime) {
			return moment(self.actionTime).format('ll');
		} else {
			return '';
		}
	});
};