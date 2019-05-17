using Dotnet.Container.RegistryClient.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Dotnet.Container.RegistryClient
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

        public async Task<ConfigurationManifest> GetConfigBlobAsync(string name, string reference)
        {
            var uri = new Uri(_registryUri, $"/v2/{name}/blobs/{reference}");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.AddBasicAuthorizationHeader(_username, _password);
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseJObject = JObject.Parse(responseString);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw CreateException(responseJObject);
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw CreateException(responseJObject);
            }
            return responseJObject.ToObject<ConfigurationManifest>();
        }

        public async Task<long> PutConfigBlobAsync(string name, string config, string configDigest)
        {
            var uri = new Uri(_registryUri, $"/v2/{name}/blobs/uploads/");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.AddBasicAuthorizationHeader(_username, _password);
            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                throw new RegistryException();
            }
            var location = response.Headers.GetValues("Location").FirstOrDefault();
            uri = new Uri(_registryUri, $"{location}&digest={configDigest}");
            Console.WriteLine("Pusing config to " + uri.ToString());
            request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(config);
            var returnVal = request.Content.Headers.ContentLength.Value;
            request.AddBasicAuthorizationHeader(_username, _password);
            response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new RegistryException();
            }
            
            return returnVal;
        }

        public async Task<(long, string)> PostConfigBlobAsync(string name, ConfigurationManifest config)
        {
            // This operation makes two calls:

            // 1. POST to create a bucket to place the config
            var uri = new Uri(_registryUri, $"/v2/{name}/blobs/uploads/");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.AddBasicAuthorizationHeader(_username, _password);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new RegistryException();
            }

            // 2. POST again with actual contents of the config
            var location = response.Headers.GetValues("Location").FirstOrDefault();
            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.None);
            var configDigest = SHAHelpers.ComputeSHA256(serializedConfig);
            uri = new Uri(_registryUri, $"{location}&digest={configDigest}");
            Console.WriteLine("Pusing config to " + uri.ToString());
            request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(serializedConfig);
            var configSize = request.Content.Headers.ContentLength.Value;
            request.AddBasicAuthorizationHeader(_username, _password);
            response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new RegistryException();
            }

            return (configSize, configDigest);
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

        public async Task PutManifestAsync(string name, string reference, Manifest manifest)
        {
            var uri = new Uri(_registryUri, $"/v2/{name}/manifests/{reference}");
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            var jsonContent = JsonConvert.SerializeObject(manifest);
            request.Content = new StringContent(jsonContent);
            request.Content.Headers.ContentType.MediaType = manifest.MediaType;
            request.AddBasicAuthorizationHeader(_username, _password);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("ERR PutManifest: " + response.ReasonPhrase);
                var responseJObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                throw CreateException(responseJObject);
            }
            return;

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
