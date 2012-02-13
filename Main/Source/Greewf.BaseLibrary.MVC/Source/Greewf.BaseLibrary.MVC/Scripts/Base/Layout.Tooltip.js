(function ($) {

    tooltipLayout = {};
    var activeWinsQueue = new Array(); // {sender: xxx, win:xxx,ownerWindow:xxx}
    var messageWins = new Array(); //{win:xxx , type:xxx}
    var inAutoHide;
    var inAutoClose = false;
    var debugMode = true;

    tooltipLayout.debug = function (value) {
        if (value != null) debugMode = value;
        return debugMode;
    };

    tooltipLayout.changeTitle = function (winTitleElement, title) {
        var ico = winTitleElement.children()[0];
        ico = ico == undefined ? '' : $("<span>").append($(ico).clone()).html();
        winTitleElement.html(ico + ' ' + title);
    }

    tooltipLayout.makeReadyToShow = function (sender, link, title, ownerWindow) {
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
                var confirm = $.layoutCore.confirmRefresh(curWin.sender);

                if (confirm) {
                    win = curWin.win;
                    var winTitle = $('span.t-window-title', win);
                    $.layoutCore.refreshContent($.tooltipLayout, winTitle, win);
                }
            },
            onClose: function (e) {
                if (!inAutoHide) {
                    curWin = activeWinsQueue[activeWinsQueue.length - 1]; //instead of top() function
                    var confirm = $.layoutCore.confirmRefresh(curWin.sender, inAutoClose);

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
        w.data('tWindow').modal = isModal;
        var windowTitle = $('span.t-window-title', w);

        return { widget: w, widgetTitle: windowTitle };

    }

    tooltipLayout.show = function (window) {
        window.data('tWindow').center().open();
    }

    tooltipLayout.setContent = function (window, content) {
        window.data('tWindow').content(content);
    }

    tooltipLayout.CloseAndDone = function (data) {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        inAutoClose = true;
        lw.win.data('tWindow').close();
        inAutoClose = false;
        var callBack = $(lw.sender).attr('windowcallback');
        if (typeof (callBack) != 'undefined')
            lw.ownerWindow[callBack].apply(this, new Array(lw.sender, data));
        else if (typeof (lw.ownerWindow.tooltipLayout_DoneSuccessfullyCallBack) != 'undefined')
            lw.ownerWindow.tooltipLayout_DoneSuccessfullyCallBack(lw.sender, data);
    }

    tooltipLayout.CloseTopMost = function () {
        if (activeWinsQueue.length == 0) return;
        var lw = activeWinsQueue[activeWinsQueue.length - 1];
        inAutoClose = true;
        lw.win.data('tWindow').close();
        inAutoClose = false;
    }

    tooltipLayout.maximize = function (win) {
        win.data('tWindow').maximize();
    }

    tooltipLayout.center = function (win) {
        win.data('tWindow').center();
    }

    tooltipLayout.setHeight = function (win, height) {
        $(win.data('tWindow').element).find(".t-window-content").height(height);
    }

    tooltipLayout.setWidth = function (win, width) {
        $(win.data('tWindow').element).find(".t-window-content").width(width);
    }

    tooltipLayout.getTitleHeight = function (winTitle) {
        return winTitle.outerHeight();
    }

    var errorWindow = null;
    tooltipLayout.OpenErrorWindow = function (content) {
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

    $.extend({ tooltipLayout: tooltipLayout });
})(jQuery);

