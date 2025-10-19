using LibrarySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.Infrastructure.Data;

public partial class DataSeeder(LibraryDbContext context, ILogger<DataSeeder> logger)
{
    // LoggerMessage delegates for high-performance logging
    private static readonly Action<ILogger, Exception?> _databaseSeededSuccessfully =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, "DatabaseSeeded"), "Database seeded with initial data successfully.");

    private static readonly Action<ILogger, string, Exception?> _seedingError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(2, "SeedingError"), "An error occurred while seeding the database: {ErrorMessage}");

    private static readonly Action<ILogger, Exception?> _librariesSeeded =
        LoggerMessage.Define(LogLevel.Information, new EventId(3, "LibrariesSeeded"), "Libraries seeded successfully.");

    private static readonly Action<ILogger, Exception?> _booksSeeded =
        LoggerMessage.Define(LogLevel.Information, new EventId(4, "BooksSeeded"), "Books seeded successfully.");

    private static readonly Action<ILogger, Exception?> _borrowRecordsSeeded =
        LoggerMessage.Define(LogLevel.Information, new EventId(5, "BorrowRecordsSeeded"), "Borrow records seeded successfully.");

    public async Task SeedAsync()
    {
        try
        {
            await SeedLibrariesAsync().ConfigureAwait(false);
            await SeedBooksAsync().ConfigureAwait(false);
            await SeedBorrowRecordsAsync().ConfigureAwait(false);
            _databaseSeededSuccessfully(logger, null);
        }
        catch (DbUpdateException dbEx)
        {
            _seedingError(logger, dbEx.Message, dbEx);
            throw;
        }
        catch (InvalidOperationException invalidOpEx)
        {
            _seedingError(logger, invalidOpEx.Message, invalidOpEx);
            throw;
        }
        catch (Exception ex)
        {
            _seedingError(logger, ex.Message, ex);
            throw;
        }
    }

    private async Task SeedLibrariesAsync()
    {
        if (!await context.Libraries.AnyAsync().ConfigureAwait(false))
        {
            // Get the first organization unit
            OrganizationUnit? defaultOu = await context.OrganizationUnits.FirstOrDefaultAsync().ConfigureAwait(false);
            if (defaultOu == null)
            {
                return;
            }

            List<Library> libraries = new()
            {
                Library.Create(
                    "Central Library",           
                    "Main Street",               
                    "A hub for book lovers",     
                    defaultOu.Id                 
                ),
                Library.Create(
                    "Downtown Branch",           
                    "Downtown",                  
                    "A small branch offering study space",
                    defaultOu.Id                 
                )
            };

            await context.Libraries.AddRangeAsync(libraries).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            _librariesSeeded(logger, null);
        }
    }

    private async Task SeedBooksAsync()
    {
        if (!await context.Books.AnyAsync().ConfigureAwait(false))
        {
            Library? library = await context.Libraries.FirstOrDefaultAsync().ConfigureAwait(false);
            if (library != null)
            {
                List<Book> books = new()
                {
                    Book.Create(
                        "To Kill a Mockingbird", 
                        "Harper Lee",           
                        "9780061120084",        
                        1960,                   
                        10,                     
                        library.Id              
                    ),
                    Book.Create(
                        "1984",                 
                        "George Orwell",        
                        "9780451524935",        
                        1949,                   
                        5,                      
                        library.Id              
                    )
                };

                await context.Books.AddRangeAsync(books).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                _booksSeeded(logger, null);
            }
        }
    }

    private async Task SeedBorrowRecordsAsync()
    {
        if (!await context.BorrowRecords.AnyAsync().ConfigureAwait(false))
        {
            ApplicationUser? user = await context.Users.FirstOrDefaultAsync().ConfigureAwait(false);
            Book? book = await context.Books.FirstOrDefaultAsync().ConfigureAwait(false);

            if (user != null && book != null)
            {
                var borrowRecord = BorrowRecord.Create(
                    book.Id,                   
                    user.Id,                   
                    14,                        
                    "First Borrow"             
                );

                await context.BorrowRecords.AddAsync(borrowRecord).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                _borrowRecordsSeeded(logger, null);
            }
        }
    }
}