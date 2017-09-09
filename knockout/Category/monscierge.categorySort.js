function SortCategoryByName(a, b) {
	var result = 0;

	if (a != null && b != null) {
		var aName = a.Name();
		var bName = b.Name();

		if (aName < bName)
			result = -1;
		else if (aName > bName)
			result = 1;
		else
			result = 0;
	}

	return result;
};

function SortCategoryByOrder(a, b) {
	return a.Order() - b.Order();
};