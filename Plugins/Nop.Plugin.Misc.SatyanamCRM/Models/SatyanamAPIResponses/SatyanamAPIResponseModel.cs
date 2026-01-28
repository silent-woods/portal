using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.SatyanamAPIResponses
{
    public partial class SatyanamAPIResponseModel
    {
        #region Properties

        [JsonProperty("response_message")]
        public string ResponseMessage { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        #endregion
    }
}