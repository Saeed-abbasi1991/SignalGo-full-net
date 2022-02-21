﻿using SignalGo.Shared.Models;
using System.Collections.Generic;

namespace SignalGo.Shared.Http
{
    public abstract class HttpRequestController : IHttpClientInfo
    {
        private static readonly string[][] s_HTTPStatusDescriptions;

        static HttpRequestController()
        {
            string[][] textArrayArray1 = new string[6][];
            textArrayArray1[1] = new string[] { "Continue", "Switching Protocols", "Processing" };
            textArrayArray1[2] = new string[] { "OK", "Created", "Accepted", "Non-Authoritative Information", "No Content", "Reset Content", "Partial Content", "Multi-Status" };
            textArrayArray1[3] = new string[] { "Multiple Choices", "Moved Permanently", "Found", "See Other", "Not Modified", "Use Proxy", string.Empty, "Temporary Redirect" };
            textArrayArray1[4] = new string[] {
                "Bad Request", "Unauthorized", "Payment Required", "Forbidden", "Not Found", "Method Not Allowed", "Not Acceptable", "Proxy Authentication Required", "Request Timeout", "Conflict", "Gone", "Length Required", "Precondition Failed", "Request Entity Too Large", "Request-Uri Too Long", "Unsupported Media Type",
                "Requested Range Not Satisfiable", "Expectation Failed", string.Empty, string.Empty, string.Empty, string.Empty, "Unprocessable Entity", "Locked", "Failed Dependency"
            };
            textArrayArray1[5] = new string[] { "Internal Server Error", "Not Implemented", "Bad Gateway", "Service Unavailable", "Gateway Timeout", "Http Version Not Supported", string.Empty, "Insufficient Storage" };
            s_HTTPStatusDescriptions = textArrayArray1;
        }

        public static string GetStatusDescription(int code)
        {
            if ((code >= 100) && (code < 600))
            {
                int index = code / 100;
                int num2 = code % 100;
                if (num2 < s_HTTPStatusDescriptions[index].Length)
                {
                    return s_HTTPStatusDescriptions[index][num2];
                }
            }
            return string.Empty;
        }

        public System.Net.HttpStatusCode Status { get; set; } = System.Net.HttpStatusCode.OK;
        public IDictionary<string, string[]> RequestHeaders { get; set; }
        public IDictionary<string, string[]> ResponseHeaders { get; set; } = new WebHeaderCollection();

        string _IPAddress;
        /// <summary>
        /// ip address of client
        /// </summary>
        public string IPAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_IPAddress))
                    _IPAddress = new System.Net.IPAddress(IPAddressBytes).ToString();
                return _IPAddress;
            }
        }

        byte[] _IPAddressBytes;
        /// <summary>
        /// bytes of ip address
        /// </summary>
        public byte[] IPAddressBytes
        {
            get
            {
                return _IPAddressBytes;
            }
            set
            {
                _IPAddressBytes = value;
                _IPAddress = null;
            }
        }

        public ActionResult Content(string text)
        {
            return new ActionResult(text);
        }

        public ActionResult Content(object data)
        {
            return new ActionResult(data);
        }

        private HttpPostedFileInfo _currentFile = null;
        public void SetFirstFile(HttpPostedFileInfo fileInfo)
        {
            _currentFile = fileInfo;
        }

        public HttpPostedFileInfo TakeNextFile()
        {
            return _currentFile;
        }
    }
}
