using System;
using System.Net;
using RestSharp;

namespace Kontur.HttpClient
{
    internal class HttpClient
    {
        RestClient Client;
        public HttpClient(string connectionString)
        {
            Client = new RestClient(connectionString);
        }
        public string TransformImage(string url, string image, out HttpStatusCode statusCode)
        {
            var request = new RestRequest(url, Method.POST);
            request.AddParameter("text/xml", image, ParameterType.RequestBody);
            Client.Execute(request);
            var response = Client.Execute(request);
            statusCode = response.StatusCode;
            return response.Content;
        }
    }
}