(function ($) {

    tabStripMain = {};
    var options = { tabstripId: 'tabstripMain', currentPageUrl: '/', currentPageTitle: '', isNewWindowOk: true, isSPA: true, isAJAX: true };
    var arrTabs = new Array();
    var telerikGridRefreshPattern = ':not(div.t-status>a.t-icon.t-refresh)'//this is because of telerik grid refresh button problem in ajax mode
    var telerikGridGroupingPattern = ':not(div.t-group-indicator>a)'//this is because of telerik grid grouping buttons(server-side initiated ones) problem in ajax mode
    var linkPattern = 'a[href][href!^="#"][href!^="#"]:not([href^="javascript:"]):not([target^="_blank"]):not([href*="ajax=True"]):not([inline]):not([justwindow]):not([justMain]):not([responsiveAjax]):not([tooltipWindow])' + telerikGridRefreshPattern + telerikGridGroupingPattern; //pattern which accept links to open
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
        if (o.currentPageUrl.length > 0) tabStripMain.addByLink(o.currentPageUrl, options.currentPageTitle, null, currentPageContent, null);
        if (!options.isSPA)
            $tabs.tabs("remove", 0);//we should remove it after inserting(moving) content to prevent javascript functions brake
        else
            $('> div:first', options.tabstripId).remove();//anyway, removing it is not important!

    }

    function trim(stringToTrim) {
        return stringToTrim.replace(/^\s+|\s+$/g, "");
    }

    tabStripMain.ChangeCurrentTabLocation = function (senderAnchor) {
        addOrReplace($(senderAnchor), true);
    }

    tabStripMain.AddTab = function (senderAnchor, beforeAddingCallBack) {
        if ($(senderAnchor).attr('href')[0].indexOf('#') == 0) return; //becuase senderAnchor.href returns full url. then the pattern works fails in some bookmarking 
        if (beforeAddingCallBack) beforeAddingCallBack();
        addOrReplace($(senderAnchor), false);
    }

    function addOrReplace(senderAnchor, replaceActiveTab) {
        link = senderAnchor.attr('href').toLowerCase();
        title = trim(senderAnchor.text()) == '' ? senderAnchor.attr('title') : trim(senderAnchor.text());

        tabStripMain.addByLink(link, title, replaceActiveTab, null, senderAnchor);
        return false;

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

        var p = jsHelper.getQueryStringParameterByName("_", link);//jquery parameter
        if (p.length > 0) {
            p = "_=" + p;
            link = link
            .replace('&' + p, '')
            .replace('?' + p + '&', '?')
            .replace('?' + p, '');
        }

        return link;
    }

    tabStripMain.addByLink = function (link, title, replaceActiveTab, pageContent, senderAnchor) {
        if (options.isSPA)
            tabStripMain.addByLinkForSPA(link, title, pageContent, senderAnchor);
        else
            tabStripMain.addByLinkForTab(link, title, replaceActiveTab, pageContent, senderAnchor);
    }

    tabStripMain.addByLinkForTab = function (link, title, replaceActiveTab, pageContent, sender) {
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
        var panel = null;

        //make the tab ready (make it or replace it)
        if (replaceActiveTab) {//replace tab
            panelId = currentTabId;
            panel = $('>#' + panelId, tabStrip);
            correctActiveTabInfo(title, link, tabStrip);
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

        makePanelReady(panel, link, tabStrip, pageContent, sender);

    }

    function correctActiveTabInfo(title, link, tabStrip) {
        panelId = currentTabId;
        $('li>a[href="#' + panelId + '"]', tabStrip).text(title);
        panel = $('>#' + panelId, tabStrip);
        $(arrTabs).each(function (i, o) {
            if (currentTabId == o.id) { o.link = link; };
        });
    }

    tabStripMain.addByLinkForSPA = function (link, title, pageContent, sender) {

        var url = link.replace(/\//g, "_");
        link = tabStripMain.clearLink(link);

        //make the tab ready (make it or replace it)
        panel = $('#spaContainer' + options.tabstripId.replace('#', ''), options.tabstripId);
        if (panel.length == 0) panel = $('<div style="height:100%" id="' + 'spaContainer' + options.tabstripId.replace('#', '') + '"></div>').appendTo(options.tabstripId);
        panel.attr('link', link);
        //$('>*', panel).remove(); NOTE : we don't need to remove it! we replace it by html() method in next calls. removing causes page flick which is not good !
        var g = $('<div style="position:absolute;width:100%;height:100%;left:0;right:0;bottom:0;top:0;z-index:999999999"></div>').appendTo(document.body).focus();//it is just to make the shown menu to be hide (because of mouse over!--indeed telerik bug...it should hide the menu after the click...isn't it?)
        makePanelReady(panel, link, null, pageContent, sender);
        window.setTimeout(function () { g.remove() });

    }

    function makePanelReady(panel, link, tabStrip, pageContent, sender) {

        if (options.isAJAX) //we should load the link content through an ajax call(if the pageContent is ready we should ajaxify it). NOTE : it is practical in SPA mode. in none SPA mode , we are prone to have lots of errors (because of javascript interference)
            loadFileOrHtmlThroughAjax(link, panel, pageContent, null, sender);
        else if (pageContent && tabStrip)//first page of tab (which not load in Iframe anyway so we should handle it through ajax(ajaxifying its content))
            loadFileOrHtmlThroughAjax(link, panel, pageContent, tabStrip, sender);
        else
            setPanelContent(panel, link, tabStrip, pageContent);
    }

    function correctLinkForTab(link) {
        if (link.indexOf('istab=1') == -1) {
            if (link.indexOf('?') == -1)
                link = link + "?";
            else
                link = link + "&";
            link = link + 'istab=1';
        }
        return link;
    }

    function setPanelContent(panel, link, tabStrip, pageContent, hideOldContent/*just for ajax mode! so when pageContent is present*/) {
        if (pageContent) {
            if (!hideOldContent) {
                panel.html(pageContent);
            } else//hide old content
            {
                $('>*', panel).css('display', 'none');
                var newContentPointer = { oldTitle: null/*TODO*/, newElement: $(pageContent).prependTo(panel) };
                return newContentPointer;
            }
            return;
        }

        panel.css('overflow', 'hidden'); //new to ...
        panel.append('<div class="bigprogress-icon t-content" style="width:95%;height:99.5%;position:absolute;background-color:inherit;border:0px;" ></div>');

        panel.append('<iframe frameborder="0" style="width:100%;height:100%;background-color:inherit;direction:rtl;" ></iframe>');

        var iframe = $("iframe", panel);

        link = correctLinkForTab(link);

        iframe.attr('src', link);

        iframe.load(function () {
            //todo: same origin policy check!
            var iframeLink = (this.contentWindow.location.pathname + this.contentWindow.location.search).toLowerCase();
            if (iframeLink.indexOf("/savedsuccessfully") > 0 && iframeLink.indexOf("forcetopassedurl=1") > 0) {
                layoutHelper.windowLayout.ShowSuccessMessage('اطلاعات با موفقیت ذخیره شد', 'پیغام سیستم');
                this.contentWindow.location = correctLinkForTab(decodeURI(jsHelper.getQueryStringParameterByName('url', iframeLink)));
                return false;
            }

            var contentWindowPath = decodeURI(tabStripMain.clearLink(iframeLink).toLowerCase());
            if (decodeURI(window.location.pathname + window.location.search).toLowerCase() != contentWindowPath)
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

    function retrieveOldContent(newContentPointer, panel) {
        if (newContentPointer) newContentPointer.newElement.remove();
        $('>*', panel).css('display', '');

    }

    function loadFileOrHtmlThroughAjax(link, panel, pageContent, tabStrip, sender) {
        if (link.indexOf('istab=1') == -1) {
            if (link.indexOf('?') == -1)
                link = link + "?";
            else
                link = link + "&";
            link = link + 'istab=1';
        }

        var supportFile = $(sender).attr('supportFile') != null;
        var senderOptions = {
            sendMethod: $(sender).attr('sendMethod'),
            getPostDataCallback: window[$(sender).attr('getPostDataCallBack')]
        };

        if (supportFile) {
            $.fileDownload(link, {
                cookieName: 'fileDownloadPlugin',
                httpMethod: senderOptions.sendMethod ? senderOptions.sendMethod : 'GET',
                data: senderOptions.getPostDataCallback ? senderOptions.getPostDataCallback() : null,
                prepareCallback: function () {
                    layoutHelper.windowLayout.ShowProgressMessage();
                },
                successCallback: function () {
                    layoutCore.notifySuccessMessage(sender, true);
                    layoutHelper.windowLayout.HideProgressMessage();
                },
                failCallback: function (responseHtml) {
                    layoutHelper.windowLayout.HideProgressMessage();

                    //we dont have any information about wheather it is a common html response or a http error (like access denied ones)
                    //and we have no xhr here. so we sould resend the request through an ajax call to be able to check xhr.
                    loadHtmlThroughAjax(link, panel, pageContent, tabStrip, sender);
                }
            });

        }
        else
            loadHtmlThroughAjax(link, panel, pageContent, tabStrip, sender);



    }

    function loadHtmlThroughAjax(link, panel, pageContent, tabStrip, sender) {
        //NOTE! don't call this method directly. call the parent method('loadFileOrHtmlThroughAjax') instead.

        var firstCallProgress = null;


        layoutHelper.formAjaxifier.load({
            link: layoutHelper.formAjaxifier.correctLink(link, true, false, true, null),
            content: pageContent,
            widgetHtmlTag: panel,
            widgetType: -1,//means tab
            getAddedAjaxWindowContentContainerStyle: function () {
                return 'height:100%';
            },
            beforeSend: function () {
                firstCallProgress = window.setTimeout(function () {
                    setPanelContent(panel, link, null, ajaxProgressHtml());
                }, 600);
            },
            contentReady: function (content, isErrorContent) {
                if (firstCallProgress) window.clearTimeout(firstCallProgress);
                setPanelContent(panel, link, null, content);
            },
            widgetLinkCorrected: function (options, content) {
                correctedLink = options.correctedLink.toLowerCase();
                if (correctedLink != null && correctedLink.indexOf('enforcelayout=1') != -1) {
                    window.location = tabStripMain.clearLink(correctedLink).replace('enforcelayout=1', '');
                    return { cancel: true };
                } else if (correctedLink != null) {
                    if (correctedLink.indexOf("/savedsuccessfully") > 0 && correctedLink.indexOf("forcetopassedurl=1") > 0) {
                        layoutHelper.windowLayout.ShowSuccessMessage('اطلاعات با موفقیت ذخیره شد', 'پیغام سیستم');
                        loadFileOrHtmlThroughAjax(decodeURI(jsHelper.getQueryStringParameterByName('url', correctedLink)), panel, null, null, sender);//TODO: i'm not sure why tabstrip parameter is null.
                        return { cancel: true };
                    }
                    else if ((correctedLink.indexOf("/savedsuccessfully") > 0 && correctedLink.indexOf("forcetopassedurl=1") == -1) || correctedLink.indexOf('successfullysaved') > 0) {
                        layoutHelper.windowLayout.ShowSuccessMessage('اطلاعات با موفقیت ذخیره شد', 'پیغام سیستم');
                        return { cancel: true };
                    }
                }
                var windowPath = decodeURI(tabStripMain.clearLink(window.location.pathname + window.location.search).toLowerCase());
                var newLink = decodeURI(tabStripMain.clearLink(correctedLink).toLowerCase());
                if (windowPath != newLink)
                    window.location.hash = newLink;
                else
                    window.location.hash = "";

                var pageContentTitle = $('#currentPageTitle', panel);
                if (pageContentTitle.length > 0)
                    window.document.title = pageContentTitle.text();
                if (tabStrip)
                    correctActiveTabInfo(window.document.title, newLink, tabStrip);
            },
            loadCompleted: function (isErrorContent) {
            },
            afterSuccessLoadCompleted: function () {
            },
            error: function (isCustomErrorPage, isAccessDeniedPage) {
            },
            innerFormBeforeSubmit: function (form) {
            },
            innerFormBeforeSend: function (innerFormLink) {
                var newContentPointer;
                if (layoutCore.options.showPageFormErrorsInExternalWindow)
                    newContentPointer = setPanelContent(panel, innerFormLink, null, ajaxProgressHtml(), true);
                else
                    setPanelContent(panel, innerFormLink, null, ajaxProgressHtml());
                return newContentPointer;
            },
            retrieveOldContent: function (newContentPointer) {
                retrieveOldContent(newContentPointer, panel);
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

