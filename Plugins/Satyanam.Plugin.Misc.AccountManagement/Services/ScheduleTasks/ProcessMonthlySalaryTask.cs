using App.Services.Helpers;
using App.Services.Logging;
using App.Services.ScheduleTasks;
using System;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services.ScheduleTasks;

public partial class ProcessMonthlySalaryTask : IScheduleTask
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IEmployeeSalaryService _employeeSalaryService;
    private readonly ILogger _logger;
    private readonly ExpenseManagementSettings _expenseManagementSettings;

    #endregion

    #region Ctor

    public ProcessMonthlySalaryTask(IDateTimeHelper dateTimeHelper,
        IEmployeeSalaryService employeeSalaryService,
        ILogger logger,
        ExpenseManagementSettings expenseManagementSettings)
    {
        _dateTimeHelper = dateTimeHelper;
        _employeeSalaryService = employeeSalaryService;
        _logger = logger;
        _expenseManagementSettings = expenseManagementSettings;
    }

    #endregion

    #region Methods

    public virtual async Task ExecuteAsync()
    {
        var today = await _dateTimeHelper.GetIndianTimeAsync();

        if (_expenseManagementSettings.SalaryProcessingDay <= 0)
            return;

        if (today.Day != _expenseManagementSettings.SalaryProcessingDay)
            return;

        try
        {
            await _employeeSalaryService.ProcessMonthlySalariesAsync(today.Month, today.Year);
            await _logger.InformationAsync($"[ProcessMonthlySalaryTask] Completed salary processing for {today.Month}/{today.Year}");
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("[ProcessMonthlySalaryTask] Error during monthly salary processing.", ex);
        }
    }

    #endregion
}
