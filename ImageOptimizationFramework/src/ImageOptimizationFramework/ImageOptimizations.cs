using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Xml;

namespace Microsoft.Web.Samples {

    /// <summary>
    /// Automates the creation of sprites and base64 inlining for CSS
    /// </summary>
    public static class ImageOptimizations {
        public static readonly string TimestampFileName = "timeStamp.dat";
        public static readonly string SettingsFileName = "settings.xml";
        public static readonly string HighCompatibilityCssFileName = "highCompat.css";
        public static readonly string LowCompatibilityCssFileName = "lowCompat.css";
        public static readonly string BlankFileName = "blank.gif";
        public static readonly string SpriteDirectoryRelativePath = "~/App_Sprites/";
        public static readonly string GeneratedSpriteFileName = "sprite{0}";
        public static readonly string SpriteFileNameRegex = "^sprite[0-9]+$";
        public static readonly bool SupportIE8MaximalInlineSize = true;

        private static readonly string[] _extensions = { "*.jpg", "*.gif", "*.png", "*.bmp", "*.jpeg" };
        private static readonly object _lockObj = new object();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Inlined")]
        private static readonly string TransparentGif = "R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
        public static readonly string InlinedTransparentGif = "data:image/gif;base64," + TransparentGif;
        private static readonly int IE8MaximalInlineSize = 32768;

        public static bool Initialized { get; set; }

        /// <summary>
        /// Ensures the ImageOptimizations has been initialized.
        /// </summary>
        public static void EnsureInitialized() {
            if (!ImageOptimizations.Initialized) {
                throw new InvalidOperationException(ImageOptimizationResources.InitializationErrorMessage);
            }
        }

        /// <summary>
        /// Makes the appropriate CSS ID name for the sprite to be used.
        /// </summary>
        /// <param name="pathToImage">The path to the image</param>
        /// <param name="pathToSpriteDirectory">The path to the directory used to store sprites, used if the path to the image was not relative to the sprites directory</param>
        /// <returns>The CSS class used to reference the optimized image</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "By design.")]
        public static string MakeCssClassName(string pathToImage, string pathToSpriteDirectory = null) {
            if (pathToImage == null) {
                throw new ArgumentNullException("pathToImage");
            }

            string cssFilename = TrimPathForCss(pathToImage, pathToSpriteDirectory);
            return CssNameHelper.GenerateClassName(cssFilename);
        }

        public static string GetBlankImageSource(HttpBrowserCapabilitiesBase browser) {
            if (browser == null) {
                return null;
            }

            if (browser.Type.ToUpperInvariant().Contains("IE") && browser.MajorVersion <= 7) {
                return FixVirtualPathSlashes(Path.Combine(SpriteDirectoryRelativePath, BlankFileName));
            }

            return InlinedTransparentGif;
        }

        /// <summary>
        /// Returns the name of the CSS file containing the best compatibility settings for the user's browser. Returns null if the browser does not support any optimizations. 
        /// </summary>
        /// <param name="browser">The HttpBrowserCapabilities object for the user's browser</param>
        /// <returns>The name of the correct CSS file, or Null if not supported</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static string LinkCompatibleCssFile(HttpBrowserCapabilitiesBase browser) {
            if (browser == null) {
                return null;
            }

            if (browser.Type.ToUpperInvariant().Contains("IE")) {
                if (browser.MajorVersion < 6) {
                    return null;
                }
                else if (browser.MajorVersion <= 7) {
                    return LowCompatibilityCssFileName;
                }
            }
            else if (browser.Type.ToUpperInvariant().Contains("FIREFOX")) {
                if (browser.MajorVersion < 2) {
                    return LowCompatibilityCssFileName;
                }
            }

            return HighCompatibilityCssFileName;
        }

        /// <summary>
        /// Checks if an image (passed by path or image name) is a sprite image or CSS file created by the framework
        /// </summary>
        /// <param name="path">The path or filename of the image in question</param>
        /// <returns>True if the image is a sprite, false if it is not</returns>
        public static bool IsOutputSprite(string path) {
            string name = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path).TrimStart('.');
            List<string> imageExtensions = new List<string>(_extensions);

            return ((Regex.Match(name, SpriteFileNameRegex).Success && imageExtensions.Contains("*." + extension)) || extension == "css");
        }

        internal static string MakeCssSelector(string pathToImage, string pathToSpriteDirectory = null) {
            string cssFilename = TrimPathForCss(pathToImage, pathToSpriteDirectory);
            return CssNameHelper.GenerateSelector(cssFilename);
        }

        /// <summary>
        /// Rebuilds the cache / dependencies for all subdirectories below the specified directory
        /// </summary>
        /// <param name="path">The root directory for the cache rebuild (usually app_sprites)</param>
        /// <param name="rebuildImages">Indicate whether the directories should be rebuilt as well</param>
        internal static void AddCacheDependencies(string path, bool rebuildImages) {
            List<string> subDirectories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
            subDirectories.Add(path);

            foreach (string subDirectory in subDirectories) {
                if (rebuildImages) {
                    ProcessDirectory(subDirectory, checkIfFilesWereModified: true);
                }

                InsertItemIntoCache(subDirectory, Directory.GetDirectories(subDirectory));
            }
        }

        /// <summary>
        /// Executes the image optimizer on a specific subdirectory of the root image directory (non-recursive)
        /// </summary>
        /// <param name="path">The path to the directory to be rebuilt</param>
        /// <param name="checkIfFilesWereModified">Indicate whether the directory should only be rebuilt if files were modified</param>
        /// <returns>False if the directory does not exist</returns>
        internal static bool ProcessDirectory(string path, bool checkIfFilesWereModified) {
            // Check if directory was deleted
            if (!Directory.Exists(path)) {
                return false;
            }

            try {
                if (checkIfFilesWereModified && !HaveFilesBeenModified(path)) {
                    return true;
                }

                // Make a list of the disk locations of each image
                List<string> imageLocations = new List<string>();

                foreach (string extension in _extensions) {
                    imageLocations.AddRange(Directory.GetFiles(path, extension));
                }

                // Make sure to delete any existing sprites (or other images with the filename sprite###.imageExtension)
                imageLocations.RemoveAll(DeleteSpriteFile);

                // Make sure to not include the blank.gif file
                string blankFile = HostingEnvironment.MapPath(Path.Combine(SpriteDirectoryRelativePath, BlankFileName)).ToUpperInvariant();
                imageLocations.RemoveAll(file => blankFile == file.ToUpperInvariant());

                // Import settings from settings file
                ImageSettings settings = GetSettings(path);

                // Create pointer to the CSS output file
                lock (_lockObj) {
                    using (TextWriter cssHighCompatOutput = new StreamWriter(Path.Combine(path, HighCompatibilityCssFileName), append: false),
                                      cssLowCompatOutput = new StreamWriter(Path.Combine(path, LowCompatibilityCssFileName), append: false)) {

                        PerformOptimizations(path, settings, cssHighCompatOutput, cssLowCompatOutput, imageLocations);

                        // Merge with a user's existing CSS file(s)
                        MergeExternalCss(path, cssHighCompatOutput, cssLowCompatOutput);
                    }
                }

                imageLocations.Clear();
                foreach (string extension in _extensions) {
                    imageLocations.AddRange(Directory.GetFiles(path, extension));
                }

                SaveFileModificationData(path);
                return true;
            }
            catch (Exception) {
                if (!Directory.Exists(path)) {
                    return false;
                }
                throw;
            }
        }

        internal static void SaveBlankFile(string path) {
            string blankFileFullPath = Path.Combine(path, BlankFileName);
            if (!File.Exists(blankFileFullPath)) {
                using (FileStream blankFile = new FileStream(blankFileFullPath, FileMode.OpenOrCreate, FileAccess.Write)) {
                    byte[] data = Convert.FromBase64String(TransparentGif);
                    blankFile.Write(data, 0, data.Length);
                }
            }
        }

        // Copied from \ndp\fx\src\xsp\System\Web\Util\UrlPath.cs
        // Change backslashes to forward slashes, and remove duplicate slashes
        internal static string FixVirtualPathSlashes(string virtualPath) {
            // Make sure we don't have any back slashes
            virtualPath = virtualPath.Replace('\\', '/');

            // Replace any double forward slashes
            for (; ; ) {
                string newPath = virtualPath.Replace("//", "/");

                // If it didn't do anything, we're done
                if ((object)newPath == (object)virtualPath)
                    break;

                virtualPath = newPath;
            }

            return virtualPath;
        }

        private static string TrimPathForCss(string pathToImage, string pathToSpriteDirectory) {
            pathToSpriteDirectory = pathToSpriteDirectory ?? ImageOptimizations.SpriteDirectoryRelativePath;
            pathToImage = MakePathRelative(pathToImage, pathToSpriteDirectory);

            return pathToImage.Replace('\\', '/');
        }

        /// <summary>
        /// Called when the cache dependancy of a subdirectory of the root image directory is modified, created, or removed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Keeping the cache dependency active.")]
        private static void RebuildFromCacheHit(string key, object value, CacheItemRemovedReason reason) {
            var data = (Tuple<string, IEnumerable<string>>)value;
            string path = data.Item1;
            IEnumerable<string> cachedDirectoriesBelowCurrentDirectory = data.Item2;
            IEnumerable<string> directoriesBelowCurrentDirectory;

            try {
                directoriesBelowCurrentDirectory = Directory.GetDirectories(path);
            }
            catch (Exception) {
                // If the directory is not found, it was probably deleted, and the cache item does not need to be re-inserted
                if (!Directory.Exists(path)) {
                    return;
                }

                throw;
            }

            switch (reason) {
                case CacheItemRemovedReason.DependencyChanged:
                    if (ProcessDirectory(path, true)) {
                        // Add new directories to the cache
                        if (!directoriesBelowCurrentDirectory.SequenceEqual(cachedDirectoriesBelowCurrentDirectory)) {
                            foreach (string directory in directoriesBelowCurrentDirectory.Except(cachedDirectoriesBelowCurrentDirectory)) {
                                AddCacheDependencies(directory, true);
                            }
                        }

                        // Add the current directory back into the cache
                        InsertItemIntoCache(path, directoriesBelowCurrentDirectory);

                        // Rebuild subdirectories without a settings file if they inherit from this directory
                        if (File.Exists(Path.Combine(path, SettingsFileName))) {
                            foreach (string subDirectory in directoriesBelowCurrentDirectory) {
                                if (!File.Exists(Path.Combine(subDirectory, SettingsFileName))) {
                                    HttpRuntime.Cache.Remove(subDirectory);
                                }
                            }
                        }
                    }
                    break;

                // Cache items will only be manually removed if they have to be rebuilt due to changes in a directory that they inherit settings from
                case CacheItemRemovedReason.Removed:
                    if (ProcessDirectory(path, false)) {
                        InsertItemIntoCache(path, directoriesBelowCurrentDirectory);

                        foreach (string subDirectory in directoriesBelowCurrentDirectory) {
                            if (!File.Exists(Path.Combine(subDirectory, SettingsFileName))) {
                                HttpRuntime.Cache.Remove(subDirectory);
                            }
                        }
                    }
                    break;

                case CacheItemRemovedReason.Expired:
                case CacheItemRemovedReason.Underused:
                    // Don't need to reprocess parameters, just re-insert the item into the cache
                    HttpRuntime.Cache.Insert(key, value, new CacheDependency(path), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, RebuildFromCacheHit);
                    break;

                default:
                    break;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Keeping the cache dependency active.")]
        private static void InsertItemIntoCache(string path, IEnumerable<string> directoriesBelowCurrentDirectory) {
            string key = Guid.NewGuid().ToString();
            var value = Tuple.Create(path, directoriesBelowCurrentDirectory);
            HttpRuntime.Cache.Insert(key, value, new CacheDependency(path), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, RebuildFromCacheHit);
        }

        private static void MergeExternalCss(string path, TextWriter cssHighCompatOutput, TextWriter cssLowCompatOutput) {
            string[] extraCssFiles = Directory.GetFiles(path, "*.css");

            foreach (string cssFile in extraCssFiles) {
                if (cssFile.Contains(HighCompatibilityCssFileName) || cssFile.Contains(LowCompatibilityCssFileName)) {
                    continue;
                }

                using (TextReader cssRead = new StreamReader(cssFile)) {
                    string textToBeCopied = cssRead.ReadToEnd();

                    cssHighCompatOutput.Write(textToBeCopied);
                    cssLowCompatOutput.Write(textToBeCopied);
                }
            }
        }

        private static void SaveFileModificationData(string path) {
            using (TextWriter timeStamp = new StreamWriter(GetTimeStampFile(path))) {
                timeStamp.Write(GetCurrentTimeStampInfo(path));
            }
        }

        /// <summary>
        /// Reads the timestamps of all of the files within a directory, and outputs them in a single sorted string. Used to determine if changes have occured to a directory upon application start.
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <returns>A sorted string containing all filenames and last modified timestamps</returns>
        private static string GetCurrentTimeStampInfo(string path) {
            List<string> fileLocations = Directory.GetFiles(path).ToList();

            // Remove the timestamp file, since it can't be included in the comparison
            string timeStampFile = GetTimeStampFile(path);
            fileLocations.Remove(timeStampFile);
            fileLocations.Sort();

            StringBuilder timeStampBuilder = new StringBuilder();

            foreach (string file in fileLocations) {
                string name = Path.GetFileName(file);
                DateTime lastModificationTime = File.GetLastWriteTimeUtc(file);

                timeStampBuilder.Append(name).Append(lastModificationTime);
            }

            return timeStampBuilder.ToString();
        }

        private static string GetSavedTimeStampInfo(string path) {
            try {
                using (TextReader timeStamp = new StreamReader(GetTimeStampFile(path))) {
                    return timeStamp.ReadToEnd();
                }
            }
            // In the case of an exception, regenerate all sprites
            catch (FileNotFoundException) {
                return null;
            }
        }

        private static string GetTimeStampFile(string path) {
            return Path.Combine(path, TimestampFileName);
        }

        private static bool HaveFilesBeenModified(string path) {
            return GetCurrentTimeStampInfo(path) != GetSavedTimeStampInfo(path);
        }

        /// <summary>
        /// Checks if the image at the path is a sprite generated by the framework, and deletes it if it was
        /// </summary>
        /// <param name="path">The file path to the image in question</param>
        /// <returns>True if the image was a sprite (and was by extension, deleted)</returns>
        private static bool DeleteSpriteFile(string path) {
            if (IsOutputSprite(path)) {
                File.Delete(path);
                return true;
            }

            return false;
        }

        private static void PerformOptimizations(string path, ImageSettings settings, TextWriter cssHighCompatOutput, TextWriter cssLowCompatOutput, List<string> imageLocations) {
            // Create a list containing each image (in Bitmap format), and calculate the total size (in pixels) of final image
            int x = 0;
            int y = 0;
            int imageIndex = 0;
            long size = 0;
            int spriteNumber = 0;
            List<Bitmap> images = new List<Bitmap>();

            try {
                foreach (string imagePath in imageLocations) {
                    // If the image is growing above the specified max file size, make the sprite with the existing images
                    // and add the new image to the next sprite list
                    if ((imageIndex > 0) && IsSpriteOversized(settings.MaxSize, size, imagePath)) {
                        GenerateSprite(path, settings, x, y, spriteNumber, images, cssHighCompatOutput, cssLowCompatOutput);

                        // Clear the existing images
                        foreach (Bitmap image in images) {
                            image.Dispose();
                        }

                        // Reset variables to initial values, and increment the spriteNumber
                        images.Clear();
                        x = 0;
                        y = 0;
                        imageIndex = 0;
                        size = 0;
                        spriteNumber++;
                    }

                    // Add the current image to the list of images that are to be processed
                    images.Add(new Bitmap(imagePath));

                    // Use the image tag to store its name
                    images[imageIndex].Tag = MakeCssSelector(imagePath, SpriteDirectoryRelativePath);

                    // Find the total pixel size of the sprite based on the tiling direction
                    if (settings.TileInYAxis) {
                        y += images[imageIndex].Height;
                        if (x < images[imageIndex].Width) {
                            x = images[imageIndex].Width;
                        }
                    }
                    else {
                        x += images[imageIndex].Width;
                        if (y < images[imageIndex].Height) {
                            y = images[imageIndex].Height;
                        }
                    }

                    // Update the filesize size of the bitmap list
                    size += (new FileInfo(imagePath)).Length;

                    imageIndex++;
                }

                // Merge the final list of bitmaps into a sprite
                if (imageIndex != 0) {
                    GenerateSprite(path, settings, x, y, spriteNumber, images, cssHighCompatOutput, cssLowCompatOutput);
                }
            }
            finally {
                // Close the CSS file and clear the images list
                foreach (Bitmap image in images) {
                    image.Dispose();
                }
            }
        }

        private static bool IsSpriteOversized(int maxSize, long spriteSize, string imagePath) {
            // Estimate the size of the sprite after adding the current image
            long nextSize = spriteSize + new FileInfo(imagePath).Length;

            // Determine of the size is too large
            return nextSize > (1024 * maxSize);
        }

        private static string MakePathRelative(string fullPath, string pathToRelativeRoot) {
            if (fullPath.IndexOf("~", StringComparison.OrdinalIgnoreCase) == 0) {
                fullPath = HostingEnvironment.MapPath(fullPath);
            }

            if (pathToRelativeRoot.IndexOf("~", StringComparison.OrdinalIgnoreCase) >= 0) {
                pathToRelativeRoot = HostingEnvironment.MapPath(pathToRelativeRoot);
            }

            fullPath = GetTrimmedPath(fullPath);
            pathToRelativeRoot = GetTrimmedPath(pathToRelativeRoot);

            if (fullPath.ToUpperInvariant().Contains(pathToRelativeRoot.ToUpperInvariant())) {
                return fullPath.Remove(0, fullPath.IndexOf(pathToRelativeRoot, StringComparison.OrdinalIgnoreCase) + pathToRelativeRoot.Length + 1);
            }
            else {
                return fullPath;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void GenerateSprite(string path, ImageSettings settings, int x, int y, int spriteNumber, List<Bitmap> images, TextWriter cssHighCompatOutput, TextWriter cssLowCompatOutput) {
            // Create a drawing surface and add the images to it
            // Since we'll be padding each image by 1px later on, we need to increase the sprites's size correspondingly.
            if (settings.TileInYAxis) {
                y += images.Count;
            }
            else {
                x += images.Count;
            }

            using (Bitmap sprite = new Bitmap(x, y)) {
                using (Graphics drawingSurface = Graphics.FromImage(sprite)) {

                    // Set the background to the specs from the settings file
                    drawingSurface.Clear(settings.BackgroundColor);

                    // Make the final sprite and save it
                    int xOffset = 0;
                    int yOffset = 0;
                    foreach (Bitmap image in images) {
                        drawingSurface.DrawImage(image, new Rectangle(xOffset, yOffset, image.Width, image.Height));

                        // Add the CSS data
                        GenerateCss(xOffset, yOffset, spriteNumber, settings.Format, settings.Base64, image, cssHighCompatOutput);
                        GenerateCss(xOffset, yOffset, spriteNumber, settings.Format, false, image, cssLowCompatOutput);

                        if (settings.TileInYAxis) {
                            // pad each image in the sprite with a 1px margin
                            yOffset += image.Height + 1;
                        }
                        else {
                            // pad each image in the sprite with a 1px margin
                            xOffset += image.Width + 1;
                        }
                    }

                    // Set the encoder parameters and make the image
                    try {
                        using (EncoderParameters spriteEncoderParameters = new EncoderParameters(1)) {
                            spriteEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, settings.Quality);

                            // Attempt to save the image to disk with the specified encoder
                            sprite.Save(Path.Combine(path, GenerateSpriteFileName(spriteNumber, settings.Format)), GetEncoderInfo(settings.Format), spriteEncoderParameters);
                        }
                    }
                    catch (Exception) {
                        // If errors occur, get the CLI to auto-choose an encoder. Unfortunately this means that the quality settings will be not used.
                        try {
                            sprite.Save(Path.Combine(path, GenerateSpriteFileName(spriteNumber, settings.Format)));
                        }
                        catch (Exception) {
                            // If errors occur again, try to save as a png
                            sprite.Save(Path.Combine(path, GenerateSpriteFileName(spriteNumber, "png")));
                        }
                    }
                }
            }
        }

        private static string GenerateSpriteFileName(int spriteNumber, string fileExtension) {
            return String.Format(CultureInfo.InvariantCulture, GeneratedSpriteFileName, spriteNumber) + "." + fileExtension;
        }

        private static void GenerateCss(int xOffset, int yOffset, int spriteNumber, string fileExtension, bool base64, Bitmap image, TextWriter cssOutput) {
            cssOutput.WriteLine("." + (string)image.Tag);
            cssOutput.WriteLine("{");
            cssOutput.WriteLine("width:" + image.Width.ToString(CultureInfo.InvariantCulture) + "px !important;");
            cssOutput.WriteLine("height:" + image.Height.ToString(CultureInfo.InvariantCulture) + "px !important;");

            if (base64) {
                string base64Image = ConvertImageToBase64(image, GetImageFormat(fileExtension));
                if (SupportIE8MaximalInlineSize && base64Image.Length > IE8MaximalInlineSize) {
                    GenerateCssBackgroundLow(cssOutput, fileExtension, spriteNumber, xOffset, yOffset);
                }
                else {
                    GenerateCssBackgroundHigh(cssOutput, fileExtension, base64Image);
                }
            }
            else {
                GenerateCssBackgroundLow(cssOutput, fileExtension, spriteNumber, xOffset, yOffset);
            }

            cssOutput.WriteLine("}");
        }

        private static void GenerateCssBackgroundHigh(TextWriter cssOutput, string fileExtension, string base64Image) {
            cssOutput.WriteLine("background:url(data:image/" + fileExtension + ";base64," + base64Image + ") no-repeat 0% 0% !important;");//added by moravej
        }

        private static string GetOffsetPosition(int offset) {
            // Offset of 0 doesn't need to have the minus sign
            if (offset == 0) {
                return offset.ToString(CultureInfo.InvariantCulture);
            }
            else {
                return "-" + offset.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static void GenerateCssBackgroundLow(TextWriter cssOutput, string fileExtension, int spriteNumber, int xOffset, int yOffset) {
            string xPosition = GetOffsetPosition(xOffset);
            string yPosition = GetOffsetPosition(yOffset);

            cssOutput.WriteLine("background-image:url(sprite" + spriteNumber.ToString(CultureInfo.InvariantCulture) + "." + fileExtension + ") !important;");//added by morave
            cssOutput.WriteLine("background-position:" + xPosition + "px " + yPosition + "px !important;");//added by moravej
        }

        private static string ConvertImageToBase64(Bitmap image, ImageFormat format) {
            string base64;
            using (MemoryStream memory = new MemoryStream()) {
                image.Save(memory, format);
                base64 = Convert.ToBase64String(memory.ToArray());
            }

            return base64;
        }

        private static ImageFormat GetImageFormat(string fileExtension) {
            switch (fileExtension.ToUpperInvariant()) {
                case "JPG":
                case "JPEG":
                    return ImageFormat.Jpeg;

                case "GIF":
                    return ImageFormat.Gif;

                case "PNG":
                    return ImageFormat.Png;

                case "BMP":
                    return ImageFormat.Bmp;

                default:
                    return ImageFormat.Png;
            }
        }

        private static string LocateSettingFile(string path) {
            string rootPath = GetTrimmedPath(HostingEnvironment.MapPath(SpriteDirectoryRelativePath));

            DirectoryInfo pathInfo = new DirectoryInfo(GetTrimmedPath(path));
            try {
                string file;
                do {
                    file = Path.Combine(pathInfo.FullName, SettingsFileName);
                    if (File.Exists(file)) {
                        return file;
                    }
                }
                while ((!pathInfo.FullName.Equals(rootPath, StringComparison.OrdinalIgnoreCase)) && ((pathInfo = Directory.GetParent(path)) != null));
            }
            catch (SecurityException) {
                // We went too high in the hierarchy, simply return null
            }

            return null;
        }

        private static ImageSettings GetSettings(string path) {
            ImageSettings settings = new ImageSettings();

            string settingFileLocation = LocateSettingFile(path);
            if (settingFileLocation != null) {
                XmlTextReader settingsData;

                // Open the settings file. If it fails here, we throw an exception since we expect the file to be there and readable.
                using (settingsData = new XmlTextReader(settingFileLocation)) {
                    while (settingsData.Read()) {
                        if (settingsData.NodeType == XmlNodeType.Element) {
                            string nodeName = settingsData.Name;

                            if (nodeName.Equals("FileFormat", StringComparison.OrdinalIgnoreCase)) {
                                settings.Format = settingsData.ReadElementContentAsString().Trim('.');
                            }
                            else if (nodeName.Equals("Quality", StringComparison.OrdinalIgnoreCase)) {
                                settings.Quality = settingsData.ReadElementContentAsInt();
                            }
                            else if (nodeName.Equals("MaxSize", StringComparison.OrdinalIgnoreCase)) {
                                settings.MaxSize = settingsData.ReadElementContentAsInt();
                            }
                            else if (nodeName.Equals("BackgroundColor", StringComparison.OrdinalIgnoreCase)) {
                                string output = settingsData.ReadElementContentAsString();
                                int temp = Int32.Parse(output, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                settings.BackgroundColor = Color.FromArgb(temp);
                            }
                            else if (nodeName.Equals("Base64Encoding", StringComparison.OrdinalIgnoreCase)) {
                                settings.Base64 = settingsData.ReadElementContentAsBoolean();
                            }
                            else if (nodeName.Equals("TileInYAxis", StringComparison.OrdinalIgnoreCase)) {
                                settings.TileInYAxis = settingsData.ReadElementContentAsBoolean();
                            }
                        }
                    }
                }

                return settings;
            }

            return settings;
        }

        private static string GetTrimmedPath(string path) {
            return path
                .TrimEnd(Path.AltDirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar);
        }

        private static ImageCodecInfo GetEncoderInfo(string format) {
            format = format.ToUpperInvariant();

            // Find the appropriate codec for the specified file extension
            if (format == "JPG") {
                format = "JPEG";
            }

            format = "IMAGE/" + format;
            // Get a list of all the available encoders
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            // Search the list for the proper encoder
            foreach (ImageCodecInfo encoder in encoders) {
                if (encoder.MimeType.ToUpperInvariant() == format) {
                    return encoder;
                }
            }

            // If a format cannot be found, throw an exception
            throw new FormatException("Encoder not found! The CLI will attempt to automatically choose an encoder, however image quality settings will be ignored");
        }
    }
}