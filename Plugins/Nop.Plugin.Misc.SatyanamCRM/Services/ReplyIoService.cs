using Newtonsoft.Json;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// ReplyIo service
    /// </summary>
    public partial class ReplyIoService : IReplyIoService
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "O5zvqbTAaHHJnwKrHlUfxIPH";
        #endregion

        #region Ctor

        public ReplyIoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #endregion

        #region Methods

        #region ReplyIo

        public async Task<bool> CreateOrUpdateLeadAsync(ReplyLeadDto lead)
        {
            try
            {
                var json = JsonConvert.SerializeObject(lead);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.reply.io/v1/people", content);
                var resultContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Reply.io API error: {response.StatusCode} - {resultContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception syncing to Reply.io: {ex.Message}");
                throw;
            }
        }

        #endregion
        #endregion
    }
}