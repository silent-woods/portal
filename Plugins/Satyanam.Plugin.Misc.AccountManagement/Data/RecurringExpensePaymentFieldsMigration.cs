using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/12 12:00:00:0000001", "Misc.AccountManagement add AccountGroupId and PaymentMethodId to RecurringExpense", MigrationProcessType.Installation)]
public partial class RecurringExpensePaymentFieldsMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table("RecurringExpense").Exists())
        {
            if (!Schema.Table("RecurringExpense").Column("AccountGroupId").Exists())
            {
                Alter.Table("RecurringExpense")
                    .AddColumn("AccountGroupId").AsInt32().NotNullable().WithDefaultValue(0);
            }
            if (!Schema.Table("RecurringExpense").Column("PaymentMethodId").Exists())
            {
                Alter.Table("RecurringExpense")
                    .AddColumn("PaymentMethodId").AsInt32().NotNullable().WithDefaultValue(0);
            }
        }
    }

    #endregion
}
