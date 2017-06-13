using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Practices.Unity.InterceptionExtension;
using Newtonsoft.Json;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;

namespace ProfileSample.Business
{
    public class LoggingInterceptor : IInterceptionBehavior
    {
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var message = $"{DateTime.Now} Invoked {input.MethodBase} with arguments {string.Join(", ", GetArguments(input.Arguments).Select(kvp => $"{kvp.Key} = {kvp.Value}"))}";
            Log(message);

            var result = getNext()(input, getNext);

            message = $"{DateTime.Now} {input.MethodBase} returned {result.ReturnValue}";
            Log(message);

            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute => true;

        private void Log(string message)
        {
            Trace.TraceInformation(message);
        }

        private Dictionary<string, object> GetArguments(IParameterCollection parameterCollection)
        {
            var result = new Dictionary<string, object>();

            for (int i = 0; i < parameterCollection.Count; i++)
                result.Add(parameterCollection.ParameterName(i), parameterCollection[i]);

            return result;
        }
    }

    [Serializable]
    public class LoggingAspect : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            var message = $"{DateTime.Now} Invoked {args.Method} with arguments {string.Join(", ", GetArguments(args).Select(kvp => $"{kvp.Key} = {kvp.Value}"))}";
            Log(message);
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            var message = $"{DateTime.Now} {args.Method} returned {args.ReturnValue}";
            Log(message);
        }

        private void Log(string message)
        {
            Trace.TraceInformation(message);
        }

        private Dictionary<string, object> GetArguments(MethodExecutionArgs args)
        {
            var result = new Dictionary<string, object>();

            var parameters = args.Method.GetParameters();
            
            for (int i = 0; i < args.Arguments.Count; i++)
                result.Add(parameters[i].Name, Serialize(args.Arguments[i]));

            return result;
        }

        private string Serialize(object o)
        {
            var serializer = new XmlSerializer(o.GetType());
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, o);
                return writer.ToString();
            }
        }
    }
}