using System.ComponentModel.DataAnnotations;

namespace App.Core.Domain.Employees
{
    /// <summary>
    /// Represents a BloodGroup
    /// </summary>
    public enum BloodGroupEnum
    {
        Select = 0,
        OPositive,
        APositive,
        BPositive,
        ABPositive,
        ABNegative,
        ANegative,
        BNegative,
        ONegative
    }
}