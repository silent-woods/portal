using App.Core.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountGroups;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountTransactions;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.BankAccounts;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoiceItems;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Invoices;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PaymentTerms;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ProjectBillings;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using Satyanam.Plugin.Misc.AccountManagement.ViewEngine;

namespace Satyanam.Plugin.Misc.AccountManagement.Infrastructure;

public partial class NopStartup : INopStartup
{
    #region Methods

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAccountManagementModelFactory, AccountManagementModelFactory>();

        services.AddScoped<IAccountManagementService, AccountManagementService>();

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new AccountManagementViewEngine());
        });

        services.AddTransient<IValidator<AccountGroupModel>, AccountGroupValidator>();

        services.AddTransient<IValidator<AccountTransactionModel>, AccountTransactionValidator>();

        services.AddTransient<IValidator<BankAccountModel>, BankAccountValidator>();

        services.AddTransient<IValidator<ProjectBillingModel>, ProjectBillingValidator>();

        services.AddTransient<IValidator<PaymentTermModel>, PaymentTermValidator>();

        services.AddTransient<IValidator<InvoiceModel>, InvoiceValidator>();

        services.AddTransient<IValidator<InvoiceItemModel>, InvoiceItemValidator>();

        services.AddTransient<IValidator<InvoicePaymentHistoryModel>, InvoicePaymentHistoryValidator>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => int.MaxValue;

    #endregion
}
