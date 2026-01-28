using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentMigrator.Infrastructure;

namespace App.Web.Areas.Admin.Models.LeaveManagement
{
    /// <summary>
    /// Represents a LeaveManagement model
    /// </summary>
    public partial record LeaveTransactionLogModel : BaseNopEntityModel
    {
       public  LeaveTransactionLogModel()
        {
            Months = new List<SelectListItem>();
            Years = new List<SelectListItem>();

            AvailableEmployees = new List<SelectListItem>();
            AvailableLeaveTypes= new List<SelectListItem>();

        }
        #region Properties


        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.EmployeeId")]
        [Required(ErrorMessage = "Please select employee")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select employee")]
        public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.EmployeeId")]

        public string EmployeeName { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }


        public int LeaveId { get; set; }
    
        public int StatusId { get; set; }

        public string StatusName { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.LeaveTypeId")]

        [Required(ErrorMessage = "Please select leave type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select leave type")]
        public int ApprovedId { get; set; }


        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.LeaveTypeId")]

        public string LeaveTypeName { get; set; }


        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.LeaveBalance")]

        public decimal LeaveBalance { get; set; }


        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.BalanceChange")]

        public decimal BalanceChange { get; set; }

        public string BalanceChangeString { get; set; }


        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.Comment")]
        [Required(ErrorMessage = "Please enter comment")]

        public string Comment { get; set; }
        public DateTime BalanceMonthYear { get; set; }
        public string BalanceMonthYearString { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.Year")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Year")]

        public int Year { get; set; }


        [NopResourceDisplayName("Admin.Extension.LeaveTransaction.AddLeaveMonthly.MonthId")]

        public string YearMonth { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.IsEdited")]

        public bool IsEdited { get; set; }


        public decimal ManualBalanceChange { get; set; }


        public DateTime CreatedOnUTC { get; set; }

        public IList<SelectListItem> Months { get; set; }

        public IList<SelectListItem> Years { get; set; }


        public IList<SelectListItem> AvailableLeaveTypes { get; set; }


        //[Range(1, int.MaxValue, ErrorMessage = "Please select month")]

        [NopResourceDisplayName("Admin.Extension.LeaveTransaction.AddLeaveMonthly.MonthId")]


        public int MonthId { get; set; }

        #endregion
    }
}