using Microsoft.EntityFrameworkCore;
using RecordDB.APIEF.Data;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Repositories
{
    public class TrackRepository(RecordDbContext context) : ITrackRepository
    {
        private readonly RecordDbContext _context = context;

        public async Task<IEnumerable<ArtistRecordDiscTrackDto>> SelectAllTrackEntitiesAsync()
        {
            return await (from a in _context.Artists.AsNoTracking()
                          join r in _context.Records.AsNoTracking() on a.ArtistId equals r.ArtistId
                          join d in _context.Discs.AsNoTracking() on r.RecordId equals d.RecordId
                          join t in _context.Tracks.AsNoTracking() on d.DiscId equals t.DiscId
                          orderby a.LastName, a.FirstName, r.Name, r.Recorded, d.DiscNo, t.TrackNo
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

        public async Task<IEnumerable<ArtistRecordDiscTrackDto>> SelectArtistRecordTracksAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return [];

            // First check if an artist matches this name
            var tracksByArtist = await (from a in _context.Artists.AsNoTracking()
                                        join r in _context.Records.AsNoTracking() on a.ArtistId equals r.ArtistId
                                        join d in _context.Discs.AsNoTracking() on r.RecordId equals d.RecordId
                                        join t in _context.Tracks.AsNoTracking() on d.DiscId equals t.DiscId
                                        where a.Name == name
                                        orderby r.Recorded, d.DiscNo, t.TrackNo
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

            if (tracksByArtist.Count > 0)
                return tracksByArtist;

            // Fallback to record name matching (in case legacy sproc behavior was relied upon)
            return await SelectTracksByRecordAsync(name);
        }

        public async Task<IEnumerable<ArtistRecordDiscTrackDto>> SelectTracksByRecordAsync(string name)
        {
            var recordId = await _context.Records
                .AsNoTracking()
                .Where(r => r.Name == name)
                .OrderBy(r => r.RecordId)
                .Select(r => (int?)r.RecordId)
                .FirstOrDefaultAsync();

            if (recordId is null)
                return [];

            return await (from a in _context.Artists.AsNoTracking()
                          join r in _context.Records.AsNoTracking() on a.ArtistId equals r.ArtistId
                          join d in _context.Discs.AsNoTracking() on r.RecordId equals d.RecordId
                          join t in _context.Tracks.AsNoTracking() on d.DiscId equals t.DiscId
                          where r.RecordId == recordId.Value
                          orderby d.DiscNo, t.TrackNo
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

        public async Task<IEnumerable<ArtistRecordDiscTrackDto>> SelectTracksByPartialNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return [];

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

        public async Task<ArtistRecordDiscTrackDto?> SelectTrackByIdAsync(int trackId)
        {
            return await (from a in _context.Artists.AsNoTracking()
                          join r in _context.Records.AsNoTracking() on a.ArtistId equals r.ArtistId
                          join d in _context.Discs.AsNoTracking() on r.RecordId equals d.RecordId
                          join t in _context.Tracks.AsNoTracking() on d.DiscId equals t.DiscId
                          where t.TrackId == trackId
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
                          }).FirstOrDefaultAsync();
        }

        public async Task<int> GetTrackNumberAsync(int recordId)
        {
            return await (from d in _context.Discs.AsNoTracking()
                          join t in _context.Tracks.AsNoTracking() on d.DiscId equals t.DiscId
                          where d.RecordId == recordId
                          select t.TrackId).CountAsync();
        }

        public async Task<int> CheckForTracksAsync(int discId)
        {
            return await _context.Tracks.AsNoTracking().CountAsync(t => t.DiscId == discId);
        }

        public async Task<int> InsertTrackAsync(Track track)
        {
            try
            {
                _context.Tracks.Add(track);
                await _context.SaveChangesAsync();
                return track.TrackId;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<int> UpdateTrackAsync(Track track)
        {
            try
            {
                var existing = await _context.Tracks.FirstOrDefaultAsync(t => t.TrackId == track.TrackId);
                if (existing is null)
                    return -1;

                existing.DiscId      = track.DiscId;
                existing.TrackNo     = track.TrackNo;
                existing.Name        = track.Name;
                existing.TrackLength = track.TrackLength;
                existing.Extended    = track.Extended;

                await _context.SaveChangesAsync();
                return existing.TrackId;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task DeleteTrackAsync(int trackId)
        {
            try
            {
                await _context.Tracks.Where(t => t.TrackId == trackId).ExecuteDeleteAsync();
            }
            catch (Exception)
            {
                // swallow — caller receives 204 regardless
            }
        }

        public async Task BulkInsertTracksAsync(IEnumerable<Track> tracks)
        {
            await _context.Tracks.AddRangeAsync(tracks);
            await _context.SaveChangesAsync();
        }

        public Task BulkTrackInsertAsync(List<Track> tracks) => BulkInsertTracksAsync(tracks);
    }
}
