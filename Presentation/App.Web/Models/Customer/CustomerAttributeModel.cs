using System.Collections.Generic;
using App.Core.Domain.Catalog;
using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record CustomerAttributeModel : BaseNopEntityModel
    {
        public CustomerAttributeModel()
        {
            Values = new List<CustomerAttributeValueModel>();
        }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Default value for textboxes
        /// </summary>
        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<CustomerAttributeValueModel> Values { get; set; }

    }

    public partial record CustomerAttributeValueModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}