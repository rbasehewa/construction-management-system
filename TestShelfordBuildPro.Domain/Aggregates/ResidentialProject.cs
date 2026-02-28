using TestShelfordBuildPro.Domain.Enumerations;
using TestShelfordBuildPro.Domain.ValueObjects;

namespace TestShelfordBuildPro.Domain.Aggregates;

// =====================================================
// ResidentialProject — Shelford Quality Homes Division
// =====================================================
// WHO USES THIS?
//   Shelford Quality Homes — building people's homes.
//   Everything from first home buyers to luxury custom:
//   - Single and double storey homes
//   - Display homes (Karlup — HIA Award Winner)
//   - Custom luxury homes (Bicton — HIA Award Winner)
//   - Affordable first homes
//   - Knockdown rebuilds
//
// KEY DIFFERENCES FROM COMMERCIAL:
//   - Uses HIA (Housing Industry Association) contracts
//     not commercial building contracts
//   - Shelford Lifetime Warranty applies to ALL homes
//   - Pre-Start meeting is a REQUIRED step before building
//     (client locks in all colour and fixture selections)
//   - First Home Owner Grant (FHOG) may apply
//   - Typically fixed price (LumpSum) contracts
//   - Smaller dollar values but higher volume of projects
//
// REAL SHELFORD EXAMPLES:
//   Karlup Display Home — HIA 2020 Display Home of the Year
//   Bicton Luxury Home  — HIA 2020 Best New Bathroom Winner
// =====================================================

public sealed class ResidentialProject : Project
{
    // ------------------------------------------------
    // RESIDENTIAL-SPECIFIC PROPERTIES
    // ------------------------------------------------

    // What type of home is being built?
    // SingleStorey, DoubleStorey, CustomLuxury, DisplayHome...
    public HomeType HomeType { get; private set; }

    // Number of bedrooms — used in pricing and marketing
    // "4 bedroom double storey from $450,000"
    public int Bedrooms { get; private set; }

    // Number of bathrooms
    public int Bathrooms { get; private set; }

    // Total living area in square metres
    // Used for price per m² benchmarking
    // Typical Perth home: 180m² - 350m²
    public decimal? LivingAreaM2 { get; private set; }

    // Does this home have the Shelford Lifetime Warranty?
    // Shelford Quality Homes offers this on ALL new builds
    // This is a major selling point vs competitors
    public bool HasLifetimeWarranty { get; private set; }

    // Has the Pre-Start meeting been completed?
    // This is where clients confirm ALL selections:
    // - Brick colour, roof tiles, floor tiles
    // - Kitchen cabinets, benchtops, appliances
    // - Bathroom fixtures, tapware, mirrors
    // - Paint colours, door handles, light fittings
    // Construction CANNOT begin until Pre-Start is done
    public bool PreStartMeetingCompleted { get; private set; }

    // HIA contract number — all residential builds use
    // Housing Industry Association standard contracts
    // Required by WA Building Commission
    public string? HiaContractNumber { get; private set; }

    // Is this client a first home buyer?
    // First Home Owner Grant (FHOG) may apply in WA
    // Shelford's team helps clients navigate this
    public bool IsFirstHomeBuyer { get; private set; }

    // Land title details for building permit
    // e.g. "Lot 502 on DP 12345"
    public string? LandTitleReference { get; private set; }

    // ------------------------------------------------
    // FACTORY METHOD
    // ------------------------------------------------
    public static ResidentialProject Create(
        string projectCode,
        string name,
        Guid clientId,
        Money contractValue,
        SiteLocation siteLocation,
        HomeType homeType,
        int bedrooms,
        int bathrooms,
        DateRange projectDates,
        string createdBy,
        decimal? livingAreaM2 = null,
        bool isFirstHomeBuyer = false,
        string? hiaContractNumber = null,
        string? landTitleReference = null)
    {
        // BUSINESS RULE: Must have at least 1 bedroom
        if (bedrooms < 1)
            throw new ArgumentException(
                "A home must have at least 1 bedroom.",
                nameof(bedrooms));

        // BUSINESS RULE: Must have at least 1 bathroom
        if (bathrooms < 1)
            throw new ArgumentException(
                "A home must have at least 1 bathroom.",
                nameof(bathrooms));

        // BUSINESS RULE: Living area must be realistic if provided
        // Smallest Perth home is ~80m², largest custom ~800m²
        if (livingAreaM2.HasValue && livingAreaM2 < 50)
            throw new ArgumentException(
                "Living area seems too small. " +
                "Minimum realistic size is 50m².",
                nameof(livingAreaM2));

        var project = new ResidentialProject(
            projectCode,
            name,
            clientId,
            contractValue,
            siteLocation,
            projectDates,
            createdBy);

        project.HomeType = homeType;
        project.Type = ProjectType.Residential;
        project.Bedrooms = bedrooms;
        project.Bathrooms = bathrooms;
        project.LivingAreaM2 = livingAreaM2;
        project.IsFirstHomeBuyer = isFirstHomeBuyer;
        project.HiaContractNumber = hiaContractNumber;
        project.LandTitleReference = landTitleReference;

        // ALL Shelford Quality Homes get lifetime warranty
        project.HasLifetimeWarranty = true;

        // Pre-Start always starts as not completed
        project.PreStartMeetingCompleted = false;

        // Residential is almost always Brick and Steel in Perth
        project.ConstructionType = ConstructionType.BrickAndSteel;

        // Residential is always fixed price lump sum
        project.ContractType = ContractType.LumpSum;

        return project;
    }

    // Private constructor
    private ResidentialProject(
        string projectCode,
        string name,
        Guid clientId,
        Money contractValue,
        SiteLocation siteLocation,
        DateRange projectDates,
        string createdBy)
        : base(
            projectCode,
            name,
            clientId,
            contractValue,
            siteLocation,
            ConstructionType.BrickAndSteel, // Perth residential default
            ContractType.LumpSum,           // Residential always lump sum
            projectDates,
            createdBy)
    {
        Type = ProjectType.Residential;
    }

    // ------------------------------------------------
    // RESIDENTIAL-SPECIFIC DOMAIN METHODS
    // ------------------------------------------------

    /// <summary>
    /// Marks the Pre-Start meeting as complete.
    /// This GATES construction — cannot start building
    /// until all client selections are locked in.
    /// Prevents costly changes mid-build.
    /// </summary>
    public void CompletePreStart(string completedBy)
    {
        // BUSINESS RULE: Contract must be signed first
        if (Status < ProjectStatus.ContractSigned)
            throw new InvalidOperationException(
                "Contract must be signed before the " +
                "Pre-Start meeting can be completed.");

        if (PreStartMeetingCompleted)
            throw new InvalidOperationException(
                "Pre-Start meeting has already been completed.");

        PreStartMeetingCompleted = true;
        SetModified(completedBy);
    }

    /// <summary>
    /// Records the HIA contract number.
    /// Required by WA Building Commission for all
    /// residential construction contracts.
    /// </summary>
    public void RecordHiaContract(
        string hiaContractNumber,
        string recordedBy)
    {
        if (string.IsNullOrWhiteSpace(hiaContractNumber))
            throw new ArgumentException(
                "HIA contract number is required.",
                nameof(hiaContractNumber));

        HiaContractNumber = hiaContractNumber;
        SetModified(recordedBy);
    }

    /// <summary>
    /// Records the land title reference.
    /// Required for the building permit application.
    /// </summary>
    public void RecordLandTitle(
        string landTitleReference,
        string recordedBy)
    {
        if (string.IsNullOrWhiteSpace(landTitleReference))
            throw new ArgumentException(
                "Land title reference is required.",
                nameof(landTitleReference));

        LandTitleReference = landTitleReference;
        SetModified(recordedBy);
    }

    // ------------------------------------------------
    // COMPUTED PROPERTIES
    // ------------------------------------------------

    // Price per square metre
    // Shelford benchmarks: ~$2,000-$4,000/m² for Perth homes
    // CustomLuxury can be $5,000+/m²
    public decimal? PricePerSquareMetre =>
        LivingAreaM2.HasValue && LivingAreaM2 > 0
            ? Math.Round(
                ContractValue.Amount / LivingAreaM2.Value, 2)
            : null;

    // Is this home ready to start building?
    // Pre-Start must be complete AND contract must be signed
    public bool IsReadyToCommence =>
        PreStartMeetingCompleted &&
        Status >= ProjectStatus.ContractSigned;

    // Friendly description for dashboard display
    // e.g. "4 Bed / 2 Bath — Double Storey"
    public string HomeDescription =>
        $"{Bedrooms} Bed / {Bathrooms} Bath — {HomeType}";
}