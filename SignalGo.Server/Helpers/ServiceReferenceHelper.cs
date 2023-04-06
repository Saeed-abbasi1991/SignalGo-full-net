﻿using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using SignalGo.Shared.Models.ServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.Helpers
{
    /// <summary>
    /// when client side want ot add service reference of signalgo
    /// </summary>
    public class ServiceReferenceHelper
    {
        public static Func<Type, MethodInfo, string> GetCustomClientSideMethodNameFunction { get; set; }
        public bool IsRenameDuplicateMethodNames { get; set; }
        public ClientServiceReferenceConfigInfo ClientServiceReferenceConfigInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ServiceReferenceHelper()
        {
            GenerateBase();
        }

        private NamespaceReferenceInfo NamespaceReferenceInfo { get; set; } = new NamespaceReferenceInfo();

        private List<Assembly> ModellingReferencesAssemblies { get; set; }

        /// <summary>
        /// Type adds to this list after generated so a duplicate generate is not needed
        /// </summary>
        private List<Type> ModelsCodeGenerated { get; set; } = new List<Type>();

        /// <summary>
        /// This types must be generated. After generate this will remove from list
        /// </summary>
        private List<Type> TypeToGenerate { get; set; } = new List<Type>();

        private string BaseAssemblyPath { get; set; }

        private void GenerateBase()
        {
            BaseAssemblyPath = Environment.GetEnvironmentVariable("SystemRoot");
            if (string.IsNullOrEmpty(BaseAssemblyPath))
            {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                var assm = typeof(string).GetTypeInfo().Assembly;
#else
                Assembly assm = typeof(string).Assembly;
#endif
                string codeBase = assm.CodeBase;
                if (codeBase.Length < 32)
                    BaseAssemblyPath = codeBase;
                else
                    BaseAssemblyPath = codeBase.Substring(0, 32);
            }
            else
            {
                BaseAssemblyPath = "file:///" + System.IO.Path.Combine(BaseAssemblyPath, "Microsoft.Net").Replace("\\", "/").ToLower();
            }
        }

        internal bool CannotGenerateAssemblyTypes(Type type)
        {
            if (type == typeof(Task))
            {
                return false;
            }
            else if (type.GetBaseType() == typeof(Task))
            {
                type = type.GetGenericArguments()[0];
            }

#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var assm = type.GetTypeInfo().Assembly;
#else
            Assembly assm = type.Assembly;
#endif
            var fileName = System.IO.Path.GetFileName(assm.Location);
            if (ClientServiceReferenceConfigInfo != null && ClientServiceReferenceConfigInfo.SkipAssemblies != null)
            {
                if (ClientServiceReferenceConfigInfo.SkipAssemblies.Any(x => x.ToLower() == fileName.ToLower()))
                    return true;
            }
            if (ModellingReferencesAssemblies.Contains(type.GetAssembly()) || assm.CodeBase.ToLower() == BaseAssemblyPath || assm.CodeBase.ToLower().StartsWith(BaseAssemblyPath))
                return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="serverBase"></param>
        /// <returns></returns>
        public NamespaceReferenceInfo GetServiceReferenceCSharpCode(string nameSpace, ServerBase serverBase, string httpServiceName, string serviceMethodName)
        {
            serviceMethodName = serviceMethodName.ToLower().Trim();
            NamespaceReferenceInfo.Name = nameSpace;
            ModellingReferencesAssemblies = serverBase.ModellingReferencesAssemblies;
            AddUsingIfNotExist(typeof(ServiceContractAttribute));
            AddUsingIfNotExist(typeof(System.Threading.Tasks.Task));
            AddUsingIfNotExist(typeof(NotifyPropertyChangedBase));
            ModellingReferencesAssemblies.Add(typeof(NotifyPropertyChangedBase).GetAssembly());
            ModellingReferencesAssemblies.Add(typeof(ServerBase).GetAssembly());
            List<string> generatedServices = new List<string>();

            //if the generation password is true and service methodname is password,then it will full generate everything
            bool getFull = false;
            bool wrongPassword = false;
            if (!string.IsNullOrEmpty(serverBase.CodeGeneratorPassword))
            {
                if (!serverBase.CodeGeneratorPassword.Equals(httpServiceName, StringComparison.OrdinalIgnoreCase))
                {
                    //when the user is gust that will not generate anything
                    if (!serverBase.RegisteredServiceTypes.Any(x =>
                    x.Value.GetCustomAttributes<GenerationServiceAttribute>(true).ToList().Any(g => g.Urls.Any(url => url.Equals(httpServiceName, StringComparison.OrdinalIgnoreCase)))
                    || x.Value.GetListOfMethods().Any(m => m.GetCustomAttributes<GenerationServiceAttribute>(true).Any(at => at.Urls.Any(url => url.Equals(httpServiceName, StringComparison.OrdinalIgnoreCase))))))
                    {
                        wrongPassword = true;
                    }
                }
                else
                {
                    getFull = true;
                }
            }
            if (!wrongPassword)
            {
                foreach (KeyValuePair<string, Type> serviceInfo in serverBase.RegisteredServiceTypes)
                {
                    if (!getFull)
                    {
                        //generate a service if that has service generation attribute
                        //if serviceMethodName != any urls of GenerationServiceAttribute that service will ignore to generate in service generation
                        var serviceServiceGenerationAttributes = serviceInfo.Value.GetCustomAttributes<GenerationServiceAttribute>(true).ToList();

                        var hasMethodServiceGenerationAttribute = serviceInfo.Value.GetListOfMethods().Count() == 0 || serviceInfo.Value.GetListOfMethods().Any(m => m.GetCustomAttributes<GenerationServiceAttribute>(true).Length == 0 || m.GetCustomAttributes<GenerationServiceAttribute>(true).Any(x => x.Urls.Any(url => url.Equals(httpServiceName, StringComparison.OrdinalIgnoreCase))));

                        if (serviceServiceGenerationAttributes.Count > 0 && !serviceServiceGenerationAttributes.Any(x => x.Urls.Any(url => url.Equals(httpServiceName, StringComparison.OrdinalIgnoreCase))) || !hasMethodServiceGenerationAttribute)
                        {
                            continue;
                        }
                    }


                    ServiceContractAttribute[] attributes = null;
                    if (serviceInfo.Value.HasServiceAttribute())
                        attributes = serviceInfo.Value.GetServiceContractAttributes();
                    else
                        attributes = new ServiceContractAttribute[0];

                    if (!generatedServices.Contains(serviceInfo.Key) && attributes.Any(x => x.ServiceType == ServiceType.ServerService && x.GetServiceName(false) == serviceInfo.Key))
                    {
                        generatedServices.Add(serviceInfo.Key);
                        GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.ServiceLevel, ServiceType.ServerService, httpServiceName, serviceMethodName, getFull);
                    }

                    if (!generatedServices.Contains(serviceInfo.Key) && attributes.Any(x => x.ServiceType == ServiceType.ClientService && ServiceContractExtensions.GetServiceNameWithGeneric(serviceInfo.Value, x.GetServiceName(false)) == serviceInfo.Key))
                    {
                        generatedServices.Add(serviceInfo.Key);
                        GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.CallbackLevel, ServiceType.ClientService, httpServiceName, serviceMethodName, getFull);
                    }

                    if (!generatedServices.Contains(serviceInfo.Key) && attributes.Any(x => x.ServiceType == ServiceType.StreamService && x.GetServiceName(false) == serviceInfo.Key))
                    {
                        generatedServices.Add(serviceInfo.Key);
                        GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.StreamLevel, ServiceType.StreamService, httpServiceName, serviceMethodName, getFull);
                    }

                    if (!generatedServices.Contains(serviceInfo.Key) && attributes.Any(x => x.ServiceType == ServiceType.OneWayService && x.GetServiceName(false) == serviceInfo.Key))
                    {
                        generatedServices.Add(serviceInfo.Key);
                        GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.OneWayLevel, ServiceType.OneWayService, httpServiceName, serviceMethodName, getFull);
                    }

                    if (!generatedServices.Contains(serviceInfo.Key) && (attributes.Any(x => x.ServiceType == ServiceType.HttpService && x.GetServiceName(false) == serviceInfo.Key) || attributes.Length == 0))
                    {
                        generatedServices.Add(serviceInfo.Key);
                        GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.HttpServiceLevel, ServiceType.HttpService, httpServiceName, serviceMethodName, getFull);
                    }
                }
            }
            //foreach (var serviceInfo in serverBase.RegisteredServiceTypes)
            //{
            //    GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.ServiceLevel, ServiceType.ServerService);
            //}

            //foreach (var serviceInfo in serverBase.RegisteredServiceTypes.Where(x=>x.Value))
            //{
            //    GenerateServiceClass(serviceInfo.Key, serviceInfo.Value, ClassReferenceType.CallbackLevel, ServiceType.ClientService);
            //}


            //foreach (var serviceInfo in serverBase.StreamServices)
            //{
            //    GenerateServiceClass(serviceInfo.Key, serviceInfo.Value.GetType(), ClassReferenceType.StreamLevel, ServiceType.StreamService);
            //}

            //foreach (var serviceInfo in serverBase.RegisteredHttpServiceTypes)
            //{
            //    GenerateHttpServiceClass(serviceInfo.Key, serviceInfo.Value);
            //}

            //foreach (var serviceInfo in serverBase.OneWayServices)
            //{
            //    GenerateServiceClass(serviceInfo.Key, serviceInfo.Value.GetType(), ClassReferenceType.OneWayLevel, ServiceType.OneWayService);
            //}

            Type find = null;
            while ((find = TypeToGenerate.FirstOrDefault()) != null)
            {
                if (find.GetIsClass())
                    GenerateModelClass(find);
                else if (find.GetIsInterface())
                    GenerateModelInterface(find);
                else if (find.GetIsEnum())
                    GenerateModelEnum(find);

                TypeToGenerate.Remove(find);
            }

            return NamespaceReferenceInfo;
        }

        private void AddToGenerate(Type type)
        {
            if (!TypeToGenerate.Contains(type) && !CannotGenerateAssemblyTypes(type) && !type.IsGenericParameter)
                TypeToGenerate.Add(type);
        }

        private List<Type> typeGenerated = new List<Type>();
        private List<MethodInfo> methodGenerated = new List<MethodInfo>();

        private void GenerateServiceClass(string serviceName, Type type, ClassReferenceType classReferenceType, ServiceType serviceTypeEnum, string httpServiceName, string serviceMethodName, bool getFull)
        {
            string typeName = type.Name.Split('`')[0];
            if (classReferenceType == ClassReferenceType.CallbackLevel && type.GetIsGenericType())
            {
                typeName += string.Join("", type.GetListOfGenericArguments().Select(x => x.Name).ToArray());
            }
            ClassReferenceInfo classReferenceInfo = new ClassReferenceInfo
            {
                ServiceName = serviceName,
                Name = typeName,
                Type = classReferenceType,
                NameSpace = type.GetNamesapce()
            };
            List<Type> services = type.GetTypesByAttribute<ServiceContractAttribute>(x => x.ServiceType == serviceTypeEnum).ToList();
            bool justDeclared = false;
            if (serviceTypeEnum == ServiceType.HttpService && services.Count == 0)
            {
                services = new List<Type>() { type };
                justDeclared = true;
            }

            foreach (Type serviceType in services)
            {
                typeGenerated.Add(serviceType);
                List<MethodInfo> methods = null;
                if (justDeclared)
                    methods = serviceType.GetListOfDeclaredMethods().Distinct().ToList();
                else
                    methods = serviceType.GetListOfMethodsWithAllOfBases().Distinct().Where(x => x.IsPublic && !x.IsStatic).ToList();
                methods = methods.GroupBy(x => x.Name).Select(x => x.First()).ToList();
                foreach (MethodInfo methodInfo in methods.Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) && !x.IsStatic))
                {
                    if (!getFull)
                    {
                        //generate a method if that has service generation attribute
                        //if serviceMethodName != any urls of GenerationServiceAttribute that method will ignore to generate in service generation
                        var methodServiceGenerationAttributeshas = methodInfo.GetCustomAttributes<GenerationServiceAttribute>(true).ToList();
                        if (methodServiceGenerationAttributeshas.Count > 0 && !methodServiceGenerationAttributeshas.Any(x => x.Urls.Any(url => url.Equals(httpServiceName, StringComparison.OrdinalIgnoreCase))))
                        {
                            continue;
                        }
                    }

                    GenerateMethod(methodInfo, classReferenceInfo);
                    methodGenerated.Add(methodInfo);
                }
            }
            if (classReferenceInfo.Methods.Count > 0)
                NamespaceReferenceInfo.Classes.Add(classReferenceInfo);
        }

        private void GenerateHttpServiceClass(string serviceName, Type type)
        {
            ClassReferenceInfo classReferenceInfo = new ClassReferenceInfo
            {
                ServiceName = serviceName,
                Name = type.Name,
                Type = ClassReferenceType.HttpServiceLevel,
                NameSpace = type.GetNamesapce()
            };
            List<Type> services = type.GetTypesByAttribute<ServiceContractAttribute>(x => x.ServiceType == ServiceType.HttpService).ToList();
            if (services.Count == 0)
                services = new List<Type>() { type };

            foreach (Type serviceType in services)
            {
                typeGenerated.Add(serviceType);
                foreach (MethodInfo methodInfo in serviceType.GetListOfDeclaredMethods().Distinct().Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) && !x.IsStatic))
                {
                    GenerateMethod(methodInfo, classReferenceInfo);
                    methodGenerated.Add(methodInfo);
                }
            }
            if (classReferenceInfo.Methods.Count > 0)
                NamespaceReferenceInfo.Classes.Add(classReferenceInfo);
        }

        private void GenerateModelEnum(Type type)
        {
            if (ModelsCodeGenerated.Contains(type) || CannotGenerateAssemblyTypes(type))
                return;
            ModelsCodeGenerated.Add(type);
            EnumReferenceInfo enumReferenceInfo = new EnumReferenceInfo
            {
                Name = type.Name,
                NameSpace = type.GetNamesapce()
            };
            Type baseEumType = Enum.GetUnderlyingType(type);
            int startIndexAt = 0;
            enumReferenceInfo.TypeName = GetFullNameOfType(baseEumType, true, null, null, ref startIndexAt, out _);
            foreach (object name in Enum.GetValues(type))
            {
                object res = Convert.ChangeType(name, baseEumType);
                string value = "";
                if (res != null)
                    value = res.ToString();
                if (!enumReferenceInfo.KeyValues.Any(x => x.Key == name.ToString()))
                    enumReferenceInfo.KeyValues.Add(new KeyValue<string, string>(name.ToString(), value));
            }
            NamespaceReferenceInfo.Enums.Add(enumReferenceInfo);
        }

        private void GenerateModelClass(Type type)
        {
            if (type == null)
                return;

            if (type == typeof(Task))
            {
                return;
            }
            else if (type.GetBaseType() == typeof(Task))
            {
                type = type.GetGenericArguments()[0];
            }
            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            if (ModelsCodeGenerated.Contains(type) || CannotGenerateAssemblyTypes(type))
                return;
            if (type == typeof(object) || type.GetAssembly() == typeof(string).GetAssembly())
                return;
            bool isGeneric = type.GetIsGenericType();
            if (isGeneric && type.GetGenericTypeDefinition() != type)
            {
                GenerateModelClass(type.GetGenericTypeDefinition());
                return;
            }
            ClassReferenceInfo classReferenceInfo = new ClassReferenceInfo
            {
                Type = ClassReferenceType.ModelLevel,
                NameSpace = type.GetNamesapce()
            };

            Type baseType = type.GetBaseType();
            string genericParameterConstraints = null;
            int startIndexAt = 0;
            if (baseType != typeof(object) && baseType != null)
            {
                classReferenceInfo.BaseClassName = GetFullNameOfType(baseType, true, null, null, ref startIndexAt, out _);
            }
            else
                classReferenceInfo.BaseClassName = GetFullNameOfType(typeof(NotifyPropertyChangedBase), true, null, null, ref startIndexAt, out _);

            classReferenceInfo.Name = GetFullNameOfType(type, false, null, null, ref startIndexAt, out genericParameterConstraints);
            classReferenceInfo.GenericParameterConstraints = genericParameterConstraints;

            ModelsCodeGenerated.Add(type);

            foreach (PropertyInfo propertyInfo in type.GetListOfDeclaredProperties())
            {
                GenerateProperty(propertyInfo, classReferenceInfo);
            }
            GenerateModelClass(type.GetBaseType());
            NamespaceReferenceInfo.Classes.Add(classReferenceInfo);
        }

        private void GenerateModelInterface(Type type)
        {
            if (type == null)
                return;

            if (type == typeof(Task))
            {
                return;
            }
            else if (type.GetBaseType() == typeof(Task))
            {
                type = type.GetGenericArguments()[0];
            }
            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            if (ModelsCodeGenerated.Contains(type) || CannotGenerateAssemblyTypes(type))
                return;
            if (type == typeof(object) || type.GetAssembly() == typeof(string).GetAssembly())
                return;
            bool isGeneric = type.GetIsGenericType();
            if (isGeneric && type.GetGenericTypeDefinition() != type)
            {
                GenerateModelClass(type.GetGenericTypeDefinition());
                return;
            }
            ClassReferenceInfo classReferenceInfo = new ClassReferenceInfo
            {
                Type = ClassReferenceType.InterfaceLevel,
                NameSpace = type.GetNamesapce()
            };

            Type baseType = type.GetBaseType();
            int startIndexAt = 0;
            if (baseType != typeof(object) && baseType != null)
            {
                classReferenceInfo.BaseClassName = GetFullNameOfType(baseType, true, null, null, ref startIndexAt, out _);
            }
            else
                classReferenceInfo.BaseClassName = GetFullNameOfType(typeof(NotifyPropertyChangedBase), true, null, null, ref startIndexAt, out _);

            classReferenceInfo.Name = GetFullNameOfType(type, false, null, null, ref startIndexAt, out _);

            ModelsCodeGenerated.Add(type);

            foreach (PropertyInfo propertyInfo in type.GetListOfDeclaredProperties())
            {
                GenerateProperty(propertyInfo, classReferenceInfo);
            }
            GenerateModelInterface(type.GetBaseType());
            NamespaceReferenceInfo.Classes.Add(classReferenceInfo);
        }

        private Dictionary<string, List<string>> ServiceMethods = new Dictionary<string, List<string>>();

        private void GenerateMethod(MethodInfo methodInfo, ClassReferenceInfo classReferenceInfo)
        {
            if (methodInfo.GetParameters().Any(x => x.IsOut || x.IsRetval) || methodInfo.DeclaringType == typeof(object))
                return;
            if (IsRenameDuplicateMethodNames)
            {
                if (!ServiceMethods.ContainsKey(classReferenceInfo.ServiceName))
                    ServiceMethods[classReferenceInfo.ServiceName] = new List<string>();
            }
            MethodReferenceInfo methodReferenceInfo = new MethodReferenceInfo();

            AddToGenerate(methodInfo.ReturnType);
            string returnType = "void";
            if (methodInfo.ReturnType != typeof(void))
            {
                int startIndexAt = 0;
                returnType = GetFullNameOfType(methodInfo.ReturnType, true, methodInfo, null, ref startIndexAt, out _);
            }
            string methodName = methodInfo.Name;

            if (IsRenameDuplicateMethodNames)
            {
                int i = 1;
                while (ServiceMethods[classReferenceInfo.ServiceName].Contains(methodName))
                {
                    methodName = methodInfo.Name + i;
                    i++;
                }
            }
            var protocolAttribute = methodInfo.GetCustomAttribute<ProtocolAttribute>(true);
            if (protocolAttribute != null)
                methodReferenceInfo.ProtocolType = protocolAttribute.Type;
            methodReferenceInfo.ReturnTypeName = returnType;
            if (GetCustomClientSideMethodNameFunction == null)
                methodReferenceInfo.Name = methodInfo.Name;
            else
                methodReferenceInfo.Name = GetCustomClientSideMethodNameFunction(methodInfo.DeclaringType, methodInfo);
            methodReferenceInfo.RealName = methodInfo.Name;
            methodReferenceInfo.DuplicateName = methodName;

            if (IsRenameDuplicateMethodNames)
                ServiceMethods[classReferenceInfo.ServiceName].Add(methodName);
            GenerateMethodParameters(methodInfo, methodReferenceInfo);
            classReferenceInfo.Methods.Add(methodReferenceInfo);
        }

        private void GenerateProperty(PropertyInfo propertyInfo, ClassReferenceInfo classReferenceInfo)
        {
            if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
                return;
            AddToGenerate(propertyInfo.PropertyType);
            int startIndexAt = 0;
            PropertyReferenceInfo propertyReferenceInfo = new PropertyReferenceInfo
            {
                Name = propertyInfo.Name,
                ReturnTypeName = GetFullNameOfType(propertyInfo.PropertyType, true, null, propertyInfo, ref startIndexAt, out _)
            };

            classReferenceInfo.Properties.Add(propertyReferenceInfo);
        }

        private void GenerateMethodParameters(MethodInfo methodInfo, MethodReferenceInfo methodReferenceInfo)
        {
            foreach (System.Reflection.ParameterInfo item in methodInfo.GetParameters())
            {
                ParameterReferenceInfo parameterReferenceInfo = new ParameterReferenceInfo();
                AddToGenerate(item.ParameterType);
                parameterReferenceInfo.Name = item.Name;
                int startIndexAt = 0;
                parameterReferenceInfo.TypeName = GetFullNameOfType(item.ParameterType, true, methodInfo, null, ref startIndexAt, out _);
                methodReferenceInfo.Parameters.Add(parameterReferenceInfo);
            }
        }

        private void AddUsingIfNotExist(Type type)
        {
            if (!NamespaceReferenceInfo.Usings.Contains(type.GetNamesapce()))
                NamespaceReferenceInfo.Usings.Add(type.GetNamesapce());
        }

        private string GetFullNameOfType(Type type, bool withNameSpace, MethodInfo method, PropertyInfo property, ref int startIndexAt, out string genericParameterConstraints)
        {
            genericParameterConstraints = null;
            if (method != null)
            {
                var result = GetTupleParameterNames(type, method, ref startIndexAt);
                if (result != "()" && result != ")")
                    return result;
            }
            if (property != null)
            {
                var result = GetTupleParameterNames(type, property, ref startIndexAt);
                if (result != "()" && result != ")")
                    return result;
            }
            if (type == typeof(bool))
                return "bool";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(ushort))
                return "ushort";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(sbyte))
                return "sbyte";
            else if (type == typeof(int))
                return "int";
            else if (type == typeof(uint))
                return "uint";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(ulong))
                return "ulong";
            else if (type == typeof(string))
                return "string";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(object))
                return "object";
            else if (type == typeof(Task))
            {
                return "void";
            }
            else if (type.GetBaseType() == typeof(Task))
            {
                return GetFullNameOfType(type.GetGenericArguments()[0], withNameSpace, method, property, ref startIndexAt, out _);
            }
            if (type.GetBaseType() == typeof(Array))
            {
                return GetFullNameOfType(type.GetElementType(), withNameSpace, method, property, ref startIndexAt, out _) + "[]";
            }
            if (CannotGenerateAssemblyTypes(type))
                AddUsingIfNotExist(type);
            if (type.GetIsGenericType())
            {

                string generics = "";
                StringBuilder genericParameterConstraintsBuilder = new StringBuilder();
#if (NETSTANDARD2_0 || NET45 || NET6_0)
                Type[] defparams = type.GetGenericTypeDefinition().GetGenericArguments();
                foreach (Type tp in defparams)
                {
                    Type[] tpConstraints = tp.GetGenericParameterConstraints();
                    foreach (Type tpc in tpConstraints)
                    {
                        if (tpc == typeof(ValueType))
                        {
                            genericParameterConstraintsBuilder.AppendLine($"where {tp.Name} : struct");
                        }
                    }
                }
#endif
                foreach (Type item in type.GetListOfGenericArguments())
                {
                    AddToGenerate(item);
                    if (CannotGenerateAssemblyTypes(item))
                        AddUsingIfNotExist(item);
                    if (!string.IsNullOrEmpty(generics))
                    {
                        generics += ", ";
                    }
                    generics += GetFullNameOfType(item, true, method, property, ref startIndexAt, out _);
                }
                string name = "";
                if (type.Name.IndexOf("`") != -1)
                {
                    if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return generics + "?";
                    }
                    else
                    {
                        name = type.Name.Substring(0, type.Name.IndexOf("`"));
                        if (withNameSpace)
                            name = type.GetNamesapce() + "." + name;
                    }
                }
                genericParameterConstraints = genericParameterConstraintsBuilder.ToString();
                //add type.GetNamesapce() to name spaces
                return $"{name}<{generics}>";
            }
            else if (type.IsGenericParameter)
                return type.Name;
            else
            {
                if (withNameSpace)
                    return type.GetNamesapce() + "." + type.Name;
                else
                    return type.Name;
            }
        }

        string GetTupleParameterNames(Type type, MethodInfo method, ref int startIndexAt)
        {
            StringBuilder result = new StringBuilder();
            result.Append("(");
#if (!NETSTANDARD1_6 && !NET45)
            if (type.GetBaseType() == typeof(ValueType))
            {
                var tupleGenerics = type.GetGenericArguments();
                if (tupleGenerics.Length > 0)
                {
                    var attribs = method.ReturnTypeCustomAttributes;
                    if (attribs.GetCustomAttributes(true) != null)
                    {
                        foreach (var at in attribs.GetCustomAttributes(true))
                        {
                            if (at is System.Runtime.CompilerServices.TupleElementNamesAttribute)
                            {
                                var ng = ((System.Runtime.CompilerServices.TupleElementNamesAttribute)at).TransformNames;
                                int index = 0;
                                var items = ng.Skip(startIndexAt).Take(tupleGenerics.Length).ToArray();
                                foreach (var ca in items)
                                {
                                    int myStartIndexAt = tupleGenerics.Length;
                                    result.Append($"{GetFullNameOfType(tupleGenerics[index], true, method, null, ref myStartIndexAt, out _)} ");
                                    result.Append($"{ca}");
                                    result.Append(", ");
                                    index++;
                                    startIndexAt++;
                                }
                                if (result.Length > 2)
                                    result = result[result.Length - 1] == ',' ? result.Remove(result.Length - 1, 1) : result.Remove(result.Length - 2, 2);
                                result.Append(")");
                            }
                        }
                    }
                }
            }
#endif
            if (result.Length > 2)
                result = result[result.Length - 1] == ')' ? result.Remove(result.Length - 1, 1) : result.Remove(result.Length - 2, 2);
            result.Append(")");
            return result.ToString();
        }

        string GetTupleParameterNames(Type type, PropertyInfo property, ref int startIndexAt)
        {
            StringBuilder result = new StringBuilder();
            result.Append("(");
            if (type.GetBaseType() == typeof(ValueType))
            {
                var tupleGenerics = type.GetGenericArguments();
                if (tupleGenerics.Length > 0)
                {
                    if (property.CustomAttributes != null)
                    {
                        foreach (var at in property.CustomAttributes)
                        {
                            if (at is System.Reflection.CustomAttributeData)
                            {
                                var ng = ((System.Reflection.CustomAttributeData)at).ConstructorArguments;
                                int index = 0;
                                foreach (var ca in ng)
                                {
                                    try
                                    {
                                        foreach (var val in (IEnumerable<System.Reflection.CustomAttributeTypedArgument>)ca.Value)
                                        {
                                            result.Append($"{GetFullNameOfType(tupleGenerics[index], true, null, property, ref startIndexAt, out _)} ");
                                            result.Append($"{val.Value}");
                                            result.Append(", ");
                                            index++;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (result.Length > 2)
                result = result.Remove(result.Length - 2, 2);
            result.Append(")");
            return result.ToString();
        }
    }
}
