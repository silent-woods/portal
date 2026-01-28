using System.Collections.Generic;
using App.Core.Domain.Catalog;
using App.Web.Framework.Models;

namespace App.Web.Models.Common
{
    public partial record AddressAttributeModel : BaseNopEntityModel
    {
        public AddressAttributeModel()
        {
            Values = new List<AddressAttributeValueModel>();
        }

        public string ControlId { get; set; }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Default value for textboxes
        /// </summary>
        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<AddressAttributeValueModel> Values { get; set; }
    }

    public partial record AddressAttributeValueModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}