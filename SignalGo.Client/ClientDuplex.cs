﻿using SignalGo.Client.ClientManager;

namespace SignalGo.Client
{
    /// <summary>
    /// a client duplex class
    /// </summary>
    public class ClientDuplex : OperationCalls
    {
        public ConnectorBase Connector { get; set; }
    }

    /// <summary>
    /// an opration calls interface for inject code
    /// </summary>
    public interface OperationCalls
    {
        ConnectorBase Connector { get; set; }
    }
}
