using App.Core.Domain.Employees;
using App.Core.Domain.ManageResumes;
using App.Data.Extensions;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Customers
{
    /// <summary>
    /// Represents a customer address mapping entity builder
    /// </summary>
    public partial class CandidateInterviwerMappingBuilder : NopEntityBuilder<CandidateInterviewerMapping>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(CandidateInterviewerMapping), nameof(CandidateInterviewerMapping.CandidatesId)))
                    .AsInt32().ForeignKey<CandidatesResumes>().PrimaryKey()
                .WithColumn(NameCompatibilityManager.GetColumnName(typeof(CandidateInterviewerMapping), nameof(CandidateInterviewerMapping.EmployeeId)))
                    .AsInt32().ForeignKey<Employee>().PrimaryKey();
        }

        #endregion
    }
}