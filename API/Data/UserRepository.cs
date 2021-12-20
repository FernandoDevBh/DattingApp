using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
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

    public async Task<MemberDTO> GetMemberAsync(string userName) =>
      await _context
            .Users
            .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<MemberDTO>> GetMembersAsync() =>
      await _context
            .Users
            .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
      return await _context
                    .Users
                    .Include(e => e.Photos)
                    .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    public void Update(AppUser user)
    {
      _context.Entry(user).State = EntityState.Modified;
    }
  }
}
