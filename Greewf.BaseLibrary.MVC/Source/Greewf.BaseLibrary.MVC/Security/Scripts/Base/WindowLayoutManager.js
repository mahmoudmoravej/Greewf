(function ($) {

    windowLayout = {};
    var activeWinsQueue = new Array(); // {sender: xxx, win:xxx,ownerWindow:xxx}
    var messageWins = new Array(); //{win:xxx , type:xxx}
    var inAutoHide;
    var inAutoClose = false;
    var progressHtml = '<div class="bigprogress-icon t-content" style="width:99%;height:97%;position:absolute;" ></div>';
    var debugMode = true;

    windowLayout.debug = function (value) {
        if (value != null) debugMode = value;
        return debugMode;
    };

    windowLayout.ShowInformationMessage = function (msg, title) {
        return showMessage(msg, title);
    }

    windowLayout.ShowSuccessMessage = function (msg, title) {
        return showMessage(msg, title, 1);
    }

    windowLayout.ShowErrorMessage = function (msg, title) {
        return showMessage(msg, title, 4);
    }

    windowLayout.ShowForbiddenMessage = function (msg, title) {
        return showMessage(msg, title, 3);
    }

    windowLayout.ShowWarningMessage = function (msg, title) {
        return showMessage(msg, title, 2);
    }

    getMessageIcon = function (type) {
        switch (type) {
            case 1: //success
                return 'check48-png';
            case 2: //warning
                return 'warning48-png';
            case 3: //security
                return 'forbidden48-png';
            case 4: //error
                return 'error48-png';
            case 5: //question
                return 'Question48-png';
            default:
                return 'info48-png';

        }
    }

    showMessage = function (msg, title, type) {
        template = "<table style='height:100%;width:300px;'><tr><td><img class='###' src='data:image/gif;base64,R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==' /></td><td style='vertical-align:middle;width:100%;white-space: nowrap;'>$$$</td></tr></table><button class='t-button editor-focus2' style='float:left'><span class='icon16 stop-png'></span>&nbsp;بستن</button>";
        var oldWin = null;
        $(messageWins).each(function (i, o) { if (o.type == type) oldWin = o; });

        if (oldWin != null) {
            oldWin.win.data('tWindow').close();
            oldWin.win.data('tWindow').destroy();
            oldWin.win = null;
        }

        msgBoxWindow = $.telerik.window.create({
            title: title,
            visible: false,
            resizable: false,
            draggable: false,
            actions: new Array('Close'),
            html: template.replace('$$$', msg).replace('###', getMessageIcon(type)),
            onClose: function (s) {
                var x = $('.editor-focus2', this);
                if (x.data('closeHandler') != undefined) x.data('closeHandler')();
                x.attr('class', '');
                $(s).remove();
            }
        });

        if (oldWin == null)
            messageWins.push({ type: type, win: msgBoxWindow });
        else
            oldWin.win = msgBoxWindow;

        $('iframe', msgBoxWindow).remove();
        //$('.t-content', msgBoxWindow).css('background-color', '#99FF99');
        $('button.t-button', msgBoxWindow).click(function () { $(msgBoxWindow).data('tWindow').close(); });
        $(msgBoxWindow).keyup(function (e) { if (e.keyCode == 27) $(this).data('tWindow').close(); });

        var w = msgBoxWindow.data('tWindow');
        w.center().open();
        $('button.t-button', msgBoxWindow).focus();

    }

    function fetchIcon(sender) {
        var ico = $('span[class*="icon16"]:first', sender);
        if (ico.length == 0)
            ico = $('span[class*="t-sprite"]:first', sender);
        if (ico.length == 0)
            ico = $('img:first', sender);
        if (ico.length > 0) {
            return $("<span>").append($(ico[0]).clone().css('vertical-align', 'middle')).html();
        }
        return '';
    }

    function changeWindowTitle(winTitleElement, title) {
        var ico = winTitleElement.children()[0];
        ico = ico == undefined ? '' : $("<span>").append($(ico).clone()).html();
        winTitleElement.html(ico + ' ' + title);
    }

    windowLayout.OpenWindow = function (sender, link, title, ownerWindow) {
        var isModal = $(sender).attr('winNoModal') == undefined;
        if (activeWinsQueue.length > 0) {
            var doBackHide = $(sender).attr('winDisableBackHide') == undefined;
            if (doBackHide) {
                var oldWindow = activeWinsQueue[activeWinsQueue.length - 1].win.data('tWindow');
                inAutoHide = true;
                oldWindow.modal = false; //because of telerik bug!
                oldWindow.close();
                inAutoHide = false;
            }
        }

        var winWidth = $(sender).attr('winwidth');
        var winHeight = $(sender).attr('winheight');
        var maximaizable = $(sender).attr('winNoMaximaizable') == undefined;
        var winMax = $(sender).attr('winMax');
        var doAjax = $(sender).attr('ajax');
        var actions = new Array('Refresh');
        if (maximaizable) actions.push('Maximize');
        actions.push('Close');

        w = $.telerik.window.create({
            title: fetchIcon(sender) + " در حال دریافت...",
            visible: false,
            resizable: true,
            draggable: true,
            width: winWidth == undefined ? 800 : winWidth,
            height: winHeight == undefined ? 300 : winHeight,
            actions: actions,
            refresh: function () {
                var curWin = activeWinsQueue[activeWinsQueue.length - 1];
                var confirm = true;
                if ($(curWin.sender).attr('windowActionsNeedConfirm') == 'true')
                    confirm = window.confirm("آیا نسبت به بازخوانی مجدد اطلاعات این پنجره مطمئن هستید؟");

                if (confirm) {
                    win = curWin.win;
                    var winTitle = $('span.t-window-title', win);
                    changeWindowTitle(winTitle, 'در حال دریافت...');
                    var ajaxContent = $("#addedAjaxWindowContentContainer", win);
                    if (ajaxContent.length > 0)//ajax refresh
                        loadThroughAjax(win.data('tWindow'), winTitle, null, $(ajaxContent).attr('link'));
                    else//iframe refresh
                    {
                        var ifrm = $("iframe", win)[0];
                        //$(ifrm).css('visibility', 'hidden');
                        $('div', $(ifrm).parent()).show();
                        ifrm.contentWindow.onbeforeunload = null; //to avoid getting any confirmation if provided
                        ifrm.contentWindow.location.reload(true);
                        $(ifrm).attr('isrefresh', 'true');
                    }
                }
            },
            onClose: function (e) {
                if (!inAutoHide) {
                    curWin = activeWinsQueue[activeWinsQueue.length - 1]; //instead of top() function
                    var confirm = true;
                    if (!inAutoClose && $(curWin.sender).attr('windowActionsNeedConfirm') == 'true')
                        confirm = window.confirm("آیا نسبت به بستن این پنجره مطمئن هستید؟");

                    if (confirm) {
                        activeWinsQueue.pop();
                        if (activeWinsQueue.length > 0 && $(curWin.sender).attr('winDisableBackHide') == undefined) {
                            var w = activeWinsQueue[activeWinsQueue.length - 1];
                            var lw = w.win.data('tWindow');
                            lw.modal = w.isModal;
                            lw.open();
                        }
                        curWin.win.data('tWindow').destroy();
                        e.preventDefault();
                    }
                    else
                        return false;
                }
            }
        });

        activeWinsQueue.push({ win: w, sender: sender, ownerWindow: ownerWindow, isModal: isModal });
        windowElement = w.data('tWindow');
        windowElement.modal = isModal;
        var windowTitle = $('span.t-window-title', w);

        link = correctLink(link, doAjax != undefined, true, true);

        //ajax or iframe?
        if (doAjax != undefined) {//ajax request : pure mode
            loadThroughAjax(windowElement, windowTitle, title, link, function () {
                var contentContainer = $('#addedAjaxWindowContentContainer', w);
                correctWindowSize(windowElement, windowTitle, winMax, winWidth, winHeight, true, contentContainer.outerHeight(), contentContainer.outerWidth());
            });

        }
        else {//iframe request : simple mode
            windowElement.content(progressHtml + '<iframe frameborder="0" style="width:100%;height:99%;visibility:visible;" src="' + link + '"></iframe>');
        }

        $("iframe", w).load(function () {
            $('div', $(this).parent()).hide();
            //$(this).css('visibility', 'visible'); //1:jquery hide/show methods makes some problem with inner content,2:making invisible makes problem in first field focusing
            changeWindowTitle(windowTitle, title == '' ? (this.contentWindow.document != undefined ? this.contentWindow.document.title : '') : title);

            var correctingSizeCondition = $(this).attr('isrefresh') != 'true' && this.contentWindow.location.toString().indexOf("/SavedSuccessfully") == -1 && this.contentWindow.location.hash.toString().indexOf('successfullysaved') == -1;
            correctWindowSize(windowElement, windowTitle, winMax, winWidth, winHeight, correctingSizeCondition, $(this.contentWindow.document.body).outerHeight(), $(this.contentWindow.document.body).outerWidth());

            handleSpecialPagesByLink(this.contentWindow.location);

        });


        windowElement.center().open();

    }

    function handleSpecialPagesByLink(location, linkHash) {
        var handled = false;
        var link = location;
        if (linkHash == null) linkHash = ''; //todo : linkhash is null in ajax mode.

        if (location.hash != undefined) {//means window.loaction is passed
            link = location.toString();
            linkHash = location.hash.toString();
        }

        if (link.indexOf("/SavedSuccessfully") > 0 || linkHash.indexOf('successfullysaved') > 0) {
            windowLayout.CloseAndDone(location.hash != undefined ? location : null); //when ajax request
            handled = true;
        }
        else if (link.indexOf("/accessdenied") > 0) {
            windowLayout.CloaseTopMost();
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

    function loadThroughAjax(windowElement, windowTitle, title, link, postSuccessAction) {
        windowElement.content(progressHtml);
        $.ajax({
            url: link,
            cache: false,
            success: function (html) {
                insertAjaxContent(windowElement, windowTitle, title, link, html);
                if (postSuccessAction) postSuccessAction();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                windowElement.content(xhr.responseText);
            }
        });
    }

    function insertAjaxContent(windowElement, windowTitle, title, link, html) {
        windowElement.content('<div id="addedAjaxWindowContentContainer" link="' + link + '">' + html + '</div>');
        var urlData = $('#currentPageUrl', windowElement.element); //when redirecting in ajax request
        if (urlData.length > 0) {
            link = urlData.text();
            $('#addedAjaxWindowContentContainer', windowElement.element).attr('link', link);
        }

        //handle page title
        var pageContentTitle = $('#currentPageTitle', windowElement.element);
        if (windowTitle != null)
            if (pageContentTitle.length > 0)
                changeWindowTitle(windowTitle, pageContentTitle.text());
            else
                changeWindowTitle(windowTitle, '');

        //handle validation+content
        if (!handleSpecialPagesByLink(link)) {//the page is not closed
            if (windowTitle != null && title != null && title != '') changeWindowTitle(windowTitle, title);
            enableValidation(windowElement.element);
            ajaxifyInnerForms(windowElement, windowTitle);
        }

    }

    function correctWindowSize(windowElement, windowTitle, winMax, winWidth, winHeight, correctionCondition, contentHieght, contentWidth) {
        if (winMax != undefined) windowElement.maximize();
        //correct window size
        if (winMax == undefined && correctionCondition == true) {
            var contentWindow = $(windowElement.element).find(".t-window-content");
            if (winHeight == undefined) {
                var maxHeight = $(window).height() - 100;
                var newHeight = contentHieght + windowTitle.outerHeight();
                if (newHeight > maxHeight) newHeight = maxHeight;
                contentWindow.height(newHeight);
            }
            if (winWidth == undefined) {
                var maxWidth = $(window).width() - 100;
                var newWidth = contentWidth; //indeed it get its value from the window
                if (newWidth > maxWidth) newWidth = maxWidth;
                contentWindow.width(newWidth);
            }
            if (winWidth == undefined || winHeight == undefined)
                windowElement.center();
        }
    }

    function enableValidation(element) {
        //unobtrusive validation
        if ($.validator.unobtrusive != undefined && $.validator.unobtrusive != null) {
            $(element).find('form').each(function (i, o) {
                $.validator.unobtrusive.parse(o);
            });
        }
        //non-unobtrusive validation
        if (Sys.Mvc.FormContext != undefined && Sys.Mvc.FormContext != null) {
            Sys.Mvc.FormContext._Application_Load();
        }
    }

    function ajaxifyInnerForms(windowElement, windowTitle) {
        //NOTE: just disable unobtrosive forms! TODO : handle old fasion ajax forms too
        $(windowElement.element).find('form:not([data-ajax*="true"])').each(function (i, o) {
            handleInnerFormSubmitButtons(o);
            $(o).submit(function () {
                if (!$(this).valid()) return false;
                changeWindowTitle(windowTitle, 'در حال دریافت...');
                var link = correctLink(this.action, true, true, true);
                $.ajax({
                    type: this.method.toLowerCase() == 'get' ? 'GET' : 'POST',
                    url: link,
                    cache: false,
                    data: appendSubmitButtonValue($(this).serialize(), this),
                    beforeSend: function () {
                        windowElement.content(progressHtml);
                    },
                    success: function (html, status, xhr) {
                        insertAjaxContent(windowElement, windowTitle, null, link, html);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        insertAjaxContent(windowElement, windowTitle, null, link, xhr.responseText);
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

    windowLayout.CloseAndDone = function (data) {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        inAutoClose = true;
        lw.win.data('tWindow').close();
        inAutoClose = false;
        var callBack = $(lw.sender).attr('windowcallback');
        if (typeof (callBack) != 'undefined')
            lw.ownerWindow[callBack].apply(this, new Array(lw.sender, data));
        else if (typeof (lw.ownerWindow.WindowLayout_DoneSuccessfullyCallBack) != 'undefined')
            lw.ownerWindow.WindowLayout_DoneSuccessfullyCallBack(lw.sender, data);
    }

    windowLayout.CloaseTopMost = function () {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        inAutoClose = true;
        lw.win.data('tWindow').close();
        inAutoClose = false;
    }

    windowLayout.HandleChildPageLinks = function (ownerWindow, isWindowLayout, isTabularLayout) {
        var pattern = 'a[justwindow]';
        if (isWindowLayout) pattern = pattern + ',a[newwindow]';
        $(pattern, ownerWindow.document).live('click', function () {
            if (parent.$.windowLayout != null) {
                parent.$.windowLayout.OpenWindow(this, this.href, this.title, ownerWindow);
            }
            else
                $.windowLayout.OpenWindow(this, this.href, this.title, window);
            return false;
        });

        $('a[justMain]', ownerWindow.document).live('click', function () {
            if (ownerWindow.location.toString().indexOf('iswindow=1') != -1)
                if (parent.$.windowLayout != null) parent.$.windowLayout.CloaseTopMost();

            if (isTabularLayout)
                parent.$.tabStripMain.AddTab(this);
            else
                parent.location = this.href;

            return false;
        });

        $('a[responsiveAjax]', ownerWindow.document).live('click', function () {
            handleResponsiveAjaxLink(this);
            return false;
        });

    }


    var errorWindow = null;
    windowLayout.OpenErrorWindow = function (content) {
        alert(content);
        if (errorWindow == null)
            errorWindow = $.telerik.window.create({
                title: "خطاهای ثبت فرم",
                visible: false,
                resizable: true,
                draggable: true,
                html: content,
                actions: new Array('Close')
            });

        errorWindow.data('tWindow').center().open();
    }


    function handleResponsiveAjaxLink(link) {
        $.ajax({
            type: 'POST',
            url: link.href,
            cache: false,
            success: function (json) {
                if (json.ResponseType == 0)//information
                    layoutHelper.windowLayout().ShowInformationMessage(json.Message, '');
                else if (json.ResponseType == 1)//success
                    layoutHelper.windowLayout().ShowSuccessMessage(json.Message, '');
                else if (json.ResponseType == 2)//warning
                    layoutHelper.windowLayout().ShowWarningMessage(json.Message, 'هشدار');
                else if (json.ResponseType == 3)//failed
                    layoutHelper.windowLayout().ShowErrorMessage(json.Message, 'بروز خطا');
            },
            error: function (xhr, ajaxOptions, thrownError) {
                layoutHelper.windowLayout().ShowErrorMessage('<div style="overflow:auto;direction:ltr;max-width:400px;max-height:300px">' + xhr.responseText + '</div>', 'بروز خطا');
            }
        });
    }

    $.extend({ windowLayout: windowLayout });
})(jQuery);

