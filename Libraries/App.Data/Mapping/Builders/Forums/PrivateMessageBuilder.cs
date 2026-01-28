using System.Data;
using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Customers;
using App.Core.Domain.Forums;
using App.Data.Extensions;

namespace App.Data.Mapping.Builders.Forums
{
    /// <summary>
    /// Represents a private message entity builder
    /// </summary>
    public partial class PrivateMessageBuilder : NopEntityBuilder<PrivateMessage>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PrivateMessage.Subject)).AsString(450).NotNullable()
                .WithColumn(nameof(PrivateMessage.Text)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(PrivateMessage.FromCustomerId)).AsInt32().ForeignKey<Customer>().OnDelete(Rule.None)
                .WithColumn(nameof(PrivateMessage.ToCustomerId)).AsInt32().ForeignKey<Customer>().OnDelete(Rule.None);
        }

        #endregion
    }
}