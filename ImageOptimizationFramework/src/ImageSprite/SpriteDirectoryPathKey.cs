namespace Microsoft.Web.Samples {
    internal sealed class SpriteDirectoryPathKey {
        public SpriteDirectoryPathKey(string value) {
            Value = ImageOptimizations.FixVirtualPathSlashes(value).ToUpperInvariant();
        }

        public string Value {
            get;
            private set;
        }

        public override bool Equals(object obj) {
            var otherWrapper = obj as SpriteDirectoryPathKey;
            return ((otherWrapper != null) && (otherWrapper.Value.Equals(Value)));
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }
}