(function ($) {

    layoutCore = {};
    var progressHtml = '<div isProgress="1" class="bigprogress-icon t-content" style="width:99%;height:97%;position:absolute;" ></div>';
    var widgetManager = { changeTitle: '' };
    /*
    widgetManager.changeTitle(widgetTitle,title);
    layoutCore.OpenWidget = function (widgetLayout, sender, link, title, ownerWindow) {
    
    */


    function changeWidgetTitle(widgetLayout, widgetTitle, title) {
        widgetLayout.changeTitle(widgetTitle, title);
    }

    layoutCore.confirmRefresh = function (sender) {
        var confirm = true;
        if ($(sender).attr('windowActionsNeedConfirm') == 'true')
            confirm = window.confirm("آیا نسبت به بازخوانی مجدد اطلاعات این پنجره مطمئن هستید؟");
        return confirm;
    }

    layoutCore.confirmClose = function (sender, autoClose) {
        var confirm = true;
        if (!autoClose && $(sender).attr('windowActionsNeedConfirm') == 'true')
            confirm = window.confirm("آیا نسبت به بستن این پنجره مطمئن هستید؟");
        return confirm
    }

    layoutCore.refreshContent = function (widgetLayout, widgetTitle, widget) {
        changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...');
        var ajaxContent = $("#addedAjaxWindowContentContainer", widget);
        if (ajaxContent.length > 0)//ajax refresh
            loadThroughAjax(widgetLayout, widget, widgetTitle, null, $(ajaxContent).attr('link'));
        else//iframe refresh
        {
            var ifrm = $("iframe", widget)[0];
            //$(ifrm).css('visibility', 'hidden');
            $('div', $(ifrm).parent()).show();
            ifrm.contentWindow.onbeforeunload = null; //to avoid getting any confirmation if provided
            ifrm.contentWindow.location.reload(true);
            $(ifrm).attr('isrefresh', 'true');
        }

    }

    layoutCore.OpenWidget = function (widgetLayout, sender, link, title, ownerWindow) {
        var isModal = $(sender).attr('winNoModal') == undefined;
        var widgetInfo = widgetLayout.makeReadyToShow(sender, link, title, ownerWindow);

        var widget = widgetInfo.widget;
        var widgetTitle = widgetInfo.widgetTitle;


        var winWidth = $(sender).attr('winwidth');
        var winHeight = $(sender).attr('winheight');
        var maximaizable = $(sender).attr('winNoMaximaizable') == undefined;
        var winMax = $(sender).attr('winMax');
        var doAjax = $(sender).attr('ajax');

        link = correctLink(link, doAjax != undefined, true, true);

        //ajax or iframe?
        if (doAjax != undefined) {//ajax request : pure mode
            loadThroughAjax(widgetLayout, widget, widgetTitle, title, link, function () {
                var contentContainer = $('#addedAjaxWindowContentContainer', widget);
                correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, true, contentContainer.outerHeight(), contentContainer.outerWidth());
            });

        }
        else {//iframe request : simple mode
            widgetLayout.setContent(widget, progressHtml + '<iframe frameborder="0" style="width:100%;height:99%;visibility:visible;" src="' + link + '"></iframe>');


            $("iframe", widget).load(function () {
                $('div[isProgress]', $(this).parent()).hide();
                //$(this).css('visibility', 'visible'); //1:jquery hide/show methods makes some problem with inner content,2:making invisible makes problem in first field focusing
                changeWidgetTitle(widgetLayout, widgetTitle, title == '' ? (this.contentWindow.document != undefined ? this.contentWindow.document.title : '') : title);

                var correctingSizeCondition = $(this).attr('isrefresh') != 'true' && this.contentWindow.location.toString().indexOf("/SavedSuccessfully") == -1 && this.contentWindow.location.hash.toString().indexOf('successfullysaved') == -1;
                correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, correctingSizeCondition, $(this.contentWindow.document.body).outerHeight(), $(this.contentWindow.document.body).outerWidth());

                handleSpecialPagesByLink(widgetLayout, this.contentWindow.location);

            });
        }

        widgetLayout.show(widget);


    }

    function handleSpecialPagesByLink(widgetLayout, location, linkHash) {
        var handled = false;
        var link = location;
        if (linkHash == null) linkHash = ''; //todo : linkhash is null in ajax mode.

        if (location.hash != undefined) {//means window.loaction is passed
            link = location.toString();
            linkHash = location.hash.toString();
        }

        if (link.indexOf("/SavedSuccessfully") > 0 || linkHash.indexOf('successfullysaved') > 0) {
            widgetLayout.CloseAndDone(location.hash != undefined ? location : null); //when ajax request
            handled = true;
        }
        else if (link.indexOf("/accessdenied") > 0) {
            widgetLayout.CloseTopMost();
            handled = true;
        }

        return handled;

    }

    function correctLink(link, isPure, isInWindow, inclueUrlInContent) {
        if (link.indexOf('?') == -1)
            link = link + "?";
        else
            link = link + "&";
        link = link + (isPure ? checkToPaste(link, 'puremode=1') : checkToPaste(link, 'simplemode=1'));
        if (isInWindow) link = link + checkToPaste(link, '&iswindow=1');
        if (inclueUrlInContent) link = link + checkToPaste(link, '&includeUrlInContent=1');

        return link;
    }

    function checkToPaste(str, value) {
        return (str.indexOf(value) >= 0) ? '' : value;
    }

    function loadThroughAjax(widgetLayout, widget, widgetTitle, title, link, postSuccessAction) {
        widgetLayout.setContent(widget, progressHtml);
        $.ajax({
            url: link,
            cache: false,
            success: function (html) {
                insertAjaxContent(widgetLayout, widget, widgetTitle, title, link, html);
                if (postSuccessAction) postSuccessAction();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                widgetLayout.setContent(widget, xhr.responseText);
            }
        });
    }

    function insertAjaxContent(widgetLayout, widget, widgetTitle, title, link, html) {
        widgetLayout.setContent(widget, '<div id="addedAjaxWindowContentContainer" link="' + link + '">' + html + '</div>');
        var urlData = $('#currentPageUrl', widget); //when redirecting in ajax request
        if (urlData.length > 0) {
            link = urlData.text();
            $('#addedAjaxWindowContentContainer', widget).attr('link', link);
        }

        //handle page title
        var pageContentTitle = $('#currentPageTitle', widget);
        if (widgetTitle != null)
            if (pageContentTitle.length > 0)
                changeWidgetTitle(widgetLayout, widgetTitle, pageContentTitle.text());
            else
                changeWidgetTitle(widgetLayout, widgetTitle, '');

        //handle validation+content
        if (!handleSpecialPagesByLink(widgetLayout, link)) {//the page is not closed
            if (widgetTitle != null && title != null && title != '') changeWidgetTitle(widgetLayout, widgetTitle, title);
            enableValidation(widgetLayout, widget);
            ajaxifyInnerForms(widgetLayout, widget, widgetTitle);
        }

    }

    function correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, correctionCondition, contentHieght, contentWidth) {
        if (winMax != undefined) widgetLayout.maximize(widget);
        //correct windget size
        if (winMax == undefined && correctionCondition == true) {
            if (winHeight == undefined) {
                var maxHeight = $(window).height() - 100;
                var newHeight = contentHieght + widgetLayout.getTitleHeight(widgetTitle);
                if (newHeight > maxHeight) newHeight = maxHeight;
                widgetLayout.setHeight(widget, newHeight);
            }
            if (winWidth == undefined) {
                var maxWidth = $(window).width() - 100;
                var newWidth = contentWidth; //indeed it get its value from the window
                if (newWidth > maxWidth) newWidth = maxWidth;
                widgetLayout.setWidth(widget, newWidth);
            }
            if (winWidth == undefined || winHeight == undefined)
                widgetLayout.center(widget);
        }
    }

    function enableValidation(widgetLayout, widget) {
        //unobtrusive validation
        if ($.validator.unobtrusive != undefined && $.validator.unobtrusive != null) {
            $(widget).find('form').each(function (i, o) {
                $.validator.unobtrusive.parse(o);
            });
        }
        //non-unobtrusive validation
        if (Sys.Mvc.FormContext != undefined && Sys.Mvc.FormContext != null) {
            Sys.Mvc.FormContext._Application_Load();
        }
    }

    function ajaxifyInnerForms(widgetLayout, widget, widgetTitle) {
        //NOTE: just disable unobtrosive forms! TODO : handle old fasion ajax forms too
        $(widget).find('form:not([data-ajax*="true"])').each(function (i, o) {
            handleInnerFormSubmitButtons(o);
            $(o).submit(function () {
                if (!$(this).valid()) return false;
                changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...');
                var link = correctLink(this.action, true, true, true);
                $.ajax({
                    type: this.method.toLowerCase() == 'get' ? 'GET' : 'POST',
                    url: link,
                    cache: false,
                    data: appendSubmitButtonValue($(this).serialize(), this),
                    beforeSend: function () {
                        widgetLayout.setContent(widget, progressHtml);
                    },
                    success: function (html, status, xhr) {
                        insertAjaxContent(widgetLayout, widget, widgetTitle, null, link, html);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        insertAjaxContent(widgetLayout, widget, widgetTitle, null, link, xhr.responseText);
                    }
                });
                return false;
            });
        });

    }

    function appendSubmitButtonValue(serializedString, form) {
        var buttonName = $(form).attr('submiterName');
        if (buttonName == null || buttonName == '') return serializedString;
        if (serializedString.length > 0) serializedString = serializedString + '&';
        serializedString = serializedString + buttonName + '=' + $('[name="' + buttonName + '"]', form).val();
        return serializedString;
    }

    function handleInnerFormSubmitButtons(form) {
        $(':submit', form).each(function (i, o) {
            $(o).click(function () {
                $(this).closest('form').attr('submiterName', this.name);
            });
        });
    }


    layoutCore.HandleChildPageLinks = function (ownerWindow, isWindowLayout, isTabularLayout) {
        var pattern = 'a[justwindow]:not([tooltipWindow])';
        if (isWindowLayout) pattern = pattern + ',a[newwindow]:not([tooltipWindow])';
        $(pattern, ownerWindow.document).live('click', function () {
            if (parent.$.layoutCore != null) {
                parent.$.layoutCore.OpenWidget(parent.$.windowLayout, this, this.href, this.title, ownerWindow);
            }
            else
                $.layoutCore.OpenWidget($.windowLayout, this, this.href, this.title, window);
            return false;
        });

        $('a[justMain]', ownerWindow.document).live('click', function () {
            if (ownerWindow.location.toString().indexOf('iswindow=1') != -1)
                if (parent.$.layoutCore != null) parent.$.layoutCore.CloseTopMost();

            if (isTabularLayout)
                parent.$.tabStripMain.AddTab(this);
            else
                parent.location = this.href;

            return false;
        });

        $('a[tooltipWindow]', ownerWindow.document).live('click', function () {
            if (parent.$.layoutCore != null) {
                parent.$.layoutCore.OpenWidget(parent.$.tooltipLayout, this, this.href, this.title, ownerWindow);
            }
            else
                $.layoutCore.OpenWidget($.tooltipLayout, this, this.href, this.title, window);
            return false;
        });

        $('a[responsiveAjax]', ownerWindow.document).live('click', function () {
            handleResponsiveAjaxLink(this);
            return false;
        });


    }

    function handleResponsiveAjaxLink(link) {
        $.ajax({
            type: 'POST',
            url: link.href,
            cache: false,
            success: function (json) {
                if (json.ResponseType == 0)//information
                    layoutHelper.windowLayout.ShowInformationMessage(json.Message, '');
                else if (json.ResponseType == 1)//success
                    layoutHelper.windowLayout.ShowSuccessMessage(json.Message, '');
                else if (json.ResponseType == 2)//warning
                    layoutHelper.windowLayout.ShowWarningMessage(json.Message, 'هشدار');
                else if (json.ResponseType == 3)//failed
                    layoutHelper.windowLayout.ShowErrorMessage(json.Message, 'بروز خطا');
            },
            error: function (xhr, ajaxOptions, thrownError) {
                layoutHelper.windowLayout.ShowErrorMessage('<div style="overflow:auto;direction:ltr;max-width:400px;max-height:300px">' + xhr.responseText + '</div>', 'بروز خطا');
            }
        });
    }

    layoutCore.showSuccessMessage = function (msg,title) {
        layoutHelper.windowLayout.ShowSuccessMessage(msg, title);
    }

    layoutCore.showErrorMessage = function (msg, title) {
        layoutHelper.windowLayout.ShowErrorMessage(msg, title);
    }

    layoutCore.showWarningMessage = function (msg, title) {
        layoutHelper.windowLayout.ShowWarningMessage(msg, title);
    }

    layoutCore.OpenErrorWindow = function (content) {
        layoutHelper.windowLayout.OpenErrorWindow(content);
    }

    $.extend({ layoutCore: layoutCore });
})(jQuery);

