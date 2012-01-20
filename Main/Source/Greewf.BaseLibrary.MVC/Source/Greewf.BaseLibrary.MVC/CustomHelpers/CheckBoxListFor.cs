using System;
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

        #region CheckBoxList

        public enum CheckBoxLisLayout
        {
            SimpleHorizontal,
            SimpleVertical,
            Tabular
        }

        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IEnumerable<SpecialSelectListItem> items, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal)
        {
            return CheckBoxListFor(helper, expression, items, null, null, readOnly, layout);
        }

        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal)
        {
            return CheckBoxListFor(helper, expression, items, parentItems, null, readOnly, layout);
        }


        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IEnumerable<SpecialSelectListItem> items, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal)
        {
            return CheckBoxListFor(helper, expression, items, null, checkboxHtmlAttributes, readOnly, layout);
        }

        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal)
        {

            string name = helper.GetFullPropertyName(expression,false);
            return CheckBoxList(helper, name, items, parentItems, checkboxHtmlAttributes, readOnly, layout);
        }


        public static MvcHtmlString CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal)
        {
            return CheckBoxList(helper, name, items, null, readOnly, layout);
        }


        public static MvcHtmlString CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal)
        {
            return CheckBoxList(helper, name, items, items.Where(o => o.ParentId == null), checkboxHtmlAttributes, readOnly, layout);
        }

        public static MvcHtmlString CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal)
        {
            StringBuilder output = new StringBuilder();
            parentItems = parentItems ?? items.Where(o => o.ParentId == null);

            switch (layout)
            {
                case CheckBoxLisLayout.SimpleHorizontal:
                case CheckBoxLisLayout.SimpleVertical:
                    BuildFieldSetCheckboxList(name, items, parentItems, checkboxHtmlAttributes, readOnly, layout, output);
                    break;
                case CheckBoxLisLayout.Tabular:
                    BuildTabularCheckboxList(helper, name, items, parentItems, checkboxHtmlAttributes, readOnly, output);
                    break;
            }

            return new MvcHtmlString(output.ToString());
        }

        private static void BuildFieldSetCheckboxList(string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, CheckBoxLisLayout layout, StringBuilder output)
        {
            output.Append("<table>");
            bool isVerticalLayout = layout == CheckBoxLisLayout.SimpleVertical;

            if (!isVerticalLayout) output.Append("<tr>");

            foreach (var parentItem in GetParentItems(parentItems))
            {
                if (isVerticalLayout) output.Append("<tr>");
                output.AppendFormat("<td valign='top'><fieldset><legend>{0}</legend>", parentItem.Text);

                foreach (var item in GetChidItems(items, parentItem))
                    output.Append(BuidCheckBox(name, checkboxHtmlAttributes, readOnly, item));

                output.Append("</fieldset></td>");
                if (isVerticalLayout) output.Append("</tr>");
            }

            if (!isVerticalLayout) output.Append("</tr>");
            output.Append("</table>");
        }

        private static IEnumerable<SpecialSelectListItem> GetParentItems(IEnumerable<SpecialSelectListItem> parentItems)
        {
            return parentItems.OrderBy(o => o.Order);
        }

        private static IEnumerable<SpecialSelectListItem> GetChidItems(IEnumerable<SpecialSelectListItem> items, SpecialSelectListItem parentItem)
        {
            return items.Where(o => o.ParentId == int.Parse(parentItem.Value)).OrderBy(o => o.Order);
        }

        private static void BuildTabularCheckboxList(HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, StringBuilder output)
        {

            if (checkboxHtmlAttributes == null) checkboxHtmlAttributes = new Dictionary<string, object>();
            if (!checkboxHtmlAttributes.ContainsKey("height")) checkboxHtmlAttributes.Add("height", "500px");

            var tabStrip = helper.Telerik().TabStrip().Name(name).HtmlAttributes(checkboxHtmlAttributes).SelectedIndex(0).ToComponent();

            foreach (var parentItem in GetParentItems(parentItems))
            {
                var tabStripItem = new TabStripItem() { Text = parentItem.Text };
                tabStrip.Items.Add(tabStripItem);
                string contnt = "";

                foreach (var item in GetChidItems(items, parentItem))
                    contnt += BuidCheckBox(name, checkboxHtmlAttributes, readOnly, item);

                tabStripItem.Html = contnt;

            }

            output.Append(tabStrip.ToHtmlString());

        }

        private static string BuidCheckBox(string name, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, SpecialSelectListItem item)
        {
            string output = "";
            output = "<div class='fields'><label>";
            var checkboxList = new TagBuilder("input");
            checkboxList.MergeAttribute("type", "checkbox");
            checkboxList.MergeAttribute("name", name);
            checkboxList.MergeAttribute("value", item.Value);
            if (readOnly) checkboxList.MergeAttribute("disabled", "disabled");

            // Check to see if it's checked
            if (item.Selected)
                checkboxList.MergeAttribute("checked", "checked");

            // Add any attributes
            if (checkboxHtmlAttributes != null)
                checkboxList.MergeAttributes(checkboxHtmlAttributes);

            checkboxList.SetInnerText(item.Text);
            output += checkboxList.ToString(TagRenderMode.SelfClosing);
            output += "&nbsp; " + item.Text + "</label></div>";

            return output;
        }



        #endregion

    }

}