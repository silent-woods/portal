using App.Core.Domain.Customers;
using App.Services.Customers;
using App.Services.Localization;
using App.Services.Security;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Areas.Admin.Models.Security;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the security model factory implementation
    /// </summary>
    public partial class SecurityModelFactory : ISecurityModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public SecurityModelFactory(ICustomerService customerService,
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare permission mapping model
        /// </summary>
        /// <param name="model">Permission mapping model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the permission mapping model
        /// </returns>
        public virtual async Task<PermissionMappingModel> PreparePermissionMappingModelAsync(PermissionMappingModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // Load roles from DB
            var customerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            model.AvailableCustomerRoles = customerRoles.Select(r => r.ToModel<CustomerRoleModel>()).ToList();

            // Load all permission records from DB
            var permissionRecords = await _permissionService.GetAllPermissionRecordsAsync();

            // Define dynamic actions
            model.AvailableActions = new[] { "Full", "View", "Add", "Edit", "Delete" };

            foreach (var pr in permissionRecords)
            {
                var permissionKey = pr.SystemName.Replace(".", "_");

                model.AvailablePermissions.Add(new PermissionRecordModel
                {
                    Name = await _localizationService.GetLocalizedPermissionNameAsync(pr),
                    SystemName = pr.SystemName
                });

                foreach (var role in customerRoles)
                {
                    // Existing ACL mapping
                    if (!model.Allowed.ContainsKey(pr.SystemName))
                        model.Allowed[pr.SystemName] = new Dictionary<int, bool>();

                    model.Allowed[pr.SystemName][role.Id] =
                        (await _permissionService.GetMappingByPermissionRecordIdAsync(pr.Id))
                        .Any(x => x.CustomerRoleId == role.Id);

                    // Initialize dynamic action permissions
                    if (!model.ActionPermissions.ContainsKey(pr.SystemName))
                        model.ActionPermissions[pr.SystemName] = new Dictionary<int, HashSet<string>>();

                    model.ActionPermissions[pr.SystemName][role.Id] = new HashSet<string>();

                    // Load saved action permissions from your DB if needed
                    var savedActions = await _permissionService.GetExistingActionsByPermissionAndRoleAsync(pr.Id, role.Id);
                    if (savedActions != null)
                    {
                        if (savedActions.Add) model.ActionPermissions[pr.SystemName][role.Id].Add("Add");
                        if (savedActions.Edit) model.ActionPermissions[pr.SystemName][role.Id].Add("Edit");
                        if (savedActions.Delete) model.ActionPermissions[pr.SystemName][role.Id].Add("Delete");
                        if (savedActions.View) model.ActionPermissions[pr.SystemName][role.Id].Add("View");
                        if (savedActions.Full) model.ActionPermissions[pr.SystemName][role.Id].Add("Full");
                    }
                }
            }

            return model;
        }

        #endregion

        #region Permission Record User Mapping Methods

        public async Task<PermissionUserMappingModel> PreparePermissionUserMappingModelAsync(PermissionUserMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var mappings = await _permissionService.GetAllPermissionRecordsUserMappingAsync();
            var permissionRecords = await _permissionService.GetAllPermissionRecordsAsync();

            foreach (var map in mappings)
            {
                var user = await _customerService.GetCustomerByIdAsync(map.UserId);
                if (user == null)
                    continue;

                if (!model.Users.Any(u => u.Id == user.Id))
                {
                    model.Users.Add(new CustomerModel
                    {
                        Id = user.Id,
                        Username = await _customerService.GetCustomerFullNameAsync(user)
                    });
                }

                if (!model.UserPermissions.ContainsKey(user.Id))
                    model.UserPermissions[user.Id] = new List<PermissionUserMappingActionModel>();

                var permission = permissionRecords.FirstOrDefault(p => p.Id == map.PermissionId);
                if (permission == null)
                    continue;

                model.UserPermissions[user.Id].Add(new PermissionUserMappingActionModel
                {
                    UserId = user.Id,
                    PermissionRecordId = permission.Id,
                    PermissionName = await _localizationService.GetLocalizedPermissionNameAsync(permission),
                    Add = map.Add,
                    Edit = map.Edit,
                    Delete = map.Delete,
                    View = map.View,
                    Full = map.Full
                });
            }

            return model;
        }


        public virtual async Task<PermissionUserRecordModel> PreparePermissionUserRecordModelAsync(PermissionUserRecordModel model)
        {
            model.AvailablePermissions = (await _permissionService.GetAllPermissionRecordsAsync()).Skip(1).Select(permissionRecord => new SelectListItem
            {
                Text = permissionRecord.Name,
                Value = permissionRecord.Id.ToString()
            }).ToList();

            var selectedCustomerRoleIds = new List<int>();
            var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole != null)
                selectedCustomerRoleIds.Add(registeredRole.Id);
            int[] selectedRoleIds = selectedCustomerRoleIds.ToArray();

            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: selectedRoleIds);
            model.AvailableUsers = (await Task.WhenAll(customers.Select(async customer => new SelectListItem
            {
                Text = await _customerService.GetCustomerFullNameAsync(customer),
                Value = customer.Id.ToString()
            }))).Where(x => !string.IsNullOrWhiteSpace(x.Text)).ToList();

            return model;
        }

        #endregion
    }
}