using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Mvc.Html;
using Telerik.Web.Mvc.UI;
using AutoMapper;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
    public static partial class CustomHelper
    {

        #region PartialFor

        public static MvcHtmlString ImagePickerFor<TModel, TPropertyImageKey>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TPropertyImageKey>> expressionImageKey,
            Expression<Func<TModel, HttpPostedFileBase>> expressionImageStream,
            Func<TPropertyImageKey, string> imagePlaceHolderHtmlRetriever,
            Func<string> emptyImagePlaceHolderHtmlRetriever,
            bool includeFieldLabel = true)
        {
            string imageKeyName = helper.GetFullPropertyName(expressionImageKey);
            string imageStreamName = helper.GetFullPropertyName(expressionImageStream, true);
            string imagePlaceHolderId = "imgPlaceHolder" + imageStreamName;
            dynamic imageKeyValue = ModelMetadata.FromLambdaExpression(expressionImageKey, helper.ViewData).Model;
            string imageSelectFunctionName = imageStreamName + "file_selected";
            string removeImageButtonId = "buttonRemoveImage" + imageStreamName;
            string emptyImagePlaceHolderId = "emptyImage" + imageStreamName;

            //create telerik upload
            var telerikUpload = helper.Telerik().Upload()
                .Name(imageStreamName)
                .Localizable("fa-IR")
                .Multiple(false)
                .HtmlAttributes(new { style = "float:right;" })
                .ShowFileList(false)
                .ClientEvents(o => o.OnSelect(imageSelectFunctionName));

            StringBuilder output = new StringBuilder();

            output.AppendFormat(
                @"<div class='t-link t-state-hover image-border'>
                    {0}
                    <div id='{1}'>{2}</div>
                    <div class='image-picker-controlpanel'>
                        <table><tr>
                            <td>{3}</td>
                            <td><button class='t-button t-button-icontext t-upload-action' type='button' id='{4}'><span class='t-icon t-delete'></span></button></td>
                        </tr></table>
                    </div>
                  </div>
                  <div id='{5}' style='display: none; visibility: hidden'>{6}</div>{7}",
                includeFieldLabel ? string.Format("<div class='image-picker-top'>{0}</div>", helper.LabelFor(expressionImageKey)) : "",
                imagePlaceHolderId,
                imagePlaceHolderHtmlRetriever(imageKeyValue),
                telerikUpload.ToHtmlString(),
                removeImageButtonId,
                emptyImagePlaceHolderId,
                emptyImagePlaceHolderHtmlRetriever(),
                helper.HiddenFor(expressionImageKey).ToHtmlString()); 

            output.AppendFormat
                (@"<script type='text/javascript'>
                        function {0}(e) {{
                            $('span', this).html('...تغییر تصویر');
                            $('#{1}').html('<span class=""icon16 check48-png""></span><br/>آماده بارگذاری...');
                        }}
                        $('#{2}').click(function () {{
                            $('#{1}').html($('#{3}').html());
                            var upload = $('#{4}').data('tUpload');
                            $('#{5}').attr('value', '');
                            //todo : remove it from upload control too 
                        }});
                  </script>",
               imageSelectFunctionName,
               imagePlaceHolderId,
               removeImageButtonId,
               emptyImagePlaceHolderId,
               imageStreamName,
               imageKeyName
               );

            return new MvcHtmlString(output.ToString());

            //this function is designed based on the following codes:
            //<div class="t-link t-state-hover image-border">
            //    <div class="image-picker-top">@Html.LabelFor(model => model.ImageFileIdentity)</div>
            //    <div id="imgPlaceHolder">@UiFunctions.ResourceImage(Model.ImageFileIdentity, Greewf.PIN.Infrastracture.Global.ImageSize.Medium)</div>
            //    <div class="image-picker-controlpanel">
            //        <table>
            //            <tr>
            //                <td>@(Html.Telerik().Upload().Name(Html.GetPropertyName(o => o.ImageFile)).Localizable("fa-IR").Multiple(false).HtmlAttributes(new { style = "float:right;" }).ShowFileList(true).ClientEvents(o => o.OnSelect("file_selected")))
            //                </td>
            //                <td>
            //                            <button class="t-button t-button-icontext t-upload-action" type="button" id="buttonRemoveImage">
            //                                <span class="t-icon t-delete"></span>
            //                            </button>
            //                </td>
            //            </tr>
            //        </table>
            //    </div>
            //</div>
            //<div id="emptyImage" style="display: none; visibility: hidden">@UiFunctions.ResourceImage(null, Greewf.PIN.Infrastracture.Global.ImageSize.Medium)</div>
            //<script type="text/javascript">
            //    function file_selected(e) {
            //        $('span', this).html('...تغییر تصویر');
            //        $('#imgPlaceHolder').html('<span class="icon16 check48-png"></span><br/>آماده بارگذاری...');
            //    }
            //    $('#buttonRemoveImage').click(function () {
            //        $('#imgPlaceHolder').html($('#emptyImage').html());
            //        var upload = $('#@Html.GetPropertyName(o => o.ImageFile)').data("tUpload");
            //        $('#@Html.GetPropertyName(o => o.ImageFileIdentity)').attr('value', '');
            //        //todo : remove it from upload control too 
            //    });
            //</script>
            //@Html.HiddenFor(model => model.ImageFileIdentity)


        }



        #endregion
    }

}