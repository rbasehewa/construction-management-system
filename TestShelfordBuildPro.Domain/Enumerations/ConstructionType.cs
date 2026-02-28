namespace TestShelfordBuildPro.Domain.Enumerations;

// =====================================================
// ConstructionType — HOW is the building being built?
// =====================================================
// Shelford has deep expertise in ALL of these methods.
// The construction type drives which crew, equipment,
// and materials are needed on site.
//
// Shelford's specialties (from their website):
//   - Complex formwork
//   - Brick and steel (standard Perth residential)
//   - Precast tilt-up concrete panels (warehouses)
//   - Composite building materials
// =====================================================

public enum ConstructionType
{
    // Complex concrete formwork — Shelford specialty
    // Used for: multi-storey commercial, complex structures
    Formwork = 1,

    // Traditional Perth residential construction
    // Used for: all Shelford Quality Homes builds
    BrickAndSteel = 2,

    // Tilt-up precast concrete panels
    // Used for: warehouses, industrial (Fenner Conveyors)
    PrecastTiltUp = 3,

    // Mix of materials — modern commercial
    Composite = 4,

    // Timber framing — some residential/renovation
    Timber = 5,

    // Steel portal frames — large commercial/industrial
    SteelFrame = 6
}