using App.Core.Domain.ManageResumes;
using App.Data;
using App.Data.Extensions;
using App.Data.Mapping;
using App.Data.Migrations;
using FluentMigrator;

namespace Nop.Data.Migrations.UpgradeTo460
{
    [NopMigration("2024-03-27 01:01:12", "CandiatesInterviewMappingMigration for 4.60.0", MigrationProcessType.Update)]
    public class CandiatesInterviewMappingMigration : Migration
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public CandiatesInterviewMappingMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CandidateInterviewerMapping))).Exists())
            {
                Create.TableFor<CandidateInterviewerMapping>();
            }

        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
