using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStoreAPI.Data;
using BookStoreAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Repositories
{
    public class BookDbEfCoreRepository : IdentityDbContext<ApplicationUser>, IBookRepository 
    {

        //creating a variable of the BookStoreDbContext
        private readonly BookStoreDbContext dbContext;

        //creating a constructor that will take in the BookStoreDbContext
        public BookDbEfCoreRepository (BookStoreDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task AddAsync(Book book)
        {
           dbContext.Books.Add(book);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await dbContext.Books.FindAsync(id);
            if (book != null)
            {
                dbContext.Books.Remove(book);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await dbContext.Books.ToListAsync();
        }

        public async Task<(List<Book>, int)> GetBooksAsync(string? search, 
        string? genre, 
        string? author, 
        decimal? minPrice, 
        decimal? maxPrice, 
        string? sortBy, 
        bool descending, 
        int page, 
        int pageSize)
        {
            IQueryable<Book> query = dbContext.Books;

        // Search by Title, Author, or Genre
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearch = search.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(lowerSearch) ||
                                     b.Author.ToLower().Contains(lowerSearch) ||
                                     b.Genre.ToLower().Contains(lowerSearch));
        }

        // Filter by Genre
        if (!string.IsNullOrEmpty(genre))
        {
            query = query.Where(b => b.Genre.ToLower() == genre.ToLower());
        }

        //  Filter by Author
        if (!string.IsNullOrEmpty(author))
        {
            query = query.Where(b => b.Author.ToLower().Contains(author.ToLower()));
        }

        //  Filter by Price Range
        if (minPrice.HasValue)
        {
            query = query.Where(b => b.Price >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(b => b.Price <= maxPrice.Value);
        }

        //  Sorting
        query = sortBy switch
        {
            "title" => descending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "author" => descending ? query.OrderByDescending(b => b.Author) : query.OrderBy(b => b.Author),
            "price" => descending ? query.OrderByDescending(b => b.Price) : query.OrderBy(b => b.Price),
            _ => query.OrderBy(b => b.Title) // Default sorting
        };

        //  Get total count before pagination
        int totalBooks = await query.CountAsync();

        //  Apply Pagination
        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        var books = await query.ToListAsync();
     return (books, totalBooks);
    


        }

    

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await dbContext.Books.FindAsync(id);
        }

        public async Task<IEnumerable<Book>> GetFilteredBooksAsync(string? title, string? author, string? genre, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
           var query =dbContext.Books.AsQueryable();

           // applying various filters 
           if (!string.IsNullOrWhiteSpace(title)){
            query = query.Where(b =>b.Title.ToLower().Contains(title.ToLower()));
           }

            if (!string.IsNullOrWhiteSpace(author)){
                query =query.Where(b => b.Author.ToLower().Contains(author.ToLower()));
            }

           if (!string.IsNullOrWhiteSpace(genre)){
                query = query.Where(b => b.Genre.ToLower().Contains(genre.ToLower()));
           }

           if (minPrice.HasValue){
            query =query.Where(b=> b.Price>=minPrice.Value);
           }
            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice.Value);
            }

            // applying sorting
            query = sortBy?.ToLower() switch
                    {
                "title" => query.OrderBy(b => b.Title),
                "author" => query.OrderBy(b => b.Author),
                "price" => query.OrderBy(b => b.Price),
                _ => query.OrderBy(b => b.Id) // Default sorting by ID
                         };

            return await query.ToListAsync();



        }

        public  async Task<IEnumerable<Book>> SearchBookAsync(string searchTerm)
        {
            return await dbContext.Books
            .Where(b=> b.Title.ToLower().Contains(searchTerm.ToLower()) || 
            b.Author.ToLower().Contains(searchTerm.ToLower()))
            .ToListAsync();
        }

        public  async Task UpdateAsync(Book book)
        {
             dbContext.Books.Update(book);
            await dbContext.SaveChangesAsync();
        }
    }
}