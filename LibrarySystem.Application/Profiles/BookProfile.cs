using AutoMapper;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Profiles;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookDto>()
            .ForMember(dest => dest.LibraryId, opt => opt.MapFrom(src => src.LibraryId ?? 0));

        CreateMap<CreateBookDto, Book>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CopiesAvailable, opt => opt.MapFrom(src => src.TotalCopies))
            .ForMember(dest => dest.BorrowedCopiesCount, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.TotalCopies > 0))
            .ForMember(dest => dest.IsOutOfStock, opt => opt.MapFrom(src => src.TotalCopies == 0))
            .ForMember(dest => dest.UtilizationRate, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.LibraryId, opt => opt.Ignore())
            .ForMember(dest => dest.Library, opt => opt.Ignore())
            .ForMember(dest => dest.BorrowRecords, opt => opt.Ignore());

        CreateMap<UpdateBookDto, Book>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CopiesAvailable, opt => opt.Ignore())
            .ForMember(dest => dest.BorrowedCopiesCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
            .ForMember(dest => dest.IsOutOfStock, opt => opt.Ignore())
            .ForMember(dest => dest.UtilizationRate, opt => opt.Ignore())
            .ForMember(dest => dest.LibraryId, opt => opt.Ignore())
            .ForMember(dest => dest.Library, opt => opt.Ignore())
            .ForMember(dest => dest.BorrowRecords, opt => opt.Ignore());
    }
}