(function ($) {
    layoutCore = {};
    var widgetManager = { changeTitle: '' };

    layoutCore.options = {
        notifySuccess: false,
        notifySuccessMessage: "تغییرات با موفقیت ذخیره شد",
        notifySuccessTimeout: 2000,
        ajax: false,
        responsiveAjaxProgress: true,
        showPageFormErrorsInExternalWindow: true,
        window: { autoCenteredGrowingSize: false, autoGrowingSize: false, autoButtonBar: true, autoBackHide: false }
    }

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
        var ajaxContent = $("#addedAjaxWindowContentContainer", widget.htmlTag);
        if (ajaxContent.length > 0)//ajax refresh
        {
            changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...');
            loadThroughAjax(widgetLayout, widget, widgetTitle, null, $(ajaxContent).attr('link'));
        }
        else//iframe refresh
        {
            var ifrm = $("iframe", widget.htmlTag)[0];
            var sameOrigin = this.contentWindow != null && this.contentWindow.document != null; //same origin policy makes document == null for external URLs
            if (!sameOrigin) return;
            changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...');

            $('div', $(ifrm).parent()).show();
            ifrm.contentWindow.onbeforeunload = null; //to avoid getting any confirmation if provided
            ifrm.contentWindow.location.reload(true);
            $(ifrm).attr('isrefresh', 'true');
        }

    }

    layoutCore.handleCloseCallBack = function (sender, data, ownerWindow, isSuccessfulFlagUp) {
        var callBack = $(sender, ownerWindow).attr('windowcallback');
        if (callBack)
            ownerWindow[callBack].apply(this, new Array(sender, data, isSuccessfulFlagUp));
        else if (ownerWindow.Layout_DoneSuccessfullyCallBack)
            ownerWindow.Layout_DoneSuccessfullyCallBack(sender, data, isSuccessfulFlagUp);

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
        doAjax = doAjax != undefined ? true : layoutCore.options.ajax;

        link = correctLink(link, doAjax, true, true, widgetLayout.getTypeCode());

        //ajax or iframe?
        if (doAjax) {//ajax request : pure mode
            loadThroughAjax(widgetLayout, widget, widgetTitle, title, link, function () {
                $('#addedAjaxWindowContentContainer', widget.htmlTag).attr('contentLoaded', 'true');
                var contentContainer = $('#addedAjaxWindowContentContainer', widget.htmlTag);
                correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, true, contentContainer.outerHeight(), contentContainer.outerWidth());
                setAutoGrowingSize(sender, widgetLayout, widget, widgetTitle);
            });

        }
        else {//iframe request : simple mode
            widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout) + '<iframe frameborder="0" style="width:100%;height:99%;display:none;" src="' + link + '"></iframe>');


            $("iframe", widget.htmlTag).load(function () {//note : just one iframe is alowed
                var sameOrigin = this.contentWindow != null && this.contentWindow.document != null; //same origin policy makes document == null for external URLs
                if (sameOrigin)
                    if (handleSpecialPages(widgetLayout, widget, this.contentWindow.location, null, this.contentWindow.document.body.innerText, true)) return;

                $(this).data('contentLoaded', true);
                $('div[isProgress]', $(this).parent()).hide();
                //$(this).css('visibility', 'visible'); //1:jquery hide/show methods makes some problem with inner content,2:making invisible makes problem in first field focusing
                $(this).show();
                changeWidgetTitle(widgetLayout, widgetTitle, title == '' ? (this.contentWindow.document != undefined ? this.contentWindow.document.title : '') : title);

                if (sameOrigin) {
                    correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, getIframeResizingCondition(this), $(this.contentWindow.document.body).outerHeight(), $(this.contentWindow.document.body).outerWidth());
                    setAutoGrowingSize(sender, widgetLayout, widget, widgetTitle);
                }


            });
        }

        widgetLayout.show(widget);

    }

    function getGrowingSizeOptions(sender) {
        var s = $(sender);
        var autoGrowingSize = s.attr('autoGrowingSize') != null ? true : layoutCore.options.window.autoGrowingSize;
        var autoCenteredGrowingSize = s.attr('autoCenteredGrowingSize') != null ? true : layoutCore.options.window.autoCenteredGrowingSize;

        return {
            enabled: autoGrowingSize || autoCenteredGrowingSize,
            autoCenter: autoCenteredGrowingSize
        };
    }

    function setAutoGrowingSize(sender, widgetLayout, widget, widgetTitle) {
        var options = getGrowingSizeOptions(sender);
        if (!options.enabled || widget == null) return;
        if (widget.autoSizeGrowerCallBack) clearInterval(widget.autoSizeGrowerCallBack);
        widget.autoSizeGrowerCallBack =
            setInterval(function () {
                if (widget.htmlTag[0].parentElement == null) clearInterval(widget.autoSizeGrowerCallBack);
                try {
                    layoutCore.maximizeToContent(widgetLayout, widget, widgetTitle, options.autoCenter);
                } catch (e) {
                    clearInterval(widget.autoSizeGrowerCallBack);
                }
            }, 500);
    }

    layoutCore.widgetActivated = function (widgetLayout, widget, widgetTitle) {
        sender = widget.sender;
        var doAjax = $(sender).attr('ajax');
        var winWidth = $(sender).attr('winwidth');
        var winHeight = $(sender).attr('winheight');
        var winMax = $(sender).attr('winMax');
        doAjax = doAjax != undefined ? true : layoutCore.options.ajax;

        if (doAjax) {//ajax request : pure mode
            if ($('#addedAjaxWindowContentContainer', widget.htmlTag).attr('contentLoaded') == undefined) return; //dont correct size if the content is not loaded
            var contentContainer = $('#addedAjaxWindowContentContainer', widget.htmlTag);
            correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, true, contentContainer.outerHeight(), contentContainer.outerWidth());
            setAutoGrowingSize(sender, widgetLayout, widget, widgetTitle);
        }
        else {//iframe
            var frame = $("iframe", widget.htmlTag)[0]; //note : just one iframe is alowed
            if ($(frame).data('contentLoaded') == true) return; //dont correct size if the content is not loaded

            if (frame.contentWindow.document != null) { //same origin policy makes location = null for external URLs
                correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, getIframeResizingCondition(frame), $(frame.contentWindow.document.body).outerHeight(), $(frame.contentWindow.document.body).outerWidth());
                setAutoGrowingSize(sender, widgetLayout, widget, widgetTitle);
            }
        }

    }

    function getIframeResizingCondition(frame) {
        if (frame.contentWindow.document == null) return false; //same origin policy makes location = null for external URLs
        return $(frame).attr('isrefresh') != 'true' && frame.contentWindow.location.toString().indexOf("/SavedSuccessfully") == -1 && frame.contentWindow.location.hash.toString().indexOf('successfullysaved') == -1;
    }

    function handleSpecialPages(widgetLayout, widget, location, linkHash, data, isIframeBody) {
        var handled = handleSpecialPagesByLink(widgetLayout, widget, location, linkHash);
        if (handled) return true;
        var jsonResponse = null;
        //maybe response json results
        if (typeof (data) === "string") {//means html data. but it may be a json data in Iframe mode
            if (isIframeBody)
                try {
                    jsonResponse = $.parseJSON(data);
                } catch (e) {
                    return false;
                }
        }
        else //it is json result in ajax mode
            jsonResponse = data;

        if (jsonResponse) {
            var isSuccessFlagUp = handleResponsiveJsonResult(jsonResponse);
            widgetLayout.CloseAndDone(location.hash != undefined ? location : null, widget, isSuccessFlagUp); //when ajax request
            handled = true;
        }

        return handled;

    }

    function handleSpecialPagesByLink(widgetLayout, widget, location, linkHash) {
        var handled = false;
        var link = location;
        if (linkHash == null) linkHash = ''; //todo : linkhash is null in ajax mode.

        if (location.hash != undefined) {//means window.loaction is passed
            link = location.toString();
            linkHash = location.hash.toString();
        }

        link = link.toLocaleLowerCase();
        linkHash = linkHash.toLocaleLowerCase()

        if (link.indexOf("/savedsuccessfully") > 0 || linkHash.indexOf('successfullysaved') > 0) {
            widgetLayout.CloseAndDone(location.hash != undefined ? location : null, widget, true); //when ajax request
            if (layoutCore.options.notifySuccess && jQuery.noticeAdd) {
                jQuery.noticeAdd({
                    text: layoutCore.options.notifySuccessMessage,
                    stay: false,
                    stayTime: layoutCore.options.notifySuccessTimeout
                });
            }
            handled = true;
        }
        else if (link.indexOf("/accessdenied") > 0 || link.indexOf("/error") > 0) {
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
                var result = insertAjaxContent(widgetLayout, widget, widgetTitle, title, link, html); //returns false when handled through special pages
                if (postSuccessAction && result) postSuccessAction();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                if (xhr.getResponseHeader("GreewfCustomErrorPage"))//custom error page
                    insertAjaxContent(widgetLayout, widget, widgetTitle, title, link, xhr.responseText); //returns false when handled through special pages
                else
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

        if (handleSpecialPages(widgetLayout, widget, link, null, html, false)) return false;

        //handle page title
        var pageContentTitle = $('#currentPageTitle', widget.htmlTag);
        if (widgetTitle != null)
            if (pageContentTitle.length > 0)
                changeWidgetTitle(widgetLayout, widgetTitle, pageContentTitle.text());
            else
                changeWidgetTitle(widgetLayout, widgetTitle, '');

        //handle validation+content
        if (widgetTitle != null && title != null && title != '') changeWidgetTitle(widgetLayout, widgetTitle, title);
        enableValidation(widgetLayout, widget);
        ajaxifyInnerForms(widgetLayout, widget, widgetTitle);
        handleCloseButtons(widgetLayout, widget);
        widgetLayout.contentLoaded(widget);
        return true;

    }

    function correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, correctionCondition, contentHieght, contentWidth, justGrow, discardCentering) {
        if (winMax != undefined) widgetLayout.maximize(widget);
        if (widgetLayout.isMaximized(widget)) return;
        //correct windget size
        if (winMax == undefined && correctionCondition == true) {
            var changingDone = false;
            if (winHeight == undefined) {
                var maxHeight = $(window).height() - 100;
                var newHeight = contentHieght + widgetLayout.getTitleHeight(widgetTitle) + widgetLayout.getFooterHeight(widget);
                if (newHeight > maxHeight) newHeight = maxHeight;
                changingDone = changingDone || widgetLayout.setHeight(widget, newHeight, justGrow);
            }
            if (winWidth == undefined) {
                var maxWidth = $(window).width() - 100;
                var newWidth = contentWidth; //indeed it get its value from the window
                if (newWidth > maxWidth) newWidth = maxWidth;
                changingDone = changingDone || widgetLayout.setWidth(widget, newWidth, justGrow);
            }
            if (discardCentering) return;
            if (winWidth == undefined || winHeight == undefined)
                if (changingDone) widgetLayout.center(widget);
        }
    }

    layoutCore.maximizeToContent = function (widgetLayout, widget, widgetTitle, adjustCenter) {
        var widgetHtmlTag = $(widget.htmlTag);
        var contentContainer = $('#addedAjaxWindowContentContainer', widgetHtmlTag);
        if (contentContainer.length == 0) return; //TODO : iframes resizing is not supported yet.
        var winWidth = $(widget.sender).attr('winwidth');
        var winHeight = $(widget.sender).attr('winheight');
        var winMax = $(widget.sender).attr('winMax');
        var realWidth = Math.max(contentContainer[0].scrollWidth, contentContainer.outerWidth());
        var realHeight = Math.max(contentContainer[0].scrollHeight, contentContainer.outerHeight());
        correctWidgetSize(widgetLayout, widget, widgetTitle, winMax, winWidth, winHeight, true, realHeight, realWidth, true, adjustCenter != true);
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
                var link = correctLink(this.action, true, true, true, widgetLayout.getTypeCode());
                var newContentPointer = null; //just when having external window show for errors to preserve current content in error conditions.

                $.ajax({
                    type: this.method.toLowerCase() == 'get' ? 'GET' : 'POST',
                    url: link,
                    cache: false,
                    data: appendSubmitButtonValue($(this).serialize(), this),
                    beforeSend: function () {
                        if (layoutCore.options.showPageFormErrorsInExternalWindow)
                            newContentPointer = widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout), true);
                        else
                            widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout));
                        changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...'); //we do it here because we may need to access the current title in "setContent" function
                    },
                    success: function (html, status, xhr) {
                        insertAjaxContent(widgetLayout, widget, widgetTitle, null, link, html);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        handleSubmittedFormAjaxErrorContent(widgetLayout, widget, widgetTitle, link, xhr, newContentPointer);
                    }
                });
                return false;
            });
        });

    }

    function handleSubmittedFormAjaxErrorContent(widgetLayout, widget, widgetTitle, link, xhr, newContentPointer) {
        if (layoutCore.options.showPageFormErrorsInExternalWindow) {
            if (xhr.getResponseHeader("GreewfCustomErrorPage"))//custom error page
            { //TODO : change it if your custom error content needs a different way to show.
                var x = $('.custom-error-page', widget.htmlTag);
                if (x.length == 0)
                    x = $("<div style='display:none' class='custom-error-page'></div>").appendTo(widget.htmlTag);
                x.html(xhr.responseText);

                //we remove these to avoid conflicting with current page (specially we need the previous one in refreshing click)
                $('#currentPageUrl', x).remove();
                $('#currentPageTitle', x).remove();

            }
            else//regular error
                layoutHelper.windowLayout.ShowErrorMessage('<div style="overflow:auto;direction:ltr;max-width:500px;max-height:600px">' + xhr.responseText + '</div>', 'بروز خطا');
            widgetLayout.retrieveOldContent(widget, newContentPointer);
        }
        else//internal view is ok
            insertAjaxContent(widgetLayout, widget, widgetTitle, null, link, xhr.responseText);

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
            handleResponsiveAjaxLink(this, ownerWindow);
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

    function handleResponsiveAjaxLink(link, ownerWindow) {
        var succeeded;
        $.ajax({
            type: 'POST',
            url: link.href,
            cache: false,
            beforeSend: function () {
                var progress;
                if (!$(link).attr('responsiveAjaxProgress'))
                    progress = layoutCore.options.responsiveAjaxProgress;
                else
                    progress = $(link).attr('responsiveAjaxProgress').toLowerCase() != 'false';

                if (!progress) return;
                window.setTimeout(function () {
                    if (succeeded) return;
                    layoutHelper.windowLayout.ShowProgressMessage();
                }, 400);
            },
            success: function (json) {
                succeeded = 1;
                handleResponsiveJsonResult(json);
                layoutCore.handleCloseCallBack(link, null, ownerWindow, true);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                layoutHelper.windowLayout.ShowErrorMessage('<div style="overflow:auto;direction:ltr;max-width:400px;max-height:300px">' + xhr.responseText + '</div>', 'بروز خطا');
            }
        });
    }

    function handleResponsiveJsonResult(json, widgetLayout, widget, location, linkHash) {

        var isSuccessFlagUp = false;

        if (json.ResponseType == 0)//information
        {
            isSuccessFlagUp = true;
            layoutHelper.windowLayout.ShowInformationMessage(json.Message, '');
        }
        else if (json.ResponseType == 1)//success
        {
            isSuccessFlagUp = true;
            layoutHelper.windowLayout.ShowSuccessMessage(json.Message, '');
        }
        else if (json.ResponseType == 2)//warning
        {
            isSuccessFlagUp = false;
            layoutHelper.windowLayout.ShowWarningMessage(json.Message, 'هشدار');
        }
        else if (json.ResponseType == 3)//failed
        {
            isSuccessFlagUp = false;
            layoutHelper.windowLayout.ShowErrorMessage(json.Message, 'بروز خطا');
        }
        return isSuccessFlagUp;


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

    layoutCore.ShowQuestionMessage = function (msg, title, options) {
        layoutHelper.windowLayout.ShowQuestionMessage(msg, title, options);
    }


    $.extend({ layoutCore: layoutCore });
})(jQuery);
