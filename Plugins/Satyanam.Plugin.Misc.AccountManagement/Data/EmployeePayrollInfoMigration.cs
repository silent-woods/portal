using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/09 13:00:00:0000001", "Misc.AccountManagement add EmployeePayrollInfo table", MigrationProcessType.Installation)]
public partial class EmployeePayrollInfoMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table("EmployeePayrollInfo").Exists())
        {
            Create.Table("EmployeePayrollInfo")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
                .WithColumn("EmployeeId").AsInt32().NotNullable()
                .WithColumn("PanCardNumber").AsString(20).Nullable()
                .WithColumn("BankName").AsString(200).Nullable()
                .WithColumn("BankAccountNumber").AsString(100).Nullable()
                .WithColumn("CreatedOnUtc").AsDateTime2().NotNullable()
                .WithColumn("UpdatedOnUtc").AsDateTime2().NotNullable();
        }
    }

    #endregion
}
