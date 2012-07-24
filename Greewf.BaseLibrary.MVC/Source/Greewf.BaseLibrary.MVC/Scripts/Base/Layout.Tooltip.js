(function ($) {

    if ($.tooltipLayout != undefined) return; //I don't know why this object call more than once. this insure us to hace only one instance per page
    tooltipLayout = {};
    var lastQtipSender = null;
    var lastQtipContainer = null;
    var lastTooltip = null;

    tooltipLayout.getTypeCode = function () {
        return 2; //by convention
    }

    tooltipLayout.progressHtml = function () {
        return '<div isProgress="1" class="bigprogress-icon t-content" style="min-width:48px;min-height:48px;" ></div>';
    }


    tooltipLayout.changeTitle = function (api, title) {
        api.set({ 'content.title.text': title });
    }

    tooltipLayout.closeLastTip = function () {
        $(lastQtipSender, lastQtipContainer).qtip('hide');
    }

    tooltipLayout.makeReadyToShow = function (sender, link, title, ownerWindow) {

        var winWidth = $(sender).attr('winwidth');
        var winHeight = $(sender).attr('winheight');
        var showEvents = $(sender).attr('showEvents');
        var hideEvents = $(sender).attr('hideEvents');
        showEvents = showEvents == null ? 'click' : showEvents;
        hideEvents = hideEvents == null ? 'unfocus' : hideEvents;

        winWidth = winWidth > 0 ? winWidth : 'auto';
        winHeight = winHeight > 0 ? winHeight : 'auto';

        this.closeLastTip();
        sender = $(sender);
        lastQtipSender = sender;
        lastQtipContainer = sender.closest('body');

        if (sender.qtip('api') == null)
            sender.qtip({
                content: { text: ' ', title: { text: ' ', button: 'close'} }, //sender with empty title (at least) cause error in creation!
                show: {
                    event: showEvents,
                    solo: true,
                    effect: function (offset) {
                        $(this).slideDown(500);
                    }
                },
                hide: {
                    fixed: true,
                    event: hideEvents,
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
                    viewport: $(ownerWindow), //$(window), // Keep it on-screen at all times if possible
                    container: lastQtipContainer,
                    target: sender,
                    adjust: { x: -10, y: -5 }
                },
                events: {
                    hide: function (event, api) {
                        api.destroy();
                    }
                }
            });

        var api = sender.qtip('api');
        api.render();
        if (winHeight > 0) $(api.elements.content).height(winHeight); //becuase of bug in qtip we should handle it manually
        $(api.elements.button).attr('title', 'بستن');

        lastTooltip = { widget: { api: api, sender: sender, htmlTag: api.elements.content, ownerWindow: ownerWindow }, widgetTitle: sender.qtip('api') };
        return lastTooltip;

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

    tooltipLayout.CloseAndDone = function (data, tooltip, isSuccessfulFlagUp) {
        if (!tooltip) tooltip = lastTooltip.widget;
        if (!tooltip) return;

        tooltip.api.hide();
        $.layoutCore.handleCloseCallBack(tooltip.sender, data, tooltip.ownerWindow, isSuccessfulFlagUp);
    }

    tooltipLayout.CloseTopMost = function (tooltip/*can be null*/) {
        if (tooltip != null) tooltip.api.hide();
    }

    tooltipLayout.MaximizeToContent = function (adjustCenter) {
        if (lastTooltip == null) return;
        $.layoutCore.maximizeToContent($.tooltipLayout, lastTooltip.widget, lastTooltip.widgetTitle, adjustCenter);
    }

    tooltipLayout.maximize = function (win) {
        // win.data('tWindow').maximize();
    }

    tooltipLayout.isMaximized = function (win) {
        return false; //$('.t-window-actions .t-restore', win.htmlTag).length > 0;
    }


    tooltipLayout.center = function (win) {
        //  win.data('tWindow').center();
    }

    tooltipLayout.setHeight = function (win, height, justGrow) {
        //  $(win.data('tWindow').element).find(".t-window-content").height(height);
    }

    tooltipLayout.setWidth = function (win, width, justGrow) {
        //  $(win.data('tWindow').element).find(".t-window-content").width(width);
    }

    tooltipLayout.getTitleHeight = function (winTitle) {
        // return winTitle.outerHeight();
        return 0;
    }

    $.extend({ tooltipLayout: tooltipLayout });
})(jQuery);

