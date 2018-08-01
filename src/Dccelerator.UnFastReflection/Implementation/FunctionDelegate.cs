using System;
using System.Diagnostics;
using System.Reflection;


namespace Dccelerator.UnFastReflection
{
    public class FunctionDelegate<TContext, TOut> : DelegateContainer<Func<TContext, TOut>>, IFunctionDelegate<TContext, TOut> {
        public FunctionDelegate(MethodInfo method) : base(method) {}

        public bool TryInvoke(TContext context, out TOut result) {
            try {
                result = Delegate(context);
                return true;
            }
            catch (Exception e) { //todo: add logging
                Log.TraceEvent(TraceEventType.Error, e.ToString());
                result = default(TOut);
                return false;
            }
        }
    }




    public class FunctionDelegate<TContext, TP1, TOut> : DelegateContainer<Func<TContext, TP1, TOut>>, IFunctionDelegate<TContext, TP1, TOut>
    {
        public FunctionDelegate(MethodInfo method) : base(method) {}


        public bool TryInvoke(TContext context, TP1 p1, out TOut result) {
            try {
                result = Delegate(context, p1);
                return true;
            }
            catch (Exception e) { //todo: add logging
                Log.TraceEvent(TraceEventType.Error, e.ToString());
                result = default(TOut);
                return false;
            }
        }
    }



    public class FunctionDelegate<TContext, TP1, TP2, TOut> : DelegateContainer<Func<TContext, TP1, TP2, TOut>>, IFunctionDelegate<TContext, TP1, TP2, TOut> {
        public FunctionDelegate(MethodInfo method) : base(method) {}


        public bool TryInvoke(TContext context, TP1 p1, TP2 p2, out TOut result) {
            try {
                result = Delegate(context, p1, p2);
                return true;
            }
            catch (Exception e) { //todo: add logging
                Log.TraceEvent(TraceEventType.Error, e.ToString());
                result = default(TOut);
                return false;
            }
        }
    }


}