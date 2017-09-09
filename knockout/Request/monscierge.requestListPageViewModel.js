/// <reference path="monscierge.requestListPageViewModel.js" />
var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};
ConnectCMS.Globalization = ConnectCMS.Globalization || {};

ConnectCMS.Requests.RequestStatus = {
	APPROVAL: -1,
	PENDING: 0,
	ACCEPTED: 1,
	CLOSED: 2,
	ARCHIVED: 3
};

ConnectCMS.Requests.RequestStatusTabViewModel = function (label, visible, enabled, clickMethod, index, selectedIndex, statusCountMethod) {
	// This object uses TabViewModel as a parent.

	TabViewModel.call(this, label, visible, enabled, clickMethod, index, selectedIndex);
	this.statusCount = statusCountMethod;
};

ConnectCMS.Requests.ListPageViewModel = function (requestUserId, culture) {
	var self = this;
	var api = ConnectCMS.Requests.RequestApi;

	self.showNotifications = false;
	self.unreadNotification = null;
	self.unreadCount = 0;

	self.requestUserId = requestUserId || 0;
	self.culture = culture || 'en-US';

	self.timer = new ConnectCMS.Requests.RequestRefreshTimer(2);
	self.requestsLoading = ko.observable(false);
	self.actionsLoading = ko.observable(false);
	self.updatingReadActions = ko.observable(false);
	self.actionIsPending = ko.observable(false);

	self.maxRequestActionId = 0;

	self.selectedTabIndex = ko.observable(-1);
	self.requests = ko.observableArray([]);
	self.selectedRequest = ko.observable(null);

	self.newChatMessage = ko.observable('');
	self.newActionMessage = ko.observable('');
	self.availableEtaMinutes = [5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90];
	self.selectedEtaMinutes = ko.observable(null);

	self.showAcceptForm = ko.observable(false);
	self.showCompleteForm = ko.observable(false);
	self.showDenyForm = ko.observable(false);
	self.showBlockForm = ko.observable(false);
	self.showForwardForm = ko.observable(false);
	self.showAddUserForm = ko.observable(false);
	self.showRemoveUserForm = ko.observable(false);
	self.showDeleteForm = ko.observable(false);

	self.requestGroups = ko.observableArray([]);
	self.selectedRequestGroupId = ko.observable();

	self.requestParticipants = ko.observableArray([]);
	self.requestManagerGroups = [];

	self.selectedParticipantToAddId = ko.observable(null);
	self.selectedParticipantToRemoveId = ko.observable(null);

	self.hotelRequestParticipantsLoaded = false;
	self.hotelRequestParticipants = ko.observableArray([]);

	self.updateActionInProgress = ko.observable(false);

	self.selectedMessagePrivacy = ko.observable('public');

	self.stockResponses = ko.observableArray([]);
	self.stockResponsesAreVisible = ko.observable(false);

	self.retryCount = 0;
	self.retry = function () {
		self.error(null);
		self.loadData(true);
	};

	self.error = ko.observable();

	self.approvalStatusCount = ko.computed(function () {
		return ko.utils.arrayFilter(self.requests(), function (item) {
			return item.status() === ConnectCMS.Requests.RequestStatus.APPROVAL;
		}).length;
	});

	self.pendingStatusCount = ko.computed(function () {
		return ko.utils.arrayFilter(self.requests(), function (item) {
			return item.status() === ConnectCMS.Requests.RequestStatus.PENDING;
		}).length;
	});

	self.acceptedStatusCount = ko.computed(function () {
		return ko.utils.arrayFilter(self.requests(), function (item) {
			return (item.status() === ConnectCMS.Requests.RequestStatus.ACCEPTED);
		}).length;
	});

	self.closedStatusCount = ko.computed(function () {
		return ko.utils.arrayFilter(self.requests(), function (item) {
			return item.status() === ConnectCMS.Requests.RequestStatus.CLOSED;
		}).length;
	});

	self.archivedStatusCount = ko.computed(function () {
		return ko.utils.arrayFilter(self.requests(), function (item) {
			return item.status() === ConnectCMS.Requests.RequestStatus.ARCHIVED;
		}).length;
	});

	self.tabs = [
		new ConnectCMS.Requests.RequestStatusTabViewModel(ConnectCMS.Globalization.RequestMgmtApprovals, true, true, self.changeTab, ConnectCMS.Requests.RequestStatus.APPROVAL, self.selectedTabIndex, self.approvalStatusCount),
		new ConnectCMS.Requests.RequestStatusTabViewModel(ConnectCMS.Globalization.RequestMgmtPending, true, true, self.changeTab, ConnectCMS.Requests.RequestStatus.PENDING, self.selectedTabIndex, self.pendingStatusCount),
		new ConnectCMS.Requests.RequestStatusTabViewModel(ConnectCMS.Globalization.RequestMgmtAccepted, true, true, self.changeTab, ConnectCMS.Requests.RequestStatus.ACCEPTED, self.selectedTabIndex, self.acceptedStatusCount),
		new ConnectCMS.Requests.RequestStatusTabViewModel(ConnectCMS.Globalization.RequestMgmtClosed, true, true, self.changeTab, ConnectCMS.Requests.RequestStatus.CLOSED, self.selectedTabIndex, self.closedStatusCount),
		new ConnectCMS.Requests.RequestStatusTabViewModel(ConnectCMS.Globalization.RequestMgmtArchived, true, true, self.changeTab, ConnectCMS.Requests.RequestStatus.ARCHIVED, self.selectedTabIndex, self.archivedStatusCount)
	];

	var _selectedRequestSort = ko.observable('oldestfirst');
	self.selectedRequestSort = ko.computed({
		read: function () { return _selectedRequestSort(); },
		write: function (value) {
			if (value == null || _selectedRequestSort() == value) return;
			_selectedRequestSort(value);
			self.saveUserSettings();
		},
		deferEvaluation: true
	}
	);

	self.showRequestList = ko.computed(function () {
		return self.selectedRequest() == null;
	});

	self.showRequestDetails = ko.computed(function () {
		return !(self.showRequestList());
	});

	self.showRequestLoadingProgressBar = ko.computed(function () {
		return self.requestsLoading() && self.requests().length > 0;
	});

	self.filteredRequestList = ko.computed(function () {
		var list = ko.utils.arrayFilter(self.requests(), function (request) {
			if (self.selectedTabIndex() === ConnectCMS.Requests.RequestStatus.ACCEPTED) {
				return (request.status() == self.selectedTabIndex());
			}

			if (self.selectedTabIndex() === ConnectCMS.Requests.RequestStatus.CLOSED) {
				return (request.status() == self.selectedTabIndex());
			}

			if (self.selectedTabIndex() === ConnectCMS.Requests.RequestStatus.ARCHIVED) {
				return (request.status() === self.selectedTabIndex());
			}
			return request.status() == self.selectedTabIndex();
		});
		list.sort(function (a, b) {
			switch (self.selectedRequestSort()) {
				case 'newestfirst':
					return b.lastStatusActionTime - a.lastStatusActionTime;
				default:
					return a.lastStatusActionTime - b.lastStatusActionTime;
			}
		});
		return list;
	});

	self.showDeleteAction = ko.computed(function () {
		return self.selectedRequest() && requestCanDelete() && self.actionIsPending() === false;
	});
	self.showApprovalAction = ko.computed(function () {
		return (self.selectedRequest() && self.selectedRequest().status() === ConnectCMS.Requests.RequestStatus.APPROVAL && self.actionIsPending() === false);
	});

	self.showAcceptedAction = ko.computed(function () {
		return (self.selectedRequest() && self.selectedRequest().status() === ConnectCMS.Requests.RequestStatus.ACCEPTED && self.actionIsPending() === false);
	});

	self.showPendingAction = ko.computed(function () {
		return (self.selectedRequest() && self.selectedRequest().status() === ConnectCMS.Requests.RequestStatus.PENDING && self.actionIsPending() === false);
	});

	self.showOtherAction = ko.computed(function () {
		return self.actionIsPending() === false;
	});

	self.showUpdateEta = ko.computed(function () {
		return (self.selectedRequest() && self.selectedRequest().status() < ConnectCMS.Requests.RequestStatus.CLOSED) && self.actionIsPending() === false;
	});

	self.canUpdateEta = ko.computed(function () {
		return self.selectedEtaMinutes();
	});

	self.detailHeaderText = ko.computed(function () {
		var text = '';
		if (self.selectedRequest()) {
			switch (self.selectedRequest().status()) {
				case ConnectCMS.Requests.RequestStatus.PENDING:
					text = ConnectCMS.Globalization.RequestDetailHeaderPending;
					break;
				case ConnectCMS.Requests.RequestStatus.ACCEPTED:
					text = ConnectCMS.Globalization.RequestDetailHeaderAccepted;
					break;
				case ConnectCMS.Requests.RequestStatus.CLOSED:
					text = ConnectCMS.Globalization.RequestDetailHeaderCompleted;
					break;
				default:
					text = ConnectCMS.Globalization.RequestDetailHeaderApproval;
					break;
			}
		}
		return text;
	});

	self.filteredRequestGroups = ko.computed(function () {
		if (self.selectedRequest()) {
			return ko.utils.arrayFilter(self.requestGroups(), function (item) {
				return item.key !== (self.selectedRequest()) ? self.selectedRequest().requestGroupId : -1;
			});
		}
		return [];
	});

	self.filteredHotelRequestParticipants = ko.computed(function () {
		return ko.utils.arrayFilter(self.hotelRequestParticipants(), function (item) {
			var exists = false;
			for (var i = 0; i < self.requestParticipants().length; i++) {
				if (self.requestParticipants()[i].key === item.key) {
					exists = true;
					break;
				}
			}
			return !exists;
		});
	});

	self.requestHasParticipants = ko.computed(function () {
		return self.requestParticipants().length > 0;
	});

	self.showUpdateProgress = ko.computed(function () {
		return self.actionsLoading() || self.updateActionInProgress();
	});

	self.filteredStockResponses = ko.computed(function () {
		if (!self.selectedRequest()) {
			return [];
		}

		var stockResponsesForRequestType = ko.utils.arrayFirst(self.stockResponses(), function (item) {
			return item.key === self.selectedRequest().requestTypeId;
		});

		return (stockResponsesForRequestType) ? stockResponsesForRequestType.value : [];
	});

	self.showStockResponses = function () {
		var isVisible = self.stockResponsesAreVisible();
		self.stockResponsesAreVisible(!isVisible);
	};

	self.selectStockResponse = function (item) {
		self.newChatMessage(item.message);
		self.stockResponsesAreVisible(false);
	};

	self.loadData = function (isAutoRefresh) {
		if (self.requestsLoading() === false) {
			self.requestsLoading(true);

			api.getRequestData(self.requestUserId, self.maxRequestActionId, self.culture, isAutoRefresh, function (result) {
				if (result) {
					if (self.maxRequestActionId < result.MaxRequestAction) {
						self.maxRequestActionId = result.MaxRequestAction;
					}

					self.maxRequestActionId = result.MaxRequestAction || 0;

					if (self.timer.getState() === ConnectCMS.Requests.RequestTimerStates.STOPPED) {
						self.timer.setMinIntervalMilliseconds((result.MinRefreshIntervalSeconds || 10) * 1000);
						//	self.timer.setMaxIntervalMilliseconds((result.MaxRefreshIntervalSeconds || 180) * 1000);
						self.timer.setMaxIntervalMilliseconds(30000);
					}

					var unreadCount = 0;

					// If there are any requests in the JSON result.
					if (result.Requests && result.Requests.length > 0) {
						// Slice out any existing requests and hold onto them.
						var requestArray = self.requests.slice(0);
						self.requests.removeAll();

						// Construct RequestModel objects from all returned requests and load them into an array.
						var refreshedRequests = [];
						for (var i = 0; i < result.Requests.length; i++) {
							var data = result.Requests[i];

							// See if the request is not VisibleToUser.
							// This will reflect a change in the request that causes the request to no longer be visible to the user.
							// Find the request by ID in the requestArray collection and remove it.
							if (!data.VisibleToUser) {

								var idx = _.findIndex(requestArray, { requestId: data.RequestId });
								if(idx > 0) {
									requestArray.splice(idx, 1);
								}
								continue;
							}

							var request = new ConnectCMS.Requests.RequestModel(data, self.requestUserId, culture);
							refreshedRequests.push(request);

							if (self.requestCountsAsUnread(request)) {
								unreadCount++;
							}
						}

						var newRequests = [];

						// Go through each request and either update it or add it to the new requests array.
						for (var j = 0; j < refreshedRequests.length; j++) {
							var refreshedRequest = refreshedRequests[j];
							var existingRequestFound = false;

							// Loop throught the existing requests.
							for (var k = 0; k < requestArray.length; k++) {
								if (requestArray[k].requestId === refreshedRequest.requestId) {
									// We already have this request, so update it with the new version.
									existingRequestFound = true;
									requestArray[k] = refreshedRequest;
									break;
								}
							}

							if (existingRequestFound === false) {
								// We didn't find an existing request with this id, so add it to the new requests array.
								newRequests.push(refreshedRequest);
							}
						}

						// Add any new requests to the requests array
						for (var l = 0; l < newRequests.length; l++) {
							requestArray.push(newRequests[l]);
						}

						// Put the requests back into the observable array and signal that the array has changed.
						ko.utils.arrayPushAll(self.requests, requestArray);
						self.requests.valueHasMutated();

						// Check if any of the requests have unread actions. If so, play the notification sound.
						if (unreadCount > 0 && self.showNotifications) {
							// Play the alert sound
							self.playAlertSound();

							// Show a desktop notification
							var title = unreadCount + " " + (unreadCount == 1 ? ConnectCMS.Globalization.RequestDesktopNotificationTitleSingular :
								ConnectCMS.Globalization.RequestDesktopNotificationTitlePlural);

							if (self.unreadNotification == null || self.unreadNotification.state == "closed") {
								self.unreadCount = unreadCount;
								self.unreadNotification = ConnectCMS.Utilities.PNotify.desktopNotification(title, ConnectCMS.Globalization.RequestDesktopNotificationMessage);
							} else {
								unreadCount = self.unreadCount += unreadCount;
								title = unreadCount + " " + (unreadCount == 1 ? ConnectCMS.Globalization.RequestDesktopNotificationTitleSingular :
								ConnectCMS.Globalization.RequestDesktopNotificationTitlePlural);
								ConnectCMS.Utilities.PNotify.updateDesktopNotification(self.unreadNotification, title, ConnectCMS.Globalization.RequestDesktopNotificationMessage);
							}
						} else {
							self.showNotifications = true;
						}

						// Since we had some requests in the JSON result, reset the timer so that it starts it's increments over again.
						self.timer.reset(function () { self.loadData(true); });
					} else {
						// No results came back from the server.
						// If the timer is currently stopped, start it. If it's running, increment it.
						if (self.timer.getState() === ConnectCMS.Requests.RequestTimerStates.STOPPED) {
							self.timer.start(function () { self.loadData(true); });
						} else {
							self.timer.increment();
						}
					}
				} else {
					self.maxRequestActionId = 0;
				}
				self.requestsLoading(false);
				self.retryCount = 0;
			},
				function (jqXhr, textStatus, errorThrown) {
					if (self.retryCount < 5) {
						self.timer.reset(function () { self.loadData(true); });
						self.requestsLoading(false);
						self.retryCount++;
					} else {
						ErrorLogging.log(textStatus + ' | ' + errorThrown);
						self.error(ConnectCMS.Globalization.NetworkError);
						self.requestsLoading(false);
					}
				});
		}
	};

	self.loadUserSettings = function () {
		api.getUserSettings(
			function (result) {
				self.selectedRequestSort(result.RequestSort == null ? 'oldestfirst' : result.RequestSort);
				self.managerGroups = result.RequestManagerGroups == null ? [] : $.map(result.RequestManagerGroups.split(','), function (item) { return parseInt(item); });
			},
			function (jqXhr, textStatus, errorThrown) {
				ErrorLogging.log(textStatus + ' | ' + errorThrown);
				self.error(ConnectCMS.Globalization.NetworkError);
			}
		);
	};

	self.saveUserSettings = function () {
		api.postUserSettings({ contactSettings: [new ConnectCMS.Common.KeyValuePair('RequestSort', self.selectedRequestSort())] },
			function (result) {
				self.selectedRequestSort(result.RequestSort == null ? 'oldestfirst' : result.RequestSort);
			},
			function (jqXhr, textStatus, errorThrown) {
				ErrorLogging.log(textStatus + ' | ' + errorThrown);
				self.error(ConnectCMS.Globalization.NetworkError);
			}
		);
	};

	self.requestCountsAsUnread = function (request) {
		if (request) {
			var closeStatusCutOffDate = moment(request.lastStatusActionTime).add(24, 'h').toDate();
			var archiveCutOffDate = moment(closeStatusCutOffDate).add(7, 'd').toDate();
			var now = new Date();

			if ((request.status() === ConnectCMS.Requests.RequestStatus.APPROVAL || request.status() === ConnectCMS.Requests.RequestStatus.PENDING)) {
				return request.unreadActionCount() > 0;
			}

			if (request.status() === ConnectCMS.Requests.RequestStatus.ACCEPTED && (request.userIsParticipant === true || self.managerGroups.indexOf(request.requestGroupId) >= 0)) {
				return request.unreadActionCount() > 0;
			}
			if (request.status() === ConnectCMS.Requests.RequestStatus.CLOSED && now < closeStatusCutOffDate) {
				return request.unreadActionCount() > 0;
			}
			if (request.status() === ConnectCMS.Requests.RequestStatus.CLOSED && now > closeStatusCutOffDate && now < archiveCutOffDate) {
				return request.unreadActionCount() > 0;
			}
		}
		return false;
	};

	self.loadSelectedRequestActions = function (isAutoRefresh) {
		if (self.selectedRequest() && self.actionsLoading() === false) {
			var request = self.selectedRequest();
			self.actionsLoading(true);

			var lastKnownRequestActionId = self.getMaxActionId(request.actions());

			api.getRequestActions(request.requestId, lastKnownRequestActionId, request.requestUserId, isAutoRefresh, function (result) {
				if (result) {
					if (self.timer.getState() === ConnectCMS.Requests.RequestTimerStates.STOPPED) {
						self.timer.setMinIntervalMilliseconds((result.MinRefreshIntervalSeconds || 10) * 1000);
						//		self.timer.setMaxIntervalMilliseconds((result.MaxRefreshIntervalSeconds || 180) * 1000);
						self.timer.setMaxIntervalMilliseconds(30000);
					}

					// If there are any requests in the JSON result.
					if (result.length > 0) {
						// Slice out any existing actions and hold onto them.
						var actionArray = request.actions.slice(0);
						request.actions.removeAll();

						// Construct ActionModel objects from all returned requests and load them into an array.
						var refreshedActions = [];
						for (var i = 0; i < result.length; i++) {
							var data = result[i];
							var action = new ConnectCMS.Requests.ActionModel(data);
							refreshedActions.push(action);
						}

						var newActions = [];

						// Go through each action and either update it or add it to the new actions array.
						for (var j = 0; j < refreshedActions.length; j++) {
							var refreshedAction = refreshedActions[j];
							var existingActionFound = false;

							// Loop throught the existing actions.
							for (var k = 0; k < actionArray.length; k++) {
								if (actionArray[k].requestActionId === refreshedAction.requestActionId) {
									// We already have this action, so update it with the new version.
									existingActionFound = true;
									actionArray[k] = refreshedAction;
									break;
								}
							}

							if (existingActionFound === false) {
								// We didn't find an existing action with this id, so add it to the new actions array.
								newActions.push(refreshedAction);
							}
						}

						// Add any new actions to the actions array
						for (var l = 0; l < newActions.length; l++) {
							actionArray.push(newActions[l]);
						}

						// Put the actions back into the observable array and signal that the array has changed.
						ko.utils.arrayPushAll(request.actions, actionArray);
						request.actions.valueHasMutated();

						// Now we need to re-examine the action collection and possibly update the maxActionId on the request.
						var newMaxActionId = self.getMaxActionId(request.actions());
						if (newMaxActionId > request.maxActionId()) {
							request.maxActionId(newMaxActionId);
						}

						// Since we had some actions in the JSON result, reset the timer so that it starts it's increments over again.
						self.timer.reset(function () { self.loadSelectedRequestActions(true); });
					} else {
						// No results came back from the server.
						// If the timer is currently stopped, start it. If it's running, increment it.
						if (self.timer.getState() === ConnectCMS.Requests.RequestTimerStates.STOPPED) {
							self.timer.start(function () { self.loadSelectedRequestActions(true); });
						} else {
							self.timer.increment();
						}
					}
				} else {
					// Nothing now
				}

				self.actionsLoading(false);

				// Force the chat window to scroll to the bottom in case any new actions were added.
				setTimeout(function () {
					var container = document.getElementById('chat-container');
					if (container != null)
						container.scrollTop = 9999999;
				}, 100);

				self.updateReadActions();

				self.loadStockResponses(request.requestTypeId);
			}, function (jqXhr, textStatus, errorThrown) {
				var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
				Toast.showGenericError(errorId);
			});
		}
	};

	self.loadStockResponses = function (requestTypeId, forceReload) {
		if (!forceReload)
			forceReload = false;

		// See if stock responses have been loaded for this requestTypeId.
		var responses = ko.utils.arrayFirst(self.stockResponses(), function (item) {
			return item.key === requestTypeId;
		});

		if (!responses || (forceReload === true)) {
			// We don't have these responses yet, so get them from the server.
			var isForAccept = self.selectedRequest().needsApproval();
			api.getStockResponses(requestTypeId, isForAccept, function (json) {
				var newStockResponses = [];
				if (json && json.length > 0) {
					newStockResponses = ko.utils.arrayMap(json, function (data) {
						return new ConnectCMS.Requests.StockResponse(data);
					});
				}

				if (forceReload === true && responses) {
					// If this is a force reload and stock responses have already been loaded
					// for this requestTypeId, remove them from the array.
					ko.utils.arrayRemoveItem(self.stockResponses(), responses);
				}

				// Push the result into the array even if there weren't any responses. This will prevent sending
				// repetitive calls to the server for request types that we know don't have any stock responses.
				self.stockResponses.push(new ConnectCMS.Common.KeyValuePair(requestTypeId, newStockResponses));
			}, function (jqXhr, textStatus, errorThrown) {
				var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
				Toast.showGenericError(errorId);
			});
		}
	};

	self.getMaxActionId = function (actions) {
		var maxAction = ko.utils.arrayFirst(actions, function (action) {
			return action.requestActionId === Math.max.apply(null, ko.utils.arrayMap(actions, function (e) {
				return e.requestActionId;
			}));
		});

		return (maxAction) ? maxAction.requestActionId || 0 : 0;
	};

	self.updateReadActions = function () {
		if (self.selectedRequest() && !self.updatingReadActions()) {
			var request = self.selectedRequest();
			if (request && request.maxActionId() > request.maxReadActionId()) {
				request.maxReadActionId(request.maxActionId());

				self.updatingReadActions(true);
				api.updateViewedActions(request.requestUserId, request.requestId, request.maxReadActionId(),
					function () {
						self.updatingReadActions(false);
					},
					self.onUpdateFailureCallback
				);
			}
		}
	};

	self.sendNewChatMessage = function () {
		if (self.selectedRequest() && self.newChatMessage().length > 0 && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			var isPrivate = self.selectedMessagePrivacy() === 'private';
			self.timer.stop();
			self.updateActionInProgress(true);

			api.sendRequestMessage(request.requestUserId, request.requestId, self.newChatMessage(), null, isPrivate,
				function (result, textStatus, jqXhr) {
					self.updateActionInProgress(false);
					self.loadSelectedRequestActions(false);
					self.newChatMessage('');
					self.selectedMessagePrivacy('public');
				},
				self.onUpdateFailureCallback
			);
		}
	};

	self.changeTab = function () {
		self.selectedRequestInfo(null);
	};

	self.chooseRequest = function (request) {
		self.timer.stop();
		self.selectedRequest(request);
		self.loadSelectedRequestActions(false);
	};

	self.backToList = function () {
		self.timer.stop();
		self.selectedRequest(null);

		// Clear edit fields and close any open forms.
		self.clearEditFormFields();
		self.newChatMessage('');
		self.selectedMessagePrivacy('public');

		self.showAcceptForm(false);
		self.showCompleteForm(false);
		self.showDenyForm(false);
		self.showBlockForm(false);
		self.showForwardForm(false);
		self.showAddUserForm(false);
		self.showRemoveUserForm(false);
		self.actionIsPending(false);

		self.loadData();
	};

	self.clearEditFormFields = function () {
		self.newActionMessage('');
		self.selectedEtaMinutes(null);
		self.selectedRequestGroupId(null);
		self.selectedParticipantToAddId(null);
		self.selectedParticipantToRemoveId(null);
	};

	self.init = function () {
		self.loadUserSettings();
		self.loadData();
	}

	self.onUpdateFailureCallback = function (jqXhr, textStatus, errorThrown) {
		self.updateActionInProgress(false);
		var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
		Toast.showGenericError(errorId);
	};

	// APPROVE
	self.approveRequest = function () {
		if (self.selectedRequest() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			self.updateActionInProgress(true);

			api.approveRequest(request.guestUserId, request.requestUserId, request.hotelId, function (json) {
				self.updateActionInProgress(false);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.RequestApprovedTitle, ConnectCMS.Globalization.RequestApprovedMessage);

				self.updateSelectedRequest();
			}, self.onUpdateFailureCallback);
		}
	};
	// --> end APPROVE

	// ACCEPT
	self.clickAccept = function () {
		self.actionIsPending(true);
		self.showAcceptForm(true);
	};

	self.closeAccept = function () {
		self.showAcceptForm(false);
		self.actionIsPending(false);
		self.clearEditFormFields();
	};

	self.confirmAccept = function () {
		if (self.selectedRequest() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			var msg = self.newActionMessage() || '';

			self.updateActionInProgress(true);
			api.acceptRequest(request.requestUserId, request.requestId, msg, self.selectedEtaMinutes(), function (json) {
				self.closeAccept();
				self.updateActionInProgress(false);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.RequestAcceptedTitle, ConnectCMS.Globalization.RequestAcceptedMessage);

				self.updateSelectedRequest();
			}, self.onUpdateFailureCallback);
		}
	};
	// --> End ACCEPT

	// DENY
	self.clickDeny = function () {
		self.actionIsPending(true);
		self.showDenyForm(true);
	};

	self.closeDeny = function () {
		self.showDenyForm(false);
		self.actionIsPending(false);
	};

	self.confirmDeny = function () {
		if (self.selectedRequest() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			self.updateActionInProgress(true);
			api.denyGuest(request.guestUserId, request.requestUserId, request.hotelId, function (json) {
				self.closeDeny();
				self.updateActionInProgress(false);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.RequestDeniedTitle, ConnectCMS.Globalization.RequestDeniedMessage);

				self.updateSelectedRequest();
			}, self.onUpdateFailureCallback);
		}
	};
	// -> end DENY

	// BLOCK
	self.clickBlock = function () {
		self.showBlockForm(true);
		self.actionIsPending(true);
	};

	self.closeBlock = function () {
		self.showBlockForm(false);
		self.actionIsPending(false);
	};

	self.confirmBlock = function () {
		if (self.selectedRequest() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			self.updateActionInProgress(true);

			api.blockGuest(request.guestUserId, request.requestUserId, request.hotelId, function (json) {
				self.closeBlock();
				self.updateActionInProgress(false);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.GuestBlockedTitle, ConnectCMS.Globalization.GuestBlockedMessage);

				self.updateSelectedRequest();
			}, self.onUpdateFailureCallback);
		}
	};
	// --> end BLOCK

	// COMPLETE
	self.clickComplete = function () {
		self.actionIsPending(true);
		self.showCompleteForm(true);
	};

	self.closeComplete = function () {
		self.showCompleteForm(false);
		self.actionIsPending(false);
		self.clearEditFormFields();
	};

	self.confirmComplete = function () {
		if (self.selectedRequest() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			var msg = self.newActionMessage() || '';
			self.updateActionInProgress(true);

			api.completeRequest(request.requestUserId, request.requestId, msg, function (json) {
				self.closeComplete();
				self.updateActionInProgress(false);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.RequestCompletedTitle, ConnectCMS.Globalization.RequestCompletedMessage);

				self.updateSelectedRequest();
			}, self.onUpdateFailureCallback);
		}
	};
	// --> end COMPLETE

	// Delete

	self.clickDelete = function () {
		self.actionIsPending(true);
		self.showDeleteForm(true);
	};

	self.closeDelete = function () {
		self.showDeleteForm(false);
		self.actionIsPending(false);
		self.clearEditFormFields();
	};

	self.confirmDelete = function () {
		if (self.selectedRequest() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			self.updateActionInProgress(true);

			api.deleteRequest(request.requestUserId, request.requestId, function (json) {
				self.showDeleteForm(false);
				self.updateActionInProgress(false);
				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.RequestDeleteTitle, ConnectCMS.Globalization.RequestDeleteMessage);
				self.backToList();
			}, self.onUpdateFailureCallback);
		}
	};

	// --> end Delete

	// FORWARD
	self.clickForward = function () {
		if (self.selectedRequest() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			self.actionIsPending(true);
			self.showForwardForm(true);
			self.updateActionInProgress(true);

			api.getRequestGroups(request.hotelId, function (json) {
				var array = self.requestGroups;

				var groups = ko.utils.arrayMap(json, function (item) {
					return new ConnectCMS.Common.KeyValuePair(item.RequestGroupId, item.Name);
				});

				ko.utils.arrayPushAll(array, groups);
				self.requestGroups.valueHasMutated();

				self.updateActionInProgress(false);
			}, self.onUpdateFailureCallback);
		}
	};

	self.closeForward = function () {
		self.actionIsPending(false);
		self.showForwardForm(false);
		self.clearEditFormFields();
	};

	self.confirmForward = function () {
		if (self.selectedRequest() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			self.updateActionInProgress(true);

			api.forwardRequest(request.requestUserId, request.requestId, self.selectedRequestGroupId(), function (json) {
				self.closeForward();
				self.updateActionInProgress(false);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.RequestForwardedTitle, ConnectCMS.Globalization.RequestForwardedMessage);

				self.updateSelectedRequest();
			}, self.onUpdateFailureCallback);
		}
	};
	// --> end FORWARD

	// ADD USER
	self.clickAddUser = function () {
		self.actionIsPending(true);
		self.showAddUserForm(true);

		self.loadParticipantUserData();
	};

	self.closeAddUser = function () {
		self.actionIsPending(false);
		self.showAddUserForm(false);
		self.clearEditFormFields();
	};

	self.confirmAddUser = function () {
		if (self.selectedRequest() && self.selectedParticipantToAddId() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			self.updateActionInProgress(true);

			api.addRequestParticipant(request.requestUserId, request.requestId, self.selectedParticipantToAddId(), function (json) {
				self.closeAddUser();
				self.updateActionInProgress(false);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.RequestAddUserTitle,
					ConnectCMS.Globalization.RequestAddUserMessage);

				self.loadParticipantUserData(true);
				self.updateSelectedRequest();
			}, function (jqXhr, textStatus, errorThrown) {
				// Something went wrong.
				var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
				ConnectCMS.Utilities.PNotify.error(ConnectCMS.Globalization.ToastTitleError, ConnectCMS.Globalization.RequestErrorUserMayAlreadyBeAdded);

				// Reload the available users list.
				self.loadParticipantUserData(true);
				self.updateActionInProgress(false);
			});
		}
	};
	// --> end ADD USER

	// REMOVE USER
	self.clickRemoveUser = function () {
		self.actionIsPending(true);
		self.showRemoveUserForm(true);

		self.loadParticipantUserData();
	};

	self.closeRemoveUser = function () {
		self.actionIsPending(false);
		self.showRemoveUserForm(false);
		self.clearEditFormFields();
	};

	self.confirmRemoveUser = function () {
		if (self.selectedRequest() && self.selectedParticipantToRemoveId() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			self.updateActionInProgress(true);

			api.removeRequestParticipant(request.requestUserId, request.requestId, self.selectedParticipantToRemoveId(), function (json) {
				self.closeRemoveUser();
				self.updateActionInProgress(false);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.RequestRemoveUserTitle, ConnectCMS.Globalization.RequestRemoveUserMessage);

				self.loadParticipantUserData();
				self.updateSelectedRequest();
			}, function (jqXhr, textStatus, errorThrown) {
				// Something went wrong.
				var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
				ConnectCMS.Utilities.PNotify.error(ConnectCMS.Globalization.ToastTitleError, ConnectCMS.Globalization.RequestErrorUserMayAlreadyBeRemoved);

				// Reload the available users list.
				self.loadParticipantUserData(true);
				self.updateActionInProgress(false);
			});
		}
	};
	// --> end REMOVE USER

	self.showElement = function (elem) {
		if (elem.nodeType === 1) {
			$(elem).hide().fadeIn('slow');
		}
	};

	self.hideElement = function (elem) {
		if (elem.nodeType === 1) {
			$(elem).slideUp('fast', function () { $(elem).remove(); });
		}
	};

	self.updateEta = function () {
		if (self.selectedRequest() && self.selectedEtaMinutes() && !self.updateActionInProgress()) {
			var request = self.selectedRequest();
			var newEta = moment().utc().add(self.selectedEtaMinutes(), 'm').toDate();

			if (self.timer.status !== ConnectCMS.Requests.RequestTimerStates.STOPPED) {
				self.timer.stop();
			}

			self.updateActionInProgress(true);
			api.updateRequestEta(request.requestUserId, request.requestId, newEta, function (json) {
				self.updateActionInProgress(false);
				self.selectedEtaMinutes(null);

				ConnectCMS.Utilities.PNotify.success(ConnectCMS.Globalization.ToastTitleSuccess, ConnectCMS.Globalization.EtaUpdatedMessage);

				self.updateSelectedRequest(request);
			}, function (jqXhr, textStatus, errorThrown) {
				var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
				Toast.showGenericError(errorId);
				self.updateActionInProgress(false);
			});
		}
	};

	self.cancelUpdateEta = function () {
		self.selectedEtaMinutes(null);
	};

	self.updateSelectedRequest = function () {
		if (self.selectedRequest()) {
			var request = self.selectedRequest();
			if (request) {
				if (self.timer.status !== ConnectCMS.Requests.RequestTimerStates.STOPPED) {
					self.timer.stop();
				}

				self.updateActionInProgress(true);
				api.getRequestDetail(request.requestUserId, request.requestId, self.culture, function (result) {
					if (result) {
						var updatedRequest = new ConnectCMS.Requests.RequestModel(result, self.requestUserId);
						self.selectedRequest(updatedRequest);
						self.updateActionInProgress(false);

						// refresh the action list
						self.loadSelectedRequestActions(false);

						// force reload the stock messages
						self.loadStockResponses(self.selectedRequest().requestTypeId, true);
					}
				}, function (jqXhr, textStatus, errorThrown) {
					var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
					Toast.showGenericError(errorId);
					self.updateActionInProgress(false);
				});
			}
		}
	};

	self.loadParticipantUserData = function (forceReloadAll) {
		if (self.selectedRequest()) {
			var request = self.selectedRequest();
			var forceReload = forceReloadAll || false;

			if (forceReload || !self.hotelRequestParticipantsLoaded) {
				api.getRequestUsersForHotel(request.hotelId, request.requestUserId, function (json) {
					self.hotelRequestParticipants.removeAll();
					if (json && json.length > 0) {
						var array = self.hotelRequestParticipants;

						var participants = ko.utils.arrayMap(json, function (item) {
							return new ConnectCMS.Common.KeyValuePair(item.RequestUserDetailId, item.Name);
						});

						ko.utils.arrayPushAll(array, participants);
						self.hotelRequestParticipants.valueHasMutated();
					}
					self.hotelRequestParticipantsLoaded = true;
				}, self.onUpdateFailureCallback);
			}

			api.getUsersForRequest(request.requestId, function (json) {
				self.requestParticipants.removeAll();
				if (json && json.length > 0) {
					var array = self.requestParticipants;

					var participants = ko.utils.arrayMap(json, function (item) {
						return new ConnectCMS.Common.KeyValuePair(item.RequestUserDetailId, item.Name);
					});

					ko.utils.arrayPushAll(array, participants);
					self.requestParticipants.valueHasMutated();
				}
			}, self.onUpdateFailureCallback);
		}
	};

	self.playAlertSound = function () {
		var audio = document.getElementsByTagName('audio')[0];
		if (audio) {
			audio.play();
		}
	};

	self.init();

	return {
		tabs: self.tabs,
		filteredRequestList: self.filteredRequestList,
		selectedRequest: self.selectedRequest,
		requestsLoading: self.requestsLoading,
		showRequestList: self.showRequestList,
		showRequestDetails: self.showRequestDetails,

		error: self.error,
		retry: self.retry,

		newChatMessage: self.newChatMessage,
		newActionMessage: self.newActionMessage,
		availableEtaMinutes: self.availableEtaMinutes,
		selectedEtaMinutes: self.selectedEtaMinutes,

		showAcceptForm: self.showAcceptForm,
		showCompleteForm: self.showCompleteForm,
		showDenyForm: self.showDenyForm,
		showBlockForm: self.showBlockForm,
		showForwardForm: self.showForwardForm,
		showAddUserForm: self.showAddUserForm,
		showRemoveUserForm: self.showRemoveUserForm,
		showDeleteForm: self.showDeleteForm,

		showApprovalAction: self.showApprovalAction,
		showAcceptedAction: self.showAcceptedAction,
		showPendingAction: self.showPendingAction,
		showDeleteAction: self.showDeleteAction,
		showOtherAction: self.showOtherAction,
		showUpdateEta: self.showUpdateEta,
		canUpdateEta: self.canUpdateEta,

		detailHeaderText: self.detailHeaderText,
		filteredRequestGroups: self.filteredRequestGroups,
		selectedRequestGroupId: self.selectedRequestGroupId,

		requestParticipants: self.requestParticipants,
		selectedParticipantToAddId: self.selectedParticipantToAddId,
		selectedParticipantToRemoveId: self.selectedParticipantToRemoveId,
		filteredHotelRequestParticipants: self.filteredHotelRequestParticipants,
		requestHasParticipants: self.requestHasParticipants,
		hotelRequestParticipants: self.hotelRequestParticipants,

		chooseRequest: self.chooseRequest,
		backToList: self.backToList,

		approveRequest: self.approveRequest,

		clickAccept: self.clickAccept,
		closeAccept: self.closeAccept,
		confirmAccept: self.confirmAccept,

		clickDelete: self.clickDelete,
		closeDelete: self.closeDelete,
		confirmDelete: self.confirmDelete,

		clickDeny: self.clickDeny,
		closeDeny: self.closeDeny,
		confirmDeny: self.confirmDeny,

		clickBlock: self.clickBlock,
		closeBlock: self.closeBlock,
		confirmBlock: self.confirmBlock,

		clickComplete: self.clickComplete,
		closeComplete: self.closeComplete,
		confirmComplete: self.confirmComplete,

		clickForward: self.clickForward,
		closeForward: self.closeForward,
		confirmForward: self.confirmForward,

		clickAddUser: self.clickAddUser,
		closeAddUser: self.closeAddUser,
		confirmAddUser: self.confirmAddUser,

		clickRemoveUser: self.clickRemoveUser,
		closeRemoveUser: self.closeRemoveUser,
		confirmRemoveUser: self.confirmRemoveUser,

		loadData: self.loadData,

		showElement: self.showElement,
		hideElement: self.hideElement,
		sendNewChatMessage: self.sendNewChatMessage,
		stopListTimer: function () {
			self.timer.stop();
		},
		updateEta: self.updateEta,
		cancelUpdateEta: self.cancelUpdateEta,
		showUpdateProgress: self.showUpdateProgress,
		selectedMessagePrivacy: self.selectedMessagePrivacy,

		filteredStockResponses: self.filteredStockResponses,
		showStockResponses: self.showStockResponses,
		stockResponsesAreVisible: self.stockResponsesAreVisible,
		selectStockResponse: self.selectStockResponse,
		selectedRequestSort: self.selectedRequestSort
	}
}