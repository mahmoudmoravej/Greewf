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
        handleAutoSubmit: false,
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

    layoutCore.onClose = function (sender, ownerWindow, autoClose, widget) {
        if (!autoClose) {//manual close   
            $.layoutCore.handleCloseCallBack(sender, null, ownerWindow, false, true, widget);
        }
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

    layoutCore.handleCloseCallBack = function (sender, data, ownerWindow, isSuccessfulFlagUp, isClosedManually, widget) {
        var callBack = $(sender, ownerWindow).attr('windowcallback');
        if (widget && widget.htmlTag) {
            if (data == null) data = {};
            var urlData = $('#currentPageUrl', widget.htmlTag);
            $.extend(data, { pageUrl: urlData.text() });
        }

        if (callBack) {
            if (ownerWindow[callBack])
                ownerWindow[callBack].apply(this, new Array(sender, data, isSuccessfulFlagUp, isClosedManually));
            else
                console.warn('the passed callback function (' + callBack + ') is not defined! check the spell or function existance.');
        }
        else if (ownerWindow.Layout_DoneSuccessfullyCallBack)
            ownerWindow.Layout_DoneSuccessfullyCallBack(sender, data, isSuccessfulFlagUp, isClosedManually);

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

        link = layoutHelper.formAjaxifier.correctLink(link, doAjax, true, true, widgetLayout.getTypeCode());

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
            widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout) + '<iframe frameborder="0" style="width:0px;height:0px;visibility:hidden;position:absolute;" src="' + link + '"/>');//NOTE! TOO Important : we don't use display:none becuase in firefox & IE it doesnt call the onload event for iframe!

            $("iframe:first", widget.htmlTag).load(function () {//note : just one iframe is alowed
                var sameOrigin = this.contentWindow != null && this.contentWindow.document != null; //same origin policy makes document == null for external URLs
                if (sameOrigin)
                    if (handleSpecialPages(widgetLayout, widget, widgetTitle, title, this.contentWindow.location, null, this.contentWindow.document.body.innerText, null, this)) return;

                $(this).data('contentLoaded', true);
                $('div[isProgress]', $(this).parent()).hide();
                $(this).css({ visibility: 'visible', width: '100%', height: '99%', position: 'static' }); //1:jquery hide/show methods makes some problem with inner content,2:making invisible makes problem in first field focusing
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

    function handleSpecialPages(widgetLayout, widget, widgetTitle, title, location, linkHash, data/*it may be a jquery object*/, postSuccessAction, iframeContainer) {
        var handled = handleSpecialPagesByLink(widgetLayout, widget, widgetTitle, title, location, linkHash, postSuccessAction, iframeContainer);
        if (handled) return true;
        var jsonResponse = null;
        //maybe response json results
        if (typeof (data) === "string") {//means html data. but it may be a json data in Iframe mode
            if (iframeContainer)
                try {
                    jsonResponse = $.parseJSON(data);
                } catch (e) {
                    return false;
                }
        }
        else if (data instanceof jQuery == false) //it is json result in ajax mode
            jsonResponse = data;

        if (jsonResponse) {
            var isSuccessFlagUp = layoutCore.handleResponsiveJsonResult(jsonResponse);
            widgetLayout.CloseAndDone(location.hash != undefined ? location : null, widget, isSuccessFlagUp); //when ajax request
            handled = true;
        }

        return handled;

    }

    function handleSpecialPagesByLink(widgetLayout, widget, widgetTitle, title, location, linkHash, postSuccessAction, iframeContainer) {
        var handled = false;
        var link = location;
        if (linkHash == null) linkHash = ''; //todo : linkhash is null in ajax mode.

        if (location.hash != undefined) {//means window.loaction is passed
            link = location.toString();
            linkHash = location.hash.toString();
        }

        link = link.toLowerCase();
        linkHash = linkHash.toLowerCase()

        if (link.indexOf("/savedsuccessfully") > 0 && link.indexOf("forcetopassedurl=1") > 0) {
            notifySuccess(widget);
            redirectToUrl(widgetLayout, widget, widgetTitle, title, decodeURI(jsHelper.getQueryStringParameterByName('url', link)), postSuccessAction, iframeContainer);
            handled = true;
        }
        else if ((link.indexOf("/savedsuccessfully") > 0 && link.indexOf("forcetopassedurl=1") == -1) || linkHash.indexOf('successfullysaved') > 0) {
            widgetLayout.CloseAndDone(location.hash != undefined ? location : null, widget, true); //when ajax request
            notifySuccess(widget);
            handled = true;
        }
        else if (link.indexOf("/accessdenied") > 0 || link.indexOf("/error") > 0) {
            widgetLayout.CloseTopMost(widget);
            handled = true;
        }

        return handled;

    }

    function notifySuccess(widget) {
        if (layoutCore.options.notifySuccess && jQuery.noticeAdd && !$(widget.sender).attr('discardSuccessMessage')) {
            jQuery.noticeAdd({
                text: layoutCore.options.notifySuccessMessage,
                stay: false,
                stayTime: layoutCore.options.notifySuccessTimeout
            });

        }
    }

    function redirectToUrl(widgetLayout, widget, widgetTitle, title, link, postSuccessAction, iframeContainer) {
        if (iframeContainer)
            iframeContainer.contentWindow.location = link;
        else//ajax call
            loadThroughAjax(widgetLayout, widget, widgetTitle, title, link, postSuccessAction);
    }

    function loadThroughAjax(widgetLayout, widget, widgetTitle, title, link, postSuccessAction) {
        layoutHelper.formAjaxifier.load({
            link: link,
            content: null,
            widgetHtmlTag: widget.htmlTag,
            widgetType: widgetLayout.getTypeCode(),
            beforeSend: function () {
                widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout));
            },
            contentReady: function (content, isErrorContent) {
                widgetLayout.setContent(widget, content);
            },
            widgetLinkCorrected: function (correctedLink, content) {
                if (handleSpecialPages(widgetLayout, widget, widgetTitle, title, correctedLink, null, content, postSuccessAction, null)) return { cancel: true };

                var pageContentTitle = $('#currentPageTitle', widget.htmlTag);
                if (widgetTitle != null)
                    if (pageContentTitle.length > 0)
                        changeWidgetTitle(widgetLayout, widgetTitle, pageContentTitle.text());
                    else
                        changeWidgetTitle(widgetLayout, widgetTitle, '');

                if (widgetTitle != null && title != null && title != '') changeWidgetTitle(widgetLayout, widgetTitle, title);

                return { cancel: false };
            },
            loadCompleted: function (isErrorContent) {
                handleCloseButtons(widgetLayout, widget);
                widgetLayout.contentLoaded(widget);

            },
            afterSuccessLoadCompleted: postSuccessAction,
            error: function (isCustomErrorPage, isAccessDeniedPage) {
                //if (isAccessDeniedPage)
                //widgetLayout.CloseTopMost(widget);//i think we dont need this anymore!
            },
            innerFormBeforeSubmit: function (form) {
                if (handlePageCloserSubmitButtons(form, widgetLayout, widget)) return { cancel: true }; //no submit for submit page closer ,they are just for validation purpose
                return { cancel: false };
            },
            innerFormBeforeSend: function () {
                var newContentPointer;
                if (layoutCore.options.showPageFormErrorsInExternalWindow)
                    newContentPointer = widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout), true);
                else
                    widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout));
                changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...'); //we do it here because we may need to access the current title in "setContent" function
                return newContentPointer;
            },
            retrieveOldContent: function (newContentPointer) {
                widgetLayout.retrieveOldContent(widget, newContentPointer);
            }
        });


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
        widgetLayout.CloseAndDone(data, widget, null, true);
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
            if (ownerWindow.location.toString().indexOf('iswindow=1') != -1)//inform window
                if (layoutHelper.isParentLayoutPresent()) layoutHelper.windowLayout.CloseTopMost();

            var container = $(this).closest('div#addedAjaxWindowContentContainer').attr('link');
            if (container && container.toLowerCase().indexOf('iswindow=1') != -1)//ajax window
                layoutHelper.windowLayout.CloseTopMost();

            if (isTabularLayout)
                $.tabStripMain.AddTab(this);
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
            url: encodeURI(link.href),
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
                layoutCore.handleResponsiveJsonResult(json);
                layoutCore.handleCloseCallBack(link, null, ownerWindow, true);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                layoutHelper.windowLayout.ShowErrorMessage('<div style="overflow:auto;direction:ltr;max-width:400px;max-height:300px">' + xhr.responseText + '</div>', 'بروز خطا');
            }
        });
    }

    layoutCore.handleResponsiveJsonResult = function (json) {

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
