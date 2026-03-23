using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/12 10:00:00:0000001", "Misc.AccountManagement add AccountTransactionId to Expense", MigrationProcessType.Installation)]
public partial class ExpenseAccountTransactionIdMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table("Expense").Exists() &&
            !Schema.Table("Expense").Column("AccountTransactionId").Exists())
        {
            Alter.Table("Expense")
                .AddColumn("AccountTransactionId").AsInt32().NotNullable().WithDefaultValue(0);
        }
    }

    #endregion
}
