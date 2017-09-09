function BaseViewModel(baseViewModel, ref) {
	var self = this;

	if (baseViewModel != null) {
		var isObservable;
		var value1;

		if (ref == null)
			ref = true;

		for (var property1 in baseViewModel) {
			exists = false;

			for (var property2 in self)
				if (property1 == property2)
					exists = true;

			if (!exists) {
				value1 = baseViewModel[property1];

				isObservable = ko.isObservable(value1);

				if (!ref && isObservable)
					if (ko.isComputed(value1))
						self[property1] = value1;
					else
						self[property1] = ConnectCMS.Framework.ModelObservable(ko.utils.unwrapObservable(value1));
				else
					self[property1] = value1;
			}
		}
	}
};