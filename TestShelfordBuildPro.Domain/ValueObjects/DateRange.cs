namespace TestShelfordBuildPro.Domain.ValueObjects;

// =====================================================
// DateRange — a start and end date, validated together
// =====================================================
// WHY A VALUE OBJECT FOR DATES?
//   Start and end dates always belong together.
//   If we stored them as separate DateTime fields,
//   nothing would stop someone setting:
//     StartDate = 2025-12-01
//     EndDate   = 2025-01-01  ← end BEFORE start! ❌
//
//   By grouping them in a Value Object, we validate
//   them TOGETHER in one place. The rule "end must be
//   after start" is enforced the moment you create it.
//
// ANGULAR EQUIVALENT:
//   interface DateRange {
//     readonly start: Date;
//     readonly end?: Date;
//   }
//
// SHELFORD USE CASES:
//   - Project start and expected completion date
//     e.g. Fenner Conveyors: Jan 2025 → Dec 2025
//   - Defects period (90 days after Practical Completion)
//   - Warranty period (Shelford Lifetime Warranty start)
//   - Contract validity period
// =====================================================

public sealed record DateRange
{
    // ------------------------------------------------
    // PROPERTIES
    // ------------------------------------------------

    // Start date is always required
    // A project must have a start date
    public DateTime Start { get; }

    // End date is optional (nullable)
    // At enquiry stage we might not know the end date yet
    // It gets set once the contract is signed
    public DateTime? End { get; }

    // ------------------------------------------------
    // CONSTRUCTOR
    // ------------------------------------------------
    public DateRange(DateTime start, DateTime? end = null)
    {
        // BUSINESS RULE: End date must be AFTER start date
        // Cannot finish a project before it begins!
        if (end.HasValue && end.Value <= start)
            throw new ArgumentException(
                "End date must be after the start date.", nameof(end));

        Start = start;
        End = end;
    }

    // ------------------------------------------------
    // COMPUTED PROPERTIES
    // ------------------------------------------------

    // How many days is this project running?
    // Returns null if no end date set yet
    // e.g. 365 days for a one year commercial build
    public int? DurationInDays =>
        End.HasValue
            ? (int)(End.Value - Start).TotalDays
            : null;

    // Is this project currently active right now?
    // Used for dashboard — "show me all active projects"
    public bool IsActive =>
        Start <= DateTime.UtcNow &&
        (!End.HasValue || End.Value >= DateTime.UtcNow);

    // Has this project's end date passed?
    // Used to flag overdue projects on the dashboard
    public bool IsOverdue =>
        End.HasValue && End.Value < DateTime.UtcNow;

    // How many days remaining until end date?
    // Used for project timeline tracking
    // Returns null if no end date, negative if overdue
    public int? DaysRemaining =>
        End.HasValue
            ? (int)(End.Value - DateTime.UtcNow).TotalDays
            : null;

    // ------------------------------------------------
    // DISPLAY
    // ------------------------------------------------
    // e.g. "01 Jan 2025 → 31 Dec 2025 (365 days)"
    // e.g. "01 Jan 2025 → TBC" if no end date yet
    public override string ToString()
    {
        var endDisplay = End.HasValue
            ? End.Value.ToString("dd MMM yyyy")
            : "TBC"; // To Be Confirmed

        var duration = DurationInDays.HasValue
            ? $" ({DurationInDays} days)"
            : string.Empty;

        return $"{Start:dd MMM yyyy} → {endDisplay}{duration}";
    }
}