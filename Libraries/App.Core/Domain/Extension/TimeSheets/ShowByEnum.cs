using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.TimeSheets
{
    public enum ShowByEnum
    { 
       
        Daily = 1,
        Weekly = 2,
        Monthly = 3
      

    }

    public enum HoursEnum
    {

        TotalHours = 1,
        BillableHours = 2,

        RndHours = 3


    }
}
