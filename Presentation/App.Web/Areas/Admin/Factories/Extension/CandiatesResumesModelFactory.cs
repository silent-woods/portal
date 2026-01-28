using App.Core.Domain.Extension.Candidate;
using App.Core.Domain.ManageResumes;
using App.Core.Domain.result;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.ManageResumes;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.InterviewQeations.Models;
using App.Web.Areas.Admin.Manageresumes;
using App.Web.Areas.Admin.Models.ManageResumes;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Services.Recruitements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the timesheet model factory implementation
    /// </summary>
    public partial class CandiatesResumesModelFactory : ICandiatesResumesModelFactory
    {
        #region Fields

        private readonly ICandiatesResumesService _candiatesResumesService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly IRecruitementService _recruitementservice;
        //private readonly ITimeSheetsService _timeSheetsService;
        #endregion

        #region Ctor

        public CandiatesResumesModelFactory(ICandiatesResumesService candiatesResumesService,
            IDateTimeHelper dateTimeHelper,
           IEmployeeService employeeService
,
           IRecruitementService recruitementservice
            //ITimeSheetsService timeSheetsService
            )
        {
            _candiatesResumesService = candiatesResumesService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _recruitementservice = recruitementservice;
            //_timeSheetsService = timeSheetsService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareresultListAsync(CandiatesResumesModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var typeId = 1;
            var kPIMasters = _candiatesResumesService.GetAllQuestions(typeId);

            model.Addresses = kPIMasters.Select(item => new RecruitementModel
            {

                Category =
                   ((CategoryEnum)item.CategoryId).ToString()
            }).ToList();

            model.Addresses = kPIMasters.Select(item => new RecruitementModel
            {
                Id = item.Id,
                Category =
                   ((CategoryEnum)item.CategoryId).ToString(),
                Question = item.Question
            }).ToList();


        }
        public virtual async Task PrepareresultAsync(CandiatesResultModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var typeId = 1;
            var kPIMasters = _candiatesResumesService.GetAllQuestions(typeId);

            model.Addresses = kPIMasters.Select(item => new RecruitementModel
            {

                Category =
                   ((CategoryEnum)item.CategoryId).ToString()
            }).ToList();

            model.Addresses = kPIMasters.Select(item => new RecruitementModel
            {
                Id = item.Id,
                Category =
                   ((CategoryEnum)item.CategoryId).ToString(),
                Question = item.Question
            }).ToList();


        }
        #endregion

        #region Methods

        public virtual async Task<CandiatesResumesSearchModel> PrepareCandiatesResumesSearchModelAsync(CandiatesResumesSearchModel searchModel)
        {
            var status = await InterviewStatusEnum.Select.ToSelectListAsync();
            searchModel.AvailableStatus = status.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.StatusId.ToString() == store.Value
            }).ToList();

            var Apllyfor = await AppliedForEnum.Select.ToSelectListAsync();
            searchModel.AvailableApplyFor = Apllyfor.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.AppliedForId.ToString() == store.Value
            }).ToList();
            var employeeName = "";
            var employee = await _employeeService.GetAllEmployeeNameAsync(employeeName);

            searchModel.AvailableInterviewer = employee.Select(store => new SelectListItem
            {
                Text = store.FirstName + " " + store.LastName,
                Value = store.Id.ToString(),
                Selected = searchModel.SelectedInterviewer.Contains(store.Id)
            }).ToList();

            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<CandiatesResumesListModel> PrepareCandiatesResumesListModelAsync
          (CandiatesResumesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (!searchModel.SelectedInterviewer.Any())
            {
                var employeeName = "";
                var employee = await _employeeService.GetAllEmployeeNameAsync(employeeName);
                searchModel.SelectedInterviewer = employee
                    .Where(cr => cr.FirstName + cr.LastName != cr.FirstName + cr.LastName).Select(cr => cr.Id).ToList();
            }
            else
            {
                var employeeName = "";
                var employee = await _employeeService.GetAllEmployeeNameAsync(employeeName);
                //if (employee != null)
                //    searchModel.SelectedInterviewer.Remove(employee.Id);
            }
            //get timesheet
            var timeSheet = await _candiatesResumesService.GetAllCandiatesResumesAsync(firstName: searchModel.FirstName, lastName: searchModel.LastName, email: searchModel.Email, mobileno: searchModel.MobileNo, degree: searchModel.Degree, appliedForId: searchModel.AppliedForId, statusId: searchModel.StatusId, InterviewrIds: searchModel.SelectedInterviewer.ToArray(), PracticalRoundDate: searchModel.PracticalRoundDate, TechnicalRoundDate: searchModel.TechnicalRoundDate,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new CandiatesResumesListModel().PrepareToGridAsync(searchModel, timeSheet, () =>
            {
                return timeSheet.SelectAwait(async timesheet =>
                {
                    //fill in model values from the entity

                    var model = timesheet.ToModel<CandiatesResumesModel>();
                    model.Percentages = $"{timesheet.Percentage:F2} {"%"}";
                    model.MobileNumber = timesheet.MobileNo;
                    var selectedAvailableStatus = model.StatusId;
                    var selectedAvailableapplyfor = model.AppliedForId;
                    model.Status = ((InterviewStatusEnum)selectedAvailableStatus).ToString();
                    model.AppliedFor = ((AppliedForEnum)selectedAvailableapplyfor).ToString();
                    model.TechnicalRoundDate = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.TechnicalRoundDate, DateTimeKind.Utc);
                    model.PracticalRoundDate = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.PracticalRoundDate, DateTimeKind.Utc);

                    model.Interviewer = string.Join(", ",
                   (await _candiatesResumesService.GetCustomerRoleBySystemNameAsync(timesheet)).Select(role => role.FirstName + role.LastName));

                    return model;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<CandiatesResumesModel> PrepareCandiatesResumesModelAsync(CandiatesResumesModel model, CandidatesResumes candiatesResumes, bool excludeProperties = false)
        {
            var interviwstatus = await InterviewStatusEnum.Select.ToSelectListAsync();
            var resultstatus = await ResultStatusEnum.Select.ToSelectListAsync();
            var ApplyFor = await AppliedForEnum.Select.ToSelectListAsync();
            if (candiatesResumes != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = candiatesResumes.ToModel<CandiatesResumesModel>();
                    model.MobileNumber = candiatesResumes.MobileNo;
                }
                model.SelectedInterviewer = (await _candiatesResumesService.GetEmployeeIdsAsync(candiatesResumes)).ToList();
            }
            model.AvailableStatus = interviwstatus.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.StatusId.ToString() == store.Value
            }).ToList();
            model.AvailableApplyFor = ApplyFor.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.StatusId.ToString() == store.Value
            }).ToList();

            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var employee in employees)
            {
                model.Employee.Add(new SelectListItem
                {
                    Text = employee.FirstName + " " + employee.LastName,
                    Value = employee.Id.ToString(),
                    Selected = model.SelectedInterviewer.Contains(employee.Id)
                });
            }

            model.AvailableResultStatus = resultstatus.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.ResultStatusId.ToString() == store.Value
            }).ToList();
            var traineeList = await _candiatesResumesService.GetByConditionIdAsync(model.Id);
            // Create a list to hold the models
            foreach (var trainee in traineeList)
            {

                var selectedAvailableDaysOption = trainee.ResultStatusId;
                var models = new CandiatesResultModel();

                // Populate the model with trainee data
                models.Id = trainee.Id;
                models.CandidateId = trainee.CandidateId;
                models.Communication = trainee.Communication;
                models.ConfidentLevel = trainee.ConfidentLevel;
                models.Feedback = trainee.Feedback;
                models.Resulstatus = ((ResultStatusEnum)selectedAvailableDaysOption).ToString();
                model.resultModel.Add(models);

            }

            foreach (var trainee in traineeList)
            {
                var m = new CandiatesResultModel();
                var jsonString = trainee.ResultData;

                // Deserialize the JSON string back into a list of objects
                List<Questions> questions = JsonConvert.DeserializeObject<List<Questions>>(jsonString);
                var jsonmarks = trainee.Marks;
                var marks = JsonConvert.DeserializeObject<List<string>>(jsonmarks);

                for (int i = 0; i < questions.Count && i < marks.Count; i++)
                {
                    var rm = new CandiatesResultModel(); // Create a new instance inside the loop

                    var selectedCategoryOption = questions[i].CategoryId;
                    rm.category = ((CategoryEnum)selectedCategoryOption).ToString();
                    rm.Qeations = questions[i].Question;
                    rm.Marks = marks[i];

                    model.resultModel.Add(rm);
                }
            }
            await PrepareresultListAsync(model);

            return model;
        }


        public virtual async Task<CandiatesResumesModel> PrepareCandiatesResultModelAsync(CandiatesResumesModel model, CandidatesResult candiatesResumes, bool excludeProperties = false)
        {

            if (candiatesResumes != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = candiatesResumes.ToModel<CandiatesResumesModel>();

                }

            }

            return model;
        }
        public virtual async Task<CandiatesResumesModel> PrepareResultModelAsync(int Id)
        {
            var traineeList = await _candiatesResumesService.GetByConditionIdAsync(Id);
            var modell = new CandiatesResumesModel(); // Create a list to hold the models

            foreach (var trainee in traineeList)
            {
                var model = new CandiatesResultModel();

                // Populate the model with trainee data
                model.Id = trainee.Id;
                model.CandidateId = trainee.CandidateId;
                model.Communication = trainee.Communication;
                model.ConfidentLevel = trainee.ConfidentLevel;
                model.ResultStatusId = trainee.ResultStatusId;
                modell.resultModel.Add(model);
            }
            return modell;
        }
        #endregion
    }
}


