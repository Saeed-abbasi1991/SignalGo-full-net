﻿using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public interface ITestServerModelBase
    {
        Tuple<bool> Logout(string yourName);
    }

    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", ServiceType.ServerService, InstanceType = SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", ServiceType.HttpService, InstanceType = SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", ServiceType.OneWayService, InstanceType = SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    public interface ITestServerModel : ITestServerModelBase
    {
        //string HelloWorld([Bind(Excludes = new string[] { "CategoryDescription" })]string yourName);
        string HelloWorld(string yourName);
        List<UserInfoTest> GetListOfUsers();
        List<PostInfoTest> GetPostsOfUser(int userId);
        [CustomDataExchanger(typeof(UserInfoTest), "Id", "Password", "PostInfoes", ExchangeType = CustomDataExchangerType.TakeOnly, LimitationMode = LimitExchangeType.OutgoingCall)]
        List<UserInfoTest> GetListOfUsersCustom();
        List<PostInfoTest> GetCustomPostsOfUser(int userId);
        bool HelloBind([Bind(Include = "Id")]UserInfoTest userInfoTest, [Bind(Exclude = "Username")]UserInfoTest userInfoTest2,
            [Bind(Includes = new string[] { "Id", "Username" })]UserInfoTest userInfoTest3);
        bool Login(UserInfoTest userInfoTest);
        Task<string> ServerAsyncMethod(string name);
        ArticleInfo AddArticle(ArticleInfo articleInfo);
        MessageContract<ArticleInfo> AddArticleMessage(ArticleInfo articleInfo);
    }

    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", ServiceType.ClientService, InstanceType = SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    public interface ITestClientServiceModel
    {
        string HelloWorld(string yourName);
        Task<string> HelloWorld2(string yourName);
        string TestMethod(string param1, string param2);
        Task<string> TestMethod2(string param1, string param2);
    }
}
