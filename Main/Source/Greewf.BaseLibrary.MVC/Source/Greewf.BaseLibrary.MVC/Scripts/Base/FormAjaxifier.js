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

    formAjaxifier.ajax = function (options) {//items should be set are : link,
        if (options.beforeSend) options.beforeSend();
        $.ajax({
            url: encodeURI(options.link),
            cache: false,
            success: function (html) {
                var result = insertAjaxContent(options, html, false); //returns false when handled through special pages
                if (!result.cancel && options.afterSuccessLoadCompleted) options.afterSuccessLoadCompleted();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                insertAjaxContent(options, xhr.responseText, true);
                if (options.error) options.error(xhr.getResponseHeader("GreewfCustomErrorPage") != null, xhr.getResponseHeader("GreewfAccessDeniedPage") != null);
            }
        });
    }

    function insertAjaxContent(options, html, isErrorContent) {

        options.contentReady('<div id="addedAjaxWindowContentContainer" link="' + options.link + '">' + html + '</div>', isErrorContent);

        var urlData = $('#currentPageUrl', options.widgetHtmlTag); //when redirecting in ajax request
        if (urlData.length > 0) {
            var link = urlData.text();
            $('#addedAjaxWindowContentContainer', options.widgetHtmlTag).attr('link', link);
        }

        if (options.widgetLinkCorrected) {
            var result = options.widgetLinkCorrected();
            if (result.cancel) return result;
        }

        //handle validation+content
        enableValidation(options);
        ajaxifyInnerForms(widgetLayout, widget, widgetTitle);

        if (options.loadCompleted) options.loadCompleted(isErrorContent);

        return { cancel: false };

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
                    if (result.cancel) return false;
                }
                var link = correctLink(this.action, true, true, true, options.widgetType);
                var newContentPointer = null; //just when having external window show for errors to preserve current content in error conditions.

                $.ajax({
                    type: this.method.toLowerCase() == 'get' ? 'GET' : 'POST',
                    url: encodeURI(link),
                    cache: false,
                    data: appendSubmitButtonValue($(this).serialize(), this),
                    beforeSend: function () {
                        if (options.innerFormBeforeSend)
                            newContentPointer=options.innerFormBeforeSend();
                    },
                    success: function (html, status, xhr) {
                        insertAjaxContent($.extend({}, options, { link: link }), html, false);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        handleSubmittedFormAjaxErrorContent($.extend({}, options, { link: link }), xhr, newContentPointer);
                    }
                });
                return false;
            });
        });

    }

    function handleSubmittedFormAjaxErrorContent(options,  xhr, newContentPointer) {
        if ($.layoutHelper.core.showPageFormErrorsInExternalWindow) {
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
            insertAjaxContent(options, xhr.responseText,true);

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
