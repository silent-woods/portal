using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
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
            teamPerformanceMeasurementModel = new List<TeamPerformanceMeasurementModel>();
            Months = new List<SelectListItem>();
            KPIName = new List<string>();
            Feedbacks = new List<string>();

        }

        #region Properties
        public IList<SelectListItem> Months { get; set; }
        public List<string> KPIName { get; set; }

        public List<string> Feedbacks { get; set; }

        public List<EmployeeRatePerKPI> employeeRatePerKPIs { get; set; }

        public List<EmployeeTotalAvgRate> employeeTotalAvgRates { get; set; }
        public List<TeamPerformanceMeasurementModel> teamPerformanceMeasurementModel { get; set; }
 
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
        public int TotalRateing { get; set; }
        public double AvgRateing { get; set; }
        public double totalaveragae { get; set; }
        public int MonthId { get; set; }
    }
}