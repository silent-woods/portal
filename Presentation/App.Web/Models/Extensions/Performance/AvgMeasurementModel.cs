using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a AvgMeasurement model
    /// </summary>
    public partial record AvgMeasurementModel : BaseNopEntityModel
    {
        public AvgMeasurementModel()
        {
            employeeRatePerKPIs = new List<EmployeeRatePerKPI>();
            employeeTotalAvgRates = new List<EmployeeTotalAvgRate>();
            performanceMeasurementModel = new List<PerformanceMeasurementModel>();
            Months = new List<SelectListItem>();
            KPIName = new List<string>();
            Feedbacks = new List<string>();
        }

        #region Properties
        public IList<SelectListItem> Months { get; set; }
        public List<string> KPIName { get; set; }

        public List<string> Feedbacks { get; set; }

        public int ShowFeedbackId { get; set; }

        public List<EmployeeRatePerKPI> employeeRatePerKPIs { get; set; }

        public List<EmployeeTotalAvgRate> employeeTotalAvgRates { get; set; }
        public List<PerformanceMeasurementModel> performanceMeasurementModel { get; set; }
 
        #endregion
    }
    public class EmployeeRatePerKPI
    {
        public string KPIName { get; set; }
        public int Rate { get; set; }
        public int MonthId { get; set; }
        public string ManagerName { get; set; }
        public string EmployeeName { get; set; }
        

    }
    public class EmployeeTotalAvgRate
    {
        public string KPIName { get; set; }
        public double TotalRating { get; set; }
        public double AvgRateing { get; set; }
        public double totalaveragae { get; set; }
        public string totalaverage { get; set; }
        public int MonthId { get; set; }
    }
}