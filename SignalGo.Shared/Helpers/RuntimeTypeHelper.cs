﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// helper of types
    /// </summary>
    public static class RuntimeTypeHelper
    {
        /// <summary>
        /// check a type is nullable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableValueType(this Type type)
        {
#if (NETSTANDARD1_6)
            return type.GetGenericTypeDefinition() == typeof(Nullable<>);
#else
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
#endif
        }

        /// <summary>
        /// return types of method parameter
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="callInfo"></param>
        /// <returns></returns>
        public static List<Type> GetMethodTypes(Type serviceType, string methodName, string[] parameters)
        {
            List<Type> methodParameterTypes = new List<Type>();
#if (NETSTANDARD)
            var methods = serviceType.GetTypeInfo().GetMethods();
#else
            IEnumerable<MethodInfo> methods = serviceType.GetListOfMethods();
#endif
            //int sLen = streamType == null ? 0 : 1;
            foreach (MethodInfo item in methods)
            {
                if (item.Name == methodName)
                {
                    int plength = item.GetParameters().Length;
                    if (plength != parameters.Length)
                        continue;
                    //foreach (var p in parameters)
                    //{
                    //    methodParameterTypes.Add(p.ParameterType);
                    //}
                    break;
                }
            }
            return methodParameterTypes;
        }

        /// <summary>
        /// get full types of one type that types is in properteis
        /// </summary>
        /// <param name="type">your type</param>
        /// <param name="findedTypes">list of types you want</param>
        public static void GetListOfUsedTypes(Type type, ref List<Type> findedTypes)
        {
            if (!findedTypes.Contains(type))
                findedTypes.Add(type);
            else
                return;
            if (type.GetIsGenericType())
            {
                foreach (Type item in type.GetListOfGenericArguments())
                {
                    GetListOfUsedTypes(item, ref findedTypes);
                }
            }
            else
            {
                foreach (PropertyInfo item in type.GetListOfProperties())
                {
                    GetListOfUsedTypes(item.PropertyType, ref findedTypes);
                }
            }

            foreach (PropertyInfo item in type.GetListOfProperties())
            {
                GetListOfUsedTypes(item.PropertyType, ref findedTypes);
            }
        }
        /// <summary>
        /// Get friendly name of type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetFriendlyName(this Type type)
        {
            if (type == typeof(int))
                return "int";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(bool))
                return "bool";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(string))
                return "string";
            else if (type == typeof(Task))
            {
                return "void";
            }
            else if (type.GetBaseType() == typeof(Task))
            {
                return GetFriendlyName(type.GetGenericArguments()[0]);
            }
            else if (type.GetIsGenericType())
                return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetListOfGenericArguments().Select(x => GetFriendlyName(x)).ToArray()) + ">";
            else
                return type.Name;
        }
    }
}
