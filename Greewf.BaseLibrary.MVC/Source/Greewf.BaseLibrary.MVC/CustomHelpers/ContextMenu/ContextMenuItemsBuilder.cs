using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Text;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
   

    public class ContextMenuItemsBuilder<TModel> : IEnumerable<ContextMenuItemBuilder<TModel>>
        where TModel : class
    {
        private TModel _model = null;
        public List<ContextMenuItemBuilder<TModel>> ItemsBuilder { get; private set; }

        public ContextMenuItemsBuilder(TModel model)
        {
            _model = model;
            ItemsBuilder = new List<ContextMenuItemBuilder<TModel>>();
        }

        public ContextMenuItemBuilder<TModel> Add()
        {
            var x = new ContextMenuItemBuilder<TModel>(_model);
            ItemsBuilder.Add(x);
            return x;
        }



        public IEnumerator<ContextMenuItemBuilder<TModel>> GetEnumerator()
        {
            return ItemsBuilder.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ItemsBuilder.GetEnumerator();
        }
    }

   
}
