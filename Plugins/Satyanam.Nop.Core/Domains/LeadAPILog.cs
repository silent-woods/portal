using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    public  class LeadAPILog : BaseEntity
    {
        #region Properties

        public string EndPoint { get; set; }

        public string RequestJson { get; set; }

        public string ResponseJson { get; set; }

        public string ResponseMessage { get; set; }

        public bool Success { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        #endregion
    }
}