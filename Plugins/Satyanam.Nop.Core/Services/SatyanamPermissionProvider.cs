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
        public static readonly PermissionRecord ManageLeads = new() { Name = "Admin area. CRM - Manage Leads", SystemName = "ManageLeads", Category = "CRM" };
        public static readonly PermissionRecord ManageContacts = new() { Name = "Admin area. CRM - Manage Contacts", SystemName = "ManageContacts", Category = "CRM" };
        public static readonly PermissionRecord ManageContactDeals = new() { Name = "Admin area. CRM -  Manage Contact Deals", SystemName = "ManageContactDeals", Category = "CRM" };
        public static readonly PermissionRecord ManageCompanies = new() { Name = "Admin area. CRM - Manage Companies", SystemName = "ManageCompanies", Category = "CRM" };
        public static readonly PermissionRecord ManageCompanyContacts = new() { Name = "Admin area. CRM - Manage Company Contacts", SystemName = "ManageCompanyContacts", Category = "CRM" };
        public static readonly PermissionRecord ManageCompanyDeals = new() { Name = "Admin area. CRM - Manage Company Deals", SystemName = "ManageCompanyDeals", Category = "CRM" };
        public static readonly PermissionRecord ManageDeals = new() { Name = "Admin area. CRM - Manage Deals", SystemName = "ManageDeals", Category = "CRM" };
        public static readonly PermissionRecord ManageDealContacts = new() { Name = "Admin area. CRM - Manage Deal Contacts", SystemName = "ManageDealContacts", Category = "CRM" };
        public static readonly PermissionRecord ManageDealCompanies = new() { Name = "Admin area. CRM - Manage Deal Companies", SystemName = "ManageDealCompanies", Category = "CRM" };
        public static readonly PermissionRecord ManageCampaigns = new() { Name = "Admin area. CRM - Manage Campaigns", SystemName = "ManageCampaigns", Category = "CRM" };
        public static readonly PermissionRecord ManageIndustries = new() { Name = "Admin area. CRM - Manage Industries", SystemName = "ManageIndustries", Category = "CRM" };
        public static readonly PermissionRecord ManageLeadSources = new() { Name = "Admin area. CRM - Manage Lead Sources", SystemName = "ManageLeadSources", Category = "CRM" };
        public static readonly PermissionRecord ManageLeadStatuses = new() { Name = "Admin area. CRM - Manage Lead Statuses", SystemName = "ManageLeadStatuses", Category = "CRM" };
        public static readonly PermissionRecord ManageTitles = new() { Name = "Admin area. CRM - Manage Titles", SystemName = "ManageTitles", Category = "CRM" };
        public static readonly PermissionRecord ManageCategories = new() { Name = "Admin area. CRM - Manage Categories", SystemName = "ManageCategories", Category = "CRM" };
        public static readonly PermissionRecord ManageTags = new() { Name = "Admin area. CRM - Manage Tags", SystemName = "ManageTags", Category = "CRM" };
        public static readonly PermissionRecord ManageAccountTypes = new() { Name = "Admin area. CRM - Manage Account Types", SystemName = "ManageAccountTypes", Category = "CRM" };
        public static readonly PermissionRecord ManageInquiries = new() { Name = "Admin area. CRM - Manage Inquiry", SystemName = "ManageInquiries", Category = "CRM" };
        public static readonly PermissionRecord ManageUpdateTemplate = new() { Name = "Admin area. Update Template - Manage UpdateTemplate", SystemName = "ManageUpdateTemplate", Category = "Standard" };
        public static readonly PermissionRecord ManageUpdateQuestionTemplate = new() { Name = "Admin area. Update Template - Manage UpdateQuestionTemplate", SystemName = "ManageUpdateQuestionTemplate", Category = "Standard" };
        public static readonly PermissionRecord ManageUpdateQuestionOptionTemplate = new() { Name = "Admin area. Update Template - Manage UpdateQuestionOptionTemplate", SystemName = "ManageUpdateQuestionOptionTemplate", Category = "Standard" };


        /// <summary>
        /// Get permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageLeads,
                ManageContacts,
                ManageContactDeals,
                ManageCompanies,
                ManageCompanyContacts,
                ManageCompanyDeals,
                ManageDeals,
                ManageDealContacts,
                ManageDealCompanies,
                ManageCampaigns,
                ManageIndustries,
                ManageLeadSources,
                ManageLeadStatuses,
                ManageTitles,
                ManageCategories,
                ManageTags,
                ManageAccountTypes,
                ManageInquiries,
                ManageUpdateTemplate,
                ManageUpdateQuestionTemplate,
                ManageUpdateQuestionOptionTemplate
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
                        ManageContacts,
                        ManageContactDeals,
                        ManageCompanies,
                        ManageCompanyContacts,
                        ManageCompanyDeals,
                        ManageDeals,
                        ManageDealContacts,
                        ManageDealCompanies,
                        ManageCampaigns,
                        ManageIndustries,
                        ManageLeadSources,
                        ManageLeadStatuses,
                        ManageTitles,
                        ManageCategories,
                        ManageTags,
                        ManageAccountTypes,
                        ManageInquiries,
                        ManageUpdateTemplate,
                        ManageUpdateQuestionTemplate,
                        ManageUpdateQuestionOptionTemplate

                    }
                ),

            };
        }
    }
}