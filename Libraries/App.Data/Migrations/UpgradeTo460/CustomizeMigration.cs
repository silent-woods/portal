using FluentMigrator;
using App.Core.Domain.Customers;

namespace App.Data.Migrations.UpgradeTo460
{
    [NopMigration("2024-01-18 01:01:01", "Product customize", MigrationProcessType.Update)]
    public class CustomizeMigration : Migration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (Schema.Table(nameof(CustomerRole)).Column("PurchasedWithProductId").Exists())
                Delete.Column("PurchasedWithProductId").FromTable(nameof(CustomerRole));
        
        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
