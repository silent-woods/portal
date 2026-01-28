using FluentMigrator;
using App.Core.Domain.Media;
using App.Data.Extensions;
using App.Data.Mapping;

namespace App.Data.Migrations.UpgradeTo460
{
    [NopMigration("2022-03-16 00:00:00", "Product video", MigrationProcessType.Update)]
    public class VideoMigration : Migration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Video))).Exists())
            {
                Create.TableFor<Video>();
            }
        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
