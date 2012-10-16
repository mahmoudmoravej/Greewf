using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Text;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{

    public partial class GreewfHelperFactory
    {

        public ContextMenu<object> ContextMenu()
        {
            return new ContextMenu<object>(this.Helper, null);
        }

        public ContextMenu<TModel> ContextMenu<TModel>( TModel model) where TModel : class
        {
            return new ContextMenu<TModel>(this.Helper, model);
        }

        public ContextMenu<TModel> ContextMenuFor<TModel>( TModel model) where TModel : class
        {
            return new ContextMenu<TModel>(this.Helper, model);
        }

    }

    public enum ContextMenuStyle
    {
        Menu,
        Simple,
        Button
    }

    public enum ContextMenuRenderMode
    {
        ServerTemplates,
        ClientTemplates
    }
 
}
