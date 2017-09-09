function mapDictionary(data) {
	var result = [];

	if (data != null) {
		var value;

		for (var key in data)
			if (data.hasOwnProperty(key)) {
				value = data[key];

				result.push(new KeyValuePairModel(key, value));
			}
	}

	return result;
};