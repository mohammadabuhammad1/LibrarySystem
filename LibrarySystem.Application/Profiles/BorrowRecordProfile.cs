using AutoMapper;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Domain.Commands.Books; // Add this using
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Profiles;

public class BorrowRecordProfile : Profile
{
    public BorrowRecordProfile()
    {
        CreateMap<BorrowRecord, BorrowRecordDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book != null ? src.Book.Title : string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : string.Empty))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue()))
            .ForMember(dest => dest.DaysOverdue, opt => opt.MapFrom(src => src.DaysOverdue()))
            .ForMember(dest => dest.ConditionDescription, opt => opt.MapFrom(src => src.ConditionDescription));

        CreateMap<CreateBorrowRecordDto, BorrowRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.BorrowDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => DateTime.UtcNow.AddDays(src.BorrowDurationDays)))
            .ForMember(dest => dest.ReturnDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsReturned, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.FineAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Condition, opt => opt.Ignore())
            .ForMember(dest => dest.RenewalCount, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.Book, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        CreateMap<CreateBorrowRecordDto, BorrowBookCommand>()
            .ForMember(dest => dest.CommandBy, opt => opt.Ignore());

        CreateMap<ReturnBookDto, ReturnBookCommand>()
            .ForMember(dest => dest.CommandBy, opt => opt.Ignore());
    }
}