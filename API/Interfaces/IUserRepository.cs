using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
  public interface IUserRepository
  {
    void Update(AppUser user);
    void Insert(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser> GetByIdAsync(int id);
    Task<AppUser> GetByUserNameAsync(string userName);
    Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);
    Task<MemberDTO> GetMemberAsync(string userName);
    Task<string> GetUserGender(string username);
  }
}
