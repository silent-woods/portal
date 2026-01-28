using App.Core.Domain.WeeklyQuestion;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Areas.Admin.Models.WeeklyQuestions;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.WeeklyReports
{
    /// <summary>
    /// Represents a Project model
    /// </summary>
    public partial record WeeklyReportModel : BaseNopEntityModel
    {
        public WeeklyReportModel()
        {
            WeeklyQuestions = new List<WeeklyQuestionsModel>();
            Employees = new List<SelectListItem>();
            reportdata = new List<WeeklyQuestionData>();
        }

        #region Properties
        public Dictionary<DateTime, List<WeeklyQuestionData>> DateWiseReports { get; set; } = new Dictionary<DateTime, List<WeeklyQuestionData>>();
        public IList<SelectListItem> Employees { get; set; }

        [NopResourceDisplayName("Admin.Projects.Fields.CreateOn")]
        public int EmployeeId { get; set; }
        [NopResourceDisplayName("Admin.Projects.Fields.CreateOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Projects.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }
        public IList<WeeklyQuestionsModel> WeeklyQuestions { get; set; }
        public string[] ControlValue { get; set; }
        public string EmployeeName { get; set; }
        public IList<WeeklyQuestionData> reportdata { get; set; }
        public string rdata { get; set; }
        #endregion
    }
    public class WeeklyQuestionData
    {
        public string ControlValue { get; set; }
        public string QuestionText { get; set; }
        public DateTime Date { get; set; }
        // Additional properties if any...
    }
    public class WeeklyQuestionList
    {

        public WeeklyQuestionList()
        {
            weeklyQuestions = new List<WeeklyQuestionData>();
        }
        public List<WeeklyQuestionData> weeklyQuestions { get; set; }
    }

}