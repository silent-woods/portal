using App.Core.Domain.Directory;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.Projects;
using App.Services.Configuration;
using App.Services.Directory;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Core.Settings;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.ProjectTasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public partial class ExecutiveDashboardController : BaseAdminController
{
    #region Fields

    private readonly IAccountManagementService _accountManagementService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly ITimeSheetsService _timeSheetsService;
    private readonly IProjectTaskService _projectTaskService;
    private readonly IProjectsService _projectsService;
    private readonly IEmployeeService _employeeService;
    private readonly IWorkflowStatusService _workflowStatusService;
    private readonly IMemoryCache _memoryCache;
    // CRM services
    private readonly ILeadService _leadService;
    private readonly IDealsService _dealsService;
    private readonly ILeadSourceService _leadSourceService;
    private readonly ILinkedInFollowupsService _linkedInFollowupsService;
    private readonly IInquiryService _inquiryService;
    private readonly ICompanyService _companyService;
    private readonly ICurrencyService _currencyService;
    private readonly IZohoCampaignService _zohoCampaignService;

    #endregion

    #region Ctor

    public ExecutiveDashboardController(
        IAccountManagementService accountManagementService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService,
        ITimeSheetsService timeSheetsService,
        IProjectTaskService projectTaskService,
        IProjectsService projectsService,
        IEmployeeService employeeService,
        IWorkflowStatusService workflowStatusService,
        IMemoryCache memoryCache,
        ILeadService leadService,
        IDealsService dealsService,
        ILeadSourceService leadSourceService,
        ILinkedInFollowupsService linkedInFollowupsService,
        IInquiryService inquiryService,
        ICompanyService companyService,
        ICurrencyService currencyService,
        IZohoCampaignService zohoCampaignService)
    {
        _accountManagementService = accountManagementService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _timeSheetsService = timeSheetsService;
        _projectTaskService = projectTaskService;
        _projectsService = projectsService;
        _employeeService = employeeService;
        _workflowStatusService = workflowStatusService;
        _memoryCache = memoryCache;
        _leadService = leadService;
        _dealsService = dealsService;
        _leadSourceService = leadSourceService;
        _linkedInFollowupsService = linkedInFollowupsService;
        _inquiryService = inquiryService;
        _companyService = companyService;
        _currencyService = currencyService;
        _zohoCampaignService = zohoCampaignService;
    }

    #endregion

    #region Utilities

    private static (int month, int year) GetPreviousPeriod(int monthId, int yearId)
        => monthId == 1 ? (12, yearId - 1) : (monthId - 1, yearId);

    private static List<(int month, int year, string label)> GetMonthlyIntervals(int monthId, int yearId, int count = 6)
    {
        var result = new List<(int, int, string)>();
        var m = monthId;
        var y = yearId;
        for (var i = 0; i < count; i++)
        {
            result.Insert(0, (m, y, new DateTime(y, m, 1).ToString("MMM yy", CultureInfo.InvariantCulture)));
            if (m == 1) { m = 12; y--; } else m--;
        }
        return result;
    }

    private static (int mFrom, int yFrom, int mTo, int yTo) NormalizeMonthRange(int monthFrom, int yearFrom, int monthTo, int yearTo)
    {
        var fromTotal = yearFrom * 12 + monthFrom;
        var toTotal = yearTo * 12 + monthTo;
        return fromTotal <= toTotal
            ? (monthFrom, yearFrom, monthTo, yearTo)
            : (monthTo, yearTo, monthFrom, yearFrom);
    }

    private static List<(int month, int year, string label)> GetMonthRangeIntervals(int monthFrom, int yearFrom, int monthTo, int yearTo)
    {
        var (mFrom, yFrom, mTo, yTo) = NormalizeMonthRange(monthFrom, yearFrom, monthTo, yearTo);
        var result = new List<(int, int, string)>();
        var singleYear = yFrom == yTo;
        var fmt = singleYear ? "MMM" : "MMM yy";
        var m = mFrom; var y = yFrom;
        while (y < yTo || (y == yTo && m <= mTo))
        {
            result.Add((m, y, new DateTime(y, m, 1).ToString(fmt, CultureInfo.InvariantCulture)));
            if (++m > 12) { m = 1; y++; }
        }
        return result;
    }

    private static List<(int month, int year, string label)> GetYearRangeIntervals(int yearFrom, int yearTo)
    {
        var result = new List<(int, int, string)>();
        var fmt = yearFrom == yearTo ? "MMM" : "MMM yy";
        for (var y = yearFrom; y <= yearTo; y++)
            for (var m = 1; m <= 12; m++)
                result.Add((m, y, new DateTime(y, m, 1).ToString(fmt, CultureInfo.InvariantCulture)));
        return result;
    }

    private static List<(DateTime from, DateTime to, string label)> GetMonthWeeklyIntervals(int month, int year)
    {
        var result = new List<(DateTime, DateTime, string)>();
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
        var current = monthStart;
        var weekNum = 1;
        while (current <= monthEnd)
        {
            var weekEnd = current.AddDays(6) > monthEnd ? monthEnd : current.AddDays(6).Date.AddDays(1).AddTicks(-1);
            result.Add((current, weekEnd, $"W{weekNum} ({current:dd MMM}–{weekEnd:dd MMM})"));
            current = weekEnd.Date.AddDays(1);
            weekNum++;
        }
        return result;
    }

    private static List<(DateTime from, DateTime to, string label)> GetDailyIntervals(DateTime baseTo, int count = 7)
    {
        var result = new List<(DateTime, DateTime, string)>();
        var day = baseTo.Date;
        for (var i = 0; i < count; i++)
        {
            result.Insert(0, (day, day.AddDays(1).AddTicks(-1), day.ToString("dd MMM", CultureInfo.InvariantCulture)));
            day = day.AddDays(-1);
        }
        return result;
    }

    private static List<(DateTime from, DateTime to, string label)> GetWeeklyIntervals(DateTime baseTo, int count = 6)
    {
        var result = new List<(DateTime, DateTime, string)>();
        var cal = CultureInfo.InvariantCulture.Calendar;
        var weekEnd = baseTo.Date;
        var daysToSunday = (int)weekEnd.DayOfWeek;
        weekEnd = weekEnd.AddDays(-daysToSunday);
        var weekStart = weekEnd.AddDays(-6);
        for (var i = 0; i < count; i++)
        {
            var weekNum = cal.GetWeekOfYear(weekStart, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            result.Insert(0, (weekStart, weekEnd.AddDays(1).AddTicks(-1), $"W{weekNum} {weekStart.Year}"));
            weekEnd = weekStart.AddDays(-1);
            weekStart = weekEnd.AddDays(-6);
        }
        return result;
    }
    private static List<(int month, int year, string label)> GetPreviousPeriodIntervals(int monthFrom, int yearFrom, int count)
    {
        var result = new List<(int, int, string)>();
        var m = monthFrom;
        var y = yearFrom;
        for (var i = 0; i < count; i++)
        {
            if (--m < 1) { m = 12; y--; }
            result.Insert(0, (m, y, new DateTime(y, m, 1).ToString("MMM yy", CultureInfo.InvariantCulture)));
        }
        return result;
    }
    private static List<(int month, int year, string label)> GetFinancialYearIntervals(int fyStartCalYear, int fyStartMonth)
    {
        var result = new List<(int, int, string)>();
        var m = fyStartMonth;
        var y = fyStartCalYear;
        for (var i = 0; i < 12; i++)
        {
            result.Add((m, y, new DateTime(y, m, 1).ToString("MMM yy", CultureInfo.InvariantCulture)));
            if (++m > 12) { m = 1; y++; }
        }
        return result;
    }

    private static List<(int month, int year, string label)> GetFYRangeIntervals(int fyStartYearFrom, int fyStartYearTo, int fyStartMonth)
    {
        var result = new List<(int, int, string)>();
        for (var fy = fyStartYearFrom; fy <= fyStartYearTo; fy++)
            result.AddRange(GetFinancialYearIntervals(fy, fyStartMonth));
        return result;
    }

    private static string FyLabel(int fyStartYear, int fyStartMonth)
        => fyStartMonth == 1 ? fyStartYear.ToString() : $"FY {fyStartYear}-{(fyStartYear + 1) % 100:D2}";

    private async Task<int> GetFyStartMonthAsync()
    {
        var settings = await _settingService.LoadSettingAsync<AccountManagementSettings>();
        return settings.FinancialYearStartMonth > 0 ? settings.FinancialYearStartMonth : 4;
    }

    private async Task<decimal> GetSalaryCostAsync(int? monthId = null, int? yearId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        IEnumerable<Domain.AccountTransaction> txns;

        if (monthId.HasValue && yearId.HasValue)
            txns = await _accountManagementService.GetAllAccountTransactionsAsync(transactionTypeId: 20, monthId: monthId.Value, yearId: yearId.Value);
        else if (dateFrom.HasValue && dateTo.HasValue)
            txns = await _accountManagementService.GetAllAccountTransactionsAsync(transactionTypeId: 20, dateFrom: dateFrom, dateTo: dateTo);
        else
            return 0;

        return txns.Where(t => t.EmployeeId > 0).Sum(t => t.Amount);
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> Index()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
            return AccessDeniedView();

        ViewBag.FyStartMonth = await GetFyStartMonthAsync();
        ViewBag.CanViewFinancial   = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardFinancial);
        ViewBag.CanViewOperational = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational);
        ViewBag.CanViewCRM         = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM);
        return View();
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetFinancialKPIData(int granularityId, DateTime? dateFrom, DateTime? dateTo, int? monthId, int? yearId, int? yearFrom, int? yearTo, int? monthFrom, int? monthTo)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardFinancial))
            return AccessDeniedView();

        decimal revenue = 0, expense = 0, prevRevenue = 0, prevExpense = 0, directCost = 0, prevDirectCost = 0;
        string periodLabel = "", prevPeriodLabel = "", chartNote = "";
        var revenuePoints = new List<decimal>();
        var cashFlowPoints = new List<decimal>();
        var cashInPoints = new List<decimal>();
        var cashOutPoints = new List<decimal>();
        var directCostPoints = new List<decimal>();
        var labels = new List<string>();

        if (granularityId == 3 && monthFrom.HasValue && yearFrom.HasValue && monthTo.HasValue && yearTo.HasValue)
        {
            var (mFrom, yFrom, mTo, yTo) = NormalizeMonthRange(monthFrom.Value, yearFrom.Value, monthTo.Value, yearTo.Value);

            if (mFrom == mTo && yFrom == yTo)
            {
                var (prevMonth, prevYear) = GetPreviousPeriod(mFrom, yFrom);
                var incTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: mFrom, yearId: yFrom);
                var expTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, monthId: mFrom, yearId: yFrom);
                var prevIncTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: prevMonth, yearId: prevYear);
                var prevExpTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, monthId: prevMonth, yearId: prevYear);
                var salaryTask = GetSalaryCostAsync(monthId: mFrom, yearId: yFrom);
                var prevSalaryTask = GetSalaryCostAsync(monthId: prevMonth, yearId: prevYear);
                await Task.WhenAll(incTask, expTask, prevIncTask, prevExpTask, salaryTask, prevSalaryTask);
                revenue = incTask.Result.TotalIncome;
                expense = expTask.Result.TotalExpense;
                prevRevenue = prevIncTask.Result.TotalIncome;
                prevExpense = prevExpTask.Result.TotalExpense;
                directCost = salaryTask.Result;
                prevDirectCost = prevSalaryTask.Result;
                periodLabel = new DateTime(yFrom, mFrom, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture);
                prevPeriodLabel = new DateTime(prevYear, prevMonth, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture);
                chartNote = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.Chart.Last3MonthsNote");
                var chartIntervals = GetMonthlyIntervals(mFrom, yFrom, count: 3);
                var chartRevTasks = chartIntervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
                var chartExpTasks = chartIntervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, monthId: i.month, yearId: i.year)).ToList();
                var chartSalTasks = chartIntervals.Select(i => GetSalaryCostAsync(monthId: i.month, yearId: i.year)).ToList();
                await Task.WhenAll(chartRevTasks.Concat(chartExpTasks));
                await Task.WhenAll(chartSalTasks);
                for (var i = 0; i < chartIntervals.Count; i++)
                {
                    var r = chartRevTasks[i].Result.TotalIncome;
                    var e = chartExpTasks[i].Result.TotalExpense;
                    revenuePoints.Add(r);
                    cashFlowPoints.Add(r - e);
                    cashInPoints.Add(r);
                    cashOutPoints.Add(e);
                    directCostPoints.Add(chartSalTasks[i].Result);
                    labels.Add(chartIntervals[i].label);
                }
                goto BuildResult;
            }

            {
                var intervals = GetMonthRangeIntervals(mFrom, yFrom, mTo, yTo);
                var prevIntervals = GetPreviousPeriodIntervals(mFrom, yFrom, intervals.Count);

                var sparkRevTasks = intervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
                var sparkExpTasks = intervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, monthId: i.month, yearId: i.year)).ToList();
                var sparkSalTasks = intervals.Select(i => GetSalaryCostAsync(monthId: i.month, yearId: i.year)).ToList();
                var prevRevTasks = prevIntervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
                var prevExpTasks = prevIntervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, monthId: i.month, yearId: i.year)).ToList();
                var prevSalTasks = prevIntervals.Select(i => GetSalaryCostAsync(monthId: i.month, yearId: i.year)).ToList();

                await Task.WhenAll(sparkRevTasks.Concat(sparkExpTasks).Concat(prevRevTasks).Concat(prevExpTasks));
                await Task.WhenAll(sparkSalTasks.Concat(prevSalTasks));

                revenue = sparkRevTasks.Sum(t => t.Result.TotalIncome);
                expense = sparkExpTasks.Sum(t => t.Result.TotalExpense);
                directCost = sparkSalTasks.Sum(t => t.Result);
                prevRevenue = prevRevTasks.Sum(t => t.Result.TotalIncome);
                prevExpense = prevExpTasks.Sum(t => t.Result.TotalExpense);
                prevDirectCost = prevSalTasks.Sum(t => t.Result);

                periodLabel = $"{new DateTime(yFrom, mFrom, 1):MMM yyyy} – {new DateTime(yTo, mTo, 1):MMM yyyy}";
                var prevFirst = prevIntervals.FirstOrDefault();
                var prevLast = prevIntervals.LastOrDefault();
                prevPeriodLabel = prevIntervals.Any()
                    ? $"{new DateTime(prevFirst.year, prevFirst.month, 1):MMM yyyy} – {new DateTime(prevLast.year, prevLast.month, 1):MMM yyyy}"
                    : "";

                for (var i = 0; i < intervals.Count; i++)
                {
                    var r = sparkRevTasks[i].Result.TotalIncome;
                    var e = sparkExpTasks[i].Result.TotalExpense;
                    revenuePoints.Add(r); cashFlowPoints.Add(r - e);
                    cashInPoints.Add(r); cashOutPoints.Add(e);
                    directCostPoints.Add(sparkSalTasks[i].Result);
                    labels.Add(intervals[i].label);
                }
            }
        }
        else if (granularityId == 4 && yearFrom.HasValue && yearTo.HasValue)
        {
            var yFrom = Math.Min(yearFrom.Value, yearTo.Value);
            var yTo = Math.Max(yearFrom.Value, yearTo.Value);
            var span = yTo - yFrom + 1;
            var fyStartMonth = await GetFyStartMonthAsync();
            var yearIntervals = GetFYRangeIntervals(yFrom, yTo, fyStartMonth);
            var prevYearIntervals = GetFYRangeIntervals(yFrom - span, yTo - span, fyStartMonth);

            var sparkRevTasks = yearIntervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
            var sparkExpTasks = yearIntervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, monthId: i.month, yearId: i.year)).ToList();
            var sparkSalTasks3 = yearIntervals.Select(i => GetSalaryCostAsync(monthId: i.month, yearId: i.year)).ToList();
            var prevRevTasks = prevYearIntervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
            var prevExpTasks = prevYearIntervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, monthId: i.month, yearId: i.year)).ToList();
            var prevSalTasks3 = prevYearIntervals.Select(i => GetSalaryCostAsync(monthId: i.month, yearId: i.year)).ToList();

            await Task.WhenAll(sparkRevTasks.Concat(sparkExpTasks).Concat(prevRevTasks).Concat(prevExpTasks));
            await Task.WhenAll(sparkSalTasks3.Concat(prevSalTasks3));

            revenue = sparkRevTasks.Sum(t => t.Result.TotalIncome);
            expense = sparkExpTasks.Sum(t => t.Result.TotalExpense);
            directCost = sparkSalTasks3.Sum(t => t.Result);
            prevRevenue = prevRevTasks.Sum(t => t.Result.TotalIncome);
            prevExpense = prevExpTasks.Sum(t => t.Result.TotalExpense);
            prevDirectCost = prevSalTasks3.Sum(t => t.Result);
            periodLabel = yFrom == yTo ? FyLabel(yFrom, fyStartMonth) : $"{FyLabel(yFrom, fyStartMonth)}–{FyLabel(yTo, fyStartMonth)}";
            prevPeriodLabel = span == 1 ? FyLabel(yFrom - 1, fyStartMonth) : $"{FyLabel(yFrom - span, fyStartMonth)}–{FyLabel(yFrom - 1, fyStartMonth)}";

            for (var i = 0; i < yearIntervals.Count; i++)
            {
                var r = sparkRevTasks[i].Result.TotalIncome;
                var e = sparkExpTasks[i].Result.TotalExpense;
                revenuePoints.Add(r);
                cashFlowPoints.Add(r - e);
                cashInPoints.Add(r);
                cashOutPoints.Add(e);
                directCostPoints.Add(sparkSalTasks3[i].Result);
                labels.Add(yearIntervals[i].label);
            }
        }

        BuildResult:
        var netCashFlow = revenue - expense;
        var prevNetCashFlow = prevRevenue - prevExpense;
        var grossPct = revenue > 0 ? Math.Round((revenue - directCost) / revenue * 100, 2) : (decimal?)null;
        var netPct = revenue > 0 ? Math.Round((revenue - expense) / revenue * 100, 2) : (decimal?)null;
        var prevGrossPct = prevRevenue > 0 ? Math.Round((prevRevenue - prevDirectCost) / prevRevenue * 100, 2) : (decimal?)null;
        var prevNetPct = prevRevenue > 0 ? Math.Round((prevRevenue - prevExpense) / prevRevenue * 100, 2) : (decimal?)null;

        return Json(new
        {
            revenue,
            directCost,
            grossPct,
            totalExpense = expense,
            netPct,
            netCashFlow,
            prevRevenue,
            prevNetCashFlow,
            prevGrossPct,
            prevNetPct,
            periodLabel,
            prevPeriodLabel,
            revenuePoints,
            cashFlowPoints,
            cashInPoints,
            cashOutPoints,
            directCostPoints,
            sparklineLabels = labels,
            chartNote
        });
    }
    private List<(int month, int year)> ResolveModalIntervals(int granularityId, int? monthFrom, int? yearFrom, int? monthTo, int? yearTo, int? yearFrom4, int? yearTo4, int fyStartMonth = 4)
    {
        if (granularityId == 3 && monthFrom.HasValue && yearFrom.HasValue && monthTo.HasValue && yearTo.HasValue)
        {
            var (mF, yF, mT, yT) = NormalizeMonthRange(monthFrom.Value, yearFrom.Value, monthTo.Value, yearTo.Value);
            return GetMonthRangeIntervals(mF, yF, mT, yT).Select(i => (i.month, i.year)).ToList();
        }
        if (granularityId == 4 && yearFrom4.HasValue && yearTo4.HasValue)
        {
            var yF = Math.Min(yearFrom4.Value, yearTo4.Value);
            var yT = Math.Max(yearFrom4.Value, yearTo4.Value);
            return GetFYRangeIntervals(yF, yT, fyStartMonth).Select(i => (i.month, i.year)).ToList();
        }
        return null;
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetRevenueDetail(int granularityId, DateTime? dateFrom, DateTime? dateTo, int? monthId, int? yearId, int? yearFrom, int? yearTo, int? monthFrom, int? monthTo)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardFinancial))
            return AccessDeniedView();

        var fyStartMonth = granularityId == 4 ? await GetFyStartMonthAsync() : 4;
        decimal revenue;
        IEnumerable<object> topInvoices;

        List<Domain.Invoice> allInvoicesForRevenue;
        var intervals = ResolveModalIntervals(granularityId, monthFrom, yearFrom, monthTo, yearTo, yearFrom, yearTo, fyStartMonth);
        if (intervals != null)
        {
            var revTasks = intervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
            var invTasks = intervals.Select(i => _accountManagementService.GetAllInvoicesAsync(monthId: i.month, yearId: i.year)).ToList();
            await Task.WhenAll(revTasks.Cast<Task>().Concat(invTasks.Cast<Task>()));
            revenue = revTasks.Sum(t => t.Result.TotalIncome);
            allInvoicesForRevenue = invTasks.SelectMany(t => t.Result ?? Enumerable.Empty<Domain.Invoice>()).ToList();
        }
        else
        {
            var summaryTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, dateFrom: dateFrom, dateTo: dateTo);
            var invoicesTask = _accountManagementService.GetAllInvoicesAsync(createdFromUTC: dateFrom, createdToUTC: dateTo);
            await Task.WhenAll(summaryTask, invoicesTask);
            revenue = summaryTask.Result.TotalIncome;
            allInvoicesForRevenue = (invoicesTask.Result ?? Enumerable.Empty<Domain.Invoice>()).ToList();
        }

        var revBillingMap  = await BuildBillingMapAsync(allInvoicesForRevenue);
        var revInrCurrency = await GetInrCurrencyAsync();

        var revInrAmounts = new Dictionary<int, decimal>();
        foreach (var inv in allInvoicesForRevenue)
        {
            var currencyId = revBillingMap.TryGetValue(inv.ProjectBillingId, out var b) ? b.PaymentCurrencyId : 0;
            revInrAmounts[inv.Id] = currencyId > 0
                ? await ToInrAsync(inv.TotalPaymentAmount, currencyId, revInrCurrency)
                : inv.TotalPaymentAmount;
        }

        topInvoices = allInvoicesForRevenue
            .OrderByDescending(x => revInrAmounts[x.Id]).Take(10)
            .Select(x => (object)new { invoiceNumber = x.InvoiceNumber, amount = revInrAmounts[x.Id], date = x.InvoiceDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture), statusId = x.StatusId });

        return Json(new { revenue, topInvoices });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetGrossProfitDetail(int granularityId, DateTime? dateFrom, DateTime? dateTo, int? monthId, int? yearId, int? yearFrom, int? yearTo, int? monthFrom, int? monthTo)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardFinancial))
            return AccessDeniedView();

        var fyStartMonth = granularityId == 4 ? await GetFyStartMonthAsync() : 4;
        decimal revenue = 0, directCost = 0;
        List<object> chartData = new();

        var intervals = ResolveModalIntervals(granularityId, monthFrom, yearFrom, monthTo, yearTo, yearFrom, yearTo, fyStartMonth);
        if (intervals != null)
        {
            var revTasks = intervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
            var salTasks = intervals.Select(i => GetSalaryCostAsync(monthId: i.month, yearId: i.year)).ToList();
            await Task.WhenAll(revTasks);
            await Task.WhenAll(salTasks);
            revenue = revTasks.Sum(t => t.Result.TotalIncome);
            directCost = salTasks.Sum(t => t.Result);
            chartData = intervals.Select((iv, idx) => (object)new { label = new DateTime(iv.year, iv.month, 1).ToString("MMM yy", CultureInfo.InvariantCulture), revenue = revTasks[idx].Result.TotalIncome }).ToList();
        }
        else
        {
            var summaryTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, dateFrom: dateFrom, dateTo: dateTo);
            var salaryTask = GetSalaryCostAsync(dateFrom: dateFrom, dateTo: dateTo);
            await Task.WhenAll(summaryTask, salaryTask);
            revenue = summaryTask.Result.TotalIncome;
            directCost = salaryTask.Result;
        }

        var grossProfit = revenue - directCost;
        var grossPct = revenue > 0 ? Math.Round(grossProfit / revenue * 100, 2) : (decimal?)null;
        return Json(new { revenue, directCost, grossProfit, grossPct, chartData });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetNetProfitDetail(int granularityId, DateTime? dateFrom, DateTime? dateTo, int? monthId, int? yearId, int? yearFrom, int? yearTo, int? monthFrom, int? monthTo)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardFinancial))
            return AccessDeniedView();

        var fyStartMonth = granularityId == 4 ? await GetFyStartMonthAsync() : 4;
        decimal revenue = 0, totalExpense = 0;
        List<object> chartData = new();

        var intervals = ResolveModalIntervals(granularityId, monthFrom, yearFrom, monthTo, yearTo, yearFrom, yearTo, fyStartMonth);
        if (intervals != null)
        {
            var revTasks = intervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
            var expTasks = intervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, monthId: i.month, yearId: i.year)).ToList();
            await Task.WhenAll(revTasks.Concat(expTasks));
            revenue = revTasks.Sum(t => t.Result.TotalIncome);
            totalExpense = expTasks.Sum(t => t.Result.TotalExpense);
            chartData = intervals.Select((iv, idx) => (object)new { label = new DateTime(iv.year, iv.month, 1).ToString("MMM yy", CultureInfo.InvariantCulture), revenue = revTasks[idx].Result.TotalIncome, expense = expTasks[idx].Result.TotalExpense }).ToList();
        }
        else
        {
            var incTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, dateFrom: dateFrom, dateTo: dateTo);
            var expTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 20, dateFrom: dateFrom, dateTo: dateTo);
            await Task.WhenAll(incTask, expTask);
            revenue = incTask.Result.TotalIncome;
            totalExpense = expTask.Result.TotalExpense;
        }

        var netProfit = revenue - totalExpense;
        var netPct = revenue > 0 ? Math.Round(netProfit / revenue * 100, 2) : (decimal?)null;
        return Json(new { revenue, totalExpense, netProfit, netPct, chartData });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetCashFlowDetail(int granularityId, DateTime? dateFrom, DateTime? dateTo, int? monthId, int? yearId, int? yearFrom, int? yearTo, int? monthFrom, int? monthTo)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardFinancial))
            return AccessDeniedView();

        var fyStartMonth = granularityId == 4 ? await GetFyStartMonthAsync() : 4;
        decimal cashIn = 0, cashOut = 0;
        var expenseRows = new List<object>();

        var salariesLabel = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.Modal.CashFlow.SalariesPayroll") ?? "Salaries & Payroll";
        var otherLabel    = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.Modal.CashFlow.Other") ?? "Other";

        var intervals = ResolveModalIntervals(granularityId, monthFrom, yearFrom, monthTo, yearTo, yearFrom, yearTo, fyStartMonth);
        if (intervals != null)
        {
            var incTasks = intervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
            var txnTasks = intervals.Select(i => _accountManagementService.GetAllAccountTransactionsAsync(transactionTypeId: 20, monthId: i.month, yearId: i.year)).ToList();
            var groupsTask = _accountManagementService.GetAllAccountGroupsAsync();
            await Task.WhenAll(incTasks.Cast<Task>().Concat(txnTasks.Cast<Task>()).Append(groupsTask));

            cashIn = incTasks.Sum(t => t.Result.TotalIncome);
            var allTxns = txnTasks.SelectMany(t => t.Result ?? Enumerable.Empty<Domain.AccountTransaction>()).ToList();
            cashOut = allTxns.Sum(t => t.Amount);
            BuildExpenseRows(allTxns, groupsTask.Result, expenseRows, salariesLabel, otherLabel);
        }
        else
        {
            var incTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, dateFrom: dateFrom, dateTo: dateTo);
            var txnTask = _accountManagementService.GetAllAccountTransactionsAsync(transactionTypeId: 20, dateFrom: dateFrom, dateTo: dateTo);
            var groupsTask = _accountManagementService.GetAllAccountGroupsAsync();
            await Task.WhenAll(incTask, txnTask, groupsTask);
            cashIn = incTask.Result.TotalIncome;
            var txnList = txnTask.Result ?? Enumerable.Empty<Domain.AccountTransaction>();
            cashOut = txnList.Sum(t => t.Amount);
            BuildExpenseRows(txnList, groupsTask.Result, expenseRows, salariesLabel, otherLabel);
        }

        return Json(new { cashIn, cashOut, netPosition = cashIn - cashOut, expenseRows });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetInvoiceStatusSummary(int granularityId, DateTime? dateFrom, DateTime? dateTo, int? yearFrom, int? yearTo, int? monthFrom, int? monthTo)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardFinancial))
            return AccessDeniedView();

        var today = DateTime.UtcNow.Date;
        var unpaidStatuses = new[] { (int)InvoiceEnum.Sent, (int)InvoiceEnum.PartiallyPaid };
        var fyStartMonth = granularityId == 4 ? await GetFyStartMonthAsync() : 4;

        List<Domain.Invoice> allInvoices;
        var intervals = ResolveModalIntervals(granularityId, monthFrom, yearFrom, monthTo, yearTo, yearFrom, yearTo, fyStartMonth);

        if (intervals != null)
        {
            var tasks = intervals.Select(i => _accountManagementService.GetAllInvoicesAsync(monthId: i.month, yearId: i.year)).ToList();
            await Task.WhenAll(tasks);
            allInvoices = tasks.SelectMany(t => t.Result ?? Enumerable.Empty<Domain.Invoice>()).ToList();
        }
        else
        {
            var result = await _accountManagementService.GetAllInvoicesAsync(createdFromUTC: dateFrom, createdToUTC: dateTo);
            allInvoices = result?.ToList() ?? new List<Domain.Invoice>();
        }
        var paid    = allInvoices.Where(i => i.StatusId == (int)InvoiceEnum.Paid).ToList();
        var overdue = allInvoices.Where(i => unpaidStatuses.Contains(i.StatusId) && i.DueDate != default && i.DueDate.Date < today).ToList();
        var pending = allInvoices.Where(i => unpaidStatuses.Contains(i.StatusId) && (i.DueDate == default || i.DueDate.Date >= today)).ToList();
        var draft   = allInvoices.Where(i => i.StatusId == (int)InvoiceEnum.Draft).ToList();
        var lblPaid      = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.InvoiceStatus.Paid") ?? "Paid";
        var lblOverdue   = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.InvoiceStatus.Overdue") ?? "Overdue";
        var lblPending   = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.InvoiceStatus.Pending") ?? "Pending";
        var lblCancelled = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.InvoiceStatus.Cancelled") ?? "Cancelled";
        var lblDraft     = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.InvoiceStatus.Draft") ?? "Draft";
        var billingMap  = await BuildBillingMapAsync(allInvoices);
        var inrCurrency = await GetInrCurrencyAsync();

        var inrAmounts = new Dictionary<int, decimal>();
        foreach (var inv in allInvoices)
        {
            var currencyId = billingMap.TryGetValue(inv.ProjectBillingId, out var b) ? b.PaymentCurrencyId : 0;
            inrAmounts[inv.Id] = currencyId > 0
                ? await ToInrAsync(inv.TotalPaymentAmount, currencyId, inrCurrency)
                : inv.TotalPaymentAmount;
        }

        var invoiceRows = allInvoices
            .OrderByDescending(i => inrAmounts[i.Id])
            .Select(i =>
            {
                var isOverdue = unpaidStatuses.Contains(i.StatusId) && i.DueDate != default && i.DueDate.Date < today;
                var statusLabel = i.StatusId == (int)InvoiceEnum.Paid ? lblPaid
                    : isOverdue ? lblOverdue
                    : unpaidStatuses.Contains(i.StatusId) ? lblPending
                    : i.StatusId == (int)InvoiceEnum.Cancelled ? lblCancelled
                    : lblDraft;
                return new
                {
                    id = i.Id,
                    invoiceNumber = i.InvoiceNumber,
                    title = string.IsNullOrWhiteSpace(i.Title) ? $"Invoice #{i.InvoiceNumber}" : i.Title,
                    invoiceDate = i.InvoiceDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                    dueDate = i.DueDate != default ? i.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : "—",
                    amount = inrAmounts[i.Id],
                    statusLabel,
                    daysOverdue = isOverdue ? (today - i.DueDate.Date).Days : 0
                };
            }).ToList();

        return Json(new
        {
            paid    = new { count = paid.Count,    amount = paid.Sum(i => inrAmounts[i.Id]) },
            overdue = new { count = overdue.Count, amount = overdue.Sum(i => inrAmounts[i.Id]) },
            pending = new { count = pending.Count, amount = pending.Sum(i => inrAmounts[i.Id]) },
            draft   = new { count = draft.Count,   amount = draft.Sum(i => inrAmounts[i.Id]) },
            invoices = invoiceRows
        });
    }

    private async Task<Currency> GetInrCurrencyAsync()
    {
        var currencies = await _currencyService.GetAllCurrenciesAsync(showHidden: true);
        return currencies.FirstOrDefault(c => c.CurrencyCode == "INR");
    }

    private async Task<Dictionary<int, Domain.ProjectBilling>> BuildBillingMapAsync(IEnumerable<Domain.Invoice> invoices)
    {
        var map = new Dictionary<int, Domain.ProjectBilling>();
        foreach (var id in invoices.Select(i => i.ProjectBillingId).Distinct())
        {
            var billing = await _accountManagementService.GetProjectBillingByIdAsync(id);
            if (billing != null)
                map[id] = billing;
        }
        return map;
    }

    private async Task<decimal> ToInrAsync(decimal amount, int paymentCurrencyId, Currency inrCurrency)
    {
        if (inrCurrency == null || paymentCurrencyId == inrCurrency.Id)
            return amount;
        var source = await _currencyService.GetCurrencyByIdAsync(paymentCurrencyId);
        if (source == null)
            return amount;
        return await _currencyService.ConvertCurrencyAsync(amount, source, inrCurrency);
    }

    private const string StatusCacheKey = "exec_dashboard_workflow_statuses_v2";

    private async Task<(HashSet<int> completedIds, HashSet<int> notStartedIds)> GetCachedStatusSetsAsync()
    {
        if (_memoryCache.TryGetValue(StatusCacheKey, out (HashSet<int> c, HashSet<int> n) cached))
            return cached;

        var statuses = await _workflowStatusService.GetAllWorkflowStatusAsync(pageSize: int.MaxValue);
        var completedIds = statuses
            .Where(s => (s.StatusName ?? "").Trim().ToLowerInvariant() == "closed")
            .Select(s => s.Id).ToHashSet();
        var notStartedIds = statuses
            .Where(s => (s.StatusName ?? "").Trim().ToLowerInvariant() == "new")
            .Select(s => s.Id).ToHashSet();

        var result = (completedIds, notStartedIds);
        _memoryCache.Set(StatusCacheKey, result, TimeSpan.FromMinutes(15));
        return result;
    }
    private static (DateTime from, DateTime to) ResolveTaskHealthPeriod(string period)
    {
        var today = DateTime.UtcNow.Date;
        return (period?.ToLowerInvariant()) switch
        {
            // No upper cap on last3/last6 so tasks due later this month or next are included
            "last3months" => (new DateTime(today.AddMonths(-3).Year, today.AddMonths(-3).Month, 1), DateTime.MaxValue),
            "last6months" => (new DateTime(today.AddMonths(-6).Year, today.AddMonths(-6).Month, 1), DateTime.MaxValue),
            "thisyear"    => (new DateTime(today.Year, 1, 1), DateTime.MaxValue),
            "lastyear"    => (new DateTime(today.Year - 1, 1, 1), new DateTime(today.Year - 1, 12, 31)),
            "last2years"  => (new DateTime(today.Year - 2, 1, 1), DateTime.MaxValue),
            _             => (DateTime.MinValue, DateTime.MaxValue) // alltime
        };
    }
    private static (DateTime dateFrom, DateTime dateTo, DateTime prevFrom, DateTime prevTo, string periodLabel, string prevPeriodLabel)
        ResolveOperationalPeriod(string period,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int? monthFrom = null, int? yearFrom = null,
            int? monthTo = null, int? yearTo = null,
            int fyStartMonth = 1)
    {
        var today = DateTime.UtcNow.Date;
        DateTime from, to, pFrom, pTo;
        string lbl, pLbl;

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            from = dateFrom.Value.Date;
            to = dateTo.Value.Date;
            var span = to - from;
            pFrom = from.AddDays(-span.TotalDays - 1);
            pTo = from.AddDays(-1);
            lbl = $"{from:dd MMM yyyy} – {to:dd MMM yyyy}";
            pLbl = $"{pFrom:dd MMM yyyy} – {pTo:dd MMM yyyy}";
            return (from, to, pFrom, pTo, lbl, pLbl);
        }

        if (monthFrom.HasValue && yearFrom.HasValue && monthTo.HasValue && yearTo.HasValue)
        {
            from = new DateTime(yearFrom.Value, monthFrom.Value, 1);
            to = new DateTime(yearTo.Value, monthTo.Value, 1).AddMonths(1).AddDays(-1);
            if (to > today) to = today;
            var monthCount = ((yearTo.Value - yearFrom.Value) * 12) + (monthTo.Value - monthFrom.Value) + 1;
            pFrom = from.AddMonths(-monthCount);
            pTo = from.AddDays(-1);
            lbl = from.Year == to.Year ? $"{from:MMM} – {to:MMM yyyy}" : $"{from:MMM yyyy} – {to:MMM yyyy}";
            pLbl = pFrom.Year == pTo.Year ? $"{pFrom:MMM} – {pTo:MMM yyyy}" : $"{pFrom:MMM yyyy} – {pTo:MMM yyyy}";
            return (from, to, pFrom, pTo, lbl, pLbl);
        }
        if (yearFrom.HasValue && yearTo.HasValue && monthFrom == null)
        {
            from = new DateTime(yearFrom.Value, 1, 1);
            to = new DateTime(yearTo.Value, 12, 31);
            if (to > today) to = today;
            var yCount = yearTo.Value - yearFrom.Value + 1;
            pFrom = new DateTime(yearFrom.Value - yCount, 1, 1);
            pTo = new DateTime(yearFrom.Value - 1, 12, 31);
            lbl = yearFrom.Value == yearTo.Value ? yearFrom.Value.ToString() : $"{yearFrom.Value} – {yearTo.Value}";
            pLbl = pFrom.Year == pTo.Year ? pFrom.Year.ToString() : $"{pFrom.Year} – {pTo.Year}";
            return (from, to, pFrom, pTo, lbl, pLbl);
        }

        // Named preset token
        switch (period?.ToLowerInvariant())
        {
            case "thisweek":
                from = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
                if (from > today) from = from.AddDays(-7);
                to = today;
                pFrom = from.AddDays(-7);
                pTo = from.AddDays(-1);
                lbl = $"{from:dd MMM} – {to:dd MMM yyyy}";
                pLbl = $"{pFrom:dd MMM} – {pTo:dd MMM yyyy}";
                break;
            case "lastweek":
                var thisMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
                if (thisMonday > today) thisMonday = thisMonday.AddDays(-7);
                from = thisMonday.AddDays(-7);
                to = thisMonday.AddDays(-1);
                pFrom = from.AddDays(-7);
                pTo = from.AddDays(-1);
                lbl = $"{from:dd MMM} – {to:dd MMM yyyy}";
                pLbl = $"{pFrom:dd MMM} – {pTo:dd MMM yyyy}";
                break;
            case "currentmonth":
                from = new DateTime(today.Year, today.Month, 1);
                to = today;
                pFrom = from.AddMonths(-1);
                pTo = from.AddDays(-1);
                lbl = today.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                pLbl = pFrom.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                break;
            case "lastmonth":
                var lm = today.AddMonths(-1);
                from = new DateTime(lm.Year, lm.Month, 1);
                to = from.AddMonths(1).AddDays(-1);
                pFrom = from.AddMonths(-1);
                pTo = from.AddDays(-1);
                lbl = from.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                pLbl = pFrom.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                break;
            case "last6months":
                from = new DateTime(today.AddMonths(-6).Year, today.AddMonths(-6).Month, 1);
                to = today;
                pFrom = from.AddMonths(-6);
                pTo = from.AddDays(-1);
                lbl = $"{from:MMM yyyy} – {to:MMM yyyy}";
                pLbl = $"{pFrom:MMM yyyy} – {pTo:MMM yyyy}";
                break;
            case "thisyear":
                if (fyStartMonth <= 1)
                {
                    from = new DateTime(today.Year, 1, 1);
                    to = today;
                    pFrom = new DateTime(today.Year - 1, 1, 1);
                    pTo = new DateTime(today.Year - 1, 12, 31);
                    lbl = today.Year.ToString();
                    pLbl = (today.Year - 1).ToString();
                }
                else
                {
                    var fyYear = today.Month >= fyStartMonth ? today.Year : today.Year - 1;
                    from = new DateTime(fyYear, fyStartMonth, 1);
                    to = today;
                    pFrom = new DateTime(fyYear - 1, fyStartMonth, 1);
                    pTo = from.AddDays(-1);
                    lbl = FyLabel(fyYear, fyStartMonth);
                    pLbl = FyLabel(fyYear - 1, fyStartMonth);
                }
                break;
            case "lastyear":
                if (fyStartMonth <= 1)
                {
                    from = new DateTime(today.Year - 1, 1, 1);
                    to = new DateTime(today.Year - 1, 12, 31);
                    pFrom = new DateTime(today.Year - 2, 1, 1);
                    pTo = new DateTime(today.Year - 2, 12, 31);
                    lbl = (today.Year - 1).ToString();
                    pLbl = (today.Year - 2).ToString();
                }
                else
                {
                    var fyYear = today.Month >= fyStartMonth ? today.Year : today.Year - 1;
                    var prevFyYear = fyYear - 1;
                    from = new DateTime(prevFyYear, fyStartMonth, 1);
                    to = new DateTime(fyYear, fyStartMonth, 1).AddDays(-1);
                    pFrom = new DateTime(prevFyYear - 1, fyStartMonth, 1);
                    pTo = from.AddDays(-1);
                    lbl = FyLabel(prevFyYear, fyStartMonth);
                    pLbl = FyLabel(prevFyYear - 1, fyStartMonth);
                }
                break;
            default:
                from = new DateTime(today.AddMonths(-3).Year, today.AddMonths(-3).Month, 1);
                to = today;
                pFrom = from.AddMonths(-3);
                pTo = from.AddDays(-1);
                lbl = $"{from:MMM yyyy} – {to:MMM yyyy}";
                pLbl = $"{pFrom:MMM yyyy} – {pTo:MMM yyyy}";
                break;
        }
        return (from, to, pFrom, pTo, lbl, pLbl);
    }

    private static decimal ToHours(int hours, int minutes) => hours + Math.Round(minutes / 60m, 2);

    private static List<(DateTime from, DateTime to, string label)> GetMonthlyDateIntervals(DateTime dateFrom, DateTime dateTo)
    {
        var result = new List<(DateTime, DateTime, string)>();
        var singleYear = dateFrom.Year == dateTo.Year;
        var fmt = singleYear ? "MMM" : "MMM yy";
        var cur = new DateTime(dateFrom.Year, dateFrom.Month, 1);
        while (cur <= dateTo)
        {
            var end = cur.AddMonths(1).AddDays(-1);
            if (end > dateTo) end = dateTo;
            result.Add((cur, end, cur.ToString(fmt, CultureInfo.InvariantCulture)));
            cur = cur.AddMonths(1);
        }
        return result;
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetEmployeesList()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational))
            return AccessDeniedView();

        var employees = await _employeeService.GetAllEmployeesAsync(pageSize: int.MaxValue, showInActive: false);
        var result = employees
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            .Select(e => new { id = e.Id, name = $"{e.FirstName} {e.LastName}".Trim() })
            .ToList<object>();
        return Json(result);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetProjectsList()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational))
            return AccessDeniedView();

        var projects = await _projectsService.GetAllProjectsAsync(pageSize: int.MaxValue);
        var result = projects
            .OrderBy(p => p.ProjectTitle)
            .Select(p => new { id = p.Id, title = p.ProjectTitle ?? "" })
            .ToList<object>();
        return Json(result);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetOperationalMetricsData(string period = "lastmonth", int? projectId = null, int? employeeId = null,
        DateTime? customFrom = null, DateTime? customTo = null, int? monthFrom = null, int? yearFrom = null, int? monthTo = null, int? yearTo = null,
        string taskPeriod = "last3months")
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational))
            return AccessDeniedView();

        var fyStartMonth = await GetFyStartMonthAsync();
        var (dateFrom, dateTo, prevFrom, prevTo, periodLabel, prevPeriodLabel) = ResolveOperationalPeriod(period, customFrom, customTo, monthFrom, yearFrom, monthTo, yearTo, fyStartMonth);
        var today = DateTime.UtcNow.Date;
        var pIds = projectId.HasValue ? new List<int> { projectId.Value } : null;
        var eIds = employeeId.HasValue ? new List<int> { employeeId.Value } : null;
        var allTasksTask   = _projectTaskService.GetAllProjectTasksAsync(projectIds: pIds, pageSize: int.MaxValue);
        var currentTsTask  = _timeSheetsService.GetAllTimeSheetAsync(employeeIds: eIds, projectIds: pIds, from: dateFrom, to: dateTo, pageSize: int.MaxValue);
        var prevTsTask     = _timeSheetsService.GetAllTimeSheetAsync(employeeIds: eIds, projectIds: pIds, from: prevFrom, to: prevTo, pageSize: int.MaxValue);
        var statusSetsTask = GetCachedStatusSetsAsync();
        var projectsTask   = _projectsService.GetAllProjectsAsync(pageSize: int.MaxValue);
        await Task.WhenAll(allTasksTask, currentTsTask, prevTsTask, statusSetsTask, projectsTask);

        var (completedIds, notStartedIds) = statusSetsTask.Result;
        var (thPeriodFrom, thPeriodTo)    = ResolveTaskHealthPeriod(taskPeriod);

        var bugCountByParent = allTasksTask.Result
            .Where(t => t.Tasktypeid == (int)TaskTypeEnum.Bug && t.ParentTaskId > 0)
            .GroupBy(t => t.ParentTaskId)
            .ToDictionary(g => g.Key, g => g.Count());
        var allTaskMap = allTasksTask.Result.ToDictionary(t => t.Id);

        var allTasks = allTasksTask.Result
            .Where(t => t.Tasktypeid != (int)TaskTypeEnum.UserStory).ToList();
        var curTs = currentTsTask.Result.ToList();
        var prevTs = prevTsTask.Result.ToList();
        var curTotalHrs = curTs.Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
        var curBillHrs = curTs.Where(t => t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
        var billableUtilPct = curTotalHrs > 0 ? Math.Round(curBillHrs / curTotalHrs * 100, 1) : 0m;
        var prevTotalHrs = prevTs.Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
        var prevBillHrs = prevTs.Where(t => t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
        var prevBillableUtilPct = prevTotalHrs > 0 ? Math.Round(prevBillHrs / prevTotalHrs * 100, 1) : 0m;
        var curTsTaskIds  = curTs.Where(t => t.TaskId > 0).Select(t => t.TaskId).ToHashSet();
        var prevTsTaskIds = prevTs.Where(t => t.TaskId > 0).Select(t => t.TaskId).ToHashSet();

        static int ResolveTaskId(ProjectTask t, Dictionary<int, App.Core.Domain.ProjectTasks.ProjectTask> map)
            => t.Tasktypeid == 3 && t.ParentTaskId > 0 && map.ContainsKey(t.ParentTaskId)
               ? t.ParentTaskId : t.Id;

        var curPeriodTasks = allTasks
            .Where(t => curTsTaskIds.Contains(t.Id))
            .Select(t => allTaskMap.GetValueOrDefault(ResolveTaskId(t, allTaskMap), t))
            .DistinctBy(t => t.Id)
            .ToList();

        var prevPeriodTasks = allTasks
            .Where(t => prevTsTaskIds.Contains(t.Id))
            .Select(t => allTaskMap.GetValueOrDefault(ResolveTaskId(t, allTaskMap), t))
            .DistinctBy(t => t.Id)
            .ToList();

        var curDotTasks  = curPeriodTasks.Where(t => t.DOTPercentage.HasValue).ToList();
        var prevDotTasks = prevPeriodTasks.Where(t => t.DOTPercentage.HasValue).ToList();
        var dotPct      = curDotTasks.Count > 0 ? Math.Round(curDotTasks.Average(t => t.DOTPercentage!.Value), 1) : 0m;
        var prevDotPct  = prevDotTasks.Count > 0 ? Math.Round(prevDotTasks.Average(t => t.DOTPercentage!.Value), 1) : 0m;
        var curQualTasks  = curPeriodTasks.Where(t => t.WorkQuality.HasValue).ToList();
        var prevQualTasks = prevPeriodTasks.Where(t => t.WorkQuality.HasValue).ToList();
        var workQualityScore = curQualTasks.Count > 0 ? Math.Round(curQualTasks.Average(t => t.WorkQuality.Value), 1) : (decimal?)null;
        var prevWorkQualityScore = prevQualTasks.Count > 0 ? Math.Round(prevQualTasks.Average(t => t.WorkQuality.Value), 1) : (decimal?)null;
        var workQualityTaskCount = curQualTasks.Count;
        var workQualityBugCount = curQualTasks.Sum(t => bugCountByParent.GetValueOrDefault(t.Id, 0));
        var overdueTaskCount = allTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date < today && !completedIds.Contains(t.StatusId));
        var prevOverdueTaskCount = allTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date < today.AddDays(-30) && !completedIds.Contains(t.StatusId));
        var thTasks = allTasks
            .Where(t => thPeriodFrom == DateTime.MinValue ||
                        (t.DueDate.HasValue && t.DueDate.Value.Date >= thPeriodFrom && t.DueDate.Value.Date <= thPeriodTo))
            .ToList();
        var taskHealth = new
        {
            completed  = thTasks.Count(t => completedIds.Contains(t.StatusId)),
            inProgress = thTasks.Count(t => !completedIds.Contains(t.StatusId) && !notStartedIds.Contains(t.StatusId) && !(t.DueDate.HasValue && t.DueDate.Value.Date < today)),
            notStarted = thTasks.Count(t => notStartedIds.Contains(t.StatusId) && !(t.DueDate.HasValue && t.DueDate.Value.Date < today)),
            overdue    = thTasks.Count(t => !completedIds.Contains(t.StatusId) && t.DueDate.HasValue && t.DueDate.Value.Date < today),
            total      = thTasks.Count
        };
        var intervals = GetMonthlyDateIntervals(dateFrom, dateTo);
        var billableHoursPoints = new List<decimal>();
        var nonBillableHoursPoints = new List<decimal>();
        var billableUtilPoints = new List<decimal>();
        var dotPoints = new List<decimal>();
        var workQualityPoints = new List<decimal?>();
        var sparklineLabels = new List<string>();

        foreach (var iv in intervals)
        {
            var mTs = curTs.Where(t => t.SpentDate.Date >= iv.from.Date && t.SpentDate.Date <= iv.to.Date).ToList();
            var mTotal = mTs.Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
            var mBill = mTs.Where(t => t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
            billableHoursPoints.Add(Math.Round(mBill, 1));
            nonBillableHoursPoints.Add(Math.Round(mTotal - mBill, 1));
            billableUtilPoints.Add(mTotal > 0 ? Math.Round(mBill / mTotal * 100, 1) : 0m);

            var mTsTaskIds   = mTs.Where(t => t.TaskId > 0).Select(t => t.TaskId).ToHashSet();
            var mPeriodTasks = allTasks
                .Where(t => mTsTaskIds.Contains(t.Id))
                .Select(t => allTaskMap.GetValueOrDefault(ResolveTaskId(t, allTaskMap), t))
                .DistinctBy(t => t.Id)
                .ToList();
            var mDot  = mPeriodTasks.Where(t => t.DOTPercentage.HasValue).ToList();
            dotPoints.Add(mDot.Count > 0 ? Math.Round(mDot.Average(t => t.DOTPercentage!.Value), 1) : 0m);
            var mQual = mPeriodTasks.Where(t => t.WorkQuality.HasValue).ToList();
            workQualityPoints.Add(mQual.Count > 0 ? Math.Round(mQual.Average(t => t.WorkQuality.Value), 1) : (decimal?)null);

            sparklineLabels.Add(iv.label);
        }

        var projectMap = projectsTask.Result.ToDictionary(p => p.Id, p => p.ProjectTitle ?? $"Project #{p.Id}");

        var isAllProjects   = !projectId.HasValue || projectId.Value <= 0;
        var isEmpFilter     = employeeId.HasValue && employeeId.Value > 0;

        object projectRadar;
        if (isAllProjects && isEmpFilter)
        {
            var empTsTaskIds  = curTs.Where(t => t.TaskId > 0).Select(t => t.TaskId).ToHashSet();
            var empPeriodTasks = allTasks
                .Where(t => empTsTaskIds.Contains(t.Id))
                .Select(t => allTaskMap.GetValueOrDefault(ResolveTaskId(t, allTaskMap), t))
                .DistinctBy(t => t.Id)
                .ToList();

            var empOverdue   = empPeriodTasks.Count(t => !completedIds.Contains(t.StatusId) && t.DueDate.HasValue && t.DueDate.Value.Date < today);
            var empOvrdHlth  = empPeriodTasks.Count > 0 ? Math.Round(100m - (decimal)empOverdue / empPeriodTasks.Count * 100, 1) : 100m;
            var avgLabel     = await _localizationService.GetResourceAsync("Dashboard.TeamAverage");

            var perProjectEntries = curTs
                .GroupBy(t => t.ProjectId)
                .Where(g => g.Key > 0 && projectMap.ContainsKey(g.Key))
                .Select(g =>
                {
                    var pid      = g.Key;
                    var pTotal   = g.Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
                    var pBill    = g.Where(t => t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
                    var pTasks   = empPeriodTasks.Where(t => t.ProjectId == pid).ToList();
                    var pDotTasks     = pTasks.Where(t => t.DOTPercentage.HasValue).ToList();
                    var pDot          = pDotTasks.Count > 0 ? Math.Round(pDotTasks.Average(t => t.DOTPercentage!.Value), 1) : 0m;
                    var pQualT        = pTasks.Where(t => t.WorkQuality.HasValue).ToList();
                    var pQual         = pQualT.Count > 0 ? Math.Round(pQualT.Average(t => t.WorkQuality.Value), 1) : 0m;
                    var pOverdue      = pTasks.Count(t => !completedIds.Contains(t.StatusId) && t.DueDate.HasValue && t.DueDate.Value.Date < today);
                    var pOverdueHlth  = pTasks.Count > 0 ? Math.Round(100m - (decimal)pOverdue / pTasks.Count * 100, 1) : 100m;
                    return new
                    {
                        project       = projectMap[pid],
                        totalHrs      = Math.Round(pTotal, 1),
                        billablePct   = pTotal > 0 ? Math.Round(pBill / pTotal * 100, 1) : 0m,
                        dotPct        = pDot,
                        workQuality   = pQual,
                        overdueHealth = pOverdueHlth,
                        isAverage     = false
                    };
                })
                .OrderByDescending(p => p.totalHrs)
                .Take(6)
                .ToList();

            perProjectEntries.Add(new
            {
                project       = avgLabel,
                totalHrs      = Math.Round(curTotalHrs, 1),
                billablePct   = billableUtilPct,
                dotPct        = dotPct,
                workQuality   = workQualityScore ?? 0m,
                overdueHealth = empOvrdHlth,
                isAverage     = true
            });
            projectRadar = perProjectEntries.ToArray();
        }
        else
        {
            var overdueInPeriod  = curPeriodTasks.Count(t => !completedIds.Contains(t.StatusId) && t.DueDate.HasValue && t.DueDate.Value.Date < today);
            var radarOvrdHealth  = curPeriodTasks.Count > 0
                ? Math.Round(100m - (decimal)overdueInPeriod / curPeriodTasks.Count * 100, 1)
                : 100m;
            var radarLabel = isAllProjects
    ? await _localizationService.GetResourceAsync("Dashboard.TeamAverage")
    : (projectMap.TryGetValue(projectId!.Value, out var pn)
        ? pn
        : string.Format(
            await _localizationService.GetResourceAsync("Dashboard.ProjectFallback"),
            projectId.Value));
            projectRadar = new[] { new {
                project       = radarLabel,
                totalHrs      = Math.Round(curTotalHrs, 1),
                billablePct   = billableUtilPct,
                dotPct,
                workQuality   = workQualityScore ?? 0m,
                overdueHealth = radarOvrdHealth,
                isAverage     = isAllProjects
            }};
        }

        return Json(new
        {
            billableUtilPct, prevBillableUtilPct,
            dotPct, prevDotPct,
            workQualityScore, prevWorkQualityScore, workQualityTaskCount, workQualityBugCount,
            overdueTaskCount, prevOverdueTaskCount,
            billableHoursPoints, nonBillableHoursPoints, billableUtilPoints, dotPoints, workQualityPoints,
            sparklineLabels, periodLabel, prevPeriodLabel,
            taskHealth, projectRadar
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetTaskStatusBreakdown(int? projectId = null, int? employeeId = null, string taskPeriod = "alltime")
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational))
            return AccessDeniedView();

        var today = DateTime.UtcNow.Date;
        var (periodFrom, periodTo) = ResolveTaskHealthPeriod(taskPeriod);
        var pIds = projectId.HasValue ? new List<int> { projectId.Value } : null;
        var eIds = employeeId.HasValue ? new List<int> { employeeId.Value } : null;

        var allTasksTask   = _projectTaskService.GetAllProjectTasksAsync(projectIds: pIds, pageSize: int.MaxValue);
        var statusSetsTask = GetCachedStatusSetsAsync();
        await Task.WhenAll(allTasksTask, statusSetsTask);

        var (completedIds, notStartedIds) = statusSetsTask.Result;

        var tasks = allTasksTask.Result
            .Where(t => t.Tasktypeid != (int)TaskTypeEnum.UserStory)
            .Where(t => periodFrom == DateTime.MinValue ||
                        (t.DueDate.HasValue && t.DueDate.Value.Date >= periodFrom && t.DueDate.Value.Date <= periodTo))
            .ToList();

        return Json(new
        {
            completed  = tasks.Count(t => completedIds.Contains(t.StatusId)),
            inProgress = tasks.Count(t => !completedIds.Contains(t.StatusId) && !notStartedIds.Contains(t.StatusId) && !(t.DueDate.HasValue && t.DueDate.Value.Date < today)),
            notStarted = tasks.Count(t => notStartedIds.Contains(t.StatusId) && !(t.DueDate.HasValue && t.DueDate.Value.Date < today)),
            overdue    = tasks.Count(t => !completedIds.Contains(t.StatusId) && t.DueDate.HasValue && t.DueDate.Value.Date < today),
            total      = tasks.Count
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetTaskStatusList(string status = "inprogress", int page = 0, int pageSize = 15, int? projectId = null, int? employeeId = null, string taskPeriod = "alltime")
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational))
            return AccessDeniedView();

        var today = DateTime.UtcNow.Date;
        var (periodFrom, periodTo) = ResolveTaskHealthPeriod(taskPeriod);
        var pIds = projectId.HasValue ? new List<int> { projectId.Value } : null;
        var eIds = employeeId.HasValue ? new List<int> { employeeId.Value } : null;

        var allTasksTask   = _projectTaskService.GetAllProjectTasksAsync(projectIds: pIds, pageSize: int.MaxValue);
        var statusesTask   = _workflowStatusService.GetAllWorkflowStatusAsync(pageSize: int.MaxValue); 
        var statusSetsTask = GetCachedStatusSetsAsync();
        await Task.WhenAll(allTasksTask, statusesTask, statusSetsTask);

        var (completedIds, notStartedIds) = statusSetsTask.Result;
        var statusMap = statusesTask.Result.ToDictionary(s => s.Id, s => s.StatusName ?? "");

        var allTasks = allTasksTask.Result
            .Where(t => t.Tasktypeid != (int)TaskTypeEnum.UserStory)
            .Where(t => periodFrom == DateTime.MinValue ||
                        (t.DueDate.HasValue && t.DueDate.Value.Date >= periodFrom && t.DueDate.Value.Date <= periodTo))
            .ToList();

        IEnumerable<App.Core.Domain.ProjectTasks.ProjectTask> filtered = (status ?? "").ToLowerInvariant() switch
        {
            "completed"  => allTasks.Where(t => completedIds.Contains(t.StatusId)),
            "overdue"    => allTasks.Where(t => !completedIds.Contains(t.StatusId) && t.DueDate.HasValue && t.DueDate.Value.Date < today),
            "notstarted" => allTasks.Where(t => notStartedIds.Contains(t.StatusId) && !(t.DueDate.HasValue && t.DueDate.Value.Date < today)),
            _            => allTasks.Where(t => !completedIds.Contains(t.StatusId) && !notStartedIds.Contains(t.StatusId) && !(t.DueDate.HasValue && t.DueDate.Value.Date < today))
        };

        var filteredList = filtered.ToList();
        var total = filteredList.Count;
        var pageTasks = filteredList.Skip(page * pageSize).Take(pageSize).ToList();

        var employeesTask = _employeeService.GetAllEmployeesAsync(pageSize: int.MaxValue, showInActive: true);
        var projectsTask = _projectsService.GetAllProjectsAsync(pageSize: int.MaxValue);
        await Task.WhenAll(employeesTask, projectsTask);

        var employeeMap = employeesTask.Result.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());
        var projectMap = projectsTask.Result.ToDictionary(p => p.Id, p => p.ProjectTitle ?? "");

        var taskRows = pageTasks.Select(t => new
        {
            id = t.Id,
            title = t.TaskTitle ?? "",
            projectName = projectMap.GetValueOrDefault(t.ProjectId, ""),
            assignee = t.DeveloperId > 0 ? employeeMap.GetValueOrDefault(t.DeveloperId, "") : "",
            dueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : "",
            statusName = statusMap.GetValueOrDefault(t.StatusId, ""),
            isOverdue = !completedIds.Contains(t.StatusId) && t.DueDate.HasValue && t.DueDate.Value.Date < today
        }).ToList<object>();

        return Json(new
        {
            tasks = taskRows,
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetBillableUtilDetail(string period = "last3months", int? projectId = null, int? employeeId = null,
        DateTime? customFrom = null, DateTime? customTo = null, int? monthFrom = null, int? yearFrom = null, int? monthTo = null, int? yearTo = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational))
            return AccessDeniedView();

        var fyStartMonth = await GetFyStartMonthAsync();
        var (dateFrom, dateTo, _, _, _, _) = ResolveOperationalPeriod(period, customFrom, customTo, monthFrom, yearFrom, monthTo, yearTo, fyStartMonth);
        var pIds = projectId.HasValue ? new List<int> { projectId.Value } : null;
        var eIds = employeeId.HasValue ? new List<int> { employeeId.Value } : null;

        var tsTask = _timeSheetsService.GetAllTimeSheetAsync(employeeIds: eIds, projectIds: pIds, from: dateFrom, to: dateTo, pageSize: int.MaxValue);
        var employeesTask = _employeeService.GetAllEmployeesAsync(pageSize: int.MaxValue, showInActive: false);
        var projectsTask = _projectsService.GetAllProjectsAsync(pageSize: int.MaxValue);
        await Task.WhenAll(tsTask, employeesTask, projectsTask);

        var tsList = tsTask.Result.ToList();
        var employeeMap = employeesTask.Result.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());
        var projectMap = projectsTask.Result.ToDictionary(p => p.Id, p => p.ProjectTitle ?? "");

        var totalHrs = tsList.Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
        var billHrs = tsList.Where(t => t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));

        var intervals = GetMonthlyDateIntervals(dateFrom, dateTo);
        var chartData = intervals.Select(iv =>
        {
            var mTs = tsList.Where(t => t.SpentDate.Date >= iv.from.Date && t.SpentDate.Date <= iv.to.Date).ToList();
            var mBill = mTs.Where(t => t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
            var mNon = mTs.Where(t => !t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
            return new { label = iv.label, billable = Math.Round(mBill, 1), nonBillable = Math.Round(mNon, 1) };
        }).ToList<object>();

        var topEmployees = tsList
            .GroupBy(t => t.EmployeeId)
            .Where(g => g.Key > 0 && employeeMap.ContainsKey(g.Key))
            .Select(g =>
            {
                var b = g.Where(t => t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
                var nb = g.Where(t => !t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
                var tot = b + nb;
                return new
                {
                    employee = employeeMap[g.Key],
                    billableHrs = Math.Round(b, 1),
                    nonBillableHrs = Math.Round(nb, 1),
                    utilPct = tot > 0 ? Math.Round(b / tot * 100, 1) : 0m
                };
            })
            .OrderByDescending(x => x.billableHrs).ToList<object>();
        var noProjectText = await _localizationService.GetResourceAsync("Dashboard.NoProject");
        var projectFallback = await _localizationService.GetResourceAsync("Dashboard.ProjectFallback");
        var projectUtil = tsList
            .GroupBy(t => t.ProjectId)
            .Select(g =>
            {
                var b = g.Where(t => t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
                var nb = g.Where(t => !t.Billable).Sum(t => ToHours(t.SpentHours, t.SpentMinutes));
                var tot = b + nb;
                return new
                {
                    project = projectMap.GetValueOrDefault(
                        g.Key,
                        g.Key > 0
                            ? string.Format(projectFallback, g.Key)
                            : noProjectText),

                    billableHrs = Math.Round(b, 1),
                    nonBillableHrs = Math.Round(nb, 1),
                    totalHrs = Math.Round(tot, 1),
                    utilPct = tot > 0 ? Math.Round(b / tot * 100, 1) : 0m
                };
            })
            .OrderByDescending(x => x.billableHrs).ToList<object>();

        return Json(new { totalHrs = Math.Round(totalHrs, 1), billableHrs = Math.Round(billHrs, 1), nonBillableHrs = Math.Round(totalHrs - billHrs, 1), chartData, topEmployees, projectUtil });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetDotDetail(
        string period = "last3months",
        int? projectId = null,
        int? employeeId = null,
        DateTime? customFrom = null,
        DateTime? customTo = null,
        int? monthFrom = null,
        int? yearFrom = null,
        int? monthTo = null,
        int? yearTo = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational))
            return AccessDeniedView();

        var fyStartMonth = await GetFyStartMonthAsync();
        var (dateFrom, dateTo, _, _, _, _) = ResolveOperationalPeriod(period, customFrom, customTo, monthFrom, yearFrom, monthTo, yearTo, fyStartMonth);

        var pIds = projectId.HasValue ? new List<int> { projectId.Value } : null;
        var eIds = employeeId.HasValue ? new List<int> { employeeId.Value } : null;

        var tsTask = _timeSheetsService.GetAllTimeSheetAsync(
            employeeIds: eIds,
            projectIds: pIds,
            from: dateFrom,
            to: dateTo,
            pageSize: int.MaxValue
        );

        var allTasksTask = _projectTaskService.GetAllProjectTasksAsync(projectIds: pIds, pageSize: int.MaxValue);
        var employeesTask = _employeeService.GetAllEmployeesAsync(pageSize: int.MaxValue, showInActive: false);
        var projectsTask = _projectsService.GetAllProjectsAsync(pageSize: int.MaxValue);

        await Task.WhenAll(allTasksTask, employeesTask, projectsTask, tsTask);

        var employeeMap = employeesTask.Result.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());
        var projectMap = projectsTask.Result.ToDictionary(p => p.Id, p => p.ProjectTitle ?? "");

        var ts = tsTask.Result.ToList();

        var rawTaskIds   = ts.Where(t => t.TaskId > 0).Select(t => t.TaskId).ToHashSet();
        var allTaskMapD  = allTasksTask.Result.ToDictionary(t => t.Id);
        var rawToResolvedD = rawTaskIds.ToDictionary(
            id => id,
            id => allTaskMapD.TryGetValue(id, out var rt) && rt.Tasktypeid == 3 && rt.ParentTaskId > 0 && allTaskMapD.ContainsKey(rt.ParentTaskId)
                  ? rt.ParentTaskId : id);
        var resolvedTaskIds  = rawToResolvedD.Values.ToHashSet();
        var empResolvedIdsD  = employeeId.HasValue
            ? ts.Where(x => x.TaskId > 0 && x.EmployeeId == employeeId.Value)
               .Select(x => rawToResolvedD.GetValueOrDefault(x.TaskId, x.TaskId))
               .ToHashSet()
            : null;

        var periodTasks = allTasksTask.Result
            .Where(t => t.Tasktypeid != (int)TaskTypeEnum.UserStory &&
                        resolvedTaskIds.Contains(t.Id) &&
                        (empResolvedIdsD == null || empResolvedIdsD.Contains(t.Id)))
            .ToList();
        var dotTasks = periodTasks.Where(t => t.DOTPercentage.HasValue).ToList();

        var dotPct = dotTasks.Count > 0
            ? Math.Round(dotTasks.Average(t => t.DOTPercentage.Value), 1)
            : 0m;
        var onTime = dotTasks.Count(t => t.DeliveryOnTime);
        var late = dotTasks.Count - onTime;
        var intervals = GetMonthlyDateIntervals(dateFrom, dateTo);
        var chartData = intervals.Select(iv =>
        {
            var mTaskIds = ts
                .Where(t => t.SpentDate.Date >= iv.from.Date && t.SpentDate.Date <= iv.to.Date)
                .Select(t => t.TaskId)
                .ToHashSet();
            var mTasks = periodTasks
                .Where(t => mTaskIds.Contains(t.Id))
                .ToList();
            var dotValues = mTasks
                .Where(t => t.DOTPercentage.HasValue)
                .Select(t => t.DOTPercentage.Value);
            var pct = dotValues.Any()
                ? Math.Round(dotValues.Average(), 1)
                : 0m;

            return new { label = iv.label, dotPct = pct };
        }).ToList<object>();
        var employeeDot = periodTasks
            .GroupBy(t => t.DeveloperId)
            .Where(g => g.Key > 0 && employeeMap.ContainsKey(g.Key))
            .Select(g =>
            {
                var total = g.Count();
                var onTimeCnt = g.Count(t => t.DeliveryOnTime);

                var dotValues = g.Where(t => t.DOTPercentage.HasValue).Select(t => t.DOTPercentage.Value);
                var pct = dotValues.Any()
                    ? Math.Round(dotValues.Average(), 1)
                    : 0m;
                return new
                {
                    employee = employeeMap[g.Key],
                    onTime = onTimeCnt,
                    late = total - onTimeCnt,
                    total,
                    dotPct = pct
                };
            })
            .OrderByDescending(x => x.total)
            .ToList<object>();
        var projectFallback = await _localizationService.GetResourceAsync("Dashboard.ProjectFallback");
        var noProjectText = await _localizationService.GetResourceAsync("Dashboard.NoProject");
        var projectDot = periodTasks
            .GroupBy(t => t.ProjectId)
            .Select(g =>
            {
                var total = g.Count();
                var onTimeCnt = g.Count(t => t.DeliveryOnTime);

                var dotValues = g.Where(t => t.DOTPercentage.HasValue).Select(t => t.DOTPercentage.Value);
                var pct = dotValues.Any()
                    ? Math.Round(dotValues.Average(), 1)
                    : 0m;
                return new
                {
                    project = projectMap.GetValueOrDefault(
                        g.Key,
                        g.Key > 0
                            ? string.Format(projectFallback, g.Key)
                            : noProjectText),

                    onTime = onTimeCnt,
                    late = total - onTimeCnt,
                    total,
                    dotPct = pct
                };
            })
            .OrderByDescending(x => x.total)
            .ToList<object>();

        return Json(new
        {
            onTime,
            late,
            dotPct,
            chartData,
            employeeDot,
            projectDot
        });
    }
    [HttpPost]
    public virtual async Task<IActionResult> GetWorkQualityDetail(
       string period = "last3months",
       int? projectId = null,
       int? employeeId = null,
       DateTime? customFrom = null,
       DateTime? customTo = null,
       int? monthFrom = null,
       int? yearFrom = null,
       int? monthTo = null,
       int? yearTo = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardOperational))
            return AccessDeniedView();

        var fyStartMonth = await GetFyStartMonthAsync();
        var (dateFrom, dateTo, _, _, _, _) = ResolveOperationalPeriod(period, customFrom, customTo, monthFrom, yearFrom, monthTo, yearTo, fyStartMonth);

        var pIds = projectId.HasValue ? new List<int> { projectId.Value } : null;
        var eIds = employeeId.HasValue ? new List<int> { employeeId.Value } : null;
        var tsTask = _timeSheetsService.GetAllTimeSheetAsync(
            employeeIds: eIds,
            projectIds: pIds,
            from: dateFrom,
            to: dateTo,
            pageSize: int.MaxValue
        );
        var allTasksTask = _projectTaskService.GetAllProjectTasksAsync(projectIds: pIds, pageSize: int.MaxValue);
        var employeesTask = _employeeService.GetAllEmployeesAsync(pageSize: int.MaxValue, showInActive: false);
        var projectsTask = _projectsService.GetAllProjectsAsync(pageSize: int.MaxValue);

        await Task.WhenAll(allTasksTask, employeesTask, projectsTask, tsTask);

        var employeeMap = employeesTask.Result.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());
        var projectMap = projectsTask.Result.ToDictionary(p => p.Id, p => p.ProjectTitle ?? "");

        var ts = tsTask.Result.ToList();
        var rawTaskIdsWQ    = ts.Where(t => t.TaskId > 0).Select(t => t.TaskId).ToHashSet();
        var allTaskMapWQ    = allTasksTask.Result.ToDictionary(t => t.Id);
        var rawToResolvedWQ = rawTaskIdsWQ.ToDictionary(
            id => id,
            id => allTaskMapWQ.TryGetValue(id, out var rt) && rt.Tasktypeid == 3 && rt.ParentTaskId > 0 && allTaskMapWQ.ContainsKey(rt.ParentTaskId)
                  ? rt.ParentTaskId : id);
        var resolvedTaskIdsWQ = rawToResolvedWQ.Values.ToHashSet();
        var empResolvedIdsWQ  = employeeId.HasValue
            ? ts.Where(x => x.TaskId > 0 && x.EmployeeId == employeeId.Value)
               .Select(x => rawToResolvedWQ.GetValueOrDefault(x.TaskId, x.TaskId))
               .ToHashSet()
            : null;

        var wqBugCountByParent = allTasksTask.Result
            .Where(t => t.Tasktypeid == (int)TaskTypeEnum.Bug && t.ParentTaskId > 0)
            .GroupBy(t => t.ParentTaskId)
            .ToDictionary(g => g.Key, g => g.Count());
        var qualTasks = allTasksTask.Result
            .Where(t => t.Tasktypeid != (int)TaskTypeEnum.UserStory &&
                        t.WorkQuality.HasValue &&
                        resolvedTaskIdsWQ.Contains(t.Id) &&
                        (empResolvedIdsWQ == null || empResolvedIdsWQ.Contains(t.Id)))
            .ToList();
        var avgScore = qualTasks.Count > 0
            ? Math.Round(qualTasks.Average(t => t.WorkQuality.Value), 1)
            : (decimal?)null;

        var totalBugs = qualTasks.Sum(t => wqBugCountByParent.GetValueOrDefault(t.Id, 0));
        var tasksEvaluated = qualTasks.Count;
        var intervals = GetMonthlyDateIntervals(dateFrom, dateTo);
        var chartData = intervals.Select(iv =>
        {
            var mTaskIds = ts
                .Where(t => t.SpentDate.Date >= iv.from.Date && t.SpentDate.Date <= iv.to.Date)
                .Select(t => t.TaskId)
                .ToHashSet();

            var mTasks = qualTasks
                .Where(t => mTaskIds.Contains(t.Id))
                .ToList();

            var mAvg = mTasks.Count > 0
                ? Math.Round(mTasks.Average(t => t.WorkQuality.Value), 1)
                : (decimal?)null;

            return new { label = iv.label, avgQuality = mAvg };
        }).ToList<object>();

        var belowAvgTasks = avgScore.HasValue
            ? qualTasks
                .Where(t => t.WorkQuality.Value < avgScore.Value)
                .OrderBy(t => t.WorkQuality.Value)
                .Select(t => new
                {
                    id = t.Id,
                    title = t.TaskTitle ?? "",
                    projectName = projectMap.GetValueOrDefault(t.ProjectId, ""),
                    assignee = t.DeveloperId > 0 ? employeeMap.GetValueOrDefault(t.DeveloperId, "") : "",
                    qualityScore = Math.Round(t.WorkQuality.Value, 1),
                    bugCount = wqBugCountByParent.GetValueOrDefault(t.Id, 0)
                }).ToList<object>()
            : new List<object>();

        var employeeQuality = qualTasks
            .GroupBy(t => t.DeveloperId)
            .Where(g => g.Key > 0 && employeeMap.ContainsKey(g.Key))
            .Select(g =>
            {
                var avg = Math.Round(g.Average(t => t.WorkQuality.Value), 1);
                var bugs = g.Sum(t => wqBugCountByParent.GetValueOrDefault(t.Id, 0));

                return new
                {
                    employee = employeeMap[g.Key],
                    avgScore = avg,
                    bugCount = bugs,
                    taskCount = g.Count()
                };
            })
            .OrderByDescending(x => x.taskCount)
            .ToList<object>();

        var unknownProjectText = await _localizationService.GetResourceAsync("Dashboard.UnknownProject");
        var projectQuality = qualTasks
            .GroupBy(t => t.ProjectId)
            .Select(g =>
            {
                var avg = Math.Round(g.Average(t => t.WorkQuality.Value), 1);
                var bugs = g.Sum(t => wqBugCountByParent.GetValueOrDefault(t.Id, 0));
                return new
                {
                    project = projectMap.GetValueOrDefault(g.Key, unknownProjectText),
                    avgScore = avg,
                    bugCount = bugs,
                    taskCount = g.Count()
                };
            })
            .OrderByDescending(x => x.taskCount)
            .ToList<object>();

        return Json(new
        {
            avgScore,
            totalBugs,
            tasksEvaluated,
            chartData,
            belowAvgTasks,
            employeeQuality,
            projectQuality
        });
    }
    private static List<(int month, int year, string label)> BuildMonthlyIntervals(DateTime from, DateTime to)
    {
        var result = new List<(int, int, string)>();
        var current = new DateTime(from.Year, from.Month, 1);
        var end = new DateTime(to.Year, to.Month, 1);
        var fmt = from.Year == to.Year ? "MMM" : "MMM yy";
        while (current <= end)
        {
            result.Add((current.Month, current.Year, current.ToString(fmt, CultureInfo.InvariantCulture)));
            current = current.AddMonths(1);
        }
        return result;
    }

    private async Task<Dictionary<int, string>> GetLocalizedStageNamesAsync()
    {
        var prefix = "Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.CRM.Stage.";
        var result = new Dictionary<int, string>();
        for (var i = 1; i <= 9; i++)
            result[i] = await _localizationService.GetResourceAsync(prefix + i);
        return result;
    }

    private async Task<Dictionary<int, string>> GetLocalizedFollowUpStatusNamesAsync()
    {
        var prefix = "Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.CRM.LinkedInStatus.";
        var result = new Dictionary<int, string>();
        for (var i = 0; i <= 8; i++)
            result[i] = await _localizationService.GetResourceAsync(prefix + i);
        return result;
    }

    private static readonly Dictionary<int, string> FollowUpStatusColors = new()
    {
        { 0, "#9ca3af" },
        { 1, "#6b7280" },
        { 2, "#3b82f6" },
        { 3, "#06b6d4" },
        { 4, "#10b981" },
        { 5, "#f59e0b" },
        { 6, "#16a34a" },
        { 7, "#7c3aed" },
        { 8, "#ef4444" }
    };

    [HttpPost]
    public virtual async Task<IActionResult> GetCRMInsightsData(
        string period = "lastmonth",
        DateTime? dateFrom = null, DateTime? dateTo = null,
        int? monthFrom = null, int? yearFrom = null,
        int? monthTo = null, int? yearTo = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var fyStartMonth = await GetFyStartMonthAsync();
        var (dfrom, dto, prevFrom, prevTo, periodLabel, prevPeriodLabel) =
            ResolveOperationalPeriod(period, dateFrom, dateTo, monthFrom, yearFrom, monthTo, yearTo, fyStartMonth);
        dto     = dto.Date.AddDays(1).AddTicks(-1);
        prevTo  = prevTo.Date.AddDays(1).AddTicks(-1);
        var dealsTask    = _dealsService.GetAllDealsAsync("", 0, 0, null, pageSize: int.MaxValue);
        var leadsTask    = _leadService.GetAllLeadAsync("", "", null, "", "", 0, 0, null, 0, pageSize: int.MaxValue);
        var followupsTask = _linkedInFollowupsService.GetAllLinkedInFollowupsAsync("", "", "", "", "", pageSize: int.MaxValue);
        var inquiriesTask = _inquiryService.GetAllInquiryAsync(pageSize: int.MaxValue);
        var sourcesTask  = _leadSourceService.GetAllLeadSourceAsync("", pageSize: int.MaxValue);

        await Task.WhenAll(dealsTask, leadsTask, followupsTask, inquiriesTask, sourcesTask);

        var allDeals     = dealsTask.Result.ToList();
        var allLeads     = leadsTask.Result.ToList();
        var allFollowups = followupsTask.Result.ToList();
        var allInquiries = inquiriesTask.Result.ToList();
        var sourceMap    = sourcesTask.Result.ToDictionary(s => s.Id, s => s.Name);

        var stageNamesMap       = await GetLocalizedStageNamesAsync();
        var followUpStatusNames = await GetLocalizedFollowUpStatusNamesAsync();
        var openDeals = allDeals.Where(d => d.StageId >= 1 && d.StageId <= 6).ToList();
        var totalPipelineValue = (decimal)openDeals.Sum(d => d.Amount);
        var dealsWon = allDeals.Where(d =>
            d.StageId == 7 && d.ClosingDate.HasValue &&
            d.ClosingDate.Value >= dfrom && d.ClosingDate.Value <= dto).ToList();
        var prevDealsWon = allDeals.Where(d =>
            d.StageId == 7 && d.ClosingDate.HasValue &&
            d.ClosingDate.Value >= prevFrom && d.ClosingDate.Value <= prevTo).ToList();
        var newLeads     = allLeads.Where(l => l.CreatedOnUtc >= dfrom && l.CreatedOnUtc <= dto).ToList();
        var prevLeads    = allLeads.Where(l => l.CreatedOnUtc >= prevFrom && l.CreatedOnUtc <= prevTo).ToList();
        var newInqs      = allInquiries.Where(i => i.CreatedOnUtc >= dfrom && i.CreatedOnUtc <= dto).ToList();
        var prevInqs     = allInquiries.Where(i => i.CreatedOnUtc >= prevFrom && i.CreatedOnUtc <= prevTo).ToList();
        var today = DateTime.UtcNow.Date;
        var overdueCount = allFollowups.Count(f =>
            f.NextFollowUpDate.HasValue &&
            f.NextFollowUpDate.Value.Date < today &&
            f.RemainingFollowUps > 0 &&
            f.StatusId != (int)FollowUpStatusEnum.Closed &&
            f.StatusId != (int)FollowUpStatusEnum.Converted &&
            f.StatusId != (int)FollowUpStatusEnum.NotInterested &&
            f.StatusId != (int)FollowUpStatusEnum.BlockedOrRemoved);
        var intervals = BuildMonthlyIntervals(dfrom, dto);
        var dealsWonPoints = intervals.Select(i =>
            (decimal)allDeals
                .Where(d => d.StageId == 7 && d.ClosingDate.HasValue &&
                            d.ClosingDate.Value.Year == i.year && d.ClosingDate.Value.Month == i.month)
                .Sum(d => d.Amount)).ToList();
        var newLeadsPoints = intervals.Select(i =>
            allLeads.Count(l => l.CreatedOnUtc.Year == i.year && l.CreatedOnUtc.Month == i.month)).ToList();
        var sparklineLabels = intervals.Select(i => i.label).ToList();
        var dealsByStage = Enumerable.Range(1, 6).Select(stageId =>
        {
            var stageDeals = openDeals.Where(d => d.StageId == stageId).ToList();
            return new
            {
                stageName = stageNamesMap.GetValueOrDefault(stageId, stageId.ToString()),
                count = stageDeals.Count,
                value = (decimal)stageDeals.Sum(d => d.Amount)
            };
        }).ToList();
        var leadsBySource = allLeads
            .GroupBy(l => l.LeadSourceId)
            .Select(g => new
            {
                sourceName = sourceMap.GetValueOrDefault(g.Key, "Unknown"),
                count = g.Count()
            })
            .Where(s => s.count > 0)
            .OrderByDescending(s => s.count)
            .ToList();
        var trendIntervals   = BuildMonthlyIntervals(dfrom, dto);
        var monthlyLeadTrend = trendIntervals.Select(i =>
            allLeads.Count(l => l.CreatedOnUtc.Year == i.year && l.CreatedOnUtc.Month == i.month)).ToList();
        var monthlyDealTrend = trendIntervals.Select(i =>
            allDeals.Count(d => d.StageId == 7 && d.ClosingDate.HasValue &&
                                d.ClosingDate.Value.Year == i.year && d.ClosingDate.Value.Month == i.month)).ToList();
        var monthlyTrendLabels = trendIntervals.Select(i => i.label).ToList();
        var dealsClosedWon      = allDeals.Count(d => d.StageId == 7);
        var dealsClosedLost     = allDeals.Count(d => d.StageId == 8);
        var dealsClosedLostComp = allDeals.Count(d => d.StageId == 9);
        var linkedInStatusCounts = allFollowups
            .GroupBy(f => f.StatusId)
            .Select(g => new
            {
                statusName = followUpStatusNames.GetValueOrDefault(g.Key, g.Key.ToString()),
                count = g.Count(),
                color = FollowUpStatusColors.GetValueOrDefault(g.Key, "#9ca3af")
            })
            .Where(s => s.count > 0)
            .OrderBy(s => s.statusName)
            .ToList();

        var linkedInTotal     = allFollowups.Count;
        var linkedInConverted = allFollowups.Count(f => f.StatusId == (int)FollowUpStatusEnum.Converted);
        var linkedInReplied   = allFollowups.Count(f =>
            f.StatusId == (int)FollowUpStatusEnum.Replied ||
            f.StatusId == (int)FollowUpStatusEnum.InConversation);
        var linkedInConvRate  = linkedInTotal > 0
            ? Math.Round((decimal)linkedInConverted / linkedInTotal * 100, 1)
            : 0m;

        var zohoCampaigns       = new List<object>();
        var zohoAvailable       = false;
        string zohoLastSynced   = null;
        double zohoAvgOpenRate      = 0;
        double zohoAvgDeliveredRate = 0;
        double zohoAvgClickRate     = 0;
        double zohoAvgBounceRate    = 0;
        int    zohoCampaignCount    = 0;
        try
        {
            var zohoSettings = await _settingService.LoadSettingAsync<ZohoCampaignSettings>();
            if (zohoSettings.IsEnabled)
            {
                var stats = await _zohoCampaignService.GetStoredStatsAsync(limit: 10);
                zohoAvailable  = stats.Any();
                zohoLastSynced = zohoSettings.LastSyncedUtc?.ToString("MMM d, yyyy h:mm tt");
                var campaignList = new List<object>();
                foreach (var s in stats)
                {
                    var locs = await _zohoCampaignService.GetLocationStatsByKeyAsync(s.CampaignKey);
                    campaignList.Add(new
                    {
                        campaignKey      = s.CampaignKey,
                        name             = s.CampaignName,
                        emailSubject     = s.EmailSubject ?? "",
                        emailFrom        = s.EmailFrom ?? "",
                        sentOn           = s.SentTime.HasValue ? s.SentTime.Value.ToString("MMM d") : "",
                        sentOnFull       = s.SentTime.HasValue ? s.SentTime.Value.ToString("dd MMM yyyy, h:mm tt") : "",
                        sentTimeIso      = s.SentTime.HasValue ? s.SentTime.Value.ToString("yyyy-MM-dd") : "",
                        createdOn        = s.CreatedTime.HasValue ? s.CreatedTime.Value.ToString("dd MMM yyyy, h:mm tt") : "",
                        emailsSent       = s.EmailsSentCount,
                        deliveredCount   = s.DeliveredCount,
                        deliveredPercent = s.DeliveredPercent,
                        opensCount       = s.OpensCount,
                        openPercent      = s.OpenPercent,
                        unopenedCount    = s.UnopenedCount,
                        clickedCount     = s.UniqueClicksCount,
                        clickedPercent   = s.UniqueClickedPercent,
                        clicksPerOpen    = s.ClicksPerOpenRate,
                        bouncesCount     = s.BouncesCount,
                        hardBounce       = s.HardBounceCount,
                        softBounce       = s.SoftBounceCount,
                        bouncePercent    = s.BouncePercent,
                        unsubCount       = s.UnsubCount,
                        unsubPercent     = s.UnsubscribePercent,
                        spamsCount       = s.SpamsCount,
                        complaintsCount  = s.ComplaintsCount,
                        forwardsCount    = s.ForwardsCount,
                        autoreplyCount   = s.AutoreplyCount,
                        countries        = locs.Select(l => new { country = l.Country, opens = l.OpensCount }).ToList()
                    });
                }
                zohoCampaigns = campaignList;

                if (stats.Any())
                {
                    zohoAvgOpenRate      = Math.Round((double)stats.Average(s => s.OpenPercent), 1);
                    zohoAvgDeliveredRate = Math.Round((double)stats.Average(s => s.DeliveredPercent), 1);
                    zohoAvgClickRate     = Math.Round((double)stats.Average(s => s.UniqueClickedPercent), 1);
                    zohoAvgBounceRate    = Math.Round((double)stats.Average(s => s.BouncePercent), 1);
                    zohoCampaignCount    = stats.Count;
                }
            }
        }
        catch { }

        return Json(new
        {
            // KPIs
            totalPipelineValue,
            pipelineDealsCount = openDeals.Count,
            dealsWonCount    = dealsWon.Count,
            dealsWonValue    = (decimal)dealsWon.Sum(d => d.Amount),
            prevDealsWonValue = (decimal)prevDealsWon.Sum(d => d.Amount),
            newLeadsCount    = newLeads.Count,
            newInquiriesCount = newInqs.Count,
            prevNewLeadsCount = prevLeads.Count,
            prevNewInquiriesCount = prevInqs.Count,
            overdueFollowupsCount = overdueCount,
            dealsWonPoints,
            newLeadsPoints,
            sparklineLabels,
            dealsByStage,
            leadsBySource,
            monthlyLeadTrend,
            monthlyDealTrend,
            monthlyTrendLabels,
            dealsClosedWon,
            dealsClosedLost,
            dealsClosedLostToComp = dealsClosedLostComp,
            linkedInStatusCounts,
            linkedInTotalCount    = linkedInTotal,
            linkedInConvertedCount = linkedInConverted,
            linkedInRepliedCount  = linkedInReplied,
            linkedInConversionRate = linkedInConvRate,
            zohoAvailable,
            zohoLastSynced,
            zohoCampaigns,
            zohoAvgOpenRate,
            zohoAvgDeliveredRate,
            zohoAvgClickRate,
            zohoAvgBounceRate,
            zohoCampaignCount,
            periodLabel,
            prevPeriodLabel
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetCRMPipelineDetail()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var allDeals = (await _dealsService.GetAllDealsAsync("", 0, 0, null, pageSize: int.MaxValue)).ToList();
        var openDeals = allDeals.Where(d => d.StageId >= 1 && d.StageId <= 6).OrderBy(d => d.StageId).ToList();

        var allCompanies = (await _companyService.GetAllCompanyAsync("", "", pageSize: int.MaxValue))
            .ToDictionary(c => c.Id, c => c.CompanyName);
        var stageNamesMap = await GetLocalizedStageNamesAsync();

        var rows = openDeals.Select(d => new
        {
            dealName    = d.DealName,
            company     = allCompanies.GetValueOrDefault(d.CompanyId, "—"),
            stage       = stageNamesMap.GetValueOrDefault(d.StageId, "—"),
            amount      = d.Amount,
            probability = d.Probability,
            closingDate = d.ClosingDate.HasValue ? d.ClosingDate.Value.ToString("dd MMM yyyy") : "—"
        });

        return Json(rows);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetCRMDealsWonDetail(
        string period = "lastmonth",
        DateTime? dateFrom = null, DateTime? dateTo = null,
        int? monthFrom = null, int? yearFrom = null,
        int? monthTo = null, int? yearTo = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var fyStartMonth = await GetFyStartMonthAsync();
        var (dfrom, dto, _, _, _, _) =
            ResolveOperationalPeriod(period, dateFrom, dateTo, monthFrom, yearFrom, monthTo, yearTo, fyStartMonth);

        var allDeals = (await _dealsService.GetAllDealsAsync("", 0, 7, null, pageSize: int.MaxValue)).ToList();
        var wonDeals = allDeals.Where(d =>
            d.ClosingDate.HasValue && d.ClosingDate.Value >= dfrom && d.ClosingDate.Value <= dto)
            .OrderByDescending(d => d.ClosingDate).ToList();

        var allCompanies = (await _companyService.GetAllCompanyAsync("", "", pageSize: int.MaxValue))
            .ToDictionary(c => c.Id, c => c.CompanyName);

        var rows = wonDeals.Select(d => new
        {
            dealName    = d.DealName,
            company     = allCompanies.GetValueOrDefault(d.CompanyId, "—"),
            amount      = d.Amount,
            probability = d.Probability,
            closingDate = d.ClosingDate.HasValue ? d.ClosingDate.Value.ToString("dd MMM yyyy") : "—"
        });

        return Json(rows);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetCRMLeadsInquiriesDetail(
        string period = "lastmonth",
        DateTime? dateFrom = null, DateTime? dateTo = null,
        int? monthFrom = null, int? yearFrom = null,
        int? monthTo = null, int? yearTo = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var fyStartMonth = await GetFyStartMonthAsync();
        var (dfrom, dto, _, _, _, _) =
            ResolveOperationalPeriod(period, dateFrom, dateTo, monthFrom, yearFrom, monthTo, yearTo, fyStartMonth);

        var leadsTask  = _leadService.GetAllLeadAsync("", "", null, "", "", 0, 0, null, 0, pageSize: int.MaxValue);
        var inqTask    = _inquiryService.GetAllInquiryAsync(pageSize: int.MaxValue);
        var sourcesTask = _leadSourceService.GetAllLeadSourceAsync("", pageSize: int.MaxValue);

        await Task.WhenAll(leadsTask, inqTask, sourcesTask);

        var sourceMap = sourcesTask.Result.ToDictionary(s => s.Id, s => s.Name);

        var leads = leadsTask.Result
            .Where(l => l.CreatedOnUtc >= dfrom && l.CreatedOnUtc <= dto)
            .OrderByDescending(l => l.CreatedOnUtc)
            .Select(l => new
            {
                name        = $"{l.FirstName} {l.LastName}".Trim(),
                company     = l.CompanyName,
                source      = sourceMap.GetValueOrDefault(l.LeadSourceId, "—"),
                createdOn   = l.CreatedOnUtc.ToString("dd MMM yyyy")
            }).ToList();

        var inquiries = inqTask.Result
            .Where(i => i.CreatedOnUtc >= dfrom && i.CreatedOnUtc <= dto)
            .OrderByDescending(i => i.CreatedOnUtc)
            .Select(i => new
            {
                name        = $"{i.FirstName} {i.LastName}".Trim(),
                company     = i.Company,
                projectType = i.ProjectType,
                budget      = i.Budget,
                createdOn   = i.CreatedOnUtc.ToString("dd MMM yyyy")
            }).ToList();

        return Json(new { leads, inquiries });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetCRMOverdueFollowupsDetail()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var today = DateTime.UtcNow.Date;
        var all = (await _linkedInFollowupsService.GetAllLinkedInFollowupsAsync("", "", "", "", "", pageSize: int.MaxValue)).ToList();
        var followUpStatusNames = await GetLocalizedFollowUpStatusNamesAsync();

        var overdue = all
            .Where(f =>
                f.NextFollowUpDate.HasValue &&
                f.NextFollowUpDate.Value.Date < today &&
                f.RemainingFollowUps > 0 &&
                f.StatusId != (int)FollowUpStatusEnum.Closed &&
                f.StatusId != (int)FollowUpStatusEnum.Converted &&
                f.StatusId != (int)FollowUpStatusEnum.NotInterested &&
                f.StatusId != (int)FollowUpStatusEnum.BlockedOrRemoved)
            .OrderBy(f => f.NextFollowUpDate)
            .Select(f => new
            {
                name              = $"{f.FirstName} {f.LastName}".Trim(),
                linkedinUrl       = f.LinkedinUrl,
                nextFollowUpDate  = f.NextFollowUpDate.HasValue ? f.NextFollowUpDate.Value.ToString("dd MMM yyyy") : "—",
                remainingFollowUps = f.RemainingFollowUps,
                status            = followUpStatusNames.GetValueOrDefault(f.StatusId, "—")
            }).ToList();

        return Json(overdue);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetCRMLinkedInContactsDetail(string filter = "all")
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var all = (await _linkedInFollowupsService.GetAllLinkedInFollowupsAsync("", "", "", "", "", pageSize: int.MaxValue)).ToList();

        IEnumerable<LinkedInFollowups> filtered = filter switch
        {
            "converted" => all.Where(f => f.StatusId == (int)FollowUpStatusEnum.Converted),
            "replied"   => all.Where(f => f.StatusId == (int)FollowUpStatusEnum.Replied
                                       || f.StatusId == (int)FollowUpStatusEnum.InConversation),
            _           => all
        };

        var followUpStatusNamesLi = await GetLocalizedFollowUpStatusNamesAsync();

        var rows = filtered
            .OrderByDescending(f => f.UpdatedOnUtc)
            .Select(f => new
            {
                fullName          = $"{f.FirstName} {f.LastName}".Trim(),
                linkedinUrl       = f.LinkedinUrl,
                email             = f.Email,
                status            = followUpStatusNamesLi.GetValueOrDefault(f.StatusId, "—"),
                nextFollowUpDate  = f.NextFollowUpDate.HasValue ? f.NextFollowUpDate.Value.ToString("dd MMM yyyy") : "—",
                remainingFollowUps = f.RemainingFollowUps,
                lastMessageDate   = f.LastMessageDate.HasValue ? f.LastMessageDate.Value.ToString("dd MMM yyyy") : "—"
            }).ToList();

        return Json(rows);
    }

    private static void BuildExpenseRows(
        IEnumerable<Domain.AccountTransaction> transactions,
        IEnumerable<Domain.AccountGroup> accountGroups,
        List<object> rows,
        string salariesLabel,
        string otherLabel)
    {
        var groupMap = (accountGroups ?? Enumerable.Empty<Domain.AccountGroup>()).ToDictionary(g => g.Id, g => g.Name);
        var txnList = transactions.ToList();

        var salaryTotal = txnList.Where(t => t.EmployeeId > 0).Sum(t => t.Amount);
        if (salaryTotal > 0)
            rows.Add(new { category = salariesLabel, amount = salaryTotal, isSalary = true });

        var grouped = txnList
            .Where(t => t.EmployeeId == 0)
            .GroupBy(t => t.AccountGroupId > 0 && groupMap.ContainsKey(t.AccountGroupId) ? groupMap[t.AccountGroupId] : otherLabel);

        foreach (var g in grouped)
            rows.Add(new { category = g.Key, amount = g.Sum(t => t.Amount), isSalary = false });
    }

    #endregion

    #region Zoho Campaign Timeline

    [HttpGet]
    public virtual async Task<IActionResult> GetCampaignTimeline(string campaignKey, int days = 30, string fromDate = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        if (string.IsNullOrEmpty(campaignKey))
            return Json(new { success = false });

        DateTime? from = DateTime.TryParse(fromDate, out var fd) ? fd.Date : (DateTime?)null;
        var rows = await _zohoCampaignService.GetDailyStatsByKeyAsync(campaignKey, days, from);

        return Json(new
        {
            success = true,
            dates   = rows.Select(r => r.StatDate.ToString("dd MMM")).ToList(),
            opens   = rows.Select(r => r.OpensCount).ToList(),
            clicks  = rows.Select(r => r.ClicksCount).ToList(),
            bounces = rows.Select(r => r.BouncesCount).ToList()
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> SyncCampaignTimeline(string campaignKey)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        if (string.IsNullOrEmpty(campaignKey))
            return Json(new { success = false });

        await _zohoCampaignService.SyncRecipientDataAsync(campaignKey);
        return Json(new { success = true });
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetCombinedTimeline(string fromDate = null, string toDate = null, string keys = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var from = DateTime.TryParse(fromDate, out var fd) ? fd.Date : DateTime.UtcNow.Date.AddDays(-30);
        var to   = DateTime.TryParse(toDate,   out var td) ? td.Date : DateTime.UtcNow.Date;

        var campaignKeys = string.IsNullOrEmpty(keys)
            ? null
            : keys.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        var rows  = await _zohoCampaignService.GetCombinedDailyStatsAsync(from, to, campaignKeys);
        var stats = await _zohoCampaignService.GetStoredStatsAsync(limit: 200);
        var totalSent      = stats.Sum(s => s.EmailsSentCount);
        var totalDelivered = stats.Sum(s => s.DeliveredCount);

        return Json(new
        {
            success        = true,
            totalSent      = totalSent,
            totalDelivered = totalDelivered,
            dates   = rows.Select(r => r.StatDate.ToString("dd MMM")).ToList(),
            opens   = rows.Select(r => r.OpensCount).ToList(),
            clicks  = rows.Select(r => r.ClicksCount).ToList(),
            bounces = rows.Select(r => r.BouncesCount).ToList()
        });
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetCampaignPeriodStats(string fromDate = null, string toDate = null)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var from = DateTime.TryParse(fromDate, out var fd) ? fd.Date : DateTime.UtcNow.Date.AddDays(-30);
        var to   = DateTime.TryParse(toDate,   out var td) ? td.Date : DateTime.UtcNow.Date;

        var periodStats = await _zohoCampaignService.GetPeriodStatsByCampaignAsync(from, to);
        var allStats    = await _zohoCampaignService.GetStoredStatsAsync(limit: 200);

        var campaigns = allStats
            .Select(s =>
            {
                periodStats.TryGetValue(s.CampaignKey, out var ps);
                return new
                {
                    campaignKey = s.CampaignKey,
                    name        = s.CampaignName,
                    sentOn      = s.SentTime.HasValue ? s.SentTime.Value.ToString("MMM d") : "",
                    sentTimeIso = s.SentTime.HasValue ? s.SentTime.Value.ToString("yyyy-MM-dd") : "",
                    emailsSent  = s.EmailsSentCount,
                    opens       = ps.Opens,
                    clicks      = ps.Clicks,
                    bounces     = ps.Bounces
                };
            })
            .OrderByDescending(x => x.opens)
            .ToList();

        return Json(new { success = true, campaigns });
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetLeadIntelligenceData(int page = 1, int pageSize = 20, string sortBy = "interestScore", string sortDir = "desc")
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboardCRM))
            return AccessDeniedView();

        var leadsTask      = _leadService.GetAllLeadAsync("", "", null, "", "", 0, 0, null, 0, pageSize: int.MaxValue);
        var emailStatsTask = _zohoCampaignService.GetLeadEmailStatsAsync();
        await Task.WhenAll(leadsTask, emailStatsTask);

        var allLeads   = leadsTask.Result.ToList();
        var emailStats = emailStatsTask.Result;

        var joined = allLeads.Select(l =>
        {
            emailStats.TryGetValue(l.Id, out var es);
            return new
            {
                lead              = l,
                totalOpens        = es.TotalOpens,
                totalClicks       = es.TotalClicks,
                totalBounces      = es.TotalBounces,
                totalUnsubscribes = es.TotalUnsubscribes,
                interestScore     = l.InterestScore ?? 0
            };
        });

        var asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        joined = sortBy switch
        {
            "opens"        => asc ? joined.OrderBy(x => x.totalOpens).ThenBy(x => x.interestScore)
                                  : joined.OrderByDescending(x => x.totalOpens).ThenByDescending(x => x.interestScore),
            "clicks"       => asc ? joined.OrderBy(x => x.totalClicks).ThenBy(x => x.interestScore)
                                  : joined.OrderByDescending(x => x.totalClicks).ThenByDescending(x => x.interestScore),
            "bounces"      => asc ? joined.OrderBy(x => x.totalBounces).ThenBy(x => x.interestScore)
                                  : joined.OrderByDescending(x => x.totalBounces).ThenByDescending(x => x.interestScore),
            "unsubscribes" => asc ? joined.OrderBy(x => x.totalUnsubscribes).ThenBy(x => x.interestScore)
                                  : joined.OrderByDescending(x => x.totalUnsubscribes).ThenByDescending(x => x.interestScore),
            _              => asc ? joined.OrderBy(x => x.interestScore).ThenBy(x => x.totalOpens)
                                  : joined.OrderByDescending(x => x.interestScore).ThenByDescending(x => x.totalOpens)
        };

        var totalCount = joined.Count();
        var skip       = (Math.Max(1, page) - 1) * pageSize;
        var pageData   = joined.Skip(skip).Take(pageSize).Select(x => new
        {
            id                = x.lead.Id,
            name              = (x.lead.FirstName + " " + x.lead.LastName).Trim(),
            company           = x.lead.CompanyName ?? "",
            interestScore     = x.interestScore,
            totalOpens        = x.totalOpens,
            totalClicks       = x.totalClicks,
            totalBounces      = x.totalBounces,
            totalUnsubscribes = x.totalUnsubscribes
        }).ToList();

        return Json(new { leads = pageData, totalCount, page, pageSize });
    }

    #endregion
}
