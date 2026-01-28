using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Security;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.PerformanceMeasurements;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Framework.Mvc.Filters;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class TeamPerformanceMeasurementController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ITeamPerformanceMeasurementModelFactory _teamPerformanceMeasurementModelFactory;
        private readonly ITeamPerformanceMeasurementService _teamPerformanceMeasurementService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public TeamPerformanceMeasurementController(IPermissionService permissionService,
            ITeamPerformanceMeasurementModelFactory teamPerformanceMeasurementModelFactory,
            ITeamPerformanceMeasurementService teamPerformanceMeasurementService,
            INotificationService notificationService,
            ILocalizationService localizationService
            )
        {
            _permissionService = permissionService;
            _teamPerformanceMeasurementModelFactory = teamPerformanceMeasurementModelFactory;
            _teamPerformanceMeasurementService = teamPerformanceMeasurementService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Add Rating Methods

        public virtual async Task<IActionResult> Create(int monthId, int managerId, int employeeId, int Year)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAddRating, PermissionAction.View))
                return AccessDeniedView();
            TeamPerformanceMeasurementModel teamPerformanceMeasurementModel = new TeamPerformanceMeasurementModel();
            teamPerformanceMeasurementModel.MonthId = monthId;
            teamPerformanceMeasurementModel.EmployeeManagerId = managerId;
            teamPerformanceMeasurementModel.EmployeeId = employeeId;
            teamPerformanceMeasurementModel.SelectedEmployeeId.Add(employeeId);
            teamPerformanceMeasurementModel.SelectedManagerId.Add(managerId);
            teamPerformanceMeasurementModel.Year = Year;


            //prepare model
            var model = await _teamPerformanceMeasurementModelFactory.PrepareTeamPerformanceMeasurementModelAsync(teamPerformanceMeasurementModel, null);

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/TeamPerformanceMeasurement/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(TeamPerformanceMeasurementModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAddRating, PermissionAction.View))
                return AccessDeniedView();


            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeId = selectedEmployeeId;
            int selectedManagerId = model.SelectedManagerId.FirstOrDefault();
            model.EmployeeManagerId = selectedManagerId;

            if (selectedManagerId == 0 || selectedEmployeeId == 0)
            {
                if (selectedManagerId == 0)
                { _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectManagerValidation")); }
                else { _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation")); }

                //prepare model
                model = await _teamPerformanceMeasurementModelFactory.PrepareTeamPerformanceMeasurementModelAsync(model, null, true);

                //if we got this far, something failed, redisplay form
                return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/TeamPerformanceMeasurement/Create.cshtml", model);
            }
            if (selectedEmployeeId == selectedManagerId && selectedEmployeeId != 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.EmployeeManagerNotSameValidation"));
                //prepare model
                model = await _teamPerformanceMeasurementModelFactory.PrepareTeamPerformanceMeasurementModelAsync(model, null, true);

                //if we got this far, something failed, redisplay form
                return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/TeamPerformanceMeasurement/Create.cshtml", model);
            }
            if (ModelState.IsValid)
            {
                var teamPerformances = model.ToEntity<TeamPerformanceMeasurement>();
                teamPerformances.CreateOnUtc = DateTime.UtcNow;
                teamPerformances.UpdateOnUtc = DateTime.UtcNow;
                teamPerformances.Year = model.Year;
                teamPerformances.KPIMasterData = JsonConvert.SerializeObject(model.KPIMaster);
                if (model.Id == 0)
                {
                    await _teamPerformanceMeasurementService.InsertTeamPerformanceMeasurementAsync(teamPerformances);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Rating.Added"));
                }
                else
                {
                    teamPerformances.UpdateOnUtc = DateTime.UtcNow;
                    await _teamPerformanceMeasurementService.UpdateTeamPerformanceMeasurementAsync(teamPerformances);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Rating.Updated"));
                }

                if (!continueEditing)
                    return RedirectToAction("Create", "TeamPerformanceMeasurement", new { Monthid = model.MonthId, ManagerId = model.EmployeeManagerId, employeeId = model.EmployeeId, year = model.Year });

                return RedirectToAction("Edit", new { id = teamPerformances.Id });
            }
            //prepare model
            model = await _teamPerformanceMeasurementModelFactory.PrepareTeamPerformanceMeasurementModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/TeamPerformanceMeasurement/Create.cshtml", model);
        }

        #endregion

        #region Monthly Review Methods

        public virtual async Task<IActionResult> AvgCreate(int monthId, int year, int selectedEmployeeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMonthlyReview, PermissionAction.View))
                return AccessDeniedView();
            TeamPerformanceMeasurementModel teamPerformanceMeasurementModel = new TeamPerformanceMeasurementModel();

            teamPerformanceMeasurementModel.MonthId = monthId;
            teamPerformanceMeasurementModel.EmployeeId = selectedEmployeeId;
            teamPerformanceMeasurementModel.Year = year;
            TeamPerformanceMeasurement teamPerformance = new TeamPerformanceMeasurement();

            var model = await _teamPerformanceMeasurementModelFactory.PrepareTeamPerformanceMeasurementModelAsync(teamPerformanceMeasurementModel, null);

            model.measurementModel = await _teamPerformanceMeasurementModelFactory.PrepareMonthlyReviewModelAsync(teamPerformanceMeasurementModel, teamPerformance, false);

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/TeamPerformanceMeasurement/AvgCreate.cshtml", model);
        }

        #endregion

        #region Yearly Review Methods

        public virtual async Task<IActionResult> YearlyReviewCreate(int startmonth, int endmonth, int startYear, int endYear, int selectedEmployeeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageYearlyReview, PermissionAction.View))
                return AccessDeniedView();

            TeamPerformanceMeasurementModel teamPerformanceMeasurementModel = new TeamPerformanceMeasurementModel();
            teamPerformanceMeasurementModel.StartMonth = startmonth;
            teamPerformanceMeasurementModel.EndMonth = endmonth;
            teamPerformanceMeasurementModel.StartYear = startYear;
            teamPerformanceMeasurementModel.EndYear = endYear;
            teamPerformanceMeasurementModel.EmployeeId = selectedEmployeeId;
            TeamPerformanceMeasurement teamPerformance = new TeamPerformanceMeasurement();

            //prepare model
            var model = await _teamPerformanceMeasurementModelFactory.PrepareTeamPerformanceMeasurementModelAsync(teamPerformanceMeasurementModel, null);

            // Validate the date range (months difference)
            if (startYear != 0 && startmonth != 0 && endYear != 0 && endmonth != 0)
            {
                var startDate = new DateTime(startYear, startmonth, 1);
                var endDate = new DateTime(endYear, endmonth, 1);

                var monthDifference = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

                // Check if the difference in months exceeds 12
                if (monthDifference >= 12)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.YearError"));
                    return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/TeamPerformanceMeasurement/YearlyReviewCreate.cshtml", model);
                }
            }
            model.measurementModel = await _teamPerformanceMeasurementModelFactory.PrepareYearlyReviewModelAsync(teamPerformanceMeasurementModel, teamPerformance, false);

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/TeamPerformanceMeasurement/YearlyReviewCreate.cshtml", model);
        }

        #endregion

        #region Project Leader Review Methods

        public virtual async Task<IActionResult> ProjectLeaderReviewCreate(int monthId, int year, int managerId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectLeaderReview, PermissionAction.View))
                return AccessDeniedView();

            TeamPerformanceMeasurementModel teamPerformanceMeasurementModel = new TeamPerformanceMeasurementModel();
            teamPerformanceMeasurementModel.MonthId = monthId;
            teamPerformanceMeasurementModel.Year = year;
            if (teamPerformanceMeasurementModel.SelectedManagerId != null)

                teamPerformanceMeasurementModel.EmployeeManagerId = managerId;
            teamPerformanceMeasurementModel.SelectedManagerId = new List<int> { managerId };

            TeamPerformanceMeasurement teamPerformance = new TeamPerformanceMeasurement();

            var model = await _teamPerformanceMeasurementModelFactory.PrepareTeamPerformanceMeasurementModelAsync(teamPerformanceMeasurementModel, null);

            model.measurementModel = await _teamPerformanceMeasurementModelFactory.PrepareProjectLeaderReviewModelAsync(teamPerformanceMeasurementModel, teamPerformance, false);

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/TeamPerformanceMeasurement/ProjectLeaderReviewCreate.cshtml", model);
        }

        #endregion

        #region Delete/DeleteSelect

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTeamPerformanceMeasurement))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var teamPerformances = await _teamPerformanceMeasurementService.GetTeamPerformanceMeasurementByIdAsync(id);
            if (teamPerformances == null)
                return RedirectToAction("List");

            await _teamPerformanceMeasurementService.DeleteTeamPerformanceMeasurementAsync(teamPerformances);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.projects.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTeamPerformanceMeasurement))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _teamPerformanceMeasurementService.GetTeamPerformanceMeasurementByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _teamPerformanceMeasurementService.DeleteTeamPerformanceMeasurementAsync(item);
            }
            return Json(new { Result = true });
        }

        #endregion
    }
}