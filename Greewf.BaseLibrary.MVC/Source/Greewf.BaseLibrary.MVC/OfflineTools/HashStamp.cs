// BY  :  http://deanhume.com/Home/BlogPost/mvc-and-the-html5-application-cache/59 
// AND :  http://twitter.com/#!/ShirtlessKirk

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace Greewf.BaseLibrary.MVC.OfflineTools
{


    public static class Utilities
    {
        private static readonly MD5CryptoServiceProvider Md5 = new MD5CryptoServiceProvider();
        private static readonly Dictionary<string, Guid> FileHash = new Dictionary<string, Guid>();

        /// <summary>
        /// Appends the hash of the file as a querystring parameter to a supplied string.
        /// </summary>
        /// <param name="fname">The filename.</param>
        /// <param name="request">The current HttpRequest.</param>
        /// <returns>String with hash of the file appended.</returns>
        public static string AppendHash(this string fname, HttpRequest request, string hashFileLocation = null)
        {
            return String.Format(@"{0}{1}hash={2}", fname, fname.Contains("?") ? "&" : "?", GetFileHash(hashFileLocation ?? fname, request));
        }

        /// <summary>
        /// Returns a hash of the supplied file.
        /// </summary>
        /// <param name="fname">The name of the file.</param>
        /// <param name="request">The current HttpRequest.</param>
        /// <returns>A Guid representing the hash of the file.</returns>
        public static Guid GetFileHash(string fname, HttpRequest request)
        {
            Guid hash;
            var localPath = request.RequestContext.HttpContext.Server.MapPath(fname.Replace('/', '\\'));

            using (var ms = new MemoryStream())
            {
                using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    StreamCopy(fs, ms);
                }

                hash = new Guid(Md5.ComputeHash(ms.ToArray()));
                Guid check;
                if (!FileHash.TryGetValue(localPath, out check))
                {
                    FileHash.Add(localPath, hash);
                }
                else if (check != hash)
                {
                    FileHash[localPath] = hash;
                }
            }

            return hash;
        }

        /// <summary>
        /// Copies from one Stream to another.
        /// </summary>
        /// <param name="from">The Stream to copy from.</param>
        /// <param name="to">The Stream to copy to.</param>
        public static void StreamCopy(Stream from, Stream to)
        {
            if (from == to)
            {
                return;
            }

            var buffer = new byte[4096];

            from.Seek(0, SeekOrigin.Begin);

            while (true)
            {
                var done = from.Read(buffer, 0, 4096);

                if (done <= 0)
                {
                    return;
                }

                to.Write(buffer, 0, done);
            }
        }
    }
}
