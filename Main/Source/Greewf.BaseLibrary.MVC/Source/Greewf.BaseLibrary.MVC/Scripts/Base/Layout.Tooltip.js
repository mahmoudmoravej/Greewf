(function ($) {

    tooltipLayout = {};
    var activeWinsQueue = new Array(); // {sender: xxx, win:xxx,ownerWindow:xxx}
    var messageWins = new Array(); //{win:xxx , type:xxx}
    var inAutoHide;
    var inAutoClose = false;

    tooltipLayout.progressHtml = function () {
        return '<div isProgress="1" class="bigprogress-icon t-content" style="min-width:48px;min-height:48px;" ></div>';
    }


    tooltipLayout.changeTitle = function (winTitleElement, title) {
        //var ico = winTitleElement.children()[0];
        //ico = ico == undefined ? '' : $("<span>").append($(ico).clone()).html();
        //winTitleElement.html(ico + ' ' + title);
    }

    tooltipLayout.makeReadyToShow = function (sender, link, title, ownerWindow) {

        var winWidth = $(sender).attr('winwidth');
        var winHeight = $(sender).attr('winheight');

        winWidth = winWidth > 0 ? winWidth : 'auto';
        winHeight = winHeight > 0 ? winHeight : 'auto';

        sender = $(sender);

        if (sender.qtip('api') == null)
            sender.qtip({
                show: {
                    event: 'click',
                    solo: true,
                    effect: function (offset) {
                        $(this).slideDown(500);
                    }
                },
                hide: {
                    event: 'unfocus',
                    effect: function (offset) {
                        $(this).slideUp(200);
                    }
                },
                style: {
                    classes: 'ui-tooltip-wiki ui-tooltip-light ui-tooltip-shadow',
                    height: winHeight,
                    width: winWidth
                },
                position: {
                    my: 'top right',
                    at: 'bottom right',
                    viewport: $(window), // Keep it on-screen at all times if possible
                    adjust: { x: -10 }
                }
            });

        var api = sender.qtip('api');
        api.render();
        if (winHeight > 0) $(api.elements.content).height(winHeight); //becuase of bug in qtip we should handle it manually

        return { widget: { api: api, sender: sender, htmlTag: api.elements.content, ownerWindow: ownerWindow }, widgetTitle: sender.qtip('api') };


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

    tooltipLayout.show = function (tooltip) {
        //tooltip.api.reposition();
        tooltip.api.show();

    }

    tooltipLayout.setContent = function (tooltip, content) {
        //tooltip.hide();
        tooltip.api.set('content.text', $(content));
        //if (tooltip.api.elements.content != null)
        //    tooltip.api.elements.content.html(content);
        //if (tooltip.source.css('visibility') != 'visible') 
        tooltip.api.show();
    }

    tooltipLayout.CloseAndDone = function (data, tooltip) {
        if (tooltip == null) return;
        tooltip.api.hide();
        var callBack = $(tooltip.sender).attr('windowcallback');
        if (typeof (callBack) != 'undefined')
            tooltip.ownerWindow[callBack].apply(this, new Array(tooltip.sender, data));
        else if (typeof (tooltip.ownerWindow.tooltipLayout_DoneSuccessfullyCallBack) != 'undefined')
            tooltip.ownerWindow.tooltipLayout_DoneSuccessfullyCallBack(tooltip.sender, data);
    }

    tooltipLayout.CloseTopMost = function (tooltip/*can be null*/) {
        if (tooltip != null) tooltip.api.hide();
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

