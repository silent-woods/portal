using App.Core.Domain.ManageResumes;
using App.Data.Extensions;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using System.Data;

namespace Nop.Data.Mapping.Extentions.StockManagement
{
    public partial class CandiatesResumesBuilder : NopEntityBuilder<CandidatesResumes>
    {
        #region Methods
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
      //      table
      //.WithColumn(nameof(CandidatesResumes.TraineeId)).AsInt32().ForeignKey<Candidate>(onDelete: Rule.SetNull).Nullable();
        }
        #endregion
    }
}