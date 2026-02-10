using App.Core;
using App.Core.Domain.Security;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Plugins;
using App.Services.Security;
using App.Web.Framework.Menu;
using Microsoft.AspNetCore.Routing;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Core.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM
{
    public class SatyanamCRMPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public SatyanamCRMPlugin(IPermissionService permissionService,
            IWebHelper webHelper,
            ISettingService settingService)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
            _settingService = settingService;
        }

        #endregion

        #region Plugin Configuration Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/SatyanamCRM/Configure";
        }

        #endregion

        #region Plugin Install/Uninstall Methods

        public override async Task InstallAsync()
        {
            var settings = await _settingService.LoadSettingAsync<CampaignEmailSettings>();

            if (string.IsNullOrEmpty(settings.LinkedInUrl)) // Ensure only saving if not set
            {
                settings.LinkedInUrl = "https://linkedin.com/company/satyanam";
                settings.WebsiteUrl = "https://satyanamsoft.com";
                settings.FooterText = @"Copyright © 2025 Satyanam Info Solution Pvt. Ltd, All rights reserved.<br>
                                      You are receiving this email because you opted in via our website.< br >
                Want to change how you receive these emails ?< br >
                              You can < a href = '{unsubscribeLink}' style = 'color: #ff4c4c; text-decoration: none;' target = '_blank' > unsubscribe from this list </ a >.";


                await _settingService.SaveSettingAsync(settings);
            }
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //await base.UninstallAsync();
        }

        #endregion

        #region Manage SiteMap Methods

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = "CRM",
                Visible = await Authenticate(),
                IconClass = "fas fa-users crm-icon"
            };

            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Lead",
                ActionName = "List",
                ControllerName = "Lead",
                SystemName = "Lead",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-user-plus" // Lead icon
            });

            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Contacts",
                ActionName = "List",
                ControllerName = "Contacts",
                SystemName = "Contacts",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-address-book" // Contacts icon
            });

            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Company",
                ActionName = "List",
                ControllerName = "Company",
                SystemName = "Company",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-building" // Company icon
            });

            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Deals",
                ActionName = "List",
                ControllerName = "Deals",
                SystemName = "Deals",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-handshake" // Deals icon
            });

            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Campaigns",
                ActionName = "List",
                ControllerName = "Campaings",
                SystemName = "Campaings",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-bullhorn"
            });
            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                Title = "LinkedIn Follow-ups",
                ActionName = "List",
                ControllerName = "LinkedInFollowups",
                SystemName = "LinkedInFollowups",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLinkedInFollowups, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-user-tie"
            });
            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Connection Request",
                ActionName = "List",
                ControllerName = "ConnectionRequest",
                SystemName = "ConnectionRequest",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-user-tie"
            });
            menuItem.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Inquiry",
                ActionName = "List",
                ControllerName = "Inquiry",
                SystemName = "Inquiries",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageInquiries, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-envelope-open-text"
            });

            var mastersMenu = new SiteMapNode()
            {
                Title = "Masters",
                Visible = await AuthenticateMasters(),
                IconClass = "nav-icon far fa-circle"
            };

            mastersMenu.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Industry",
                ActionName = "List",
                ControllerName = "Industry",
                SystemName = "Industrys",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-industry"
            });

            mastersMenu.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Lead Source",
                ActionName = "List",
                ControllerName = "LeadSource",
                SystemName = "LeadSources",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-share-alt"
            });

            mastersMenu.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Lead Status",
                ActionName = "List",
                ControllerName = "LeadStatus",
                SystemName = "LeadStatus",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadStatuses, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-tasks"
            });

            mastersMenu.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Title",
                ActionName = "List",
                ControllerName = "Title",
                SystemName = "Titles",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-id-badge"
            });

            mastersMenu.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Categories",
                ActionName = "List",
                ControllerName = "Categorys",
                SystemName = "Categorys",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-tags"
            });

            mastersMenu.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Tags",
                ActionName = "List",
                ControllerName = "Tags",
                SystemName = "Tags",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-tag"
            });

            mastersMenu.ChildNodes.Add(new SiteMapNode()
            {
                Title = "Account Type",
                ActionName = "List",
                ControllerName = "AccountType",
                SystemName = "AccountType",
                Visible = await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.View),
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                IconClass = "fas fa-user-cog"
            });

            menuItem.ChildNodes.Add(mastersMenu);

            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.Title == "CRM");
            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);
        }

        public async Task<bool> Authenticate()
        {
            bool flag = false;
            if (await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageInquiries, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLinkedInFollowups, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadStatuses, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.View))
                flag = true;
            return flag;
        }

        public async Task<bool> AuthenticateMasters()
        {
            bool flag = false;
            if (await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadStatuses, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.View) ||
                await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.View))
                flag = true;
            return flag;
        }

        #endregion
    }
}
