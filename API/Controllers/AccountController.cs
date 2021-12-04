using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly DataContext _dataContext;
    private readonly ITokenService _tokenService;

    public AccountController(DataContext dataContext, ITokenService tokenService)
    {
      _dataContext = dataContext;
      _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO register)
    {
      if (await UserExists(register.Username)) return BadRequest("User name is taken");
      
      using var hmac = new HMACSHA512();
      var user = new AppUser()
      {
        UserName = register.Username.ToLower(),
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
        PasswordSalt = hmac.Key
      };

      _dataContext.Users.Add(user);
      await _dataContext.SaveChangesAsync();
      return Ok(new UserDTO
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      });
    } 

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO login)
    {
      var user = await _dataContext.Users.SingleOrDefaultAsync(u => u.UserName == login.Username);

      if (user == null) return Unauthorized("Invalid Username or password");

      using var hmac = new HMACSHA512(user.PasswordSalt);

      var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

      for (int i = 0; i < computedHash.Length; i++)
      {
        if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Username or password");
      }

      return Ok(new UserDTO
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      });
    }

    private async Task<bool> UserExists(string username)
    {
      return await _dataContext.Users.AnyAsync(u => u.UserName == username.ToLower());
    }
  }
}
