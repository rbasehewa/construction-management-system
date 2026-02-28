using TestShelfordBuildPro.Domain.Common;
using TestShelfordBuildPro.Domain.Entities;
using TestShelfordBuildPro.Domain.Enumerations;
using TestShelfordBuildPro.Domain.ValueObjects;

namespace TestShelfordBuildPro.Domain.Aggregates;

// =====================================================
// Project — THE ROOT AGGREGATE
// =====================================================
// WHAT IS AN AGGREGATE ROOT?
//   Think of it like a HEAD component in Angular.
//   Everything related to a Project flows THROUGH here.
//   You never directly touch Milestones, Variations,
//   or Progress Claims — you always go through Project.
//
//   ANGULAR EQUIVALENT:
//   Imagine a parent component that owns child components.
//   You never call child methods directly from outside —
//   you always emit events UP to the parent, and the
//   parent decides what to do with the children.
//
// WHY IS PROJECT THE ROOT?
//   Everything Shelford does revolves around a Project:
//   ✅ A Client commissions a Project
//   ✅ A Contract is signed FOR a Project
//   ✅ Milestones belong TO a Project
//   ✅ Variations are raised ON a Project
//   ✅ Progress Claims are invoiced FOR a Project
//   ✅ Site issues are logged ON a Project
//
// BUSINESS RULES (INVARIANTS) THIS CLASS ENFORCES:
//   1. A Project MUST have a name
//   2. Contract value must be greater than zero
//   3. Cannot add variations to a cancelled project
//   4. Cannot complete a project without going through
//      Practical Completion and Defects first
//   5. Cannot raise progress claims before InProgress
//
// 'abstract' means you cannot create a Project directly.
//   You must create either:
//   - CommercialProject (Shelford Constructions)
//   - ResidentialProject (Shelford Quality Homes)
//   Just like an abstract base class in TypeScript!
// =====================================================

public abstract class Project : BaseEntity
{
    // ------------------------------------------------
    // CORE IDENTITY
    // ------------------------------------------------

    // Human readable code — appears on contracts, invoices, site signs
    // e.g. "SHF-COMM-2025-001" for a commercial project
    public string ProjectCode { get; protected set; } = string.Empty;

    // Display name of the project
    // e.g. "Fenner Conveyors Kwinana Facility"
    // e.g. "Smith Residence - Double Storey Bicton"
    public string Name { get; protected set; } = string.Empty;

    public string? Description { get; protected set; }

    // Which Shelford division — Commercial or Residential
    public ProjectType Type { get; protected set; }

    // ------------------------------------------------
    // STATUS
    // ------------------------------------------------
    // Where is this project in the Shelford lifecycle?
    // Enquiry → Quoting → ContractSigned → InProgress
    //        → PracticalCompletion → Defects → Complete
    public ProjectStatus Status { get; protected set; }

    // ------------------------------------------------
    // FINANCIAL
    // ------------------------------------------------
    // These are the numbers Shelford management watches daily

    // The original signed contract amount — never changes
    // e.g. $5,000,000 for the Fenner Conveyors project
    public Money ContractValue { get; protected set; } = Money.Zero();

    // Running total of ALL approved variation orders
    // e.g. client added a mezzanine floor = +$150,000
    public Money ApprovedVariationsTotal { get; protected set; } = Money.Zero();

    // Current contract value = original + all variations
    // This is what Shelford will actually be paid in total
    // e.g. $5,000,000 + $150,000 = $5,150,000
    public Money CurrentContractValue =>
        ContractValue + ApprovedVariationsTotal;

    // How much has been invoiced so far via progress claims
    // e.g. claimed 60% so far = $3,090,000
    public Money AmountInvoiced { get; protected set; } = Money.Zero();

    // How much is still left to invoice
    // e.g. $5,150,000 - $3,090,000 = $2,060,000 remaining
    public Money AmountToInvoice =>
        CurrentContractValue - AmountInvoiced;

    // ------------------------------------------------
    // CONSTRUCTION DETAILS
    // ------------------------------------------------
    public ConstructionType ConstructionType { get; protected set; }
    public ContractType ContractType { get; protected set; }
    public SiteLocation SiteLocation { get; protected set; } = null!;
    public DateRange ProjectDates { get; protected set; } = null!;

    // ------------------------------------------------
    // RELATIONSHIPS
    // ------------------------------------------------
    // We store IDs only — not the full objects
    // This keeps aggregates loosely coupled
    // ANGULAR EQUIVALENT: storing an ID reference, not
    // embedding the whole object in your component state
    public Guid ClientId { get; protected set; }
    public Guid? ProjectManagerId { get; protected set; }
    public Guid? SiteSupervisorId { get; protected set; }

    // ------------------------------------------------
    // CHILD COLLECTIONS
    // ------------------------------------------------
    // Private backing fields — ONLY this class can modify them
    // External code gets a READ ONLY view via the properties below
    // This is ENCAPSULATION — a core OOP principle
    //
    // ANGULAR EQUIVALENT:
    // private _milestones: Milestone[] = [];
    // get milestones(): readonly Milestone[] { return this._milestones; }

    private readonly List<ProjectMilestone> _milestones = new();
    private readonly List<VariationOrder> _variations = new();
    private readonly List<ProgressClaim> _progressClaims = new();

    // Public read-only views of the collections
    // Nobody outside can call _milestones.Add() directly
    // They MUST go through AddMilestone() method below
    public IReadOnlyList<ProjectMilestone> Milestones =>
        _milestones.AsReadOnly();
    public IReadOnlyList<VariationOrder> Variations =>
        _variations.AsReadOnly();
    public IReadOnlyList<ProgressClaim> ProgressClaims =>
        _progressClaims.AsReadOnly();

    // ------------------------------------------------
    // CONSTRUCTOR
    // ------------------------------------------------
    // Protected — only child classes can call this
    // (CommercialProject and ResidentialProject)
    protected Project(
        string projectCode,
        string name,
        Guid clientId,
        Money contractValue,
        SiteLocation siteLocation,
        ConstructionType constructionType,
        ContractType contractType,
        DateRange projectDates,
        string createdBy)
    {
        // INVARIANT: All required fields must be provided
        if (string.IsNullOrWhiteSpace(projectCode))
            throw new ArgumentException(
                "Project code is required.", nameof(projectCode));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Project name is required.", nameof(name));

        // INVARIANT: Contract value must be positive
        if (contractValue.Amount <= 0)
            throw new ArgumentException(
                "Contract value must be greater than zero.",
                nameof(contractValue));

        ProjectCode = projectCode;
        Name = name;
        ClientId = clientId;
        ContractValue = contractValue;
        SiteLocation = siteLocation
            ?? throw new ArgumentNullException(nameof(siteLocation));
        ConstructionType = constructionType;
        ContractType = contractType;
        ProjectDates = projectDates
            ?? throw new ArgumentNullException(nameof(projectDates));
        CreatedBy = createdBy;

        // All new projects start at Enquiry stage
        Status = ProjectStatus.Enquiry;

        // Financial totals all start at zero
        ApprovedVariationsTotal = Money.Zero();
        AmountInvoiced = Money.Zero();
    }

    // ------------------------------------------------
    // DOMAIN METHODS — the behaviour of a Project
    // ------------------------------------------------
    // This is the KEY difference between Domain Driven Design
    // and a simple data model.
    //
    // BAD (anemic model — just data, no behaviour):
    //   project.Status = ProjectStatus.InProgress; // anyone can set anything!
    //
    // GOOD (rich domain model — behaviour + rules):
    //   project.AdvanceStatus(ProjectStatus.InProgress, "ryan.m");
    //   // validates the transition, raises events, updates audit trail
    //
    // ANGULAR EQUIVALENT:
    //   Instead of setting properties directly on a model,
    //   you call a service method that validates and updates state.

    // ------------------------------------------------
    // ADVANCE STATUS
    // ------------------------------------------------
    /// <summary>
    /// Moves the project to the next status in the lifecycle.
    /// Enforces valid transitions — you can't skip stages.
    /// </summary>
    public void AdvanceStatus(ProjectStatus newStatus, string modifiedBy)
    {
        // INVARIANT: Cannot change a cancelled project
        if (Status == ProjectStatus.Cancelled)
            throw new InvalidOperationException(
                "Cannot change the status of a cancelled project.");

        // INVARIANT: Must go through Defects before Complete
        // Shelford requires a defects period before final signoff
        if (newStatus == ProjectStatus.Complete
            && Status != ProjectStatus.Defects)
            throw new InvalidOperationException(
                "Project must complete the Defects period before " +
                "being marked Complete.");

        Status = newStatus;
        SetModified(modifiedBy);
    }

    // ------------------------------------------------
    // ADD VARIATION ORDER
    // ------------------------------------------------
    /// <summary>
    /// Adds a Variation Order to the project.
    /// Variations are scope changes mid-build — very common
    /// in construction. e.g. client wants to add a mezzanine
    /// floor after the slab has already been poured.
    /// </summary>
    public void AddVariation(
        string description,
        Money value,
        string raisedBy)
    {
        // INVARIANT: No variations on cancelled projects
        if (Status == ProjectStatus.Cancelled)
            throw new InvalidOperationException(
                "Cannot add a variation to a cancelled project.");

        // INVARIANT: No variations on completed projects
        if (Status == ProjectStatus.Complete)
            throw new InvalidOperationException(
                "Cannot add a variation to a completed project.");

        // Create the variation entity
        var variation = new VariationOrder(
            projectId: Id,
            description: description,
            value: value,
            raisedBy: raisedBy);

        _variations.Add(variation);

        // Update the running total immediately
        ApprovedVariationsTotal += value;

        SetModified(raisedBy);
    }

    // ------------------------------------------------
    // ADD MILESTONE
    // ------------------------------------------------
    /// <summary>
    /// Adds a project milestone.
    /// Milestones are the key stages Shelford tracks:
    /// Slab, Frame, Lock-Up, Fixing, Practical Completion
    /// </summary>
    public ProjectMilestone AddMilestone(
        string name,
        DateTime dueDate,
        string createdBy)
    {
        var milestone = new ProjectMilestone(
            projectId: Id,
            name: name,
            dueDate: dueDate,
            createdBy: createdBy);

        _milestones.Add(milestone);
        SetModified(createdBy);

        return milestone; // return it so caller knows the new ID
    }

    // ------------------------------------------------
    // RAISE PROGRESS CLAIM
    // ------------------------------------------------
    /// <summary>
    /// Raises a progress claim — how Shelford gets paid.
    /// At each milestone, Shelford claims a % of the
    /// contract value. e.g. Slab complete = claim 20%
    /// On a $5M contract that's a $1M invoice.
    /// </summary>
    public ProgressClaim RaiseProgressClaim(
        Percentage completionPercentage,
        string raisedBy)
    {
        // INVARIANT: Can only claim once project is active
        if (Status < ProjectStatus.InProgress)
            throw new InvalidOperationException(
                "Cannot raise progress claims before " +
                "project is In Progress.");

        // Calculate the claim amount based on % complete
        // e.g. 20% of $5,000,000 = $1,000,000
        var claimAmount = new Money(
            CurrentContractValue.Amount * completionPercentage.AsDecimal,
            CurrentContractValue.Currency);

        var claim = new ProgressClaim(
            projectId: Id,
            completionPercentage: completionPercentage,
            claimAmount: claimAmount,
            raisedBy: raisedBy);

        _progressClaims.Add(claim);
        AmountInvoiced += claimAmount;
        SetModified(raisedBy);

        return claim;
    }

    // ------------------------------------------------
    // ASSIGN PROJECT MANAGER
    // ------------------------------------------------
    /// <summary>
    /// Assigns a Shelford Project Manager to this project.
    /// Every project needs a PM once contract is signed.
    /// </summary>
    public void AssignProjectManager(
        Guid projectManagerId,
        string modifiedBy)
    {
        ProjectManagerId = projectManagerId;
        SetModified(modifiedBy);
    }

    // ------------------------------------------------
    // CANCEL PROJECT
    // ------------------------------------------------
    /// <summary>
    /// Cancels the project with a reason.
    /// Records who cancelled it and why.
    /// </summary>
    public void Cancel(string reason, string cancelledBy)
    {
        // INVARIANT: Cannot cancel an already completed project
        if (Status == ProjectStatus.Complete)
            throw new InvalidOperationException(
                "Cannot cancel a completed project.");

        Status = ProjectStatus.Cancelled;
        Description = $"{Description} | CANCELLED: {reason}";
        SetModified(cancelledBy);
    }
}