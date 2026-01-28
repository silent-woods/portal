using System;
using System.Collections.Generic;
using App.Core.Domain.Customers;
using App.Core.Domain.Forums;
using App.Core.Domain.News;
using App.Core.Domain.Security;

namespace App.Data.Mapping
{
    /// <summary>
    /// Base instance of backward compatibility of table naming
    /// </summary>
    public partial class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(CustomerAddressMapping), "CustomerAddresses" },
            { typeof(CustomerCustomerRoleMapping), "Customer_CustomerRole_Mapping" },
            { typeof(PermissionRecordCustomerRoleMapping), "PermissionRecord_Role_Mapping" },
            { typeof(PermissionRecordUserMapping), "PermissionRecord_User_Mapping" },
            { typeof(ForumGroup), "Forums_Group" },
            { typeof(Forum), "Forums_Forum" },
            { typeof(ForumPost), "Forums_Post" },
            { typeof(ForumPostVote), "Forums_PostVote" },
            { typeof(ForumSubscription), "Forums_Subscription" },
            { typeof(ForumTopic), "Forums_Topic" },
            { typeof(PrivateMessage), "Forums_PrivateMessage" },
            { typeof(NewsItem), "News" }
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
            { (typeof(Customer), "BillingAddressId"), "BillingAddress_Id" },
            { (typeof(Customer), "ShippingAddressId"), "ShippingAddress_Id" },
            { (typeof(CustomerCustomerRoleMapping), "CustomerId"), "Customer_Id" },
            { (typeof(CustomerCustomerRoleMapping), "CustomerRoleId"), "CustomerRole_Id" },
            { (typeof(PermissionRecordCustomerRoleMapping), "PermissionRecordId"), "PermissionRecord_Id" },
            { (typeof(PermissionRecordCustomerRoleMapping), "CustomerRoleId"), "CustomerRole_Id" },
            { (typeof(CustomerAddressMapping), "AddressId"), "Address_Id" },
            { (typeof(CustomerAddressMapping), "CustomerId"), "Customer_Id" },
        };
    }
}