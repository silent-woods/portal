using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Customers;
using App.Services.Customers;
using App.Services.Seo;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Framework.Models.Extensions;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer role model factory implementation
    /// </summary>
    public partial class CustomerRoleModelFactory : ICustomerRoleModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CustomerRoleModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerService = customerService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare customer role search model
        /// </summary>
        /// <param name="searchModel">Customer role search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer role search model
        /// </returns>
        public virtual Task<CustomerRoleSearchModel> PrepareCustomerRoleSearchModelAsync(CustomerRoleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged customer role list model
        /// </summary>
        /// <param name="searchModel">Customer role search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer role list model
        /// </returns>
        public virtual async Task<CustomerRoleListModel> PrepareCustomerRoleListModelAsync(CustomerRoleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get customer roles
            var customerRoles = (await _customerService.GetAllCustomerRolesAsync(true)).ToPagedList(searchModel);

            //prepare grid model
            var model = new CustomerRoleListModel().PrepareToGrid(searchModel, customerRoles, () =>
            {
                return customerRoles.Select(role =>
                {
                    //fill in model values from the entity
                    var customerRoleModel = role.ToModel<CustomerRoleModel>();

                    return customerRoleModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare customer role model
        /// </summary>
        /// <param name="model">Customer role model</param>
        /// <param name="customerRole">Customer role</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer role model
        /// </returns>
        public virtual Task<CustomerRoleModel> PrepareCustomerRoleModelAsync(CustomerRoleModel model, CustomerRole customerRole, bool excludeProperties = false)
        {
            if (customerRole != null)
            {
                //fill in model values from the entity
                model ??= customerRole.ToModel<CustomerRoleModel>();
            }

            //set default values for the new model
            if (customerRole == null)
                model.Active = true;

            return Task.FromResult(model);
        }

        #endregion
    }
}