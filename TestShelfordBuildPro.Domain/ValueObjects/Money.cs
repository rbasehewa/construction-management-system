namespace TestShelfordBuildPro.Domain.ValueObjects;

// =====================================================
// Money — represents a monetary amount + currency
// =====================================================
// WHY NOT JUST USE decimal?
//   Because $500,000 AUD is NOT the same as $500,000 USD.
//   Currency must ALWAYS travel with the amount.
//   If we just used decimal, someone could accidentally
//   compare AUD with USD and get wrong results.
//
// ANGULAR EQUIVALENT:
//   Like a TypeScript interface:
//   interface Money { amount: number; currency: string; }
//   But with built-in validation and math operations.
//
// IMMUTABLE — means once created, it NEVER changes.
//   You don't "change" Money — you create a NEW Money.
//   project.ContractValue = new Money(600_000, "AUD")
//   This prevents bugs where shared values get mutated.
//
// SHELFORD USE CASES:
//   - Contract values ($1M commercial warehouse)
//   - Variation orders (+$50k for mezzanine floor)
//   - Progress claims (20% complete = $200k claim)
//   - Performance bonds (5% of contract held by client)
//   - Retention amounts (5% withheld until completion)
// =====================================================

// 'sealed' means nothing can inherit from Money
// 'record' means equality is based on VALUES not reference
// Two Money(500, "AUD") objects are EQUAL — unlike classes
public sealed record Money
{
    // ------------------------------------------------
    // PROPERTIES — read only after creation
    // ------------------------------------------------
    public decimal Amount { get; }
    public string Currency { get; }

    // ------------------------------------------------
    // CONSTRUCTOR — validates on creation
    // ------------------------------------------------
    // If bad data comes in, it FAILS IMMEDIATELY here.
    // Not silently later when saving to the database.
    // This is "fail fast" — find bugs as early as possible.
    public Money(decimal amount, string currency = "AUD")
    {
        // BUSINESS RULE: Money cannot be negative
        // You can't have a contract for -$500,000
        if (amount < 0)
            throw new ArgumentException(
                "Money amount cannot be negative.", nameof(amount));

        // BUSINESS RULE: Currency must be a valid 3-letter ISO code
        // AUD, USD, EUR — not "dollars" or "aud" or ""
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException(
                "Currency must be a 3-letter ISO code (e.g. AUD).", nameof(currency));

        // Always round to 2 decimal places (cents)
        // No $500,000.123456 — just $500,000.12
        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpper(); // always store as "AUD" not "aud"
    }

    // ------------------------------------------------
    // MATH OPERATIONS
    // ------------------------------------------------
    // These let us do: contractValue + variation = newTotal
    // Just like adding numbers, but with currency checking

    // Adding two Money values
    // e.g. $500,000 + $50,000 variation = $550,000
    public static Money operator +(Money a, Money b)
    {
        // BUSINESS RULE: Cannot add different currencies
        // $500k AUD + $50k USD = ERROR (not $550k anything)
        if (a.Currency != b.Currency)
            throw new InvalidOperationException(
                $"Cannot add {a.Currency} and {b.Currency}. " +
                $"Convert to same currency first.");

        return new Money(a.Amount + b.Amount, a.Currency);
    }

    // Subtracting Money values
    // e.g. $500,000 contract - $25,000 retention = $475,000 payable
    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException(
                $"Cannot subtract {b.Currency} from {a.Currency}.");

        return new Money(a.Amount - b.Amount, a.Currency);
    }

    // Comparing Money values
    // e.g. if (progressClaim > minimumClaimAmount)
    public static bool operator >(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot compare different currencies.");
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot compare different currencies.");
        return a.Amount < b.Amount;
    }

    // ------------------------------------------------
    // HELPER FACTORIES
    // ------------------------------------------------
    // Shortcut to create a zero-dollar value
    // e.g. when a project starts, invoiced amount = Zero
    public static Money Zero(string currency = "AUD")
        => new(0, currency);

    // ------------------------------------------------
    // DISPLAY
    // ------------------------------------------------
    // How Money looks when printed / logged
    // e.g. "AUD 500,000.00"
    public override string ToString()
        => $"{Currency} {Amount:N2}";
}