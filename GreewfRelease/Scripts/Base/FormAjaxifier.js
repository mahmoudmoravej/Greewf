(function ($) {
    formAjaxifier = {};

    formAjaxifier.correctLink = function (link, isPure, isInWindow, inclueUrlInContent, widgetType) {
        if (link.indexOf('?') == -1)
            link = link + "?";
        else
            link = link + "&";
        link = link + (isPure ? checkToPaste(link, 'puremode=1') : checkToPaste(link, 'simplemode=1'));
        if (isInWindow) link = link + checkToPaste(link, '&iswindow=1');
        if (widgetType == 2) link = link + checkToPaste(link, '&istooltip=1'); //1:window , 2:tooltip , -1:tab
        if (widgetType == -1) link = link + checkToPaste(link, '&istab=1'); //1:window , 2:tooltip , -1:tab
        if (inclueUrlInContent) link = link + checkToPaste(link, '&includeUrlInContent=1');

        return link;
    }

    function checkToPaste(str, value) {
        return (str.indexOf(value) >= 0) ? '' : value;
    }

    formAjaxifier.load = function (options) {
        if (options.content) {
            loadContent(options, options.content);
        }
        else
            ajax(options);
    }

    function ajax(options) {
        if (options.beforeSend) options.beforeSend();
        $.ajax({
            url: encodeURI(options.link),
            cache: false,
            type: options.sendMethod != undefined ? options.sendMethod.toUpperCase() : 'GET',
            data: options.getPostData ? options.getPostData() : null,
            success: function (html) {
                loadContent(options, html);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                insertAjaxContent(options, xhr.responseText, true);
                if (options.error) options.error(xhr.getResponseHeader("GreewfCustomErrorPage") != null, xhr.getResponseHeader("GreewfAccessDeniedPage") != null);
            }
        });
    }

    function loadContent(options, html) {
        var result = insertAjaxContent(options, html, false); //returns false when handled through special pages
        if (!result.cancel && options.afterSuccessLoadCompleted) options.afterSuccessLoadCompleted();
    }

    function insertAjaxContent(options, html/*it may be an jquery object*/, isErrorContent) {
        var containerStyle = null;
        if (options.getAddedAjaxWindowContentContainerStyle)
            containerStyle = options.getAddedAjaxWindowContentContainerStyle();

        var closingFetchedData = fetchClosingData(options);

        if (html instanceof jQuery) {
            var x = $('<div id="addedAjaxWindowContentContainer" style="display:none;' + containerStyle + '" link="' + options.link + '"></div>').appendTo(document.body);
            x.append(html);
            options.contentReady(x, isErrorContent);
            x.show();
        }
        else
            options.contentReady('<div id="addedAjaxWindowContentContainer" style="' + containerStyle + '" link="' + options.link + '">' + html + '</div>', isErrorContent);

        var urlData = $('#currentPageUrl', options.widgetHtmlTag); //when redirecting in ajax request
        if (urlData.length > 0) {
            options.link = urlData.text();
            $('#addedAjaxWindowContentContainer', options.widgetHtmlTag).attr('link', options.link);
        }

        if (options.widgetLinkCorrected) {
            var result = options.widgetLinkCorrected({ correctedLink: options.link, closingFetchedData: closingFetchedData }, html);
            if (result && result.cancel) return result;
        }

        //handle validation+content
        enableValidation(options);
        ajaxifyInnerForms(options);

        if (options.loadCompleted) options.loadCompleted(isErrorContent);

        return { cancel: false };

    }

    function fetchClosingData(options) {
        var closeData = null;
        if (options.submitterButton) {
            var pageCloserDataFetcher = $(options.submitterButton).attr('pageCloserDataFetcher');
            if (pageCloserDataFetcher && window[pageCloserDataFetcher])
                closeData = window[pageCloserDataFetcher]();
        }
        return closeData;
    }

    function enableValidation(options) {
        //unobtrusive validation
        if ($.validator.unobtrusive != undefined && $.validator.unobtrusive != null) {
            $(options.widgetHtmlTag).find('form').each(function (i, o) {
                $.validator.unobtrusive.parse(o);
            });
        }
        //non-unobtrusive validation
        if (window.Sys && Sys.Mvc && Sys.Mvc.FormContext) {
            Sys.Mvc.FormContext._Application_Load();
        }
    }

    function handleInnerFormSubmitButtons(form) {
        var buttons = $(':submit', form);
        buttons.each(function (i, o) {
            $(o).click(function () {
                if (this.name != null && this.name.length > 0) $(this).closest('form').attr('submiterName', this.name).data('submitter', o);
            });
        });

        return buttons.length == 1 ? buttons[0] : null; //as default button when exactly one submit button presents
    }





    function ajaxifyInnerForms(options) {
        //NOTE: just disable unobtrosive forms! TODO : handle old fasion ajax forms too
        $(options.widgetHtmlTag).find('form:not([data-ajax*="true"])').each(function (i, o) {
            var defaultSubmitButton = handleInnerFormSubmitButtons(o);
            $(o).submit(function () {
                if (!$(this).valid()) return false;
                if (defaultSubmitButton != null && $(this).data('submitter') == null) $(this).data('submitter', defaultSubmitButton); //asume default button
                if (options.innerFormBeforeSubmit) {
                    result = options.innerFormBeforeSubmit(this);
                    if (result && result.cancel) return false;
                }
                var link = layoutHelper.formAjaxifier.correctLink(this.action, true, options.widgetType == 1, true, options.widgetType);
                var currentForm = this;

                var supportFile = $(this).attr('supportFile') ? true : false;
                if (supportFile)
                    submitForFileOrHtmlResponse(currentForm, link, options);
                else
                    submitForHtmlResponse(currentForm, link, options);

                return false;
            });
        });

    }

    function submitForHtmlResponse(currentForm, link, options) {

        var newContentPointer = null; //just when having external window show for errors to preserve current content in error conditions.

        $.ajax({
            type: currentForm.method.toLowerCase() == 'get' ? 'GET' : 'POST',
            url: encodeURI(link),
            cache: false,
            data: appendSubmitButtonValue($(currentForm).serialize(), currentForm),
            beforeSend: function () {
                if (options.innerFormBeforeSend)
                    newContentPointer = options.innerFormBeforeSend(link);
            },
            success: function (html, status, xhr) {
                insertAjaxContent($.extend({}, options, { link: link, submitterForm: currentForm, submitterButton: $(currentForm).data('submitter') }), html, false);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                handleSubmittedFormAjaxErrorContent($.extend({}, options, { link: link }), xhr, newContentPointer);
            }
        });

    }

    function submitForFileOrHtmlResponse(currentForm, link, options) {

        var newContentPointer = null; //just when having external window show for errors to preserve current content in error conditions.

        $.fileDownload(encodeURI(link), {
            cookieName: 'fileDownloadPlugin',
            httpMethod: currentForm.method.toLowerCase() == 'get' ? 'GET' : 'POST',
            data: appendSubmitButtonValue($(currentForm).serialize(), currentForm),
            prepareCallback: function () {
                if (options.innerFormBeforeSend)
                    newContentPointer = options.innerFormBeforeSend(link);
            },
            successCallback: function () {
                if (options.innerFormSuccessDownloadFile)
                    options.innerFormSuccessDownloadFile();
            },
            failCallback: function (responseHtml) {
                //we dont have any information about wheather it is a common html response or a http error (like access denied ones)
                //and we have no xhr here. so we sould resend the request through an ajax call to be able to check xhr.
                submitForHtmlResponse(currentForm, link, options);
            }
        });

    }

    function handleSubmittedFormAjaxErrorContent(options, xhr, newContentPointer) {
        if (layoutHelper.core.options.showPageFormErrorsInExternalWindow) {
            if (xhr.getResponseHeader("GreewfCustomErrorPage"))//custom error page
            { //TODO : change it if your custom error content needs a different way to show.
                var x = $('.custom-error-page', options.widgetHtmlTag);
                if (x.length == 0)
                    x = $("<div style='display:none' class='custom-error-page'></div>").appendTo(options.widgetHtmlTag);
                x.html(xhr.responseText);

                //we remove these to avoid conflicting with current page (specially we need the previous one in refreshing click)
                $('#currentPageUrl', x).remove();
                $('#currentPageTitle', x).remove();

            }
            else if (xhr.getResponseHeader("GreewfAccessDeniedPage")) {//access denied page
                insertAjaxContent(options, xhr.responseText, true);//todo : currently it doesn't make any problem because the access denied page content is a script which calls windowLayout.ShowErrorMessage. But if we change it, the content may disapear after next line call(CloseTopMost).
                if (options.error) options.error(xhr.getResponseHeader("GreewfCustomErrorPage") != null, xhr.getResponseHeader("GreewfAccessDeniedPage") != null);
            }
            else//regular error
                layoutHelper.windowLayout.ShowErrorMessage('<div style="overflow:auto;direction:ltr;max-width:500px;max-height:600px">' + xhr.responseText + '</div>', 'بروز خطا');
            options.retrieveOldContent(newContentPointer);
        }
        else//internal view is ok
            insertAjaxContent(options, xhr.responseText, true);

    }

    function appendSubmitButtonValue(serializedString, form) {
        var buttonName = $(form).attr('submiterName');
        if (buttonName == null || buttonName == '') return serializedString;
        if (serializedString.length > 0) serializedString = serializedString + '&';
        serializedString = serializedString + buttonName + '=' + $('[name="' + buttonName + '"]', form).val();
        return serializedString;
    }



    $.extend({ formAjaxifier: formAjaxifier });
})(jQuery);
