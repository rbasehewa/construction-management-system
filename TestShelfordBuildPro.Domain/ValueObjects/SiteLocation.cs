namespace TestShelfordBuildPro.Domain.ValueObjects;

// =====================================================
// SiteLocation — the full legal location of a site
// =====================================================
// WHY IS THIS SEPARATE FROM ADDRESS?
//   A street address tells you WHERE to drive.
//   A SiteLocation tells the COUNCIL and GOVERNMENT
//   exactly which piece of land is being built on.
//
//   In Perth construction you need BOTH:
//   - Street address → for the contract, subcontractors
//   - Lot/Plan number → for the building permit, title search
//
// REAL SHELFORD EXAMPLE:
//   Street:  "131 Dixon Rd, East Rockingham WA 6168"
//   Lot:     "Lot 502"
//   Plan:    "Deposited Plan 12345"
//   These are TWO different ways to identify the SAME land.
//
// ANGULAR EQUIVALENT:
//   interface SiteLocation {
//     address: Address;
//     lotNumber?: string;
//     planNumber?: string;
//     latitude?: number;
//     longitude?: number;
//   }
// =====================================================

public sealed record SiteLocation
{
    // ------------------------------------------------
    // PROPERTIES
    // ------------------------------------------------

    // The street address — wraps our Address value object
    public Address Address { get; }

    // Lot number from the Certificate of Title
    // e.g. "Lot 502" — required for building permit
    // Nullable (?) because we might not have it at enquiry stage
    public string? LotNumber { get; }

    // Deposited Plan number
    // e.g. "DP 12345" — identifies the survey plan
    public string? PlanNumber { get; }

    // GPS coordinates — useful for site maps and navigation
    // Nullable because not always available
    public double? Latitude { get; }
    public double? Longitude { get; }

    // ------------------------------------------------
    // CONSTRUCTOR
    // ------------------------------------------------
    public SiteLocation(
        Address address,
        string? lotNumber = null,
        string? planNumber = null,
        double? latitude = null,
        double? longitude = null)
    {
        // Address is the ONLY required field
        // Everything else can come later as the project progresses
        Address = address
            ?? throw new ArgumentNullException(
                nameof(address),
                "Site address is required.");

        // Validate GPS coordinates if provided
        // Latitude: -90 to 90, Longitude: -180 to 180
        // Perth is approx Lat: -31.9, Long: 115.8
        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new ArgumentException(
                "Latitude must be between -90 and 90.", nameof(latitude));

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new ArgumentException(
                "Longitude must be between -180 and 180.", nameof(longitude));

        LotNumber = lotNumber?.Trim();
        PlanNumber = planNumber?.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }

    // ------------------------------------------------
    // COMPUTED PROPERTIES
    // ------------------------------------------------

    // Does this site have full legal title details?
    // Required before submitting a building permit
    public bool HasTitleDetails =>
        !string.IsNullOrWhiteSpace(LotNumber) &&
        !string.IsNullOrWhiteSpace(PlanNumber);

    // Does this site have GPS coordinates?
    // Used for site maps and navigation links
    public bool HasGpsCoordinates =>
        Latitude.HasValue && Longitude.HasValue;

    // Full legal description for permit documents
    // e.g. "Lot 502 on DP 12345, 131 Dixon Rd East Rockingham WA 6168"
    public string LegalDescription =>
        HasTitleDetails
            ? $"{LotNumber} on {PlanNumber}, {Address.FullAddress}"
            : Address.FullAddress;

    public override string ToString() => LegalDescription;
}