(function ($) {

    if ($.tooltipLayout != undefined) return; //I don't know why this object call more than once. this insure us to hace only one instance per page
    tooltipLayout = {};
    var lastQtipSender = null;
    var lastQtipContainer = null;
    var lastTooltip = null;
    var lastTooltipHeight = null;
    var lastTooltipWidth = null;
    var insideRepositioningTimer = false;

    window.setInterval(function () {
        if (insideRepositioningTimer) return;
        insideRepositioningTimer = true;
        if (lastTooltip != null) {
            if (lastTooltipWidth != lastTooltip.widget.htmlTag.innerWidth() || lastTooltipHeight != lastTooltip.widget.htmlTag.innerHeight()) {
                lastTooltip.widget.api.reposition();
                lastTooltipWidth = lastTooltip.widget.htmlTag.innerWidth();
                lastTooltipHeight = lastTooltip.widget.htmlTag.innerHeight();
            }
        }
        insideRepositioningTimer = false;

    }, 700);

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
        var winMaxHeight = $(sender).attr('winMaxHeight');
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
                content: { text: ' ', title: { text: ' ', button: 'close' } }, //sender with empty title (at least) cause error in creation!
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
                    height: 'auto',//because of bug we set it always to auto and change the content height instead. If we don't do this, the shadow of tooltip collapses.
                    width: winWidth,
                    tip: { offset: 5 }
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
        if (winMaxHeight > 0) $(api.elements.content).css('max-height', winMaxHeight + 'px'); //becuase of bug in qtip we should handle it manually
        $(api.elements.button).attr('title', 'بستن');

        lastTooltip = { widget: { api: api, sender: sender, htmlTag: api.elements.content, ownerWindow: ownerWindow }, widgetTitle: sender.qtip('api') };
        lastTooltipWidth = api.elements.content.innerWidth();
        lastTooltipHeight = api.elements.content.innerHeight();
        return lastTooltip;

    }

    tooltipLayout.show = function (tooltip) {
        //tooltip.api.reposition();
        tooltip.api.show();

    }

    tooltipLayout.contentLoaded = function (window) {
        //createButtonsBar(window.core);
    }

    tooltipLayout.setContent = function (tooltip, content, hideOldContent) {
        if (!hideOldContent)
            tooltip.api.set('content.text', $(content));
        else {//hide old content (to enable retriving it in error conditions)
            var tooltipElement = tooltip.htmlTag;
            $('>*', tooltipElement).css('display', 'none');
            //$('.g-window-buttonbar', tooltipElement).css('visibility', 'hidden'); not button toolbar suprt
            var newContentPointer = { oldTitle: tooltip.api.get('content.title.text'), newElement: $(content).prependTo(tooltipElement) };
            return newContentPointer;
        }
        tooltip.api.show();
    }

    tooltipLayout.retrieveOldContent = function (tooltip, newContentPointer) {
        var tooltipElement = tooltip.htmlTag;
        if (newContentPointer) newContentPointer.newElement.remove();
        $('>*', tooltipElement).css('display', '');
        tooltip.api.set({ 'content.title.text': newContentPointer.oldTitle });
        //$('.g-window-buttonbar', winElement).css('visibility', 'visible'); not button toolbar suprt
    }

    tooltipLayout.CloseAndDone = function (data, tooltip, isSuccessfulFlagUp, isClosedManually) {
        if (!tooltip) tooltip = lastTooltip.widget;
        if (!tooltip) return;

        tooltip.api.hide();
        $.layoutCore.handleCloseCallBack(tooltip.sender, data, tooltip.ownerWindow, isSuccessfulFlagUp, isClosedManually, lastTooltip.widget);
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
        return false;
    }

    tooltipLayout.setWidth = function (win, width, justGrow) {
        //  $(win.data('tWindow').element).find(".t-window-content").width(width);
        return false;
    }

    tooltipLayout.getTitleHeight = function (winTitle) {
        // return winTitle.outerHeight();
        return 0;
    }

    tooltipLayout.getFooterHeight = function (win) {
        //return $('.g-window-buttonbar', windowElement).outerHeight();
        return 0;
    }


    $.extend({ tooltipLayout: tooltipLayout });
})(jQuery);

