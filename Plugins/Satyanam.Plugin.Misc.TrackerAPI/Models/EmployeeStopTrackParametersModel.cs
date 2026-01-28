using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeStopTrackParametersModel : TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("keyboard_hit")]
    public int KeyboardHits { get; set; }

    [JsonProperty("mouse_hit")]
    public int MouseHits { get; set; }

    #endregion
}
