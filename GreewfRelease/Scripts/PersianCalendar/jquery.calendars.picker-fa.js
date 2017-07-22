/* http://keith-wood.name/calendars.html
Farsi/Persian localisation for calendars datepicker for jQuery.
Javad Mowlanezhad -- jmowla@gmail.com. */
(function ($) {
    $.calendars.picker.regional['fa'] = {
        renderer: $.calendars.picker.defaultRenderer,
        prevText: '<span class="t-icon t-arrow-next" style="height:100%"></span>', prevStatus: 'نمايش ماه قبل',
        prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
        nextText: '<span class="t-icon t-arrow-prev"></span>', nextStatus: 'نمايش ماه بعد',
        nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
        currentText: 'امروز', currentStatus: 'نمايش ماه جاري',
        todayText: 'امروز', todayStatus: 'نمايش ماه جاري',
        clearText: '<span class="icon16 picture_empty-png"></span>', clearStatus: 'پاک کردن تاريخ جاري',
        closeText: '<span class="t-icon t-close">بستن</span>', closeStatus: 'بستن بدون اعمال تغييرات',
        yearStatus: 'نمايش سال متفاوت', monthStatus: 'نمايش ماه متفاوت',
        weekText: 'هف', weekStatus: 'هفتهِ سال',
        dayStatus: 'انتخاب D, M d', defaultStatus: 'انتخاب تاريخ',
        isRTL: true
    };
    $.calendars.picker.setDefaults($.calendars.picker.regional['fa']);
})(jQuery);
(function ($) {
    $.calendars.calendars.persian.prototype.regional['fa'] = {
        name: 'Persian', // The calendar name
        epochs: ['BP', 'AP'],
        monthNames: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
        'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
        monthNamesShort: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
        'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
        dayNames: ['يک شنبه', 'دوشنبه', 'سه شنبه', 'چهار شنبه', 'پنج شنبه', 'جمعه', 'شنبه'],
        dayNamesShort: ['يک', 'دو', 'سه', 'چهار', 'پنج', 'جمعه', 'شنبه'],
        dayNamesMin: ['ي', 'د', 'س', 'چ', 'پ', 'ج', 'ش'],
        dateFormat: 'yyyy/mm/dd', // See format options on BaseCalendar.formatDate
        firstDay: 6, // The first day of the week, Sun = 0, Mon = 1, ...
        isRTL: true // True if right-to-left language, false if left-to-right
    };
})(jQuery);

$().ready(function () {
    persianCalendarHelper.reBind();
});


persianCalendarHelper = new function () {

    this.reBind = function (container) {
        var calendar = $.calendars.instance("persian", "fa");
        (container == null ? $(".pcalendar") : $(".pcalendar", '#' + container)).calendarsPicker($.extend({ calendar: calendar, dateFormat: 'yyyy/mm/dd', showAnim: 'slideDown' }, $.calendars.picker.regional['fa']));
        $.calendars.picker.setDefaults($.calendars.picker.regional['fa']);
    }

};