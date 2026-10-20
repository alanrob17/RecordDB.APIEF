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

## Current Status: Phase 1 — Artist Layer

The **Artist Layer** is fully implemented and operational:
* `RecordDbContext` with Fluent API table mapping (`[dbo].[Artist]`).
* Idiomatic EF Core LINQ queries replacing all Dapper stored procedures.
* Complete Artist REST API endpoints matching the original specification.
* Triple API Documentation (Scalar, ReDoc, Swagger UI).
* Side-by-side execution support on ports `https://localhost:7097` and `http://localhost:5184`.

---

## Project Structure

```
RecordDB.APIEF/
├── RecordDB.APIEF.slnx                   # Solution file (.NET 10 format)
└── RecordDB.APIEF/
    ├── Program.cs                        # DI setup, EF Core DbContext, & Swagger/Scalar
    ├── appsettings.json                  # Database connection string & logging config
    ├── Controllers/
    │   └── ArtistController.cs           # Artist CRUD, bios, and search endpoints
    ├── Repositories/
    │   ├── IArtistRepository.cs          # Data access abstraction
    │   └── ArtistRepository.cs           # EF Core LINQ repository implementation
    ├── Data/
    │   ├── RecordDbContext.cs            # EF Core DbContext
    │   └── Configurations/
    │       ├── ArtistConfiguration.cs    # Fluent API mappings for Artist
    │       └── RecordConfiguration.cs    # Minimal navigation stub for Record
    ├── Models/
    │   ├── Artist.cs                     # Artist entity with navigation to Records
    │   └── Record.cs                     # Minimal Record stub for foreign key queries
    └── DTOs/
        ├── ArtistDto.cs                  # Response DTO
        ├── CreateArtistDto.cs            # Request DTO for creating artists
        └── UpdateArtistDto.cs            # Request DTO for updating artists
```

---

## Running the API

From the terminal:

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

## Artist Endpoints (`/api/artist`)

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
