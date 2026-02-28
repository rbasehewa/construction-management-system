using TestShelfordBuildPro.Domain.Common;
using TestShelfordBuildPro.Domain.ValueObjects;

namespace TestShelfordBuildPro.Domain.Entities;

// =====================================================
// VariationOrder — a change to the original scope
// =====================================================
// WHAT IS A VARIATION ORDER (VO)?
//   A VO is a formal document that changes the original
//   contract scope. It can ADD or REMOVE work.
//
// WHY ARE VARIATIONS SO COMMON IN CONSTRUCTION?
//   - Client changes mind mid-build (new bathroom layout)
//   - Unforeseen site conditions (rock excavation needed)
//   - Latent conditions (asbestos found in renovation)
//   - Design changes required by council
//   - Client wants to add extras (solar, upgraded tiles)
//
// REAL SHELFORD EXAMPLE:
//   Fenner Conveyors project — client decides mid-build
//   to add a mezzanine storage level.
//   Original contract: $5,000,000
//   VO-001: Mezzanine floor addition = +$150,000
//   New contract value: $5,150,000
//
// VARIATION STATUS FLOW:
//   Submitted → Approved (work proceeds, value added)
//   Submitted → Rejected (work does not proceed)
// =====================================================

public class VariationOrder : BaseEntity
{
    public Guid ProjectId { get; private set; }

    // e.g. "VO-001", "VO-002" — sequential per project
    public string VariationNumber { get; private set; }
        = string.Empty;

    // What is being changed?
    // e.g. "Add mezzanine storage level — Level 1 north end"
    public string Description { get; private set; }
        = string.Empty;

    // The dollar value of this variation
    public Money Value { get; private set; } = Money.Zero();

    // Current status of this variation
    public VariationStatus Status { get; private set; }

    // Why was it rejected? (if applicable)
    public string? RejectionReason { get; private set; }

    // When was it approved?
    public DateTime? ApprovedDate { get; private set; }

    // Internal — only Project.AddVariation() creates these
    internal VariationOrder(
        Guid projectId,
        string description,
        Money value,
        string raisedBy)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                "Variation description is required.",
                nameof(description));

        ProjectId = projectId;
        Description = description;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        CreatedBy = raisedBy;
        Status = VariationStatus.Submitted;

        // Auto generate VO number
        VariationNumber = $"VO-{DateTime.UtcNow:yyyyMMdd}" +
            $"-{Guid.NewGuid().ToString()[..4].ToUpper()}";
    }

    public void Approve(string approvedBy)
    {
        if (Status != VariationStatus.Submitted)
            throw new InvalidOperationException(
                "Only submitted variations can be approved.");

        Status = VariationStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
        SetModified(approvedBy);
    }

    public void Reject(string reason, string rejectedBy)
    {
        if (Status != VariationStatus.Submitted)
            throw new InvalidOperationException(
                "Only submitted variations can be rejected.");

        Status = VariationStatus.Rejected;
        RejectionReason = reason;
        SetModified(rejectedBy);
    }
}

// Enum lives here since it only relates to VariationOrder
public enum VariationStatus
{
    Submitted = 1,
    Approved = 2,
    Rejected = 3
}