﻿using SignalGo.DataExchanger.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SignalGoTest.DataExhanger
{
    public class QueryDataExchangerAllTests
    {
        [Fact]
        public void Example1()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali""}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali").ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example2()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" and user.family = ""yousefi""}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" && x.Family == "yousefi").ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example3()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" and user.family = ""yousefi"" or (user.name == ""reza"" or user.family == ""jamal"")}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" && x.Family == "yousefi" || (x.Name == "reza" || x.Family == "jamal")).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example4()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" and user.family = ""yousefi"" or (user.name == ""reza"" or user.family == ""jamal"" or (user.family == ""jamal""))}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" && x.Family == "yousefi" || (x.Name == "reza" && x.Family == "jamal" || (x.Family == "jamal"))).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example5()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" or user.name = ""ali"" and (user.family == ""jamal"")}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();

            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" || x.Name == "ali" && (x.Family == "jamal")).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example6()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" or user.name = ""ali"" and (user.family == ""jamal"") or user.family == ""jamal""}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Name == "ali" || x.Name == "ali" && (x.Family == "jamal") || x.Family == "jamal").ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example7()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" or count(user.posts) = 1}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" || x.Posts.Count() == 1).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example8()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" or count ( user.posts ) = 1 or count(user.posts)=0}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" || x.Posts.Count() == 1 || x.Posts.Count() == 0).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example9()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" or count ( user.posts ) != count(user.posts)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" || x.Posts.Count() != x.Posts.Count()).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example10()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" or(user.name == ""ali"" and count ( user.posts ) >0)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" || (x.Name == "ali" && x.Posts.Count() > 0)).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);

        }

        [Fact]
        public void Example11()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" and sum ( 5 , 1 , 4 ) == 10)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" && 5 + 1 + 4 == 10).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);

        }

        [Fact]
        public void Example12()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.family=""yousefi"" and sum(5,1,4)==10)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => x.Family == "yousefi" && 5 + 1 + 4 == 10).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);

        }

        [Fact]
        public void Example13()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where sum(count(user.posts),count(user.posts),count(user.posts))==6)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => (x.Posts.Count() + x.Posts.Count() + x.Posts.Count()) == 6).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example14()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where sum(sum(count(user.posts),count(user.posts)),count(user.posts))==6)}";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple.Where(x => ((x.Posts.Count() + x.Posts.Count()) + x.Posts.Count()) == 6).ToList();
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example15()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}
                                    var user
                                    {
                                        where sum(sum(count(user.posts),count(user.posts)),count(user.posts))==6)
                                        var post in user.posts
	                                    {
		                                    where post.Title = ""hello every body""
	                                    }
                                        var file in user.files
	                                    {
		                                    where file.Name = ""page.jpg""
	                                    }
                                    }";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();
            IEnumerable<UserEx> toComiple2 = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple2.Where(x => ((x.Posts.Count() + x.Posts.Count()) + x.Posts.Count()) == 6).ToList();
            foreach (UserEx item in linqList)
            {
                if (item.Files != null)
                {
                    item.Files = item.Files.Where(x => x.Name == "page.jpg").ToList();
                    foreach (var file in item.Files)
                    {
                        file.DateTime = DateTime.MinValue;
                    }
                }
                if (item.Posts != null)
                {
                    foreach (var post in item.Posts)
                    {
                        post.Content = null;
                        foreach (var news in post.News)
                        {
                            news.Description = null;
                        }
                        foreach (var art in post.Articles)
                        {
                            art.Date = DateTime.MinValue;
                        }
                    }
                    item.Posts = item.Posts.Where(x => x.Title == "hello every body").ToList();
                }
            }
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }

        [Fact]
        public void Example16()
        {
            string query = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}
                                    var user
                                    {
                                        where sum(sum(count(user.posts),count(user.posts)),count(user.posts))==6)
                                        var post in user.posts
	                                    {
		                                    where post.Title = ""hello every body""
                                            var news in post.news
                                            {
                                                where news.NewsName = ""how are you world?""
                                            }
	                                    }
                                        var file in user.files
	                                    {
		                                    where file.Name = ""page.jpg""
	                                    }
                                    }";
            SelectCompiler selectCompiler = new SelectCompiler();
            string anotherResult = selectCompiler.Compile(query);
            ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
            conditionsCompiler.Compile(anotherResult);
            IEnumerable<UserEx> toComiple = QueryDataExchangerText.GetUsersEx();
            IEnumerable<UserEx> toComiple2 = QueryDataExchangerText.GetUsersEx();

            object result = selectCompiler.Run(toComiple);
            IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
            List<UserEx> resultData = resultWheres.ToList();
            List<UserEx> linqList = toComiple2.Where(x => ((x.Posts.Count() + x.Posts.Count()) + x.Posts.Count()) == 6).ToList();
            foreach (UserEx item in linqList)
            {
                if (item.Files != null)
                {
                    item.Files = item.Files.Where(x => x.Name == "page.jpg").ToList();
                    foreach (var file in item.Files)
                    {
                        file.DateTime = DateTime.MinValue;
                    }
                }
                if (item.Posts != null)
                {
                    foreach (var post in item.Posts)
                    {
                        post.Content = null;
                        post.News = post.News.Where(x => x.NewsName == "how are you world?").ToList();
                        foreach (var news in post.News)
                        {
                            news.Description = null;
                        }
                        foreach (var art in post.Articles)
                        {
                            art.Date = DateTime.MinValue;
                        }
                    }
                    item.Posts = item.Posts.Where(x => x.Title == "hello every body").ToList();
                }
            }
            bool equal = resultData.SequenceEqual(linqList);
            Assert.True(equal);
        }
    }
}
