﻿using SignalGo.Shared.Models.ServiceReference;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// method of service
    /// </summary>
    public class ServiceDetailsMethod
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// name of method
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// comment of class
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// comment of return type
        /// </summary>
        public string ReturnComment { get; set; }
        /// <summary>
        /// comment of exceptions
        /// </summary>
        public string ExceptionsComment { get; set; }
        /// <summary>
        /// return type
        /// </summary>
        public string ReturnType { get; set; }
        /// <summary>
        /// test example to call thi method
        /// </summary>
        public string TestExample { get; set; }
        /// <summary>
        /// example of json with fill data in request
        /// </summary>
        public string RequestJsonExample { get; set; }
        /// <summary>
        /// example of json with fill data in response
        /// </summary>
        public string ResponseJsonExample { get; set; }
        /// <summary>
        /// requests of method
        /// </summary>
#if (!NET35)
        public ObservableCollection<ServiceDetailsRequestInfo> Requests { get; set; }
#endif
        /// <summary>
        /// if item is exanded from treeview
        /// </summary>
        public bool IsExpanded { get; set; }
        /// <summary>
        /// if item is selected from treeview
        /// </summary>
        public bool IsSelected { get; set; }

        public List<ParameterReferenceInfo> Parameters { get; set; } = new List<ParameterReferenceInfo>();

#if (!NET35)
        public ServiceDetailsMethod Clone()
        {
            return new ServiceDetailsMethod()
            {
                Id = Id,
                Comment = Comment,
                ExceptionsComment = ExceptionsComment,
                MethodName = MethodName,
                Requests = new ObservableCollection<ServiceDetailsRequestInfo>(),
                ReturnComment = ReturnComment,
                ReturnType = ReturnType,
                TestExample = TestExample,
                IsSelected = IsSelected,
                IsExpanded = IsExpanded,
                Parameters = Parameters.Select(x => x.Clone()).ToList()
            };
        }
#endif
    }

    public class ServiceDetailsRequestInfo : INotifyPropertyChanged
    {
        public bool IsSelected { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// list of parameters
        /// </summary>
        public List<ServiceDetailsParameterInfo> Parameters { get; set; }

        private string _Response;

        /// <summary>
        /// response of request
        /// </summary>
        public string Response
        {
            get
            {
                return _Response;
            }
            set
            {
                _Response = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Response)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ServiceDetailsRequestInfo Clone()
        {
            return new ServiceDetailsRequestInfo() { Name = Name, Parameters = new List<ServiceDetailsParameterInfo>() };
        }
    }

    public class MethodParameterDetails
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public int ParameterIndex { get; set; }
        public int ParametersCount { get; set; }
        public bool IsFull { get; set; }
    }
}
