using App.Core.Domain.Employees;
using App.Core.Domain.PerformanceMeasurements;
using App.Data.Extensions;
using App.Services;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.PerformanceMeasurements;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Framework.Models.Extensions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the TeamPerformanceMeasurement model factory implementation
    /// </summary>
    public partial class TeamPerformanceMeasurementModelFactory : ITeamPerformanceMeasurementModelFactory
    {
        #region Fields

        private readonly ITeamPerformanceMeasurementService _teamPerformanceMeasurementService;
        private readonly IKPIMasterService _kPIMasterService;
        private readonly IEmployeeService _employeeService;
        private readonly IDesignationService _designationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IKPIWeightageService _kPIWeightageService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor
        public TeamPerformanceMeasurementModelFactory(ITeamPerformanceMeasurementService teamPerformanceMeasurementService,
            IKPIMasterService kPIMasterService,
            IEmployeeService employeeService,
            IDesignationService designationService,
            IDateTimeHelper dateTimeHelper,
            IKPIWeightageService kPIWeightageService,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _teamPerformanceMeasurementService = teamPerformanceMeasurementService;
            _dateTimeHelper = dateTimeHelper;
            _kPIMasterService = kPIMasterService;
            _designationService = designationService;
            _employeeService = employeeService;
            _kPIWeightageService = kPIWeightageService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }
        #endregion

        #region Utilities
        public virtual async Task PrepareKPIMasterListAsync(TeamPerformanceMeasurementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var kPIMasters = _kPIMasterService.GetAllKPIMasterAsync(null).Result.ToList();
            var kPIWeightages = await _kPIWeightageService.GetAllKPIWeightageAsync("", 0, int.MaxValue, false, null);
            var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            if (model.EmployeeId > 0 && employee != null)
            {
                var kPIMasterDesignationId = kPIMasters
                    .Join(kPIWeightages,
                        km => km.Id,
                        kw => kw.KPIMasterId,
                        (km, kw) => new { KPIMaster = km, KPIWeightage = kw })
                    .Where(j => j.KPIWeightage.DesignationId == Convert.ToInt32(employee.DesignationId))
                    .Select(j => j.KPIMaster)
                    .ToList();

                kPIMasters = kPIMasterDesignationId;
            }

            model.KPIMaster = kPIMasters.Select(item => new KPIMasterModels
            {
                Id = item.Id,
                Name = item.Name,
                Rating = 0
            }).ToList();
        }
        public virtual async Task PrepareEmployeeListAsync(TeamPerformanceMeasurementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Employees.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var employeeName = "";
            var employee = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employee)
            {
                model.Employees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareEmployeeManagerListAsync(TeamPerformanceMeasurementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Manager.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var designations = await _designationService.GetCanRatingDesignationAsync();
            List<int> designationIds = new List<int> {};
           foreach (var designation in designations)
            {
                designationIds.Add(designation.Id);
            }
           

            var getAllManagers = await _employeeService.GetAllEmployeeNameAsync(null);
            var managers = getAllManagers.Where(x => designationIds.Contains(Convert.ToInt32(x.DesignationId)));
            foreach (var p in managers)
            {
                model.AvailableManagers.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareYearrListAsync(TeamPerformanceMeasurementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Years.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            int startYear = DateTime.Now.Year - 110; // Start from 110 years ago
            int currentYear = DateTime.Now.Year;

            // Iterate over the range of years and add them to the model
            for (int year = currentYear; year >= startYear; year--)
            {
                model.Years.Add(new SelectListItem
                {
                    Text = year.ToString(),
                    Value = year.ToString()
                });
            }
        }

        #endregion
        #region Methods

        public virtual async Task<TeamPerformanceMeasurementSearchModel> PrepareTeamPerformanceMeasurementSearchModelAsync(TeamPerformanceMeasurementSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }
        public virtual async Task<TeamPerformanceMeasurementListModel> PrepareTeamPerformanceMeasurementListModelAsync(TeamPerformanceMeasurementSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get TeamPerformance
            var teamPerformance = await _teamPerformanceMeasurementService.GetAllTeamPerformanceMeasurementAsync(
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new TeamPerformanceMeasurementListModel().PrepareToGridAsync(searchModel, teamPerformance, () =>
            {
                return teamPerformance.SelectAwait(async teamPerformances =>
                {
                    var selectedAvailableMonthOption = teamPerformances.MonthId;
                    //fill in model values from the entity
                    var teamPerformanceModel = teamPerformances.ToModel<TeamPerformanceMeasurementModel>();
                    teamPerformanceModel.MonthName = ((MonthEnum)selectedAvailableMonthOption).ToString();

                    KPIMaster kPIMaster = new KPIMaster();
                    kPIMaster = await _kPIMasterService.GetKPIMasterByIdAsync(teamPerformanceModel.KPIMasterId);
                    teamPerformanceModel.KPIName = kPIMaster.Name;

                    Employee employee = new Employee();
                    employee = await _employeeService.GetEmployeeByIdAsync(teamPerformanceModel.EmployeeId);
                    teamPerformanceModel.EmployeeName = employee.FirstName + " " + employee.LastName;

                    employee = await _employeeService.GetEmployeeByIdAsync(teamPerformanceModel.EmployeeManagerId);
                    teamPerformanceModel.EmployeeManagerName = employee.FirstName + " " + employee.LastName;

                    teamPerformanceModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(teamPerformances.CreateOnUtc, DateTimeKind.Utc);
                    teamPerformanceModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(teamPerformances.UpdateOnUtc, DateTimeKind.Utc);

                    return teamPerformanceModel;
                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<TeamPerformanceMeasurementModel> PrepareTeamPerformanceMeasurementModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false)
        {
            var month = await MonthEnum.Select.ToSelectListAsync();
            if (teamPerformance != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = teamPerformance.ToModel<TeamPerformanceMeasurementModel>();

                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                    if (employee != null)
                    {
                        model.EmployeeName = employee.FirstName + " " + employee.LastName;
                    }
                    var employeemanager = await _employeeService.GetEmployeeByIdAsync(model.EmployeeManagerId);
                    if (employeemanager != null)
                    {
                        model.EmployeeManagerName = employeemanager.FirstName + " " + employeemanager.LastName;
                    }


                }

                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                var manager = await _employeeService.GetEmployeeByIdAsync(model.EmployeeManagerId);
                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
                if (manager != null)
                {
                    model.SelectedManagerId.Add(manager.Id);
                }
               

            }
            model.Months = month.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.MonthId.ToString() == store.Value
            }).ToList();

            var teamPerformanceMeasurements = await _teamPerformanceMeasurementService.GetAllTeamPerformanceMeasurementAsync(0, int.MaxValue, false, null);
            var teamPerformanceMeasurement = teamPerformanceMeasurements
                                            .FirstOrDefault(x => x.EmployeeId == model.EmployeeId
                                                            && x.EmployeeManagerId == model.EmployeeManagerId
                                                            && x.MonthId == model.MonthId && x.Year == model.Year);

            if (teamPerformanceMeasurement == null)
            {
                await PrepareKPIMasterListAsync(model);
            }
            else
            {
                model.KPIMaster = JsonConvert.DeserializeObject<List<KPIMasterModels>>(teamPerformanceMeasurement.KPIMasterData);
                model.Id = teamPerformanceMeasurement.Id;
                model.Feedback = teamPerformanceMeasurement.Feedback;
            }
            await _baseAdminModelFactory.PrepareEmployeeAsync(model.AvailableEmployees, false);
            foreach (var employeeItem in model.AvailableEmployees)
            {
                employeeItem.Selected = int.TryParse(employeeItem.Value, out var employeeId)
                    && model.SelectedEmployeeId.Contains(employeeId);
            }
            //await _baseAdminModelFactory.PrepareEmployeeAsync(model.AvailableManagers, false);
            //foreach (var employeeItem in model.AvailableManagers)
            //{
            //    employeeItem.Selected = int.TryParse(employeeItem.Value, out var employeeId)
            //        && model.SelectedEmployeeId.Contains(employeeId);
            //}
            await PrepareEmployeeListAsync(model);
            await PrepareEmployeeManagerListAsync(model);
            await PrepareYearrListAsync(model);
            return model;
        }
        public virtual async Task<AvgMeasurementModel> PrepareMonthlyReviewModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false)
        {
            List<TeamPerformanceMeasurementModel> teamPerformanceMeasurementModels = new List<TeamPerformanceMeasurementModel>();
            AvgMeasurementModel avgMeasurementModel = new AvgMeasurementModel();
            var month = await MonthEnum.Select.ToSelectListAsync();
            if (teamPerformance != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = teamPerformance.ToModel<TeamPerformanceMeasurementModel>();

                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                    if (employee != null)
                    {
                        model.EmployeeName = employee.FirstName + " " + employee.LastName;
                    }

                }
                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);

                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
            }
            model.Months = month.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.MonthId.ToString() == store.Value
            }).ToList();
            var teamPerformanceMeasurements = await _teamPerformanceMeasurementService.GetAllTeamPerformanceMeasurementAsync(0, int.MaxValue, false, null);
            if (model.EmployeeId > 0 && model.MonthId > 0 && model.Year > 0)
            {
                var teamPerformanceMeasurement = teamPerformanceMeasurements
                                                 .Where(x => x.EmployeeId == model.EmployeeId
                                                        && x.MonthId == model.MonthId
                                                        && x.Year == model.Year).ToList();
                //await PrepareKPIMasterListAsync(model);
                if (teamPerformanceMeasurement != null && teamPerformanceMeasurement.Any())
                {
                    foreach (var item in teamPerformanceMeasurement)
                    {
                        TeamPerformanceMeasurementModel measurementModel = new TeamPerformanceMeasurementModel();
                        measurementModel.Id = item.Id;
                        measurementModel.EmployeeId = item.EmployeeId;
                        measurementModel.KPIMaster = JsonConvert.DeserializeObject<List<KPIMasterModels>>(item.KPIMasterData);
                        measurementModel.Feedback = item.Feedback;

                        measurementModel.EmployeeManagerId = item.EmployeeManagerId;
                        teamPerformanceMeasurementModels.Add(measurementModel);
                    }
                    var kPIMasters = _kPIMasterService.GetAllKPIMasterAsync(null).Result.ToList();
                    var kPIWeightages = await _kPIWeightageService.GetAllKPIWeightageAsync("", 0, int.MaxValue, false, null);
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                    if (model.EmployeeId > 0 && employee != null )
                    {
                        var kPIMasterDesignationId = kPIMasters
                            .Join(kPIWeightages,
                                km => km.Id,
                                kw => kw.KPIMasterId,
                                (km, kw) => new { KPIMaster = km, KPIWeightage = kw })
                            .Where(j => j.KPIWeightage.DesignationId == Convert.ToInt32(employee.DesignationId))
                            .Select(j => j.KPIMaster)
                            .ToList();
                        avgMeasurementModel.KPIName = kPIMasterDesignationId.Select(x => x.Name).ToList();
                    }
                    var Managers = await _employeeService.GetAllEmployeeNameAsync(null);
                    var manager = model.Manager;
                    foreach (var item in teamPerformanceMeasurementModels)
                    {
                        foreach (var KPI in avgMeasurementModel.KPIName)
                        {
                            EmployeeRatePerKPI employeeRatePerKPI = new EmployeeRatePerKPI();
                            employeeRatePerKPI.ManagerName = Managers.Where(x => x.Id == item.EmployeeManagerId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                            employeeRatePerKPI.KPIName = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Name).FirstOrDefault();
                            employeeRatePerKPI.Rate = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Rating).FirstOrDefault();
                            avgMeasurementModel.employeeRatePerKPIs.Add(employeeRatePerKPI);
                        }

                        if (avgMeasurementModel != null && item.Feedback != null)
                        {



                            var EmpManager = await _employeeService.GetEmployeeByIdAsync(item.EmployeeManagerId);
                            if (EmpManager != null)
                            {
                                string showFeedback = $@"
<tr>
    <td style='width: 25%;'><strong>{EmpManager.FirstName} {EmpManager.LastName}</strong></td>
    <td style='width: 75%;'>{item.Feedback}</td>
</tr>";

                                avgMeasurementModel.Feedbacks.Add(showFeedback);
                            }

                        }
                    }
                    foreach (var KPI in avgMeasurementModel.KPIName)
                    {
                        EmployeeTotalAvgRate employeeTotalAvgRate = new EmployeeTotalAvgRate();
                        employeeTotalAvgRate.KPIName = KPI;
                        employeeTotalAvgRate.TotalRateing = avgMeasurementModel.employeeRatePerKPIs.Where(x => x.KPIName == KPI).Select(x => x.Rate).Sum();
                        employeeTotalAvgRate.AvgRateing = avgMeasurementModel.employeeRatePerKPIs.Where(x => x.KPIName == KPI).Select(x => x.Rate).Average();
                        avgMeasurementModel.employeeTotalAvgRates.Add(employeeTotalAvgRate);
                    }
                }
                else
                {
                    avgMeasurementModel = null;
                }
            }
         

            //await PrepareEmployeeListAsync(model);
            await PrepareEmployeeManagerListAsync(model);
            //await PrepareYearrListAsync(model);
            // avgMeasurementModel
            return avgMeasurementModel;
        }


        //public virtual async Task<AvgMeasurementModel> PrepareYearlyReviewModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false)
        //{
        //    List<TeamPerformanceMeasurementModel> teamPerformanceMeasurementModels = new List<TeamPerformanceMeasurementModel>();
        //    AvgMeasurementModel avgMeasurementModel = new AvgMeasurementModel();
        //    var month = await MonthEnum.Select.ToSelectListAsync();
        //    var enumValues = Enum.GetValues(typeof(MonthEnum));
        //    var startMonthValue = model.StartMonth;
        //    var endMonthValue = model.EndMonth;

        //    if (teamPerformance != null)
        //    {
        //        if (model == null)
        //        {
        //            model = teamPerformance.ToModel<TeamPerformanceMeasurementModel>();
        //        }
        //    }
        //    model.Months = month.Select(store => new SelectListItem
        //    {
        //        Value = store.Value,
        //        Text = store.Text,
        //        Selected = model.MonthId.ToString() == store.Value
        //    }).ToList();
        //    var teamPerformanceMeasurements = await _teamPerformanceMeasurementService.GetAllTeamPerformanceMeasurementAsync(0, int.MaxValue, false, null);
        //    if (model.EmployeeId > 0 && model.StartMonth > 0 && model.EndMonth > 0 && model.StartYear > 0 && model.EndYear > 0)
        //    {
        //        //if (startMonthValue > 0)
        //        //{
        //        //    foreach (var enumValue in enumValues)
        //        //    {
        //        //        int monthId = (int)enumValue;


        //        //    }
        //        //}

        //        if (startMonthValue > 0 && endMonthValue < 12)
        //        {
        //            // Assuming model.StartMonth and model.EndMonth represent April and March respectively
        //            int previousYear = model.StartYear - 1;
        //            int currentYear = model.EndYear;

        //            // Loop through the enumValues (presumably months)
        //            foreach (var enumValue in enumValues)
        //            {
        //                int monthId = (int)enumValue;

        //                // Check if the current month falls within the desired range
        //                if ((monthId >= startMonthValue && monthId <= 12) || (monthId >= 1 && monthId <= endMonthValue))
        //                {
        //                    // Check if the month falls within the specified period (April of previous year to March of current year)
        //                    if ((monthId >= startMonthValue && monthId <= 12 && model.StartYear == previousYear) ||
        //                        (monthId >= 1 && monthId <= endMonthValue && model.EndYear == currentYear))
        //                    {
        //                        //if (monthId >= startMonthValue && monthId <= endMonthValue)
        //                        //{
        //                        avgMeasurementModel.Months.Add(new SelectListItem
        //                        {
        //                            Value = monthId.ToString(),
        //                            Text = enumValue.ToString()
        //                        });

        //                        var teamPerformanceMeasurement = teamPerformanceMeasurements
        //                            .Where(x => x.EmployeeId == model.EmployeeId
        //                                        && x.MonthId >= model.StartMonth
        //                                        && x.MonthId <= model.EndMonth
        //                                        && x.UpdateOnUtc.Year >= model.StartYear
        //                                        && x.UpdateOnUtc.Year <= model.EndYear)
        //                            .ToList();

        //                        if (teamPerformanceMeasurement != null && teamPerformanceMeasurement.Any())
        //                        {
        //                            foreach (var item in teamPerformanceMeasurement)
        //                            {
        //                                TeamPerformanceMeasurementModel measurementModel = new TeamPerformanceMeasurementModel();
        //                                measurementModel.Id = item.Id;
        //                                measurementModel.EmployeeId = item.EmployeeId;
        //                                measurementModel.KPIMaster = JsonConvert.DeserializeObject<List<KPIMasterModels>>(item.KPIMasterData);
        //                                measurementModel.Feedback = item.Feedback;
        //                                measurementModel.EmployeeManagerId = item.EmployeeManagerId;
        //                                teamPerformanceMeasurementModels.Add(measurementModel);
        //                            }
        //                            var kPIMasters = _kPIMasterService.GetAllKPIMasterAsync(null).Result.ToList();
        //                            var kPIWeightages = await _kPIWeightageService.GetAllKPIWeightageAsync("", 0, int.MaxValue, false, null);
        //                            var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
        //                            if (model.EmployeeId > 0 && employee != null)
        //                            {
        //                                var kPIMasterDesignationId = kPIMasters
        //                                    .Join(kPIWeightages,
        //                                        km => km.Id,
        //                                        kw => kw.KPIMasterId,
        //                                        (km, kw) => new { KPIMaster = km, KPIWeightage = kw })
        //                                    .Where(j => j.KPIWeightage.DesignationId == Convert.ToInt32(employee.DesignationId))
        //                                    .Select(j => j.KPIMaster)
        //                                    .ToList();
        //                                avgMeasurementModel.KPIName = kPIMasterDesignationId.Select(x => x.Name).ToList();
        //                            }
        //                            var Managers = await _employeeService.GetAllEmployeeNameAsync(null);
        //                            var manager = model.Manager;
        //                            foreach (var item in teamPerformanceMeasurementModels)
        //                            {
        //                                foreach (var KPI in avgMeasurementModel.KPIName)
        //                                {
        //                                    EmployeeRatePerKPI employeeRatePerKPI = new EmployeeRatePerKPI();
        //                                    employeeRatePerKPI.ManagerName = Managers.Where(x => x.Id == item.EmployeeManagerId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
        //                                    employeeRatePerKPI.KPIName = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Name).FirstOrDefault();
        //                                    employeeRatePerKPI.Rate = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Rating).FirstOrDefault();
        //                                    avgMeasurementModel.employeeRatePerKPIs.Add(employeeRatePerKPI);
        //                                }
        //                            }

        //                            foreach (var KPI in avgMeasurementModel.KPIName)
        //                            {
        //                                if (monthId >= startMonthValue && monthId <= endMonthValue)
        //                                {
        //                                    // Filter teamPerformanceMeasurement for the current month and KPI
        //                                    var measurementsForMonthAndKPI = teamPerformanceMeasurement
        //                                        .Where(x => x.MonthId == monthId)
        //                                        .SelectMany(x => JsonConvert.DeserializeObject<List<KPIMasterModels>>(x.KPIMasterData))
        //                                        .Where(x => x.Name == KPI)
        //                                        .ToList();
        //                                    double totalAverage = 0;
        //                                    // Calculate total and average ratings for the current KPI and month
        //                                    double totalRating = measurementsForMonthAndKPI.Sum(x => x.Rating);
        //                                    double avgRating = measurementsForMonthAndKPI.Any() ? measurementsForMonthAndKPI.Average(x => x.Rating) : 0;
        //                                    totalAverage += avgRating;
        //                                    totalAverage /= (endMonthValue - startMonthValue + 1) * avgMeasurementModel.KPIName.Count;
        //                                    // Create and populate a new EmployeeTotalAvgRate object
        //                                    EmployeeTotalAvgRate employeeTotalAvgRate = new EmployeeTotalAvgRate();
        //                                    employeeTotalAvgRate.KPIName = KPI;
        //                                    employeeTotalAvgRate.MonthId = monthId;
        //                                    employeeTotalAvgRate.AvgRateing = avgRating;
        //                                    employeeTotalAvgRate.totalaveragae = totalAverage;

        //                                    // Add the object to the list
        //                                    avgMeasurementModel.employeeTotalAvgRates.Add(employeeTotalAvgRate);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            // If no records found for this month, add a default rating of 0 for each KPI
        //                            foreach (var KPI in avgMeasurementModel.KPIName)
        //                            {
        //                                EmployeeTotalAvgRate employeeTotalAvgRate = new EmployeeTotalAvgRate();
        //                                employeeTotalAvgRate.KPIName = KPI;
        //                                employeeTotalAvgRate.MonthId = monthId;
        //                                employeeTotalAvgRate.AvgRateing = 0;

        //                                avgMeasurementModel.employeeTotalAvgRates.Add(employeeTotalAvgRate);
        //                            }
        //                            //}
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    else
        //    {
        //        avgMeasurementModel = null;
        //    }
        //    await PrepareEmployeeManagerListAsync(model);
        //    await PrepareYearrListAsync(model);
        //    return avgMeasurementModel;
        //}

        public virtual async Task<AvgMeasurementModel> PrepareYearlyReviewModelAsync(
       TeamPerformanceMeasurementModel model,
       TeamPerformanceMeasurement teamPerformance,
       bool excludeProperties = false)
        {
            List<TeamPerformanceMeasurementModel> teamPerformanceMeasurementModels = new List<TeamPerformanceMeasurementModel>();
            AvgMeasurementModel avgMeasurementModel = new AvgMeasurementModel();
            var month = await MonthEnum.Select.ToSelectListAsync();
            var enumValues = Enum.GetValues(typeof(MonthEnum));
            var startMonthValue = model.StartMonth;
            var endMonthValue = model.EndMonth;

            if (teamPerformance != null)
            {
                if (model == null)
                {
                    model = teamPerformance.ToModel<TeamPerformanceMeasurementModel>();
                }
            }

            model.Months = month.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.MonthId.ToString() == store.Value
            }).ToList();

            var teamPerformanceMeasurements = await _teamPerformanceMeasurementService.GetAllTeamPerformanceMeasurementAsync(0, int.MaxValue, false, null);

            if (model.EmployeeId > 0 && model.StartMonth > 0 && model.EndMonth > 0 && model.StartYear > 0 && model.EndYear > 0)
            {
                int previousYear = model.StartYear - 1;
                int currentYear = model.EndYear;

                // Initialize KPI names early
                var kPIMasters = await _kPIMasterService.GetAllKPIMasterAsync(null);
                var kPIWeightages = await _kPIWeightageService.GetAllKPIWeightageAsync("", 0, int.MaxValue, false, null);
                var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                if (employee != null)
                {
                    avgMeasurementModel.KPIName = kPIMasters
                        .Join(kPIWeightages,
                            km => km.Id,
                            kw => kw.KPIMasterId,
                            (km, kw) => new { KPIMaster = km, KPIWeightage = kw })
                        .Where(j => j.KPIWeightage.DesignationId == Convert.ToInt32(employee.DesignationId))
                        .Select(j => j.KPIMaster.Name)
                        .ToList();
                }

                var Managers = await _employeeService.GetAllEmployeeNameAsync(null);

                // Loop through the months from start to end
                for (int year = model.StartYear; year <= model.EndYear; year++)
                {
                    int startMonth = (year == model.StartYear) ? startMonthValue : 1;
                    int endMonth = (year == model.EndYear) ? endMonthValue : 12;

                    for (int monthId = startMonth; monthId <= endMonth; monthId++)
                    {
                        avgMeasurementModel.Months.Add(new SelectListItem
                        {
                            Value = monthId.ToString(),
                            Text = ((MonthEnum)monthId).ToString()
                        });

                        var teamPerformanceMeasurement = teamPerformanceMeasurements
                            .Where(x => x.EmployeeId == model.EmployeeId && x.MonthId == monthId && x.Year == year)
                            .ToList();

                        if (teamPerformanceMeasurement.Any())
                        {
                            foreach (var item in teamPerformanceMeasurement)
                            {
                                TeamPerformanceMeasurementModel measurementModel = new TeamPerformanceMeasurementModel
                                {
                                    Id = item.Id,
                                    EmployeeId = item.EmployeeId,
                                    KPIMaster = JsonConvert.DeserializeObject<List<KPIMasterModels>>(item.KPIMasterData),
                                    Feedback = item.Feedback,
                                    EmployeeManagerId = item.EmployeeManagerId
                                };
                                teamPerformanceMeasurementModels.Add(measurementModel);
                            }

                            foreach (var KPI in avgMeasurementModel.KPIName)
                            {
                                var measurementsForMonthAndKPI = teamPerformanceMeasurement
                                    .SelectMany(x => JsonConvert.DeserializeObject<List<KPIMasterModels>>(x.KPIMasterData))
                                    .Where(x => x.Name == KPI)
                                    .ToList();

                                double avgRating = measurementsForMonthAndKPI.Any() ? measurementsForMonthAndKPI.Average(x => x.Rating) : 0;

                                EmployeeTotalAvgRate employeeTotalAvgRate = new EmployeeTotalAvgRate
                                {
                                    KPIName = KPI,
                                    MonthId = monthId,
                                    AvgRateing = avgRating
                                };

                                avgMeasurementModel.employeeTotalAvgRates.Add(employeeTotalAvgRate);
                            }

                            foreach (var item in teamPerformanceMeasurementModels)
                            {
                                foreach (var KPI in avgMeasurementModel.KPIName)
                                {
                                    EmployeeRatePerKPI employeeRatePerKPI = new EmployeeRatePerKPI
                                    {
                                        ManagerName = Managers.Where(x => x.Id == item.EmployeeManagerId)
                                    .Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                        KPIName = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Name).FirstOrDefault(),
                                        Rate = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Rating).FirstOrDefault()
                                    };
                                    avgMeasurementModel.employeeRatePerKPIs.Add(employeeRatePerKPI);
                                }
                            }
                        }
                        else
                        {
                            // If no measurements found for the month, add zero averages for each KPI
                            foreach (var KPI in avgMeasurementModel.KPIName)
                            {
                                EmployeeTotalAvgRate employeeTotalAvgRate = new EmployeeTotalAvgRate
                                {
                                    KPIName = KPI,
                                    MonthId = monthId,
                                    AvgRateing = 0
                                };

                                avgMeasurementModel.employeeTotalAvgRates.Add(employeeTotalAvgRate);
                            }
                        }
                    }
                }
            }
            else
            {
                avgMeasurementModel = null; // If the model is not valid, return null
            }

            // Prepare additional data for the model
            await PrepareEmployeeManagerListAsync(model);
           

            return avgMeasurementModel; // Return the constructed average measurement model
        }



        //prepare team leader review
        public virtual async Task<AvgMeasurementModel> PrepareProjectLeaderReviewModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false)
        {
            List<TeamPerformanceMeasurementModel> teamPerformanceMeasurementModels = new List<TeamPerformanceMeasurementModel>();
            AvgMeasurementModel avgMeasurementModel = new AvgMeasurementModel();
            var month = await MonthEnum.Select.ToSelectListAsync();
            if (teamPerformance != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = teamPerformance.ToModel<TeamPerformanceMeasurementModel>();
                }
            }
            model.Months = month.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.MonthId.ToString() == store.Value
            }).ToList();
            var teamPerformanceMeasurements = await _teamPerformanceMeasurementService.GetAllTeamPerformanceMeasurementAsync(0, int.MaxValue, false, null);
            if (model.EmployeeManagerId > 0 && model.MonthId > 0 && model.Year > 0)
            {
                var teamPerformanceMeasurement = teamPerformanceMeasurements
                                                 .Where(x => x.EmployeeManagerId == model.EmployeeManagerId
                                                        && x.MonthId == model.MonthId
                                                        && x.Year == model.Year).ToList();
                //await PrepareKPIMasterListAsync(model);
                if (teamPerformanceMeasurement != null && teamPerformanceMeasurement.Any())
                {
                    foreach (var item in teamPerformanceMeasurement)
                    {
                        TeamPerformanceMeasurementModel measurementModel = new TeamPerformanceMeasurementModel();
                        measurementModel.Id = item.Id;
                        measurementModel.EmployeeId = item.EmployeeId;
                        measurementModel.KPIMaster = JsonConvert.DeserializeObject<List<KPIMasterModels>>(item.KPIMasterData);
                        measurementModel.Feedback = item.Feedback;
                        measurementModel.EmployeeManagerId = item.EmployeeManagerId;
                        teamPerformanceMeasurementModels.Add(measurementModel);
                    }
                    var kPIMasters = _kPIMasterService.GetAllKPIMasterAsync(null).Result.ToList();
                    var kPIWeightages = await _kPIWeightageService.GetAllKPIWeightageAsync("", 0, int.MaxValue, false, null);
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeManagerId);
                    if (model.EmployeeManagerId > 0 && employee != null)
                    {
                        //var kPIMasterDesignationId = kPIMasters
                        //    .Join(kPIWeightages,
                        //        km => km.Id,
                        //        kw => kw.KPIMasterId,
                        //        (km, kw) => new { KPIMaster = km, KPIWeightage = kw })
                        //    .Where(j => j.KPIWeightage.DesignationId == Convert.ToInt32(employee.DesignationId))
                        //    .Select(j => j.KPIMaster)
                        //    .ToList();
                        //avgMeasurementModel.KPIName = kPIMasterDesignationId.Select(x => x.Name).ToList();
                        var kPIMasterList = kPIMasters
    .Join(kPIWeightages,
        km => km.Id,
        kw => kw.KPIMasterId,
        (km, kw) => new { KPIMaster = km, KPIWeightage = kw })
    .Select(j => j.KPIMaster) // No filtering by designation
    .Distinct()
    .ToList();

                        avgMeasurementModel.KPIName = kPIMasterList.Select(x => x.Name).ToList();

                    }
                    var employees = await _employeeService.GetAllEmployeeNameAsync(model.EmployeeId.ToString());
                    var emp = model.Employees;
                    foreach (var item in teamPerformanceMeasurementModels)
                    {
                        foreach (var KPI in avgMeasurementModel.KPIName)
                        {
                            EmployeeRatePerKPI employeeRatePerKPI = new EmployeeRatePerKPI();
                            employeeRatePerKPI.EmployeeName = employees.Where(x => x.Id == item.EmployeeId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                            employeeRatePerKPI.KPIName = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Name).FirstOrDefault();
                            employeeRatePerKPI.Rate = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Rating).FirstOrDefault();
                            avgMeasurementModel.employeeRatePerKPIs.Add(employeeRatePerKPI);
                        }
                    }

                    //foreach (var KPI in avgMeasurementModel.KPIName)
                    //{
                    //    EmployeeTotalAvgRate employeeTotalAvgRate = new EmployeeTotalAvgRate();
                    //    employeeTotalAvgRate.KPIName = KPI;
                    //    employeeTotalAvgRate.TotalRateing = avgMeasurementModel.employeeRatePerKPIs.Where(x => x.KPIName == KPI).Select(x => x.Rate).Sum();
                    //    employeeTotalAvgRate.AvgRateing = avgMeasurementModel.employeeRatePerKPIs.Where(x => x.KPIName == KPI).Select(x => x.Rate).Average();
                    //    avgMeasurementModel.employeeTotalAvgRates.Add(employeeTotalAvgRate);
                    //}
                }
                else
                {
                    avgMeasurementModel = null;
                }
            }
            //await PrepareEmployeeListAsync(model);
            //await PrepareEmployeeManagerListAsync(model);
        
            // avgMeasurementModel
            return avgMeasurementModel;
        }

        //public virtual async Task<AvgMeasurementModel> PrepareProjectLeaderReviewModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false)
        //{
        //    List<TeamPerformanceMeasurementModel> teamPerformanceMeasurementModels = new List<TeamPerformanceMeasurementModel>();
        //    AvgMeasurementModel avgMeasurementModel = new AvgMeasurementModel();
        //    var month = await MonthEnum.Select.ToSelectListAsync();
        //    if (teamPerformance != null)
        //    {
        //        //fill in model values from the entity
        //        if (model == null)
        //        {
        //            model = teamPerformance.ToModel<TeamPerformanceMeasurementModel>();
        //        }
        //    }
        //    model.Months = month.Select(store => new SelectListItem
        //    {
        //        Value = store.Value,
        //        Text = store.Text,
        //        Selected = model.MonthId.ToString() == store.Value
        //    }).ToList();
        //    var teamPerformanceMeasurements = await _teamPerformanceMeasurementService.GetAllTeamPerformanceMeasurementAsync(0, int.MaxValue, false, null);
        //    if (model.EmployeeManagerId > 0 && model.StartMonth > 0 && model.EndMonth > 0 && model.StartYear > 0 && model.EndYear > 0)
        //    {
        //        var teamPerformanceMeasurement = teamPerformanceMeasurements
        //                                         .Where(x => x.EmployeeManagerId == model.EmployeeManagerId
        //                                                && x.MonthId >= model.StartMonth
        //                                                && x.MonthId <= model.EndMonth
        //                                                && x.UpdateOnUtc.Year >= model.StartYear
        //                                                && x.UpdateOnUtc.Year <= model.EndYear).ToList();
        //        //await PrepareKPIMasterListAsync(model);
        //        if (teamPerformanceMeasurement != null && teamPerformanceMeasurement.Any())
        //        {
        //            foreach (var item in teamPerformanceMeasurement)
        //            {
        //                TeamPerformanceMeasurementModel measurementModel = new TeamPerformanceMeasurementModel();
        //                measurementModel.Id = item.Id;
        //                measurementModel.EmployeeId = item.EmployeeId;
        //                measurementModel.KPIMaster = JsonConvert.DeserializeObject<List<KPIMasterModels>>(item.KPIMasterData);
        //                measurementModel.Feedback = item.Feedback;
        //                measurementModel.EmployeeManagerId = item.EmployeeManagerId;
        //                teamPerformanceMeasurementModels.Add(measurementModel);
        //            }
        //            var kPIMasters = _kPIMasterService.GetAllKPIMasterAsync(null).Result.ToList();
        //            var kPIWeightages = await _kPIWeightageService.GetAllKPIWeightageAsync("", 0, int.MaxValue, false, null);
        //            var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeManagerId);
        //            if (model.EmployeeManagerId > 0 && employee != null)
        //            {
        //                var kPIMasterDesignationId = kPIMasters
        //                    .Join(kPIWeightages,
        //                        km => km.Id,
        //                        kw => kw.KPIMasterId,
        //                        (km, kw) => new { KPIMaster = km, KPIWeightage = kw })
        //                    .Where(j => j.KPIWeightage.DesignationId == Convert.ToInt32(employee.DesignationId))
        //                    .Select(j => j.KPIMaster)
        //                    .ToList();
        //                avgMeasurementModel.KPIName = kPIMasterDesignationId.Select(x => x.Name).ToList();
        //            }
        //            var employees = await _employeeService.GetAllEmployeeNameAsync(model.EmployeeId.ToString());
        //            var emp = model.Employees;
        //            foreach (var item in teamPerformanceMeasurementModels)
        //            {
        //                foreach (var KPI in avgMeasurementModel.KPIName)
        //                {
        //                    EmployeeRatePerKPI employeeRatePerKPI = new EmployeeRatePerKPI();
        //                    employeeRatePerKPI.EmployeeName = employees.Where(x => x.Id == item.EmployeeId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
        //                    employeeRatePerKPI.KPIName = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Name).FirstOrDefault();
        //                    employeeRatePerKPI.Rate = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Rating).FirstOrDefault();
        //                    avgMeasurementModel.employeeRatePerKPIs.Add(employeeRatePerKPI);
        //                }
        //            }
        //            //foreach (var KPI in avgMeasurementModel.KPIName)
        //            //{
        //            //    EmployeeTotalAvgRate employeeTotalAvgRate = new EmployeeTotalAvgRate();
        //            //    employeeTotalAvgRate.KPIName = KPI;
        //            //    employeeTotalAvgRate.TotalRateing = avgMeasurementModel.employeeRatePerKPIs.Where(x => x.KPIName == KPI).Select(x => x.Rate).Sum();
        //            //    employeeTotalAvgRate.AvgRateing = avgMeasurementModel.employeeRatePerKPIs.Where(x => x.KPIName == KPI).Select(x => x.Rate).Average();
        //            //    avgMeasurementModel.employeeTotalAvgRates.Add(employeeTotalAvgRate);
        //            //}
        //        }
        //        else
        //        {
        //            avgMeasurementModel = null;
        //        }
        //    }
        //    //await PrepareEmployeeListAsync(model);
        //    //await PrepareEmployeeManagerListAsync(model);
        //    await PrepareYearrListAsync(model);
        //    // avgMeasurementModel
        //    return avgMeasurementModel;
        //}
        #endregion
    }
}
