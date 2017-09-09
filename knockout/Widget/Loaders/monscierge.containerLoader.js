ko.bindingHandlers.containerLoader = {
    'init': function (element, valueAccessor) {
        var vm = ko.utils.unwrapObservable(valueAccessor());

        var divElements = "";

        divElements += "<div class='containerLoader' data-bind='allowBindings: false, visible: Visible'>";
        divElements += "    <div class='overlay'></div>";
        divElements += "    <div class='flexSpacer'>";
        divElements += "    </div>";
        divElements += "    <div class='modal'>";
        divElements += "        <div class='loadingText'>";
        divElements += "            <span class='header' data-bind='text: HeaderText'>";
        divElements += "            </span>";
        divElements += "        </div>";
        divElements += "        <div class='loadingIcon'>";
        divElements += "        </div>";
        divElements += "        <div class='loadingText'>";
        divElements += "            <span class='footer' data-bind='text: FooterText'>";
        divElements += "            </span>";
        divElements += "        </div>";
        divElements += "    </div>";
        divElements += "    <div class='flexSpacer'>";
        divElements += "    </div>";
        divElements += "</div>";

        $(element).prepend(divElements);

        var node = $(element).find('.containerLoader')[0];
        if (node != null) {
            ko.cleanNode(node);
            ko.applyBindings(vm, node);
        }
    },
};