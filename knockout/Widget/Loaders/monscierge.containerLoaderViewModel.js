function ContainerLoaderBarViewModel(headerText, footerText, visible) {
    var self = this;
    self.Visible = visible == null ? ko.observable(false) : visible;
    self.HeaderText = headerText == null ? ko.observable() : headerText;
    self.FooterText = footerText == null ? ko.observable() : footerText;
};