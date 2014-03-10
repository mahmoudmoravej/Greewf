using System.Web.UI;

namespace Microsoft.Web.Samples {
    /// <summary>
    /// Image HtmlTextWriter to prevent the usage of the automatic SRC provided by WebForms.
    /// </summary>
    internal sealed class ImageHtmlTextWriter : HtmlTextWriter {
        private string _imageUrl;

        public ImageHtmlTextWriter(HtmlTextWriter writer, string imageUrl)
            : base(writer.InnerWriter) {
            _imageUrl = imageUrl;
        }

        public override void AddAttribute(HtmlTextWriterAttribute key, string value) {
            if (key == HtmlTextWriterAttribute.Src) {
                value = _imageUrl;
            }

            base.AddAttribute(key, value);
        }

        public override void AddAttribute(HtmlTextWriterAttribute key, string value, bool fEncode) {
            if (key == HtmlTextWriterAttribute.Src) {
                value = _imageUrl;
            }

            base.AddAttribute(key, value, fEncode);
        }

        public override void AddAttribute(string name, string value) {
            if (name == "src") {
                value = _imageUrl;
            }

            base.AddAttribute(name, value);
        }

        public override void AddAttribute(string name, string value, bool fEndode) {
            if (name == "src") {
                value = _imageUrl;
            }

            base.AddAttribute(name, value, fEndode);
        }
    }
}
