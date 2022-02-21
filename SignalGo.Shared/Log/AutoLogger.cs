﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Shared.Log
{
    /// <summary>
    /// log exceptions and texts to a file
    /// </summary>
    public class AutoLogger
    {
        public static AutoLogger Default { get; set; } = new AutoLogger() { DirectoryName = "", FileName = "App Logs.log" };
        /// <summary>
        /// is enabled log system
        /// </summary>
        public static bool IsEnabled { get; set; } = true;
        /// <summary>
        /// full path of log
        /// </summary>
        public static string DirectoryLocation { get; set; }
        /// <summary>
        /// directory name of log
        /// </summary>
        public string DirectoryName { get; set; }
        /// <summary>
        /// file name to save
        /// </summary>
        public string FileName { get; set; }
        string _SavePath;
        private string SavePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_SavePath))
                    return _SavePath;
                return _SavePath;
            }
        }
#if (NET35 || NET40)
        public void GenerateSavePath()
#else
        public async Task GenerateSavePath()
#endif
        {
            try
            {
#if (NET35 || NET40)
                lockWaitToRead.Wait();
#else
                await lockWaitToRead.WaitAsync().ConfigureAwait(false);
#endif

                if (string.IsNullOrEmpty(DirectoryName))
                    _SavePath = DirectoryLocation;
                else
                    _SavePath = Path.Combine(DirectoryLocation, DirectoryName);
                try
                {
#if (!PORTABLE)
                    if (!Directory.Exists(_SavePath))
                        Directory.CreateDirectory(_SavePath);
#endif
                }
                catch
                {

                }
                _SavePath = Path.Combine(_SavePath, FileName);
            }
            finally
            {
                lockWaitToRead.Release();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public AutoLogger()
        {
#if (!PORTABLE)
            try
            {
#if (NETSTANDARD1_6)
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetEntryAssembly();
                if (asm != null)
                    DirectoryLocation = Path.GetDirectoryName(asm.Location);
#else
                if (!string.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory))
                    DirectoryLocation = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar);
#endif
                if (!string.IsNullOrEmpty(DirectoryLocation) && !Directory.Exists(DirectoryLocation))
                    Directory.CreateDirectory(DirectoryLocation);
                //Console.WriteLine("log location:" + DirectoryLocation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            DirectoryName = "SignalGoDiagnostic";
            FileName = "SignalGo Logs.log";
#endif
        }
#if (!PORTABLE)

        private void GetOneStackTraceText(StackTrace stackTrace, StringBuilder builder)
        {
            builder.AppendLine("<------------------------------StackTrace One Begin------------------------------>");

            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            // write call stack method names
            foreach (StackFrame stackFrame in stackFrames)
            {
                System.Reflection.MethodBase method = stackFrame.GetMethod();

                builder.AppendLine("<---Method Begin--->");
                builder.AppendLine("File Name: " + stackFrame.GetFileName());
                builder.AppendLine("Line Number: " + stackFrame.GetFileLineNumber());
                builder.AppendLine("Column Number: " + stackFrame.GetFileColumnNumber());



                builder.AppendLine("Name: " + method.Name);
                builder.AppendLine("Class: " + method.DeclaringType.Name);
                System.Reflection.ParameterInfo[] param = method.GetParameters();
                builder.AppendLine("Params Count: " + param.Length);
                int i = 1;
                foreach (System.Reflection.ParameterInfo p in param)
                {
                    builder.AppendLine("Param " + i + ":" + p.ParameterType.Name);
                    i++;
                }
                builder.AppendLine("<---Method End--->");
            }
            builder.AppendLine("<------------------------------StackTrace One End------------------------------>");
        }
#endif
        static readonly SemaphoreSlim lockWaitToRead = new SemaphoreSlim(1);
        /// <summary>
        /// log text message
        /// </summary>
        /// <param name="text">text to log</param>
        /// <param name="stacktrace">log stacktrace</param>
#if (NET35 || NET40)
        public void LogText(string text, bool stacktrace = false)
#else
        public async void LogText(string text, bool stacktrace = false)
#endif
        {
            if (string.IsNullOrEmpty(DirectoryLocation) || !IsEnabled)
            {
                return;
            }
            StringBuilder str = new StringBuilder();
            str.AppendLine($"<Text Log Start> {DateTime.Now} {DateTime.Now.Ticks}");
            str.AppendLine(text);
            if (stacktrace)
            {
                str.AppendLine("<StackTrace>");
                StringBuilder builder = new StringBuilder();
#if (NETSTANDARD || NETCOREAPP)
                GetOneStackTraceText(new StackTrace(new Exception(text), true), builder);
#else
                GetOneStackTraceText(new StackTrace(true), builder);
#endif
                str.AppendLine(builder.ToString());

                str.AppendLine("</StackTrace>");
            }
            str.AppendLine("<Text Log End>");
#if (NET35 || NET40)
            GenerateSavePath();
#else
            await GenerateSavePath().ConfigureAwait(false);
#endif

            try
            {
#if (NET35 || NET40)
                lockWaitToRead.Wait();
#else
                await lockWaitToRead.WaitAsync().ConfigureAwait(false);
#endif
                string fileName = SavePath;
                using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    stream.Seek(0, SeekOrigin.End);
                    byte[] bytes = Encoding.UTF8.GetBytes(Helpers.TextHelper.NewLine + str.ToString());
#if (NET35 || NET40)
                    stream.Write(bytes, 0, bytes.Length);
#else
                    await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
#endif
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"for file {SavePath}" + ex);
            }
            finally
            {
                lockWaitToRead.Release();
            }
        }


        /// <summary>
        /// log an exception to file
        /// </summary>
        /// <param name="e">exception of log</param>
        /// <param name="title">title of log</param>
#if (NET35 || NET40)
        public void LogError(Exception e, string title)
#else
        public async void LogError(Exception e, string title)
#endif
        {
            if (string.IsNullOrEmpty(DirectoryLocation) || !IsEnabled)
                return;
            try
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine(title);
                str.AppendLine(e.ToString());
                str.AppendLine("Time : " + DateTime.Now.ToLocalTime().ToString());
                str.AppendLine("--------------------------------------------------------------------------------------------------");
                str.AppendLine("--------------------------------------------------------------------------------------------------");
#if (NET35 || NET40)
                GenerateSavePath();
#else
                await GenerateSavePath().ConfigureAwait(false);
#endif

                try
                {
#if (NET35 || NET40)
                    lockWaitToRead.Wait();
#else
                    await lockWaitToRead.WaitAsync().ConfigureAwait(false);
#endif
                    string fileName = SavePath;
                    using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        stream.Seek(0, SeekOrigin.End);
                        byte[] bytes = Encoding.UTF8.GetBytes(Helpers.TextHelper.NewLine + str.ToString());
#if (NET35 || NET40)
                        stream.Write(bytes, 0, bytes.Length);
#else
                        await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
#endif

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"for file {SavePath}" + ex);
                }
                finally
                {
                    lockWaitToRead.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
