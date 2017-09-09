function RecommendationRecommendReorderViewModel(recommendationRecommendViewModel) {
	var self = this;

	self.RecommendationRecommendViewModel = recommendationRecommendViewModel;

	self.Enterprises = ko.observableArray(null);
	self.ErrorVisible = ko.observable(false);
	self.Executed = ko.observable(false);
	self.Executing = ko.observable(false);
	self.FilteredEnterprises = ko.observable(null);
	self.HelpVisible = ko.observable(true);
	self.HelpVisibleKey = 1;

	self.ErrorMessageViewModel = new ErrorMessageViewModel(ConnectCMS.Globalization.AnErrorHasOccured, self.ErrorVisible);
	self.HelpMessageViewModel = new HelpMessageViewModel(ConnectCMS.Globalization.ChangeTheDisplayedOrderForRecommendationsByDraggingEachLocationToItsDesiredPosition, self.HelpVisible, self.HelpVisibleKey, null);
	self.ProgressBarViewModel = new ProgressBarViewModel(ConnectCMS.Globalization.FetchingRecommendations, self.Executing);
	var selectedCategory = ko.observable(null);
	self.SelectedCategory = ko.computed({
		read: function () {
			return selectedCategory();
		},
		write: function (value) {
			selectedCategory(value);

			var enterprises = self.Enterprises();
			var filteredEnterprises;

			if (!ConnectCMS.IsNullOrEmpty(enterprises) && selectedCategory() != null) {
				var enterpriseLocations;
				var recommendedCategories;

				ko.utils.arrayForEach(enterprises, function (enterpriseRecommendReorderViewModel) {
					SetOrder(enterpriseRecommendReorderViewModel.EnterpriseLocations, self.Sort, function (enterpriseLocationRecommendReorderViewModel, order) {
						enterpriseLocationRecommendReorderViewModel.Order(order);
					});

					recommendedCategories = enterpriseRecommendReorderViewModel.RecommendedCategories();

					if (!ConnectCMS.IsNullOrEmpty(recommendedCategories))
						ko.utils.arrayForEach(recommendedCategories, function (orderModel) {
							if (selectedCategory().PKID == orderModel.Key) {
								if (filteredEnterprises == null)
									filteredEnterprises = new Array();

								filteredEnterprises.push(enterpriseRecommendReorderViewModel);
							}
						});
				});

				SetOrder(filteredEnterprises, ConnectCMS.Enterprises.SortEnterpriseByOrder, function (enterpriseReorderViewModel, order) {
					enterpriseReorderViewModel.Order(order);
				});
			}

			self.FilteredEnterprises(filteredEnterprises);
		}
	});
	self.ToolBarEnabled = ko.computed(function () {
		return !self.Executing();
	});

	self.CategoryNoDataVisible = ko.computed(function () {
		var result = true;

		if (self.SelectedCategory() != null)
			result = false;

		return result;
	});
	self.NoDataVisible = ko.computed(function () {
		var result = true;

		if (!ConnectCMS.IsNullOrEmpty(self.FilteredEnterprises))
			result = false;

		return result;
	});

	self.CategoryNoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.SelectACategory, null, self.CategoryNoDataVisible);
	self.NoDataViewModel = new NoDataViewModel(ConnectCMS.Globalization.NoRecommendationsWereFound, null, self.NoDataVisible);

	self.ClickBack = function () {
		self.OnClickBack(true);
	};
	self.ClickUpdate = function () {
		var enterprises = self.Enterprises();

		if (!ConnectCMS.IsNullOrEmpty(enterprises)) {
			ConnectCMS.ShowPleaseWait();

			var enterprisesReorder;

			ko.utils.arrayForEach(enterprises, function (enterpriseReorderViewModel) {
				if (enterpriseReorderViewModel.HasSelfChanged()) {
					enterpriseReorderViewModel.commitData(true);

					if (enterprisesReorder == null)
						enterprisesReorder = new Array();

					enterprisesReorder.push(enterpriseReorderViewModel.getData());
				}
			});

			if (!ConnectCMS.IsNullOrEmpty(enterprisesReorder)) {
				var data = {
					deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
					enterprises: enterprisesReorder
				};

				$.ajax({
					url: "/ConnectCMS/Recommendation/UpdateRecommendationReorder",
					type: "POST",
					contentType: 'application/json; charset=utf-8;',
					dataType: 'json',
					data: JSON.stringify(data),
					success: function (result, textStatus, jqXHR) {
						self.Executed(false);
						self.GetRecommendedOnDevice();
						self.OnClickBack(false);
					},
					error: function (jqXHR, textStatus, errorThrown) {
						self.ErrorVisible(true);
						ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
					},
					complete: function (jqXHR, textStatus) {
						ConnectCMS.HidePleaseWait();
					}
				});
			} else
				ConnectCMS.HidePleaseWait();
		}
	};
	self.GetRecommendedOnDevice = function () {
		self.Executed(false);
		self.Executing(true);

		$.ajax({
			url: "/ConnectCMS/Recommendation/GetRecommendedEnterprisesOnDevice",
			data: {
				deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID()
			},
			type: "POST",
			success: function (result, textStatus, jqXHR) {
				self.Enterprises($.map(result, function (enterpriseModel) {
					return new ConnectCMS.Enterprises.EnterpriseRecommendReorderViewModel(enterpriseModel, { array: self.FilteredEnterprises });
				}));
			},
			error: function (jqXHR, textStatus, errorThrown) {
				self.ErrorVisible(true);
				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			},
			complete: function (jqXHR, textStatus) {
				self.Executed(true);
				self.Executing(false);
			}
		});
	};
	self.OnClickBack = function (revertData) {
		NavigateDispose(self.RecommendationRecommendViewModel.RecommendationViewModel.SelectedContentIndex, self.RecommendationRecommendViewModel.RecommendationViewModel.NavigationElement);
		self.ErrorVisible(false);
		self.SelectedCategory(null);

		if (revertData != null && revertData == true) {
			var enterprises = self.Enterprises();

			if (!ConnectCMS.IsNullOrEmpty(enterprises))
				ko.utils.arrayForEach(enterprises, function (enterpriseViewModel) {
					enterpriseViewModel.revertData(true);
				});
		}
	};
	self.OnSortUpdate = function (event, ui, viewModel, index) {
		if (index != null && viewModel != null) {
			var order = viewModel.Order();

			if (order != null) {
				index++;

				var position = index - order;

				viewModel.OnMove(index, position);

				var array = viewModel.Array();
				var selectedCategory = self.SelectedCategory();

				if (!ConnectCMS.IsNullOrEmpty(array) && selectedCategory != null) {
					var recommendedCategories;

					ko.utils.arrayForEach(array, function (arrayViewModel) {
						recommendedCategories = arrayViewModel.RecommendedCategories();

						if (!ConnectCMS.IsNullOrEmpty(recommendedCategories))
							ko.utils.arrayForEach(recommendedCategories, function (orderViewModel) {
								if (orderViewModel.Key == selectedCategory.PKID)
									orderViewModel.Order(arrayViewModel.Order());
							});
					});
				}
			}
		}
	};

	self.SortableViewModel = new ConnectCMS.Widgets.Sortable.SortableViewModel(self.FilteredEnterprises, false, null, self.OnSortUpdate);

	self.GetRecommendedOnDevice();
};