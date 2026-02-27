namespace TestShelfordBuildPro.Domain.ValueObjects;

// =====================================================
// Address — represents a physical location
// =====================================================
// WHY A VALUE OBJECT?
//   Two addresses with the same street, suburb, postcode
//   ARE the same address. We don't care WHICH address
//   object it is — just what the values ARE.
//   Exactly like Money — equality is based on values.
//
// ANGULAR EQUIVALENT:
//   interface Address {
//     streetNumber: string;
//     streetName: string;
//     suburb: string;
//     state: string;
//     postCode: string;
//     country: string;
//   }
//
// WHY DOES SHELFORD NEED THIS?
//   Every project has a site address. This address:
//   - Appears on the Building Permit
//   - Appears on the Contract
//   - Used for Council approvals
//   - Insurance policies reference it
//   - Subcontractors need it to find the site
//
// REAL SHELFORD EXAMPLES:
//   131 Dixon Rd, East Rockingham WA 6168 (HQ)
//   Kwinana Beach industrial site (Fenner Conveyors)
//   HMAS Stirling, Rockingham WA
// =====================================================

public sealed record Address
{
    // ------------------------------------------------
    // PROPERTIES
    // ------------------------------------------------
    public string StreetNumber { get; }
    public string StreetName { get; }
    public string Suburb { get; }
    public string State { get; }
    public string PostCode { get; }
    public string Country { get; }

    // ------------------------------------------------
    // CONSTRUCTOR
    // ------------------------------------------------
    public Address(
        string streetNumber,
        string streetName,
        string suburb,
        string state,
        string postCode,
        string country = "Australia") // Default to Australia — Shelford is Perth-based
    {
        // BUSINESS RULE: Street name is required
        // Every building permit needs a valid street
        if (string.IsNullOrWhiteSpace(streetName))
            throw new ArgumentException(
                "Street name is required.", nameof(streetName));

        // BUSINESS RULE: Suburb is required
        if (string.IsNullOrWhiteSpace(suburb))
            throw new ArgumentException(
                "Suburb is required.", nameof(suburb));

        // BUSINESS RULE: PostCode is required
        if (string.IsNullOrWhiteSpace(postCode))
            throw new ArgumentException(
                "PostCode is required.", nameof(postCode));

        StreetNumber = streetNumber?.Trim() ?? string.Empty;
        StreetName = streetName.Trim();
        Suburb = suburb.Trim();

        // Default to WA if not provided — Shelford operates in Perth
        State = string.IsNullOrWhiteSpace(state)
            ? "WA"
            : state.Trim().ToUpper();

        PostCode = postCode.Trim();
        Country = country.Trim();
    }

    // ------------------------------------------------
    // COMPUTED PROPERTY
    // ------------------------------------------------
    // Builds the full address string for display
    // on contracts, permits, and emails
    // e.g. "131 Dixon Rd, East Rockingham WA 6168, Australia"
    public string FullAddress =>
        $"{StreetNumber} {StreetName}, {Suburb} {State} {PostCode}, {Country}"
        .Trim();

    // Short version for UI display
    // e.g. "East Rockingham WA 6168"
    public string ShortAddress =>
        $"{Suburb} {State} {PostCode}";

    public override string ToString() => FullAddress;
}