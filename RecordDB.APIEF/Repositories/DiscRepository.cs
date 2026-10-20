using Microsoft.EntityFrameworkCore;
using RecordDB.APIEF.Data;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Repositories
{
    public class DiscRepository(RecordDbContext context) : IDiscRepository
    {
        private readonly RecordDbContext _context = context;

        public async Task<IEnumerable<ArtistRecordDiscDto>> SelectAllDiscEntitiesAsync()
        {
            return await (from r in _context.Records.AsNoTracking()
                          join a in _context.Artists.AsNoTracking() on r.ArtistId equals a.ArtistId
                          join d in _context.Discs.AsNoTracking() on r.RecordId equals d.RecordId
                          orderby a.LastName, a.FirstName, r.Recorded, d.DiscNo
                          select new ArtistRecordDiscDto
                          {
                              RecordId     = r.RecordId,
                              DiscId       = d.DiscId,
                              ArtistName   = a.Name,
                              Name         = r.Name,
                              DiscNo       = d.DiscNo,
                              FreeDbId     = d.FreeDbId,
                              FreeDbDiscId = d.FreeDbDiscId,
                              Length       = d.Length
                          }).ToListAsync();
        }

        public async Task<IEnumerable<ArtistRecordDiscDto>> GetDiscRecordsByRecordNameAsync(string recordName)
        {
            if (string.IsNullOrWhiteSpace(recordName))
                return [];

            return await (from r in _context.Records.AsNoTracking()
                          join a in _context.Artists.AsNoTracking() on r.ArtistId equals a.ArtistId
                          join d in _context.Discs.AsNoTracking() on r.RecordId equals d.RecordId
                          where r.Name != null && r.Name.Contains(recordName)
                          orderby a.LastName, a.FirstName, r.Recorded, d.DiscNo
                          select new ArtistRecordDiscDto
                          {
                              RecordId     = r.RecordId,
                              DiscId       = d.DiscId,
                              ArtistName   = a.Name,
                              Name         = r.Name,
                              DiscNo       = d.DiscNo,
                              FreeDbId     = d.FreeDbId,
                              FreeDbDiscId = d.FreeDbDiscId,
                              Length       = d.Length
                          }).ToListAsync();
        }

        public async Task<ArtistRecordDiscDto?> SelectSingleDiscAsync(int discId)
        {
            return await (from r in _context.Records.AsNoTracking()
                          join a in _context.Artists.AsNoTracking() on r.ArtistId equals a.ArtistId
                          join d in _context.Discs.AsNoTracking() on r.RecordId equals d.RecordId
                          where d.DiscId == discId
                          select new ArtistRecordDiscDto
                          {
                              RecordId     = r.RecordId,
                              DiscId       = d.DiscId,
                              ArtistName   = a.Name,
                              Name         = r.Name,
                              DiscNo       = d.DiscNo,
                              FreeDbId     = d.FreeDbId,
                              FreeDbDiscId = d.FreeDbDiscId,
                              Length       = d.Length
                          }).FirstOrDefaultAsync();
        }

        public async Task<int> InsertDiscAsync(Disc disc)
        {
            try
            {
                var recordExists = await _context.Records.AnyAsync(r => r.RecordId == disc.RecordId);
                if (!recordExists)
                    return -1;

                _context.Discs.Add(disc);
                await _context.SaveChangesAsync();
                return disc.DiscId;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<int> UpdateDiscAsync(Disc disc)
        {
            try
            {
                var existing = await _context.Discs.FirstOrDefaultAsync(d => d.DiscId == disc.DiscId);
                if (existing is null)
                    return -1;

                existing.DiscNo       = disc.DiscNo;
                existing.FreeDbDiscId = disc.FreeDbDiscId;
                existing.FreeDbId     = disc.FreeDbId;
                existing.Length       = disc.Length;

                await _context.SaveChangesAsync();
                return existing.DiscId;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task DeleteDiscAsync(int discId)
        {
            try
            {
                await _context.Tracks.Where(t => t.DiscId == discId).ExecuteDeleteAsync();
                await _context.Discs.Where(d => d.DiscId == discId).ExecuteDeleteAsync();
            }
            catch (Exception)
            {
                // swallow — caller receives 204 regardless
            }
        }

        public async Task UpdateDiscLengthAsync(int discId, int? totalLength)
        {
            await _context.Discs
                .Where(d => d.DiscId == discId)
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.Length, totalLength));
        }
    }
}
