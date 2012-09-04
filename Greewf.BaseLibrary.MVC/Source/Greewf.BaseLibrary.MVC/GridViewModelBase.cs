using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greewf.BaseLibrary.MVC
{
    public class GridViewModelBase<T>
    {
        public GridViewModelBase()
        {
            Editable = true;
        }

        public IEnumerable<T> Data { get; set; }

        public bool Editable { get; set; }

        public bool IsInSearchMode { get; set; }

        public string DefaultAction { get; set; }

        private string _ajaxSelectAction;
        public string AjaxSelectAction
        {
            get
            {
                if (_ajaxSelectAction == null)
                    return DefaultAction;
                return _ajaxSelectAction;
            }
            set
            {
                _ajaxSelectAction = value;
            }
        }
    }
}
