using AutoMapper;
using LibrarySystem.Application.Dtos.Libraries;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Profiles;

public class LibraryProfile : Profile
{
    public LibraryProfile()
    {
        CreateMap<Library, LibraryDto>()
            .ForMember(dest => dest.BookCount, opt => opt.MapFrom(src => src.Books != null ? src.Books.Count : 0));

        CreateMap<Library, LibraryDetailsDto>()
            .ForMember(dest => dest.Books, opt => opt.MapFrom(src => src.Books ?? new List<Book>()));

        CreateMap<CreateLibraryDto, Library>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Books, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateLibraryDto, Library>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Books, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}