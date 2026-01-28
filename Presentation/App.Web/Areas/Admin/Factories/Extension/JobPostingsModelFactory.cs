using App.Core.Domain.JobPostings;
using App.Data.Extensions;
using App.Services;
using App.Services.Helpers;
using App.Services.JobPostings;
using App.Services.Localization;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.JobPostings;
using App.Web.Areas.Admin.Models.JobPostings;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the timesheet model factory implementation
    /// </summary>
    public partial class JobPostingsModelFactory : IJobPostingsModelFactory
    {
        #region Fields

        private readonly IJobPostingService _jobPostingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        //private readonly IEmployeeService _employeeService;
        //private readonly ITimeSheetsService _timeSheetsService;
        #endregion

        #region Ctor

        public JobPostingsModelFactory(IJobPostingService jobPostingService,
            IDateTimeHelper dateTimeHelper
,
            ILocalizationService localizationService
            //IEmployeeService employeeService,
            //ITimeSheetsService timeSheetsService
            )
        {
            _jobPostingService = jobPostingService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            //_employeeService = employeeService;
            //_timeSheetsService = timeSheetsService;
        }

        #endregion
        #region Utilities
        #endregion

        #region Methods

        public virtual async Task<JobPostingSearchModel> PrepareJobPostingsSearchModelAsync(JobPostingSearchModel searchModel)
        {
            var questiontype = await JobPositionEnum.Select.ToSelectListAsync();
            searchModel.AvailablePosition = questiontype.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.PositionId.ToString() == store.Value
            }).ToList();
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<JobPostingListModel> PrepareJobPostingsListModelAsync
          (JobPostingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get timesheet
            var timeSheet = await _jobPostingService.GetAllJobPostingAsync(tittle: searchModel.Title, positionid: searchModel.PositionId,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new JobPostingListModel().PrepareToGridAsync(searchModel, timeSheet, () =>
            {
                return timeSheet.SelectAwait(async timesheet =>
                {
                    //fill in model values from the entity
                    var selectedAvailablePosition = timesheet.PositionId;
                    var model = timesheet.ToModel<JobPostingModel>();
                    model.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.CreatedOn, DateTimeKind.Utc);
                    model.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.UpdatedOn, DateTimeKind.Utc);
                    model.Position = ((JobPositionEnum)selectedAvailablePosition).ToString();
                    return model;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<JobPostingModel> PrepareJobPostingsModelAsync(JobPostingModel model, JobPosting jobPosting, bool excludeProperties = false)
        {
            var questiontype = await JobPositionEnum.Select.ToSelectListAsync();
            if (jobPosting != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = jobPosting.ToModel<JobPostingModel>();

                }
            }
            model.AvailablePosition = questiontype.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.PositionId.ToString() == store.Value
            }).ToList();

            //model.AvailablePosition.Insert(0, new SelectListItem
            //{
            //    Text = await _localizationService.GetResourceAsync("Admin.Extension.Jobposting.Fields.Position.select"),
            //    Value = "0"
            //});

            return model;
        }
        #endregion
    }
}


