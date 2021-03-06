﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Kontur.ImageTransformer
{
    internal class HttpServer:IDisposable
    {
        private readonly HttpListener Listener;
        private volatile bool IsRunning;
        private Thread ListenerThread;
        private bool Disposed;
        private ImageConverter ImageConverter;
        public HttpServer()
        {
            Listener = new HttpListener();
            IsRunning = false;
            Disposed = false;
            ImageConverter = new ImageConverter();
        }
        public void Start(string prefix)
        {
            lock (Listener)
            {
                if (!IsRunning)
                {
                    Listener.Prefixes.Clear();
                    Listener.Prefixes.Add(prefix);
                    Listener.Start();
                    ListenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    ListenerThread.Start();
                    IsRunning = true;
                }
            }
        }
        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (Listener.IsListening)
                    {
                        var context = Listener.GetContext();
                        Task.Run(() => HandleContext(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }
        private bool IsCorrectFilter(string filterName)
        {
            if (filterName == "grayscale")
                return true;
            if (filterName == "sepia")
                return true;
            int i = filterName.IndexOf('(');
            if (filterName.Substring(0, i) != "threshold")
                return false;
            int j = filterName.IndexOf(')');
            byte parameter;
            if (!Byte.TryParse(filterName.Substring(i + 1, j - i - 1), out parameter))
                return false;
            if ((parameter < 0) || (parameter > 100))
                return false;
            if (filterName.Length > j+1)
                return false;
            return true;
        }
        private void HandleContext(HttpListenerContext listenerContext)
        {
            var sw = new Stopwatch();
            sw.Start();
            const int maxImageSize = 102400;//максимальный размер изображения в байтах
            const int maxTime = 1000;//максимальное допустимое время обработки запроса в миллисекундах
            var request = listenerContext.Request;
            string filterName;
            int[] frameParameters=new int[4];
            string inputImage, resultImage;
            bool isTimeout;
            try
            {
                if (request.HttpMethod != "POST")
                    throw new ArgumentException();
                var requestAbsolutePath = request.Url.AbsolutePath;
                string[] urlParameters = requestAbsolutePath.Split('/');
                if (urlParameters.Length != 4)
                    throw new ArgumentException();
                if (urlParameters[0] != "")
                    throw new ArgumentException();
                if (urlParameters[1] != "process")
                    throw new ArgumentException();
                filterName = urlParameters[2];
                if (!IsCorrectFilter(filterName))
                    throw new ArgumentException();
                string[] strFrameParameters = urlParameters[3].Split(',');
                if (strFrameParameters.Length != 4)
                    throw new ArgumentException();
                for (int i = 0; i < strFrameParameters.Length; i++)
                    if(!Int32.TryParse(strFrameParameters[i], out frameParameters[i]))
                        throw new ArgumentException();
                if(request.ContentLength64 > maxImageSize)
                    throw new ArgumentException();
                var requestBodyStream = request.InputStream;
                using (var reader = new StreamReader(requestBodyStream))
                    inputImage = reader.ReadToEnd();
                resultImage = ImageConverter.Convert(inputImage, filterName, frameParameters[0], 
                    frameParameters[1], frameParameters[2], frameParameters[3], sw, maxTime, out isTimeout);
            }
            catch(ArgumentException)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    writer.Write("");
                return;
            }
            if(isTimeout)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    writer.Write("");
                return;
            }
            if (resultImage == null)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    writer.Write("");
                return;
            }
            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                writer.Write(resultImage);
        }
        public void Stop()
        {
            lock (Listener)
            {
                if (!IsRunning)
                    return;
                Listener.Stop();
                ListenerThread.Abort();
                ListenerThread.Join();
                IsRunning = false;
            }
        }
        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            Stop();
            Listener.Close();
        }
    }
}