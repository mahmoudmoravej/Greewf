(function ($) {

    if ($.windowLayout != undefined) return; //I don't know why this object call more than once. this insure us to hace only one instance per page
    windowLayout = {};
    var activeWinsQueue = new Array(); // activeWinsQueue.push({ win: w, sender: sender, ownerWindow: ownerWindow, isModal: isModal, widget: widget });  +   widget = { core: w, htmlTag: w, ownerWindow: ownerWindow, sender: sender };
    var messageWins = new Array(); //{win:xxx , type:xxx}
    var inAutoHide;
    var inAutoClose = false;
    var debugMode = true;

    windowLayout.getTypeCode = function () {
        return 1; //by convention
    }

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

    windowLayout.ShowQuestionMessage = function (msg, title, options) {
        return showMessage(msg, title, 5, null, options);
    }

    windowLayout.ShowProgressMessage = function (msg, title) {
        if (!title) title = 'در حال پردازش';
        if (!msg) msg = 'لطفا چند لحظه صبر نمایید...';
        return showMessage(msg, title, 6);
    }

    windowLayout.HideProgressMessage = function () {
        closeOldWin(6);
    }


    windowLayout.progressHtml = function () {
        return '<div isProgress="1" class="bigprogress-icon t-content" style="width:99%;height:97%;position:absolute;" ></div>';
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
            case 6: //progress
                return 'bigprogress-icon';
            default:
                return 'info48-png';

        }
    }

    var messageTemplateBase = "<table style='width:300px;'><tr><td><img class='###1' src='data:image/gif;base64,R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==' /></td><td style='vertical-align:middle;width:100%;white-space: nowrap;'>###2</td></tr></table>";
    var commonMessageTemplate = messageTemplateBase + "<div class='g-buttons-content'><button class='t-button editor-focus2' style='float:left'><span class='icon16 stop-png'></span>&nbsp;بستن</button></div>";
    var questionTemplate = messageTemplateBase + "<div class='g-buttons-content'><button class='t-button g-no ###3' style='float:left;margin-right:5px;'><span class='icon16 stop-png'></span>&nbsp;###5</button><button class='t-button ###4 g-yes' style='float:left;'><span class='icon16 apply-png'></span>&nbsp;###6</button></div>";

    showMessage = function (msg, title, type, ignoreTemplate, options) {

        var options = $.extend({ focusToYes: false, yesText: 'بلی', noText: 'خیر', callBack: null }, options, { callBackHandled: null/*internal use*/ });
        var msgHtml = "";
        if (ignoreTemplate)
            msgHtml = msg;
        else if (type == 5)//question
        {
            msgHtml = questionTemplate
                .replace('###2', msg)
                .replace('###1', getMessageIcon(type))
                .replace('###3', options.focusToYes ? '' : 'editor-focus2')
                .replace('###4', options.focusToYes ? 'editor-focus2' : '')
                .replace('###5', options.noText)
                .replace('###6', options.yesText);
        }
        else //other types
            msgHtml = commonMessageTemplate.replace('###2', msg).replace('###1', getMessageIcon(type));

        var oldWin = closeOldWin(type);
        if (type != 6)//if not progress, hide all old progress
            closeOldWin(6);

        msgBoxWindow = $.telerik.window.create({
            title: title,
            visible: false,
            resizable: false,
            draggable: false,
            actions: type == 6 ? new Array() : new Array('Close'),
            html: msgHtml,
            onClose: function (s) {
                window.clearInterval($(this).data('alwaysOnTop'));
                $(s.target).css('z-index', '').css('visibility', 'hidden');
                var x = $('.editor-focus2', this); //it works with helper.js to return the focus to the previous item
                if (x.data('closeHandler')) x.data('closeHandler')();
                if (s.currentTarget == msgBoxWindow[0]/*to ensure it is for current window call*/ && options.callBack && !options.callBackHandled) options.callBack(false);
                x.attr('class', '');
                $(s).remove();
            }
        });

        if (oldWin == null)
            messageWins.push({ type: type, win: msgBoxWindow });
        else
            oldWin.win = msgBoxWindow;

        $('iframe', msgBoxWindow).remove();
        msgBoxWindow.css('z-index', '50000'); //not a best solution. 
        //$('.t-content', msgBoxWindow).css('background-color', '#99FF99');

        if (!ignoreTemplate) {
            $('button.t-button', msgBoxWindow).click(function () {
                var action = $(this).hasClass('g-yes'); //we should get it at first
                options.callBackHandled = true;
                $(msgBoxWindow).data('tWindow').close();
                if (options.callBack) options.callBack(action);
            });
        }

        $(msgBoxWindow).keyup(function (e) { if (e.keyCode == 27) $(this).data('tWindow').close(); });

        var w = msgBoxWindow.data('tWindow');

        if (type == 6) {//progress
            $('button', msgBoxWindow).remove();
            w.modal = true;
        }
        else
            createButtonsBar(msgBoxWindow);


        w.center().open();
        msgBoxWindow.data('alwaysOnTop', window.setInterval(function () { w.bringToTop(); }, 100));
        $('button.t-button.editor-focus2', msgBoxWindow).focus();

    }

    function closeOldWin(type) {
        var oldWin = null;
        $(messageWins).each(function (i, o) { if (o.type == type) oldWin = o; });

        if (oldWin != null && oldWin.win != null) {
            oldWin.win.data('tWindow').close();
            oldWin.win.data('tWindow').destroy();
            oldWin.win = null;
        }

        return oldWin;

    }

    windowLayout.closeAllMessageWindows = function () {//Note: close all except progress windows
        $(messageWins).each(function (i, oldWin) {
            if (oldWin.type != 6) {
                if (oldWin != null && oldWin.win != null) {
                    oldWin.win.data('tWindow').close();
                    oldWin.win.data('tWindow').destroy();
                    oldWin.win = null;
                }
            }
        });
    }

    function createButtonsBar(windowElement, assumeLastParagraphAsButtonBar/*and only if it has buttons*/) {/*NOTE : NOT Works for IFRAME mode!*/
        windowElement = $(windowElement);
        if (windowElement.length == 0) return;
        contentElement = $('div.t-content', windowElement);

        removeButtonBar(windowElement);
        var mainContainer = $('.g-buttons-content', contentElement);

        if (mainContainer.length == 0) {
            if (!assumeLastParagraphAsButtonBar) return;
            mainContainer = $('p:last', contentElement);
            if (mainContainer.length == 0 || mainContainer.find('button,a,input').length == 0) return;
            mainContainer.addClass('g-buttons-content');
        }
        if (mainContainer.hasClass('g-window-nobuttonbar')) return; //cancel it if the related tag requested
        $(mainContainer).closest('.t-window-content').css({ position: 'static' }); //becuase absolute objectes, positioned into their nearest non-static parent(http://www.w3schools.com/cssref/pr_class_position.asp)
        mainContainer.addClass('t-grid-pager g-clearfix g-window-buttonbar');
        var clonedContainer = mainContainer.clone(false, false).css({ display: 'block', visibility: 'hidden' }).appendTo(windowElement);
        $('*', clonedContainer).removeAttr('id').removeAttr('name'); //to avoid id confilicting in scripts
        mainContainer.css({ position: 'absolute', left: 0, right: 0, bottom: 0 });

    }

    function removeButtonBar(windowElement) {
        $('.g-window-buttonbar', windowElement).remove();
    }

    function clearButtonBar(windowElement) {
        $('.g-window-buttonbar', windowElement).css('visibility', 'hidden'); //to preserver its height and prevent flashingn resizing (in refreshing content)
    }

    function fetchIcon(sender) {
        var ico = $('span[class*="icon16"]:first', sender);
        if (ico.length == 0)
            ico = $('span[class*="t-sprite"]:first', sender);
        if (ico.length == 0)
            ico = $('span[class*="t-icon"]:first', sender);
        if (ico.length == 0)
            ico = $('img:first', sender);
        if (ico.length > 0) {
            return $("<span>").append($(ico[0]).clone().css('vertical-align', 'middle').css('max-height', '16px')).html();
        }
        return '';
    }

    windowLayout.changeTitle = function (winTitleElement, title) {
        var ico = winTitleElement.children()[0];
        ico = ico == undefined ? '' : $("<span>").append($(ico).clone()).html();
        winTitleElement.html(ico + ' ' + title);
    }

    function doBackHide(sender) {
        var doBackHide = $.layoutCore.options.window.autoBackHide;

        var x = $(sender).attr('winDisableBackHide');
        if (x) doBackHide = false;

        x = $(sender).attr('winEnableBackHide');
        if (x) doBackHide = true;

        return doBackHide;
    }

    windowLayout.makeReadyToShow = function (sender, link, title, ownerWindow) {
        var isModal = $(sender).attr('winNoModal') == undefined;
        if (activeWinsQueue.length > 0) {
            if (doBackHide(sender)) {
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
                var confirm = $.layoutCore.confirmRefresh(curWin.sender);

                if (confirm) {
                    win = curWin.win;
                    var winTitle = $('span.t-window-title', win);
                    $.layoutCore.refreshContent($.windowLayout, curWin.widget, winTitle);
                }
            },
            onActivate: function () {
                var curWin = activeWinsQueue[activeWinsQueue.length - 1];
                var winTitle = $('span.t-window-title', curWin.win);
                $.layoutCore.widgetActivated($.windowLayout, curWin.widget, winTitle);
            },
            onClose: function (e) {
                if (!inAutoHide) {
                    curWin = activeWinsQueue[activeWinsQueue.length - 1]; //instead of top() function
                    var confirm = $.layoutCore.confirmClose(curWin.sender, inAutoClose);

                    if (confirm) {
                        activeWinsQueue.pop();
                        if (activeWinsQueue.length > 0 && doBackHide(curWin.sender)) {
                            var w = activeWinsQueue[activeWinsQueue.length - 1];
                            var lw = w.win.data('tWindow');
                            lw.modal = w.isModal;
                            lw.open();
                        }
                        $.layoutCore.onClose(curWin.sender, curWin.ownerWindow, inAutoClose, widget);
                        curWin.win.data('tWindow').destroy();
                        e.preventDefault();
                    }
                    else
                        return false;
                }
            }
        });

        var widget = { core: w, htmlTag: w, ownerWindow: ownerWindow, sender: sender };
        activeWinsQueue.push({ win: w, sender: sender, ownerWindow: ownerWindow, isModal: isModal, widget: widget });
        w.data('tWindow').modal = isModal;
        var windowTitle = $('span.t-window-title', w);

        return { widget: widget, widgetTitle: windowTitle };

    }

    windowLayout.show = function (window) {
        windowLayout.closeAllMessageWindows();
        window.core.data('tWindow').center().open();
    }

    windowLayout.contentLoaded = function (window) {
        var auto = $(window.sender).attr('winAutoButtonsBar');
        if (!auto) auto = $.layoutCore.options.window.autoButtonBar;
        createButtonsBar(window.core, auto);
    }

    windowLayout.setContent = function (window, content, hideOldContent) {
        if (!hideOldContent) {
            clearButtonBar(window.core);
            window.core.data('tWindow').content(content);
        }
        else {//hide old content (to enable retriving it in error conditions)
            var winElement = window.core.data('tWindow').element;
            $('.t-window-content>*', winElement).css('display', 'none');
            $('.g-window-buttonbar', winElement).css('visibility', 'hidden');
            var newContentPointer = { oldTitle: $('.t-window-title', winElement).html(), newElement: $(content).prependTo($('.t-window-content', winElement)) };
            return newContentPointer;
        }
    }

    windowLayout.retrieveOldContent = function (window, newContentPointer) {
        var winElement = window.core.data('tWindow').element;
        if (newContentPointer) newContentPointer.newElement.remove();
        $('.t-window-content>*', winElement).css('display', '');
        $('.g-window-buttonbar', winElement).css('visibility', 'visible');
        $('.t-window-title', winElement).html(newContentPointer.oldTitle);
    }

    windowLayout.CloseAndDone = function (data, widget, isSuccessfulFlagUp, isClosedManually) {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        inAutoClose = true;
        lw.win.data('tWindow').close();
        inAutoClose = false;
        $.layoutCore.handleCloseCallBack(lw.sender, data, lw.ownerWindow, isSuccessfulFlagUp, isClosedManually, widget);

        //        var callBack = $(lw.sender).attr('windowcallback');
        //        if (typeof (callBack) != 'undefined')
        //            lw.ownerWindow[callBack].apply(this, new Array(lw.sender, data));
        //        else if (typeof (lw.ownerWindow.WindowLayout_DoneSuccessfullyCallBack) != 'undefined')
        //            lw.ownerWindow.WindowLayout_DoneSuccessfullyCallBack(lw.sender, data);
    }

    windowLayout.CloseTopMost = function (widget/*can be null*/) {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        inAutoClose = true;
        lw.win.data('tWindow').close();
        inAutoClose = false;
    }

    windowLayout.MaximizeToContent = function (adjustCenter) {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        var winTitle = $('span.t-window-title', lw.win);
        $.layoutCore.maximizeToContent($.windowLayout, lw.widget, winTitle, adjustCenter);
    }


    windowLayout.maximize = function (win) {
        win.core.data('tWindow').maximize();
    }

    windowLayout.isMaximized = function (win) {
        return $('.t-window-actions .t-restore', win.htmlTag).length > 0;
    }

    windowLayout.center = function (win) {
        win.core.data('tWindow').center();
    }

    windowLayout.setHeight = function (win, height, justGrow) {
        var cnt = $(win.core.data('tWindow').element).find(".t-window-content");
        if (justGrow == false || height - cnt.height() >= 1) {
            win.core.data('tWindow').height = height;
            cnt.height(height);
            return true;
        }
        return false;
    }

    windowLayout.setWidth = function (win, width, justGrow) {
        var cnt = $(win.core.data('tWindow').element).find(".t-window-content");
        if (justGrow == false || (width - cnt.width() >= 1)) {
            win.core.data('tWindow').width = width;
            cnt.width(width);
            return true;
        }
        return false;
    }

    windowLayout.getTitleHeight = function (winTitle) {
        return winTitle.outerHeight();
    }

    windowLayout.getFooterHeight = function (win) {
        var x = $('.g-window-buttonbar', win.core);
        if (x.length > 0)
            return x.outerHeight();
        return 0;
    }

    windowLayout.IsFooterHeightCalculatedInWidgetHeight = function () {
        return true;//because we just reposition the footer position.it is inside the content body indeed
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

    $.extend({ windowLayout: windowLayout });
})(jQuery);

