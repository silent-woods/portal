using App.Core.Domain.ManageResumes;
using App.Core.Domain.result;
using App.Data.Extensions;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace Nop.Data.Mapping.Extentions.result
{
    public partial class CandidateResultBuilder : NopEntityBuilder<CandidatesResult>
    {
        #region Methods
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CandidatesResult.CandidateId)).AsInt32().ForeignKey<CandidatesResumes>()
                            .WithColumn(nameof(CandidatesResult.ResultStatusId)).AsInt32().Nullable()
            .WithColumn(nameof(CandidatesResult.Feedback)).AsString().Nullable()
            .WithColumn(nameof(CandidatesResult.Communication)).AsString().Nullable()
            .WithColumn(nameof(CandidatesResult.ConfidentLevel)).AsString().Nullable()
            .WithColumn(nameof(CandidatesResult.Incorrect)).AsString().Nullable()
             .WithColumn(nameof(CandidatesResult.partially)).AsString().Nullable()
            .WithColumn(nameof(CandidatesResult.correct)).AsString().Nullable();
           
        }
        #endregion
    }
}