using App.Core.Domain.WeeklyQuestion;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;


namespace Nop.Data.Mapping.Extentions.StockManagement
{
    public partial class WeeklyQuestionBuilder : NopEntityBuilder<WeeklyQuestions>
    {
        #region Methods
        public override void MapEntity(CreateTableExpressionBuilder table)
        {

        }
        #endregion
    }
}