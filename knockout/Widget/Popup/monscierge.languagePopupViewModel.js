function LanguagePopupViewModel() {
    var self = this;

    self.SelectedCulture = ko.observable(ConnectCMS.SelectedCulture());
    self.SupportedCultures = ko.computed(function () {
        return ConnectCMS.SupportedCultures();
    });

    self.Close = function () {
        ConnectCMS.MainViewModel.ShowOverlay(false);
    };

    self.Dispose = function () {
        var node = $('#popup-content > .innerContent');
        if (node.length == 0) return;
        ko.cleanNode(node[0]);
        $('#popup-content').html('');
    };

    self.CanOk = ko.computed(function() {
        return self.SelectedCulture() != ConnectCMS.SelectedCulture();
    });

    self.Ok = function () {
        if (!self.CanOk())
            return;

        $.ajax({
            url: "/ConnectCMS/Utility/SetCulture",
            data: self.SelectedCulture(),
            type: "POST",
            success: function (result, textStatus, jqXHR) {
                ConnectCMS.MainViewModel.ShowOverlay(false);
                window.location = window.location.protocol + "//" + window.location.host + "/ConnectCMS";
            },
            error: function (jqXHR, textStatus, errorThrown) {
                ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
            }
        });
    };
};