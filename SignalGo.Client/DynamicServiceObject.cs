﻿#if (!NET35)
using System;
using System.Collections.Generic;
using System.Dynamic;
using SignalGo.Client;
using System.Linq;
using System.Text;
using System.Reflection;
using SignalGo.Shared.Helpers;

namespace SignalGo.Client
{
    /// <summary>
    /// helper of dynamic calls
    /// </summary>
    internal class DynamicServiceObject : DynamicObject
    {
        /// <summary>
        /// service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// connector to call methods
        /// </summary>
        public ConnectorBase Connector { get; set; }

        /// <summary>
        /// types can return of method names
        /// </summary>
        public Dictionary<string, Type> ReturnTypes = new Dictionary<string, Type>();

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var type = ReturnTypes[binder.Name];
            if (type == typeof(void))
            {
                ConnectorExtension.SendData(Connector, ServiceName, binder.Name, args);
                result = null;
            }
            else
            {
                var data = ConnectorExtension.SendData(Connector, ServiceName, binder.Name, args).ToString();
                result = Newtonsoft.Json.JsonConvert.DeserializeObject(data, type);
            }
            return true;
        }

        /// <summary>
        /// initialize type to returnTypes
        /// </summary>
        /// <param name="type"></param>
        public void InitializeInterface(Type type)
        {
            var items = type.GetListOfMethods();
            foreach (var item in items)
            {
                ReturnTypes.Add(item.Name, item.ReturnType);
            }
        }
    }


    /// <summary>
    /// helper of dynamic calls
    /// </summary>
    internal class DynamicServiceObjectWitoutInterface : DynamicObject
    {
        /// <summary>
        /// service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// connector to call methods
        /// </summary>
        public ConnectorBase Connector { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = ConnectorExtension.SendData(Connector, ServiceName, binder.Name, args);
            return true;
        }
    }
}
#endif
