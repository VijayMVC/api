Toast = function() {
    toastr.options = {
        "positionClass": "toast-bottom-right"
    };

    var success = function(text, title) {
        if (title) {
            toastr.success(text, title);
        } else {
            toastr.success(text);
        }
        return this;
    };

    var error = function (text, title) {
        if (title) {
            toastr.error(text, title);
        } else {
            toastr.error(text);
        }

        return this;
    };

	var showGenericError = function(errorId)
	{
		toastr.error('An unexpected error has occurred. If the error persists, please contact Monscierge Support. Reference error code [' + errorId + '].', ConnectCMS.Globalization.ToastTitleError);	
	};

    return {
        success: success,
        error: error,
		showGenericError: showGenericError
    }
}();