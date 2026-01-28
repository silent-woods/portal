using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Localization;
using App.Core.Domain.Polls;
using App.Data.Extensions;

namespace App.Data.Mapping.Builders.Polls
{
    /// <summary>
    /// Represents a poll entity builder
    /// </summary>
    public partial class PollBuilder : NopEntityBuilder<Poll>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Poll.Name)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(Poll.LanguageId)).AsInt32().ForeignKey<Language>();
        }

        #endregion
    }
}