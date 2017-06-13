using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.Practices.Unity.InterceptionExtension;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;

namespace ProfileSample.Business
{
    public class LoggingAdvice : IInterceptionBehavior
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
            Debug.WriteLine(message);
        }

        private Dictionary<string, object> GetArguments(IParameterCollection parameterCollection)
        {
            var result = new Dictionary<string, object>();

            for (int i = 0; i < parameterCollection.Count; i++)
                result.Add(parameterCollection.ParameterName(i), parameterCollection[i]);

            return result;
        }
    }

    public class PostSharpLoggingAspect : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            Debug.WriteLine($"Invoked {args.Method.Name}");
        }
    }
}