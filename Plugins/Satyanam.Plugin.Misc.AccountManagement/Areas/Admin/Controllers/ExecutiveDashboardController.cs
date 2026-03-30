using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.Projects;
using App.Services.Configuration;
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
using Satyanam.Nop.Core.Services;
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
        IMemoryCache memoryCache)
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
        return View();
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetFinancialKPIData(int granularityId, DateTime? dateFrom, DateTime? dateTo, int? monthId, int? yearId, int? yearFrom, int? yearTo, int? monthFrom, int? monthTo)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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

                // KPI aggregates = sum of all months in range
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
            return AccessDeniedView();

        var fyStartMonth = granularityId == 4 ? await GetFyStartMonthAsync() : 4;
        decimal revenue;
        IEnumerable<object> topInvoices;

        var intervals = ResolveModalIntervals(granularityId, monthFrom, yearFrom, monthTo, yearTo, yearFrom, yearTo, fyStartMonth);
        if (intervals != null)
        {
            var revTasks = intervals.Select(i => _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, monthId: i.month, yearId: i.year)).ToList();
            var invTasks = intervals.Select(i => _accountManagementService.GetAllInvoicesAsync(monthId: i.month, yearId: i.year)).ToList();
            await Task.WhenAll(revTasks.Cast<Task>().Concat(invTasks.Cast<Task>()));
            revenue = revTasks.Sum(t => t.Result.TotalIncome);
            topInvoices = invTasks.SelectMany(t => t.Result ?? Enumerable.Empty<Domain.Invoice>())
                .OrderByDescending(x => x.TotalPaymentAmount).Take(10)
                .Select(x => (object)new { invoiceNumber = x.InvoiceNumber, amount = x.TotalPaymentAmount, date = x.InvoiceDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture), statusId = x.StatusId });
        }
        else
        {
            var summaryTask = _accountManagementService.GetAccountTransactionSummaryAsync(transactionTypeId: 10, dateFrom: dateFrom, dateTo: dateTo);
            var invoicesTask = _accountManagementService.GetAllInvoicesAsync(createdFromUTC: dateFrom, createdToUTC: dateTo);
            await Task.WhenAll(summaryTask, invoicesTask);
            revenue = summaryTask.Result.TotalIncome;
            topInvoices = (invoicesTask.Result ?? Enumerable.Empty<Domain.Invoice>()).OrderByDescending(x => x.TotalPaymentAmount).Take(10)
                .Select(x => (object)new { invoiceNumber = x.InvoiceNumber, amount = x.TotalPaymentAmount, date = x.InvoiceDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture), statusId = x.StatusId });
        }

        return Json(new { revenue, topInvoices });
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetGrossProfitDetail(int granularityId, DateTime? dateFrom, DateTime? dateTo, int? monthId, int? yearId, int? yearFrom, int? yearTo, int? monthFrom, int? monthTo)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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
        var invoiceRows = allInvoices
            .OrderByDescending(i => i.TotalPaymentAmount)
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
                    amount = i.TotalPaymentAmount,
                    statusLabel,
                    daysOverdue = isOverdue ? (today - i.DueDate.Date).Days : 0
                };
            }).ToList();

        return Json(new
        {
            paid    = new { count = paid.Count,    amount = paid.Sum(i => i.TotalPaymentAmount) },
            overdue = new { count = overdue.Count, amount = overdue.Sum(i => i.TotalPaymentAmount) },
            pending = new { count = pending.Count, amount = pending.Sum(i => i.TotalPaymentAmount) },
            draft   = new { count = draft.Count,   amount = draft.Sum(i => i.TotalPaymentAmount) },
            invoices = invoiceRows
        });
    }

    private const string StatusCacheKey = "exec_dashboard_workflow_statuses";

    private async Task<(HashSet<int> completedIds, HashSet<int> notStartedIds)> GetCachedStatusSetsAsync()
    {
        if (_memoryCache.TryGetValue(StatusCacheKey, out (HashSet<int> c, HashSet<int> n) cached))
            return cached;

        var statuses = await _workflowStatusService.GetAllWorkflowStatusAsync(pageSize: int.MaxValue);
        var completedIds = statuses
            .Where(s => (s.StatusName ?? "").Trim().ToLowerInvariant() == "closed")
            .Select(s => s.Id).ToHashSet();
        var notStartedIds = statuses
            .Where(s => { var n = (s.StatusName ?? "").Trim().ToLowerInvariant(); return n == "new" || n.Contains("not start"); })
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
            "last3months" => (new DateTime(today.AddMonths(-3).Year, today.AddMonths(-3).Month, 1), today),
            "last6months" => (new DateTime(today.AddMonths(-6).Year, today.AddMonths(-6).Month, 1), today),
            "thisyear"    => (new DateTime(today.Year, 1, 1), today),
            "lastyear"    => (new DateTime(today.Year - 1, 1, 1), new DateTime(today.Year - 1, 12, 31)),
            "last2years"  => (new DateTime(today.Year - 2, 1, 1), today),
            _             => (DateTime.MinValue, DateTime.MaxValue) // alltime
        };
    }
    private static (DateTime dateFrom, DateTime dateTo, DateTime prevFrom, DateTime prevTo, string periodLabel, string prevPeriodLabel)
        ResolveOperationalPeriod(string period,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int? monthFrom = null, int? yearFrom = null,
            int? monthTo = null, int? yearTo = null)
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
                from = new DateTime(today.Year, 1, 1);
                to = today;
                pFrom = new DateTime(today.Year - 1, 1, 1);
                pTo = new DateTime(today.Year - 1, 12, 31);
                lbl = today.Year.ToString();
                pLbl = (today.Year - 1).ToString();
                break;
            case "lastyear":
                from = new DateTime(today.Year - 1, 1, 1);
                to = new DateTime(today.Year - 1, 12, 31);
                pFrom = new DateTime(today.Year - 2, 1, 1);
                pTo = new DateTime(today.Year - 2, 12, 31);
                lbl = (today.Year - 1).ToString();
                pLbl = (today.Year - 2).ToString();
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
            return AccessDeniedView();

        var (dateFrom, dateTo, prevFrom, prevTo, periodLabel, prevPeriodLabel) = ResolveOperationalPeriod(period, customFrom, customTo, monthFrom, yearFrom, monthTo, yearTo);
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

        // allTaskMap built from raw result (includes UserStory) so bug→parent resolution works
        // even when the parent task is a UserStory (matches performance report behaviour)
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
            // Use timesheets as task source (same as performance report: SpentDate filter, bug→parent resolution)
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

            // Append global weighted average (same values as KPI cards) so JS uses correct average line
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
            return AccessDeniedView();

        var (dateFrom, dateTo, _, _, _, _) = ResolveOperationalPeriod(period, customFrom, customTo, monthFrom, yearFrom, monthTo, yearTo);
        var pIds = projectId.HasValue ? new List<int> { projectId.Value } : null;
        var eIds = employeeId.HasValue ? new List<int> { employeeId.Value } : null;

        var tsTask = _timeSheetsService.GetAllTimeSheetAsync(employeeIds: eIds, projectIds: pIds, from: dateFrom, to: dateTo, pageSize: int.MaxValue);
        var employeesTask = _employeeService.GetAllEmployeesAsync(pageSize: int.MaxValue, showInActive: true);
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
            .OrderByDescending(x => x.billableHrs).Take(10).ToList<object>();
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
            return AccessDeniedView();

        var (dateFrom, dateTo, _, _, _, _) = ResolveOperationalPeriod(period, customFrom, customTo, monthFrom, yearFrom, monthTo, yearTo);

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
        var employeesTask = _employeeService.GetAllEmployeesAsync(pageSize: int.MaxValue, showInActive: true);
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
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard))
            return AccessDeniedView();

        var (dateFrom, dateTo, _, _, _, _) = ResolveOperationalPeriod(period, customFrom, customTo, monthFrom, yearFrom, monthTo, yearTo);

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
        var employeesTask = _employeeService.GetAllEmployeesAsync(pageSize: int.MaxValue, showInActive: true);
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
}
