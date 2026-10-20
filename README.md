# RecordDB.APIEF

![.NET 10](https://img.shields.io/badge/.NET%2010-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![C#](https://img.shields.io/badge/C%23%2013-239120?logo=csharp&logoColor=white&style=for-the-badge)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![Microsoft SQL Server](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC292B?logo=microsoftsqlserver&logoColor=white&style=for-the-badge)
![Scalar](https://img.shields.io/badge/Scalar-API%20Reference-E9573F?style=for-the-badge)
![ReDoc](https://img.shields.io/badge/ReDoc-API%20Docs-263238?logo=openapiinitiative&logoColor=white&style=for-the-badge)
![Swagger](https://img.shields.io/badge/Swagger%20%2F%20OpenAPI-85EA2D?logo=swagger&logoColor=black&style=for-the-badge)

A modern, high-performance **.NET 10 Web API** for the **RecordDB** music catalog system built with **Entity Framework Core 10**.

This project ports the data access layer from stored procedures and Dapper to idiomatic EF Core LINQ queries, change tracking, and Fluent API configurations while maintaining 100% endpoint, route, DTO, and contract parity.

---

## Current Status

- [x] **Artist Layer** (`/api/artist`) — 11 endpoints
- [x] **Record Layer** (`/api/record`) — 17 endpoints
- [ ] **Disc Layer** (`/api/disc`)
- [ ] **Track Layer** (`/api/track`)

---

## Project Structure

```
RecordDB.APIEF/
├── RecordDB.APIEF.slnx                   # Solution file (.NET 10 format)
└── RecordDB.APIEF/
    ├── Program.cs                        # DI setup, EF Core DbContext, & Swagger/Scalar
    ├── appsettings.json                  # Database connection string & logging config
    ├── RecordDB.APIEF.http               # Integration tests for all endpoints
    ├── Controllers/
    │   ├── ArtistController.cs           # Artist CRUD, bios, and search endpoints
    │   └── RecordController.cs           # Record CRUD, reviews, totals, and counts
    ├── Repositories/
    │   ├── IArtistRepository.cs / ArtistRepository.cs
    │   └── IRecordRepository.cs / RecordRepository.cs
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
- **OpenAPI JSON**: `https://localhost:7097/openapi/v1.json`

---

## Record Endpoints (`/api/record`)

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
