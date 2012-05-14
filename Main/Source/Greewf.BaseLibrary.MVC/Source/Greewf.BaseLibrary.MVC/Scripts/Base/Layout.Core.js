
(function ($) {
    layoutCore = {};
    var widgetManager = { changeTitle: '' };

    layoutCore.progressHtml = function (widgetLayout) {
        return widgetLayout.progressHtml();
    }

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

    layoutCore.refreshContent = function (widgetLayout, widget, widgetTitle) {
        changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...');
        var ajaxContent = $("#addedAjaxWindowContentContainer", widget.htmlTag);
        if (ajaxContent.length > 0)//ajax refresh
            loadThroughAjax(widgetLayout, widget, widgetTitle, null, $(ajaxContent).attr('link'));
        else//iframe refresh
        {
            var ifrm = $("iframe", widget.htmlTag)[0];
            //$(ifrm).css('visibility', 'hidden');
            $('div', $(ifrm).parent()).show();
            ifrm.contentWindow.onbeforeunload = null; //to avoid getting any confirmation if provided
            ifrm.contentWindow.location.reload(true);
            $(ifrm).attr('isrefresh', 'true');
        }

    }

    layoutCore.handleCloseCallBack = function (sender, data, ownerWindow) {
        var callBack = $(sender, ownerWindow).attr('windowcallback');
        if (typeof (callBack) != 'undefined')
            ownerWindow[callBack].apply(this, new Array(sender, data));
        else if (typeof (ownerWindow.Layout_DoneSuccessfullyCallBack) != 'undefined')
            ownerWindow.Layout_DoneSuccessfullyCallBack(sender, data);

    }

    layoutCore.OpenWidget = function (widgetLayout, sender, link, title, ownerWindow) {
        var isModal = $(sender).attr('winNoModal') == undefined;
        var widgetInfo = widgetLayout.makeReadyToShow(sender, link, title, ownerWindow);

        var widget = widgetInfo.widget; //note : widget must have these three properties : sender, htmlTag, ownerWindow
        var widgetTitle = widgetInfo.widgetTitle;

        var winWidth = $(sender).attr('winwidth');
        var winHeight = $(sender).attr('winheight');
        var maximaizable = $(sender).attr('winNoMaximaizable') == undefined;
        var winMax = $(sender).attr('winMax');
        var doAjax = $(sender).attr('ajax');

        link = correctLink(link, doAjax != undefined, true, true, widgetLayout.getTypeCode());

        //ajax or iframe?
        if (doAjax != undefined) {//ajax request : pure mode
            loadThroughAjax(widgetLayout, widget, widgetTitle, title, link, function () {
                $('#addedAjaxWindowContentContainer', widget.htmlTag).attr('contentLoaded', 'true');
                var contentContainer = $('#addedAjaxWindowContentContainer', widget.htmlTag);
                correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, true, contentContainer.outerHeight(), contentContainer.outerWidth());
            });

        }
        else {//iframe request : simple mode
            widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout) + '<iframe frameborder="0" style="width:100%;height:99%;visibility:visible;" src="' + link + '"></iframe>');


            $("iframe", widget.htmlTag).load(function () {//note : just one iframe is alowed
                $(this).data('contentLoaded', true);
                $('div[isProgress]', $(this).parent()).hide();
                //$(this).css('visibility', 'visible'); //1:jquery hide/show methods makes some problem with inner content,2:making invisible makes problem in first field focusing
                changeWidgetTitle(widgetLayout, widgetTitle, title == '' ? (this.contentWindow.document != undefined ? this.contentWindow.document.title : '') : title);
                correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, getIframeResizingCondition(this), $(this.contentWindow.document.body).outerHeight(), $(this.contentWindow.document.body).outerWidth());

                handleSpecialPagesByLink(widgetLayout, widget, this.contentWindow.location);

            });
        }

        widgetLayout.show(widget);

    }

    layoutCore.widgetActivated = function (widgetLayout, widget, widgetTitle) {
        sender = widget.sender;
        var doAjax = $(sender).attr('ajax');
        var winWidth = $(sender).attr('winwidth');
        var winHeight = $(sender).attr('winheight');
        var winMax = $(sender).attr('winMax');

        if (doAjax != undefined) {//ajax request : pure mode
            if ($('#addedAjaxWindowContentContainer', widget.htmlTag).attr('contentLoaded') == undefined) return; //dont correct size if the content is not loaded
            var contentContainer = $('#addedAjaxWindowContentContainer', widget.htmlTag);
            correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, true, contentContainer.outerHeight(), contentContainer.outerWidth());
        }
        else {//iframe
            var frame = $("iframe", widget.htmlTag)[0]; //note : just one iframe is alowed
            if ($(frame).data('contentLoaded') == true) return; //dont correct size if the content is not loaded

            correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, getIframeResizingCondition(frame), $(frame.contentWindow.document.body).outerHeight(), $(frame.contentWindow.document.body).outerWidth());
        }

    }

    function getIframeResizingCondition(frame) {
        return $(frame).attr('isrefresh') != 'true' && frame.contentWindow.location.toString().indexOf("/SavedSuccessfully") == -1 && frame.contentWindow.location.hash.toString().indexOf('successfullysaved') == -1;
    }

    function handleSpecialPagesByLink(widgetLayout, widget, location, linkHash) {
        var handled = false;
        var link = location;
        if (linkHash == null) linkHash = ''; //todo : linkhash is null in ajax mode.

        if (location.hash != undefined) {//means window.loaction is passed
            link = location.toString();
            linkHash = location.hash.toString();
        }

        if (link.indexOf("/SavedSuccessfully") > 0 || linkHash.indexOf('successfullysaved') > 0) {
            widgetLayout.CloseAndDone(location.hash != undefined ? location : null, widget); //when ajax request
            handled = true;
        }
        else if (link.indexOf("/accessdenied") > 0) {
            widgetLayout.CloseTopMost(widget);
            handled = true;
        }

        return handled;

    }

    function correctLink(link, isPure, isInWindow, inclueUrlInContent, widgetType) {
        if (link.indexOf('?') == -1)
            link = link + "?";
        else
            link = link + "&";
        link = link + (isPure ? checkToPaste(link, 'puremode=1') : checkToPaste(link, 'simplemode=1'));
        if (isInWindow) link = link + checkToPaste(link, '&iswindow=1');
        if (widgetType == 2) link = link + checkToPaste(link, '&istooltip=1'); //1:window , 2:tooltip
        if (inclueUrlInContent) link = link + checkToPaste(link, '&includeUrlInContent=1');

        return link;
    }

    function checkToPaste(str, value) {
        return (str.indexOf(value) >= 0) ? '' : value;
    }

    function loadThroughAjax(widgetLayout, widget, widgetTitle, title, link, postSuccessAction) {
        widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout));
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
        var urlData = $('#currentPageUrl', widget.htmlTag); //when redirecting in ajax request
        if (urlData.length > 0) {
            link = urlData.text();
            $('#addedAjaxWindowContentContainer', widget.htmlTag).attr('link', link);
        }

        //handle page title
        var pageContentTitle = $('#currentPageTitle', widget.htmlTag);
        if (widgetTitle != null)
            if (pageContentTitle.length > 0)
                changeWidgetTitle(widgetLayout, widgetTitle, pageContentTitle.text());
            else
                changeWidgetTitle(widgetLayout, widgetTitle, '');

        //handle validation+content
        if (!handleSpecialPagesByLink(widgetLayout, widget, link)) {//the page is not closed
            if (widgetTitle != null && title != null && title != '') changeWidgetTitle(widgetLayout, widgetTitle, title);
            enableValidation(widgetLayout, widget);
            ajaxifyInnerForms(widgetLayout, widget, widgetTitle);
            handleCloseButtons(widgetLayout, widget);
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
            $(widget.htmlTag).find('form').each(function (i, o) {
                $.validator.unobtrusive.parse(o);
            });
        }
        //non-unobtrusive validation
        if (window.Sys && Sys.Mvc && Sys.Mvc.FormContext) {
            Sys.Mvc.FormContext._Application_Load();
        }
    }

    function handleInnerFormSubmitButtons(form) {
        var buttons = $(':submit', form);
        buttons.each(function (i, o) {
            $(o).click(function () {
                if (this.name != null && this.name.length > 0) $(this).closest('form').attr('submiterName', this.name).data('submitter', o);
            });
        });

        return buttons.length == 1 ? buttons[0] : null; //as default button when exactly one submit button presents
    }

    function handleCloseButtons(widgetLayout, widget) {
        $(widget.htmlTag).find('[isPageCloser]:not(:submit)').each(function (i, o) {
            $(o).click(function () {
                doCloseWidget($(this), widgetLayout, widget);
            });
        });
    }

    function handlePageCloserSubmitButtons(form, widgetLayout, widget) {
        var submitter = $($(form).data('submitter'));
        if (submitter.attr('isPageCloser') != null) {
            doCloseWidget(submitter, widgetLayout, widget);
            return true;
        }
        return false;
    }

    function doCloseWidget(closerButton, widgetLayout, widget) {
        var dataFetcher = closerButton.attr('pageCloserDataFetcher');
        var data = null;
        if (typeof (dataFetcher) != 'undefined') data = widget.ownerWindow[dataFetcher].apply(closerButton, new Array(closerButton));
        widgetLayout.CloseAndDone(data, widget);
    }

    function ajaxifyInnerForms(widgetLayout, widget, widgetTitle) {
        //NOTE: just disable unobtrosive forms! TODO : handle old fasion ajax forms too
        $(widget.htmlTag).find('form:not([data-ajax*="true"])').each(function (i, o) {
            var defaultSubmitButton = handleInnerFormSubmitButtons(o);
            $(o).submit(function () {
                if (!$(this).valid()) return false;
                if (defaultSubmitButton != null && $(this).data('submitter') == null) $(this).data('submitter', defaultSubmitButton); //asume default button
                if (handlePageCloserSubmitButtons(this, widgetLayout, widget)) return false; //no submit , just for validation needs
                changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...');
                var link = correctLink(this.action, true, true, true, widgetLayout.getTypeCode());
                $.ajax({
                    type: this.method.toLowerCase() == 'get' ? 'GET' : 'POST',
                    url: link,
                    cache: false,
                    data: appendSubmitButtonValue($(this).serialize(), this),
                    beforeSend: function () {
                        widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout));
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



    layoutCore.HandleChildPageLinks = function (ownerWindow, isWindowLayout, isTabularLayout) {
        var pattern = 'a[justwindow]:not([tooltipWindow])';
        if (isWindowLayout) pattern = pattern + ',a[newwindow]:not([tooltipWindow])';
        $(pattern, ownerWindow.document).live('click', function () {
            layoutHelper.tooltipLayout.closeLastTip();
            if (layoutHelper.isParentLayoutPresent())
                layoutHelper.core.OpenWidget(layoutHelper.windowLayout, this, this.href, this.title, ownerWindow);
            else
                layoutHelper.core.OpenWidget(layoutHelper.windowLayout, this, this.href, this.title, window);
            return false;
        });

        $('a[justMain]', ownerWindow.document).live('click', function () {
            layoutHelper.tooltipLayout.closeLastTip();
            if (ownerWindow.location.toString().indexOf('iswindow=1') != -1)
                if (layoutHelper.isParentLayoutPresent()) layoutHelper.windowLayout.CloseTopMost();

            if (isTabularLayout)
                parent.$.tabStripMain.AddTab(this);
            else
                parent.location = this.href;

            return false;
        });

        $('a[tooltipWindow]', ownerWindow.document).live('click', function () {
            if (layoutHelper.isParentLayoutPresent())
                layoutHelper.core.OpenWidget(layoutHelper.tooltipLayout, this, this.href, this.title, ownerWindow);
            else
                layoutHelper.core.OpenWidget(layoutHelper.tooltipLayout, this, this.href, this.title, window);
            return false;
        });

        $('a[responsiveAjax]', ownerWindow.document).live('click', function () {
            handleResponsiveAjaxLink(this);
            return false;
        });


    }

    layoutCore.openWindowFor = function (sender, link, settings) {
        layoutCore.openWidgetFor(sender, link, settings, 1);
    }

    layoutCore.openTooltipFor = function (sender, link, settings) {
        var options = null;
        if (settings == null || (settings != null && settings.ajax != false))
            options = { ajax: '1' };
        //if (settings != null && settings.ajax == false)//becuase ajax property value is not important. 
        //options = { hideEvents: 'unfocus'};

        $.extend(options, settings);

        layoutCore.openWidgetFor(sender, link, options, 2);
    }

    layoutCore.openWidgetFor = function (sender, link, settings, widgetType) {
        sender.href = link;

        for (key in settings) {
            sender[key] = settings[key];
            $(sender).attr(key, settings[key]);
        }

        if (widgetType == 1)
            layoutHelper.core.OpenWidget(layoutHelper.windowLayout, sender, sender.href, sender.title, window);
        else if (widgetType == 2)
            layoutHelper.core.OpenWidget(layoutHelper.tooltipLayout, sender, sender.href, sender.title, window);
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

    layoutCore.showSuccessMessage = function (msg, title) {
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
