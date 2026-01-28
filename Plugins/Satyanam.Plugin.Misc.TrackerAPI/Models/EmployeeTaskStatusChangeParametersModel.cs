using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeTaskStatusChangeParametersModel : TrackerAPIResponseModel
{
    #region Ctor

    public EmployeeTaskStatusChangeParametersModel()
    {
        AvailableTaskChecklistItems = new List<EmployeeTaskChecklistItemParametersModel>();
    }

    #endregion

    #region Properties

    [JsonProperty("task_id")]
	public int TaskId { get; set; }

    [JsonProperty("status_id")]
    public int StatusId { get; set; }

    [JsonProperty("comments")]
    public string Comments { get; set; }

    [JsonProperty("checklists")]
    public IList<EmployeeTaskChecklistItemParametersModel> AvailableTaskChecklistItems { get; set; }

	#endregion
}
