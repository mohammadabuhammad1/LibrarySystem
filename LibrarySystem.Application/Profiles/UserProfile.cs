using AutoMapper;
using LibrarySystem.Application.Dtos.Roles;
using LibrarySystem.Application.Dtos.Users;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.TotalBooksBorrowed, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveBorrows, opt => opt.Ignore())
            .ForMember(dest => dest.OverdueBooks, opt => opt.Ignore())
            .ForMember(dest => dest.TotalFines, opt => opt.Ignore());

        CreateMap<ApplicationUser, AdminUserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.TotalBorrows, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveBorrows, opt => opt.Ignore())
            .ForMember(dest => dest.LastLogin, opt => opt.Ignore());

        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.MembershipDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());
    }
}