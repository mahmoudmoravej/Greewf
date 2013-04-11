(function ($) {

    tabStripMain = {};
    var options = { tabstripId: 'tabstripMain', currentPageUrl: '/', currentPageTitle: '', isNewWindowOk: true, isSPA: true, isAJAX: true };
    var arrTabs = new Array();
    var telerikGridRefreshPattern = ':not(div.t-status>a.t-icon.t-refresh)'//this is because of telerik grid refresh button problem in ajax mode
    var telerikGridGroupingPattern = ':not(div.t-group-indicator>a)'//this is because of telerik grid grouping buttons(server-side initiated ones) problem in ajax mode
    var linkPattern = 'a[href][href!^="#"][href!^="#"]:not([href^="javascript:"]):not([href*="ajax=True"]):not([inline]):not([justwindow]):not([justMain]):not([responsiveAjax]):not([tooltipWindow])' + telerikGridRefreshPattern + telerikGridGroupingPattern; //pattern which accept links to open
    var currentTabId = -1;

    function ajaxProgressHtml() {
        return '<div isProgress="1" class="bigprogress-icon t-content" style="width:99%;height:100%;position:absolute;" ></div>';
    }

    tabStripMain.load = function (o) {
        $.extend(options, o); //merge user passed options with default
        if (!options.isSPA) options.isAJAX = false;//todo : we dont support none SPA (tabular) ajax view.
        if (options.tabstripId.charAt[0] != '#') options.tabstripId = '#' + options.tabstripId;
        if (options.isNewWindowOk) { linkPattern = linkPattern + ':not([newwindow])'; }
        if (!options.isSPA) {
            var tabStrip = $(options.tabstripId);
            tabStrip.find('>ul').show();
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
        } else //in spa mode we dont need it, it causes page height problem
            $(options.tabstripId).find('>ul').remove();

        //listen to all links click
        $(linkPattern).live('click', function () {
            $.tabStripMain.AddTab(this);
            return false;
        });

        currentPageContent = $('> div:first>*', options.tabstripId);
        if (o.currentPageUrl.length > 0) tabStripMain.addByLink(o.currentPageUrl, options.currentPageTitle, null, currentPageContent);
        if (!options.isSPA)
            $tabs.tabs("remove", 0);//we should remove it after inserting(moving) content to prevent javascript functions brake
        else
            $('> div:first', options.tabstripId).remove();//anyway, removing it is not important!

    }

    function trim(stringToTrim) {
        return stringToTrim.replace(/^\s+|\s+$/g, "");
    }

    tabStripMain.ChangeCurrentTabLocation = function (anchor) {
        addOrReplace($(anchor), true);
    }

    tabStripMain.AddTab = function (anchor, beforeAddingCallBack) {
        if ($(anchor).attr('href')[0].indexOf('#') == 0) return; //becuase anchor.href returns full url. then the pattern works fails in some bookmarking 
        if (beforeAddingCallBack) beforeAddingCallBack();
        addOrReplace($(anchor), false);
    }

    function addOrReplace(anchor, replaceActiveTab) {
        link = anchor.attr('href').toLowerCase();
        title = trim(anchor.text()) == '' ? anchor.attr('title') : trim(anchor.text());

        tabStripMain.addByLink(link, title, replaceActiveTab);
        return false;

    }

    function getQueryStringParameterByName(name, link) {
        name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regexS = "[\\?&]" + name + "=([^&#]*)";
        var regex = new RegExp(regexS);
        var results = regex.exec(link);
        if (results == null)
            return "";
        else
            return decodeURIComponent(results[1].replace(/\+/g, " "));
    }

    tabStripMain.clearLink = function (link) {
        link = link.toLowerCase()
            .replace('&istab=1', '')
            .replace('?istab=1&', '?')
            .replace('?istab=1', '')
            .replace('&puremode=1', '')
            .replace('?puremode=1&', '?')
            .replace('?puremode=1', '')
            .replace('&includeurlincontent=1', '')
            .replace('?includeurlincontent=1&', '?')
            .replace('?includeurlincontent=1', '')
            .replace(/^\s+|\s+$/g, "")
            .replace(/\?$/g, "").replace(/\#$/g, '');

        if (link.indexOf("http://") == 0 && link.indexOf("http://" + window.location.hostname.toLowerCase()) == 0)
            link = link.replace("http://" + window.location.hostname, "");

        var p = getQueryStringParameterByName("_", link);//jquery parameter
        if (p.length > 0) {
            p = "_=" + p;
            link = link
            .replace('&' + p, '')
            .replace('?' + p + '&', '?')
            .replace('?' + p, '');
        }

        return link;
    }

    tabStripMain.addByLink = function (link, title, replaceActiveTab, pageContent) {
        if (options.isSPA)
            tabStripMain.addByLinkForSPA(link, title, pageContent);
        else
            tabStripMain.addByLinkForTab(link, title, replaceActiveTab, pageContent);
    }

    tabStripMain.addByLinkForTab = function (link, title, replaceActiveTab, pageContent) {
        link = link.toLowerCase();
        var url = link.replace(/\//g, "_");
        var tabStrip = $(options.tabstripId);

        var oldTabItem = null;
        link = tabStripMain.clearLink(link);
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
        title = pageContent ? title : 'در حال دریافت...';
        var panelId = '';
        var panel = null;

        //make the tab ready (make it or replace it)
        if (replaceActiveTab) {//replace tab
            panelId = currentTabId;
            $('li>a[href="#' + panelId + '"]', tabStrip).text(title);
            panel = $('>#' + panelId, tabStrip);
            $(arrTabs).each(function (i, o) {
                if (currentTabId == o.id) { o.link = link; };
            });
            tabStrip.tabs('select', currentTabId);
            $('>*', panel).remove();
        }
        else {//new tab
            pos = tabStrip.tabs('length');
            tabStrip.tabs('add', url, title);
            panel = $($(options.tabstripId + '>div')[pos]);
            panelId = panel.attr('id');
            arrTabs.push({ id: panelId, link: link });
            tabStrip.tabs('select', pos);

        }

        makePanelReady(panel, link, tabStrip, pageContent);

    }

    tabStripMain.addByLinkForSPA = function (link, title, pageContent) {

        var url = link.replace(/\//g, "_");
        link = tabStripMain.clearLink(link);

        //make the tab ready (make it or replace it)
        panel = $('#spaContainer' + options.tabstripId.replace('#', ''), options.tabstripId);
        if (panel.length == 0) panel = $('<div style="height:100%" id="' + 'spaContainer' + options.tabstripId.replace('#', '') + '"></div>').appendTo(options.tabstripId);
        panel.attr('link', link);
        $('>*', panel).remove();
        var g = $('<div style="position:absolute;width:100%;height:100%;left:0;right:0;bottom:0;top:0;z-index:999999999"></div>').appendTo(document.body).focus();//it is just to make the shown menu to be hide (because of mouse over!--indeed telerik bug...it should hide the menu after the click...isn't it?)
        makePanelReady(panel, link, null, pageContent);
        window.setTimeout(function () { g.remove() });

    }

    function makePanelReady(panel, link, tabStrip, pageContent) {

        if (options.isAJAX) //we should load the link content through an ajax call(if the pageContent is ready we should ajaxify it). NOTE : it is practical in SPA mode. in none SPA mode , we are prone to have lots of errors (because of javascript interference)
            loadThroughAjax(link, panel, pageContent);
        else
            setPanelContent(panel, link, tabStrip, pageContent);
    }

    function setPanelContent(panel, link, tabStrip, pageContent) {
        if (pageContent) {
            panel.html('');
            panel.append(pageContent);
            return;
        }

        panel.css('overflow', 'hidden'); //new to ...
        panel.append('<div class="bigprogress-icon t-content" style="width:95%;height:99.5%;position:absolute;background-color:inherit;border:0px;" ></div>');

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
            var contentWindowPath = tabStripMain.clearLink(this.contentWindow.location.pathname + this.contentWindow.location.search).toLowerCase();
            if ((window.location.pathname + window.location.search).toLowerCase() != contentWindowPath)
                window.location.hash = contentWindowPath;
            else
                window.location.hash = "";

            if (!options.isSPA) {
                var tabId = $(this).parent()[0].id;
                $(arrTabs).each(function (i, o) {
                    if (o.id == tabId) { o.link = contentWindowPath; }
                });
            }

            $('div.bigprogress-icon', panel).hide();
            //$('iframe',panel).first().css('visibility','visible'); //NOTE!! : making iframe invisible at load causes to lost focus at start up (in IE,Firefox)
            if (!options.isSPA) $('li>a[href="#' + panel.attr('id') + '"]', tabStrip).text(this.contentWindow.document.title);
            window.document.title = this.contentWindow.document.title;
        });


    }

    function loadThroughAjax(link, panel, pageContent) {
        if (link.indexOf('istab=1') == -1) {
            if (link.indexOf('?') == -1)
                link = link + "?";
            else
                link = link + "&";
            link = link + 'istab=1';
        }


        layoutHelper.formAjaxifier.load({
            link: layoutHelper.formAjaxifier.correctLink(link, true, false, true, null),
            content: pageContent,
            widgetHtmlTag: panel,
            widgetType: -1,//means tab
            getAddedAjaxWindowContentContainerStyle: function () {
                return 'height:100%';
            },
            beforeSend: function () {
                setPanelContent(panel, link, null, ajaxProgressHtml());
            },
            contentReady: function (content, isErrorContent) {
                setPanelContent(panel, link, null, content);
            },
            widgetLinkCorrected: function (correctedLink, content) {
                correctedLink = correctedLink.toLowerCase();
                if (correctedLink != null && correctedLink.indexOf('enforcelayout=1') != -1) {
                    window.location = tabStripMain.clearLink(correctedLink).replace('enforcelayout=1', '');
                    return { cancel: true };
                } else if (correctedLink != null && (correctedLink.indexOf("/savedsuccessfully") > 0 || correctedLink.indexOf('successfullysaved') > 0)) {
                    layoutHelper.windowLayout.ShowSuccessMessage('اطلاعات با موفقیت ذخیره شد', 'پیغام سیستم');
                }
                var contentWindowPath = tabStripMain.clearLink(window.location.pathname + window.location.search).toLowerCase();
                if (tabStripMain.clearLink(window.location.pathname + window.location.search).toLowerCase() != tabStripMain.clearLink(correctedLink))
                    window.location.hash = tabStripMain.clearLink(correctedLink).toLowerCase();
                else
                    window.location.hash = "";

                var pageContentTitle = $('#currentPageTitle', panel);
                if (pageContentTitle.length > 0)
                    window.document.title = pageContentTitle.text();
                // else
                //     window.document.title = '';
            },
            loadCompleted: function (isErrorContent) {
            },
            afterSuccessLoadCompleted: function () {
            },
            error: function (isCustomErrorPage, isAccessDeniedPage) {
            },
            innerFormBeforeSubmit: function (form) {
            },
            innerFormBeforeSend: function () {
                //var newContentPointer;
                //if (layoutCore.options.showPageFormErrorsInExternalWindow)
                //    newContentPointer = widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout), true);
                //else
                //    widgetLayout.setContent(widget, layoutCore.progressHtml(widgetLayout));
                //changeWidgetTitle(widgetLayout, widgetTitle, 'در حال دریافت...'); //we do it here because we may need to access the current title in "setContent" function
                //return newContentPointer;
            },
            retrieveOldContent: function (newContentPointer) {
                //widgetLayout.retrieveOldContent(widget, newContentPointer);
            }
        });




    }

    tabStripMain.HandleChildPageLinks = function (ownerWindow) {
        $(linkPattern, ownerWindow.document).live('click', function () {
            parent.$.tabStripMain.AddTab(this, function () {
                layoutHelper.tooltipLayout.closeLastTip();
            });
            return false;
        });
    }

    $.extend({ tabStripMain: tabStripMain });
})(jQuery);

