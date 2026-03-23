using App.Data.Migrations;
using FluentMigrator;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/11 10:00:00:0000001", "Misc.AccountManagement add EmployeeSalaryCustomComponent table", MigrationProcessType.Installation)]
public partial class SalaryCustomComponentMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table("EmployeeSalaryCustomComponent").Exists())
        {
            Create.Table("EmployeeSalaryCustomComponent")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
                .WithColumn("SalaryRecordId").AsInt32().NotNullable()
                .WithColumn("TypeId").AsInt32().NotNullable()
                .WithColumn("Name").AsString(200).NotNullable()
                .WithColumn("Amount").AsDecimal(18, 4).NotNullable()
                .WithColumn("CreatedOnUtc").AsDateTime().NotNullable();
        }
    }

    #endregion
}
