﻿using SignalGo.Shared.Models;
using System;

namespace SignalGo.Shared.Events
{
    public delegate void BeginHttpCallAction(object clientInfo, string callGuid, string address, System.Reflection.MethodInfo method, SignalGo.Shared.Models.ParameterInfo[] values);
    public delegate void EndHttpCallAction(object clientInfo, string callGuid, string address, System.Reflection.MethodInfo method, SignalGo.Shared.Models.ParameterInfo[] values, object result, Exception exception);

    public delegate void BeginMethodCallAction(object clientInfo, string callGuid, string serviceName, System.Reflection.MethodInfo method, ParameterInfo[] values);
    public delegate void EndMethodCallAction(object clientInfo, string callGuid, string serviceName, System.Reflection.MethodInfo method, ParameterInfo[] values, string result, Exception exception);

    public delegate void BeginClientMethodCallAction(object clientInfo, string callGuid, string serviceName, string methodName, ParameterInfo[] values);
    public delegate void EndClientMethodCallAction(object clientInfo, string callGuid, string serviceName, string methodName, object[] values, string result, Exception exception);

    public delegate void BeginStreamCallAction(object clientInfo, string callGuid, string serviceName, string methodName, ParameterInfo[] values);
    public delegate void EndStreamCallAction(object clientInfo, string callGuid, string serviceName, string methodName, ParameterInfo[] values, string result, Exception exception);

    public static class MethodsCallHandler
    {
        public static BeginHttpCallAction BeginHttpMethodCallAction;
        public static EndHttpCallAction EndHttpMethodCallAction;

        public static BeginMethodCallAction BeginMethodCallAction;
        public static EndMethodCallAction EndMethodCallAction;

        public static BeginClientMethodCallAction BeginClientMethodCallAction;
        public static EndClientMethodCallAction EndClientMethodCallAction;

        public static BeginStreamCallAction BeginStreamCallAction;
        public static EndStreamCallAction EndStreamCallAction;
    }
}
