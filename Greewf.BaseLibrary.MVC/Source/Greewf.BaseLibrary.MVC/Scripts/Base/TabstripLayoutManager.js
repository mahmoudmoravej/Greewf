(function ($) {

    tabStripMain = {};
    var options = { tabstripId: 'tabstripMain', firstPageUrl: '/', firstPageTitle: '', isNewWindowOk: true };
    var arrTabs = new Array();
    var linkPattern = 'a[href][href!^="#"][href!^="#"]:not([href^="javascript:"]):not([href*="ajax=True"]):not([inline]):not([justwindow]):not([justMain]):not([responsiveAjax])'; //pattern which accept links to open
    var currentTabId = -1;

    tabStripMain.load = function (o) {
        $.extend(options, o); //merge user passed options with default
        if (options.tabstripId.charAt[0] != '#') options.tabstripId = '#' + options.tabstripId;
        if (options.isNewWindowOk) { linkPattern = linkPattern + ':not([newwindow])'; }
        var tabStrip = $(options.tabstripId);

        var $tabs = tabStrip.tabs({
            tabTemplate: "<li><a href='#{href}' class='t-link'>#{label}</a><span class='t-icon t-delete' style='cursor:pointer'>بستن</span></li>",
            select: function (event, ui) {
                $(arrTabs).each(function (i, o) {
                    if (ui.panel.id == o.id) {
                        var ifrm = $('iframe', '#' + o.id)[0];
                        if (ifrm != null) { setTimeout(function () { ifrm.focus(); ifrm.contentWindow.focus(); if (ifrm.contentWindow.setInitialFocusForCurrentPage != null) ifrm.contentWindow.setInitialFocusForCurrentPage(ifrm); }); }
                        currentTabId = o.id;
                        window.location.hash = o.link;
                        window.document.title = $('li>a[href="#' + o.id + '"]', tabStrip).text();
                    };
                })
            }

        });

        $(options.tabstripId + " span.t-icon.t-delete").live("click", function () {
            var tabItem = $("li", $tabs);
            var index = tabItem.index($(this).parent());
            $tabs.tabs("remove", index);
            $tabs.tabs("select", index == 0 ? 0 : index - 1);

        });

        //        function setFocusToActiveTab() {
        //            $(arrTabs).each(function (i, o) { 
        //            if (o.id==currentTabId)
        //                var ifrm = $('iframe', '#' + o.id)[0];
        //            });
        //        }

        //arrTabs.push({ id: 'tabs-1', link: options.firstPageUrl });

        //listen to all links click
        $(linkPattern).live('click', function () {
            $.tabStripMain.AddTab(this);
            return false;
        });

        $tabs.tabs("remove", 0);
        if (o.firstPageUrl.length > 0) tabStripMain.addByLink(o.firstPageUrl, options.firstPageTitle);

    }

    function trim(stringToTrim) {
        return stringToTrim.replace(/^\s+|\s+$/g, "");
    }

    tabStripMain.ChangeCurrentTabLocation = function (anchor) {
        addOrReplace($(anchor), true);
    }

    tabStripMain.AddTab = function (anchor) {
        if ($(anchor).attr('href')[0].indexOf('#') == 0) return; //becuase anchor.href returns full url. then the pattern works fails in some bookmarking 
        addOrReplace($(anchor), false);
    }

    function addOrReplace(anchor, replaceActiveTab) {
        link = anchor.attr('href').toLowerCase();
        title = trim(anchor.text()) == '' ? anchor.attr('title') : trim(anchor.text());

        tabStripMain.addByLink(link, title, replaceActiveTab);
        return false;

    }

    function clearLink(link) {
        link = link.replace('&istab=1', '').replace('?istab=1&', '?').replace('?istab=1', '').replace(/^\s+|\s+$/g, "").replace(/\?$/g, "").replace(/\#$/g, '');
        return link;
    }

    tabStripMain.addByLink = function (link, title, replaceActiveTab) {

        var url = link.replace(/\//g, "_");
        var tabStrip = $(options.tabstripId);

        var oldTabItem = null;
        link = clearLink(link);
        $(arrTabs).each(function (i, item) { if (item.link == link) oldTabItem = item });

        //select if opened previously
        if (oldTabItem != null)
            if ($(options.tabstripId + '>div[id="' + oldTabItem.id + '"]').length > 0) //if is alive
            {
                tabStrip.tabs('select', oldTabItem.id);
                return;
            }

        var tabStrip = $(options.tabstripId);
        var pos = tabStrip.tabs('length');
        title = 'در حال دریافت...';
        var panelId = '';
        var panel = null;

        //make the tab ready (make it or replace it)
        if (replaceActiveTab) {//replace tab
            panelId = currentTabId;
            $('li>a[href="#' + panelId + '"]', tabStrip).text(title);
            panel = $('#' + panelId, tabStrip);
            $(arrTabs).each(function (i, o) {
                if (currentTabId == o.id) { o.link = link; };
            });
            tabStrip.tabs('select', currentTabId);
            $('iframe', panel).remove();
        }
        else {//new tab
            pos = tabStrip.tabs('length');
            tabStrip.tabs('add', url, title);
            panel = $($(options.tabstripId + '>div')[pos]);
            panelId = panel.attr('id');
            arrTabs.push({ id: panelId, link: link });
            tabStrip.tabs('select', pos);

        }
        panel.css('overflow', 'hidden'); //new to ...
        panel.append('<div class="bigprogress-icon t-content" style="width:95%;height:90%;position:absolute;background-color:inherit;border:0px;" ></div>');
        panel.append('<iframe frameborder="0" style="width:100%;height:100%;background-color:inherit;direction:rtl;" ></iframe>');

        var iframe = $("iframe", panel);

        if (link.indexOf('istab=1') == -1) {
            if (link.indexOf('?') == -1)
                link = link + "?";
            else
                link = link + "&";
            link = link + 'istab=1';
        }

        iframe.attr('src', link);

        iframe.load(function () {
            contentWindowPath = this.contentWindow.location.pathname + this.contentWindow.location.search;
            window.location.hash = contentWindowPath;
            var tabId = $(this).parent()[0].id;
            $(arrTabs).each(function (i, o) {
                if (o.id == tabId) { o.link = clearLink(contentWindowPath); }
            });

            $('div.bigprogress-icon', panel).hide();
            //$('iframe',panel).first().css('visibility','visible'); //NOTE!! : making iframe invisible at load causes to lost focus at start up (in IE,Firefox)
            $('li>a[href="#' + panelId + '"]', tabStrip).text(this.contentWindow.document.title);
            window.document.title = this.contentWindow.document.title;
        });
    }


    tabStripMain.HandleChildPageLinks = function (ownerWindow) {
        $(linkPattern, ownerWindow.document).live('click', function () {
            parent.$.tabStripMain.AddTab(this);
            return false;
        });
    }

    $.extend({ tabStripMain: tabStripMain });
})(jQuery);

