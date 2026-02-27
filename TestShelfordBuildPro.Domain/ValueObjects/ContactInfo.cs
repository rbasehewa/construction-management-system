namespace TestShelfordBuildPro.Domain.ValueObjects;

// =====================================================
// ContactInfo — email + phone, always together
// =====================================================
// WHY A VALUE OBJECT FOR THIS?
//   Email and phone number always belong together.
//   You never store just an email without a phone
//   for a Shelford client — they need both to:
//   - Send contract documents (email)
//   - Call about site issues (phone)
//
//   If we stored them as separate strings on Client,
//   we'd have no way to validate them together or
//   ensure both are always provided.
//
// ANGULAR EQUIVALENT:
//   interface ContactInfo {
//     readonly email: string;
//     readonly phone: string;
//     readonly mobile?: string;
//   }
//
// IMMUTABLE — same reason as Money and Address.
//   To "update" a contact, you replace the whole
//   ContactInfo object, not individual fields.
// =====================================================

public sealed record ContactInfo
{
    // ------------------------------------------------
    // PROPERTIES
    // ------------------------------------------------
    public string Email { get; }
    public string Phone { get; }

    // Mobile is optional — not everyone has one
    // The ? means nullable — this CAN be null
    public string? Mobile { get; }

    // ------------------------------------------------
    // CONSTRUCTOR
    // ------------------------------------------------
    public ContactInfo(string email, string phone, string? mobile = null)
    {
        // BUSINESS RULE: Email is required and must look valid
        // We do a basic check — a proper email has an @ symbol
        // In production we'd use a more thorough regex
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException(
                "Email must be a valid email address.", nameof(email));

        // BUSINESS RULE: Phone is required
        // Shelford needs a phone number for every client
        // Site emergencies, progress updates, defect notifications
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException(
                "Phone number is required.", nameof(phone));

        // Store email in lowercase — consistency
        // "Ryan@Email.com" and "ryan@email.com" are the same
        Email = email.Trim().ToLower();
        Phone = phone.Trim();
        Mobile = mobile?.Trim(); // ?. means "only trim if not null"
    }

    // ------------------------------------------------
    // COMPUTED PROPERTY
    // ------------------------------------------------
    // Best number to call — mobile preferred over landline
    // Used when site supervisor needs to contact client urgently
    public string BestContactNumber =>
        !string.IsNullOrWhiteSpace(Mobile) ? Mobile : Phone;

    public override string ToString() =>
        $"{Email} | {BestContactNumber}";
}