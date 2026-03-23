using App.Core;
using App.Core.Domain.Common;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class RecurringExpense : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public int ExpenseCategoryId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public int FrequencyId { get; set; }
    public int RecurrenceDay { get; set; }
    public int? RecurrenceMonth { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? LastGeneratedOnUtc { get; set; }
    public DateTime? NextGenerateOnUtc { get; set; }
    public bool IsActive { get; set; }
    public int AccountGroupId { get; set; }
    public int PaymentMethodId { get; set; }
    public int CreatedByEmployeeId { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
    #endregion

    #region Enum Accessors

    public RecurringFrequencyEnum Frequency
    {
        get => (RecurringFrequencyEnum)FrequencyId;
        set => FrequencyId = (int)value;
    }

    #endregion
}
