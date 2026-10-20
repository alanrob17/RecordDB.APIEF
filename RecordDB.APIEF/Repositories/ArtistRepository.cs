using Microsoft.EntityFrameworkCore;
using RecordDB.APIEF.Data;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Repositories
{
    public class ArtistRepository(RecordDbContext context) : IArtistRepository
    {
        private readonly RecordDbContext _context = context;

        public async Task<IEnumerable<Artist>> GetArtistsAsync()
        {
            return await _context.Artists
                .AsNoTracking()
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Artist>> GetArtistsByPartialNameAsync(string name)
        {
            return await _context.Artists
                .AsNoTracking()
                .Where(a => a.Name != null && a.Name.Contains(name))
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();
        }

        public async Task<List<Artist>> SelectAsync()
        {
            return await _context.Artists
                .AsNoTracking()
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();
        }

        public async Task<Artist?> SelectAsync(int artistId)
        {
            return await _context.Artists
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArtistId == artistId);
        }

        public async Task<IEnumerable<Artist>> GetArtistsWithNoBiographyAsync()
        {
            return await _context.Artists
                .AsNoTracking()
                .Where(a => a.Biography == null || EF.Functions.Like(a.Biography, ""))
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();
        }

        public async Task<Artist?> GetArtistWithNoBiographyAsync(string name)
        {
            return await _context.Artists
                .AsNoTracking()
                .Where(a => (a.Biography == null || EF.Functions.Like(a.Biography, "")) && a.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(Artist artist)
        {
            try
            {
                var firstName = string.IsNullOrWhiteSpace(artist.FirstName) ? null : artist.FirstName;
                var lastName = artist.LastName;

                // Check for duplicate artist matching adm_ArtistInsert logic
                var exists = await _context.Artists.AnyAsync(a =>
                    a.FirstName == firstName && a.LastName == lastName);

                if (exists)
                {
                    return -1;
                }

                // Check for biography length < 7 characters
                var bio = artist.Biography;
                if (!string.IsNullOrEmpty(bio) && bio.Length < 7)
                {
                    bio = null;
                }

                // Compute Name if missing
                var computedName = string.IsNullOrWhiteSpace(artist.Name)
                    ? $"{firstName} {lastName}".Trim()
                    : artist.Name;

                var newArtist = new Artist
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Name = computedName,
                    Biography = bio
                };

                _context.Artists.Add(newArtist);
                await _context.SaveChangesAsync();
                return newArtist.ArtistId;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<int> UpdateArtistAsync(Artist artist)
        {
            try
            {
                var existing = await _context.Artists.FirstOrDefaultAsync(a => a.ArtistId == artist.ArtistId);
                if (existing is null)
                {
                    return -1;
                }

                var firstName = string.IsNullOrWhiteSpace(artist.FirstName) ? null : artist.FirstName;
                var lastName = artist.LastName;

                existing.FirstName = firstName;
                existing.LastName = lastName;
                existing.Name = string.IsNullOrWhiteSpace(artist.Name)
                    ? $"{firstName} {lastName}".Trim()
                    : artist.Name;

                var bio = artist.Biography;
                if (!string.IsNullOrEmpty(bio) && bio.Length < 7)
                {
                    bio = null;
                }
                existing.Biography = bio;

                await _context.SaveChangesAsync();
                return existing.ArtistId;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task DeleteAsync(int artistId)
        {
            try
            {
                // Delete child records first if any exist, matching up_deleteArtist logic
                await _context.Records.Where(r => r.ArtistId == artistId).ExecuteDeleteAsync();
                await _context.Artists.Where(a => a.ArtistId == artistId).ExecuteDeleteAsync();
            }
            catch (Exception)
            {
                // swallow — caller receives 204 regardless
            }
        }

        public async Task<Artist?> GetArtistByRecordIdAsync(int recordId)
        {
            return await _context.Records
                .AsNoTracking()
                .Where(r => r.RecordId == recordId)
                .Select(r => r.Artist)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetBiographyAsync(int recordId)
        {
            return await _context.Records
                .AsNoTracking()
                .Where(r => r.RecordId == recordId && r.Artist != null)
                .Select(r => r.Artist!.Biography ?? string.Empty)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        public async Task<int> GetArtistIdAsync(string firstName, string lastName)
        {
            var fn = (firstName != null && firstName.Length >= 2) ? firstName : null;

            IQueryable<Artist> query = _context.Artists.AsNoTracking();

            if (string.IsNullOrEmpty(fn))
            {
                return await query
                    .Where(a => a.LastName.StartsWith(lastName))
                    .Select(a => a.ArtistId)
                    .FirstOrDefaultAsync();
            }

            return await query
                .Where(a => a.FirstName != null && a.FirstName.StartsWith(fn) && a.LastName.StartsWith(lastName))
                .Select(a => a.ArtistId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetArtistIdAsync(int recordId)
        {
            return await _context.Records
                .AsNoTracking()
                .Where(r => r.RecordId == recordId)
                .Select(r => r.ArtistId)
                .FirstOrDefaultAsync();
        }
    }
}
