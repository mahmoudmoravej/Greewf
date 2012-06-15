using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Mvc.Html;
using Telerik.Web.Mvc.UI;
using AutoMapper;
using System.Web.Routing;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
    public static partial class CustomHelper
    {

        #region CheckBoxList

        public enum CheckBoxLisLayout
        {
            SimpleHorizontal,
            SimpleVertical,
            Tabular,
            Chosen,
            Tree,
            PanelBar
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

            string name = helper.GetFullPropertyName(expression, false);
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

        public static MvcHtmlString CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, object checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal)
        {
            return CheckBoxList(helper, name, items, parentItems, new RouteValueDictionary(checkboxHtmlAttributes), readOnly, layout);
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
                case CheckBoxLisLayout.Tree:
                    BuildTreeCheckboxList(helper, name, items, parentItems, checkboxHtmlAttributes, readOnly, output);
                    break;
                case CheckBoxLisLayout.PanelBar:
                    BuildPanelBaredCheckboxList(helper, name, items, parentItems, checkboxHtmlAttributes, readOnly, output);
                    break;
                case CheckBoxLisLayout.Chosen:
                    BuildChosenList(helper, name, items, parentItems, checkboxHtmlAttributes, readOnly, output);
                    break;
            }

            return new MvcHtmlString(output.ToString());
        }

        private static void BuildChosenList(HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, StringBuilder output)
        {

            if (checkboxHtmlAttributes == null)
                checkboxHtmlAttributes = new RouteValueDictionary();
            else
                checkboxHtmlAttributes = new RouteValueDictionary(checkboxHtmlAttributes);

            if (!checkboxHtmlAttributes.ContainsKey("multiple"))
                checkboxHtmlAttributes.Add("multiple", "multiple");

            if (!checkboxHtmlAttributes.ContainsKey("class"))
                checkboxHtmlAttributes.Add("class", "");
            checkboxHtmlAttributes["class"] = "chzn-rtl " + checkboxHtmlAttributes["class"];

            if (readOnly && checkboxHtmlAttributes.ContainsKey("disabled"))
                checkboxHtmlAttributes.Add("disabled", "disabled");


            var tagSelect = helper.ListBox(name, new List<SelectListItem>(), checkboxHtmlAttributes).ToHtmlString();
            output.Append(tagSelect.Substring(0, tagSelect.IndexOf(">") + 1));

            //TagBuilder tagSelect = new TagBuilder("select");
            //tagSelect.MergeAttribute("name", name);
            //tagSelect.MergeAttribute("multiple", "multiple");


            //tagSelect.AddCssClass("chzn-rtl");
            // if (readOnly)
            //   tagSelect.MergeAttribute("disabled", "disabled");

            if (items.Count() != parentItems.Count() && parentItems.Count() > 0)
                foreach (var parentItem in GetParentItems(parentItems))
                {
                    TagBuilder tagOptGroup = new TagBuilder("optgroup");
                    tagOptGroup.Attributes.Add("label", parentItem.Text);

                    foreach (var item in GetChidItems(items, parentItem))
                        tagOptGroup.InnerHtml += BuidOption(name, checkboxHtmlAttributes, readOnly, item);

                    output.Append(tagOptGroup.ToString());

                }
            else
                foreach (var item in items)
                    output.Append(BuidOption(name, checkboxHtmlAttributes, readOnly, item));

            output.AppendFormat("</{0}>", tagSelect.Substring(1, tagSelect.IndexOf(" ")));
            output.Append("<script  type='text/javascript' language='javascript'>$(document).ready(function(){$('select[name=\"" + name + "\"]').chosen({no_results_text: 'موردی یافت نشد'});});</script>");

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
                    output.Append(BuildCheckBox(name, checkboxHtmlAttributes, readOnly, item));

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
                    contnt += BuildCheckBox(name, checkboxHtmlAttributes, readOnly, item);

                tabStripItem.Html = contnt;

            }

            output.Append(tabStrip.ToHtmlString());

        }

        private static string BuildCheckBox(string name, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, SpecialSelectListItem item)
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

        private static string BuidOption(string name, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, SpecialSelectListItem item)
        {
            string output = "";
            var option = new TagBuilder("option");
            option.MergeAttribute("value", item.Value);
            option.SetInnerText(item.Text);
            if (readOnly) option.MergeAttribute("disabled", "disabled");

            // Check to see if it's checked
            if (item.Selected)
                option.MergeAttribute("selected", "selected");

            // Add any attributes
            //if (checkboxHtmlAttributes != null)
            //    option.MergeAttributes(checkboxHtmlAttributes);

            output += option.ToString(TagRenderMode.Normal);

            return output;
        }

        private static void BuildTreeCheckboxList(HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, StringBuilder output)
        {

            if (checkboxHtmlAttributes == null) checkboxHtmlAttributes = new Dictionary<string, object>();
            if (!checkboxHtmlAttributes.ContainsKey("height")) checkboxHtmlAttributes.Add("height", "500px");

            var treeView = helper.Telerik()
                .TreeView()
                .Name(name)
                .HtmlAttributes(checkboxHtmlAttributes)
                .ClientEvents(o => o.OnExpand(x => "function(x){$(this).data('tTreeView').collapse($('li',this).not(x));}"))
                .ToComponent();

            bool firstGroupExpanded = false;
            foreach (var parentItem in GetParentItems(parentItems))
            {
                var treeViewItem = new TreeViewItem() { Text = parentItem.Text };
                treeView.Items.Add(treeViewItem);

                if (firstGroupExpanded == false)
                {
                    treeViewItem.Selected = true;
                    treeViewItem.Expanded = true;
                    firstGroupExpanded = true;
                }

                foreach (var item in GetChidItems(items, parentItem))
                    treeViewItem.Items.Add(new TreeViewItem()
                    {
                        Text = BuildCheckBox(name, checkboxHtmlAttributes, readOnly, item),
                        Encoded = false
                    });

            }

            output.Append(treeView.ToHtmlString());

        }

        private static void BuildPanelBaredCheckboxList(HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, StringBuilder output)
        {

            if (checkboxHtmlAttributes == null) checkboxHtmlAttributes = new Dictionary<string, object>();
            if (!checkboxHtmlAttributes.ContainsKey("height")) checkboxHtmlAttributes.Add("height", "500px");

            var panelBar = helper.Telerik().PanelBar().Name(name).HtmlAttributes(checkboxHtmlAttributes).SelectedIndex(0).ExpandMode(PanelBarExpandMode.Single).ToComponent();

            foreach (var parentItem in GetParentItems(parentItems))
            {
                var panelBarItem = new PanelBarItem() { Text = parentItem.Text };
                panelBar.Items.Add(panelBarItem);

                foreach (var item in GetChidItems(items, parentItem))
                    panelBarItem.Items.Add(new PanelBarItem() { Text = BuildCheckBox(name, checkboxHtmlAttributes, readOnly, item), Encoded = false });

            }

            output.Append(panelBar.ToHtmlString());

        }        

        #endregion

    }

}