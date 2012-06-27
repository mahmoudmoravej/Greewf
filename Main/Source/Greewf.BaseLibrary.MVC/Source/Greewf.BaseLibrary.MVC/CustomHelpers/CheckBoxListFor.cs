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

        public enum SelectionMode
        {
            Multiple,
            Single
        }

        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IEnumerable<SpecialSelectListItem> items, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal, SelectionMode selectionMode = SelectionMode.Multiple)
        {
            return CheckBoxListFor(helper, expression, items, null, null, readOnly, layout, selectionMode);
        }

        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal, SelectionMode selectionMode = SelectionMode.Multiple)
        {
            return CheckBoxListFor(helper, expression, items, parentItems, null, readOnly, layout, selectionMode);
        }


        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IEnumerable<SpecialSelectListItem> items, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal, SelectionMode selectionMode = SelectionMode.Multiple)
        {
            return CheckBoxListFor(helper, expression, items, null, checkboxHtmlAttributes, readOnly, layout, selectionMode);
        }

        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal, SelectionMode selectionMode = SelectionMode.Multiple)
        {

            string name = helper.GetFullPropertyName(expression, false);
            return CheckBoxList(helper, name, items, parentItems, checkboxHtmlAttributes, readOnly, layout, selectionMode);
        }


        public static MvcHtmlString CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal, SelectionMode selectionMode = SelectionMode.Multiple)
        {
            return CheckBoxList(helper, name, items, null, readOnly, layout, selectionMode);
        }


        public static MvcHtmlString CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal, SelectionMode selectionMode = SelectionMode.Multiple)
        {
            return CheckBoxList(helper, name, items, items.Where(o => o.ParentId == null), checkboxHtmlAttributes, readOnly, layout, selectionMode);
        }

        public static MvcHtmlString CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, object checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal, SelectionMode selectionMode = SelectionMode.Multiple)
        {
            return CheckBoxList(helper, name, items, parentItems, new RouteValueDictionary(checkboxHtmlAttributes), readOnly, layout, selectionMode);
        }
        public static MvcHtmlString CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly = false, CheckBoxLisLayout layout = CheckBoxLisLayout.SimpleHorizontal, SelectionMode selectionMode = SelectionMode.Multiple)
        {
            StringBuilder output = new StringBuilder();
            parentItems = parentItems ?? items.Where(o => o.ParentId == null);

            if (selectionMode == SelectionMode.Single && layout != CheckBoxLisLayout.Chosen)
                throw new Exception("Single Mode Selection just works in Chosen layout currently! Ask the Greewf owner(s) to implement it if you need it in other layout! ;) ");

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
                    BuildChosenList(helper, name, items, parentItems, checkboxHtmlAttributes, readOnly, output, selectionMode);
                    break;
            }

            return new MvcHtmlString(output.ToString());
        }

        private static void BuildChosenList(HtmlHelper helper, string name, IEnumerable<SpecialSelectListItem> items, IEnumerable<SpecialSelectListItem> parentItems, IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, StringBuilder output, SelectionMode selectionMode = SelectionMode.Multiple)
        {

            if (checkboxHtmlAttributes == null)
                checkboxHtmlAttributes = new RouteValueDictionary();
            else
                checkboxHtmlAttributes = new RouteValueDictionary(checkboxHtmlAttributes);

            if (selectionMode == SelectionMode.Multiple && !checkboxHtmlAttributes.ContainsKey("multiple"))
                checkboxHtmlAttributes.Add("multiple", "multiple");

            if (!checkboxHtmlAttributes.ContainsKey("class"))
                checkboxHtmlAttributes.Add("class", "");
            checkboxHtmlAttributes["class"] = "chzn-rtl " + checkboxHtmlAttributes["class"];

            if (readOnly && checkboxHtmlAttributes.ContainsKey("disabled"))
                checkboxHtmlAttributes.Add("disabled", "disabled");


            //NOTE!!! : if you pass a name which is defined in your MODEL , its type should be enumerable in multiple selection mode. otherwise get a strange exception which says Null refrence !
            string tagSelect = "";
            if (selectionMode == SelectionMode.Multiple)
                tagSelect = helper.ListBox(name, new List<SelectListItem>(), checkboxHtmlAttributes).ToHtmlString();
            else
                tagSelect = helper.DropDownList(name, new List<SelectListItem>(), checkboxHtmlAttributes).ToHtmlString();

            output.Append(tagSelect.Substring(0, tagSelect.IndexOf(">") + 1));
            output.Append(BuidOption(null, readOnly, new SpecialSelectListItem() { }));//we need it to show " (select...) " item. it is the Chosen rule.

            if (items.Count() != parentItems.Count() && parentItems.Count() > 0)
                foreach (var parentItem in GetParentItems(parentItems))
                {
                    TagBuilder tagOptGroup = new TagBuilder("optgroup");
                    tagOptGroup.Attributes.Add("label", parentItem.Text);

                    foreach (var item in GetChidItems(items, parentItem))
                        tagOptGroup.InnerHtml += BuidOption(checkboxHtmlAttributes, readOnly, item);

                    output.Append(tagOptGroup.ToString());

                }
            else
                foreach (var item in items)
                    output.Append(BuidOption( checkboxHtmlAttributes, readOnly, item));

            output.AppendFormat("</{0}>", tagSelect.Substring(1, tagSelect.IndexOf(" ")));
            output.AppendFormat("<script  type='text/javascript' language='javascript'>$(document).ready(function(){{$('select[name=\"" + name + "\"]').chosen({{no_results_text: 'موردی یافت نشد'{0}}});}});</script>", selectionMode == SelectionMode.Single ? ",allow_single_deselect: true":"");

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

        private static string BuidOption(IDictionary<string, object> checkboxHtmlAttributes, bool readOnly, SpecialSelectListItem item)
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