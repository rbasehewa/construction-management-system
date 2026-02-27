namespace TestShelfordBuildPro.Domain.Enumerations;

// =====================================================
// CommercialSector — what TYPE of commercial building?
// =====================================================
// Shelford Constructions has built ALL of these.
// Real examples from their portfolio:
//   Warehouse    → Fenner Conveyors, Kwinana
//   Defence      → HMAS Stirling Naval Base
//   Retail       → Woolworths Supermarket Coolbellup
//   Recreation   → Fremantle Dockers Facility
//   Industrial   → Alcoa Wagerup Refinery
// =====================================================

public enum CommercialSector
{
    Office = 1,
    Warehouse = 2,
    Workshop = 3,
    Retail = 4,
    Hospitality = 5,  // Cafes, restaurants, hotels
    Healthcare = 6,  // Clinics, aged care facilities
    Education = 7,  // Schools, universities
    SportRecreation = 8,  // Ovals, gyms, community centres
    Defence = 9,  // Military facilities, high security
    Industrial = 10, // Factories, refineries, conveyors
    Civil = 11  // Roads, drainage, earthworks
}