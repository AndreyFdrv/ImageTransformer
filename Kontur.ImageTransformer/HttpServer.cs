using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Kontur.ImageTransformer
{
    internal class HttpServer:IDisposable
    {
        private readonly HttpListener Listener;
        private volatile bool IsRunning;
        private Thread ListenerThread;
        private bool Disposed;
        public HttpServer()
        {
            Listener = new HttpListener();
            IsRunning = false;
            Disposed = false;
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
        private void HandleContext(HttpListenerContext listenerContext)
        {
            var request = listenerContext.Request;
            var requestAbsolutePath = request.Url.AbsolutePath;
            var requestHttpMethod = request.HttpMethod;
            var requestContentType = request.ContentType;
            var requestBodyStream = request.InputStream;
            string image;
            using (var reader = new StreamReader(requestBodyStream))
                image = reader.ReadToEnd();
            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                writer.Write(image);
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