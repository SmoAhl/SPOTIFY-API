using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace SpotifyAPI
{
    public partial class MainWindow : Window
    {
        private string clientId = "d58053c82c0445eb8c363bdca929e9c4";
        private string clientSecret = "46b4e7d33daa45578aa3491d70bc9b50";
        private TokenManager tokenManager;

        public MainWindow()
        {
            InitializeComponent();
            tokenManager = new TokenManager(clientId, clientSecret);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var accessToken = await tokenManager.GetAccessToken();
            var trackInfo = await GetTrackInfo(accessToken);
            showTrackInfo.Content = JsonConvert.SerializeObject(trackInfo, Formatting.Indented);
        }


        private async Task<dynamic> GetTrackInfo(string accessToken)
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/tracks/4cOdK2wGLETKBW3PvgPWqT");
            request.Headers.Add("Authorization", "Bearer " + accessToken);

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject(responseContent);
        }
    }

    class TokenManager
    {
        private string clientId;
        private string clientSecret;
        private string accessToken;
        private DateTime expiryTime;

        public TokenManager(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public async Task<string> GetAccessToken()
        {
            if (accessToken == null || DateTime.Now >= expiryTime)
            {
                await RefreshToken();
            }
            return accessToken;
        }

        private async Task RefreshToken()
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");

            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });
            request.Content = body;

            var base64Auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Add("Authorization", "Basic " + base64Auth);

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic responseData = JsonConvert.DeserializeObject(responseContent);

            accessToken = responseData.access_token;
            int expiresIn = responseData.expires_in;
            expiryTime = DateTime.Now.AddSeconds(expiresIn);
        }
    }
}