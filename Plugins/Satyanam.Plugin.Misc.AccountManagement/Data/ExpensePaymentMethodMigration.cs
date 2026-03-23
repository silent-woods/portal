using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/12 11:00:00:0000001", "Misc.AccountManagement add PaymentMethodId to Expense", MigrationProcessType.Installation)]
public partial class ExpensePaymentMethodMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table("Expense").Exists() &&
            !Schema.Table("Expense").Column("PaymentMethodId").Exists())
        {
            Alter.Table("Expense")
                .AddColumn("PaymentMethodId").AsInt32().NotNullable().WithDefaultValue(0);
        }
    }

    #endregion
}
