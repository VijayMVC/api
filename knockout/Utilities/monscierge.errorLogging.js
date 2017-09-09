ErrorLogging = function () {
	if(Raygun != null)
		Raygun.init('gSZ9ZaKhuBLFAHUXGjMNmQ==', { excludedHostnames: ['localhost'] });
	var log = function (e) {
		var errorId = Math.floor(Math.random() * 10000001);
		if(Raygun != null)
			Raygun.send(e, { errorCode: errorId });
		return errorId;
	};

	return {
		log: log
	}
}();