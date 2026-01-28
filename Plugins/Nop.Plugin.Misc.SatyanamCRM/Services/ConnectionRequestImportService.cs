using App.Services.Messages;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// ConnectionRequest Import service
    /// </summary>
    public partial class ConnectionRequestImportService : IConnectionRequestImportService
    {
        #region Fields
        private readonly IConnectionRequestService _connectionRequestService;
        private readonly INotificationService _notificationService;
        #endregion

        #region Ctor

        public ConnectionRequestImportService(IConnectionRequestService connectionRequestService, INotificationService notificationService)
        {
            _connectionRequestService = connectionRequestService;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        #region ConnectionRequest Import

        public async Task<string> ImportConnectionRequestFromExcelAsync(IFormFile importFile)
        {
            if (importFile == null || importFile.Length == 0)
                throw new ArgumentException("No file uploaded");

            var isExcel = importFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
            var isCsv = importFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

            if (!isExcel && !isCsv)
                throw new ArgumentException("Invalid file type. Please upload a valid Excel (.xlsx) or CSV (.csv) file.");

            var stream = new MemoryStream();
            await importFile.CopyToAsync(stream);
            stream.Position = 0;

            var workbook = new XLWorkbook(stream, XLEventTracking.Disabled);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed();
            if (rows == null || !rows.Any())
                throw new Exception("No data found in file.");

            var headerRow = rows.First();
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed().Select((c, idx) => new { Name = c.GetString().Trim(), Index = idx + 1 }))
            {
                if (!headers.ContainsKey(cell.Name))
                    headers[cell.Name] = cell.Index;
            }

            var existing = (await _connectionRequestService.GetAllConnectionRequestAsync(
                firstname: "",
                lastname: "",
                email: "",
                linkedinUrl: "",
                website: "",
                statusId:0,
                pageIndex: 0,
                pageSize: int.MaxValue,
                showHidden: true
            )).ToList();

            int created = 0, updated = 0, skipped = 0;
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows.Skip(1))
            {
                string Read(params string[] names)
                {
                    foreach (var n in names)
                    {
                        var key = headers.Keys.FirstOrDefault(h => h.Equals(n, StringComparison.OrdinalIgnoreCase));
                        if (key != null)
                            return row.Cell(headers[key]).GetString().Trim();
                    }
                    foreach (var n in names)
                    {
                        var key = headers.Keys.FirstOrDefault(h => h.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (key != null)
                            return row.Cell(headers[key]).GetString().Trim();
                    }
                    return string.Empty;
                }

                var firstName = Read("First Name", "FirstName");
                var lastName = Read("Last Name", "LastName");
                var email = Read("Email", "E-mail");
                var linkedin = Read("Linkedin Link/Url", "LinkedIn Url", "Linkedin");
                var website = Read("Website Link", "Website");
                var status = Read("Status (manual)", "Status");
                var notes = Read("Notes", "Note");

                string keyForSeen = !string.IsNullOrWhiteSpace(email) ? email.ToLowerInvariant() :
                                    !string.IsNullOrWhiteSpace(linkedin) ? linkedin.ToLowerInvariant() :
                                    (firstName + "|" + lastName).ToLowerInvariant();

                if (seenKeys.Contains(keyForSeen))
                {
                    skipped++;
                    continue;
                }
                seenKeys.Add(keyForSeen);
                // --- Try find existing record ---
                var existingRecord = (!string.IsNullOrWhiteSpace(email))
                    ? existing.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Email) && x.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                    : null;

                if (existingRecord == null && !string.IsNullOrWhiteSpace(linkedin))
                {
                    existingRecord = existing.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.LinkedinUrl) &&
                                                                  x.LinkedinUrl.Equals(linkedin, StringComparison.OrdinalIgnoreCase));
                }

                if (existingRecord != null)
                {
                    var changed = false;

                    if (!string.IsNullOrWhiteSpace(firstName) && existingRecord.FirstName != firstName) { existingRecord.FirstName = firstName; changed = true; }
                    if (!string.IsNullOrWhiteSpace(lastName) && existingRecord.LastName != lastName) { existingRecord.LastName = lastName; changed = true; }
                    if (!string.IsNullOrWhiteSpace(linkedin) && existingRecord.LinkedinUrl != linkedin) { existingRecord.LinkedinUrl = linkedin; changed = true; }
                    if (!string.IsNullOrWhiteSpace(email) && existingRecord.Email != email) { existingRecord.Email = email; changed = true; }
                    if (!string.IsNullOrWhiteSpace(website) && existingRecord.WebsiteUrl != website) { existingRecord.WebsiteUrl = website; changed = true; }
                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        if (Enum.TryParse(typeof(FollowUpStatusEnum), status, true, out var parsedStatus))
                        {
                            int statusId = (int)(FollowUpStatusEnum)parsedStatus;
                            if (existingRecord.StatusId != statusId)
                            {
                                existingRecord.StatusId = statusId;
                                changed = true;
                            }
                        }
                    }

                    if (changed)
                    {
                        existingRecord.UpdatedOnUtc = DateTime.UtcNow;
                        await _connectionRequestService.UpdateConnectionRequestAsync(existingRecord);
                        updated++;
                    }
                    else
                    {
                        skipped++;
                    }
                }
                else
                {
                    var toInsert = new ConnectionRequest
                    {
                        FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName,
                        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName,
                        LinkedinUrl = string.IsNullOrWhiteSpace(linkedin) ? null : linkedin,
                        Email = string.IsNullOrWhiteSpace(email) ? null : email,
                        WebsiteUrl = string.IsNullOrWhiteSpace(website) ? null : website,
                        StatusId = Enum.TryParse(typeof(FollowUpStatusEnum), status, true, out var parsedStatus)? (int)(FollowUpStatusEnum)parsedStatus: (int)FollowUpStatusEnum.None,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    await _connectionRequestService.InsertConnectionRequestAsync(toInsert);
                    existing.Add(toInsert);
                    created++;
                }
            }

            var summary = $"Imported: {created}, Updated: {updated}, Skipped/Duplicates: {skipped}";
            _notificationService.SuccessNotification(summary);
            return summary;
        }
        #endregion

        #endregion
    }
}