//Global
var _scheduler;
var _calendar;

Date.prototype.addDays = function (days) {
    var dat = new Date(this.valueOf());
    dat.setDate(dat.getDate() + days);
    return dat;
};

Date.prototype.addHours = function (h) {
    this.setHours(this.getHours() + h);
    return this;
};

String.prototype.replaceAt = function (index, character) {
    return this.substr(0, index) + character + this.substr(index + character.length);
};

String.prototype.toCamel = function () {
    var str = this.toLowerCase();
    return str.replaceAt(0, str.substr(0, 1).toUpperCase());
};

//Scheduler Events
var _selectedDate;
var _eventWnd;

function SchedulerData(e) {
    if (!_view) _view = 'day';
    if (!_selectedDate)
        if (_scheduler)
            _selectedDate = kendo.toString(_scheduler.date(), 'g');
        else
            _selectedDate = kendo.toString(new Date(), 'g');
    return {
    	deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
        view: _view,
        selectedDate: _selectedDate
    };
}

function AllDayClicked() {
    if ($('#AllDayCheck').is(':checked')) {
        $('.EventTimePicker').hide();
    } else {
        $('.EventTimePicker').show();
    }
}

function EventSchedulerDataBinding() {
    var winH = $(window).height();
    var times = $('.k-scheduler-times');
    var contents = $('.k-scheduler-content');
    var allday = $('.k-scheduler-table.k-scheduler-header-all-day');
    var test = allday.find('tr').height();
    $(times[1]).height(winH - 200 - test);
    $(contents[0]).height(winH - 200 - test);
}

function EventSchedulerEdit(e) {
    $('.k-window-titlebar.k-header').addClass('k-state-selected');
    _eventWnd = $(e.container).data("kendoWindow");
    LoadRecurrence(e.event.recurrenceRule);
    AllDayClicked();
}

function EventSchedulerAdd(e) {
    $('.k-window-titlebar.k-header').addClass('k-state-selected');
    _eventWnd = $(e.container).data("kendoWindow");
}

function EventSchedulerSave(e) {
	e.event.DeviceId = ConnectCMS.MainViewModel.SelectedDevice().PKID();
    e.event.recurrenceRule = BuildRecurrenceString();
}

function EventSchedulerCancel(e) {
    _scheduler.dataSource.read();
}

function NewEvent() {
    var now = new Date();
    if (now.getMinutes() < 30)
        now.setMinutes(30);
    else {
        now.setMinutes(0);
        now.setHours(now.getHours() + 1);
    }
    _scheduler.addEvent({ start: now, end: now.addHours(1) });
}

// Recurrence
var _loadingRecurrence;
var _freq;
var _interval;
var _byday;
var _bymonth;
var _bymonthday;
var _bysetpos;
var _count;
var _until;
var daysArray = ["SU", "MO", "TU", "WE", "TH", "FR", "SA"];

function LoadRecurrence(rule) {
    if (!rule || rule == "") return;
    _loadingRecurrence = true;
    $('#RecurrenceCheck').trigger("click");
    var rules = rule.split(";");
    var frequency = "";
    var cD = ["1", "2", "3", "4", "-"];
    for (var i = 0; i < rules.length; i++) {
        var ruless = rules[i].split("=");
        switch (ruless[0]) {
            case "FREQ":
                frequency = ruless[1];
                $('#' + ruless[1].toCamel() + 'Radio').trigger("click");
                break;
            case "INTERVAL":
                $('#' + frequency.toCamel() + 'Interval').data('kendoNumericTextBox').value(ruless[1]);
                break;
            case "BYDAY":
                switch (frequency) {
                    case "WEEKLY":
                        $('.WeeklyDays').prop("checked", false);
                        var days = ruless[1].split(",");
                        for (var x = 0; x < days.length; x++) {
                            $('.WeeklyDays[value=' + days[x] + ']').prop("checked", true);
                        }
                        WeeklyDaysChanged();
                        break;
                    case "MONTHLY":
                        $('#MonthlyTypeCombo').trigger('click');
                        if ($.inArray(cD, ruless[1].substr(0, 1)) >= 0) {
                            var week = ruless[1].substr(0, ruless[1].length == 3 ? 1 : 2);
                            var day = ruless[1].substr(ruless[1].length == 3 ? 1 : 2, 2);
                            $('#MonthlyTypeComboFirst').data('kendoDropDownList').value(week);
                            $('#MonthlyTypeComboSecond').data('kendoDropDownList').value(day);
                        }
                        else {
                            $('#MonthlyTypeComboSecond').data('kendoDropDownList').value(ruless[1]);
                        }
                        break;
                    case "YEARLY":
                        $('#YearlyTypeCombo').trigger('click');
                        if ($.inArray(cD, ruless[1].substr(0, 1)) >= 0) {
                            var week = ruless[1].substr(0, ruless[1].length == 3 ? 1 : 2);
                            var day = ruless[1].substr(ruless[1].length == 3 ? 1 : 2, 2);
                            $('#YearlyTypeComboFirst').data('kendoDropDownList').value(week);
                            $('#YearlyTypeComboSecond').data('kendoDropDownList').value(day);
                        }
                        else {
                            $('#YearlyTypeComboSecond').data('kendoDropDownList').value(ruless[1]);
                        }
                        break;
                }
                break;
            case "BYSETPOS":
                switch (frequency) {
                    case "MONTHLY":
                        $('#MonthlyTypeComboFirst').data('kendoDropDownList').value(ruless[1]);
                        break;
                }
                break;
            case "BYMONTHDAY":
                switch (frequency) {
                    case "MONTHLY":
                        $('#MonthlyDayCount').data('kendoNumericTextBox').value(ruless[1]);
                        break;
                    case "YEARLY":
                        $('#YearlyDayCount').data('kendoNumericTextBox').value(ruless[1]);
                        break;
                }
                break;
            case "BYMONTH":
                if (rule.indexOf("BYMONTHDAY") >= 0) {
                    $('#YearlyTypeDayFirst').data('kendoDropDownList').value(ruless[1]);
                }
                else {
                    $('#YearlyTypeComboThird').data('kendoDropDownList').value(ruless[1]);
                }
                break;

            case "COUNT":
                $('#EndTypeAfter').trigger('click');
                $('#EndAfterCount').data('kendoNumericTextBox').value(ruless[1]);
                break;
            case "UNTIL":
                $('#EndTypeUntil').trigger('click');
                var y = ruless[1].substr(0, 4);
                var m = ruless[1].substr(4, 2);
                var d = ruless[1].substr(6, 2);
                $('#EndOnDate').data('kendoDatePicker').value(new Date(y, m - 1, d));
                break;
        }
    }
    _loadingRecurrence = false;
}

function RecurrenceClicked() {
    if ($('#RecurrenceCheck').is(':checked')) {
        $('#RecurrenceDiv').show();
        if (!_loadingRecurrence) $('#WeeklyRadio').trigger("click");
    } else {
        $('#RecurrenceDiv').hide();
        _freq = "";
        _interval = "";
        _byday = "";
        _bymonth = "";
        _bymonthday = "";
        _bysetpos = "";
        _count = "";
        _until = "";
    }

    _eventWnd.center();
}

function WeeklyDaysChanged() {
    var daysChecked = $('.WeeklyDays:checked');
    if (daysChecked.length == 0) {
        _byday = "";
        return;
    }
    _byday = "BYDAY=";
    daysChecked.each(function () {
        _byday += this.value + ",";
    });
    _byday = _byday.substr(0, _byday.length - 1);
    _byday += ";";
}

function MonthlyTypeChanged(val) {
    if (val == "Combo") {
        $('#MonthlyTypeComboFirst').data('kendoDropDownList').enable();
        $('#MonthlyTypeComboSecond').data('kendoDropDownList').enable();
        $('#MonthlyDayCount').data('kendoNumericTextBox').enable(false);
    } else {
        $('#MonthlyTypeComboFirst').data('kendoDropDownList').enable(false);
        $('#MonthlyTypeComboSecond').data('kendoDropDownList').enable(false);
        $('#MonthlyDayCount').data('kendoNumericTextBox').enable();
    }
}

function YearlyTypeChanged(val) {
    if (val == "Combo") {
        $('#YearlyTypeComboFirst').data('kendoDropDownList').enable();
        $('#YearlyTypeComboSecond').data('kendoDropDownList').enable();
        $('#YearlyTypeComboThird').data('kendoDropDownList').enable();
        $('#YearlyDayCount').data('kendoNumericTextBox').enable(false);
        $('#YearlyTypeDayFirst').data('kendoDropDownList').enable(false);
    } else {
        $('#YearlyTypeComboFirst').data('kendoDropDownList').enable(false);
        $('#YearlyTypeComboSecond').data('kendoDropDownList').enable(false);
        $('#YearlyTypeComboThird').data('kendoDropDownList').enable(false);
        $('#YearlyDayCount').data('kendoNumericTextBox').enable();
        $('#YearlyTypeDayFirst').data('kendoDropDownList').enable();
    }
}

function EndTypeChanged(val) {
    switch (val) {
        case "Never":
            $('#EndAfterCount').data('kendoNumericTextBox').enable(false);
            $('#EndOnDate').data('kendoDatePicker').enable(false);
            break;
        case "After":
            $('#EndAfterCount').data('kendoNumericTextBox').enable();
            $('#EndOnDate').data('kendoDatePicker').enable(false);
            break;
        case "On":
            $('#EndAfterCount').data('kendoNumericTextBox').enable(false);
            $('#EndOnDate').data('kendoDatePicker').enable();
            break;
        default:
    }
}

function RecurrenceChanged(val) {
    _freq = "";
    _interval = "";
    _byday = "";
    _bymonth = "";
    _bymonthday = "";
    _bysetpos = "";
    _count = "";
    _until = "";

    $(".RecurrenceDetailsDiv").hide();
    $('#' + val + 'Details').show();
    switch (val) {
        case "Daily":
            $('#DailyInterval').data('kendoNumericTextBox').value(1);
            $('#DayRadio').prop('checked', true);
            break;
        case "Weekly":
            $('#WeeklyInterval').data('kendoNumericTextBox').value(1);
            $('.WeeklyDays').prop("checked", false);
            $('#WD' + ($("#StartDate").data('kendoDatePicker').value().getDay())).prop('checked', true);
            WeeklyDaysChanged();
            break;
        case "Monthly":
            $('#MonthlyInterval').data('kendoNumericTextBox').value(1);
            $('#MonthlyTypeDay').prop('checked', true);
            $('#MonthlyDayCount').data('kendoNumericTextBox').value(1);
            $('#MonthlyTypeComboFirst').data('kendoDropDownList').value(1);
            $('#MonthlyTypeComboSecond').data('kendoDropDownList').value(daysArray[$("#StartDate").data('kendoDatePicker').value().getDay()]);
            break;
        case "Yearly":
            $('#YearlyInterval').data('kendoNumericTextBox').value(1);
            $('#YearlyTypeDay').prop('checked', true);
            $('#YearlyTypeDayFirst').data('kendoDropDownList').value($("#StartDate").data('kendoDatePicker').value().getMonth() + 1);
            $('#YearlyTypeComboThird').data('kendoDropDownList').value($("#StartDate").data('kendoDatePicker').value().getMonth() + 1);
            $('#YearlyDayCount').data('kendoNumericTextBox').value($("#StartDate").data('kendoDatePicker').value().getDate());
            $('#YearlyTypeComboFirst').data('kendoDropDownList').value(1);
            $('#YearlyTypeComboSecond').data('kendoDropDownList').value(daysArray[$("#StartDate").data('kendoDatePicker').value().getDay()]);
            break;
    }
}

function BuildRecurrenceString() {
    if (!$('#RecurrenceCheck').is(':checked')) return '';

    var frq = $('.RecurrenceTypeRadio:checked').val();
    _freq = "FREQ=" + frq.toUpperCase() + ";";
    _interval = "INTERVAL=" + $('#' + frq + "Interval").data('kendoNumericTextBox').value() + ";";
    switch (frq) {
        case "Daily":
            if ($('#WeekdayRadio').is(':checked'))
                _byday = "BYDAY=MO,TU,WE,TH,FR";
            break;
        case "Weekly":
            WeeklyDaysChanged();
            break;
        case "Monthly":
            if ($('#MonthlyTypeDay').is(":checked")) {
                _bymonthday = "BYMONTHDAY=" + $('#MonthlyDayCount').data('kendoNumericTextBox').value() + ';';
            } else {
                var mtc1 = $('#MonthlyTypeComboFirst').data('kendoDropDownList').value();
                var mtc2 = $('#MonthlyTypeComboSecond').data('kendoDropDownList').value();
                _byday = "BYDAY=" + mtc2 + ";";
                _bysetpos = "BYSETPOS=" + mtc1 + ';';
            }
            break;
        case "Yearly":
            if ($('#YearlyTypeDay').is(':checked')) {
                _bymonth = "BYMONTH=" + $('#YearlyTypeDayFirst').data('kendoDropDownList').value() + ";";
                _bymonthday = "BYMONTHDAY=" + $('#YearlyDayCount').data('kendoNumericTextBox').value() + ";";
            } else {
                _bymonth = "BYMONTH=" + $('#YearlyTypeComboThird').data('kendoDropDownList').value() + ";";
                _bysetpos = "BYSETPOS=" + $('#YearlyTypeComboFirst').data('kendoDropDownList').value() + ';';
                _byday = "BYDAY=" + $('#YearlyTypeComboSecond').data('kendoDropDownList').value() + ";";
            }
            break;
    }

    if ($('#EndTypeAfter').is(":checked")) {
        _count = "COUNT=" + $('#EndAfterCount').data('kendoNumericTextBox').value() + ';';
    } else {
        _until = "UNTIL=" + kendo.toString($('#EndOnDate').data('kendoDatePicker').value(), "yyyyMMddTHHmmssZ") + ';';
    }
    return _freq + _interval + _bysetpos + _byday + _bymonthday + _bymonth + _count + _until;
}
//@* Date Navigation *@
    var _view;

function EventSchedulerNavigate(e) {
    var firstDate = Date.now();
    var lastDate = Date.now();

    _view = e.view;
    _selectedDate = kendo.toString(e.date, "g");

    switch (e.view) {
        case "day":
            firstDate = e.date;
            lastDate = e.date;
            _calendar.value(e.date);
            break;
        case "week":
            _calendar.value(e.date);
            firstDate = e.date.addDays(0 - e.date.getDay());
            lastDate = firstDate.addDays(6);
            break;
        case "month":
            firstDate = new Date(e.date.getFullYear(), e.date.getMonth(), 1);
            lastDate = e.date.getMonth() == 11 ? new Date(e.date.getFullYear() + 1, 0, 1).addDays(-1) : new Date(e.date.getFullYear(), e.date.getMonth() + 1, 1).addDays(-1);
            _calendar.value(e.date);
            break;
        default:
            break;
    }

    for (var d = firstDate; d <= lastDate; d = d.addDays(1)) {
        if (d == e.date) continue;
        var day = $('#EventCalendar').find("a[data-value = '" + d.getFullYear() + "/" + d.getMonth() + "/" + d.getDate() + "']");
        day.parent().addClass('k-state-selected');
    }

    _scheduler.dataSource.read();
}

function EventCalendarChange(e) {
    var firstDate = Date.now();
    var lastDate = Date.now();
    var day = this.value();
    var view = _scheduler.view().name;
    _view = view;
    _selectedDate = kendo.toString(day, "g");
    switch (view) {
        case "day":
            firstDate = day;
            lastDate = day;
            _scheduler.date(day);
            break;
        case "week":
            firstDate = day.addDays(0 - day.getDay());
            lastDate = firstDate.addDays(6);
            _scheduler.date(day);
            break;
        case "month":
            firstDate = new Date(day.getFullYear(), day.getMonth(), 1);
            lastDate = day.getMonth() == 11 ? new Date(day.getFullYear() + 1, 0, 1).addDays(-1) : new Date(day.getFullYear(), day.getMonth() + 1, 1).addDays(-1);
            _scheduler.date(day);
            break;
        default:
    }
    for (var d = firstDate; d <= lastDate; d = d.addDays(1)) {
        if (d == day) continue;
        var dayA = $('#EventCalendar').find("a[data-value = '" + d.getFullYear() + "/" + d.getMonth() + "/" + d.getDate() + "']");
        dayA.parent().addClass('k-state-selected');
    }

    _scheduler.dataSource.read();
}
//@* Manage Resources *@
    var _eventResourceWindow;

function ManageResources(resourceType) {
    _eventResourceWindow.title(resourceType == "Group" ? "Manage Event Groups" : "Manage Event Locations");

    $.ajax({
        url: '/ConnectCMS/Events/ManageEventResources',
        data: {
        	deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
            resourceType: resourceType
        },
        success: function (data, textStatus, jqXHR) {
            _eventResourceWindow.content(data);
            _eventResourceWindow.center();
            _eventResourceWindow.open();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            debugger;
        },
    });
}

function EventResourceWindowClose() {
    _eventResourceWindow.close();
}
//@*New Resource*@
    var _newResourceId;
var _newResourceType;
var _newResourceWindow;

function NewResourceClick(resourceType) {
    _newResourceWindow.title(resourceType == "Group" ? "Add New Event Groups" : "Add new Event Locations");

    $.ajax({
        url: '/ConnectCMS/Events/AddEventResource',
        data: {
        	deviceId: ConnectCMS.MainViewModel.SelectedDevice().PKID(),
            resourceType: resourceType
        },
        success: function (data, textStatus, jqXHR) {
            _newResourceWindow.content(data);
            _newResourceWindow.center();
            _newResourceWindow.open();
        },
        error: function (jqXHR, textStatus, errorThrown) {
        },
    });
}

function NewResourceWindowClose(e) {
    if (_newResourceId > 0) {
        switch (_newResourceType) {
            case "Group":
                $('#GroupSelect').data('kendoDropDownList').dataSource.read();
                $('#GroupSelect').data('kendoDropDownList').value(_newResourceId);
                break;
            case "Location":
                $('#LocationSelect').data('kendoDropDownList').dataSource.read();
                $('#LocationSelect').data('kendoDropDownList').value(_newResourceId);
                break;
        }
    }
}