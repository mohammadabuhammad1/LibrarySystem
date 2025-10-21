using AutoMapper;
using LibrarySystem.Application.Dtos.Roles;
using LibrarySystem.Domain.Entities; 
using Microsoft.AspNetCore.Identity; 

namespace LibrarySystem.Application.Mappings;

public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {

        CreateMap<IdentityRole, RoleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? string.Empty))
            .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.NormalizedName ?? string.Empty));


        CreateMap<ApplicationUser, UserRoleDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name)) 
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty));

        CreateMap<ApplicationUser, AdminUserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.MembershipDate, opt => opt.MapFrom(src => src.MembershipDate))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))

            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.TotalBorrows, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveBorrows, opt => opt.Ignore())
            .ForMember(dest => dest.LastLogin, opt => opt.Ignore());
    }
}