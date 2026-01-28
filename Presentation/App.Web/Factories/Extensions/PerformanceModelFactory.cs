using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.PerformanceMeasurements;
using App.Core.Domain.PerformanceMeasurements;
using App.Services;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.PerformanceMeasurements;
using App.Services.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Models.PerformanceMeasurements;
using LinqToDB;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the TeamPerformanceMeasurement model factory implementation
    /// </summary>
    public partial class PerformanceModelFactory : IPerformanceModelFactory
    {
        #region Fields

        private readonly ITeamPerformanceMeasurementService _teamPerformanceMeasurementService;
        private readonly IKPIMasterService _kPIMasterService;
        private readonly IEmployeeService _employeeService;
        private readonly IDesignationService _designationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IKPIWeightageService _kPIWeightageService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IWorkContext _workContext;
        private readonly TeamPerformanceSettings _teamPerformanceSettings;

        #endregion

        #region Ctor
        public PerformanceModelFactory(ITeamPerformanceMeasurementService teamPerformanceMeasurementService,
            IKPIMasterService kPIMasterService,
            IEmployeeService employeeService,
            IDesignationService designationService,
            IDateTimeHelper dateTimeHelper,
            IKPIWeightageService kPIWeightageService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            IWorkContext workContext,
            TeamPerformanceSettings teamPerformanceSettings)
        {
            _teamPerformanceMeasurementService = teamPerformanceMeasurementService;
            _dateTimeHelper = dateTimeHelper;
            _kPIMasterService = kPIMasterService;
            _designationService = designationService;
            _employeeService = employeeService;
            _kPIWeightageService = kPIWeightageService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _workContext = workContext;
            _teamPerformanceSettings = teamPerformanceSettings;
        }
        #endregion

        #region Utilities
        public virtual async Task PrepareKPIMasterListAsync(PerformanceMeasurementModel model)
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
        protected virtual async Task PrepareEmployeeListAsync(PerformanceMeasurementModel model, bool excludeCurrentEmployee = false)
        {
            // Get the current logged-in employee ID
            int currentEmployeeId = 0;
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            if (currentCustomer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(currentCustomer.Id);
                if (employee != null)
                {
                    currentEmployeeId = employee.Id;
                }
            }

            // Fetch IDs of junior employees
            var juniorEmployeeIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currentEmployeeId);

            // Fetch details of all junior employees based on their IDs
            var juniorEmployees = new List<Employee>();
            foreach (var employeeId in juniorEmployeeIds)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(employeeId); // Ensure this method exists in the service
                if (employee != null)
                {
                    juniorEmployees.Add(employee);
                }
            }

            
            model.Employees = new List<SelectListItem>
    {
        new SelectListItem
        {
            Text = "Select",
            Value = "0"
        }
    };

            foreach (var e in juniorEmployees.Where(e => !excludeCurrentEmployee || e.Id != currentEmployeeId))
            {
                model.Employees.Add(new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.FirstName} {e.LastName}", 
                    Selected = model.EmployeeId == e.Id
                });
            }
        }



        public virtual async Task PrepareEmployeeManagerListAsync(PerformanceMeasurementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Manager.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            List<int> designationIds = new List<int> { 2, 3, 4 };
            var getAllManagers = await _employeeService.GetAllEmployeeNameAsync(null);
            var managers = getAllManagers.Where(x => designationIds.Contains(Convert.ToInt32(x.DesignationId)));
            foreach (var p in managers)
            {
                model.Manager.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareYearrListAsync(PerformanceMeasurementModel model)
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


        public virtual async Task<PerformanceMeasurementModel> PreparePerformanceMeasurementModelAsync(PerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false)
        {
            var month = await MonthEnum.Select.ToSelectListAsync();
            if (teamPerformance != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = teamPerformance.ToModel<PerformanceMeasurementModel>();
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
                                                            && x.EmployeeManagerId ==               model.EmployeeManagerId
                                                            && x.MonthId == model.MonthId);

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
            await PrepareEmployeeListAsync(model,true);
            await PrepareYearrListAsync(model);
            await PrepareEmployeeManagerListAsync(model);
            return model;
        }
        public virtual async Task<AvgMeasurementModel> PrepareMonthlyReviewModelAsync(PerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false)
        {
            List<PerformanceMeasurementModel> teamPerformanceMeasurementModels = new List<PerformanceMeasurementModel>();
            AvgMeasurementModel avgMeasurementModel = new AvgMeasurementModel();
            var month = await MonthEnum.Select.ToSelectListAsync();
            if (teamPerformance != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = teamPerformance.ToModel<PerformanceMeasurementModel>();
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
                        PerformanceMeasurementModel measurementModel = new()
                        {
                            Id = item.Id,
                            EmployeeId = item.EmployeeId,
                            KPIMaster = JsonConvert.DeserializeObject<List<KPIMasterModels>>(item.KPIMasterData),
                            Feedback = item.Feedback,
                            EmployeeManagerId = item.EmployeeManagerId
                        };
                        teamPerformanceMeasurementModels.Add(measurementModel);
                        if (avgMeasurementModel != null && item.Feedback != null)
                        {
                            avgMeasurementModel.ShowFeedbackId = _teamPerformanceSettings.FeedbackShowId;
                            if (_teamPerformanceSettings.FeedbackShowId == 1)
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
                            else if (_teamPerformanceSettings.FeedbackShowId == 2)
                            {


                                string showFeedback = $@"
<tr>
    <td>{item.Feedback}</td>
</tr>";

                                avgMeasurementModel.Feedbacks.Add(showFeedback);

                            }
                        }

                    }
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
                        avgMeasurementModel.KPIName = kPIMasterDesignationId.Select(x => x.Name).ToList();
                    }
                    var Managers = await _employeeService.GetAllEmployeeNameAsync(null);
                    var manager = model.Manager;
                    foreach (var item in teamPerformanceMeasurementModels)
                    {
                        foreach (var KPI in avgMeasurementModel.KPIName)
                        {
                            EmployeeRatePerKPI employeeRatePerKPI = new()
                            {
                                ManagerName = Managers.Where(x => x.Id == item.EmployeeManagerId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                KPIName = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Name).FirstOrDefault(),
                                Rate = item.KPIMaster.Where(x => x.Name == KPI).Select(x => x.Rating).FirstOrDefault()
                            };
                            avgMeasurementModel.employeeRatePerKPIs.Add(employeeRatePerKPI);
                        }
                    }
                    foreach (var KPI in avgMeasurementModel.KPIName)
                    {
                        EmployeeTotalAvgRate employeeTotalAvgRate = new()
                        {
                            KPIName = KPI,
                            TotalRating = avgMeasurementModel.employeeRatePerKPIs.Where(x => x.KPIName == KPI).Select(x => x.Rate).Sum(),
                            AvgRateing = avgMeasurementModel.employeeRatePerKPIs.Where(x => x.KPIName == KPI).Select(x => x.Rate).Average()
                        };
                        avgMeasurementModel.employeeTotalAvgRates.Add(employeeTotalAvgRate);
                    }
                }
                else
                {
                    avgMeasurementModel = null;
                }
            }
            //await PrepareEmployeeListAsync(model);
            await PrepareEmployeeListAsync(model, false);

            await PrepareEmployeeManagerListAsync(model);
            //await PrepareYearrListAsync(model);
            // avgMeasurementModel
            if(avgMeasurementModel != null)
            avgMeasurementModel.employeeRatePerKPIs = null;

            return avgMeasurementModel;
        }

        public virtual async Task<AvgMeasurementModel> PrepareYearlyReviewModelAsync(
       PerformanceMeasurementModel model,
       TeamPerformanceMeasurement teamPerformance,
       bool excludeProperties = false)
        {
            List<PerformanceMeasurementModel> teamPerformanceMeasurementModels = new List<PerformanceMeasurementModel>();
            AvgMeasurementModel avgMeasurementModel = new AvgMeasurementModel();
            var month = await MonthEnum.Select.ToSelectListAsync();
            var enumValues = Enum.GetValues(typeof(MonthEnum));
            var startMonthValue = model.StartMonth;
            var endMonthValue = model.EndMonth;

            if (teamPerformance != null)
            {
                if (model == null)
                {
                    model = teamPerformance.ToModel<PerformanceMeasurementModel>();
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
                                PerformanceMeasurementModel measurementModel = new PerformanceMeasurementModel
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
            await PrepareEmployeeListAsync(model, false);

            return avgMeasurementModel; // Return the constructed average measurement model
        }

        public virtual async Task<AvgMeasurementModel> PrepareProjectLeaderReviewModelAsync(PerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false)
        {
            List<PerformanceMeasurementModel> teamPerformanceMeasurementModels = new List<PerformanceMeasurementModel>();
            AvgMeasurementModel avgMeasurementModel = new AvgMeasurementModel();
            var month = await MonthEnum.Select.ToSelectListAsync();
            if (teamPerformance != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = teamPerformance.ToModel<PerformanceMeasurementModel>();
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
                        PerformanceMeasurementModel measurementModel = new PerformanceMeasurementModel();
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
                        var kPIMasterDesignationId = kPIMasters
   .Join(kPIWeightages,
       km => km.Id,
       kw => kw.KPIMasterId,
       (km, kw) => new { KPIMaster = km, KPIWeightage = kw })
   .Select(j => j.KPIMaster) // No filtering by designation
   .Distinct()
   .ToList();
                        avgMeasurementModel.KPIName = kPIMasterDesignationId.Select(x => x.Name).ToList();
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
         

            // avgMeasurementModel
            return avgMeasurementModel;
        }

        #endregion
    }
}
