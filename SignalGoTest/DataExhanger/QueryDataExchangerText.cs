﻿using SignalGo.DataExchanger.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SignalGoTest.DataExhanger
{
    public class UserEx
    {
        public string Name { get; set; }
        public string Family { get; set; }
        public IEnumerable<PostEx> Posts { get; set; }
        public IEnumerable<FileEx> Files { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is UserEx user)
            {
                return Name == user.Name && Family == user.Family && (Posts == null ? Posts == user.Posts : Posts.SequenceEqual(user.Posts)) && (Files == null ? Files == user.Files : Files.SequenceEqual(user.Files));
            }
            return false;
        }
    }

    public class PostEx
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<ArticleEx> Articles { get; set; }
        public IEnumerable<NewsEx> News { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is PostEx post)
            {
                return Title == post.Title && Content == post.Content && Date.ToString() == post.Date.ToString() && (News == null ? News == post.News : News.SequenceEqual(post.News)) && (Articles == null ? Articles == post.Articles : Articles.SequenceEqual(post.Articles));
            }
            return false;
        }
    }

    public class ArticleEx
    {
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is ArticleEx article)
            {
                return Author == article.Author && Date.ToString() == article.Date.ToString();
            }
            return false;
        }
    }

    public class NewsEx
    {
        public string NewsName { get; set; }
        public string Description { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is NewsEx news)
            {
                return NewsName == news.NewsName && Description == news.Description;
            }
            return false;
        }
    }

    public class FileEx
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is FileEx file)
            {
                return Id == file.Id && Name == file.Name && DateTime.ToString() == file.DateTime.ToString();
            }
            return false;
        }
    }
    /// <summary>
    /// Summary description for QueryDataExchangerText
    /// </summary>
    public class QueryDataExchangerText
    {
        [Fact]
        public void TestMethod1()
        {
            return;
            while (true)
            {
                try
                {
                    string query = @"select
{
	name 
	family 
	posts
	{
		title 
		content
		articles
		{
			author 
			date
		}
		date
        files
        {
            id 
            name 
            datetime
        }
	}
} 
var user
{
	where user.id > 10 and user.name = ""reza"" and count(user.posts) > 0 skip 10 take 20
	post in user.posts
	{
		where post.id > 20 and contains(post.title,""hello"")
 		or count(var x from post.articles where x.author = ""ali"") > 5 order by post.id desc
	}
}
";
                    string query2 = @"select{name family posts{title content articles{author date}date news{newsName description}} files{id name datetime}}var user
{
	where user.id > 10 and user.name = ""reza"" and count(user.posts) > 0 skip 10 take 20
	post in user.posts
	{
		where post.id > 20 and contains(post.title,""hello"")
 		or count(var x from post.articles where x.author = ""ali"") > 5 order by post.id desc
	}
}
";
                    string query3 = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali"" and(user.family=""yousefi""and user.name=""reza"")or user.posts.count>
1
}";
                    string query4 = @"select{name family posts{title articles{author}date news{newsName}}files{id name}}var user{where user.name=""ali""and (user.posts.count<=5 or (user.posts.count>=1))}";
                    SelectCompiler selectCompiler = new SelectCompiler();
                    string anotherResult = selectCompiler.Compile(query3);
                    ConditionsCompiler conditionsCompiler = new ConditionsCompiler();
                    conditionsCompiler.Compile(anotherResult);
                    IEnumerable<UserEx> main = GetUsersEx();
                    IEnumerable<UserEx> toComiple = GetUsersEx();

                    object result = selectCompiler.Run(toComiple);
                    IEnumerable<UserEx> resultWheres = (IEnumerable<UserEx>)conditionsCompiler.Run<UserEx>(toComiple);
                    List<UserEx> realData = resultWheres.ToList();
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        public static IEnumerable<UserEx> GetUsersEx()
        {
            List<UserEx> users = new List<UserEx>();

            UserEx user1 = new UserEx()
            {
                Name = "ali",
                Family = "yousefi"
            };

            UserEx user2 = new UserEx()
            {
                Name = "reza",
                Family = "jamal"
            };

            UserEx user3 = new UserEx()
            {
                Name = "ali",
                Family = "jamal"
            };

            List<FileEx> files1 = new List<FileEx>();

            FileEx file1 = new FileEx()
            {
                Id = 1,
                Name = "picture.png",
                DateTime = DateTime.Now
            };

            FileEx file2 = new FileEx()
            {
                Id = 2,
                Name = "page.jpg",
                DateTime = DateTime.Now.AddDays(-1)
            };

            List<FileEx> files2 = new List<FileEx>();

            FileEx file3 = new FileEx()
            {
                Id = 3,
                Name = "winrar.zip",
                DateTime = DateTime.Now
            };


            List<PostEx> posts1 = new List<PostEx>();
            List<PostEx> posts2 = new List<PostEx>();

            PostEx post1 = new PostEx()
            {
                Title = "hello every body",
                Content = "hello guys I have a problem please help me",
                Date = DateTime.Now,
            };
            PostEx post2 = new PostEx()
            {
                Title = "learn signalgo",
                Content = "click here to learn signalgo : http://no-document-no-learn.com",
                Date = DateTime.Now,
            };
            PostEx post3 = new PostEx()
            {
                Title = "data exchanger is comming!",
                Content = "lol, what the hell are you talking about when there is to more libraries?",
                Date = DateTime.Now,
            };

            List<NewsEx> newslist1 = new List<NewsEx>();
            List<NewsEx> newslist2 = new List<NewsEx>();
            List<NewsEx> newslist3 = new List<NewsEx>();

            NewsEx news1 = new NewsEx()
            {
                NewsName = "how are you world?",
                Description = "haha"
            };
            NewsEx news2 = new NewsEx()
            {
                NewsName = "clouds is comming or not?",
                Description = "yes or no?"
            };
            NewsEx news3 = new NewsEx()
            {
                NewsName = "microsoft removed signalgo posts in stackoverflow!",
                Description = "this always happening i don't know why, mybe save odata?"
            };

            NewsEx news4 = new NewsEx()
            {
                NewsName = "iran is the hell of the world",
                Description = "yes ofcurse"
            };

            List<ArticleEx> articles1 = new List<ArticleEx>();
            List<ArticleEx> articles2 = new List<ArticleEx>();
            List<ArticleEx> articles3 = new List<ArticleEx>();

            ArticleEx article1 = new ArticleEx()
            {
                Author = "ali yousefi is dead in iran witout Migration!",
                Date = DateTime.Now
            };

            ArticleEx article2 = new ArticleEx()
            {
                Author = "im dead!",
                Date = DateTime.Now
            };

            ArticleEx article3 = new ArticleEx()
            {
                Author = "dammit you all!",
                Date = DateTime.Now
            };

            ArticleEx article4 = new ArticleEx()
            {
                Author = "i try to make the world better and easier and cleaner not just with codes!",
                Date = DateTime.Now
            };

            articles1.Add(article1);
            articles2.Add(article2);
            articles3.Add(article3);
            articles3.Add(article4);

            post1.Articles = articles1;
            post2.Articles = articles2;
            post3.Articles = articles3;

            newslist1.Add(news1);
            newslist1.Add(news2);
            newslist1.Add(news3);

            newslist2.Add(news4);

            post1.News = newslist1;
            post2.News = newslist2;
            post3.News = newslist3;

            posts1.Add(post1);
            posts1.Add(post2);
            posts2.Add(post3);

            files1.Add(file1);
            files1.Add(file2);
            files2.Add(file3);

            user1.Files = files1;
            user2.Files = files2;

            user1.Posts = posts1;
            user2.Posts = posts2;
            user3.Posts = new List<PostEx>();

            users.Add(user1);
            users.Add(user2);
            users.Add(user3);
            return users;
        }
    }
}
