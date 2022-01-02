using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API.Controllers;
public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager,
                         SignInManager<AppUser> signInManager, 
                         ITokenService tokenService,
                         IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO register)
    {
        if (await UserExists(register.Username)) return BadRequest("User name is taken");

        var user = _mapper.Map<AppUser>(register);

        using var hmac = new HMACSHA512();

        user.UserName = register.Username.ToLower();

        var result = await _userManager.CreateAsync(user, register.Password);

        if(!result.Succeeded) return BadRequest(result.Errors);

        var roleResult = await _userManager.AddToRoleAsync(user, "Member");

        if(!roleResult.Succeeded) return BadRequest(result.Errors);

        return Ok(new UserDTO
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            KnowAs = user.KnowAs,
            Gender = user.Gender
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO login)
    {
        var user = await _userManager
                         .Users
                         .Include(u => u.Photos)
                         .SingleOrDefaultAsync(u => u.UserName == login.Username.ToLower());

        if (user == null) return Unauthorized("Invalid Username or password");       

        var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);

        if(!result.Succeeded) return Unauthorized();

        return Ok(new UserDTO
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
            KnowAs = user.KnowAs,
            Gender = user.Gender
        });
    }

    private async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(u => u.UserName == username.ToLower());
    }
}
