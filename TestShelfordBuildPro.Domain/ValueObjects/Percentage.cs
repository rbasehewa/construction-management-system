namespace TestShelfordBuildPro.Domain.ValueObjects;

// =====================================================
// Percentage — a validated percentage value 0-100
// =====================================================
// WHY NOT JUST USE decimal?
//   Same reason as Money — a plain decimal has no rules.
//   Nothing stops someone passing in 150% or -10%.
//   Percentage enforces: must be between 0 and 100.
//
// ANGULAR EQUIVALENT:
//   You'd probably use a number with a validator:
//   percentage: [0, [Validators.min(0), Validators.max(100)]]
//   But here the validation lives IN the object itself,
//   not in a form validator. Always enforced everywhere.
//
// SHELFORD USE CASES:
//   - Progress claims: "20% complete = claim $200k"
//   - GST: always 10% in Australia
//   - Retention: 5% withheld until Practical Completion
//     e.g. $500k contract → $25k held back until defects done
//   - Project completion tracking on the dashboard
//   - Margin percentage on Cost Plus contracts
// =====================================================

public sealed record Percentage
{
    // ------------------------------------------------
    // PROPERTIES
    // ------------------------------------------------
    public decimal Value { get; }

    // ------------------------------------------------
    // CONSTRUCTOR
    // ------------------------------------------------
    public Percentage(decimal value)
    {
        // BUSINESS RULE: Percentage must be 0 to 100
        // 110% complete makes no sense
        // -5% complete makes no sense
        if (value < 0 || value > 100)
            throw new ArgumentException(
                $"Percentage must be between 0 and 100. Got: {value}",
                nameof(value));

        // Round to 2 decimal places
        // 33.333333% becomes 33.33%
        Value = Math.Round(value, 2);
    }

    // ------------------------------------------------
    // COMMON PRE-BUILT PERCENTAGES
    // ------------------------------------------------
    // These are shortcuts for the most common values
    // Used constantly throughout the Shelford system

    // Australia GST is always 10%
    // Applied to all Shelford invoices
    public static Percentage GST => new(10);

    // Standard retention held by client
    // Released after defects period is complete
    public static Percentage Retention => new(5);

    // 0% — project just started, nothing invoiced yet
    public static Percentage Zero => new(0);

    // 100% — project fully complete
    public static Percentage Complete => new(100);

    // ------------------------------------------------
    // COMPUTED PROPERTIES
    // ------------------------------------------------

    // Converts to a decimal multiplier for calculations
    // e.g. 20% → 0.20 so we can do: $1,000,000 * 0.20 = $200,000
    // ANGULAR EQUIVALENT: value / 100
    public decimal AsDecimal => Value / 100;

    // ------------------------------------------------
    // USAGE EXAMPLE IN THE SYSTEM:
    // ------------------------------------------------
    // When Shelford raises a progress claim at 20%:
    //
    //   var progress = new Percentage(20);
    //   var contractValue = new Money(1_000_000, "AUD");
    //   var claimAmount = new Money(
    //       contractValue.Amount * progress.AsDecimal, "AUD");
    //   // claimAmount = AUD 200,000.00
    //
    // When calculating GST on an invoice:
    //   var invoice = new Money(500_000, "AUD");
    //   var gst = new Money(
    //       invoice.Amount * Percentage.GST.AsDecimal, "AUD");
    //   // gst = AUD 50,000.00
    // ------------------------------------------------

    public override string ToString() => $"{Value}%";
}