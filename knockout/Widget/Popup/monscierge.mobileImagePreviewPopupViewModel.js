function MobileBackgroundPreviewPopupViewModel(image, logo, propertyName) {
    var self = this;

    self.HotelImage = ko.observable(image);
    self.HotelLogo = ko.observable(logo);
    self.PropertyName = ko.observable(propertyName);

    self.Close = function () {
        ConnectCMS.MainViewModel.ShowOverlay(false);
    };

    self.Dispose = function () {
        var node = $('#popup-content > .innerContent');
        if (node.length == 0) return;
        ko.cleanNode(node[0]);
        $('#popup-content').html('');
    };

    self.Ok = function () {
        ConnectCMS.MainViewModel.ShowOverlay(false);
    };
};