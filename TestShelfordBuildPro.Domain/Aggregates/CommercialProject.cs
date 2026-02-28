using TestShelfordBuildPro.Domain.Enumerations;
using TestShelfordBuildPro.Domain.ValueObjects;

namespace TestShelfordBuildPro.Domain.Aggregates;

// =====================================================
// CommercialProject — Shelford Constructions Division
// =====================================================
// WHO USES THIS?
//   Shelford Constructions — the commercial division.
//   This covers everything that ISN'T a home:
//   - Offices, warehouses, workshops
//   - Retail (Woolworths Coolbellup)
//   - Defence (HMAS Stirling)
//   - Industrial (Fenner Conveyors Kwinana)
//   - Healthcare, Education, Sport & Recreation
//   - Fremantle Dockers Facility
//
// INHERITS FROM Project:
//   Gets ALL the base behaviour for free:
//   - AddVariation(), AddMilestone(), RaiseProgressClaim()
//   - Status tracking, financial totals, audit trail
//   - Domain events
//   Then ADDS commercial-specific rules and data on top.
//
// ANGULAR EQUIVALENT:
//   Like extending a base component:
//   class CommercialProjectComponent extends ProjectComponent
//   You get everything from the parent + add your own stuff.
//
// KEY COMMERCIAL-SPECIFIC RULES:
//   1. Building Permit cannot be issued without DA first
//   2. Government projects need a Performance Bond
//   3. GFA (Gross Floor Area) drives price benchmarking
// =====================================================

public sealed class CommercialProject : Project
{
    // ------------------------------------------------
    // COMMERCIAL-SPECIFIC PROPERTIES
    // ------------------------------------------------

    // Which market sector is this project in?
    // e.g. Warehouse, Defence, Retail, Healthcare
    public CommercialSector Sector { get; private set; }

    // Total floor area in square metres
    // Critical for: council approvals, cost per m² benchmarking
    // e.g. Fenner Conveyors warehouse = large industrial GFA
    public decimal? GrossFloorAreaM2 { get; private set; }

    // Number of above-ground storeys
    // Affects: structural requirements, building code class
    // e.g. single storey warehouse vs multi-storey office
    public int? NumberOfStoreys { get; private set; }

    // Has Development Approval been granted by Council?
    // Shelford CANNOT start construction without this
    // e.g. City of Cockburn approves the Kwinana project
    public bool DevelopmentApprovalObtained { get; private set; }

    // Has the Building Permit been issued?
    // Required BEFORE any physical works on site
    // Issued by a registered Building Surveyor
    public bool BuildingPermitIssued { get; private set; }

    // Is this a Government project?
    // Defence, Council, Healthcare, Education
    // These have extra compliance, security, reporting
    // e.g. HMAS Stirling required Defence clearances
    public bool IsGovernmentProject { get; private set; }

    // Security bond held by client
    // Typically 5-10% of contract value
    // Released when project is complete
    // e.g. 5% of $10M HMAS Stirling = $500k bond
    public Money? PerformanceBond { get; private set; }

    // ------------------------------------------------
    // FACTORY METHOD
    // ------------------------------------------------
    // WHY A FACTORY METHOD INSTEAD OF A PUBLIC CONSTRUCTOR?
    //   A constructor called "CommercialProject()" tells you nothing.
    //   A factory called "Create()" is self-documenting —
    //   it clearly says "I am creating a new commercial project".
    //
    //   Also lets us run extra setup logic after construction
    //   without making the constructor messy.
    //
    // ANGULAR EQUIVALENT:
    //   Like a static service method:
    //   static createProject(data: ProjectData): CommercialProject

    public static CommercialProject Create(
        string projectCode,
        string name,
        Guid clientId,
        Money contractValue,
        SiteLocation siteLocation,
        CommercialSector sector,
        ConstructionType constructionType,
        ContractType contractType,
        DateRange projectDates,
        string createdBy,
        decimal? grossFloorAreaM2 = null,
        int? numberOfStoreys = null,
        bool isGovernmentProject = false)
    {
        // Call the base Project constructor first
        // This runs ALL the base validation rules
        var project = new CommercialProject(
            projectCode,
            name,
            clientId,
            contractValue,
            siteLocation,
            constructionType,
            contractType,
            projectDates,
            createdBy);

        // Set commercial-specific properties
        project.Sector = sector;
        project.Type = ProjectType.Commercial;
        project.GrossFloorAreaM2 = grossFloorAreaM2;
        project.NumberOfStoreys = numberOfStoreys;
        project.IsGovernmentProject = isGovernmentProject;
        project.DevelopmentApprovalObtained = false;
        project.BuildingPermitIssued = false;

        return project;
    }

    // Private constructor — ONLY Create() can instantiate this
    private CommercialProject(
        string projectCode,
        string name,
        Guid clientId,
        Money contractValue,
        SiteLocation siteLocation,
        ConstructionType constructionType,
        ContractType contractType,
        DateRange projectDates,
        string createdBy)
        : base( // calls Project constructor
            projectCode,
            name,
            clientId,
            contractValue,
            siteLocation,
            constructionType,
            contractType,
            projectDates,
            createdBy)
    {
        Type = ProjectType.Commercial;
    }

    // ------------------------------------------------
    // COMMERCIAL-SPECIFIC DOMAIN METHODS
    // ------------------------------------------------

    /// <summary>
    /// Records that Development Approval has been received.
    /// This is a major milestone — Shelford can now proceed
    /// with detailed design and permit application.
    /// </summary>
    public void RecordDevelopmentApproval(string recordedBy)
    {
        if (DevelopmentApprovalObtained)
            throw new InvalidOperationException(
                "Development Approval has already been recorded.");

        DevelopmentApprovalObtained = true;
        SetModified(recordedBy);
    }

    /// <summary>
    /// Records that the Building Permit has been issued.
    /// Without this, Shelford CANNOT legally start construction.
    /// DA must be obtained FIRST — enforced as a business rule.
    /// </summary>
    public void RecordBuildingPermit(string recordedBy)
    {
        // BUSINESS RULE: DA must come before Building Permit
        // This is a legal requirement in Western Australia
        if (!DevelopmentApprovalObtained)
            throw new InvalidOperationException(
                "Development Approval must be obtained " +
                "before a Building Permit can be issued.");

        if (BuildingPermitIssued)
            throw new InvalidOperationException(
                "Building Permit has already been recorded.");

        BuildingPermitIssued = true;
        SetModified(recordedBy);
    }

    /// <summary>
    /// Sets the performance bond amount.
    /// Common on government and large commercial projects.
    /// e.g. Council requires 5% bond on $10M project = $500k
    /// </summary>
    public void SetPerformanceBond(Money bondAmount, string modifiedBy)
    {
        if (bondAmount.Amount <= 0)
            throw new ArgumentException(
                "Performance bond must be greater than zero.");

        PerformanceBond = bondAmount;
        SetModified(modifiedBy);
    }

    // ------------------------------------------------
    // COMPUTED PROPERTIES
    // ------------------------------------------------

    // Price per square metre — management benchmarking tool
    // Shelford compares this across similar projects
    // e.g. warehouse at $800/m², office fitout at $2,500/m²
    public decimal? PricePerSquareMetre =>
        GrossFloorAreaM2.HasValue && GrossFloorAreaM2 > 0
            ? Math.Round(
                ContractValue.Amount / GrossFloorAreaM2.Value, 2)
            : null;

    // Is this project ready to start on site?
    // Both DA and Building Permit must be in place
    public bool IsReadyToCommence =>
        DevelopmentApprovalObtained && BuildingPermitIssued;
}