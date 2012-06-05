﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Mvc.Html;
using Telerik.Web.Mvc.UI;
using AutoMapper;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
    public static partial class CustomHelper
    {

        #region CompundComboList

        public static MvcHtmlString CompoundComboListForParent<TModel, TProperty, TPropertyKey>(
          this HtmlHelper<TModel> helper,
          System.Linq.Expressions.Expression<Func<TModel, TPropertyKey>> expressionKey,
          IEnumerable<TProperty> allItems,
          Func<TProperty, SelectListItem> selectListItemGenerator,
          object htmlAttributes = null,
          bool isRigidList = false)
        {
            string fullHtmlName = helper.GetFullPropertyName(expressionKey, false);
            return CompoundComboListForParent(helper, fullHtmlName, allItems, selectListItemGenerator, htmlAttributes, isRigidList);
        }

        public static MvcHtmlString CompoundComboListForParent<TModel, TProperty>(
            this HtmlHelper<TModel> helper,
            string name,
            IEnumerable<TProperty> allItems,
            Func<TProperty, SelectListItem> selectListItemGenerator,
            object htmlAttributes = null,
            bool isRigidList = false)
        {
            string fullHtmlName = name;
            //string changeJsFunction = name.Replace(".", "_") + "Changed";

            StringBuilder output = new StringBuilder();

            if (isRigidList)
            {
                //output.Append("<script>function funcccc(){ " + changeJsFunction + "(); }</script>");
                output.Append(
                    new MvcHtmlString(
                     helper.Telerik()
                        .DropDownList()
                        .Name(fullHtmlName)
                        .Encode(false)
                        .SelectedIndex(int.MaxValue)
                        .ClientEvents(o => o
                            .OnLoad(x => "function(){$(this).data('child-listeners',[]);}")
                            .OnChange(x => "function(){$($(this).data('child-listeners')).each(function(i,o){ o(); });}"))
                        .HtmlAttributes(htmlAttributes)
                    //.Value("")
                        .BindTo(allItems.Select(selectListItemGenerator)).ToHtmlString())

                 );
            }
            else
                output.Append(
                new MvcHtmlString(
                     helper.Telerik()
                        .ComboBox()
                        .Name(fullHtmlName)
                        .HighlightFirstMatch(true)
                        .OpenOnFocus(true)
                        .Encode(false)
                        .ClientEvents(o => o
                            .OnLoad(x => "function(){$(this).data('child-listeners',[]);}")
                            .OnChange(x => "function(){$($(this).data('child-listeners')).each(function(i,o){ o(); });}"))
                    //.ClientEvents(o => o.OnChange(x => "function(){" + changeJsFunction + "();}"))
                        .HtmlAttributes(htmlAttributes)
                        .BindTo(allItems.Select(selectListItemGenerator)).ToHtmlString()
                 ));

            return new MvcHtmlString(output.ToString());

        }

        public static MvcHtmlString CompoundComboListForChild<TModel, TProperty, TPropertyKey, TParentProperty, TParentPropertyKey>(
            this HtmlHelper<TModel> helper,
            System.Linq.Expressions.Expression<Func<TModel, TPropertyKey>> expressionKey,
            System.Linq.Expressions.Expression<Func<TModel, TParentProperty>> expressionParent,
            System.Linq.Expressions.Expression<Func<TModel, TParentPropertyKey>> expressionParentKey,
            IEnumerable<TProperty> allItems,
            Func<TProperty, TParentProperty, bool> parentMatcher,
            Func<TProperty, SelectListItem> selectListItemGenerator,
            string childsLoaderUrl,
            string childsLoaderUrlParameterName,
            object htmlAttributes = null,
            bool isParentToo = false,
            bool isRigidList = false)
        {

            string childKeyName = helper.GetFullPropertyName(expressionKey);
            string childFullHtmlName = helper.GetFullPropertyName(expressionKey, false);
            string parentKeyName = helper.GetFullPropertyName(expressionParentKey);

            TParentProperty parentValue = (TParentProperty)ModelMetadata.FromLambdaExpression(expressionParent, helper.ViewData).Model;
            // TPropertyKey childValue = (TPropertyKey)ModelMetadata.FromLambdaExpression(expressionKey, helper.ViewData).Model;

            string func_ChildLoaded = childKeyName + "Loaded";
            string func_LoadChilds = "load" + childKeyName;
            //string func_ParentChanged = parentKeyName + "Changed";

            StringBuilder output = new StringBuilder();
            //string changeJsFunction = childFullHtmlName.Replace(".", "_") + "Changed";


            //1st: script generation  (becuase of some rare problems in ajax loading , we put scripts at first)
            output.Append("<script type='text/javascript'>");
            //output.AppendFormat("function {0}(){{ {1}(); }}", func_ParentChanged, func_LoadChilds);


            //why do we need this when filling at server?
            output.AppendFormat("function {0}() {{ {1} {2} {3} }}",
                func_ChildLoaded,
                string.Format("   $('#{0}').data('child-listeners').push(function(){{ {1}(); }}); ", parentKeyName, func_LoadChilds),
                string.Format("   var ddC = $('#{0}').data('tComboBox');", childKeyName),
                string.Format("   if ((ddC.value() && ddC.value()!=0) && ddC.length==0) {{ {0}(); }}", func_LoadChilds)
            );


            output.AppendFormat("function {0}() {{ {1} {2} {3} {4} }}",
                func_LoadChilds,
                string.Format("var ddP = $('#{0}').data('tComboBox');", parentKeyName),//tDropDownList
                string.Format("var ddC = $('#{0}').data('tComboBox');if(ddC==undefined) {{return;}}", childKeyName),//in some senarios (when the control is load through ajax) ddC is null!
                              "ddC.dataBind({}); if(!(ddP.value() && ddP.value()!=0)) {return;} ddC.disable();ddP.disable();",// means : if ddP has not value
                string.Format("$.ajax({{ type: 'POST',url: '{0}',data: '{1}=' + ddP.value(),success: function (data) {{ddC.enable();ddP.enable();ddC.dataBind(data);}},error: function (req, status, error) {{ddC.enable();ddP.enable();alert('خطا در دریافت اطلاعات')}} }});", GetUrl(childsLoaderUrl), childsLoaderUrlParameterName)
            );

            output.Append("</script>");


            //2nd : telerik component 
            if (isRigidList)
            {
                var telerikDropDown =
                    helper.Telerik()
                        .DropDownList()
                        .Name(childFullHtmlName)
                        .Encode(false)
                        .ClientEvents(o => o.OnLoad(x => "function(){$(this).data('child-listeners',[]); " + func_ChildLoaded + "();}"))
                        .HtmlAttributes(htmlAttributes)
                    //.Value("")
                        .BindTo(allItems.Where(o => (parentValue != null) && parentMatcher(o, parentValue)).Select(selectListItemGenerator));

                if (isParentToo)
                    telerikDropDown.ClientEvents(o => o
                        //.OnLoad(x => "function(){$(this).data('child-listeners',[]);}")
                        .OnChange(x => "function(){$($(this).data('child-listeners')).each(function(i,o){ o(); });}"));
                        //telerikDropDown.ClientEvents(o => o.OnChange(changeJsFunction));

                output.Append(telerikDropDown.ToHtmlString());
            }
            else
            {
                var telerikCombo =
                    helper.Telerik()
                        .ComboBox()
                        .Name(childFullHtmlName)
                        .HighlightFirstMatch(true)
                        .OpenOnFocus(true)
                        .Encode(false)
                        .ClientEvents(o => o.OnLoad(x => "function(){$(this).data('child-listeners',[]); " + func_ChildLoaded + "();}"))
                        .HtmlAttributes(htmlAttributes)
                        .BindTo(allItems.Where(o => (parentValue != null) && parentMatcher(o, parentValue)).Select(selectListItemGenerator));

                if (isParentToo)
                    telerikCombo.ClientEvents(o => o
                        //.OnLoad(x => "function(){$(this).data('child-listeners',[]);}")
                        .OnChange(x => "function(){$($(this).data('child-listeners')).each(function(i,o){ o(); });}"));

                output.Append(telerikCombo.ToHtmlString());

            }



            if (isRigidList) output = output.Replace(".data('tComboBox')", ".data('tDropDownList')");

            return new MvcHtmlString(output.ToString());

            /*sample output result : 
            <script type="text/javascript">
                function provinceChanged() {
                    loadCities();
                }

                function citiesLoaded() {
                    var ddC = $('#@(Html.GetPropertyName(o => o.CityCode))').data("tDropDownList");
                    if (!ddC.value() > 0) { loadCities(); }
                }

                function loadCities() {
                    var ddP = $('#@(Html.GetPropertyName(o => o.Province.Id))').data("tDropDownList");
                    var ddC = $('#@(Html.GetPropertyName(o => o.CityCode))').data("tDropDownList");
                    ddC.disable();
                    ddP.disable();

                    $.ajax(
                             { type: "POST",
                                 url: '@Url.Content("~/AttributeValues/Cities")',
                                 data: "provinceId=" + ddP.value(),
                                 success: function (data) {
                                     ddC.enable();
                                     ddP.enable();
                                     ddC.dataBind(data);
                                 },
                                 error: function (req, status, error) {
                                     ddC.enable();
                                     ddP.enable();
                                     alert('خطا در دریافت اطلاعات')
                                 }
                             });
                }
        
            </script>*/

        }


        #endregion

    }

}