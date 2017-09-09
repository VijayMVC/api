var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};

ConnectCMS.Requests.StockResponse = function (data) {
	this.stockResponseId = data.StockResponseId || 0;
	this.name = data.Name || '';
	this.message = data.Message || '';
};