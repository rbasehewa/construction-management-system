namespace TestShelfordBuildPro.Domain.Enumerations;

// =====================================================
// ProjectType — what KIND of project is this?
// =====================================================
// In TypeScript you'd write:
//   enum ProjectType { Commercial, Residential, Civil }
// This is exactly the same thing in C#!
//
// Shelford has TWO main divisions:
//   1. Shelford Constructions  → Commercial, Civil, Government
//   2. Shelford Quality Homes  → Residential
//
// WHY USE AN ENUM?
//   Instead of storing "Commercial" as a plain string
//   (which could be misspelled as "commercial" or "COMMERCIAL")
//   we use an enum so the COMPILER catches mistakes for us.
//   Same reason you use TypeScript enums on the frontend!
// =====================================================

public enum ProjectType
{
    // Shelford Constructions division
    // Offices, warehouses, retail, industrial, defence
    Commercial = 1,

    // Shelford Quality Homes division
    // Custom homes, display homes, double storey
    Residential = 2,

    // Roads, drainage, earthworks
    // Usually sits under Commercial division
    Civil = 3,

    // Defence, council, healthcare, education
    // Extra compliance and reporting required
    Government = 4,

    // Fit-outs and remodels of existing buildings
    Renovation = 5
}