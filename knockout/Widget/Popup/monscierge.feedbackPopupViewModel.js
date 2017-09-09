function FeedbackPopupViewModel() {
    var self = this;

    self.Feedback = ko.validatedObservable({
        Name: ko.observable().extend({
            required: true
        }),
        EmailAddress: ko.observable().extend({
            required: true,
            email: true
        }),
        Subject: ko.observable(),
        FeedbackType: ko.observable().extend({
            required: true
        }),
        Comments: ko.observable().extend({
            required: true
        }),
        FeedbackTypes: ko.observableArray([
            new KeyValuePairModel(0, ConnectCMS.Globalization.General),
            new KeyValuePairModel(1, ConnectCMS.Globalization.Account),
            //new KeyValuePair(2, ConnectCMS.Globalization.APIDeveloper),
            new KeyValuePairModel(3, ConnectCMS.Globalization.Billing),
            new KeyValuePairModel(4, ConnectCMS.Globalization.BugReport),
            new KeyValuePairModel(5, ConnectCMS.Globalization.Recommendations),
            new KeyValuePairModel(6, ConnectCMS.Globalization.SupportRequest),
            new KeyValuePairModel(7, ConnectCMS.Globalization.Upgrade),
            new KeyValuePairModel(8, ConnectCMS.Globalization.Other)
        ]),
        BuildJs: function() {
            var js = {};
            js.Name = self.Feedback().Name();
            js.EmailAddress = self.Feedback().EmailAddress();
            js.Subject = self.Feedback().Subject();
            js.FeedbackType = self.Feedback().FeedbackType();
            js.Comments = self.Feedback().Comments();
            return js;
        }
    });

    self.CanOk = ko.computed(function() {
       return self.Feedback.isValid();
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

    self.Ok = function () {
        if (!self.CanOk()) {
            self.Feedback.errors.showAllMessages();
            return;
        }

        $.ajax({
            url: "/ConnectCMS/Utility/SendFeedback",
            data: self.Feedback().BuildJs(),
            type: "POST",
            success: function (result, textStatus, jqXHR) {
                ConnectCMS.MainViewModel.ShowOverlay(false);
                var success = new PopupViewModel({
                    title: ConnectCMS.Globalization.Success,
                    header: ConnectCMS.Globalization.FeedbackSentHeader,
                    body: ConnectCMS.Globalization.FeedbackSentBody,
                    okButton: ConnectCMS.Globalization.Success
                });
                success.Open();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
            }
        });
    };
};