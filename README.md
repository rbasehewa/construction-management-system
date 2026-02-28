# 🏗️ ShelfordBuildPro — Construction Management System

A .NET 10 Web API built with **Clean Architecture** and **Domain-Driven Design (DDD)** principles, modelled around the real-world operations of a Perth-based construction company managing both commercial and residential building projects.

---

## 📋 Table of Contents

- [What This System Does](#what-this-system-does)
- [Architecture Overview](#architecture-overview)
- [Solution Structure](#solution-structure)
- [Domain Layer — Deep Dive](#domain-layer--deep-dive)
  - [Common](#-common)
  - [Enumerations](#-enumerations)
  - [Value Objects](#-value-objects)
  - [Entities](#-entities)
  - [Aggregates](#-aggregates)
  - [Events](#-events)
- [Git Cheat Sheet](#git-cheat-sheet)
- [🆕 Modern C# and .NET 10 Features Used](#-modern-c-and-net-10-features-used)
- [Tech Stack](#tech-stack)

---

## What This System Does

This system manages the full lifecycle of construction projects:

```
Client Enquiry → Feasibility → Quoting → Contract Signed
→ In Progress → Practical Completion → Defects → Complete
```

It handles two types of projects:

| Division | Type | Examples |
|---|---|---|
| **Shelford Constructions** | Commercial | Warehouses, offices, defence, retail, healthcare |
| **Shelford Quality Homes** | Residential | Custom homes, display homes, first home buyers |

---

## Architecture Overview

Clean Architecture means the code is organised in layers like an onion.
Each layer can only talk to the layer **inside** it — never outward.

```
┌─────────────────────────────────────────┐
│              API Layer 🟣                │  ← HTTP Controllers, Swagger
│  ┌───────────────────────────────────┐  │
│  │       Application Layer 🟠        │  │  ← Use Cases, Commands, Queries
│  │  ┌─────────────────────────────┐  │  │
│  │  │   Infrastructure Layer 🔵   │  │  │  ← EF Core, SQL Server, Repos
│  │  │  ┌───────────────────────┐  │  │  │
│  │  │  │    Domain Layer 🔴    │  │  │  │  ← Business Rules (NO dependencies)
│  │  │  └───────────────────────┘  │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

### Dependency Rule
> Dependencies always point **inward**. Domain knows nothing about the database, HTTP, or any framework.

| Project | References | Template |
|---|---|---|
| `Domain` | Nothing ✅ | Class Library |
| `Application` | Domain | Class Library |
| `Infrastructure` | Domain + Application | Class Library |
| `API` | Application + Infrastructure | ASP.NET Core Web API |

---

## Solution Structure

```
ShelfordBuildPro.sln
└── src/
    ├── TestShelfordBuildPro.Domain/          🔴 Business rules
    │   ├── Common/
    │   ├── Enumerations/
    │   ├── ValueObjects/
    │   ├── Entities/
    │   ├── Aggregates/
    │   └── Events/
    ├── TestShelfordBuildPro.Application/     🟠 Use cases
    ├── TestShelfordBuildPro.Infrastructure/  🔵 Database, EF Core
    └── TestShelfordBuildPro.API/             🟣 HTTP Controllers
```

---

## Domain Layer — Deep Dive

The Domain layer is the **brain** of the system.
It contains pure C# with **zero external dependencies** — no NuGet packages, no database, no HTTP.
If the database changes from SQL Server to PostgreSQL, this layer is untouched.

---

### 📁 Common

**Business meaning:** Shared foundation that every domain object builds on.
Like a base contract that every department at Shelford must follow — every record has an ID, an audit trail, and the ability to raise internal notifications.

| File | What it does | Business purpose |
|---|---|---|
| `BaseEntity.cs` | Base class all domain objects inherit | Every Project, Client, Milestone gets a unique ID, audit trail (who created it, when, who last changed it), and domain event support |
| `IDomainEvent.cs` | Empty marker interface | Labels a class as "something that happened" so the system knows to handle it — like tagging an email as urgent |

**Key concepts in these files:**
```csharp
// Every domain object gets these automatically:
public Guid Id { get; }              // Unique identifier
public DateTime CreatedAt { get; }   // When was this created?
public string CreatedBy { get; }     // Who created it?
public DateTime? LastModifiedAt { }  // When was it last changed?
public string? LastModifiedBy { }    // Who last changed it?
```

---

### 📁 Enumerations

**Business meaning:** Fixed lists of options that Shelford uses to classify everything.
Like dropdown menus in a form — the options are predefined and cannot be misspelled.

> **Angular equivalent:** Exactly like TypeScript `enum`. You already know this!

| File | What it does | Business purpose |
|---|---|---|
| `ProjectType.cs` | Commercial, Residential, Civil, Government, Renovation | Which Shelford division handles this project? |
| `ProjectStatus.cs` | Enquiry → Quoting → ContractSigned → InProgress → Complete | Where is this project in its lifecycle right now? |
| `CommercialSector.cs` | Warehouse, Defence, Retail, Healthcare, Education... | What industry is this commercial build for? |
| `HomeType.cs` | SingleStorey, DoubleStorey, CustomLuxury, DisplayHome... | What type of home is Shelford Quality Homes building? |
| `ContractType.cs` | LumpSum, CostPlus, DesignBuild, ManagementFee | How is Shelford being paid for this project? |
| `ConstructionType.cs` | BrickAndSteel, PrecastTiltUp, Formwork, SteelFrame... | What building technique is being used on site? |

**Why not just use strings?**
```csharp
// ❌ BAD — string can be anything, typos cause bugs
project.Status = "in progress";   // is it "in progress", "InProgress", "active"?

// ✅ GOOD — enum is locked, compiler catches mistakes
project.Status = ProjectStatus.InProgress;  // only valid options exist
```

---

### 📁 Value Objects

**Business meaning:** Measurements and descriptors used across the entire business.
A value object has **no identity** — only its value matters.
Two `Money(500, "AUD")` objects are equal because $500 AUD equals $500 AUD.
They are **immutable** — once created, never changed. You replace them entirely.

> **Angular equivalent:** Like a TypeScript `Readonly<interface>` — data only, no tracking.

| File | What it does | Business purpose |
|---|---|---|
| `Money.cs` | Amount + Currency, self-validating | Every contract value, invoice, variation, retention — always AUD with validation |
| `Address.cs` | Street, suburb, state, postcode | Site address for building permits, contracts, subcontractor directions |
| `SiteLocation.cs` | Address + Lot/Plan number + GPS | Full legal site identification required for Council permits and title searches |
| `ContactInfo.cs` | Email + Phone, always together | Client and stakeholder contact — email for documents, phone for site emergencies |
| `DateRange.cs` | Start + End date, validated together | Project timeline — end must always be after start, tracks overdue status |
| `Percentage.cs` | 0-100 validated, with helpers | GST (10%), retention (5%), progress claims (20%, 40%...) |

**Why immutable?**
```csharp
// ❌ BAD — mutable, anyone can change contract value silently
project.ContractValue.Amount = 999; // sneaky bug!

// ✅ GOOD — immutable, must explicitly replace
project.UpdateContractValue(new Money(5_000_000, "AUD"), "ryan.m");
// now we know WHO changed it, WHEN, and the old value is preserved in audit
```

**Built-in math operators on Money:**
```csharp
var contract   = new Money(5_000_000, "AUD");  // $5,000,000
var variation  = new Money(150_000, "AUD");     // +$150,000
var totalValue = contract + variation;          // $5,150,000 ✅
```

---

### 📁 Entities

**Business meaning:** Things that belong TO a project and cannot exist independently.
A Milestone only makes sense in the context of a project. A Variation Order only exists because a project exists.

> **Angular equivalent:** Like child components that only work inside a parent component.

| File | What it does | Business purpose |
|---|---|---|
| `ProjectMilestone.cs` | A key build stage with due date and completion | Slab, Frame, Lock-Up, Fixing, Practical Completion — gates progress claims and inspections |
| `VariationOrder.cs` | A formal change to original scope | Client adds mezzanine floor mid-build = VO-001, +$150k. Must be approved before work proceeds |
| `ProgressClaim.cs` | An invoice based on % completion | Shelford claims payment at each milestone — 20% slab = $1M claim, less 5% retention = $950k paid |

**Why can't you create these directly?**
```csharp
// ❌ This will NOT compile — constructor is internal
var milestone = new ProjectMilestone(...);

// ✅ Must go through the Project aggregate
var milestone = project.AddMilestone("Frame Stage", dueDate, "ryan.m");
// This ensures ALL business rules are enforced by Project
```

---

### 📁 Aggregates

**Business meaning:** The main "things" Shelford cares about — the managers of the system.
An Aggregate Root is the ONLY way to interact with its children.
You never directly modify a Milestone — you always go through its Project.

> **Angular equivalent:** Like a smart container component that owns and controls all its children.

| File | What it does | Business purpose |
|---|---|---|
| `Project.cs` | Abstract base — all shared project behaviour | Holds contract financials, status lifecycle, collections of milestones/variations/claims. Enforces ALL core business rules |
| `CommercialProject.cs` | Shelford Constructions division | Adds: sector, GFA, storeys, DA/permit tracking, performance bonds. Used for warehouses, defence, retail, industrial |
| `ResidentialProject.cs` | Shelford Quality Homes division | Adds: home type, bedrooms/bathrooms, pre-start meeting, HIA contract, lifetime warranty. Used for all home builds |
| `Client.cs` | Anyone who commissions Shelford | Stores legal name, ABN, billing address, contact, credit limit. One client can have many projects |

**The Aggregate enforces all business rules:**
```csharp
// ❌ BAD — no rules enforced, anyone can set anything
project.Status = ProjectStatus.Complete; // skipped Defects period!

// ✅ GOOD — business rules enforced inside the method
project.AdvanceStatus(ProjectStatus.Complete, "ryan.m");
// throws: "Project must complete the Defects period before being marked Complete"
```

**Why `abstract` for Project?**
```csharp
// ❌ You cannot do this — Project is abstract
var project = new Project(...);

// ✅ You must use a concrete type
var commercial   = CommercialProject.Create(...);  // Shelford Constructions
var residential  = ResidentialProject.Create(...); // Shelford Quality Homes
```

---

### 📁 Events

**Business meaning:** Formal announcements that something important happened.
The Domain fires the event. It does NOT know or care what happens next.
The Application layer listens and decides the consequences.

> **Angular equivalent:** Exactly like `EventEmitter` — fires and forgets.

| Event | When it fires | What the Application layer can do |
|---|---|---|
| `ProjectCreatedEvent` | New project created | Notify estimating team, create SharePoint folder, update CRM |
| `ContractSignedEvent` | Contract signed | Send welcome email to client, assign PM, create default milestones |
| `VariationAddedEvent` | Variation order added | Notify contract admin, update financial forecast, send VO to client |
| `PracticalCompletionEvent` | Build is complete | Issue defects list, start 90-day defects timer, arrange final inspection |
| `ProgressClaimRaisedEvent` | Progress claim submitted | Create invoice in Xero/MYOB, update cashflow forecast, notify client |
| `ProjectCompletedEvent` | Project fully signed off | Archive project, send satisfaction survey, release retention funds |
| `ProjectCancelledEvent` | Project cancelled | Notify affected staff, release resources, issue cancellation invoice |

**How events work:**
```csharp
// Inside Project.cs — domain fires the event
protected void SomeMethod() {
    // ... business logic ...
    RaiseDomainEvent(new ContractSignedEvent(Id, ProjectCode, ContractValue));
    // Domain's job is done. It doesn't send emails or update Xero.
}

// Inside Application layer — handler responds to the event
public class ContractSignedEventHandler {
    public async Task Handle(ContractSignedEvent evt) {
        await _emailService.SendWelcomeEmail(evt.ProjectCode);
        await _crmService.UpdatePipeline(evt.ProjectId);
        // Application layer handles the consequences
    }
}
```

---

## Git Cheat Sheet

### First time setup (run once)
```bash
# Initialise git in your project folder
git init

# Connect to your GitHub repository
git remote add origin https://github.com/rbasehewa/construction-management-system.git

# Add .gitignore (ignores bin/, obj/, .vs/ folders)
dotnet new gitignore

# Stage all files
git add .

# First commit
git commit -m "Initial commit"

# Set main as default branch and push
git branch -M main
git push -u origin main
```

### Daily workflow (use these every day)
```bash
# Check what files have changed
git status

# Stage ALL changed files
git add .

# Stage a SPECIFIC file only
git add src/TestShelfordBuildPro.Domain/Aggregates/Project.cs

# Commit with a message describing what you did
git commit -m "Add Project aggregate root with status lifecycle"

# Push to GitHub
git push
```

### Good commit message examples
```bash
git commit -m "Add BaseEntity and IDomainEvent to Domain/Common"
git commit -m "Add Value Objects - Money, Address, SiteLocation"
git commit -m "Add Project aggregate root with business rules"
git commit -m "Add CommercialProject and ResidentialProject"
git commit -m "Add Client aggregate - completes Domain layer"
git commit -m "Add Application layer interfaces and commands"
git commit -m "Add EF Core DbContext and migrations"
```

### Useful commands
```bash
# See all your commits
git log --oneline

# See what changed in a specific file
git diff src/TestShelfordBuildPro.Domain/Aggregates/Project.cs

# Undo staged changes (before commit)
git restore --staged .

# See all branches
git branch

# Create a new branch for a feature
git checkout -b feature/application-layer

# Switch back to main
git checkout main
```

### If you get errors
```bash
# If remote already exists
git remote set-url origin https://github.com/rbasehewa/construction-management-system.git

# If push is rejected (remote has changes you don't have)
git pull origin main --allow-unrelated-histories
git push

# If you want to see what's on remote
git fetch origin
git status
```

---

## 🆕 Modern C# and .NET 10 Features Used

This project takes advantage of the latest C# and .NET 10 language features.
Here's every modern feature used, what it does, and WHERE you'll find it in this codebase.

---

### 1. `record` and `sealed record` — C# 9+ ⭐
**Where used:** All Value Objects — `Money.cs`, `Address.cs`, `SiteLocation.cs`, `ContactInfo.cs`, `DateRange.cs`, `Percentage.cs`

Records are a modern C# type designed specifically for **immutable data objects**.
They automatically generate equality based on VALUES, not object references.

```csharp
// OLD WAY (C# 8 and earlier) — needed 20+ lines to do this:
public class Money {
    public decimal Amount { get; }
    public string Currency { get; }
    public override bool Equals(object obj) { ... }  // manual!
    public override int GetHashCode() { ... }         // manual!
    public static bool operator ==(Money a, Money b) { ... } // manual!
}

// NEW WAY — sealed record (C# 9+, used in .NET 10)
// Equality, immutability and hash code are ALL automatic
public sealed record Money {
    public decimal Amount { get; }
    public string Currency { get; }
    // That's it! C# handles the rest ✅
}

// Two Money objects with same values are automatically equal:
var a = new Money(500, "AUD");
var b = new Money(500, "AUD");
Console.WriteLine(a == b); // true ✅ — no manual Equals() needed
```

> **Angular equivalent:** Like a TypeScript `readonly` interface with built-in deep equality checking.

---

### 2. Nullable Reference Types (`?`) — C# 8+ ⭐
**Where used:** Every file — `string?`, `DateTime?`, `Money?`, `Guid?`

The `?` after a type means "this value CAN be null — I am declaring this intentionally".
Without `?`, the compiler WARNS you if something could be null unexpectedly.
This eliminates a whole class of runtime `NullReferenceException` bugs at compile time.

```csharp
// OLD WAY — no warnings, null crashes at runtime
public string TradingName { get; }   // could be null — no compiler warning ❌

// NEW WAY — compiler knows this is intentionally nullable
public string? TradingName { get; }  // ? = "this can be null on purpose" ✅
public string LegalName { get; }     // no ? = "this must NEVER be null" ✅

// Compiler catches this before you even run the code:
string? mobile = null;
Console.WriteLine(mobile.Length); // ❌ compiler warning: possible null!
Console.WriteLine(mobile?.Length); // ✅ safe null conditional operator
```

> **Angular equivalent:** Exactly like TypeScript optional properties: `tradingName?: string`

---

### 3. Null Conditional Operator (`?.`) — C# 6+ ⭐
**Where used:** `ContactInfo.cs`, `Client.cs`, `SiteLocation.cs`

Safely accesses a property or method only if the object is not null.
If null, returns null instead of throwing an exception.

```csharp
// OLD WAY — verbose null check
string result = mobile != null ? mobile.Trim() : null;

// NEW WAY — null conditional
string? result = mobile?.Trim(); // if mobile is null → result is null, no crash ✅

// Chaining — works through multiple levels
string? suburb = project?.SiteLocation?.Address?.Suburb;
```

> **Angular equivalent:** Exactly like the Angular template `{{ project?.name }}` safe navigation operator.

---

### 4. Null Coalescing Operator (`??`) — C# 6+ ⭐
**Where used:** `Address.cs`, `Client.cs`, `SiteLocation.cs`

Returns the left side if not null, otherwise returns the right side.

```csharp
// OLD WAY
string state = inputState != null ? inputState : "WA";

// NEW WAY
string state = inputState ?? "WA"; // if null → use "WA" as default ✅

// Null coalescing assignment (C# 8+)
_tradingName ??= "Unknown"; // only assigns if currently null
```

> **Angular equivalent:** Like the JavaScript `||` operator: `state || 'WA'`

---

### 5. Expression Body Members (`=>`) — C# 6+ ⭐
**Where used:** Computed properties in every Value Object and Aggregate

Single-line properties and methods written as expressions — cleaner than full `{ get { return ... } }` syntax.

```csharp
// OLD WAY — verbose
public string FullAddress {
    get {
        return $"{StreetNumber} {StreetName}, {Suburb} {State} {PostCode}";
    }
}

// NEW WAY — expression body (used throughout this project)
public string FullAddress =>
    $"{StreetNumber} {StreetName}, {Suburb} {State} {PostCode}"; // ✅ clean!

// Same for methods:
public void ClearDomainEvents() => _domainEvents.Clear(); // ✅ one liner
```

> **Angular equivalent:** Like TypeScript arrow functions: `getFullAddress = () => \`${this.street}...\``

---

### 6. `switch` Expression — C# 8+ ⭐
**Where used:** Application layer commands (coming in Phase 3)

Modern switch that returns a value directly — no `break` statements needed.

```csharp
// OLD WAY — verbose switch statement
string prefix;
switch(type) {
    case ProjectType.Commercial:
        prefix = "COMM";
        break;
    case ProjectType.Residential:
        prefix = "RES";
        break;
    default:
        prefix = "PRJ";
        break;
}

// NEW WAY — switch expression (C# 8+)
var prefix = type switch {
    ProjectType.Commercial  => "COMM",
    ProjectType.Residential => "RES",
    ProjectType.Civil       => "CIV",
    _                       => "PRJ"  // _ is the default case
}; // ✅ clean, returns a value directly
```

> **Angular equivalent:** Like a TypeScript ternary chain or object map lookup.

---

### 7. `nameof()` Operator — C# 6+ ⭐
**Where used:** All argument validation in constructors

Gets the NAME of a variable as a string — refactor-safe.
If you rename a parameter, `nameof()` updates automatically.

```csharp
// OLD WAY — hardcoded string, breaks on rename
throw new ArgumentException("Value is invalid.", "contractValue");

// NEW WAY — nameof() (used throughout this project)
throw new ArgumentException(
    "Contract value must be greater than zero.",
    nameof(contractValue)); // ✅ if you rename the param, this updates too!
```

---

### 8. String Interpolation (`$""`) — C# 6+ ⭐
**Where used:** `ToString()` overrides in every Value Object

Embed variables directly in strings — no more `string.Format()` or concatenation.

```csharp
// OLD WAY
string display = currency + " " + amount.ToString("N2");

// NEW WAY — string interpolation
string display = $"{Currency} {Amount:N2}";  // ✅ clean and readable

// With format specifiers:
$"{Start:dd MMM yyyy}"         // "15 Mar 2025"
$"{Amount:N2}"                 // "5,000,000.00"
$"{Value}%"                    // "20%"
```

> **Angular equivalent:** Exactly like template literals in TypeScript: `` `${currency} ${amount}` ``

---

### 9. `readonly` Collections — C# ⭐
**Where used:** `Project.cs` — child collections (Milestones, Variations, Claims)

Exposes a collection for reading without allowing external modification.
Internal code can still modify the private backing list.

```csharp
// Private list — only Project can modify this
private readonly List<ProjectMilestone> _milestones = new();

// Public read-only view — external code can READ but not ADD
public IReadOnlyList<ProjectMilestone> Milestones =>
    _milestones.AsReadOnly();

// External code trying to add directly:
project.Milestones.Add(new ProjectMilestone()); // ❌ compiler error!

// Must go through the Project method:
project.AddMilestone("Frame Stage", dueDate, "ryan.m"); // ✅
```

---

### 10. `required` Keyword — C# 11+ / .NET 7+ ⭐⭐ NEW
**Where used:** Will be used in Application DTOs (Phase 3)

Forces the caller to set a property — compile-time safety for object initializers.

```csharp
// C# 11+ required keyword
public class CreateProjectRequest {
    public required string Name { get; init; }         // MUST be set
    public required decimal ContractValue { get; init; } // MUST be set
    public string? Description { get; init; }          // optional
}

// Compiler error if you forget required properties:
var request = new CreateProjectRequest {
    // forgot Name — ❌ compiler error before you even run!
    ContractValue = 5_000_000
};
```

---

### 11. `init` Property Accessor — C# 9+ ⭐
**Where used:** DTOs and request objects in Application layer (Phase 3)

Like `set` but can ONLY be called during object initialisation.
After the object is created, the property is frozen.

```csharp
public class ProjectDto {
    public string Name { get; init; }  // set once at creation, then immutable
}

var dto = new ProjectDto { Name = "Fenner Conveyors" }; // ✅ allowed
dto.Name = "Something else"; // ❌ compiler error — init only!
```

---

### 12. Target-Typed `new()` — C# 9+ ⭐
**Where used:** Throughout — `new()` without repeating the type name

When the type is already known from context, you can use `new()` without repeating it.

```csharp
// OLD WAY — repeating the type name twice
private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

// NEW WAY — target-typed new (C# 9+)
private readonly List<IDomainEvent> _domainEvents = new(); // ✅ cleaner!

// Works in method calls too:
Money contractValue = new(5_000_000, "AUD"); // type inferred from left side
```

---

### 13. Minimal APIs — .NET 6+ / .NET 10 ⭐⭐ NEW
**Where used:** `Program.cs` in API project

.NET 10 uses a single `Program.cs` with no `Startup.cs`.
Massively reduces boilerplate compared to older .NET versions.

```csharp
// OLD WAY (.NET 5 and earlier) — needed Startup.cs + Program.cs
public class Startup {
    public void ConfigureServices(IServiceCollection services) { ... }
    public void Configure(IApplicationBuilder app) { ... }
}

// NEW WAY (.NET 10) — everything in ONE file, top to bottom
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();           // add services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.Run(); // ✅ clean, simple, no Startup.cs needed
```

---

### 14. Global Usings — C# 10+ / .NET 6+ ⭐
**Where used:** Auto-generated by .NET 10 project template

Common namespaces are automatically imported across ALL files.
You never need to write `using System;` or `using System.Collections.Generic;` again.

```csharp
// OLD WAY — needed at top of EVERY file
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// NEW WAY (.NET 10) — these are global, available everywhere automatically ✅
// Just add your project-specific usings:
using TestShelfordBuildPro.Domain.ValueObjects;
using TestShelfordBuildPro.Domain.Enumerations;
```

---

### 15. `_` Discard Pattern — C# 7+ ⭐
**Where used:** Switch expressions default case, unused variables

The underscore `_` means "I don't care about this value".

```csharp
// In switch expressions — default case
var prefix = type switch {
    ProjectType.Commercial => "COMM",
    ProjectType.Residential => "RES",
    _ => "PRJ"  // _ = "everything else" ✅
};
```

---

### Summary Table

| Feature | C# Version | .NET Version | Used In |
|---|---|---|---|
| `sealed record` | C# 9 | .NET 5+ | All Value Objects |
| Nullable `?` | C# 8 | .NET 3+ | Every file |
| `?.` null conditional | C# 6 | .NET 4.6+ | ContactInfo, Client |
| `??` null coalescing | C# 6 | .NET 4.6+ | Address, SiteLocation |
| `=>` expression body | C# 6 | .NET 4.6+ | All computed properties |
| `switch` expression | C# 8 | .NET 3+ | Commands (Phase 3) |
| `nameof()` | C# 6 | .NET 4.6+ | All constructors |
| `$""` interpolation | C# 6 | .NET 4.6+ | All ToString() methods |
| `IReadOnlyList<T>` | C# 5 | .NET 4.5+ | Project collections |
| `required` keyword | **C# 11** | **.NET 7+** | DTOs (Phase 3) |
| `init` accessor | **C# 9** | **.NET 5+** | DTOs (Phase 3) |
| Target-typed `new()` | **C# 9** | **.NET 5+** | Throughout |
| Minimal APIs | **C# 10** | **.NET 6+** | Program.cs |
| Global usings | **C# 10** | **.NET 6+** | Auto-generated |
| `_` discard | C# 7 | .NET 4.6+ | Switch expressions |

> **Bold** = features that did NOT exist in older .NET (Framework 4.x). These are the ones that make .NET 10 code look and feel very different from old C#.

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET | 10.0 | Runtime and framework |
| C# | 13 | Programming language |
| ASP.NET Core | 10.0 | Web API framework |
| Entity Framework Core | 10.x | ORM — maps C# to SQL tables |
| SQL Server | 2022 | Database |
| Stored Procedures | — | Complex queries and reports |
| MediatR | 12.x | Command/Query dispatcher |
| FluentValidation | 11.x | Input validation |
| Swagger / OpenAPI | — | API documentation and testing |

---

## Progress

| Phase | Status |
|---|---|
| ✅ Phase 1 — Solution Setup | Complete |
| ✅ Phase 2 — Domain Layer | Complete |
| ⏳ Phase 3 — Application Layer | Up next |
| ⏳ Phase 4 — Infrastructure (EF Core + SQL Server + Stored Procedures) | Pending |
| ⏳ Phase 5 — API Controllers | Pending |
| ⏳ Phase 6 — End-to-End Testing | Pending |

---

*Built as a portfolio and learning project demonstrating Clean Architecture and DDD principles in .NET 10.*
