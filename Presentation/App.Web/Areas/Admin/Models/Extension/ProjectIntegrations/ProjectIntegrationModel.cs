using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.ProjectIntegrations;

public partial record ProjectIntegrationModel : BaseNopEntityModel
{
    #region Ctor

    public ProjectIntegrationModel()
    {
        SelectedProjectIds = new List<int>();
        AvailableProjects = new List<SelectListItem>();
        ProjectIntegrationMappingsSearchModel = new ProjectIntegrationMappingsSearchModel();
    }

    #endregion

    #region Project Integration Properties

    [NopResourceDisplayName("Admin.System.ProjectIntegration.Fields.IntegrationName")]
    [Required(ErrorMessage = "Please Enter an integration name")]
    public string IntegrationName { get; set; }

    [NopResourceDisplayName("Admin.System.ProjectIntegration.Fields.SystemName")]
    [Required(ErrorMessage = "Please Enter a system name")]
    public string SystemName { get; set; }

    [NopResourceDisplayName("Admin.System.ProjectIntegration.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Admin.System.ProjectIntegration.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    #endregion

    #region Project Integration Mapping Properties

    [NopResourceDisplayName("Admin.System.ProjectIntegration.ProjectIntegrationMappings.Fields.SelectedProjectIds")]
    public IList<int> SelectedProjectIds { get; set; }

    public IList<SelectListItem> AvailableProjects { get; set; }

    public ProjectIntegrationMappingsSearchModel ProjectIntegrationMappingsSearchModel { get; set; }

    #endregion
}
