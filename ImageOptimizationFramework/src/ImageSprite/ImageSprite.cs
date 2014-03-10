using System;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microsoft.Web.Samples {
    public class ImageSprite : Image {
        private bool _usingSprites;
        private string _cssFileName;

        /// <summary>
        /// The "EnableSprites" property (enabled by default) instructs the application to use an optimized sprite or inlined image in place of
        /// a normal image tag (from which this inherits).
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Specifies that the image should use an optimized sprite or inlined image.")]
        public bool EnableSprites {
            get {
                bool? value = (bool?)ViewState["EnableSprites"] ?? true;
                return value.Value;
            }
            set {
                ViewState["EnableSprites"] = value;
            }
        }

        protected override void OnPreRender(EventArgs e) {
            if (PrepareSpriteRendering()) {
                string spriteDirectoryPath = Path.GetDirectoryName(ImageUrl);

                string spriteCssClassName = ImageOptimizations.MakeCssClassName(ImageUrl);
                if (CssClass.Length != 0) {
                    CssClass = spriteCssClassName + " " + CssClass;
                }
                else {
                    CssClass = spriteCssClassName;
                }

                ImageSpriteCssLink.AddCssToPage(Page, Path.Combine(spriteDirectoryPath, _cssFileName));

                _usingSprites = true;
            }

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer) {
            if (_usingSprites) {
                // Need to output the src element but without using the ImageUrl (ViewState)
                string imageUrl = ImageOptimizations.GetBlankImageSource(new HttpContextWrapper(Context).Request.Browser);
                ImageHtmlTextWriter imageHtmlTextWriter = new ImageHtmlTextWriter(writer, ResolveClientUrl(imageUrl));
                base.Render(imageHtmlTextWriter);
            }
            else {
                base.Render(writer);
            }
        }

        /// <summary>
        /// Prepares some field variables and check them if they are correct for sprite rendering.
        /// </summary>
        /// <returns>true if ready for sprite rendering</returns>
        private bool PrepareSpriteRendering() {
            ImageOptimizations.EnsureInitialized();

            if (!EnableSprites) {
                return false;
            }

            _cssFileName = ImageOptimizations.LinkCompatibleCssFile(new HttpContextWrapper(Context).Request.Browser);
            if (String.IsNullOrEmpty(_cssFileName)) {
                return false;
            }

            string spriteDirectoryPath = Path.GetDirectoryName(ImageUrl);

            // Check that CSS file is accessible
            if (!File.Exists(Path.Combine(Context.Server.MapPath(spriteDirectoryPath), _cssFileName))) {
                return false;
            }

            return true;
        }
    }
}