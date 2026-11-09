namespace TaxStream.Core.Enums;

/// <summary>
/// Filing status values. Extend with jurisdiction-specific values once jurisdiction is confirmed.
/// </summary>
public enum FilingStatus
{
    Single = 1,
    MarriedFilingJointly = 2,
    MarriedFilingSeparately = 3,
    HeadOfHousehold = 4,
    QualifyingSurvivingSpouse = 5
}
