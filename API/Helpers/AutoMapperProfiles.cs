using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
  public class AutoMapperProfiles : Profile
  {
    public AutoMapperProfiles()
    {
      CreateMap<AppUser, MemberDTO>()
        .ForMember(d => d.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
        .ForMember(d => d.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
      CreateMap<Photo, PhotoDTO>();

      CreateMap<MemberUpdateDTO, AppUser>();

      CreateMap<RegisterDTO, AppUser>();

      CreateMap<Message, MessageDto>()
        .ForMember(d => d.SenderPhotoUrl, opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
        .ForMember(d => d.RecipientPhotoUrl, opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));

      CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
    }
  }
}
