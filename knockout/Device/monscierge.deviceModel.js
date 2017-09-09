function DeviceModel(data) {
    DataModel.call(this, data);

    var self = this;

    self.PKID = ko.observable(data != null ? data.PKID : null);
    self.Name = ko.observable(data != null ? data.Name : null);
};

ExtendDataModel(DeviceModel);