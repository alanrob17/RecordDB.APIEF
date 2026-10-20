# RecordDB.APIEF

![.NET 10](https://img.shields.io/badge/.NET%2010-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![C#](https://img.shields.io/badge/C%23%2013-239120?logo=csharp&logoColor=white&style=for-the-badge)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core%2010-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![Microsoft SQL Server](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC292B?logo=microsoftsqlserver&logoColor=white&style=for-the-badge)
![Scalar](https://img.shields.io/badge/Scalar-API%20Reference-E9573F?style=for-the-badge)
![ReDoc](https://img.shields.io/badge/ReDoc-API%20Docs-263238?logo=openapiinitiative&logoColor=white&style=for-the-badge)
![Swagger](https://img.shields.io/badge/Swagger%20%2F%20OpenAPI-85EA2D?logo=swagger&logoColor=black&style=for-the-badge)

A standalone, high-performance **.NET 10 Web API** for the **RecordDB** music catalog system, powered by **Entity Framework Core 10** and **Microsoft SQL Server**.

---

## Overview

**RecordDB.APIEF** is a full port of the original Dapper-based `RecordDB.API` to **Entity Framework Core**. It manages a comprehensive music collection database spanning **Artists**, **Records (Albums)**, **Discs**, and **Tracks**. 

While the original system relied on legacy SQL Server stored procedures and manual ADO.NET/Dapper object mapping, this project completely re-engineers the data access layer into idiomatic **EF Core 10 LINQ queries**, **Fluent API entity configurations**, **Change Tracking**, and **batched database operations**.

Crucially, RecordDB.APIEF delivers **100% contract, route, parameter, and response payload parity** with the original API across all **46 RESTful endpoints**. Existing frontend consumers and API integrations can switch seamlessly to this implementation without modifying a single client request.

---

## Features & Technologies Used

### Runtime & Framework
* **.NET 10 & C# 13**: Built on the latest .NET 10 runtime using modern C# features, including primary constructors (`class ArtistController(IArtistRepository artistRepository)`), collection expressions (`[]`), pattern matching, and nullable reference types (`#nullable enable`).
* **ASP.NET Core Web API**: Controller-based architecture with explicit attribute routing, strongly-typed `ActionResult<T>` responses, and standardized HTTP status codes (`200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`, `404 NotFound`).

### Data Access & ORM
* **Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`)**: Replaces 30+ stored procedures with strongly-typed, compiled LINQ queries while maintaining query plan efficiency.
* **Fluent API Configuration**: Entity configurations are decoupled into dedicated classes implementing `IEntityTypeConfiguration<T>` in `Data/Configurations/`, separating persistence concerns from domain models.
* **Direct Set Execution (`ExecuteDeleteAsync` / `ExecuteUpdateAsync`)**: High-performance bulk deletions and field updates (such as disc length patching) run directly in SQL Server without fetching or tracking entities in memory.
* **Automatic Batching**: Track creation leverages EF Core's built-in multi-row `INSERT` batching, replacing SQL Server Table-Valued Parameters (TVPs) while preserving throughput.
* **Change Tracking & Transactions**: Relational graph inserts (such as automatically scaffolding child `Disc` entities when a `Record` is inserted) are executed atomically in a single `SaveChangesAsync()` transaction.

### Database Engine
* **Microsoft SQL Server**: Connects to the `RecordDB` database schema, mapping legacy SQL Server types (`money`, `text`, `datetime`, `IDENTITY(1,1)`) to modern .NET primitives.
* **Legacy SQL Compatibility**: Implements specialized LINQ patterns (`EF.Functions.Like`, `EF.Functions.DataLength`) to handle SQL Server's legacy `text` LOB column limitations without requiring destructive database schema alterations.

### Architecture & Security
* **Repository Pattern**: Strict abstraction through repository interfaces (`IArtistRepository`, `IRecordRepository`, `IDiscRepository`, `ITrackRepository`) injected with a `Scoped` lifetime per HTTP request.
* **Dedicated DTOs & Over-Posting Defense**: Incoming request bodies (`CreateRecordDto`, `UpdateTrackDto`, etc.) and outgoing responses (`ArtistRecordDto`, `ArtistRecordDiscTrackDto`, etc.) are separated from internal database entities, eliminating circular serialization references and preventing mass-assignment vulnerabilities.
* **Data Annotations Validation**: Model attributes (`[Required]`, `[MaxLength]`) enforce client input validation before reaching the database layer.
* **Vulnerability-Hardened Dependencies**: Explicit package referencing guarantees secure, audit-clean dependencies (`System.Security.Cryptography.Pkcs 10.0.13`), ensuring 0 security audit warnings.

### Documentation & Developer Experience
* **Triple Interactive Documentation UIs**:
  * **Scalar API Reference** (`/scalar/v1`, with root `/` redirection) with the modern *Mars* theme.
  * **Swagger UI** (`/swagger`) powered by Swashbuckle and OpenAPI specifications.
  * **ReDoc** (`/redoc`) for three-panel technical documentation.
* **XML Code Documentation**: Comprehensive XML docstrings on models, DTOs, and endpoints feed into OpenAPI specifications.
* **HTTP Test Suite (`RecordDB.APIEF.http`)**: Pre-configured REST client script for Visual Studio and VS Code to execute and debug all endpoints across all layers with one click.
* **Side-by-Side Coexistence**: Configured on dedicated ports (`7097` HTTPS / `5184` HTTP) so both the Dapper and EF Core APIs can run simultaneously on the same machine.

---

## Project Structure

```
RecordDB.APIEF/
├── RecordDB.APIEF.slnx                   # Modern .NET 10 solution file
├── README.md                              # Full project documentation
└── RecordDB.APIEF/
    ├── RecordDB.APIEF.csproj              # Project file with EF Core & documentation packages
    ├── Program.cs                         # DI configuration, EF Core DbContext, & Swagger/Scalar
    ├── appsettings.json                   # Connection string pointing to RecordDb database
    ├── appsettings.Development.json
    ├── RecordDB.APIEF.http                # Interactive REST client test suite for all endpoints
    ├── Properties/
    │   └── launchSettings.json            # Dedicated ports (7097 HTTPS / 5184 HTTP)
    ├── Controllers/
    │   ├── ArtistController.cs            # 11 endpoints: Artist CRUD, bios, and search
    │   ├── RecordController.cs            # 17 endpoints: Record CRUD, reviews, totals, and counts
    │   ├── DiscController.cs              # 7 endpoints: Disc CRUD, length patching, search
    │   └── TrackController.cs             # 11 endpoints: Track CRUD, search, and bulk operations
    ├── Repositories/
    │   ├── IArtistRepository.cs / ArtistRepository.cs
    │   ├── IRecordRepository.cs / RecordRepository.cs
    │   ├── IDiscRepository.cs   / DiscRepository.cs
    │   └── ITrackRepository.cs  / TrackRepository.cs
    ├── Data/
    │   ├── RecordDbContext.cs             # EF Core DbContext exposing all 4 DbSets
    │   └── Configurations/                # Fluent API EntityTypeConfiguration mappings
    │       ├── ArtistConfiguration.cs     # Maps dbo.Artist (text Biography, Identity PK)
    │       ├── RecordConfiguration.cs     # Maps dbo.Record (money Cost, text Review, datetime)
    │       ├── DiscConfiguration.cs       # Maps dbo.Disc
    │       └── TrackConfiguration.cs      # Maps dbo.Track
    ├── Models/
    │   ├── Artist.cs                      # Domain model with ICollection<Record> navigation
    │   ├── Record.cs                      # Domain model with Artist and DiscsList navigations
    │   ├── Disc.cs                        # Domain model with Record and Tracks navigations
    │   ├── Track.cs                       # Domain model with Disc navigation
    │   └── Total.cs                       # Projection model for artist financial/disc totals
    ├── DTOs/
    │   ├── ArtistDto.cs / CreateArtistDto.cs / UpdateArtistDto.cs
    │   ├── CreateRecordDto.cs / UpdateRecordDto.cs
    │   ├── CreateDiscDto.cs / UpdateDiscDto.cs / UpdateDiscLengthDto.cs
    │   ├── CreateTrackDto.cs / UpdateTrackDto.cs
    │   ├── ArtistRecordDto.cs             # Flat projection joining Artist + Record
    │   ├── ArtistRecordDiscDto.cs         # Flat projection joining Artist + Record + Disc
    │   ├── ArtistRecordDiscTrackDto.cs    # Full projection joining Artist + Record + Disc + Track
    │   ├── RecordReviewDto.cs             # Slim projection for reviews
    │   └── MissingReviewDto.cs            # Slim projection for records missing reviews
    └── Extensions/
        └── DateTimeExtensions.cs          # Culture-invariant date formatting helpers
```

---

## Technical Highlights

### 1. Solving Legacy SQL Server `text` Data Type Limitations
The existing `RecordDB` schema defines `Artist.Biography` and `Record.Review` using SQL Server's legacy `text` data type (`[Biography] [text] NULL`, `[Review] [text] NULL`). In Microsoft SQL Server, columns of type `text` **cannot be compared using the equality (`=`) operator**:
```
Msg 402, Level 16, State 1: The data types text and varchar are incompatible in the equal to operator.
```
In EF Core, standard C# expressions such as `a.Biography == ""` translate to `WHERE [a].[Biography] = N''`, triggering Error 402.

**Solution**:
We leveraged SQL Server pattern matching and data length functions natively in LINQ:
```csharp
// Queries records missing a biography without triggering Error 402:
.Where(a => a.Biography == null || EF.Functions.Like(a.Biography, ""))

// Queries records missing reviews or having reviews shorter than 5 characters:
.Where(r => r.Review == null || EF.Functions.DataLength(r.Review) < 5)
```
This produces valid Transact-SQL (`LIKE N''` and `DATALENGTH([r].[Review]) < 5`), perfectly matching the database semantics without altering existing table structures.

---

### 2. Preventing SQL Server `datetime` Out-of-Range Exceptions
SQL Server's legacy `datetime` data type has a valid range of **January 1, 1753, through December 31, 9999**. .NET's `DateTime.MinValue`, however, represents **January 1, 0001**.

When DTOs had non-nullable `DateTime Bought` properties, projection code like `Bought = r.Bought ?? DateTime.MinValue` forced EF Core to emit:
```sql
COALESCE([r].[Bought], '0001-01-01T00:00:00.000') AS [Bought]
```
This caused SQL Server error 242 (*"The conversion of a varchar data type to a datetime data type resulted in an out-of-range value"*) whenever a record had a `NULL` purchase date (519 records in the database).

**Solution**:
We aligned all DTOs and projections to use nullable `DateTime? Bought`. EF Core translates this directly to `[r].[Bought] AS [Bought]`, allowing `NULL` values to pass cleanly to the JSON serializer without SQL Server conversion errors.

---

### 3. Automated Relational Graph Construction
In the legacy stored procedure `adm_RecordInsert`, creating a record with `Discs = 2` executed a `WHILE` loop to insert 2 rows into `dbo.Disc`.

In EF Core, `RecordRepository.InsertAsync` handles this natively:
```csharp
for (int i = 1; i <= record.Discs; i++)
{
    record.DiscsList.Add(new Disc { DiscNo = i });
}

_context.Records.Add(record);
await _context.SaveChangesAsync();
return record.RecordId;
```
EF Core inserts the `Record`, retrieves the generated IDENTITY `RecordId`, populates `Disc.RecordId` for every child disc, and executes the child inserts within a single atomic transaction.

---

### 4. Dynamic Show Filtering
The original stored procedure `up_RecordSelectShowCore` contained 15+ hardcoded `IF/ELSE` branches for media types (`cd`, `records`, `dvds`, `blurays`, `all`) and specific past purchase years (`2022`, `2021`, etc.).

In EF Core, `RecordRepository` implements dynamic, future-proof query building:
```csharp
if (int.TryParse(show, out var year))
{
    return (query.Where(r => r.Bought.HasValue && r.Bought.Value.Year == year), true);
}
```
Any past or future year is handled dynamically while preserving identical sorting logic (`ORDER BY Bought DESC` for media/year queries; `ORDER BY LastName, FirstName, Recorded DESC` for general queries).

---

### 5. Multi-Row Batch Track Creation
The Dapper solution required passing a custom SQL Server Table-Valued Parameter (`dbo.TrackTableType`) mapped through a manually constructed ADO.NET `DataTable`.

In EF Core, `BulkInsertTracksAsync` utilizes `_context.Tracks.AddRangeAsync(tracks)`, which EF Core 10 automatically optimizes into batched multi-row `INSERT INTO Track (...) VALUES (...), (...)` statements without manual ADO.NET boilerplate.

---

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or later)
* Microsoft SQL Server instance (local or remote) with the `RecordDB` database installed.

### Configuration

Open `RecordDB.APIEF/appsettings.json` and ensure your SQL Server connection string is configured:

```json
{
  "ConnectionStrings": {
    "RecordDb": "Server=YOUR_SERVER;Initial Catalog=RecordDB;Persist Security Info=False;User ID=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Note on `TrustServerCertificate=True`**: When connecting to development SQL Server instances with self-signed certificates, `TrustServerCertificate=True` is required by `Microsoft.Data.SqlClient` to bypass certificate chain validation.

---

## How to Run the API

### 1. Build the Solution

From PowerShell or terminal in the repository root:

```powershell
dotnet build D:\Projects\RecordDB.APIEF\RecordDB.APIEF.slnx
```

### 2. Run the Web API

```powershell
dotnet run --project D:\Projects\RecordDB.APIEF\RecordDB.APIEF
```

Once started, the API will listen on the dedicated ports:
* **HTTPS**: `https://localhost:7097`
* **HTTP**: `http://localhost:5184`

*(Note: These ports are configured in `Properties/launchSettings.json` specifically so you can run the Dapper API on `7096`/`5183` and this EF Core API on `7097`/`5184` at the same time).*

---

### 3. Explore the Interactive API Documentation

Once the application is running, open your browser to:

| Documentation Tool | URL | Description |
|---|---|---|
| **Scalar API Reference** | `https://localhost:7097/scalar/v1` *(or root `https://localhost:7097/`)* | Modern, interactive API client with Mars theme and request generation. |
| **Swagger UI** | `https://localhost:7097/swagger` | Classic OpenAPI interactive documentation. |
| **ReDoc** | `https://localhost:7097/redoc` | Clean, 3-panel technical API documentation. |
| **OpenAPI Specification** | `https://localhost:7097/openapi/v1.json` | Raw OpenAPI v1 JSON document. |

---

### 4. Testing with the REST Client (`.http`)

The project includes a ready-to-run HTTP test file:
[`RecordDB.APIEF/RecordDB.APIEF.http`](file:///D:/Projects/RecordDB.APIEF/RecordDB.APIEF/RecordDB.APIEF.http)

If you are using **Visual Studio** or **VS Code** (with the *REST Client* extension), you can open this file and click **"Send Request"** directly above any endpoint to test GET, POST, PUT, PATCH, and DELETE requests against the live API.

---

## Complete API Endpoints Reference (46 Endpoints)

### 1. Artist Layer (`/api/artist`) — 11 Endpoints

| HTTP Method | Route | Description |
|---|---|---|
| `GET` | `/api/artist` | Retrieve all artists ordered by LastName, FirstName |
| `GET` | `/api/artist/{id}` | Retrieve a single artist by ID |
| `GET` | `/api/artist/search/{name}` | Search artists whose name contains partial string |
| `GET` | `/api/artist/no-biography` | Retrieve all artists with missing or blank biography |
| `GET` | `/api/artist/no-biography/{name}` | Retrieve an artist missing a biography by exact name |
| `GET` | `/api/artist/by-record/{recordId}` | Retrieve the artist associated with a specific record |
| `GET` | `/api/artist/biography/{recordId}` | Retrieve the biography text for a record's artist |
| `GET` | `/api/artist/artist-id?firstName=&lastName=` | Look up an `ArtistId` by first and last name prefix |
| `POST` | `/api/artist` | Create a new artist (`CreateArtistDto`) |
| `PUT` | `/api/artist/{id}` | Update an existing artist (`UpdateArtistDto`) |
| `DELETE` | `/api/artist/{id}` | Delete an artist by ID |

---

### 2. Record Layer (`/api/record`) — 17 Endpoints

| HTTP Method | Route | Description |
|---|---|---|
| `GET` | `/api/record` | Retrieve all records with artist details |
| `GET` | `/api/record/{id}` | Retrieve a single record with artist details by ID |
| `GET` | `/api/record/show/{show}` | Filter records by show flag (`cd`, `records`, `dvds`, `blurays`, `all`, or year) |
| `GET` | `/api/record/by-artist/{name}` | Retrieve records by exact artist name |
| `GET` | `/api/record/by-artist-id/{artistId}` | Retrieve records for an artist ID as flat `Record` objects |
| `GET` | `/api/record/by-year/{year}` | Retrieve records for a specific recording year |
| `GET` | `/api/record/reviews` | Retrieve all records having reviews |
| `GET` | `/api/record/no-reviews` | Retrieve all records missing reviews |
| `GET` | `/api/record/no-tracks` | Retrieve all records and discs that have no tracks |
| `GET` | `/api/record/no-tracks/{name}` | Retrieve records/discs with no tracks for an artist |
| `GET` | `/api/record/tracks/search/{name}` | Search tracks with full artist, record, and disc context |
| `GET` | `/api/record/totals` | Retrieve per-artist disc count and cost totals |
| `GET` | `/api/record/disc-count/{show}` | Count total discs by show flag |
| `GET` | `/api/record/count/artist/{artistId}` | Count total discs for an artist ID |
| `GET` | `/api/record/count/year/{year}` | Count total discs for a recording year |
| `POST` | `/api/record` | Create a new record and scaffold child discs (`CreateRecordDto`) |
| `PUT` | `/api/record/{id}` | Update an existing record (`UpdateRecordDto`) |
| `DELETE` | `/api/record/{id}` | Delete a record and its associated discs/tracks by ID |

---

### 3. Disc Layer (`/api/disc`) — 7 Endpoints

| HTTP Method | Route | Description |
|---|---|---|
| `GET` | `/api/disc` | Retrieve all discs with artist and record context |
| `GET` | `/api/disc/{id}` | Retrieve a single disc with context by ID |
| `GET` | `/api/disc/search/{name}` | Search discs for records whose name contains string |
| `POST` | `/api/disc` | Create a new disc (`CreateDiscDto`) |
| `PUT` | `/api/disc/{id}` | Update an existing disc (`UpdateDiscDto`) |
| `PATCH` | `/api/disc/{id}/length` | Partially update only the disc's length (`UpdateDiscLengthDto`) |
| `DELETE` | `/api/disc/{id}` | Delete a disc and its tracks by ID |

---

### 4. Track Layer (`/api/track`) — 11 Endpoints

| HTTP Method | Route | Description |
|---|---|---|
| `GET` | `/api/track` | Retrieve all tracks with full artist, record, and disc context |
| `GET` | `/api/track/{id}` | Retrieve a single track by ID |
| `GET` | `/api/track/search/{name}` | Search tracks whose name contains partial string |
| `GET` | `/api/track/by-artist/{name}` | Retrieve all tracks for records by artist name |
| `GET` | `/api/track/by-record/{name}` | Retrieve all tracks for a specific record by record name |
| `GET` | `/api/track/count/record/{recordId}` | Get total track count for a record ID |
| `GET` | `/api/track/check/disc/{discId}` | Check existing track count for a disc ID |
| `POST` | `/api/track` | Create a single track (`CreateTrackDto`) |
| `POST` | `/api/track/bulk` | Bulk-insert multiple tracks (`IEnumerable<CreateTrackDto>`) |
| `PUT` | `/api/track/{id}` | Update an existing track (`UpdateTrackDto`) |
| `DELETE` | `/api/track/{id}` | Delete a track by ID |
