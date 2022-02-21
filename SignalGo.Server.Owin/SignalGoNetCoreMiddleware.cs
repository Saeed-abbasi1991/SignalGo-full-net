﻿#if (NETSTANDARD)
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SignalGo.Server.ServiceManager;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SignalGo.Server.Owin
{
    public class SignalGoNetCoreMiddleware
    {
        private ServerBase CurrentServerBase { get; set; }
        private readonly RequestDelegate _next;
        private readonly Func<Task> _next2;

        public SignalGoNetCoreMiddleware(ServerBase serverBase, RequestDelegate next)
        {
            CurrentServerBase = serverBase;
            _next = next;
        }

        public SignalGoNetCoreMiddleware(ServerBase serverBase, Func<Task> next)
        {
            CurrentServerBase = serverBase;
            _next2 = next;
        }

        public static Func<HttpContext, object> BeginInvokeConext { get; set; }
        public static Func<object, HttpContext, Task> EndInvokeConext { get; set; }

        public async Task Invoke(HttpContext context)
        {
            string uri = context.Request.Path.Value + context.Request.QueryString.ToString();
            string serviceName = uri.Contains('/') ? uri.Substring(0, uri.LastIndexOf('/')).Trim('/') : "";
            bool isWebSocketd = context.Request.Headers.ContainsKey("Sec-WebSocket-Key");
            if (!BaseProvider.ExistService(serviceName, CurrentServerBase) && !isWebSocketd && !context.Request.Headers.ContainsKey("signalgo") && !context.Request.Headers.ContainsKey("signalgo-servicedetail") && context.Request.Headers["content-type"] != "SignalGo Service Reference")
            {
                if (_next != null)
                    await _next.Invoke(context).ConfigureAwait(false);
                else if (_next2 != null)
                    await _next2.Invoke().ConfigureAwait(false);
                return;
            }
            context.Response.Headers.Add("IsSignalGoOverIIS", "true");

            var instance = BeginInvokeConext?.Invoke(context);
            OwinClientInfo owinClientInfo = new OwinClientInfo(CurrentServerBase);
            owinClientInfo.HttpContext = context;
            owinClientInfo.ChangeStatusAction = (code) =>
            {
                context.Response.StatusCode = code;
            };

            owinClientInfo.ConnectedDateTime = DateTime.Now;
            owinClientInfo.IPAddressBytes = context.Connection.RemoteIpAddress.GetAddressBytes();
            owinClientInfo.ClientId = Guid.NewGuid().ToString();
            CurrentServerBase.Clients.TryAdd(owinClientInfo.ClientId, owinClientInfo);

            //owinClientInfo.OwinContext = context;
            owinClientInfo.RequestHeaders = new HttpHeaderCollection(context.Request.Headers);
            owinClientInfo.ResponseHeaders = new HttpHeaderCollection(context.Response.Headers);
            if (context.Request.Headers.ContainsKey("signalgo") && context.Request.Headers["signalgo"] == "SignalGoHttpDuplex")
            {
                //context.Response.Headers["Content-Length"] = "170";
                //context.Request.EnableRewind();
                //context.Request.EnableBuffering();
                //owinClientInfo.StreamHelper = SignalGoStreamBase.CurrentBase;
                //owinClientInfo.ClientStream = new PipeNetworkStream(new DuplexStream(context.Request.Body, context.Response.Body));
                //owinClientInfo.ProtocolType = ClientProtocolType.HttpDuplex;
                //if (context.Request.Headers["SignalgoDuplexWebSocket"] == "true")
                //{
                //    owinClientInfo.StreamHelper = SignalGoStreamWebSocketLlight.CurrentWebSocket;
                //    await HttpProvider.AddSignalGoWebSocketHttpClient(owinClientInfo, CurrentServerBase);
                //}
                //else
                //    await HttpProvider.AddSignalGoWebSocketHttpClient(owinClientInfo, CurrentServerBase);
                //await Task.FromResult<object>(null);
            }
            else if (isWebSocketd)
            {
                owinClientInfo.StreamHelper = SignalGoStreamBase.CurrentBase;
                bool web = context.WebSockets.IsWebSocketRequest;
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                owinClientInfo.ClientStream = new PipeNetworkStream(new WebsocketStream(webSocket));
                //if (context.Request.Headers["SignalgoDuplexWebSocket"] == "true")
                //{
                //    //owinClientInfo.StreamHelper = SignalGoStreamWebSocketLlight.CurrentWebSocket;
                //    //await HttpProvider.AddSignalGoWebSocketHttpClient(owinClientInfo, CurrentServerBase);
                //    await HttpProvider.AddWebSocketHttpClient(owinClientInfo, CurrentServerBase);
                //}
                //else
                await HttpProvider.AddWebSocketHttpClient(owinClientInfo, CurrentServerBase).ConfigureAwait(false);
                await Task.FromResult<object>(null).ConfigureAwait(false);
            }
            else
            {
                owinClientInfo.StreamHelper = SignalGoStreamBase.CurrentBase;
                owinClientInfo.ClientStream = new PipeNetworkStream(new DuplexStream(context.Request.Body, context.Response.Body));
                await HttpProvider.AddHttpClient(owinClientInfo, CurrentServerBase, uri, context.Request.Method, null, null).ConfigureAwait(false);
            }

            EndInvokeConext?.Invoke(instance, context);
        }


    }

    public class HttpHeaderCollection : IDictionary<string, string[]>
    {
        private IHeaderDictionary _headerDictionary;
        public HttpHeaderCollection(IHeaderDictionary headerDictionary)
        {
            _headerDictionary = headerDictionary;
        }

        public string[] this[string key]
        {
            get
            {
                return _headerDictionary[key];
            }
            set
            {
                _headerDictionary[key] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _headerDictionary.Keys;
            }
        }

        public ICollection<string[]> Values
        {
            get
            {
                return _headerDictionary.Values.Select(x => (string[])x).ToList();
            }
        }

        public int Count
        {
            get
            {
                return _headerDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _headerDictionary.IsReadOnly;
            }
        }

        public void Add(string key, string[] value)
        {
            _headerDictionary.Add(key, value);
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            _headerDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _headerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            return _headerDictionary.Contains(new KeyValuePair<string, StringValues>(item.Key, item.Value));
        }

        public bool ContainsKey(string key)
        {
            return _headerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return _headerDictionary.Select(x => new KeyValuePair<string, string[]>(x.Key, x.Value)).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _headerDictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return _headerDictionary.Remove(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>(item.Key, item.Value));
        }

        public bool TryGetValue(string key, out string[] value)
        {
            bool result = _headerDictionary.TryGetValue(key, out StringValues data);
            value = data;
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _headerDictionary.GetEnumerator();
        }
    }
}
#endif