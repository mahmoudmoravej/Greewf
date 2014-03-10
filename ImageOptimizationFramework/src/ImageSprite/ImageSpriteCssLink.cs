using System;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Microsoft.Web.Samples {
    public class ImageSpriteCssLink : Control {
        /// <summary>
        /// The relative path to the directory in which the CSS files are to be linked from.
        /// </summary>
        [Category("Behavior")]
        [Description("The relative path to the directory in which the CSS files are to be linked from.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design.")]
        public string ImageUrl {
            get {
                return (string)ViewState["ImageUrl"];
            }
            set {
                ViewState["ImageUrl"] = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive.")]
        internal static void AddCssToPage(Page page, string href) {
            string keyDirectory = href;
            if (Path.HasExtension(keyDirectory)) {
                keyDirectory = Path.GetDirectoryName(keyDirectory);
            }

            SpriteDirectoryPathKey key = new SpriteDirectoryPathKey(keyDirectory);

            if (!page.Items.Contains(key)) {
                page.Items.Add(key, null);

                HtmlLink css = new HtmlLink();
                css.Href = ImageOptimizations.FixVirtualPathSlashes(href);
                css.Attributes["rel"] = "stylesheet";
                css.Attributes["type"] = "text/css";
                css.Attributes["media"] = "all";
                page.Header.Controls.Add(css);
            }
        }

        protected override void OnPreRender(EventArgs e) {
            ImageOptimizations.EnsureInitialized();

            if (Path.HasExtension(ImageUrl) || ImageUrl.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) || ImageUrl.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase)) {
                ImageUrl = Path.GetDirectoryName(ImageUrl);
            }

            string cssFileName = ImageOptimizations.LinkCompatibleCssFile(new HttpContextWrapper(Context).Request.Browser) ?? ImageOptimizations.LowCompatibilityCssFileName;

            // Set up fileName and path variables
            string localPath = Context.Server.MapPath(ImageUrl);

            // Check that CSS file is accessible
            if (!File.Exists(Path.Combine(localPath, cssFileName))) {
                return;
            }

            AddCssToPage(Page, Path.Combine(ImageUrl, cssFileName));
        }
    }
}