using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;

    public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _photoService = photoService;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await _userManager
                            .Users
                            .Include(u => u.UserRoles)
                            .ThenInclude(r => r.Role)
                            .OrderBy(u => u.UserName)
                            .Select(u => new
                            {
                                u.Id,
                                Username = u.UserName,
                                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                            })
                            .ToListAsync();
        return Ok(users);
    }

    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
    {
        var selectedRoles = roles.Split(",");
        var user = await _userManager.FindByNameAsync(username);

        if(user == null) return NotFound("Could not find user");

        var userRoles = await _userManager.GetRolesAsync(user);

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if(!result.Succeeded) return BadRequest("Failed to add to roles");

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if(!result.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await _userManager.GetRolesAsync(user));
    }        

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForModeration()
    {
        var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
        return Ok(photos);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

        if(photo == null) return NotFound();

        if(photo.IsApproved) return BadRequest("Photo already approved!");

        photo.IsApproved = true;

        var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);

        if(!user.Photos.Any(p => p.IsMain)) photo.IsMain = true;

        await _unitOfWork.Complete();

        return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

        if(photo == null) return NotFound();

        if(photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);

            if(result.Result == "ok")
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
        }
        else
        {
            _unitOfWork.PhotoRepository.RemovePhoto(photo);
        }

        if(await _unitOfWork.Complete())
        {
            return Ok();
        }

        return BadRequest("Erro on reject photo.");
    }
}
