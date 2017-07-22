using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Text;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{

    public class ContextMenuItemBuilder<TModel>
    where TModel : class
    {
        private TModel _model = null;
        public ContextMenuItem Item { get; private set; }

        public ContextMenuItemBuilder(TModel model)
        {
            _model = model;
            Item = new ContextMenuItem();
        }


        public ContextMenuItemBuilder<TModel> Template(Func<TModel, IHtmlString> template)
        {
            Item.Template = () => { return template(_model).ToHtmlString(); };
            return this;
        }

        public ContextMenuItemBuilder<TModel> Template(string template)
        {
            Item.Template = () => { return template; };
            return this;
        }

        public ContextMenuItemBuilder<TModel> ClientTemplate(Func<TModel, IHtmlString> template)
        {
            Item.ClientTemplate = () => { return template(_model).ToHtmlString(); };
            return this;
        }

        public ContextMenuItemBuilder<TModel> ClientTemplate(string template)
        {
            Item.ClientTemplate = () => { return template; };
            return this;
        }

        public ContextMenuItemBuilder<TModel> Visible(bool value)
        {
            Item.Visible = value;
            return this;
        }
    }


}
