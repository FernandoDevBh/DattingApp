using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers;
public class AccountController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(IUserRepository repository,
                         ITokenService tokenService,
                         IMapper mapper)
    {
        _userRepository = repository;
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
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password));
        user.PasswordSalt = hmac.Key;

        _userRepository.Insert(user);
        await _userRepository.SaveAllAsync();
        return Ok(new UserDTO
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            KnowAs = user.KnowAs,
            Gender = user.Gender
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO login)
    {
        var user = await _userRepository.GetByUserNameAsync(login.Username);

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
            Token = _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
            KnowAs = user.KnowAs,
            Gender = user.Gender
        });
    }

    private async Task<bool> UserExists(string username)
    {
        var user = await _userRepository.GetByUserNameAsync(username);
        return user != null;
    }
}
