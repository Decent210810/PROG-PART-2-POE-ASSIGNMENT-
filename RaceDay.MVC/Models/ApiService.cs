using System.Text;
using Newtonsoft.Json;

namespace RaceDay.MVC.Models
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7163/");
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? sessionCookie = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            if (!string.IsNullOrEmpty(sessionCookie)) request.Headers.Add("Cookie", sessionCookie);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            return default;
        }

        public async Task<HttpResponseMessage> PostAsync(string endpoint, object data, string? sessionCookie = null)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            if (!string.IsNullOrEmpty(sessionCookie)) request.Headers.Add("Cookie", sessionCookie);
            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PutAsync(string endpoint, object data, string? sessionCookie = null)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint) { Content = content };
            if (!string.IsNullOrEmpty(sessionCookie)) request.Headers.Add("Cookie", sessionCookie);
            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, string? sessionCookie = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            if (!string.IsNullOrEmpty(sessionCookie)) request.Headers.Add("Cookie", sessionCookie);
            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PostMultipartAsync(string endpoint, MultipartFormDataContent content, string? sessionCookie = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            if (!string.IsNullOrEmpty(sessionCookie)) request.Headers.Add("Cookie", sessionCookie);
            return await _httpClient.SendAsync(request);
        }
    }
}