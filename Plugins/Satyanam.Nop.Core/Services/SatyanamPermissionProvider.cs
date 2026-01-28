using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Services.Security;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// AccountType service
    /// </summary>
    public partial class SatyanamPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageLeads = new() { Name = "Admin area. Manage Leads", SystemName = "ManageLeads", Category = "CRM" };
        public static readonly PermissionRecord ManageCampaigns = new() { Name = "Admin area. Manage Campaigns", SystemName = "ManageCampaigns", Category = "CRM" };
        public static readonly PermissionRecord ManageContacts = new() { Name = "Admin area. Manage Contacts", SystemName = "ManageContacts", Category = "CRM" };
        public static readonly PermissionRecord ManageContactDeals = new() { Name = "Admin area. Manage Contact Deals", SystemName = "ManageContactDeals", Category = "CRM" };
        public static readonly PermissionRecord ManageTags = new() { Name = "Admin area. Manage Tags", SystemName = "ManageTags", Category = "CRM" };
        public static readonly PermissionRecord ManageLeadStatuses = new() { Name = "Admin area. Manage Lead Statuses", SystemName = "ManageLeadStatuses", Category = "CRM" };
        public static readonly PermissionRecord ManageLeadSources = new() { Name = "Admin area. Manage Lead Sources", SystemName = "ManageLeadSources", Category = "CRM" };
        public static readonly PermissionRecord ManageIndustries = new() { Name = "Admin area. Manage Industries", SystemName = "ManageIndustries", Category = "CRM" };
        public static readonly PermissionRecord ManageCategories = new() { Name = "Admin area. Manage Categories", SystemName = "ManageCategories", Category = "CRM" };
        public static readonly PermissionRecord ManageAccountTypes = new() { Name = "Admin area. Manage Account Types", SystemName = "ManageAccountTypes", Category = "CRM" };
        public static readonly PermissionRecord ManageCompanies = new() { Name = "Admin area. Manage Companies", SystemName = "ManageCompanies", Category = "CRM" };
        public static readonly PermissionRecord ManageCompanyContacts = new() { Name = "Admin area. Manage Company Contacts", SystemName = "ManageCompanyContacts", Category = "CRM" };
        public static readonly PermissionRecord ManageCompanyDeals = new() { Name = "Admin area. Manage Company Deals", SystemName = "ManageCompanyDeals", Category = "CRM" };
        public static readonly PermissionRecord ManageDeals = new() { Name = "Admin area. Manage Deals", SystemName = "ManageDeals", Category = "CRM" };
        public static readonly PermissionRecord ManageDealContacts = new() { Name = "Admin area. Manage Deal Contacts", SystemName = "ManageDealContacts", Category = "CRM" };
        public static readonly PermissionRecord ManageDealCompanies = new() { Name = "Admin area. Manage Deal Companies", SystemName = "ManageDealCompanies", Category = "CRM" };
        public static readonly PermissionRecord ManageTitles = new() { Name = "Admin area. Manage Titles", SystemName = "ManageTitles", Category = "CRM" };
        public static readonly PermissionRecord ManageInquiries = new() { Name = "Admin area. Manage Inquiry", SystemName = "ManageInquiries", Category = "CRM" };
        public static readonly PermissionRecord ManageUpdateTemplate = new() { Name = "Admin area. Manage UpdateTemplate", SystemName = "ManageUpdateTemplate", Category = "Standard" };
        public static readonly PermissionRecord ManageUpdateQuestionTemplate = new() { Name = "Admin area. Manage UpdateQuestionTemplate", SystemName = "ManageUpdateQuestionTemplate", Category = "Standard" };
        public static readonly PermissionRecord ManageUpdateQuestionOptionTemplate = new() { Name = "Admin area. Manage UpdateQuestionOptionTemplate", SystemName = "ManageUpdateQuestionOptionTemplate", Category = "Standard" };


        /// <summary>
        /// Get permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageLeads,
                ManageCampaigns,
                ManageContacts,
                ManageContactDeals,
                ManageTags,
                ManageLeadStatuses,
                ManageLeadSources,
                ManageIndustries,
                ManageCategories,
                ManageAccountTypes,
                ManageCompanies,
                ManageCompanyContacts,
                ManageCompanyDeals,
                ManageDeals,
                ManageDealContacts,
                ManageDealCompanies,
                ManageTitles,
                ManageUpdateTemplate,
                ManageUpdateQuestionTemplate,
                ManageUpdateQuestionOptionTemplate,
                ManageInquiries
            };
        }

        /// <summary>
        /// Get default permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageLeads,
                        ManageCampaigns,
                        ManageContacts,
                        ManageContactDeals,
                        ManageTags,
                        ManageLeadStatuses,
                        ManageLeadSources,
                        ManageIndustries,
                        ManageCategories,
                        ManageAccountTypes,
                        ManageCompanies,
                        ManageCompanyContacts,
                        ManageCompanyDeals,
                        ManageDeals,
                        ManageDealContacts,
                        ManageDealCompanies,
                        ManageTitles,
                        ManageInquiries

                    }
                ),

            };
        }
    }
}