using App.Core.Domain.JobPostings;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;


namespace Nop.Data.Mapping.Extentions.StockManagement
{
    public partial class JobPostingBuilder : NopEntityBuilder<JobPosting>
    {
        #region Methods
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
           
        }
        #endregion
    }
}