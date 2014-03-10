using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Microsoft.Web.Samples {
    public static class Sprite {
        private static Control s_helperControl = CreateHelperControl();

        /// <summary>
        /// Creates the proper CSS link reference within the target CSHTML page's head section
        /// </summary>
        /// <param name="virtualPath">The relative path of the image to be displayed, or its directory</param>
        /// <returns>Link tag if the file is found.</returns>
        public static IHtmlString ImportStylesheet(string virtualPath) {
            ImageOptimizations.EnsureInitialized();

            if (Path.HasExtension(virtualPath)) {
                virtualPath = Path.GetDirectoryName(virtualPath);
            }

            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

            string cssFileName = ImageOptimizations.LinkCompatibleCssFile(httpContext.Request.Browser) ?? ImageOptimizations.LowCompatibilityCssFileName;

            virtualPath = Path.Combine(virtualPath, cssFileName);
            string physicalPath = HttpContext.Current.Server.MapPath(virtualPath);

            if (File.Exists(physicalPath)) {
                TagBuilder htmlTag = new TagBuilder("link");
                htmlTag.MergeAttribute("href", ResolveUrl(virtualPath));
                htmlTag.MergeAttribute("rel", "stylesheet");
                htmlTag.MergeAttribute("type", "text/css");
                htmlTag.MergeAttribute("media", "all");
                return new HtmlString(htmlTag.ToString(TagRenderMode.SelfClosing));
            }

            return null;
        }

        /// <summary>
        /// Creates a reference to the sprite / inlined version of the desired image.
        /// </summary>
        /// <param name="virtualPath">The relative path of the image to be displayed</param>
        /// <returns>Image tag.</returns>
        public static IHtmlString Image(string virtualPath) {
            return Image(virtualPath, htmlAttributes: null);
        }

        /// <summary>
        /// Creates a reference to the sprite / inlined version of the desired image including special attributes.
        /// </summary>
        /// <param name="virtualPath">The relative path of the image to be displayed</param>
        /// <param name="htmlAttributes">Html Attributes of object form</param>
        /// <returns>Image tag.</returns>
        public static IHtmlString Image(string virtualPath, object htmlAttributes) {
            return Image(virtualPath, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// Creates a reference to the sprite / inlined version of the desired image including special attributes.
        /// </summary>
        /// <param name="virtualPath">The relative path of the image to be displayed</param>
        /// <param name="htmlAttributes">Html Attributes of IDictionary form</param>
        /// <returns>Image tag.</returns>
        public static IHtmlString Image(string virtualPath, IDictionary<string, object> htmlAttributes) {
            ImageOptimizations.EnsureInitialized();

            TagBuilder htmlTag = new TagBuilder("img");
            htmlTag.MergeAttributes(htmlAttributes);

            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

            if (ImageOptimizations.LinkCompatibleCssFile(httpContext.Request.Browser) == null) {
                htmlTag.MergeAttribute("src", ResolveUrl(virtualPath));
                return new HtmlString(htmlTag.ToString(TagRenderMode.SelfClosing));
            }
            else {
                htmlTag.AddCssClass(ImageOptimizations.MakeCssClassName(virtualPath));
                htmlTag.MergeAttribute("src", ResolveUrl(ImageOptimizations.GetBlankImageSource(httpContext.Request.Browser)));
                return new HtmlString(htmlTag.ToString(TagRenderMode.SelfClosing));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The object is used by the caller and does not go out of scope")]
        private static Control CreateHelperControl() {
            var control = new Control();
            control.AppRelativeTemplateSourceDirectory = "~/";
            return control;
        }

        // REVIEW: Taken from Util.Url in Microsoft.WebPages, is this the best way to do this.
        private static string ResolveUrl(string path) {
            return s_helperControl.ResolveClientUrl(path);
        }
    }
}