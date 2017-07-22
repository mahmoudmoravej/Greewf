using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using Greewf.BaseLibrary.Logging;

//NOTE : We should put it to different namespace to prevent ambiguous error with other extended functions from another namespace
namespace Greewf.BaseLibrary.MVC.Logging
{
    public static class LoggerExtensions
    {

        public static long Log<T>(this LoggerBase logger, T logId, object model = null, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null) where T : struct
        {
            var typ = typeof(T);
            return logger.Log((int)Convert.ChangeType(logId, typ), typ, model, modelMetadata, exludeModelProperties);
        }


        public static long Log<T>(this LoggerBase logger, T logId, object model, string[] exludeModelProperties) where T : struct
        {
            var typ = typeof(T);
            return logger.Log((int)Convert.ChangeType(logId, typ), typ, model, exludeModelProperties);
        }

        public static long Log(this LoggerBase logger, int logId, Type logEnumType, object model, string[] exludeModelProperties = null)
        {
            var metaData = ModelMetadataProviders.Current.GetMetadataForType(() => { return model; }, model.GetType());
            return logger.Log(logId, logEnumType, model, metaData, exludeModelProperties);
        }

        public static long Log(this LoggerBase logger, int logId, Type logEnumType, object model = null, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null)
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
