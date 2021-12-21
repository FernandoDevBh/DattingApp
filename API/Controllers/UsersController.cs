using System.Security.Claims;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
  private readonly IUserRepository _userRepository;
  private readonly IMapper _mapper;

  public UsersController(IUserRepository userRepository, IMapper mapper)
  {
    _userRepository = userRepository;
    _mapper = mapper;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
  {
    var users = await _userRepository.GetMembersAsync();
    return Ok(users);
  }    

  [HttpGet("{userName}")]  
  public async Task<ActionResult<MemberDTO>> GetUsers(string userName) =>
    Ok(await _userRepository.GetMemberAsync(userName));

  [HttpPut]
  public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
  {
    var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var user = await _userRepository.GetByUserNameAsync(username);
    _mapper.Map(memberUpdateDTO, user);

    _userRepository.Update(user);

    if(await _userRepository.SaveAllAsync()) return NoContent();
    
    return BadRequest("Failed to update user");
  }
}
