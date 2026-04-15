using App.Data;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Logging;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Core.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services.Zoho
{
    public partial class ZohoCampaignService : IZohoCampaignService
    {
        #region Fields

        private readonly IRepository<ZohoCampaignStat>         _statRepository;
        private readonly IRepository<ZohoCampaignLocationStat> _locationRepository;
        private readonly ISettingService                        _settingService;
        private readonly ZohoCampaignsHttpClient                _httpClient;
        private readonly ILogger                                _logger;
        private readonly ILocalizationService                   _localizationService;
        private IRepository<ZohoCampaignDailyStat> _dailyStatRepository;
        private IRepository<ZohoCampaignRecipient> _recipientRepository;
        private IRepository<Lead>                  _leadRepository;
        private IRepository<Contacts>              _contactRepository;
        private readonly IServiceProvider          _serviceProvider;
        private ILeadService                       _leadService;

        private static readonly string[] SentTimeFmts = new[]
        {
            "d MMM yyyy, h:mm tt",
            "d MMM yyyy, hh:mm tt"
        };

        #endregion

        #region Ctor

        public ZohoCampaignService(
            IRepository<ZohoCampaignStat>         statRepository,
            IRepository<ZohoCampaignLocationStat> locationRepository,
            ISettingService                        settingService,
            ZohoCampaignsHttpClient                httpClient,
            ILogger                                logger,
            ILocalizationService                   localizationService,
            IServiceProvider                       serviceProvider)
        {
            _statRepository      = statRepository;
            _locationRepository  = locationRepository;
            _settingService      = settingService;
            _httpClient          = httpClient;
            _logger              = logger;
            _localizationService = localizationService;
            _serviceProvider     = serviceProvider;
        }

        #endregion

        #region Utilities

        private IRepository<ZohoCampaignDailyStat> DailyRepo
            => _dailyStatRepository ??= (IRepository<ZohoCampaignDailyStat>)_serviceProvider.GetService(typeof(IRepository<ZohoCampaignDailyStat>));

        private IRepository<ZohoCampaignRecipient> RecipientRepo
            => _recipientRepository ??= (IRepository<ZohoCampaignRecipient>)_serviceProvider.GetService(typeof(IRepository<ZohoCampaignRecipient>));

        private IRepository<Lead> LeadRepo
            => _leadRepository ??= (IRepository<Lead>)_serviceProvider.GetService(typeof(IRepository<Lead>));

        private IRepository<Contacts> ContactRepo
            => _contactRepository ??= (IRepository<Contacts>)_serviceProvider.GetService(typeof(IRepository<Contacts>));

        private ILeadService LeadSvc
            => _leadService ??= (ILeadService)_serviceProvider.GetService(typeof(ILeadService));

        private static DateTime? ParseZohoDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (DateTime.TryParseExact(value.Trim(), SentTimeFmts,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            return null;
        }

        private static int     ToInt(Dictionary<string, string> d, string key)
            => d.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : 0;

        private static decimal ToDec(Dictionary<string, string> d, string key)
            => d.TryGetValue(key, out var v) && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ? dec : 0m;

        private static string  ToStr(Dictionary<string, string> d, string key)
            => d.TryGetValue(key, out var v) ? v : null;

        #endregion

        #region Methods

        public virtual async Task<string> GetValidAccessTokenAsync()
        {
            var settings = await _settingService.LoadSettingAsync<ZohoCampaignSettings>();

            await _logger.InformationAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.CheckingTokenValidity"));

            if (string.IsNullOrEmpty(settings.ClientId) || string.IsNullOrEmpty(settings.ClientSecret) || string.IsNullOrEmpty(settings.RefreshToken))
            {
                await _logger.ErrorAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.CredentialsMissing"), null);
                throw new InvalidOperationException(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.CredentialsNotConfigured"));
            }

            if (!string.IsNullOrEmpty(settings.AccessToken)
                && settings.AccessTokenExpiresUtc.HasValue
                && settings.AccessTokenExpiresUtc.Value > DateTime.UtcNow.AddMinutes(5))
            {
                await _logger.InformationAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.TokenReused"));
                return settings.AccessToken;
            }

            await _logger.InformationAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.TokenRefreshing"));

            string newToken;
            try
            {
                newToken = await _httpClient.RefreshAccessTokenAsync(
                    settings.ClientId, settings.ClientSecret, settings.RefreshToken);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.TokenRefreshFailed"), ex);
                throw;
            }

            if (string.IsNullOrEmpty(newToken))
            {
                await _logger.ErrorAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.TokenRefreshEmptyToken"), null);
                throw new InvalidOperationException(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.TokenRefreshEmptyException"));
            }

            await _logger.InformationAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.TokenRefreshed"));
            settings.AccessToken           = newToken;
            settings.AccessTokenExpiresUtc = DateTime.UtcNow.AddSeconds(3600);
            await _settingService.SaveSettingAsync(settings);

            return newToken;
        }

        public virtual async Task SyncAsync()
        {
            await _logger.InformationAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.SyncStarted"));

            var settings = await _settingService.LoadSettingAsync<ZohoCampaignSettings>();
            if (!settings.IsEnabled)
            {
                await _logger.InformationAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.SyncSkippedDisabled"));
                return;
            }

            string token;
            try
            {
                token = await GetValidAccessTokenAsync();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.SyncAbortedNoToken"), ex);
                return;
            }

            await _logger.InformationAsync(string.Format(
                await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.FetchingCampaigns"),
                settings.CampaignFetchLimit));

            List<Dictionary<string, string>> campaigns;
            try
            {
                var (cList, rawCampaigns) = await _httpClient.GetRecentCampaignsRawAsync(token, settings.CampaignFetchLimit);
                campaigns = cList;
                await _logger.InformationAsync(string.Format(
                    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.RawCampaignsResponse"),
                    rawCampaigns?.Substring(0, Math.Min(2000, rawCampaigns?.Length ?? 0))));
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.FetchCampaignsFailed"), ex);
                return;
            }

            await _logger.InformationAsync(string.Format(
                await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.CampaignsReceived"),
                campaigns.Count));

            if (campaigns.Count == 0)
            {
                await _logger.InformationAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.NoCampaigns"));
                settings.LastSyncedUtc = DateTime.UtcNow;
                await _settingService.SaveSettingAsync(settings);
                return;
            }

            var unknownName = await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.UnknownCampaignName");

            foreach (var c in campaigns)
            {
                var key          = ToStr(c, "campaign_key") ?? ToStr(c, "campaignkey");
                var campaignName = ToStr(c, "campaign_name") ?? ToStr(c, "campaignname") ?? unknownName;

                if (string.IsNullOrEmpty(key))
                {
                    await _logger.WarningAsync(string.Format(
                        await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.CampaignNoKey"),
                        campaignName, string.Join(", ", c.Keys)));
                    continue;
                }

                await _logger.InformationAsync(string.Format(
                    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.ProcessingCampaign"),
                    campaignName, key));

                try
                {
                    Dictionary<string, string> reports, details;
                    Dictionary<string, int>    locations;
                    string rawReport;
                    try
                    {
                        (reports, details, locations, rawReport) = await _httpClient.GetCampaignReportRawAsync(token, key);
                        await _logger.InformationAsync(string.Format(
                            await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.RawReportResponse"),
                            key, rawReport?.Substring(0, Math.Min(2000, rawReport?.Length ?? 0))));
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync(string.Format(
                            await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.ReportFetchFailed"),
                            key), ex);
                        continue;
                    }

                    await _logger.InformationAsync(string.Format(
                        await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.ReportParsed"),
                        campaignName, reports.Count, details.Count, locations.Count));

                    if (reports.Count == 0)
                        await _logger.WarningAsync(string.Format(
                            await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.ReportsEmpty"),
                            key));

                    if (details.Count == 0)
                        await _logger.WarningAsync(string.Format(
                            await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.DetailsEmpty"),
                            key));

                    var existing = _statRepository.Table.FirstOrDefault(s => s.CampaignKey == key);
                    var stat = existing ?? new ZohoCampaignStat { CampaignKey = key };

                    stat.CampaignName         = ToStr(details, "campaign_name") ?? campaignName;
                    stat.EmailSubject         = ToStr(details, "email_subject");
                    stat.EmailFrom            = ToStr(details, "email_from");
                    stat.SentTime             = ParseZohoDate(ToStr(details, "sent_time"));
                    stat.CreatedTime          = ParseZohoDate(ToStr(details, "created_time"));

                    stat.EmailsSentCount      = ToInt(reports, "emails_sent_count");
                    stat.DeliveredCount       = ToInt(reports, "delivered_count");
                    stat.DeliveredPercent     = ToDec(reports, "delivered_percent");
                    stat.OpensCount           = ToInt(reports, "opens_count");
                    stat.OpenPercent          = ToDec(reports, "open_percent");
                    stat.UnopenedCount        = ToInt(reports, "unopened");
                    stat.UniqueClicksCount    = ToInt(reports, "unique_clicks_count");
                    stat.UniqueClickedPercent = ToDec(reports, "unique_clicked_percent");
                    stat.ClicksPerOpenRate    = ToDec(reports, "clicksperopenrate");
                    stat.BouncesCount         = ToInt(reports, "bounces_count");
                    stat.HardBounceCount      = ToInt(reports, "hardbounce_count");
                    stat.SoftBounceCount      = ToInt(reports, "softbounce_count");
                    stat.BouncePercent        = ToDec(reports, "bounce_percent");
                    stat.UnsubCount           = ToInt(reports, "unsub_count");
                    stat.UnsubscribePercent   = ToDec(reports, "unsubscribe_percent");
                    stat.SpamsCount           = ToInt(reports, "spams_count");
                    stat.ComplaintsCount      = ToInt(reports, "complaints_count");
                    stat.ForwardsCount        = ToInt(reports, "forwards_count");
                    stat.AutoreplyCount       = ToInt(reports, "autoreply_count");
                    stat.LastSyncedUtc        = DateTime.UtcNow;

                    if (existing == null)
                    {
                        await _statRepository.InsertAsync(stat);
                        await _logger.InformationAsync(string.Format(
                            await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.StatInserted"),
                            stat.CampaignName));
                    }
                    else
                    {
                        await _statRepository.UpdateAsync(stat);
                        await _logger.InformationAsync(string.Format(
                            await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.StatUpdated"),
                            stat.CampaignName));
                    }

                    var existingLocs = _locationRepository.Table.Where(l => l.CampaignKey == key).ToList();
                    foreach (var loc in existingLocs)
                        await _locationRepository.DeleteAsync(loc);

                    foreach (var kv in locations)
                    {
                        await _locationRepository.InsertAsync(new ZohoCampaignLocationStat
                        {
                            CampaignKey = key,
                            Country     = kv.Key,
                            OpensCount  = kv.Value
                        });
                    }

                    await _logger.InformationAsync(string.Format(
                        await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.LocationsSaved"),
                        locations.Count, stat.CampaignName));

                    // Sync per-recipient daily stats (opens/clicks/bounces timeline)
                    await SyncRecipientDataAsync(key, token);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(string.Format(
                        await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.CampaignProcessError"),
                        key), ex);
                }
            }

            settings.LastSyncedUtc = DateTime.UtcNow;
            await _settingService.SaveSettingAsync(settings);
            await _logger.InformationAsync(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.SyncCompleted"));
        }

        public virtual async Task<IList<ZohoCampaignStat>> GetStoredStatsAsync(int limit = 10)
        {
            var results = _statRepository.Table
                .Where(s => s.SentTime != null && s.EmailsSentCount > 0)
                .OrderByDescending(s => s.SentTime)
                .Take(limit)
                .ToList();

            await _logger.InformationAsync(string.Format(
                await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.StoredStatsReturned"),
                results.Count));
            return results;
        }

        public virtual Task<IList<ZohoCampaignLocationStat>> GetLocationStatsByKeyAsync(string campaignKey)
        {
            IList<ZohoCampaignLocationStat> results = _locationRepository.Table
                .Where(l => l.CampaignKey == campaignKey)
                .OrderByDescending(l => l.OpensCount)
                .ToList();
            return Task.FromResult(results);
        }

        public virtual async Task SyncRecipientDataAsync(string campaignKey)
            => await SyncRecipientDataAsync(campaignKey, null);

        public virtual async Task SyncRecipientDataAsync(string campaignKey, string accessToken)
        {
            await _logger.InformationAsync(string.Format(
                await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.FetchingTimeline"),
                campaignKey));

            string token;
            if (!string.IsNullOrEmpty(accessToken))
            {
                token = accessToken;
            }
            else
            {
                try { token = await GetValidAccessTokenAsync(); }
                catch { return; }
            }

            var merged = new Dictionary<DateTime, (int opens, int clicks, int bounces)>();

            async Task FetchAndMerge(string action, Action<Dictionary<DateTime, (int opens, int clicks, int bounces)>, DateTime, int> apply)
            {
                try
                {
                    var (daily, recipients) = await _httpClient.GetRecipientsDataAsync(token, campaignKey, action);

                    foreach (var kv in daily)
                    {
                        if (!merged.ContainsKey(kv.Key))
                            merged[kv.Key] = (0, 0, 0);
                        apply(merged, kv.Key, kv.Value);
                    }

                    await UpsertRecipientsAsync(campaignKey, action, recipients);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(string.Format(
                        await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.TimelineFetchFailed"),
                        action, campaignKey), ex);
                }
            }

            await FetchAndMerge("openedcontacts",       (m, d, v) => { var (op, cl, bo) = m[d]; m[d] = (op + v, cl, bo); });
            await FetchAndMerge("clickedcontacts",      (m, d, v) => { var (op, cl, bo) = m[d]; m[d] = (op, cl + v, bo); });
            await FetchAndMerge("senthardbounce",       (m, d, v) => { var (op, cl, bo) = m[d]; m[d] = (op, cl, bo + v); });
            await FetchAndMerge("sentsoftbounce",       (m, d, v) => { var (op, cl, bo) = m[d]; m[d] = (op, cl, bo + v); });
            await FetchAndMerge("optoutcontacts", (m, d, v) => { /* opt-outs don't affect daily stats */ });

            var now = DateTime.UtcNow;
            var oldDaily = DailyRepo.Table.Where(d => d.CampaignKey == campaignKey).ToList();
            foreach (var old in oldDaily)
                await DailyRepo.DeleteAsync(old);

            foreach (var kv in merged)
            {
                await DailyRepo.InsertAsync(new ZohoCampaignDailyStat
                {
                    CampaignKey  = campaignKey,
                    StatDate     = kv.Key,
                    OpensCount   = kv.Value.opens,
                    ClicksCount  = kv.Value.clicks,
                    BouncesCount = kv.Value.bounces,
                    FetchedUtc   = now
                });
            }

            await _logger.InformationAsync(string.Format(
                await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.Log.TimelineSynced"),
                campaignKey, merged.Count));
            var affectedLeadIds = RecipientRepo.Table
                .Where(r => r.CampaignKey == campaignKey && r.LeadId.HasValue)
                .Select(r => r.LeadId.Value)
                .Distinct()
                .ToList();

            foreach (var lid in affectedLeadIds)
            {
                var leadRecipients = RecipientRepo.Table
                    .Where(r => r.LeadId == lid)
                    .ToList();
                try { await LeadSvc.RecalculateInterestScoreAsync(lid, leadRecipients); }
                catch {}
            }
        }

        public virtual Task<IList<ZohoCampaignDailyStat>> GetDailyStatsByKeyAsync(string campaignKey, int pastDays = 30, DateTime? from = null)
        {
            var cutoff = from?.Date ?? DateTime.UtcNow.Date.AddDays(-pastDays);
            IList<ZohoCampaignDailyStat> results = DailyRepo.Table
                .Where(d => d.CampaignKey == campaignKey && d.StatDate >= cutoff)
                .OrderBy(d => d.StatDate)
                .ToList();
            return Task.FromResult(results);
        }

        public virtual Task<IList<ZohoCampaignDailyStat>> GetCombinedDailyStatsAsync(DateTime from, DateTime to, IList<string> campaignKeys = null)
        {
            var query = DailyRepo.Table.Where(d => d.StatDate >= from && d.StatDate <= to);
            if (campaignKeys != null && campaignKeys.Count > 0)
                query = query.Where(d => campaignKeys.Contains(d.CampaignKey));
            IList<ZohoCampaignDailyStat> results = query
                .GroupBy(d => d.StatDate)
                .Select(g => new ZohoCampaignDailyStat
                {
                    StatDate     = g.Key,
                    OpensCount   = g.Sum(x => x.OpensCount),
                    ClicksCount  = g.Sum(x => x.ClicksCount),
                    BouncesCount = g.Sum(x => x.BouncesCount),
                    FetchedUtc   = g.Max(x => x.FetchedUtc)
                })
                .OrderBy(d => d.StatDate)
                .ToList();

            return Task.FromResult(results);
        }

        public virtual Task<IDictionary<string, (int Opens, int Clicks, int Bounces)>> GetPeriodStatsByCampaignAsync(DateTime from, DateTime to)
        {
            var grouped = DailyRepo.Table
                .Where(d => d.StatDate >= from && d.StatDate <= to)
                .GroupBy(d => d.CampaignKey)
                .Select(g => new
                {
                    Key     = g.Key,
                    Opens   = g.Sum(x => x.OpensCount),
                    Clicks  = g.Sum(x => x.ClicksCount),
                    Bounces = g.Sum(x => x.BouncesCount)
                })
                .ToList();

            IDictionary<string, (int, int, int)> result = grouped
                .ToDictionary(x => x.Key, x => (x.Opens, x.Clicks, x.Bounces));

            return Task.FromResult(result);
        }

        public virtual Task<IList<ZohoCampaignRecipient>> GetRecipientsByLeadIdAsync(int leadId)
        {
            IList<ZohoCampaignRecipient> results = RecipientRepo.Table
                .Where(r => r.LeadId == leadId)
                .OrderByDescending(r => r.LastOpenedAt)
                .ToList();
            return Task.FromResult(results);
        }

        public virtual Task<IList<ZohoCampaignRecipient>> GetRecipientsByContactIdAsync(int contactId)
        {
            IList<ZohoCampaignRecipient> results = RecipientRepo.Table
                .Where(r => r.ContactId == contactId)
                .OrderByDescending(r => r.LastOpenedAt)
                .ToList();
            return Task.FromResult(results);
        }

        public virtual Task<IDictionary<int, (int TotalOpens, int TotalClicks, int TotalBounces, int TotalUnsubscribes)>> GetLeadEmailStatsAsync()
        {
            var rows = RecipientRepo.Table
                .Where(r => r.LeadId.HasValue)
                .GroupBy(r => r.LeadId.Value)
                .Select(g => new
                {
                    LeadId       = g.Key,
                    Opens        = g.Sum(r => r.OpenCount),
                    Clicks       = g.Sum(r => r.ClickCount),
                    Bounces      = g.Sum(r => r.BounceCount),
                    Unsubscribes = g.Count(r => r.HasUnsubscribed)
                })
                .ToList();

            IDictionary<int, (int, int, int, int)> result = rows.ToDictionary(
                x => x.LeadId,
                x => (x.Opens, x.Clicks, x.Bounces, x.Unsubscribes));

            return Task.FromResult(result);
        }

        #endregion

        #region Utilities (recipient sync)

        private async Task UpsertRecipientsAsync(string campaignKey, string action,
            List<(string email, DateTime? firstAt, int count)> recipients)
        {
            var grouped = recipients
                .GroupBy(r => r.email)
                .Select(g => (
                    email:   g.Key,
                    firstAt: g.Where(x => x.firstAt.HasValue).Select(x => x.firstAt.Value).DefaultIfEmpty().Min(),
                    lastAt:  g.Where(x => x.firstAt.HasValue).Select(x => x.firstAt.Value).DefaultIfEmpty().Max(),
                    count:   g.Sum(x => x.count)
                ))
                .ToList();

            foreach (var (email, firstAt, lastAt, count) in grouped)
            {
                var existing = RecipientRepo.Table
                    .FirstOrDefault(x => x.CampaignKey == campaignKey && x.RecipientEmail == email);

                if (existing == null)
                {
                    var lead    = LeadRepo.Table.FirstOrDefault(l => l.Email == email);
                    var contact = lead == null
                        ? ContactRepo.Table.FirstOrDefault(c => c.Email == email)
                        : null;

                    existing = new ZohoCampaignRecipient
                    {
                        CampaignKey    = campaignKey,
                        RecipientEmail = email,
                        LeadId         = lead?.Id,
                        ContactId      = contact?.Id,
                        SyncedUtc      = DateTime.UtcNow
                    };
                }

                if (action == "openedcontacts")
                {
                    existing.HasOpened  = true;
                    existing.OpenCount  = count;
                    if (!existing.FirstOpenedAt.HasValue || (firstAt != default && firstAt < existing.FirstOpenedAt))
                        existing.FirstOpenedAt = firstAt == default ? null : (DateTime?)firstAt;
                    if (lastAt != default && (!existing.LastOpenedAt.HasValue || lastAt > existing.LastOpenedAt))
                        existing.LastOpenedAt = lastAt;
                }
                else if (action == "clickedcontacts")
                {
                    existing.HasClicked = true;
                    existing.ClickCount = count;
                }
                else if (action == "senthardbounce" || action == "sentsoftbounce")
                {
                    existing.HasBounced  = true;
                    existing.BounceCount = count;
                    existing.BounceType  = action == "senthardbounce" ? "hard" : "soft";
                    if (firstAt != default(DateTime))
                        existing.BouncedAt = firstAt;
                }
                else if (action == "optoutcontacts")
                {
                    existing.HasUnsubscribed = true;
                    if (firstAt != default(DateTime))
                        existing.UnsubscribedAt = firstAt;
                }

                existing.SyncedUtc = DateTime.UtcNow;

                if (existing.Id == 0)
                    await RecipientRepo.InsertAsync(existing);
                else
                    await RecipientRepo.UpdateAsync(existing);
            }
        }

        #endregion
    }
}
