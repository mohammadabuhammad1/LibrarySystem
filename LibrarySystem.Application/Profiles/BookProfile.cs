using AutoMapper;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Profiles;

public class BookProfile : Profile
{
    public BookProfile()
    {
        // Existing entity mappings
        CreateMap<Book, BookDto>();

        // Add this mapping for BookStatsDto
        CreateMap<Book, BookStatsDto>()
            .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.TotalCopies, opt => opt.MapFrom(src => src.TotalCopies))
            .ForMember(dest => dest.CopiesAvailable, opt => opt.MapFrom(src => src.CopiesAvailable))
            .ForMember(dest => dest.BorrowedCopiesCount, opt => opt.MapFrom(src => src.BorrowedCopiesCount))
            .ForMember(dest => dest.UtilizationRate, opt => opt.MapFrom(src => src.UtilizationRate))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
            .ForMember(dest => dest.IsOutOfStock, opt => opt.MapFrom(src => src.IsOutOfStock));

        // Mapping from CreateBookDto to Book entity
        CreateMap<CreateBookDto, Book>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TotalCopies, opt => opt.MapFrom(src => src.TotalCopies))
            .ForMember(dest => dest.CopiesAvailable, opt => opt.MapFrom(src => src.TotalCopies))
            .ForMember(dest => dest.BorrowedCopiesCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
            .ForMember(dest => dest.IsOutOfStock, opt => opt.Ignore())
            .ForMember(dest => dest.UtilizationRate, opt => opt.Ignore())
            .ForMember(dest => dest.LibraryId, opt => opt.Ignore())
            .ForMember(dest => dest.Library, opt => opt.Ignore())
            .ForMember(dest => dest.BorrowRecords, opt => opt.Ignore());

        // Mapping from UpdateBookDto to Book entity
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

        CreateMap<CreateBookDto, CreateBookCommand>()
            .ForMember(dest => dest.CommandBy, opt => opt.Ignore());

        CreateMap<UpdateBookDto, UpdateBookCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CommandBy, opt => opt.Ignore());
    }
}