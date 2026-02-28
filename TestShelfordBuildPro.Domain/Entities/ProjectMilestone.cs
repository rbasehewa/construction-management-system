using TestShelfordBuildPro.Domain.Common;

namespace TestShelfordBuildPro.Domain.Entities;

// =====================================================
// ProjectMilestone — a key date in the build process
// =====================================================
// WHAT IS A MILESTONE?
//   A milestone is a significant stage in the build.
//   When a milestone is reached, two things happen:
//   1. Shelford can raise a Progress Claim (get paid)
//   2. A building inspector may need to sign off
//
// RESIDENTIAL MILESTONES (Shelford Quality Homes):
//   1. Base/Slab    → footings and slab poured
//   2. Frame        → wall frames and roof trusses up
//   3. Lock-Up      → external walls, roof, windows done
//   4. Fixing       → internal fitout (doors, cabinets)
//   5. Practical Completion → ready to hand over keys
//
// COMMERCIAL MILESTONES (Shelford Constructions):
//   1. Site Establishment
//   2. Footings/Slab Complete
//   3. Structure Complete
//   4. Building Envelope (weatherproof)
//   5. Fitout Complete
//   6. Practical Completion
//
// WHY IS THIS AN ENTITY NOT A VALUE OBJECT?
//   Because a milestone has its OWN identity (Id).
//   "Frame Stage completed on 15 March 2025" is a
//   specific event we need to track and reference.
//   Two milestones with the same name are NOT the same
//   milestone — unlike Money where $500 = $500.
// =====================================================

public class ProjectMilestone : BaseEntity
{
    // Which project this milestone belongs to
    public Guid ProjectId { get; private set; }

    // e.g. "Frame Stage", "Lock-Up", "Practical Completion"
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    // When this milestone is expected to be reached
    public DateTime DueDate { get; private set; }

    // When it was actually completed — null means not done yet
    public DateTime? CompletedDate { get; private set; }

    // Computed — is this milestone finished?
    public bool IsCompleted => CompletedDate.HasValue;

    // Computed — is this milestone past due and not done?
    public bool IsOverdue =>
        !IsCompleted && DateTime.UtcNow > DueDate;

    // Internal constructor — only Project.AddMilestone() calls this
    // 'internal' means only code in the SAME PROJECT can use it
    internal ProjectMilestone(
        Guid projectId,
        string name,
        DateTime dueDate,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Milestone name is required.", nameof(name));

        ProjectId = projectId;
        Name = name;
        DueDate = dueDate;
        CreatedBy = createdBy;
    }

    // Mark this milestone as complete
    public void Complete(string completedBy)
    {
        if (IsCompleted)
            throw new InvalidOperationException(
                $"Milestone '{Name}' is already completed.");

        CompletedDate = DateTime.UtcNow;
        SetModified(completedBy);
    }

    // Update due date — e.g. weather delays pushed frame stage back
    public void UpdateDueDate(DateTime newDueDate, string modifiedBy)
    {
        if (IsCompleted)
            throw new InvalidOperationException(
                "Cannot change due date of a completed milestone.");

        DueDate = newDueDate;
        SetModified(modifiedBy);
    }
}