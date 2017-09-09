var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};
ConnectCMS.Globalization = ConnectCMS.Globalization || {};

ConnectCMS.Requests.RequestModel = function (data, requestUserId) {
	var self = this;
	var api = ConnectCMS.Requests.RequestApi;

	self.earliestActionTime = ConnectCMS.GetDateFromUTCMilliseconds(data.EarliestActionTimeMilliseconds);
	self.eta = ConnectCMS.GetDateFromUTCMilliseconds(data.ETAMilliseconds);
	self.guestUserId = data.GuestUserId || 0;
	self.guestUserRoomNumber = data.GuestUserRoomNumber || ConnectCMS.Globalization.RoomUnspecifiedText;
	self.lastActionTime = ConnectCMS.GetDateFromUTCMilliseconds(data.LastActionTimeMilliseconds);
	self.lastStatusActionTime = ConnectCMS.GetDateFromUTCMilliseconds(data.LastStatusActionTimeMilliseconds);
	self.hotelId = data.HotelId;
	self.hotelName = data.HotelName;
	self.requestGroupId = data.RequestGroupId || 0;
	self.userIsParticipant = data.UserIsParticipant || false;

	self.unreadActionCount = ko.observable(data.NumUnreadActions || 0);
	self.maxRequestActionId = ko.observable(data.MaxRequestActionId || 0);

	self.status = ko.observable(data.Status);
	self.requestId = data.RequestId || 0;
	self.requestTypeId = data.RequestTypeId || 0;
	self.requestTypeName = data.RequestTypeName || '';
	self.requestUserName = data.RequestUserName || '';

	self.requestUserId = requestUserId;

	self.actions = ko.observableArray([]);
	self.showDetailPanel = ko.observable(false);

	self.maxReadActionId = ko.observable(0);
	self.maxActionId = ko.computed({
		read: function () {
			return this.actions().length > 0 ? this.maxRequestActionId() : 0;
		},
		write: function (value) {
			this.maxRequestActionId(value);
		},
		owner: self
	});

	self.displayEta = ko.computed(function () {
		return self.status() < ConnectCMS.Requests.RequestStatus.CLOSED;
	});

	// Computed functions
	self.isOverdue = ko.computed(function () {
		var etaMoment = moment(self.eta);
		return moment().isAfter(etaMoment);
	});

	self.messageActions = ko.computed(function () {
		return ko.utils.arrayFilter(self.actions(), function (action, index) {
			return index > 0;
		});
	});

	self.earliestActionTimeDisplay = ko.computed(function () {
		var earliestActionMoment = moment(self.earliestActionTime);
		if (self.earliestActionTime && moment.isMoment(earliestActionMoment)) {
			return earliestActionMoment.format('llll');
		} else {
			return ConnectCMS.Globalization.TimeUnknownText;
		}
	});

	self.initialActionOptions = ko.computed(function () {
		var message = (self.actions() && self.actions().length > 0) ? (self.actions()[0].message() || '') : '';
		if (message.length > 0) {
			return message.split('\n');
		}
		return null;
	});

	self.actionReadClass = ko.computed(function () {
		if ((self.maxActionId() > self.maxReadActionId()) || (self.maxReadActionId() === 0 && self.unreadActionCount() > 0)) {
			var value = 'unread';

			switch (self.status()) {
				case ConnectCMS.Requests.RequestStatus.APPROVAL:
					value = value + ' approval';
					break;
				case ConnectCMS.Requests.RequestStatus.PENDING:
					value = value + ' pending';
					break;
				case ConnectCMS.Requests.RequestStatus.ACCEPTED:
					value = value + ' accepted';
					break;
				case ConnectCMS.Requests.RequestStatus.CLOSED:
					value = value + ' closed';
					break;
			}
			return value;
		} else {
			return 'read';
		}
	});

	self.originalMessage = ko.computed(function () {
		return self.messageActions()[0] ? self.messageActions()[0].message() : '';
	});

	self.toggleDetailPanel = function () {
		var isVisible = self.showDetailPanel() || false;
		self.showDetailPanel(!isVisible);
	};

	self.needsApproval = ko.computed(function () {
		return self.status() == ConnectCMS.Requests.RequestStatus.APPROVAL;
	});
};