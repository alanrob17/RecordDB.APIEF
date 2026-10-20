# RecordDB.APIEF

![.NET 10](https://img.shields.io/badge/.NET%2010-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![C#](https://img.shields.io/badge/C%23%2013-239120?logo=csharp&logoColor=white&style=for-the-badge)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![Microsoft SQL Server](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC292B?logo=microsoftsqlserver&logoColor=white&style=for-the-badge)
![Scalar](https://img.shields.io/badge/Scalar-API%20Reference-E9573F?style=for-the-badge)
![ReDoc](https://img.shields.io/badge/ReDoc-API%20Docs-263238?logo=openapiinitiative&logoColor=white&style=for-the-badge)
![Swagger](https://img.shields.io/badge/Swagger%20%2F%20OpenAPI-85EA2D?logo=swagger&logoColor=black&style=for-the-badge)

A standalone, modern, high-performance **.NET 10 Web API** for the **RecordDB** music catalog system built entirely with **Entity Framework Core 10**.

This project ports the whole data access layer from Dapper and stored procedures to idiomatic EF Core LINQ queries, change tracking, navigation properties, and Fluent API configurations while maintaining 100% endpoint, route, DTO, and contract parity with the original API.

---

## Migration Status: 100% Complete

- [x] **Artist Layer** (`/api/artist`) — 11 endpoints
- [x] **Record Layer** (`/api/record`) — 17 endpoints
- [x] **Disc Layer** (`/api/disc`) — 7 endpoints
- [x] **Track Layer** (`/api/track`) — 11 endpoints

---

## Project Structure

```
RecordDB.APIEF/
├── RecordDB.APIEF.slnx                   # Solution file (.NET 10 format)
└── RecordDB.APIEF/
    ├── Program.cs                        # DI setup, EF Core DbContext, & Swagger/Scalar
    ├── appsettings.json                  # Database connection string & logging config
    ├── RecordDB.APIEF.http               # Complete integration test suite
    ├── Controllers/
    │   ├── ArtistController.cs           # Artist CRUD, bios, and search endpoints
    │   ├── RecordController.cs           # Record CRUD, reviews, totals, and counts
    │   ├── DiscController.cs             # Disc CRUD and length patching
    │   └── TrackController.cs            # Track CRUD, search, and bulk operations
    ├── Repositories/
    │   ├── IArtistRepository.cs / ArtistRepository.cs
    │   ├── IRecordRepository.cs / RecordRepository.cs
    │   ├── IDiscRepository.cs   / DiscRepository.cs
    │   └── ITrackRepository.cs  / TrackRepository.cs
    ├── Data/
    │   ├── RecordDbContext.cs            # EF Core DbContext
    │   └── Configurations/
    │       ├── ArtistConfiguration.cs    # Fluent API mappings for Artist
    │       ├── RecordConfiguration.cs    # Fluent API mappings for Record
    │       ├── DiscConfiguration.cs      # Fluent API mappings for Disc
    │       └── TrackConfiguration.cs     # Fluent API mappings for Track
    ├── Models/
    │   ├── Artist.cs
    │   ├── Record.cs
    │   ├── Disc.cs
    │   ├── Track.cs
    │   └── Total.cs
    ├── DTOs/
    │   ├── ArtistDto.cs / CreateArtistDto.cs / UpdateArtistDto.cs
    │   ├── CreateRecordDto.cs / UpdateRecordDto.cs
    │   ├── CreateDiscDto.cs / UpdateDiscDto.cs / UpdateDiscLengthDto.cs
    │   ├── CreateTrackDto.cs / UpdateTrackDto.cs
    │   ├── ArtistRecordDto.cs
    │   ├── ArtistRecordDiscDto.cs
    │   ├── ArtistRecordDiscTrackDto.cs
    │   ├── RecordReviewDto.cs
    │   └── MissingReviewDto.cs
    └── Extensions/
        └── DateTimeExtensions.cs
```

---

## Running the API

```powershell
# Build solution
dotnet build D:\Projects\RecordDB.APIEF\RecordDB.APIEF.slnx

# Run the API
dotnet run --project D:\Projects\RecordDB.APIEF\RecordDB.APIEF
```

Once running:
- **Scalar API Reference**: `https://localhost:7097/scalar/v1` (or root `https://localhost:7097/`)
- **ReDoc**: `https://localhost:7097/redoc`
- **Swagger UI**: `https://localhost:7097/swagger`
- **OpenAPI Specification**: `https://localhost:7097/openapi/v1.json`

---

## API Endpoints Reference

### 1. Artist (`/api/artist`)

| HTTP Method | Route | Description |
|---|---|---|
| `GET` | `/api/artist` | Retrieve all artists |
| `GET` | `/api/artist/{id}` | Retrieve a single artist by ID |
| `GET` | `/api/artist/search/{name}` | Search artists by partial name |
| `GET` | `/api/artist/no-biography` | Retrieve all artists with missing biography |
| `GET` | `/api/artist/no-biography/{name}` | Retrieve an artist with no biography by name |
| `GET` | `/api/artist/by-record/{recordId}` | Retrieve the artist for a specific record |
| `GET` | `/api/artist/biography/{recordId}` | Retrieve the biography text for a record's artist |
| `GET` | `/api/artist/artist-id?firstName=&lastName=` | Look up an `ArtistId` by first and last name |
| `POST` | `/api/artist` | Create a new artist (`CreateArtistDto`) |
| `PUT` | `/api/artist/{id}` | Update an existing artist (`UpdateArtistDto`) |
| `DELETE` | `/api/artist/{id}` | Delete an artist by ID |

### 2. Record (`/api/record`)

| HTTP Method | Route | Description |
|---|---|---|
| `GET` | `/api/record` | Retrieve all records with artist details |
| `GET` | `/api/record/{id}` | Retrieve a single record with artist details by ID |
| `GET` | `/api/record/show/{show}` | Filter records by show flag (`cd`, `records`, `dvds`, `blurays`, `all`, or year) |
| `GET` | `/api/record/by-artist/{name}` | Retrieve records by artist name |
| `GET` | `/api/record/by-artist-id/{artistId}` | Retrieve records for an artist ID |
| `GET` | `/api/record/by-year/{year}` | Retrieve records by recording year |
| `GET` | `/api/record/reviews` | Retrieve all records having reviews |
| `GET` | `/api/record/no-reviews` | Retrieve all records missing reviews |
| `GET` | `/api/record/no-tracks` | Retrieve all records that have no tracks |
| `GET` | `/api/record/no-tracks/{name}` | Retrieve records with no tracks for a specific artist |
| `GET` | `/api/record/tracks/search/{name}` | Search tracks with record/disc context |
| `GET` | `/api/record/totals` | Retrieve per-artist disc count and cost totals |
| `GET` | `/api/record/disc-count/{show}` | Count total discs by show flag |
| `GET` | `/api/record/count/artist/{artistId}` | Count total records for an artist |
| `GET` | `/api/record/count/year/{year}` | Count total records for a recording year |
| `POST` | `/api/record` | Create a new record (`CreateRecordDto`) |
| `PUT` | `/api/record/{id}` | Update an existing record (`UpdateRecordDto`) |
| `DELETE` | `/api/record/{id}` | Delete a record by ID |

### 3. Disc (`/api/disc`)

| HTTP Method | Route | Description |
|---|---|---|
| `GET` | `/api/disc` | Retrieve all discs with artist and record context |
| `GET` | `/api/disc/{id}` | Retrieve a single disc by ID |
| `GET` | `/api/disc/search/{name}` | Search discs by record name |
| `POST` | `/api/disc` | Create a new disc (`CreateDiscDto`) |
| `PUT` | `/api/disc/{id}` | Update an existing disc (`UpdateDiscDto`) |
| `PATCH` | `/api/disc/{id}/length` | Update only the disc's length (`UpdateDiscLengthDto`) |
| `DELETE` | `/api/disc/{id}` | Delete a disc by ID |

### 4. Track (`/api/track`)

| HTTP Method | Route | Description |
|---|---|---|
| `GET` | `/api/track` | Retrieve all tracks with full context |
| `GET` | `/api/track/{id}` | Retrieve a single track by ID |
| `GET` | `/api/track/search/{name}` | Search tracks by partial name |
| `GET` | `/api/track/by-artist/{name}` | Retrieve tracks for records by artist name |
| `GET` | `/api/track/by-record/{name}` | Retrieve tracks by record name |
| `GET` | `/api/track/count/record/{recordId}` | Get total track count for a record ID |
| `GET` | `/api/track/check/disc/{discId}` | Check existing track count for a disc |
| `POST` | `/api/track` | Create a single track (`CreateTrackDto`) |
| `POST` | `/api/track/bulk` | Bulk-insert multiple tracks (`IEnumerable<CreateTrackDto>`) |
| `PUT` | `/api/track/{id}` | Update an existing track (`UpdateTrackDto`) |
| `DELETE` | `/api/track/{id}` | Delete a track by ID |
