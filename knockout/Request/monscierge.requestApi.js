var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};

ConnectCMS.Requests.RequestApi = function () {
	var scheme = window.location.protocol,
        host = window.location.host,
        mobileDataServiceUrl = 'MonsciergeDataService/MobileDataService.svc',
		lastKeepAliveSent = null,
		keepAliveRefreshIntervalMins = 3,

        createServiceUrl = function (serviceOperationName) {
        	var urlParts = new Array(host, mobileDataServiceUrl, serviceOperationName);
        	return scheme + '//' + urlParts.join('/');
        },

		handleSuccess = function(json, xhr, successCallback) {
			if (successCallback) {
				successCallback(json);
			}
		},

		handleError = function (xhr, textStatus, errorThrown, errorCallback) {
			// Check for specific status codes
			if(xhr && (xhr.status == 401 || xhr.status == 403)) {
				// The error was a 401 Unauthorized or 403 Forbidden. Trigger a page reload. 
				// If the user's authentication token has expired, this will force a redirect
				// to the login page.
				document.location.reload(true);
			} else {
				if (errorCallback) {
					errorCallback(xhr, textStatus, errorThrown);
				} else {
					ConnectCMS.LogError(xhr, textStatus, errorThrown);
				}
			}
		},

        getJsonData = function (url, data, useCaching, successCallback, errorCallback, isAutoRefresh) {
        	// If the request is going to the MobileDataService, execute a KeepAlive call to the Request controller.
        	if (!isAutoRefresh && url.indexOf(mobileDataServiceUrl) >= 0) {
        		keepAlive();
        	}

        	$.ajax({
        		url: url,
        		type: 'GET',
        		data: data,
        		contentType: 'application/json',
        		dataType: 'json',
        		cache: useCaching || false
        	}).done(function(json, textStatus, xhr) {
        		handleSuccess(json, xhr, successCallback);
        	}).fail(function (xhr, textStatus, errorThrown) {
        		handleError(xhr, textStatus, errorThrown, errorCallback);
	        });
        },

        postJsonData = function (url, data, successCallback, errorCallback) {
        	// If the request is going to the MobileDataService, execute a KeepAlive call to the Request controller.
        	if (url.indexOf(mobileDataServiceUrl) >= 0) {
        		keepAlive();
        	}

        	$.ajax({
        		url: url,
        		type: 'POST',
        		cache: false,
        		data: JSON.stringify(data),
        		dataType: 'json',
        		contentType: 'application/json'
        	}).done(function (json, textStatus, xhr) {
        		handleSuccess(json, xhr, successCallback);
        	}).fail(function (xhr, textStatus, errorThrown) {
        		handleError(xhr, textStatus, errorThrown, errorCallback);
        	});
        },

		keepAliveHasExpired = function () {
			if (lastKeepAliveSent == null) {
				return true;
			}

			var now = moment();
			var lastSent = moment(lastKeepAliveSent);
			var diff = now.diff(lastSent, 'minutes');

			return (diff >= keepAliveRefreshIntervalMins);
		},

		keepAlive = function () {
			if (keepAliveHasExpired()) {
				var url = '/ConnectCMS/Request/KeepAlive';
				postJsonData(url, null, function () { lastKeepAliveSent = new Date(); }, null);
			}
		},

		getUserSettings = function (successCallback, errorCallback) {
			var url = '/ConnectCMS/Request/GetRequestUserSettings';
			getJsonData(url, null, false, successCallback, errorCallback);
		},

		postUserSettings = function (userSettings, successCallback, errorCallback) {
			var url = '/ConnectCMS/Utility/InsertOrUpdateContactUserSettings';
			postJsonData(url, userSettings, successCallback, errorCallback);
		},

		getRequestData = function (requestUserId, lastKnownRequestAction, culture, isAutoRefresh, successCallback, errorCallback) {
			var url = createServiceUrl('GetNewRequestsV3');
			var data = { RequestUserId: requestUserId, LastKnownRequestAction: lastKnownRequestAction, culture: culture, includeArchived: true, showArchiveDays: 10 };

			getJsonData(url, data, false, successCallback, errorCallback, isAutoRefresh);
		},

		getRequestCountByStatus = function (requestUserId, status, successCallback, errorCallback) {
			var url = createServiceUrl('GetRequestCountForUserByStatus');
			var data = { RequestUserId: requestUserId, requestStatus: status };

			getJsonData(url, data, false, successCallback, errorCallback, false);
		},

        getRequestActions = function (requestId, lastKnownRequestAction, requestUserId, isAutoRefresh, successCallback, errorCallback) {
        	var url = createServiceUrl('GetActionsForRequest');
        	var data = { RequestId: requestId, LastKnownRequestAction: lastKnownRequestAction, RequestUserId: requestUserId };

        	getJsonData(url, data, false, successCallback, errorCallback, isAutoRefresh);
        },

        getRequestGroups = function (hotelId, successCallback, errorCallback) {
        	var url = createServiceUrl('GetRequestGroupsForHotel');
        	var data = { hotelId: hotelId };

        	getJsonData(url, data, false, successCallback, errorCallback);
        },

        getUsersForRequest = function (requestId, successCallback, errorCallback) {
        	var url = createServiceUrl('GetUsersForRequest');
        	var data = { requestId: requestId };

        	getJsonData(url, data, false, successCallback, errorCallback);
        },

        getRequestUsersForHotel = function (hotelId, userId, successCallback, errorCallback) {
        	var url = createServiceUrl('GetRequestUsersForHotel');
        	var data = { hotelId: hotelId, userId: userId };

        	getJsonData(url, data, false, successCallback, errorCallback);
        },

        sendRequestMessage = function (requestUserId, requestId, message, newEta, isPrivateMessage, successCallback, errorCallback) {
        	var url = createServiceUrl('SendRequestMessage');
        	var data = { RequestUserId: requestUserId, RequestId: requestId, Message: message, NewETA: newEta, privateMessage: isPrivateMessage };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        completeRequest = function (requestUserId, requestId, message, successCallback, errorCallback) {
        	var url = createServiceUrl('CloseRequest');
        	var data = { RequestUserId: requestUserId, RequestId: requestId, Message: message };

        	postJsonData(url, data, successCallback, errorCallback);
        },

		 deleteRequest = function (requestUserId, requestId, successCallback, errorCallback) {
		 	var url = createServiceUrl('DeleteRequest');
		 	var data = { RequestUserId: requestUserId, RequestId: requestId };

		 	postJsonData(url, data, successCallback, errorCallback);
		 },

        acceptRequest = function (requestUserId, requestId, message, newEtaMinutes, successCallback, errorCallback) {
        	// The AcknowledgeRequest method expects NewEta to be submitted in seconds.
        	var newEtaSeconds = null;
        	if (newEtaMinutes) {
        		newEtaSeconds = (newEtaMinutes * 60);
        	}

        	var url = createServiceUrl('AcknowledgeRequest');
        	var data = { RequestUserId: requestUserId, RequestId: requestId, Message: message, NewETA: newEtaSeconds };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        approveRequest = function (guestUserId, requestUserId, hotelId, successCallback, errorCallback) {
        	var url = createServiceUrl('ApproveGuest');
        	var data = { guestUserId: guestUserId, requestUserId: requestUserId, hotelId: hotelId };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        denyGuest = function (guestUserId, requestUserId, hotelId, successCallback, errorCallback) {
        	var url = createServiceUrl('DenyGuest');
        	var data = { guestUserId: guestUserId, requestUserId: requestUserId, hotelId: hotelId };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        blockGuest = function (guestUserId, requestUserId, hotelId, successCallback, errorCallback) {
        	var url = createServiceUrl('BlockGuest');
        	var data = { guestUserId: guestUserId, requestUserId: requestUserId, hotelId: hotelId };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        forwardRequest = function (requestUserId, requestId, requestGroupId, successCallback, errorCallback) {
        	var url = createServiceUrl('ForwardRequest');
        	var data = { RequestUserId: requestUserId, RequestId: requestId, DestinationRequestGroup: requestGroupId };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        addRequestParticipant = function (requestUserId, requestId, participantRequestUserId, successCallback, errorCallback) {
        	var url = createServiceUrl('AddRequestParticipant');
        	var data = { RequestUserId: requestUserId, RequestId: requestId, ParticipantAddRequestUserId: participantRequestUserId };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        removeRequestParticipant = function (requestUserId, requestId, participantRequestUserId, successCallback, errorCallback) {
        	var url = createServiceUrl('RemoveRequestParticipant');
        	var data = { RequestUserId: requestUserId, RequestId: requestId, ParticipantRemoveRequestUserId: participantRequestUserId };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        getRequestTypes = function (deviceId, successCallback, errorCallback) {
        	var url = '/ConnectCMS/Request/GetRequestTypes';
        	var data = { deviceId: deviceId };

        	getJsonData(url, data, false, successCallback, errorCallback);
        },

		getRequestTypesByCategory = function (deviceId, successCallback, errorCallback) {
			var url = '/ConnectCMS/Request/GetRequestTypesByCategory';
			var data = { deviceId: deviceId };

			getJsonData(url, data, false, successCallback, errorCallback);
		},

        createRequest = function (requestUserId, requestTypeId, message, guestUserName, guestUserRoomNumber, successCallback, errorCallback) {
        	var url = createServiceUrl('CreateRequestForNewGuest');
        	var data = { CreatorRequestUserId: requestUserId, RequestTypeId: requestTypeId, Message: message, GuestUserName: guestUserName, GuestUserRoomNumber: guestUserRoomNumber };

        	postJsonData(url, data, successCallback, errorCallback);
        },

        getHotelFromDevice = function (deviceId, successCallback, errorCallback) {
        	var url = '/ConnectCMS/Utility/GetHotelFromDevice';
        	var data = { deviceId: deviceId };

        	postJsonData(url, data, successCallback, errorCallback);
        },

		updateViewedActions = function (requestUserId, requestId, lastViewedRequestActionId, successCallback, errorCallback) {
			var url = createServiceUrl('UpdateViewedActions');
			var data = { RequestUserId: requestUserId, RequestId: requestId, LastViewedRequestActionId: lastViewedRequestActionId };

			postJsonData(url, data, successCallback, errorCallback);
		},

		updateRequestEta = function (requestUserId, requestId, newEta, successCallback, errorCallback) {
			var url = createServiceUrl('UpdateRequestETA');
			var data = { RequestUserId: requestUserId, RequestId: requestId, NewETA: newEta.toMSJsonFormat() };

			postJsonData(url, data, successCallback, errorCallback);
		},

		getRequestDetail = function (requestUserId, requestId, culture, successCallback, errorCallback) {
			var url = createServiceUrl('GetRequestDetail');
			var data = { RequestUserId: requestUserId, RequestId: requestId, culture: culture };

			getJsonData(url, data, false, successCallback, errorCallback);
		},

		getStockResponses = function (requestTypeId, isForAccept, successCallback, errorCallback) {
			var url = createServiceUrl('GetStockResponses');
			var data = { requestType: requestTypeId, forAccept: isForAccept };

			getJsonData(url, data, false, successCallback, errorCallback);
		};

	return {
		getRequestData: getRequestData,
		getRequestActions: getRequestActions,
		getRequestGroups: getRequestGroups,
		getUsersForRequest: getUsersForRequest,
		getRequestUsersForHotel: getRequestUsersForHotel,
		getRequestTypesByCategory: getRequestTypesByCategory,
		sendRequestMessage: sendRequestMessage,
		completeRequest: completeRequest,
		acceptRequest: acceptRequest,
		approveRequest: approveRequest,
		denyGuest: denyGuest,
		blockGuest: blockGuest,
		forwardRequest: forwardRequest,
		addRequestParticipant: addRequestParticipant,
		removeRequestParticipant: removeRequestParticipant,
		getRequestTypes: getRequestTypes,
		createRequest: createRequest,
		getHotelFromDevice: getHotelFromDevice,
		updateViewedActions: updateViewedActions,
		updateRequestEta: updateRequestEta,
		getRequestDetail: getRequestDetail,
		getStockResponses: getStockResponses,
		getUserSettings: getUserSettings,
		postUserSettings: postUserSettings,
		deleteRequest: deleteRequest,
		getRequestCountByStatus: getRequestCountByStatus
	};
}();