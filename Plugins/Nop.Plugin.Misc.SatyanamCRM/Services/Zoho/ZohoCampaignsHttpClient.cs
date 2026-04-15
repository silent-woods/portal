using App.Services.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services.Zoho
{
    public partial class ZohoCampaignsHttpClient
    {
        #region Fields

        private readonly HttpClient           _httpClient;
        private readonly ILocalizationService _localizationService;

        private const string BaseUrl  = "https://campaigns.zoho.in/api/v1.1/";
        private const string TokenUrl = "https://accounts.zoho.in/oauth/v2/token";

        #endregion

        #region Ctor

        public ZohoCampaignsHttpClient(HttpClient httpClient, ILocalizationService localizationService)
        {
            _httpClient          = httpClient;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        private static Dictionary<string, string> ParseFlElements(XElement parent)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (parent == null)
                return result;
            foreach (var el in parent.Elements("fl"))
            {
                var key = el.Attribute("val")?.Value;
                if (!string.IsNullOrEmpty(key))
                    result[key] = el.Value;
            }
            return result;
        }

        #endregion

        #region Methods

        public virtual async Task<string> RefreshAccessTokenAsync(string clientId, string clientSecret, string refreshToken)
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type",    "refresh_token"),
                new KeyValuePair<string, string>("client_id",     clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
            });

            var response = await _httpClient.PostAsync(TokenUrl, form);
            var body     = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(string.Format(
                    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Error.TokenRefreshHttp"),
                    (int)response.StatusCode, body));

            using var doc = System.Text.Json.JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("error", out var errProp))
                throw new InvalidOperationException(string.Format(
                    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Error.TokenRefreshZohoError"),
                    errProp.GetString(), body));

            if (!doc.RootElement.TryGetProperty("access_token", out var tokenProp))
                throw new InvalidOperationException(string.Format(
                    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Error.TokenRefreshNoToken"),
                    body));

            return tokenProp.GetString();
        }

        public virtual async Task<(List<Dictionary<string, string>> campaigns, string rawResponse)> GetRecentCampaignsRawAsync(string accessToken, int limit)
        {
            var url = $"{BaseUrl}recentcampaigns?resfmt=json&range={limit}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Authorization", $"Zoho-oauthtoken {accessToken}");

            var response = await _httpClient.SendAsync(request);
            var body     = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(string.Format(
                    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Error.RecentCampaignsHttp"),
                    (int)response.StatusCode, body));

            var results = new List<Dictionary<string, string>>();
            using var doc = System.Text.Json.JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("recent_campaigns", out var arr)
                && arr.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var entry = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var prop in item.EnumerateObject())
                        entry[prop.Name] = prop.Value.ToString();
                    results.Add(entry);
                }
            }

            return (results, body);
        }

        public virtual async Task<List<Dictionary<string, string>>> GetRecentCampaignsAsync(string accessToken, int limit)
        {
            var (campaigns, _) = await GetRecentCampaignsRawAsync(accessToken, limit);
            return campaigns;
        }

        public virtual async Task<(Dictionary<string, string> reports, Dictionary<string, string> details, Dictionary<string, int> locations, string rawResponse)>
            GetCampaignReportRawAsync(string accessToken, string campaignKey)
        {
            var url = $"{BaseUrl}getcampaignreports?resfmt=xml&campaignkey={Uri.EscapeDataString(campaignKey)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Authorization", $"Zoho-oauthtoken {accessToken}");

            var response = await _httpClient.SendAsync(request);
            var xml      = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(string.Format(
                    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Error.CampaignReportHttp"),
                    (int)response.StatusCode, xml));

            var doc  = XDocument.Parse(xml);
            var root = doc.Root;

            var reports = ParseFlElements(root?.Element("campaign-reports"));
            var details = ParseFlElements(root?.Element("campaign-details"));

            var locations = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var locEl = root?.Element("campaign-by-location");
            if (locEl != null)
            {
                foreach (var el in locEl.Elements("fl"))
                {
                    var country = el.Attribute("val")?.Value;
                    if (!string.IsNullOrEmpty(country) && int.TryParse(el.Value, out var count))
                        locations[country] = count;
                }
            }

            return (reports, details, locations, xml);
        }

        public virtual async Task<(Dictionary<string, string> reports, Dictionary<string, string> details, Dictionary<string, int> locations)>
            GetCampaignReportAsync(string accessToken, string campaignKey)
        {
            var (reports, details, locations, _) = await GetCampaignReportRawAsync(accessToken, campaignKey);
            return (reports, details, locations);
        }

        public virtual async Task<(Dictionary<DateTime, int> dailyCounts, List<(string email, DateTime? firstAt, int count)> recipients)>
            GetRecipientsDataAsync(string accessToken, string campaignKey, string action)
        {
            var dailyCounts = new Dictionary<DateTime, int>();
            var recipients  = new List<(string email, DateTime? firstAt, int count)>();

            // Zoho time formats:
            //   Opens/clicks reports: "Feb 12, 2026 12:08 PM"  (MMM d, yyyy h:mm tt)
            //   Bounce bouncedDate:   "12 Feb 2026, 12:01 PM"  (d MMM yyyy, h:mm tt)
            var timeFormats = new[]
            {
                "MMM d, yyyy h:mm tt",
                "MMM dd, yyyy h:mm tt",
                "MMM d, yyyy hh:mm tt",
                "MMM dd, yyyy hh:mm tt",
                "MMM d, yyyy h:mm:ss tt",
                "MMM dd, yyyy hh:mm:ss tt",
                "d MMM yyyy, h:mm tt",
                "dd MMM yyyy, h:mm tt",
                "d MMM yyyy, hh:mm tt",
                "dd MMM yyyy, hh:mm tt",
            };

            var typeParam = action == "clickedcontacts" ? "&type=click_timeline" : "&type=open_timeline";
            var pageSize  = 20;
            var fromIndex = 1;
            var hasMore   = true;

            while (hasMore)
            {
                var url = $"{BaseUrl}getcampaignrecipientsdata?resfmt=xml"
                        + $"&campaignkey={Uri.EscapeDataString(campaignKey)}"
                        + $"&action={action}"
                        + typeParam
                        + $"&range={pageSize}&fromindex={fromIndex}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Authorization", $"Zoho-oauthtoken {accessToken}");

                var response = await _httpClient.SendAsync(request);
                var body     = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(string.Format(
                        await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Error.RecipientsDataHttp"),
                        action, (int)response.StatusCode, body));

                var xdoc        = XDocument.Parse(body);
                var root        = xdoc.Root;
                var contactList = root?.Element("contacts")?.Elements("contact").ToList()
                               ?? new List<XElement>();

                if (!contactList.Any())
                {
                    hasMore = false;
                    break;
                }

                foreach (var contact in contactList)
                {
                    var email = contact.Elements("fl")
                        .FirstOrDefault(fl => fl.Attribute("val")?.Value == "contact_email")?.Value;

                    var reportEls = contact.Element("reports")?.Elements("report").ToList();
                    var isBounce  = reportEls == null; 

                    DateTime? firstAt    = null;
                    var       eventCount = 0;

                    if (isBounce)
                    {
                        var dateField = action == "optoutcontacts" ? "optOutTime" : "bouncedDate";
                        var timeStr = contact.Elements("fl")
                            .FirstOrDefault(fl => fl.Attribute("val")?.Value == dateField)?.Value;

                        DateTime? ts = null;
                        if (!string.IsNullOrWhiteSpace(timeStr))
                        {
                            if (DateTime.TryParseExact(timeStr.Trim(), timeFormats,
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                                ts = parsed;
                            else if (DateTime.TryParse(timeStr.Trim(), CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var fallback))
                                ts = fallback;
                        }

                        var day = ts.HasValue ? ts.Value.Date : DateTime.UtcNow.Date;
                        if (!dailyCounts.ContainsKey(day))
                            dailyCounts[day] = 0;
                        dailyCounts[day] += 1;

                        firstAt    = ts;
                        eventCount = 1;
                    }
                    else
                    {
                        foreach (var report in reportEls)
                        {
                            var timeStr = report.Elements("fl")
                                .FirstOrDefault(fl => fl.Attribute("val")?.Value == "time")?.Value;

                            DateTime? ts = null;
                            if (!string.IsNullOrWhiteSpace(timeStr))
                            {
                                if (DateTime.TryParseExact(timeStr.Trim(), timeFormats,
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                                    ts = parsed;
                                else if (DateTime.TryParse(timeStr.Trim(), CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var fallback))
                                    ts = fallback;
                            }

                            var day = ts.HasValue ? ts.Value.Date : DateTime.UtcNow.Date;
                            if (!dailyCounts.ContainsKey(day))
                                dailyCounts[day] = 0;
                            dailyCounts[day] += 1;

                            if (ts.HasValue && (!firstAt.HasValue || ts.Value < firstAt.Value))
                                firstAt = ts;

                            eventCount++;
                        }
                    }

                    if (!string.IsNullOrEmpty(email) && eventCount > 0)
                        recipients.Add((email.ToLowerInvariant(), firstAt, eventCount));
                }

                if (contactList.Count < pageSize)
                    hasMore = false;
                else
                    fromIndex += pageSize;
            }

            return (dailyCounts, recipients);
        }

        #endregion
    }
}
