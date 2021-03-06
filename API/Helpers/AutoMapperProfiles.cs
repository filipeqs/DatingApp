using System;
using System.Linq;
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
            CreateMap<AppUser, MemberDto>()
                .ForMember(
                    dest => dest.PhotoUrl, 
                    opt => opt.MapFrom(
                        src => src.Photos
                            .FirstOrDefault(q => q.IsMain).Url))
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(
                        src => src.DateOfBirth.CalculateAge())
                );
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<Photo, PhotoDto>();
            CreateMap<Message, MessageDto>()
                .ForMember(
                    dest => dest.SenderPhotoUrl,
                    opt => opt.MapFrom(
                        src => src.Sender.Photos
                            .FirstOrDefault(q => q.IsMain).Url))
                .ForMember(
                    dest => dest.RecipientPhotoUrl,
                    opt => opt.MapFrom(
                        src => src.Recipient.Photos
                        .FirstOrDefault(q => q.IsMain).Url));
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
        }
    }
}