using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/09 11:00:00:0000001", "Misc.AccountManagement add AccountTransactionId to EmployeeMonthlySalary", MigrationProcessType.Installation)]
public partial class SalaryAccountTransactionIdMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table("EmployeeMonthlySalary").Exists() &&
            !Schema.Table("EmployeeMonthlySalary").Column("AccountTransactionId").Exists())
        {
            Alter.Table("EmployeeMonthlySalary")
                .AddColumn("AccountTransactionId").AsInt32().NotNullable().WithDefaultValue(0);
        }
    }

    #endregion
}
