using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Text;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{

    public class ContextMenu<TModel> : IHtmlString
        where TModel : class
    {

        private string _name;
        private Func<string> _header;
        private Func<string> _headerClient;
        private bool _adjustRight;
        private ContextMenuStyle _menuStyle = ContextMenuStyle.Menu;
        private TModel _model = null;
        private bool _fillToParent;
        private bool _openOnClick;
        private ContextMenuItemsBuilder<TModel> _itemsBuilder;
        private ContextMenuRenderMode _renderMode = ContextMenuRenderMode.ServerTemplates;
        private ContextMenuOpenOrientation _openOrientation = ContextMenuOpenOrientation.Default;
        private ContextMenuArrowDirection _arrowDirection = ContextMenuArrowDirection.Down;
        

        public ContextMenu(HtmlHelper helper, TModel model)
        {
            _model = model;
            _itemsBuilder = new ContextMenuItemsBuilder<TModel>(_model);
        }

        public ContextMenu<TModel> Name(string name)
        {
            _name = name;
            return this;
        }

        public ContextMenu<TModel> HeaderTemplate(string template)
        {
            _header = () => template;
            return this;
        }

        public ContextMenu<TModel> HeaderTemplate(Func<TModel, IHtmlString> template)
        {
            _header = () => template(_model).ToHtmlString();
            return this;
        }

        public ContextMenu<TModel> HeaderClientTemplate(string template)
        {
            _headerClient = () => template;
            return this;
        }

        public ContextMenu<TModel> HeaderClientTemplate(Func<TModel, IHtmlString> template)
        {
            _headerClient = () => template(null).ToHtmlString();
            return this;
        }


        public ContextMenu<TModel> Items(Action<ContextMenuItemsBuilder<TModel>> configurator)
        {
            configurator(_itemsBuilder);
            return this;
        }

        public ContextMenu<TModel> FillToParent(bool value)
        {
            _fillToParent = value;
            return this;
        }

        public ContextMenu<TModel> AdjustRight(bool value)
        {
            _adjustRight = value;
            return this;
        }

        public ContextMenu<TModel> MenuCss(ContextMenuStyle style)
        {
            _menuStyle = style;
            return this;
        }

        public ContextMenu<TModel> OpenOnClick(bool value)
        {
            _openOnClick = value;
            return this;
        }

        public ContextMenu<TModel> RenderMode(ContextMenuRenderMode value)
        {
            _renderMode = value;
            return this;
        }

        public ContextMenu<TModel> OpenOrientation(ContextMenuOpenOrientation orientation)
        {
            _openOrientation = orientation;
            return this;
        }

        public ContextMenu<TModel> ArrowDirection(ContextMenuArrowDirection arrowDirection)
        {
            _arrowDirection = arrowDirection;
            return this;
        }

        private string getRootStyle()
        {
            switch (_menuStyle)
            {
                case ContextMenuStyle.Simple:
                    return "t-widget t-reset t-menu g-context-menu";
                case ContextMenuStyle.Button:
                    return "t-widget t-reset t-menu t-button g-context-menu";
            }
            return "t-widget t-reset t-header t-menu g-context-menu";
        }

        private string getHeaderStyle()
        {
            switch (_menuStyle)
            {
                case ContextMenuStyle.Button:
                    return "style='background-color:inherit'";
            }
            return "";
        }

        public string ToHtmlString()
        {
            bool renderClientTemplates = _renderMode == CustomHelpers.ContextMenuRenderMode.ClientTemplates;

            var output = new StringBuilder();
            output.AppendFormat("<ul {0} class='{1}' {2} {3} {4}>", getName(), getRootStyle(), getInlineRootCss(), _openOnClick ? "g-open-onclick='true'" : "",getOpenOrientationAttrib());
            output.Append("<li class='t-item XXX-PALCEHOLDER-XXX' style='border:0px;display:block'>");
            output.AppendFormat("<span class='t-link'{0}>{1}<span class='t-icon {2}'></span></span>", getHeaderStyle(), getHeaderText(renderClientTemplates),getArrowIconClass());
            output.AppendFormat("<div class='t-animation-container' style='display: none;{0}'>", _adjustRight ? "" : "left:-1px;right:auto;");
            output.Append("<ul class='t-group' style='display: block;'>");

            bool hasVisibleItem = false;
            foreach (var itemBuilder in _itemsBuilder)
                if (itemBuilder.Item.Visible)
                {
                    output.AppendFormat("<li class='t-item t-state-default'>{0}</li>", getMenuText(renderClientTemplates, itemBuilder));
                    hasVisibleItem = true;
                }

            output.Append("</ul>");
            output.Append("</div>");
            output.Append("</li>");
            output.Append("</ul>");
            output.Append("</ul>");

            if (hasVisibleItem)
                return output.Replace("XXX-PALCEHOLDER-XXX", "t-state-default").ToString();
            else
                return output.Replace("XXX-PALCEHOLDER-XXX", "t-state-disabled").ToString();
        }

        private object getArrowIconClass()
        {
            switch (_arrowDirection)
            {
                case ContextMenuArrowDirection.Up:
                    return  "t-arrow-up";
                default:
                    return  "t-arrow-down";
            }            
        }

        private object getOpenOrientationAttrib()
        {
            switch (_openOrientation)
            {
                case ContextMenuOpenOrientation.Top:
                    return "g-orientation='top'";
                default:
                    return "";
            }
        }

        private string getName()
        {
            if (!string.IsNullOrWhiteSpace("_name"))
                return string.Format("name='{0}' id='{0}'", _name);
            return "";
        }

        private static string getMenuText(bool renderClientTemplates, ContextMenuItemBuilder<TModel> itemBuilder)
        {
            Func<string> t, c;
            t = itemBuilder.Item.Template ?? (() => null);
            c = itemBuilder.Item.ClientTemplate ?? (() => null);

            return renderClientTemplates ? c() : t();
        }

        private string getHeaderText(bool renderClientTemplates)
        {
            if (_header == null) _header = () => null;
            if (_headerClient == null) _headerClient = _header;

            return renderClientTemplates ? _headerClient() : _header();
        }

        private string getInlineRootCss()
        {
            if (!_fillToParent)
                return "style='display:inline-block;white-space: nowrap;text-align:initial'";
            else
                return "";
        }
    }


}
