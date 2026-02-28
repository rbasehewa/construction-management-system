using TestShelfordBuildPro.Domain.Common;
using TestShelfordBuildPro.Domain.ValueObjects;

namespace TestShelfordBuildPro.Domain.Entities;

// =====================================================
// ProgressClaim — how Shelford gets paid during a build
// =====================================================
// WHAT IS A PROGRESS CLAIM?
//   Instead of one invoice at the end, Shelford invoices
//   at each milestone to maintain cashflow during the build.
//   This is STANDARD practice in Australian construction.
//
// HOW IT WORKS:
//   1. Milestone reached (e.g. Frame Stage complete)
//   2. Shelford raises a Progress Claim for that stage
//   3. Independent certifier verifies % complete
//   4. Client has 10 business days to respond
//   5. Client pays the certified amount
//   6. 5% retention is withheld until Practical Completion
//
// REAL EXAMPLE on a $5,000,000 contract:
//   PC-001: Slab complete     20% = $1,000,000 claimed
//           Less 5% retention      =    $50,000 withheld
//           Net payable            =   $950,000 paid
//
//   PC-002: Frame complete    40% = $2,000,000 claimed
//           Less 5% retention      =   $100,000 withheld
//           Net payable            = $1,900,000 paid
// =====================================================

public class ProgressClaim : BaseEntity
{
    public Guid ProjectId { get; private set; }

    // Sequential claim number e.g. "PC-001", "PC-002"
    public string ClaimNumber { get; private set; }
        = string.Empty;

    // What % of the project is complete at this claim?
    // e.g. 20% after slab, 40% after frame
    public Percentage CompletionPercentage { get; private set; }
        = Percentage.Zero;

    // The gross amount being claimed
    // e.g. 20% of $5,000,000 = $1,000,000
    public Money ClaimAmount { get; private set; }
        = Money.Zero();

    // 5% retention withheld by client
    // Released after Practical Completion
    // e.g. 5% of $1,000,000 = $50,000 withheld
    public Money RetentionHeld { get; private set; }
        = Money.Zero();

    // What client actually pays = ClaimAmount - Retention
    // e.g. $1,000,000 - $50,000 = $950,000
    public Money NetPayable =>
        ClaimAmount - RetentionHeld;

    // Current status of this claim
    public ProgressClaimStatus ClaimStatus { get; private set; }

    // When was it certified by the independent certifier?
    public DateTime? CertifiedDate { get; private set; }

    // When did the client actually pay?
    public DateTime? PaidDate { get; private set; }

    // Client must respond by this date
    // Standard is 10 business days ~ 14 calendar days
    public DateTime DueDate { get; private set; }

    // Internal — only Project.RaiseProgressClaim() creates these
    internal ProgressClaim(
        Guid projectId,
        Percentage completionPercentage,
        Money claimAmount,
        string raisedBy)
    {
        ProjectId = projectId;
        CompletionPercentage = completionPercentage;
        ClaimAmount = claimAmount;
        CreatedBy = raisedBy;
        ClaimStatus = ProgressClaimStatus.Submitted;

        // Calculate 5% retention
        var retentionAmount =
            claimAmount.Amount * Percentage.Retention.AsDecimal;
        RetentionHeld = new Money(
            retentionAmount, claimAmount.Currency);

        // Client has 14 calendar days to respond
        DueDate = DateTime.UtcNow.AddDays(14);

        // Auto generate claim number
        ClaimNumber = $"PC-{DateTime.UtcNow:yyyyMM}" +
            $"-{Guid.NewGuid().ToString()[..4].ToUpper()}";
    }

    public void Certify(string certifiedBy)
    {
        if (ClaimStatus != ProgressClaimStatus.Submitted)
            throw new InvalidOperationException(
                "Only submitted claims can be certified.");

        ClaimStatus = ProgressClaimStatus.Certified;
        CertifiedDate = DateTime.UtcNow;
        SetModified(certifiedBy);
    }

    public void RecordPayment(string recordedBy)
    {
        if (ClaimStatus != ProgressClaimStatus.Certified)
            throw new InvalidOperationException(
                "Claim must be certified before recording payment.");

        ClaimStatus = ProgressClaimStatus.Paid;
        PaidDate = DateTime.UtcNow;
        SetModified(recordedBy);
    }

    public void Dispute(string reason, string disputedBy)
    {
        if (ClaimStatus != ProgressClaimStatus.Submitted)
            throw new InvalidOperationException(
                "Only submitted claims can be disputed.");

        ClaimStatus = ProgressClaimStatus.Disputed;
        SetModified(disputedBy);
    }
}

// Enum lives here since it only relates to ProgressClaim
public enum ProgressClaimStatus
{
    Submitted = 1,
    Certified = 2,
    Disputed = 3,
    Paid = 4
}