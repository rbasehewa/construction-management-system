namespace TestShelfordBuildPro.Domain.Enumerations;

// =====================================================
// HomeType — what TYPE of home is Shelford building?
// =====================================================
// Shelford Quality Homes builds all of these.
// Real examples:
//   DisplayHome  → Karlup display home (HIA Award Winner)
//   CustomLuxury → Bicton luxury home (HIA Award Winner)
//   Affordable   → Entry level first homes
// =====================================================

public enum HomeType
{
    SingleStorey = 1,   // Most common Perth home
    DoubleStorey = 2,   // Growing family homes
    DisplayHome = 3,   // Show homes in estates
    CustomLuxury = 4,   // Fully custom, design-led
    Affordable = 5,   // Entry level, first home buyers
    Knockdown = 6,   // Knock down and rebuild
    Duplex = 7    // Two dwellings on one lot
}