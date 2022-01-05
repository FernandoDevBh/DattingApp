using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {        
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        var gender = await _unitOfWork.UserRepository.GetUserGender(userParams.CurrentUsername);

        if (string.IsNullOrEmpty(userParams.Gender))
            userParams.Gender = gender == "Male" ? "female" : "male";

        var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
        return Ok(users);
    }

    [HttpGet("{userName}", Name = "GetUser")]
    public async Task<ActionResult<MemberDTO>> GetUsers(string userName) =>
      Ok(await _unitOfWork.UserRepository.GetMemberAsync(userName));

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {
        var user = await _unitOfWork.UserRepository.GetByUserNameAsync(User.GetUsername());
        _mapper.Map(memberUpdateDTO, user);

        _unitOfWork.UserRepository.Update(user);

        if (await _unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to update user");
    }
    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetByUserNameAsync(User.GetUsername());
        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null)
            return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
        };

        if (user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);

        if (await _unitOfWork.Complete())
        {
            return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDTO>(photo));
        }

        return BadRequest("Problem adding photo.");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetByUserNameAsync(User.GetUsername());
        var photo = user.Photos.FirstOrDefault(u => u.Id == photoId);

        if (photo.IsMain) return BadRequest("This is already your main photo");

        var currentMain = user.Photos.FirstOrDefault(u => u.IsMain);

        if (currentMain != null) currentMain.IsMain = false;

        photo.IsMain = true;

        if (await _unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetByUserNameAsync(User.GetUsername());
        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return NotFound();

        if (photo.IsMain)
            return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest("Failed to delete the photo.");
    }
}
