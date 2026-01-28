using App.Core.Domain.Security;
using App.Data.Extensions;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc.Filters;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Inquiries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class InquiryController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IInquiryService _inquiryService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public InquiryController(IPermissionService permissionService,
                                 IInquiryService inquiryService,
                                 INotificationService notificationService,
                                 ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _inquiryService = inquiryService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        protected virtual async Task<InquirySearchModel> PrepareInquirySearchModelAsync(InquirySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableSources = Enum.GetValues(typeof(InquirySourceType))
      .Cast<InquirySourceType>()
      .Select(e => new SelectListItem
      {
          Text = e.ToString(),
          Value = ((int)e).ToString()
      })
      .ToList();
        searchModel.AvailableSources.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual async Task<InquiryListModel> PrepareInquiryListModelAsync(InquirySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var inquiries = await _inquiryService.GetAllInquiryAsync(
      name: searchModel.SearchName,
      email: searchModel.SearchEmail,
      contactNo: searchModel.SearchContactNo,
      company:searchModel.SearchCompany,
      sourceId: searchModel.SearchSourceId > 0 ? searchModel.SearchSourceId : (int?)null,
      pageIndex: searchModel.Page - 1,
      pageSize: searchModel.PageSize);

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var model = await new InquiryListModel().PrepareToGridAsync(searchModel, inquiries, () =>
            {
                return inquiries.SelectAwait(async i =>
                {
                    var m = new InquiryModel
                    { 
                        Id = i.Id,
                        Source = ((InquirySourceType)i.SourceId).ToString(),
                        Email = i.Email,
                        Describe = i.Describe,
                        CreatedOnUtc = TimeZoneInfo.ConvertTimeFromUtc(i.CreatedOnUtc, istTimeZone),
                        FirstName = i.FirstName,
                        LastName = i.LastName,
                        ContactNo = i.ContactNo,
                        Company = i.Company,
                        WantToHire = i.WantToHire,
                        ProjectType = i.ProjectType,
                        EngagementDuration = i.EngagementDuration,
                        TimeCommitment = i.TimeCommitment,
                        Budget = i.Budget                      
                    };
                    return m;
                });
            });

            return model;
        }

        protected virtual async Task<InquiryModel> PrepareInquiryModelAsync(InquiryModel model, Inquiry inquiry, bool excludeProperties = false)
        {
            if (inquiry != null)
            {
                if (model == null)
                    model = new InquiryModel();

                model.Id = inquiry.Id;
                model.FirstName = inquiry.FirstName;
                model.LastName = inquiry.LastName;
                model.Email = inquiry.Email;
                model.ContactNo = inquiry.ContactNo;
                model.Company = inquiry.Company;
                model.WantToHire = inquiry.WantToHire;
                model.ProjectType = inquiry.ProjectType;
                model.EngagementDuration = inquiry.EngagementDuration;
                model.TimeCommitment = inquiry.TimeCommitment;
                model.Budget = inquiry.Budget;
                model.Describe = inquiry.Describe;
                model.SourceId = inquiry.SourceId;
                model.CreatedOnUtc = inquiry.CreatedOnUtc;
            }

            return model;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageInquiries, PermissionAction.View))
                return AccessDeniedView();

            var model = await PrepareInquirySearchModelAsync(new InquirySearchModel());
            return View("~/Plugins/Misc.SatyanamCRM/Views/Inquiries/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(InquirySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageInquiries, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            var model = await PrepareInquiryListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageInquiries, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var inquiries = await _inquiryService.GetInquiriesByIdsAsync(selectedIds.ToArray());
            foreach (var inquiry in inquiries)
                await _inquiryService.DeleteInquiryAsync(inquiry);

            return Json(new { Result = true });
        }

        #endregion
    }
}
