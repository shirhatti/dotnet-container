using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_container.RegistryTypes
{
    public class Registry
    {
        private readonly Uri _registryUri;
        private readonly HttpClient _httpClient;
        private readonly string _username;
        private readonly string _password;

        public Registry(Uri registryUri, string username, string password)
        {
            _httpClient = new HttpClient();
            _registryUri = registryUri;
            _username = username;
            _password = password;
        }

        public async Task<ApiVersion> GetApiVersionAsync()
        {
            var uri = new Uri(_registryUri, "/v2/");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.AddBasicAuthorizationHeader(_username, _password);
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new RegistryException(response.StatusCode.ToString());
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return ApiVersion.v2;
            }

            return ApiVersion.v1;
        }

        public async Task<string> GetDigestFromReference(string name, string reference, ManifestType manifestType)
        {
            var uri = new Uri(_registryUri, $"/v2/{name}/manifests/{reference}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(manifestType.MediaType));
            request.AddBasicAuthorizationHeader(_username, _password);
            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new RegistryException();
            }
            var digest = response.Headers.GetValues("Docker-Content-Digest").FirstOrDefault();
            return digest;
        }

        public async Task<Manifest> GetManifestAsync(string name, string reference, ManifestType manifestType)
        {
            var uri = new Uri(_registryUri, $"/v2/{name}/manifests/{reference}");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(manifestType.MediaType));
            request.AddBasicAuthorizationHeader(_username, _password);
            var response = await _httpClient.SendAsync(request);
            var responseJObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw CreateException(responseJObject);
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw CreateException(responseJObject);
            }
            return responseJObject.ToObject<Manifest>();
        }

        private static Exception CreateException(JObject httpResponse)
        {
            var exceptions = new List<Exception>();
            foreach (JObject error in (JArray)httpResponse["errors"])
            {
                exceptions.Add(new RegistryException((string)error["message"], (string)error["message"], (string)error["message"]));
            }
            if (exceptions.Count == 1)
            {
                return exceptions[0];
            }
            return new AggregateException(exceptions);
        }
    }
}
