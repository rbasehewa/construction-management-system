using TestShelfordBuildPro.Domain.Common;
using TestShelfordBuildPro.Domain.ValueObjects;

namespace TestShelfordBuildPro.Domain.Aggregates;

// =====================================================
// Client — represents anyone who commissions Shelford
// =====================================================
// WHO ARE SHELFORD'S CLIENTS?
//   Shelford serves a wide range of clients:
//
//   COMMERCIAL CLIENTS:
//   - Woolworths Group (retail supermarket builds)
//   - Alcoa (Wagerup Refinery industrial expansion)
//   - Michelin/Fenner Conveyors (Kwinana industrial)
//   - Fremantle Football Club (sports facility)
//   - Department of Defence (HMAS Stirling)
//   - City of Cockburn (local government)
//
//   RESIDENTIAL CLIENTS:
//   - First home buyers (affordable homes)
//   - Families (double storey custom homes)
//   - Investors (display homes, duplexes)
//   - Luxury buyers (custom $1M+ homes)
//
// WHY IS CLIENT ITS OWN AGGREGATE?
//   A Client exists INDEPENDENTLY of any project.
//   - Woolworths can have 5 projects running at once
//   - A homeowner might build with Shelford twice
//   - Client contact details change independently
//   - Client credit limits are managed separately
//
// RELATIONSHIP TO PROJECT:
//   Project stores ClientId (just the Guid).
//   NOT the full Client object.
//   This is loose coupling — DDD best practice.
//
// ANGULAR EQUIVALENT:
//   Like a separate service managing client state,
//   not embedded inside the project component.
// =====================================================

public sealed class Client : BaseEntity
{
    // ------------------------------------------------
    // IDENTITY
    // ------------------------------------------------

    // Human readable code e.g. "CLT-001", "CLT-042"
    // Appears on invoices, contracts, correspondence
    public string ClientCode { get; private set; }
        = string.Empty;

    // Full LEGAL name — exactly as it appears on contracts
    // Company:    "Woolworths Group Limited"
    // Individual: "John Michael Smith"
    public string LegalName { get; private set; }
        = string.Empty;

    // Trading name if different from legal name
    // e.g. Legal: "Alcoa of Australia Limited"
    //      Trading: "Alcoa"
    public string? TradingName { get; private set; }

    // Australian Business Number
    // Required for all company clients
    // 11-digit number e.g. "51 008 672 179"
    // Used on all tax invoices (GST compliance)
    public string? Abn { get; private set; }

    // Is this a company or an individual?
    // Companies need ABN, individuals need different docs
    public bool IsCompany { get; private set; }

    // ------------------------------------------------
    // CONTACT
    // ------------------------------------------------

    // Primary contact person at the client organisation
    // e.g. "Site Manager at Woolworths" or "Homeowner"
    public ContactInfo PrimaryContact { get; private set; }
        = null!;

    // Where to send invoices
    // May differ from the site address
    // e.g. Woolworths invoices go to Head Office, not the store
    public Address BillingAddress { get; private set; }
        = null!;

    // ------------------------------------------------
    // FINANCIAL
    // ------------------------------------------------

    // Approved credit limit for this client
    // Repeat clients like Woolworths get higher limits
    // New clients start with zero until credit approved
    public Money? CreditLimit { get; private set; }

    // How many days client has to pay invoices
    // Standard: 30 days
    // Government: sometimes 45 days
    // Residential: per payment schedule in HIA contract
    public int PaymentTermsDays { get; private set; }

    // ------------------------------------------------
    // FACTORY METHOD
    // ------------------------------------------------
    public static Client Create(
        string clientCode,
        string legalName,
        ContactInfo primaryContact,
        Address billingAddress,
        bool isCompany,
        string createdBy,
        string? tradingName = null,
        string? abn = null,
        int paymentTermsDays = 30)
    {
        // BUSINESS RULE: Legal name is required
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException(
                "Client legal name is required.",
                nameof(legalName));

        // BUSINESS RULE: Client code is required
        if (string.IsNullOrWhiteSpace(clientCode))
            throw new ArgumentException(
                "Client code is required.",
                nameof(clientCode));

        // BUSINESS RULE: Companies should have an ABN
        // We warn but don't hard fail — ABN might come later
        // In production you'd validate ABN format (11 digits)

        // BUSINESS RULE: Payment terms must be positive
        if (paymentTermsDays <= 0)
            throw new ArgumentException(
                "Payment terms must be greater than zero.",
                nameof(paymentTermsDays));

        return new Client
        {
            ClientCode = clientCode,
            LegalName = legalName,
            TradingName = tradingName,
            PrimaryContact = primaryContact
                ?? throw new ArgumentNullException(
                    nameof(primaryContact)),
            BillingAddress = billingAddress
                ?? throw new ArgumentNullException(
                    nameof(billingAddress)),
            IsCompany = isCompany,
            Abn = abn,
            PaymentTermsDays = paymentTermsDays,
            CreatedBy = createdBy
        };
    }

    // Private constructor — only Create() can instantiate
    private Client() { }

    // ------------------------------------------------
    // DOMAIN METHODS
    // ------------------------------------------------

    /// <summary>
    /// Updates the primary contact details.
    /// e.g. New site manager assigned at Woolworths
    /// </summary>
    public void UpdatePrimaryContact(
        ContactInfo newContact,
        string modifiedBy)
    {
        PrimaryContact = newContact
            ?? throw new ArgumentNullException(nameof(newContact));
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Updates the billing address.
    /// e.g. Woolworths moves their accounts department
    /// </summary>
    public void UpdateBillingAddress(
        Address newAddress,
        string modifiedBy)
    {
        BillingAddress = newAddress
            ?? throw new ArgumentNullException(nameof(newAddress));
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Sets or updates the credit limit.
    /// Approved by Shelford's accounts team.
    /// e.g. Woolworths approved for $2M credit
    /// </summary>
    public void SetCreditLimit(
        Money creditLimit,
        string modifiedBy)
    {
        if (creditLimit.Amount < 0)
            throw new ArgumentException(
                "Credit limit cannot be negative.");

        CreditLimit = creditLimit;
        SetModified(modifiedBy);
    }

    /// <summary>
    /// Records the ABN once verified.
    /// </summary>
    public void RecordAbn(string abn, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(abn))
            throw new ArgumentException(
                "ABN is required.", nameof(abn));

        Abn = abn.Trim().Replace(" ", ""); // normalise format
        SetModified(modifiedBy);
    }

    // ------------------------------------------------
    // COMPUTED PROPERTIES
    // ------------------------------------------------

    // Display name — trading name if exists, otherwise legal
    // e.g. shows "Alcoa" not "Alcoa of Australia Limited"
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(TradingName)
            ? TradingName
            : LegalName;

    public override string ToString() =>
        $"[{ClientCode}] {DisplayName}";
}