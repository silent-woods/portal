using App.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.Employees
{
    public partial class EmployeeSettings : ISettings
    {
        public int ActiveStoreScopeConfiguration { get; set; }
        public string OnBoardingEmail { get; set; }
        public int CoordinatorRoleId { get; set; }
    }
}
