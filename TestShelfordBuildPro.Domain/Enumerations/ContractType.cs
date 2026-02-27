namespace TestShelfordBuildPro.Domain.Enumerations;

// =====================================================
// ContractType — how is Shelford being paid?
// =====================================================
// Different projects use different payment structures.
//
// LumpSum    → Fixed price. Client knows exact cost upfront.
//              Used for: all residential homes
//
// CostPlus   → Cost of work + Shelford's margin on top.
//              Used for: complex commercial where scope is unclear
//
// DesignBuild → Shelford handles BOTH design AND construction.
//               Client gets one contract, one point of contact.
//
// ManagementFee → Client supplies materials, Shelford manages.
//                 Less common, used for experienced developers.
// =====================================================

public enum ContractType
{
    LumpSum = 1,
    CostPlus = 2,
    DesignBuild = 3,
    ManagementFee = 4
}