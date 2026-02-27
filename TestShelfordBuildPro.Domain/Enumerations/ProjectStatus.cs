namespace TestShelfordBuildPro.Domain.Enumerations;

// =====================================================
// ProjectStatus — where is the project RIGHT NOW?
// =====================================================
// Every Shelford project moves through these stages.
// Think of it like an Angular route state or a
// multi-step form — you always know which step you're on.
//
// THE SHELFORD LIFECYCLE:
//   Client calls → Enquiry
//   Site visit   → Feasibility  
//   Price sent   → Quoting
//   Contract signed → ContractSigned
//   Works begin  → InProgress
//   Build done   → PracticalCompletion
//   Fix defects  → Defects (90 day period)
//   All done     → Complete
// =====================================================

public enum ProjectStatus
{
    // Client made initial contact
    // Shelford's sales team follows up
    Enquiry = 0,

    // Pre-project site assessment
    // Is this project viable? What are the risks?
    Feasibility = 1,

    // Estimating team is preparing a price
    // Can take weeks for large commercial projects
    Quoting = 2,

    // Client accepted the price, contract is signed
    // This is when Shelford officially wins the job!
    ContractSigned = 3,

    // Site is active — trades on site every day
    // Progress claims raised at each milestone
    InProgress = 4,

    // Construction complete, defects list issued to client
    // Client walks through and notes anything not right
    PracticalCompletion = 5,

    // Fixing items from the defects list
    // Typically 90 days for commercial, varies for residential
    Defects = 6,

    // Everything signed off, warranty period begins
    // Shelford Quality Homes: Lifetime Warranty starts here
    Complete = 7,

    // Temporarily paused
    // e.g. client ran out of funding, council delayed approval
    OnHold = 8,

    // Project will not proceed
    Cancelled = 9
}