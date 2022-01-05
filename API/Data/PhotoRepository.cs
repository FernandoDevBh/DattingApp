using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _context;

    public PhotoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Photo> GetPhotoById(int id)
    {
        return await GetPhotosQuery().SingleOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
        return await GetPhotosQuery()
                        .Where(p => !p.IsApproved)
                        .Select(p => new PhotoForApprovalDto{
                            Id = p.Id,
                            Username = p.AppUser.UserName,
                            Url = p.Url,
                            IsApproved = p.IsApproved
                        }).ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
        _context.Photos.Remove(photo);
    }

    private IQueryable<Photo> GetPhotosQuery() => 
        _context.Photos.IgnoreQueryFilters();
}
