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

        public ContextMenu<TModel> ContextMenu<TModel>(TModel model) where TModel : class
        {
            return new ContextMenu<TModel>(this.Helper, model);
        }

        public ContextMenu<TModel> ContextMenuFor<TModel>(TModel model) where TModel : class
        {
            return new ContextMenu<TModel>(this.Helper, model);
        }

        internal string GetContextMenuStartupScript()
        {
            return @"
                <script>
                    $(document).on('mouseenter', '.g-context-menu', function () {
                        var mnu=$(this); 
                        if (!mnu.data('t-menu')) {
                            mnu.parent().css('overflow', 'visible'); 
                            var tmnu= mnu.tMenu().data('t-menu') ; 
                            tmnu.openOnClick = mnu.attr('g-open-onclick'); 
                            if(!tmnu.openOnClick){
                                tmnu.open(mnu.find('>li'));
                            } 
                            if (mnu.attr('g-orientation')=='top')
                            {
                                var x= mnu.outerHeight() + 'px';
                                mnu.css({ 'position': 'relative' });//to prevent hiding 
                                $('.t-animation-container', mnu).css({ 'bottom':x, 'position': 'absolute' });
                            }
                        } 
                    });
                </script>";

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

    public enum ContextMenuOpenOrientation
    {
        Default,
        Top
    }

    public enum ContextMenuArrowDirection
    {
        Down,
        Up,
    }

}
