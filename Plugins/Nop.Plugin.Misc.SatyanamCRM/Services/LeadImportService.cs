using App.Core;
using App.Core.Domain.Catalog;
using App.Core.Domain.Common;
using App.Core.Domain.Localization;
using App.Data;
using App.Services.Common;
using App.Services.Directory;
using App.Services.ExportImport;
using App.Services.ExportImport.Help;
using App.Services.Localization;
using App.Services.Messages;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.EmailVerification.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// Lead service
    /// </summary>
    public partial class LeadImportService : ILeadImportService
    {
        #region Fields

        private readonly IRepository<Lead> _leadRepository;
        private readonly ITitleService _titleService;
        private readonly ILeadSourceService _leadSourceService;
        private readonly IIndustryService _industryService;
        private readonly ICountryService _countryService;
        private readonly IAddressService _addressService;
        private readonly ILeadService _leadService;
        private readonly ICategorysService _categoryService;
        private readonly ILeadStatusService _leadStatusService;
        private readonly INotificationService _notificationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILanguageService _languageService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ITagsService _tagsService;
        private readonly ILocalizationService _localizationService;
        private readonly IEmailverificationService _emailverificationService;
        #endregion

        #region Ctor

        public LeadImportService(IRepository<Lead> leadRepository,
                                 ITitleService titleService,
                                 ILeadSourceService leadSourceService,
                                 IIndustryService industryService,
                                 ICountryService countryService,
                                 IAddressService addressService,
                                 ILeadService leadService,
                                 ICategorysService categoryService,
                                 ILeadStatusService leadStatusService,
                                 INotificationService notificationService,
                                 IStateProvinceService stateProvinceService,
                                 ILanguageService languageService,
                                 CatalogSettings catalogSettings,
                                 ITagsService tagsService,
                                 ILocalizationService localizationService,
                                 IEmailverificationService emailverificationService)
        {
            _leadRepository = leadRepository;
            _titleService = titleService;
            _leadSourceService = leadSourceService;
            _industryService = industryService;
            _countryService = countryService;
            _addressService = addressService;
            _leadService = leadService;
            _categoryService = categoryService;
            _leadStatusService = leadStatusService;
            _notificationService = notificationService;
            _stateProvinceService = stateProvinceService;
            _languageService = languageService;
            _catalogSettings = catalogSettings;
            _tagsService = tagsService;
            _localizationService = localizationService;
            _emailverificationService = emailverificationService;
        }

        #endregion

        #region Methods

        #region Lead Import
        private async Task<int> TryParseEmployeeCountAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            input = input.Trim().Replace(",", ""); // Remove commas if present

            double value = 0;

            // Handle 'k' and 'm' multipliers
            if (input.EndsWith("k", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(input[..^1], out value)) // Remove 'k' and parse
                    value *= 1000;
            }
            else if (input.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(input[..^1], out value)) // Remove 'm' and parse
                    value *= 1000000;
            }
            else if (!double.TryParse(input, out value))
            {
                return 0; // Return 0 if parsing fails
            }

            return (int)value; // Convert to int and return
        }
        private async Task<int> TryParseAnnualRevenueAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0; // Default to 0 if input is null or empty
            }

            input = input.Trim().Replace(",", "").Replace("$", ""); // Remove commas and dollar signs
            double value = 0;

            if (input.EndsWith("k", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(input[..^1], out value))
                    value *= 1000;
            }
            else if (input.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(input[..^1], out value))
                    value *= 1000000;
            }
            else if (!double.TryParse(input, out value))
            {
                return 0; // If parsing fails, store as 0
            }

            return (int)value; // Convert to integer and return
        }

        // Helper method to safely get cell value by column name
        private string GetCellValue(IXLRow row, Dictionary<string, int> headers, string columnName)
        {
            return headers.ContainsKey(columnName) ? row.Cell(headers[columnName]).GetValue<string>().Trim() : null;
        }
        private string GetMultiplePhoneNumbers(IXLRow row, List<int> phoneColumns)
        {
            var phoneNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in phoneColumns)
            {
                string phoneValue = row.Cell(col).GetString().Trim();
                if (!string.IsNullOrWhiteSpace(phoneValue))
                {
                    // Split multiple numbers found in a single cell (if separated by , or ;)
                    var splitNumbers = phoneValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(p => p.Trim())
                                                 .Where(p => !string.IsNullOrEmpty(p));

                    foreach (var number in splitNumbers)
                    {
                        phoneNumbers.Add(number);
                    }
                }
            }

            return phoneNumbers.Count > 0 ? string.Join(";", phoneNumbers) : null;
        }
        private string SanitizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            url = url.Trim();

            // Ensure URL starts with http:// or https://
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            // Validate the URI
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri validUri))
            {
                return validUri.ToString();
            }

            return null; // Return null if the URL is invalid
        }

        public static WorkbookMetadata<T> GetWorkbookMetadata<T>(IXLWorkbook workbook, IList<Language> languages)
        {
            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var properties = new List<PropertyByName<T, Language>>();
            var localizedProperties = new List<PropertyByName<T, Language>>();
            var localizedWorksheets = new List<IXLWorksheet>();

            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Row(1).Cell(poz);

                    if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T, Language>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            foreach (var ws in workbook.Worksheets.Skip(1))
                if (languages.Any(l => l.UniqueSeoCode.Equals(ws.Name, StringComparison.InvariantCultureIgnoreCase)))
                    localizedWorksheets.Add(ws);

            if (localizedWorksheets.Any())
            {
                // get the first worksheet in the workbook
                var localizedWorksheet = localizedWorksheets.First();

                poz = 1;
                while (true)
                {
                    try
                    {
                        var cell = localizedWorksheet.Row(1).Cell(poz);

                        if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                            break;

                        poz += 1;
                        localizedProperties.Add(new PropertyByName<T, Language>(cell.Value.ToString()));
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            return new WorkbookMetadata<T>()
            {
                DefaultProperties = properties,
                LocalizedProperties = localizedProperties,
                DefaultWorksheet = worksheet,
                LocalizedWorksheets = localizedWorksheets
            };
        }




        public async Task ImportLeadsFromExcelAsync(IFormFile importFile)
        {
            if (importFile == null || importFile.Length == 0)
                throw new ArgumentException("No file uploaded");

            var isExcel = importFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
            var isCsv = importFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

            if (!isExcel && !isCsv)
                throw new ArgumentException("Invalid file type. Please upload a valid Excel (.xlsx) or CSV (.csv) file.");


            try
            {
                var stream = new MemoryStream();
                await importFile.CopyToAsync(stream);
                stream.Position = 0; // Ensure stream is reset before reading

                var workbook = new XLWorkbook(stream, XLEventTracking.Disabled);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();

                var headerRow = rows.FirstOrDefault();
                if (headerRow == null)
                    throw new Exception("The file is empty or the header row is missing.");
                var headers = new Dictionary<string, int>();
                var existingColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var cell in headerRow.CellsUsed().Select((cell, index) => new { Name = cell.GetString().Trim(), Index = index + 1 }))
                {
                    if (!existingColumnNames.Contains(cell.Name))
                    {
                        headers[cell.Name] = cell.Index;
                        existingColumnNames.Add(cell.Name);
                    }
                }

                // Required columns check
                bool hasFirstName = headers.ContainsKey("First Name");
                bool hasLastName = headers.ContainsKey("Last Name");
                bool hasNameColumn = headers.ContainsKey("Name");
                bool hasEmailsColumn = headers.ContainsKey("Emails");
                bool hasZipColumn = headers.ContainsKey("Zip") || headers.ContainsKey("ZipCode");
                bool hasTagsColumn = headers.ContainsKey("Tags");
                bool hasEmailOptOutColumn = headers.ContainsKey("EmailOptOut") || headers.ContainsKey("Subscribers");
                var revenueColumnName = headers.ContainsKey("Annual Revenue") ? "Annual Revenue" :
                        headers.ContainsKey("Sales Revenue USD") ? "Sales Revenue USD" : null;
                if (!hasFirstName && !hasNameColumn)
                    throw new Exception("Missing required column: First Name (or Name)");

                if (!hasLastName && !hasNameColumn)
                    throw new Exception("Missing required column: Last Name (or Name)");

                // Fetch all existing countries
                var allCountries = await _countryService.GetAllCountriesAsync();
                var existingLeads = await _leadService.GetAllLeadAsync("", "", new List<int>(), "", "", 0, 0, new List<int>(), 0);
                //    var existingLeadIdentifiers = new HashSet<string>(existingLeads.Select(l =>
                //$"{l.FirstName} {l.LastName} {l.Email} {l.CompanyName}".Trim()), StringComparer.OrdinalIgnoreCase);
                var existingLeadIdentifiers = new HashSet<string>(existingLeads
                                                                    .Where(l => !string.IsNullOrWhiteSpace(l.Email))
                                                                    .Select(l => l.Email.Trim().ToLower()),
                                                                     StringComparer.OrdinalIgnoreCase
                                                                  );


                var seenLeads = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var duplicateRecords = new List<string>();
                foreach (var row in rows.Skip(1)) // Skip header row
                {
                    string firstName = hasFirstName ? GetCellValue(row, headers, "First Name") : null;
                    string lastName = hasLastName ? GetCellValue(row, headers, "Last Name") : null;

                    if (hasNameColumn && (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)))
                    {
                        string fullName = GetCellValue(row, headers, "Name");
                        var nameParts = fullName?.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        firstName = nameParts?.Length > 0 ? nameParts[0] : "";
                        lastName = nameParts?.Length > 1 ? nameParts[1] : "";
                    }

                    string email = GetCellValue(row, headers, "Email");
                    //string verificationResult = await _emailverificationService.VerifyEmailApi(email);
                    //dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

                    EmailValidationStatus emailStatus = EmailValidationStatus.None;

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            string verificationResult = await _emailverificationService.VerifyEmailApi(email);

                            if (verificationResult == "__SESSION_EXPIRED__")
                            {
                                _notificationService.WarningNotification("Email verification limit reached or session expired. Skipping validation.");
                                emailStatus = EmailValidationStatus.None;
                            }
                            else if (!string.IsNullOrWhiteSpace(verificationResult))
                            {
                                dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

                                var result = ((string)verificationResponse?.result)?.ToLowerInvariant();
                                var safeToSend = ((string)verificationResponse?.safe_to_send)?.ToLowerInvariant();

                                if (result == "valid" && safeToSend == "true")
                                {
                                    emailStatus = EmailValidationStatus.Valid;
                                }
                                else if (result == "invalid" || safeToSend == "false")
                                {
                                    emailStatus = EmailValidationStatus.Invalid;
                                }
                                else
                                {
                                    emailStatus = EmailValidationStatus.None;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _notificationService.WarningNotification("Email verification failed. Please try again later.");
                        }
                    }



                    string secondaryEmails = hasEmailsColumn ? GetCellValue(row, headers, "Emails") : null;
                    //string leadIdentifier = $"{firstName} {lastName} {email}".Trim();
                    //if (seenLeads.Contains(leadIdentifier))
                    //{
                    //    continue; // Skip duplicate entry in file
                    //}

                    //// Check if lead exists in the database
                    //if (existingLeadIdentifiers.Contains(leadIdentifier))
                    //{
                    //    duplicateRecords.Add($"{firstName} {lastName}");
                    //    continue; // Skip adding this record
                    //}

                    //seenLeads.Add(leadIdentifier); // Mark this lead as processed
                    var normalizedEmail = email?.Trim().ToLower();

                    // Skip if email is empty
                    //if (string.IsNullOrWhiteSpace(normalizedEmail))
                    //    continue;

                    // Skip duplicate in same import file
                    if (!string.IsNullOrWhiteSpace(normalizedEmail))
                    {
                        if (seenLeads.Contains(normalizedEmail))
                            continue;

                        if (existingLeadIdentifiers.Contains(normalizedEmail))
                        {
                            duplicateRecords.Add(email);
                            continue;
                        }

                        seenLeads.Add(normalizedEmail); // Only track non-empty emails
                    }



                    if (!string.IsNullOrWhiteSpace(secondaryEmails))
                    {
                        var emailList = secondaryEmails
                            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrEmpty(e))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            emailList.Remove(email);
                        }

                        secondaryEmails = string.Join(", ", emailList);
                    }

                    var phoneColumns = headers
                        .Where(h => h.Key.Contains("phone", StringComparison.OrdinalIgnoreCase) || h.Key.Contains("telephones", StringComparison.OrdinalIgnoreCase))
                        .Select(h => h.Value)
                        .ToList();
                    string phoneNumbers = GetMultiplePhoneNumbers(row, phoneColumns);

                    //                bool isDuplicate = existingLeads.Any(l =>
                    //(!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(l.Email) && l.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ||
                    //(!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(l.FirstName) &&
                    // !string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(l.LastName) &&
                    // !string.IsNullOrWhiteSpace(l.CompanyName) && !string.IsNullOrWhiteSpace(l.CompanyName) &&
                    // l.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                    // l.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) &&
                    // l.CompanyName.Equals(l.CompanyName, StringComparison.OrdinalIgnoreCase) &&
                    // (!string.IsNullOrWhiteSpace(l.WebsiteUrl) && !string.IsNullOrWhiteSpace(l.WebsiteUrl) &&
                    //  l.WebsiteUrl.Equals(l.WebsiteUrl, StringComparison.OrdinalIgnoreCase))));

                    //                if (isDuplicate)
                    //                {
                    //                    duplicateRecords.Add($"{firstName} {lastName}"); // Store only the unique names
                    //                    continue; // Skip adding this record
                    //                }
                    bool emailOptOut = false;
                    if (hasEmailOptOutColumn)
                    {
                        string emailOptOutValue = GetCellValue(row, headers, headers.ContainsKey("EmailOptOut") ? "EmailOptOut" : "Subscribers");

                        // Convert "true"/"false" or "0"/"1" to boolean
                        if (!string.IsNullOrWhiteSpace(emailOptOutValue))
                        {
                            emailOptOut = emailOptOutValue.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                          emailOptOutValue.Trim().Equals("1");
                        }
                    }
                    string revenueRaw = revenueColumnName != null ? GetCellValue(row, headers, revenueColumnName) : null;
                    Console.WriteLine($"Raw Revenue from Excel: {revenueRaw ?? "N/A"}"); // Debugging

                    int parsedRevenue = await TryParseAnnualRevenueAsync(revenueRaw);
                    Console.WriteLine($"Stored Revenue: {parsedRevenue}");

                    var lead = new Lead
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        CompanyName = GetCellValue(row, headers, "Company"),
                        Phone = phoneNumbers,
                        Email = email,
                        SecondaryEmail = secondaryEmails,

                        // Handle Website URL (Checking "Website" or "Location on Site")
                        WebsiteUrl = SanitizeUrl(headers.ContainsKey("Website") ? GetCellValue(row, headers, "Website") :
                              headers.ContainsKey("Location on Site") ? GetCellValue(row, headers, "Location on Site") : null),
                        AnnualRevenue = parsedRevenue > 0 ? parsedRevenue : 0,
                        NoofEmployee = await TryParseEmployeeCountAsync(GetCellValue(row, headers, "Employees")),
                        SkypeId = GetCellValue(row, headers, "Skype"),
                        Twitter = GetCellValue(row, headers, "Twitter"),

                        LinkedinUrl = SanitizeUrl(GetCellValue(row, headers, "LinkedIn")),
                        Facebookurl = GetCellValue(row, headers, "Facebook"),
                        Description = GetCellValue(row, headers, "Description"),
                        EmailStatusId = (int)emailStatus,
                        EmailOptOut = emailOptOut,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                    //string revenueDisplay = $"${lead.AnnualRevenue}";
                    // Handle Title
                    var titleName = GetCellValue(row, headers, "Title");
                    var title = await _titleService.GetOrCreateTitleByNameAsync(titleName);
                    lead.TitleId = title?.Id ?? 0;

                    // Handle Lead Source
                    var leadSourceName = GetCellValue(row, headers, "Lead Source");
                    var leadSource = await _leadSourceService.GetOrCreateLeadSourceByNameAsync(leadSourceName);
                    lead.LeadSourceId = leadSource?.Id ?? 0;

                    // Handle Industry
                    var industryName = headers.ContainsKey("Industry") ? GetCellValue(row, headers, "Industry")
                        : headers.ContainsKey("Vertical") ? GetCellValue(row, headers, "Vertical")
                        : null;
                    var industry = await _industryService.GetOrCreateIndustryByNameAsync(industryName);
                    lead.IndustryId = industry?.Id ?? 0;

                    // Handle Category
                    var categoryName = GetCellValue(row, headers, "Category");
                    var category = await _categoryService.GetOrCreateCategorysByNameAsync(categoryName);
                    lead.CategoryId = category?.Id ?? 0;

                    // Handle Lead Status
                    var leadStatusName = GetCellValue(row, headers, "Lead Status");
                    var leadStatus = await _leadStatusService.GetOrCreateLeadStatusByNameAsync(leadStatusName);
                    lead.LeadStatusId = leadStatus?.Id ?? 0;
                    var stateName = GetCellValue(row, headers, "State");
                    // Handle Country & State
                    var countryName = GetCellValue(row, headers, "Country");
                    var country = (App.Core.Domain.Directory.Country)null;

                    if (!string.IsNullOrWhiteSpace(countryName))
                    {
                        country = allCountries.FirstOrDefault(c => c.Name.Equals(countryName, StringComparison.OrdinalIgnoreCase))
                                ?? allCountries.FirstOrDefault(c => c.TwoLetterIsoCode.Equals(countryName, StringComparison.OrdinalIgnoreCase))
                                ?? allCountries.FirstOrDefault(c => c.ThreeLetterIsoCode.Equals(countryName, StringComparison.OrdinalIgnoreCase));
                    }
                    var fullCountryName = country?.Name;

                    // Fetch states for the selected country

                    var state = (App.Core.Domain.Directory.StateProvince)null;
                    if (!string.IsNullOrWhiteSpace(stateName) && country != null)
                    {
                        var existingStates = await _stateProvinceService.GetStateProvincesByCountryIdAsync(country.Id);

                        // Normalize state names (trim spaces)
                        stateName = stateName.Trim();

                        // Try exact match (both lower & upper case)
                        state = existingStates.FirstOrDefault(s =>
                            s.Name.Trim().Equals(stateName, StringComparison.OrdinalIgnoreCase) ||
                            (!string.IsNullOrWhiteSpace(s.Abbreviation) && s.Abbreviation.Trim().Equals(stateName, StringComparison.OrdinalIgnoreCase))
                        );

                        // If no exact match, try partial match (contains check for both lower & upper case)
                        if (state == null)
                        {
                            state = existingStates.FirstOrDefault(s =>
                                s.Name.Trim().IndexOf(stateName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                (!string.IsNullOrWhiteSpace(s.Abbreviation) && s.Abbreviation.Trim().IndexOf(stateName, StringComparison.OrdinalIgnoreCase) >= 0)
                            );
                        }
                    }



                    var fullStateName = state?.Name;

                    var city = GetCellValue(row, headers, "City");
                    var zipCode = hasZipColumn ? GetCellValue(row, headers, headers.ContainsKey("Zip") ? "Zip" : "ZipCode") : null;

                    var address = new Address
                    {
                        City = city,
                        CountryId = country?.Id,
                        StateProvinceId = state?.Id,
                        ZipPostalCode = zipCode
                    };

                    await _addressService.InsertAddressAsync(address);
                    lead.AddressId = address.Id;

                    // Save lead
                    await _leadService.InsertLeadAsync(lead);
                    if (hasTagsColumn)
                    {
                        string tagsValue = GetCellValue(row, headers, "Tags");

                        if (!string.IsNullOrWhiteSpace(tagsValue))
                        {
                            // Split tags by ',' or ';' and remove extra spaces
                            var tagNames = tagsValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(t => t.Trim())
                                                    .Where(t => !string.IsNullOrEmpty(t))
                                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                                    .ToList();

                            foreach (var tagName in tagNames)
                            {
                                // Fetch or create the tag
                                var tag = await _tagsService.GetOrCreateTagsByNameAsync(tagName);
                                if (tag != null)
                                {
                                    // Check if the lead already has this tag before inserting
                                    var existingLeadTags = await _leadService.GetLeadTagByLeadIdAsync(lead.Id);
                                    if (existingLeadTags == null || !existingLeadTags.Any(t => t.TagsId == tag.Id))
                                    {
                                        await _leadService.InsertLeadTagsAsync(new LeadTags
                                        {
                                            LeadId = lead.Id,
                                            TagsId = tag.Id
                                        });
                                    }
                                }
                            }
                        }
                    }

                }
                // Show success message with duplicate details if any
                if (duplicateRecords.Any())
                {
                    string duplicateMessage = "Duplicate Entry: " + string.Join(",", duplicateRecords);
                    _notificationService.WarningNotification(duplicateMessage);
                }
                else
                {
                    _notificationService.SuccessNotification("Leads imported successfully!");
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);
                throw new Exception("Error importing leads: " + ex.Message);
            }
        }

        public async Task ImportLeadsFromExcelReplyAsync(IFormFile importFile)
        {
            if (importFile == null || importFile.Length == 0)
                throw new ArgumentException("No file uploaded");

            var isExcel = importFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
            var isCsv = importFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

            if (!isExcel && !isCsv)
                throw new ArgumentException("Invalid file type. Please upload a valid Excel (.xlsx) or CSV (.csv) file.");


            try
            {
                var stream = new MemoryStream();
                await importFile.CopyToAsync(stream);
                stream.Position = 0; // Ensure stream is reset before reading

                var workbook = new XLWorkbook(stream, XLEventTracking.Disabled);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();

                var headerRow = rows.FirstOrDefault();
                if (headerRow == null)
                    throw new Exception("The file is empty or the header row is missing.");
                var headers = new Dictionary<string, int>();
                var existingColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var cell in headerRow.CellsUsed().Select((cell, index) => new { Name = cell.GetString().Trim(), Index = index + 1 }))
                {
                    if (!existingColumnNames.Contains(cell.Name))
                    {
                        headers[cell.Name] = cell.Index;
                        existingColumnNames.Add(cell.Name);
                    }
                }

                // Required columns check
                bool hasFirstName = headers.ContainsKey("First Name");
                bool hasLastName = headers.ContainsKey("Last Name");
                bool hasNameColumn = headers.ContainsKey("Name");
                bool hasEmailsColumn = headers.ContainsKey("Emails");
                //bool hasZipColumn = headers.ContainsKey("Zip") || headers.ContainsKey("ZipCode");
                //bool hasTagsColumn = headers.ContainsKey("Tags");
                //bool hasEmailOptOutColumn = headers.ContainsKey("EmailOptOut") || headers.ContainsKey("Subscribers");
                //var revenueColumnName = headers.ContainsKey("Annual Revenue") ? "Annual Revenue" :
                  //      headers.ContainsKey("Sales Revenue USD") ? "Sales Revenue USD" : null;
                if (!hasFirstName && !hasNameColumn)
                    throw new Exception("Missing required column: First Name (or Name)");

                if (!hasLastName && !hasNameColumn)
                    throw new Exception("Missing required column: Last Name (or Name)");

                // Fetch all existing countries
                var allCountries = await _countryService.GetAllCountriesAsync();
                var existingLeads = await _leadService.GetAllLeadAsync("", "", new List<int>(), "", "", 0, 0, new List<int>(), 0);
                var existingLeadIdentifiers = new HashSet<string>(existingLeads
                                                                    .Where(l => !string.IsNullOrWhiteSpace(l.Email))
                                                                    .Select(l => l.Email.Trim().ToLower()),
                                                                     StringComparer.OrdinalIgnoreCase
                                                                  );


                var seenLeads = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var duplicateRecords = new List<string>();
                foreach (var row in rows.Skip(1)) // Skip header row
                {
                    string firstName = hasFirstName ? GetCellValue(row, headers, "First Name") : null;
                    string lastName = hasLastName ? GetCellValue(row, headers, "Last Name") : null;

                    if (hasNameColumn && (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)))
                    {
                        string fullName = GetCellValue(row, headers, "Name");
                        var nameParts = fullName?.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        firstName = nameParts?.Length > 0 ? nameParts[0] : "";
                        lastName = nameParts?.Length > 1 ? nameParts[1] : "";
                    }

                    string email = GetCellValue(row, headers, "Email");

                    EmailValidationStatus emailStatus = EmailValidationStatus.None;

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            string verificationResult = await _emailverificationService.VerifyEmailApi(email);

                            if (verificationResult == "__SESSION_EXPIRED__")
                            {
                                _notificationService.WarningNotification("Email verification limit reached or session expired. Skipping validation.");
                                emailStatus = EmailValidationStatus.None;
                            }
                            else if (!string.IsNullOrWhiteSpace(verificationResult))
                            {
                                dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

                                var result = ((string)verificationResponse?.result)?.ToLowerInvariant();
                                var safeToSend = ((string)verificationResponse?.safe_to_send)?.ToLowerInvariant();

                                if (result == "valid" && safeToSend == "true")
                                {
                                    emailStatus = EmailValidationStatus.Valid;
                                }
                                else if (result == "invalid" || safeToSend == "false")
                                {
                                    emailStatus = EmailValidationStatus.Invalid;
                                }
                                else
                                {
                                    emailStatus = EmailValidationStatus.None;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _notificationService.WarningNotification("Email verification failed. Please try again later.");
                        }
                    }



                    string secondaryEmails = hasEmailsColumn ? GetCellValue(row, headers, "Emails") : null;
                    var normalizedEmail = email?.Trim().ToLower();
                    if (!string.IsNullOrWhiteSpace(normalizedEmail))
                    {
                        if (seenLeads.Contains(normalizedEmail))
                            continue;

                        if (existingLeadIdentifiers.Contains(normalizedEmail))
                        {
                            duplicateRecords.Add(email);
                            continue;
                        }

                        seenLeads.Add(normalizedEmail); // Only track non-empty emails
                    }

                    if (!string.IsNullOrWhiteSpace(secondaryEmails))
                    {
                        var emailList = secondaryEmails
                            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrEmpty(e))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            emailList.Remove(email);
                        }

                        secondaryEmails = string.Join(", ", emailList);
                    }

                    var phoneColumns = headers
                        .Where(h => h.Key.Contains("phone", StringComparison.OrdinalIgnoreCase) || h.Key.Contains("telephones", StringComparison.OrdinalIgnoreCase))
                        .Select(h => h.Value)
                        .ToList();
                    string phoneNumbers = GetMultiplePhoneNumbers(row, phoneColumns);

                   
                    bool emailOptOut = false;
                    string linkedInHeader = headers.ContainsKey("LinkedIn")
    ? "LinkedIn"
    : headers.ContainsKey("LinkedIn profile")
        ? "LinkedIn profile"
        : null;

                    var lead = new Lead
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        CompanyName = GetCellValue(row, headers, "Company"),
                        Phone = phoneNumbers,
                        Email = email,
                        LinkedinUrl = linkedInHeader != null? SanitizeUrl(GetCellValue(row, headers, linkedInHeader)): null,
                        //LinkedinUrl = SanitizeUrl(GetCellValue(row, headers, "LinkedIn")),
                        EmailStatusId = (int)emailStatus,
                        //SecondaryEmail = secondaryEmails,

                        // Handle Website URL (Checking "Website" or "Location on Site")
                        //WebsiteUrl = SanitizeUrl(headers.ContainsKey("Website") ? GetCellValue(row, headers, "Website") :
                            //  headers.ContainsKey("Location on Site") ? GetCellValue(row, headers, "Location on Site") : null),
                        //AnnualRevenue = parsedRevenue > 0 ? parsedRevenue : 0,
                        //NoofEmployee = await TryParseEmployeeCountAsync(GetCellValue(row, headers, "Employees")),
                        //SkypeId = GetCellValue(row, headers, "Skype"),
                        //Twitter = GetCellValue(row, headers, "Twitter"),

                        //Facebookurl = GetCellValue(row, headers, "Facebook"),
                        //Description = GetCellValue(row, headers, "Description"),
                        //EmailOptOut = emailOptOut,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                    //string revenueDisplay = $"${lead.AnnualRevenue}";
                    // Handle Title
                    var titleName = GetCellValue(row, headers, "Title");
                    var title = await _titleService.GetOrCreateTitleByNameAsync(titleName);
                    lead.TitleId = title?.Id ?? 0;

                    // Handle Lead Source
                    //var leadSourceName = GetCellValue(row, headers, "Lead Source");
                    //var leadSource = await _leadSourceService.GetOrCreateLeadSourceByNameAsync(leadSourceName);
                    //lead.LeadSourceId = leadSource?.Id ?? 0;

                    // Handle Industry
                    //var industryName = headers.ContainsKey("Industry") ? GetCellValue(row, headers, "Industry")
                    //    : headers.ContainsKey("Vertical") ? GetCellValue(row, headers, "Vertical")
                    //    : null;
                    //var industry = await _industryService.GetOrCreateIndustryByNameAsync(industryName);
                    //lead.IndustryId = industry?.Id ?? 0;

                    // Handle Category
                    //var categoryName = GetCellValue(row, headers, "Category");
                    //var category = await _categoryService.GetOrCreateCategorysByNameAsync(categoryName);
                    //lead.CategoryId = category?.Id ?? 0;

                    // Handle Lead Status
                    //var leadStatusName = GetCellValue(row, headers, "Lead Status");
                    //var leadStatus = await _leadStatusService.GetOrCreateLeadStatusByNameAsync(leadStatusName);
                    //lead.LeadStatusId = leadStatus?.Id ?? 0;
                    var stateName = GetCellValue(row, headers, "State");
                    // Handle Country & State
                    var countryName = GetCellValue(row, headers, "Country");
                    var country = (App.Core.Domain.Directory.Country)null;

                    if (!string.IsNullOrWhiteSpace(countryName))
                    {
                        country = allCountries.FirstOrDefault(c => c.Name.Equals(countryName, StringComparison.OrdinalIgnoreCase))
                                ?? allCountries.FirstOrDefault(c => c.TwoLetterIsoCode.Equals(countryName, StringComparison.OrdinalIgnoreCase))
                                ?? allCountries.FirstOrDefault(c => c.ThreeLetterIsoCode.Equals(countryName, StringComparison.OrdinalIgnoreCase));
                    }
                    var fullCountryName = country?.Name;

                    // Fetch states for the selected country

                    var state = (App.Core.Domain.Directory.StateProvince)null;
                    if (!string.IsNullOrWhiteSpace(stateName) && country != null)
                    {
                        var existingStates = await _stateProvinceService.GetStateProvincesByCountryIdAsync(country.Id);

                        // Normalize state names (trim spaces)
                        stateName = stateName.Trim();

                        // Try exact match (both lower & upper case)
                        state = existingStates.FirstOrDefault(s =>
                            s.Name.Trim().Equals(stateName, StringComparison.OrdinalIgnoreCase) ||
                            (!string.IsNullOrWhiteSpace(s.Abbreviation) && s.Abbreviation.Trim().Equals(stateName, StringComparison.OrdinalIgnoreCase))
                        );

                        // If no exact match, try partial match (contains check for both lower & upper case)
                        if (state == null)
                        {
                            state = existingStates.FirstOrDefault(s =>
                                s.Name.Trim().IndexOf(stateName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                (!string.IsNullOrWhiteSpace(s.Abbreviation) && s.Abbreviation.Trim().IndexOf(stateName, StringComparison.OrdinalIgnoreCase) >= 0)
                            );
                        }
                    }



                    var fullStateName = state?.Name;

                    var city = GetCellValue(row, headers, "City");
                    //var zipCode = hasZipColumn ? GetCellValue(row, headers, headers.ContainsKey("Zip") ? "Zip" : "ZipCode") : null;

                    var address = new Address
                    {
                        City = city,
                        CountryId = country?.Id,
                        StateProvinceId = state?.Id,
                        //ZipPostalCode = zipCode
                    };

                    await _addressService.InsertAddressAsync(address);
                    lead.AddressId = address.Id;

                    // Save lead
                    await _leadService.InsertLeadAsync(lead);
                    //if (hasTagsColumn)
                    //{
                    //    string tagsValue = GetCellValue(row, headers, "Tags");

                    //    if (!string.IsNullOrWhiteSpace(tagsValue))
                    //    {
                    //        // Split tags by ',' or ';' and remove extra spaces
                    //        var tagNames = tagsValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    //                                .Select(t => t.Trim())
                    //                                .Where(t => !string.IsNullOrEmpty(t))
                    //                                .Distinct(StringComparer.OrdinalIgnoreCase)
                    //                                .ToList();

                    //        foreach (var tagName in tagNames)
                    //        {
                    //            // Fetch or create the tag
                    //            var tag = await _tagsService.GetOrCreateTagsByNameAsync(tagName);
                    //            if (tag != null)
                    //            {
                    //                // Check if the lead already has this tag before inserting
                    //                var existingLeadTags = await _leadService.GetLeadTagByLeadIdAsync(lead.Id);
                    //                if (existingLeadTags == null || !existingLeadTags.Any(t => t.TagsId == tag.Id))
                    //                {
                    //                    await _leadService.InsertLeadTagsAsync(new LeadTags
                    //                    {
                    //                        LeadId = lead.Id,
                    //                        TagsId = tag.Id
                    //                    });
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                }
                // Show success message with duplicate details if any
                if (duplicateRecords.Any())
                {
                    string duplicateMessage = "Duplicate Entry: " + string.Join(",", duplicateRecords);
                    _notificationService.WarningNotification(duplicateMessage);
                }
                else
                {
                    _notificationService.SuccessNotification("Leads imported successfully!");
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);
                throw new Exception("Error importing leads: " + ex.Message);
            }
        }
        #endregion

        #endregion
    }
}