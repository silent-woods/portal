using App.Core.Domain.ManageResumes;
using App.Core.Domain.result;
using App.Data;
using App.Data.Extensions;
using App.Data.Mapping;
using App.Data.Migrations;
using FluentMigrator;
using System.Data;

namespace Nop.Data.Migrations.UpgradeTo460
{
    [NopMigration("2024-01-07 01:01:12", "CandiatesResultMigration for 4.60.0", MigrationProcessType.Update)]
    public class CandiatesResultMigration : Migration
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public CandiatesResultMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            //if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CandidatesResult))).Exists())
            //{
            //    Create.TableFor<CandidatesResult>();
            //}
            //else
            //{
            //    if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CandidatesResult))).Column(nameof(CandidatesResult.CandidateId)).Exists())
            //    {
            //        Alter.Table(nameof(CandidatesResult))
            //      .AddColumn(nameof(CandidatesResult.CandidateId)).AsInt32().ForeignKey<CandidatesResumes>();

            //    }
            //}
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CandidatesResult))).Exists())
            {
                Create.TableFor<CandidatesResult>();
            }
            else
            {
                if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CandidatesResult))).Column(nameof(CandidatesResult.CandidateId)).Exists())
                {
                    Alter.Table(nameof(CandidatesResult))
                  .AddColumn(nameof(CandidatesResult.CandidateId)).AsInt32().ForeignKey<CandidatesResumes>().Nullable();

                }
            }

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CandidatesResumes))).Exists())
            {
                Create.TableFor<CandidatesResumes>();
            }


        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
