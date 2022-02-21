﻿using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using Telegram.Bot.Args;

namespace SignalGo.Server.TelegramBot.Models
{
    public enum BotLevelType : byte
    {
        Services = 0,
        Methods = 1,
        Parameters = 2
    }

    /// <summary>
    /// create your bot structre
    /// </summary>
    public interface IBotStructureInfo
    {
        /// <summary>
        /// when server want to initialize
        /// you could customize your methods responses here
        /// </summary>
        void OnStarted(SignalGoBotManager signalGoBotManager);
        /// <summary>
        /// when new client connect to bot
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="signalGoBotManager"></param>
        void OnClientConnected(TelegramClientInfo clientInfo, SignalGoBotManager signalGoBotManager);
        /// <summary>
        /// initialize all services and methods buttons  etc from attributes otherwise buttons will not initialize and show to clients
        /// </summary>
        bool InitializeServicesFromAttributes { get; set; }
        /// <summary>
        /// text of cancel button default is /Cancel
        /// </summary>
        /// <returns></returns>
        string GetCancelButtonText(TelegramClientInfo clientInfo);
        /// <summary>
        /// text of Send button default is /Send
        /// </summary>
        /// <returns></returns>
        string GetSendButtonText(TelegramClientInfo clientInfo);
        /// <summary>
        /// text of Services Generated
        /// </summary>
        /// <returns></returns>
        string GetServicesGeneratedText(TelegramClientInfo clientInfo);
        /// <summary>
        /// text of service not found
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        string GetServiceNotFoundText(string serviceName, TelegramClientInfo clientInfo);
        /// <summary>
        /// text of service selected
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        string GetServiceSelectedText(string serviceName, string caption, Type serviceType, TelegramClientInfo clientInfo);
        /// <summary>
        /// text of method selected on bot
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        string GetMethodSelectedText(string methodName, TelegramClientInfo clientInfo);
        /// <summary>
        /// text of parameter selected on bot buttons
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        string GetParameterSelectedText(string parameterName, TelegramClientInfo clientInfo);
        /// <summary>
        /// text of parameter not found
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        string GetParameterNotFoundText(string parameterName, TelegramClientInfo clientInfo);
        /// <summary>
        /// text of parameter value changed
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        string GetParameterValueChangedText(string parameterName, TelegramClientInfo clientInfo);
        /// <summary>
        ///  this method will calling before the service methods Call
        /// </summary>
        /// <param name="serverBase"></param>
        /// <param name="clientInfo"></param>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <returns>if you return false service method will not call</returns>
        bool OnBeforeMethodCall(ServerBase serverBase, TelegramClientInfo clientInfo, string serviceName, string methodName, List<ParameterInfo> parameters);
        /// <summary>
        /// make your custom response to client
        /// </summary>
        /// <param name="serverBase"></param>
        /// <param name="clientInfo"></param>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="currentResponse"></param>
        /// <returns></returns>
        string OnCustomResponse(ServerBase serverBase, TelegramClientInfo clientInfo, string serviceName, string methodName, List<ParameterInfo> parameters,
            CallMethodResultInfo<OperationContext> currentResponse, out bool changed);

        /// <summary>
        /// when service is generating as button you could handle it
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        bool OnServiceGenerating(string serviceName, TelegramClientInfo clientInfo);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttons"></param>
        /// <param name="clientInfo"></param>
        void OnButtonsGenerating(List<List<BotButtonInfo>> buttons, BotLevelType botLevelType, string serviceName, string methodName, TelegramClientInfo clientInfo);
        /// <summary>
        /// when parameter selecting
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="parameterInfo"></param>
        /// <param name="clientInfo"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool OnParameterSelecting(System.Reflection.MethodInfo methodInfo, System.Reflection.ParameterInfo parameterInfo, TelegramClientInfo clientInfo, string value);
    }
}
