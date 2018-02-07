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
            if (filterName.Substring(0, i + 1) != "threshold")
                return false;
            int j = filterName.IndexOf(')');
            UInt16 parameter;
            if (!UInt16.TryParse(filterName.Substring(i + 1, j - i - 1), out parameter))
                return false;
            if ((parameter < 0) || (parameter > 100))
                return false;
            if (filterName.Length > j+1)
                return false;
            return true;
        }
        private void HandleContext(HttpListenerContext listenerContext)
        {
            int maxImageSize = 102400;
            var request = listenerContext.Request;
            string filterName;
            int[] frameParameters=new int[4];
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
            }
            catch(ArgumentException)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    writer.Write("");
                return;
            }
            var requestBodyStream = request.InputStream;
            string image;
            using (var reader = new StreamReader(requestBodyStream))
                image = reader.ReadToEnd();
            ImageConverter.Convert(ref image, filterName, frameParameters[0], frameParameters[1], frameParameters[2], frameParameters[3]);
            if (image == null)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    writer.Write("");
                return;
            }
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