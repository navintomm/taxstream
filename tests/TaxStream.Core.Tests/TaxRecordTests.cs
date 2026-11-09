using FluentAssertions;
using TaxStream.Core.Entities;
using Xunit;

namespace TaxStream.Core.Tests;

public class TaxRecordTests
{
    [Fact]
    public void RecalculateDerivedValues_SumsDeductionsCorrectly()
    {
        var record = new TaxRecord
        {
            GrossIncome = 100_000m,
            Deductions = new List<Deduction>
            {
                new() { Amount = 10_000m },
                new() { Amount = 5_000m }
            }
        };

        record.RecalculateDerivedValues();

        record.TotalDeductions.Should().Be(15_000m);
        record.TaxableIncome.Should().Be(85_000m);
    }

    [Fact]
    public void RecalculateDerivedValues_TaxableIncomeNeverNegative()
    {
        var record = new TaxRecord
        {
            GrossIncome = 1_000m,
            Deductions = new List<Deduction>
            {
                new() { Amount = 50_000m }
            }
        };

        record.RecalculateDerivedValues();

        record.TaxableIncome.Should().Be(0m);
    }

    [Fact]
    public void RecalculateDerivedValues_NoDeductions_TaxableIncomeEqualsGross()
    {
        var record = new TaxRecord { GrossIncome = 50_000m };
        record.RecalculateDerivedValues();
        record.TaxableIncome.Should().Be(50_000m);
    }
}
