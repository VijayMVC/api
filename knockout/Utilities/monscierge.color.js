ConnectCMS.FormatColor = function (color) {
	var result;

	if (!ConnectCMS.Strings.IsNullOrWhitespace(color)) {
		color = color.replace("#", "");

		var length = color.length;

		if (length > 6)
			color = color.substring(length - 6);

		color = "#" + color;

		result = color;
	}

	return result;
};