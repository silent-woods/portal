using App.Core.Domain.Leaves;
using App.Data;
using App.Services.Leaves;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Leavetypes;
using App.Web.Framework.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using App.Web.Framework.Models.Extensions;
using App.Data.Extensions;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the leaveType model factory implementation
    /// </summary>
    public partial class LeaveTypeModelFactory : ILeaveTypeModelFactory
    {
        #region Fields

        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor

        public LeaveTypeModelFactory(ILeaveTypeService leaveTypeService,
            IDateTimeHelper dateTimeHelper)
        {
            _leaveTypeService = leaveTypeService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual async Task<LeaveTypeSearchModel> PrepareLeaveTypeSearchModelAsync(LeaveTypeSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<LeaveTypeListModel> PrepareLeaveTypeListModelAsync(LeaveTypeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get leaveType
            var leaveType = await _leaveTypeService.GetAllLeaveTypeAsync(leaveName: searchModel.SearchLeavetypesName,
                showHidden: true,

                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new LeaveTypeListModel().PrepareToGridAsync(searchModel, leaveType, () =>
            {
                return leaveType.SelectAwait(async leaveType =>
                {
                    //fill in model values from the entity
                    var leaveTypeModel = new LeaveTypeModel();
                    leaveTypeModel.Id = leaveType.Id;
                    leaveTypeModel.Type = leaveType.Type;
                    leaveTypeModel.Description = leaveType.Description;
                    leaveTypeModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(leaveType.CreateOnUtc, DateTimeKind.Utc);
                    leaveTypeModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(leaveType.UpdateOnUtc, DateTimeKind.Utc);
                    leaveTypeModel.Total_Allowed = leaveType.Total_Allowed;

                    return leaveTypeModel;
                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<LeaveTypeModel> PrepareLeaveTypeModelAsync(LeaveTypeModel model, Leave leaveType, bool excludeProperties = false)
        {
            if (leaveType != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = leaveType.ToModel<LeaveTypeModel>();
                }
            }
            return model;
        }
        #endregion
    }
}
