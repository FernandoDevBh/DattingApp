using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AppUser> GetByIdAsync(int id)
    {
        return await _context.FindAsync<AppUser>(id);
    }

    public async Task<AppUser> GetByUserNameAsync(string userName)
    {
        return await _context
                      .Users
                      .Include(e => e.Photos)
                      .SingleOrDefaultAsync(e => e.UserName == userName);
    }

    public async Task<MemberDTO> GetMemberAsync(string userName, bool isCurrentUser)
    {
        var query = _context
                        .Users
                        .Where(u => u.UserName == userName)
                        .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                        .AsQueryable();
                        
        if(isCurrentUser) query = query.IgnoreQueryFilters();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
    {
       var query = _context
                        .Users                                                
                        .AsQueryable();
        
        query = query.Where(u => u.UserName != userParams.CurrentUsername);
        query = query.Where(u => u.Gender == userParams.Gender);  
        
        var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
        var maxDob = DateTime.Today.AddYears(-userParams.MinAge - 1);

        query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };

        return await PagedList<MemberDTO>.CreateAsync(
            query.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider).AsNoTracking(),
            userParams.PageNumber, userParams.PageSize);
    }      
    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context
                      .Users
                      .Include(e => e.Photos)
                      .ToListAsync();
    }    

    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }

    public void Insert(AppUser user)
    {
        _context.Entry(user).State = EntityState.Added;
    }

    public async Task<string> GetUserGender(string username)
    {
        return await _context
                        .Users
                        .Where(x => x.UserName == username)
                        .Select(x => x.Gender)
                        .FirstOrDefaultAsync();
    }

    public async Task<AppUser> GetUserByPhotoId(int photoId)
    {
        return await _context.Users
                        .Include(p => p.Photos)
                        .IgnoreQueryFilters()
                        .Where(p => p.Photos.Any(p => p.Id == photoId))
                        .FirstOrDefaultAsync();
    }
}
