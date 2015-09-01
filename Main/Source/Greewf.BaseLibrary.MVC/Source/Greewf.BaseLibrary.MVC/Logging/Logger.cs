using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Greewf.BaseLibrary.Logging.LogContext;
using System.Web;
using System.Web.Mvc;
using System.Collections;

namespace Greewf.BaseLibrary.Logging
{
    public static class LoggerExtensions
    {

        public static long Log<T>(this Logger logger, T logId, object model = null, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null) where T : struct
        {
            var typ = typeof(T);
            return logger.Log((int)Convert.ChangeType(logId, typ), typ, model, modelMetadata, exludeModelProperties);
        }


        public static long Log<T>(this Logger logger, T logId, object model, string[] exludeModelProperties) where T : struct
        {
            var typ = typeof(T);
            return logger.Log((int)Convert.ChangeType(logId, typ), typ, model, exludeModelProperties);
        }

        public static long Log(this Logger logger, int logId, Type logEnumType, object model, string[] exludeModelProperties = null)
        {
            var metaData = ModelMetadataProviders.Current.GetMetadataForType(() => { return model; }, model.GetType());
            return logger.Log(logId, logEnumType, model, metaData, exludeModelProperties);
        }

        public static long Log(this Logger logger, int logId, Type logEnumType, object model = null, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null)
        {
            Dictionary<string, string> modelDisplayNames = null;
            if (model != null)
            {
                var typ = model.GetType();
                if (modelMetadata == null)
                    modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => { return model; }, typ);

                modelDisplayNames = modelMetadata.Properties.Select(o => new { o.PropertyName, o.DisplayName }).ToDictionary(o => o.PropertyName, o => o.DisplayName);

            }

            return logger.Log(logId, logEnumType, model, modelDisplayNames, exludeModelProperties);

        }




    }




}
