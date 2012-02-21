(function ($) {

    tooltipLayout = {};
    var lastQtipSender = null;

    tooltipLayout.progressHtml = function () {
        return '<div isProgress="1" class="bigprogress-icon t-content" style="min-width:48px;min-height:48px;" ></div>';
    }


    tooltipLayout.changeTitle = function (winTitleElement, title) {
        //var ico = winTitleElement.children()[0];
        //ico = ico == undefined ? '' : $("<span>").append($(ico).clone()).html();
        //winTitleElement.html(ico + ' ' + title);
    }

    tooltipLayout.closeLastTip = function () {
        $(lastQtipSender).qtip('hide');
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

        sender = $(sender);
        lastQtipSender = sender;

        if (sender.qtip('api') == null)
            sender.qtip({
                content: ' ', //sender with empty title (at least) cause error in creation!
                show: {
                    event: showEvents,
                    solo: true,
                    effect: function (offset) {
                        $(this).slideDown(500);
                    }
                },
                hide: {
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
                    viewport: $(window), // Keep it on-screen at all times if possible
                    adjust: { x: -10 }
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

        return { widget: { api: api, sender: sender, htmlTag: api.elements.content, ownerWindow: ownerWindow }, widgetTitle: sender.qtip('api') };

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
        $.layoutCore.handleCloseCallBack(tooltip.sender, data, tooltip.ownerWindow);
    }

    tooltipLayout.CloseTopMost = function (tooltip/*can be null*/) {
        if (tooltip != null) tooltip.api.hide();
    }

    tooltipLayout.maximize = function (win) {
        // win.data('tWindow').maximize();
    }

    tooltipLayout.center = function (win) {
        //  win.data('tWindow').center();
    }

    tooltipLayout.setHeight = function (win, height) {
        //  $(win.data('tWindow').element).find(".t-window-content").height(height);
    }

    tooltipLayout.setWidth = function (win, width) {
        //  $(win.data('tWindow').element).find(".t-window-content").width(width);
    }

    tooltipLayout.getTitleHeight = function (winTitle) {
        // return winTitle.outerHeight();
        return 0;
    }

    $.extend({ tooltipLayout: tooltipLayout });
})(jQuery);

