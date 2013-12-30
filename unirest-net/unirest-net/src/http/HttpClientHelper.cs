﻿using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System;

using unirest_net.request;

namespace unirest_net.http
{
    public class HttpClientHelper
    {
        private const string USER_AGENT = "unirest-net/1.0";

        public static HttpResponse<T> Request<T>(HttpRequest request)
        {
            var responseTask = RequestHelper(request);
            Task.WaitAll(responseTask);
            var response = responseTask.Result;

            return new HttpResponse<T>(response);
        }

        public static Task<HttpResponse<T>> RequestAsync<T>(HttpRequest request)
        {
            var responseTask = RequestHelper(request);
            return Task<HttpResponse<T>>.Factory.StartNew(() =>
            {
                Task.WaitAll(responseTask);
                return new HttpResponse<T>(responseTask.Result);
            });
        }

        private static Task<HttpResponseMessage> RequestHelper(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("user-agent"))
            {
                request.Headers.Add("user-agent", USER_AGENT);
            }

            var client = new HttpClient();
            var msg = new HttpRequestMessage(request.HttpMethod, request.URL);

            if (request.NetworkCredentials != null)
            {
                string authToken = Convert.ToBase64String(
                                    UTF8Encoding.UTF8.GetBytes(string.Format("{0}:{1}",
                                        request.NetworkCredentials.UserName,
                                        request.NetworkCredentials.Password))
                                    );

                string authValue = string.Format("Basic {0}", authToken);

                request.Headers.Add("Authorization", authValue);
            }

            foreach (var header in request.Headers)
            {
                msg.Headers.Add(header.Key, header.Value);
            }

            if (request.Body.Any())
            {
                msg.Content = request.Body;
            }

            return client.SendAsync(msg);
        }
    }
}
