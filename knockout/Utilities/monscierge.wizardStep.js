function WizardStep(data, parent, preloaded) {
	var self = this;
	self.Parent = parent;

	self.Id = data.Id;
	self.Title = data.Title;
	self.ViewUrl = data.ViewUrl;
	self.UrlData = data.UrlData;
	self.ViewModel = data.ViewModel;
    self.ViewModelParams = data.ViewModelParams;
    self.Data = data.Data;
    self.ValidationOptions = data.ValidationOptions;

	self.BindingViewModel = self.Data != null ? ko.validatedObservable(self.ViewModel == null ? null : new self.ViewModel(self.Data, self.ViewModelParams)) : null;

	self.IsLoaded = ko.observable(preloaded == null ? false : preloaded);

    self.StepIsValid = data.StepIsValid != null ? data.StepIsValid : ko.observable(true);

	self.Load = function (onComplete) {
		$.ajax({
			url: self.ViewUrl,
			type: "POST",
			data: self.UrlData,
			success: function (result, textStatus, jqXHR) {
				self.Data = result.data;
				self.BindingViewModel = ko.validatedObservable(new self.ViewModel(self.Data, self.ViewModelParams));
				$('#' + self.Id).html(result.view);

				var bindingpoint = $('#' + self.Id).find('.bindingpoint');
				if (bindingpoint.length == 0) return;

				ko.applyBindingsWithValidation(self, bindingpoint[0], self.ValidationOptions);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function () {
			    if (onComplete != null && onComplete instanceof Function ) {
			        onComplete();
			    }
			}
		});
		self.IsLoaded(true);
	}
	self.NextStep = function () {
		if (data.NextStep == null) {
			self.Parent.NextStep();
		} else {
			data.NextStep();
		}
	};
	self.PreviousStep = function () {
		if (data.PreviousStep == null) {
			self.Parent.PreviousStep();
		} else {
			data.PreviousStep();
		}
	};
}