using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.TimeSheets
{
    public enum SearchPeriodEnum
    {
        [Display(Name = "Today")]
        Today = 1,

        [Display(Name = "Yesterday")]
        Yesterday = 2,

        [Display(Name = "Current Week")]
        CurrentWeek = 3,

        [Display(Name = "Last Week")]
        LastWeek = 4,

        [Display(Name = "Current Month")]
        CurrentMonth = 5,

        [Display(Name = "Last Month")]
        LastMonth = 6,

        [Display(Name = "Current Year")]
        CurrentYear = 7,

        [Display(Name = "Last Year")]
        LastYear = 8,

        [Display(Name = "Custom Range")]
        CustomRange = 9
    }
   
}
