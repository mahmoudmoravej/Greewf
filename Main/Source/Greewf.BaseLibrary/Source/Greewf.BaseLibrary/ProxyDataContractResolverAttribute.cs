/// THis comes from http://blogs.msdn.com/b/adonet/archive/2010/01/05/poco-proxies-part-2-serializing-poco-proxies.aspx
/// We need this for OperationContract methods when return EF POCO classes .
/// sample : 
/// [ServiceContract]
/// public interface IServices
/// {
///    [OperationContract]
///    [ProxyDataContractResolver]
///    Invoice GetInvoiceInfo(int invoiceNo);
/// }
/// OR YOU SHOULD SET : "context.Configuration.ProxyCreationEnabled = false;" for EF Context
///


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Data.Objects;

namespace Greewf.BaseLibrary
{
    public class ProxyDataContractResolverAttribute:  Attribute, IOperationBehavior
    {
        public void AddBindingParameters(
                 OperationDescription description,
                 BindingParameterCollection parameters)
        {
        }

        public void ApplyClientBehavior(
                      OperationDescription description,
                      ClientOperation proxy)
        {
            DataContractSerializerOperationBehavior
               dataContractSerializerOperationBehavior =
                  description.Behaviors.Find<DataContractSerializerOperationBehavior>();
            dataContractSerializerOperationBehavior.DataContractResolver =
               new ProxyDataContractResolver();
        }

        public void ApplyDispatchBehavior(
                       OperationDescription description,
                       DispatchOperation dispatch)
        {
            DataContractSerializerOperationBehavior
               dataContractSerializerOperationBehavior =
                  description.Behaviors.Find<DataContractSerializerOperationBehavior>();
            dataContractSerializerOperationBehavior.DataContractResolver =
               new ProxyDataContractResolver();
        }

        public void Validate(OperationDescription description)
        {
        }
    }
}
