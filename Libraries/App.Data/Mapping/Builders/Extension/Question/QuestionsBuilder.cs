using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Extentions.StockManagement
{
    public partial class QuestionsBuilder : NopEntityBuilder<Questions>
    {
        #region Methods
        public override void MapEntity(CreateTableExpressionBuilder table)
        {

        }
        #endregion
    }
}