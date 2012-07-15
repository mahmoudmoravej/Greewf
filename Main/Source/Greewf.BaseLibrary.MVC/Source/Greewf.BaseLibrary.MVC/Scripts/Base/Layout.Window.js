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
            default:
                return 'info48-png';

        }
    }

    showMessage = function (msg, title, type) {
        template = "<table style='width:300px;'><tr><td><img class='###' src='data:image/gif;base64,R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==' /></td><td style='vertical-align:middle;width:100%;white-space: nowrap;'>$$$</td></tr></table><button class='t-button editor-focus2' style='float:left'><span class='icon16 stop-png'></span>&nbsp;بستن</button>";
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
        $('button.t-button', msgBoxWindow).click(function () {
            $(msgBoxWindow).data('tWindow').close();
        });
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

    windowLayout.changeTitle = function (winTitleElement, title) {
        var ico = winTitleElement.children()[0];
        ico = ico == undefined ? '' : $("<span>").append($(ico).clone()).html();
        winTitleElement.html(ico + ' ' + title);
    }

    windowLayout.makeReadyToShow = function (sender, link, title, ownerWindow) {
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

        var widget = { core: w, htmlTag: w, ownerWindow: ownerWindow, sender: sender };
        activeWinsQueue.push({ win: w, sender: sender, ownerWindow: ownerWindow, isModal: isModal, widget: widget });
        w.data('tWindow').modal = isModal;
        var windowTitle = $('span.t-window-title', w);

        return { widget: widget, widgetTitle: windowTitle };

    }

    windowLayout.show = function (window) {
        window.core.data('tWindow').center().open();
    }

    windowLayout.setContent = function (window, content) {
        window.core.data('tWindow').content(content);
    }

    windowLayout.CloseAndDone = function (data, widget, isSuccessfulFlagUp) {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        inAutoClose = true;
        lw.win.data('tWindow').close();
        inAutoClose = false;
        $.layoutCore.handleCloseCallBack(lw.sender, data, lw.ownerWindow, isSuccessfulFlagUp);

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

    windowLayout.ResizeToContent = function () {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        var winTitle = $('span.t-window-title', lw.win);
        $.layoutCore.resizeToContent($.windowLayout, lw.widget, winTitle);
    }


    windowLayout.maximize = function (win) {
        win.core.data('tWindow').maximize();
    }

    windowLayout.center = function (win) {
        win.core.data('tWindow').center();
    }

    windowLayout.setHeight = function (win, height) {
        win.core.data('tWindow').height = height;
        $(win.core.data('tWindow').element).find(".t-window-content").height(height);
    }

    windowLayout.setWidth = function (win, width) {
        win.core.data('tWindow').width = width;
        $(win.core.data('tWindow').element).find(".t-window-content").width(width);
    }

    windowLayout.getTitleHeight = function (winTitle) {
        return winTitle.outerHeight();
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

