using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Common;
using App.Core.Domain.Directory;
using App.Core.Domain.Localization;
using App.Data.Extensions;
using App.Core.Domain.Holidays;

namespace App.Data.Mapping.Builders.Holidays
{
    /// <summary>
    /// Represents a holiday entity builder
    /// </summary>
    public partial class HolidayBuilder : NopEntityBuilder<Holiday>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Holiday.Name)).AsString(1000).NotNullable();
        }

        #endregion
    }
}