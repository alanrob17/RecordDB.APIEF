using Microsoft.EntityFrameworkCore;
using RecordDB.APIEF.Data;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;
using System.Globalization;

namespace RecordDB.APIEF.Repositories
{
    public class RecordRepository(RecordDbContext context) : IRecordRepository
    {
        private readonly RecordDbContext _context = context;

        // -----------------------------------------------------------------------
        // Filtering helper for show parameter
        // -----------------------------------------------------------------------

        private static (IQueryable<Record> Query, bool OrderByBought) ApplyShowFilter(IQueryable<Record> query, string show)
        {
            var lowerShow = show.ToLowerInvariant();
            if (lowerShow == "all")
            {
                return (query, false);
            }
            if (lowerShow == "cd")
            {
                return (query.Where(r => r.Media == "CD" || r.Media == "CD/DVD" || r.Media == "CD/Blu-ray" || r.Media == "Blu-ray"), false);
            }
            if (lowerShow == "records")
            {
                return (query.Where(r => r.Media == "R"), false);
            }
            if (lowerShow == "dvds")
            {
                return (query.Where(r => r.Media == "DVD" || r.Media == "CD/DVD" || r.Media == "Blu-ray" || r.Media == "CD/Blu-ray"), true);
            }
            if (lowerShow == "blurays")
            {
                return (query.Where(r => r.Media == "Blu-ray" || r.Media == "CD/Blu-ray"), true);
            }
            if (int.TryParse(show, out var year))
            {
                return (query.Where(r => r.Bought.HasValue && r.Bought.Value.Year == year), true);
            }

            return (query.Where(r => r.Media == show), false);
        }

        // -----------------------------------------------------------------------
        // Queries
        // -----------------------------------------------------------------------

        public async Task<ArtistRecordDto?> SelectAsync(int recordId)
        {
            return await _context.Records
                .AsNoTracking()
                .Where(r => r.RecordId == recordId)
                .Select(r => new ArtistRecordDto
                {
                    ArtistId   = r.ArtistId,
                    FirstName  = r.Artist!.FirstName,
                    LastName   = r.Artist.LastName,
                    ArtistName = r.Artist.Name,
                    Biography  = r.Artist.Biography,
                    RecordId   = r.RecordId,
                    Name       = r.Name,
                    Field      = r.Field,
                    Recorded   = r.Recorded,
                    Label      = r.Label,
                    Pressing   = r.Pressing,
                    Rating     = r.Rating,
                    Discs      = r.Discs,
                    Media      = r.Media,
                    Bought     = r.Bought,
                    Cost       = r.Cost,
                    CoverName  = r.CoverName,
                    Review     = r.Review
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<ArtistRecordDto>> SelectAsync()
        {
            return await _context.Records
                .AsNoTracking()
                .OrderBy(r => r.Artist!.LastName)
                .ThenBy(r => r.Artist!.FirstName)
                .ThenByDescending(r => r.Recorded)
                .Select(r => new ArtistRecordDto
                {
                    ArtistId   = r.ArtistId,
                    FirstName  = r.Artist!.FirstName,
                    LastName   = r.Artist.LastName,
                    ArtistName = r.Artist.Name,
                    Biography  = r.Artist.Biography,
                    RecordId   = r.RecordId,
                    Name       = r.Name,
                    Field      = r.Field,
                    Recorded   = r.Recorded,
                    Label      = r.Label,
                    Pressing   = r.Pressing,
                    Rating     = r.Rating,
                    Discs      = r.Discs,
                    Media      = r.Media,
                    Bought     = r.Bought,
                    Cost       = r.Cost,
                    CoverName  = r.CoverName,
                    Review     = r.Review
                })
                .ToListAsync();
        }

        public async Task<List<Record>> SelectByShowAsync(string show)
        {
            ArgumentNullException.ThrowIfNull(show);

            var (filtered, orderByBought) = ApplyShowFilter(_context.Records.AsNoTracking().Include(r => r.Artist), show);
            var ordered = orderByBought
                ? filtered.OrderByDescending(r => r.Bought)
                : filtered.OrderBy(r => r.Artist!.LastName).ThenBy(r => r.Artist!.FirstName).ThenByDescending(r => r.Recorded);

            return await ordered.ToListAsync();
        }

        public async Task<List<ArtistRecordDto>> SelectRecordsShowAsync(string show)
        {
            if (string.IsNullOrWhiteSpace(show))
                throw new ArgumentNullException(nameof(show));

            var (filtered, orderByBought) = ApplyShowFilter(_context.Records.AsNoTracking(), show);
            var ordered = orderByBought
                ? filtered.OrderByDescending(r => r.Bought)
                : filtered.OrderBy(r => r.Artist!.LastName).ThenBy(r => r.Artist!.FirstName).ThenByDescending(r => r.Recorded);

            return await ordered.Select(r => new ArtistRecordDto
            {
                ArtistId   = r.ArtistId,
                FirstName  = r.Artist!.FirstName,
                LastName   = r.Artist.LastName,
                ArtistName = r.Artist.Name,
                Biography  = r.Artist.Biography,
                RecordId   = r.RecordId,
                Name       = r.Name,
                Field      = r.Field,
                Recorded   = r.Recorded,
                Label      = r.Label,
                Pressing   = r.Pressing,
                Rating     = r.Rating,
                Discs      = r.Discs,
                Media      = r.Media,
                Bought     = r.Bought,
                Cost       = r.Cost,
                CoverName  = r.CoverName,
                Review     = r.Review
            }).ToListAsync();
        }

        public async Task<List<Record>> GetArtistRecordsAsync(int artistId)
        {
            return await _context.Records
                .AsNoTracking()

                .Where(r => r.ArtistId == artistId)
                .OrderByDescending(r => r.Recorded)
                .ToListAsync();
        }

        public async Task<List<Record>> SelectArtistRecordsAsync(int artistId)
        {
            var records = await _context.Records
                .AsNoTracking()
                .Where(r => r.ArtistId == artistId)
                .OrderByDescending(r => r.Recorded)
                .Select(r => new Record
                {
                    RecordId = r.RecordId,
                    Name = r.Name + " (" + (r.Media ?? "").Trim() + ")",
                    Recorded = r.Recorded
                })
                .ToListAsync();

            if (records.Count > 0)
            {
                records.Insert(0, new Record { RecordId = 0, Name = "#Select a record", Recorded = 0 });
            }
            else
            {
                records.Add(new Record { RecordId = 0, Name = "none", Recorded = 0 });
            }

            return records;
        }

        public async Task<List<RecordReviewDto>> SelectRecordReviewsAsync()
        {
            return await _context.Records
                .AsNoTracking()
                .Where(r => EF.Functions.Like(r.Review!, "%"))
                .OrderBy(r => r.Artist!.LastName)
                .ThenBy(r => r.Artist!.FirstName)
                .ThenBy(r => r.Recorded)
                .Select(r => new RecordReviewDto
                {
                    Name = r.Artist!.Name,
                    Title = r.Name,
                    Review = r.Review!
                })
                .ToListAsync();
        }

        public async Task<List<ArtistRecordDto>> GetRecordsByArtistNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return [];

            return await _context.Records
                .AsNoTracking()
                .Where(r => r.Artist!.Name == name)
                .OrderByDescending(r => r.Recorded)
                .Select(r => new ArtistRecordDto
                {
                    ArtistId   = r.ArtistId,
                    FirstName  = r.Artist!.FirstName,
                    LastName   = r.Artist.LastName,
                    ArtistName = r.Artist.Name,
                    Biography  = r.Artist.Biography,
                    RecordId   = r.RecordId,
                    Name       = r.Name,
                    Field      = r.Field,
                    Recorded   = r.Recorded,
                    Label      = r.Label,
                    Pressing   = r.Pressing,
                    Rating     = r.Rating,
                    Discs      = r.Discs,
                    Media      = r.Media,
                    Bought     = r.Bought,
                    Cost       = r.Cost,
                    CoverName  = r.CoverName,
                    Review     = r.Review
                })
                .ToListAsync();
        }

        public async Task<List<ArtistRecordDto>> GetRecordsByYearAsync(int recorded)
        {
            return await _context.Records
                .AsNoTracking()
                .Where(r => r.Recorded == recorded)
                .OrderBy(r => r.Artist!.LastName)
                .ThenBy(r => r.Artist!.FirstName)
                .ThenBy(r => r.Bought)
                .Select(r => new ArtistRecordDto
                {
                    ArtistId   = r.ArtistId,
                    FirstName  = r.Artist!.FirstName,
                    LastName   = r.Artist.LastName,
                    ArtistName = r.Artist.Name,
                    Biography  = r.Artist.Biography,
                    RecordId   = r.RecordId,
                    Name       = r.Name,
                    Field      = r.Field,
                    Recorded   = r.Recorded,
                    Label      = r.Label,
                    Pressing   = r.Pressing,
                    Rating     = r.Rating,
                    Discs      = r.Discs,
                    Media      = r.Media,
                    Bought     = r.Bought,
                    Cost       = r.Cost,
                    CoverName  = r.CoverName,
                    Review     = r.Review
                })
                .ToListAsync();
        }

        public async Task<List<Total>> GetTotalCostsAsync()
        {
            return await _context.Records
                .AsNoTracking()
                .GroupBy(r => new { r.ArtistId, r.Artist!.Name, r.Artist.FirstName, r.Artist.LastName })
                .Select(g => new Total
                {
                    ArtistId   = g.Key.ArtistId,
                    Name       = g.Key.Name ?? $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                    TotalDiscs = g.Sum(r => r.Discs),
                    TotalCost  = g.Sum(r => r.Cost ?? 0m)
                })
                .OrderByDescending(t => t.TotalCost)
                .ToListAsync();
        }

        public async Task<List<ArtistRecordDiscDto>> ListRecordsWithNoTracksAsync()
        {
            return await (from a in _context.Artists.AsNoTracking()
                          join r in _context.Records.AsNoTracking() on a.ArtistId equals r.ArtistId
                          from d in _context.Discs.AsNoTracking().Where(d => d.RecordId == r.RecordId).DefaultIfEmpty()
                          where !_context.Tracks.Any(t => t.DiscId == d.DiscId)
                          orderby a.LastName, a.FirstName, r.Recorded, d.DiscNo
                          select new ArtistRecordDiscDto
                          {
                              ArtistName   = a.Name,
                              RecordId     = r.RecordId,
                              Name         = r.Name,
                              DiscId       = d != null ? d.DiscId : 0,
                              DiscNo       = d != null ? d.DiscNo : 0,
                              FreeDbDiscId = d != null ? d.FreeDbDiscId : null,
                              FreeDbId     = d != null ? d.FreeDbId : null,
                              Length       = d != null ? d.Length : null
                          }).ToListAsync();
        }

        public async Task<List<ArtistRecordDiscDto>> ArtistRecordsWithNoTracksAsync(string name)
        {
            return await (from a in _context.Artists.AsNoTracking()
                          join r in _context.Records.AsNoTracking() on a.ArtistId equals r.ArtistId
                          from d in _context.Discs.AsNoTracking().Where(d => d.RecordId == r.RecordId).DefaultIfEmpty()
                          where !_context.Tracks.Any(t => t.DiscId == d.DiscId) && a.Name == name
                          orderby a.LastName, a.FirstName, r.Recorded, d.DiscNo
                          select new ArtistRecordDiscDto
                          {
                              ArtistName   = a.Name,
                              RecordId     = r.RecordId,
                              Name         = r.Name,
                              DiscId       = d != null ? d.DiscId : 0,
                              DiscNo       = d != null ? d.DiscNo : 0,
                              FreeDbDiscId = d != null ? d.FreeDbDiscId : null,
                              FreeDbId     = d != null ? d.FreeDbId : null,
                              Length       = d != null ? d.Length : null
                          }).ToListAsync();
        }

        public async Task<List<ArtistRecordDiscTrackDto>> SelectTracksByPartialNameAsync(string name)
        {
            return await (from a in _context.Artists.AsNoTracking()
                          join r in _context.Records.AsNoTracking() on a.ArtistId equals r.ArtistId
                          join d in _context.Discs.AsNoTracking() on r.RecordId equals d.RecordId
                          join t in _context.Tracks.AsNoTracking() on d.DiscId equals t.DiscId
                          where t.Name != null && t.Name.Contains(name)
                          orderby a.LastName, a.FirstName, r.Recorded, d.DiscNo, t.TrackNo
                          select new ArtistRecordDiscTrackDto
                          {
                              ArtistName   = a.Name,
                              RecordId     = r.RecordId,
                              Name         = r.Name,
                              DiscId       = d.DiscId,
                              DiscNo       = d.DiscNo,
                              FreeDbDiscId = d.FreeDbDiscId,
                              FreeDbId     = d.FreeDbId,
                              Length       = d.Length,
                              TrackId      = t.TrackId,
                              TrackNo      = t.TrackNo,
                              TrackName    = t.Name,
                              TrackLength  = t.TrackLength,
                              Extended     = t.Extended
                          }).ToListAsync();
        }

        public async Task<List<MissingReviewDto>> NoRecordReviewsAsync()
        {
            return await _context.Records
                .AsNoTracking()
                .Where(r => r.Review == null || EF.Functions.DataLength(r.Review) < 5)
                .OrderBy(r => r.Artist!.LastName)
                .ThenBy(r => r.Artist!.FirstName)
                .Select(r => new MissingReviewDto
                {
                    RecordId = r.RecordId,
                    Name     = r.Artist!.Name,
                    Record   = r.Name
                })
                .ToListAsync();
        }

        public async Task<string> CountDiscsAsync(string show)
        {
            ArgumentNullException.ThrowIfNull(show);

            var (filtered, _) = ApplyShowFilter(_context.Records.AsNoTracking(), show);
            var discs = await filtered.SumAsync(r => (int?)r.Discs) ?? 0;
            return discs.ToString(CultureInfo.InvariantCulture);
        }

        public async Task<string> GetArtistNumberOfRecordsAsync(int artistId)
        {
            var count = await _context.Records
                .AsNoTracking()
                .Where(r => r.ArtistId == artistId)
                .SumAsync(r => (int?)r.Discs) ?? 0;
            return count.ToString(CultureInfo.InvariantCulture);
        }

        public async Task<string> GetRecordedYearNumberAsync(int year)
        {
            var count = await _context.Records
                .AsNoTracking()
                .Where(r => r.Recorded == year)
                .SumAsync(r => (int?)r.Discs) ?? 0;
            return count.ToString(CultureInfo.InvariantCulture);
        }

        public async Task<int> InsertAsync(Record record)
        {
            try
            {
                for (int i = 1; i <= record.Discs; i++)
                {
                    record.DiscsList.Add(new Disc { DiscNo = i });
                }

                _context.Records.Add(record);
                await _context.SaveChangesAsync();
                return record.RecordId;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<int> UpdateAsync(ArtistRecordDto record)
        {
            try
            {
                var existing = await _context.Records.FirstOrDefaultAsync(r => r.RecordId == record.RecordId);
                if (existing is null)
                    return 0;

                existing.ArtistId  = record.ArtistId;
                existing.Name      = record.Name ?? existing.Name;
                existing.Field     = record.Field ?? existing.Field;
                existing.Recorded  = record.Recorded;
                existing.Label     = record.Label ?? existing.Label;
                existing.Pressing  = record.Pressing ?? existing.Pressing;
                existing.Rating    = record.Rating ?? existing.Rating;
                existing.Discs     = record.Discs;
                existing.Media     = record.Media ?? existing.Media;
                existing.Bought    = record.Bought;
                existing.Cost      = record.Cost;
                existing.CoverName = record.CoverName;
                existing.Review    = record.Review;

                await _context.SaveChangesAsync();
                return existing.RecordId;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task DeleteAsync(int recordId)
        {
            try
            {
                var discIds = await _context.Discs
                    .Where(d => d.RecordId == recordId)
                    .Select(d => d.DiscId)
                    .ToListAsync();

                if (discIds.Count > 0)
                {
                    await _context.Tracks
                        .Where(t => discIds.Contains(t.DiscId))
                        .ExecuteDeleteAsync();
                }

                await _context.Discs
                    .Where(d => d.RecordId == recordId)
                    .ExecuteDeleteAsync();

                await _context.Records
                    .Where(r => r.RecordId == recordId)
                    .ExecuteDeleteAsync();
            }
            catch (Exception)
            {
                // swallow — caller receives 204 regardless
            }
        }
    }
}
