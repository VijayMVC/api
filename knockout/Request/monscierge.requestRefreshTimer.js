var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};

ConnectCMS.Requests.RequestTimerStates = {
	STOPPED: 0,
	STARTED: 1
};

ConnectCMS.Requests.RequestRefreshTimer = function (timerIncrementMultiplier) {
	var self = this;

	self.currentInterval = 0;
	self.minInterval = 0;
	self.maxInterval = 0;
	self.incrementMultiplier = timerIncrementMultiplier || 2;
	self.timeoutHandle = null;
	self.state = ConnectCMS.Requests.RequestTimerStates.STOPPED;
	self.onTimerExpiresCallback = null;

	self.start = function (callback) {
		self.clearTimeout();

		self.currentInterval = self.minInterval;
		self.state = ConnectCMS.Requests.RequestTimerStates.STARTED;
		self.setTimeout(self.currentInterval);

		if (callback) {
			self.onTimerExpiresCallback = callback;
		}
	};

	self.stop = function () {
		self.clearTimeout();
		self.currentInterval = 0;
		self.state = ConnectCMS.Requests.RequestTimerStates.STOPPED;
	};

	self.reset = function (callback) {
		self.clearTimeout();

		self.currentInterval = self.minInterval;
		self.state = ConnectCMS.Requests.RequestTimerStates.STARTED;
		self.setTimeout(self.currentInterval);

		if (callback) {
			self.onTimerExpiresCallback = callback;
		}
	};

	self.increment = function () {
		self.clearTimeout();

		var interval = self.currentInterval;
		if (interval === 0) {
			interval = self.minInterval;
		} else {
			interval = (self.currentInterval * (self.incrementMultiplier));
		}

		if (interval > self.maxInterval) {
			interval = self.maxInterval;
		}

		if (interval > 0) {
			self.currentInterval = interval;

			if (self.state === ConnectCMS.Requests.RequestTimerStates.STARTED) {
				self.setTimeout(self.currentInterval);
			}
		}
	};

	self.onTimerExpiration = function () {
		if (self.onTimerExpiresCallback) {
			self.onTimerExpiresCallback();
		}
	};

	self.clearTimeout = function () {
		if (self.timeoutHandle) {
			window.clearTimeout(self.timeoutHandle);
		}
	};

	self.setTimeout = function (timerInterval) {
		self.clearTimeout();
		self.timeoutHandle = setTimeout(self.onTimerExpiration, timerInterval);
	};

	return {
		start: self.start,
		stop: self.stop,
		reset: self.reset,
		increment: self.increment,
		setMinIntervalMilliseconds: function (min) {
			self.minInterval = min || 0;
		},
		setMaxIntervalMilliseconds: function (max) {
			self.maxInterval = max || 0;
		},
		getState: function () {
			return self.state;
		}
	}
}