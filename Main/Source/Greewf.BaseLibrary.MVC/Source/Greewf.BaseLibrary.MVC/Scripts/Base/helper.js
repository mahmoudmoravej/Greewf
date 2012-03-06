jsHelper = new function () {


    this.addCommas = function (nStr) {
        if (nStr == null || nStr.length == 0) return '';
        nStr += '';
        x = nStr.split('.');
        x1 = x[0];
        x2 = x.length > 1 ? '.' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1)) {
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
        }
        return x1 + x2;
    }

    s1 = new Array("", "يک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه")
    s2 = new Array("ده", "يازده", "دوازده", "سيزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده")
    s3 = new Array("", "", "بيست", "سي", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود")
    s4 = new Array("", "صد", "دويست", "سيصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد")

    this.DigitsToAlphabet = function (z) {
        z = z.replace(/,/g, ''); //note: /,/ is regular expression , "g" means global i.e. all matches should be replaced
        z = parseInt(z);
        var result = "";

        if (z == 0)
            result = "صفر"
        else
            result = convert2(z, result)


        if (result == "Error")
            return "--خطا--"
        else
            return result;

    }

    function convert2(y, output) {
        if (y > 999999999 && y < 1000000000000)
        { bghb = (y % 1000000000); temp = y - bghb; bil = temp / 1000000000; output = convert3r(bil, output); output = output + " ميليارد"; if (bghb != 0) { output = output + " و "; output = convert2(bghb, output); } }
        else if (y > 999999 && y <= 999999999)
        { bghm = (y % 1000000); temp = y - bghm; mil = temp / 1000000; output = convert3r(mil, output); output = output + " ميليون"; if (bghm != 0) { output = output + " و "; output = convert2(bghm, output); } }
        else if (y > 999 && y <= 999999) { bghh = (y % 1000); temp = y - bghh; hez = temp / 1000; output = convert3r(hez, output); output = output + " هزار"; if (bghh != 0) { output = output + " و "; output = convert2(bghh, output); } }
        else if (y <= 999) output = convert3r(y, output); else output = "Error";
        return output;
    }

    function convert3r(x, output) {
        bgh = (x % 100); temp = x - bgh; sad = temp / 100;
        if (bgh == 0) { output = output + s4[sad] }
        else {
            if (x > 100) output = output + s4[sad] + " و ";
            if (bgh < 10) { output = output + s1[bgh] }
            else if (bgh < 20) { bgh2 = (bgh % 10); output = output + s2[bgh2] }
            else {
                bgh2 = (bgh % 10); temp = bgh - bgh2; dah = temp / 10;
                if (bgh2 == 0) { output = output + s3[dah] }
                else { output = output + s3[dah] + " و " + s1[bgh2] }
            }
        }
        return output;
    }

    this.center = function (id) {
        var x = $('#' + id);
        x.css({ top: '50%', left: '50%', margin: '-' + (x.height() / 2) + 'px 0 0 -' + (x.width() / 2) + 'px' });
    }

    this.trim = function (stringToTrim) {
        return stringToTrim.replace(/^\s+|\s+$/g, "");
    }

    this.loadAjax = function (url, dest) {
        $.ajax({
            url: url,
            cache: false,
            success: function (html) {
                dest.html(html);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                dest.html(xhr.responseText);
            }
        });
    }

    this.handlePageLeave = function (submitFormId, msg) {
        return; // TODO: has problem in ajax-loaded forms (becuase of same window in all cases)
        $(window).bind('beforeunload', function (s) {
            if (msg == null)
                msg = "شما در حال ترک این صفحه هستید در حالیکه ممکن است تغییراتی ذخیره نشده در آن داشته باشید. در صورت ترک این صفحه این تغییرات را از دست می دهید. تصمیم شما چیست؟";
            return msg;
        });
        $("form", "#" + submitFormId).submit(function () {
            window.onbeforeunload = null;
        });

    }

};

persianHelper = new function () {

    this.displayDate = function (s) {
        if (s == null || jsHelper.trim(s) == '') return '';
        var result = s.substr(0, 4) + '/' + s.substr(4, 2) + '/' + s.substr(6, 2);
        if (result == '//') result = '';
        return result;
    }

    this.displayDateTime = function (s) {
        if (s == null || jsHelper.trim(s) == '') return '';
        return s.substr(0, 4) + '/' + s.substr(4, 2) + '/' + s.substr(6, 2) + ' - ' + s.substr(8, 2) + ':' + s.substr(10, 2);
    }

    this.GetDisplay = function (s) {
        if (s)
            return '';
        return 'none';
    }

    this.toPersianDate = function (date, considerTime) {
        //note persianDateConvertor.js should be added in advance
        if (date == null || date.length == 0) return '';
        return ToPersianDate(new Date(date), considerTime);
    }

};

layoutHelper = new function () {

    this.fakeClick = function (link, justMain) {
        if (parent.$.tabStripMain != null) {
            var item = $(document.body).append('<a href="' + link + '"></a>').children().last();
            parent.$.tabStripMain.AddTab(item);
            item.remove();
            layoutHelper.windowLayout.CloseTopMost();
        }
        else if (justMain != undefined && justMain == true)
            parent.window.location = link;
        else
            window.location = link;
    }

    Object.defineProperty(this, "core", {
        get: function () {
            return (parent.$.layoutCore != null) ? parent.$.layoutCore : $.layoutCore;
        }
    });

    Object.defineProperty(this, "windowLayout", {
        get: function () {
            return (parent.$.layoutCore != null) ? parent.$.windowLayout : $.windowLayout;
        }
    });

    Object.defineProperty(this, "tooltipLayout", {
        get: function () {
            return (parent.$.layoutCore != null) ? parent.$.tooltipLayout : $.tooltipLayout;
        }
    });

    this.isParentLayoutPresent = function () {
        return (parent.$.layoutCore != null);
    }

    this.windowLayoutActiveDocument = function () {
        return (parent.$.layoutCore != null) ? parent.document : document;
    }
    this.mainJqObject = function () {/*get the main JQuery object*/
        return (parent.$.layoutCore != null) ? parent.$ : $;
    }

    this.setInitialFocus = function () {
        var x = $(".editor-focus2", layoutHelper.windowLayoutActiveDocument()).filter(":last");
        if (x.length == 0)
            $(".editor-focus input").filter(":last").focus();
        else {
            layoutHelper.mainJqObject()(x).data('closeHandler', function () { if ($ != undefined) $(".editor-focus input").filter(":last").focus(); });
        }
    }
};

telerikHelper = new function () {

    this.setDefaultFilterToContains = function (sender) {
        this.setDefaultFilterTo(sender, 'substringof');
    }

    this.setDefaultFilterTo = function (sender, filter) {
        $(sender).find(".t-filter").click(function () {
            setTimeout(function () {
                $(".t-filter-operator").each(function () {
                    $(this).val(filter);
                });
            });
        });
    }

    this.gridBeginEdit = function () {
        $('.t-grid-cancel').click(function () { telerikHelper.gridEndEdit(); });  //we need it because of no call to data-bound event on new-row cancelation
        $('button[type="submit"]').attr('disabled', true).addClass('t-state-disabled');
        $('.t-grid-add').css('visibility', 'hidden');
    }

    this.gridEndEdit = function () {
        $('button[type="submit"]').removeAttr('disabled').removeClass('t-state-disabled');
        $('.t-grid-add').css('visibility', 'visible');
    }

    this.resizeGridTo = function (gridId, size) {
        var grid = $('#' + gridId);
        size = size - $('.t-grouping-header', grid).outerHeight() - $('.t-grid-header', grid).outerHeight() - $('.t-grid-bottom', grid).outerHeight();
        $('.t-grid-content', grid).css("height", size - 3);
    }
};

mvcHelper = new function () {

    this.isRelatedFormValid = function (tag, isUnobtrusiveForm) {
        if (typeof (tag) == typeof ('')) tag = '#' + tag;
        var form = $(tag).closest('form')[0];

        if (isUnobtrusiveForm) {//to increase the speed!
            return $(form).valid();
        }
        else if (Sys.Mvc.FormContext) {
            var x = Sys.Mvc.FormContext.getValidationForForm(form);
            if (x != undefined && x != null)
                return x.validate('submit').length == 0;
        }
        return $(form).valid(); //unknow condition! so we rely on jquery validation
    }


};
