using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStoreAPI.Models;

namespace BookStoreAPI.Repositories
{
    public interface IBookRepository
    {Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);
        Task <IEnumerable<Book>>SearchBookAsync(string searchTerm);
        //this method willbe used to sort and filter
        Task<IEnumerable<Book>>GetFilteredBooksAsync(string? title, string? author, string? genre,decimal? minPrice,decimal? maxPrice, string? sortBy) ;

        //
        Task<(List<Book>, int)> GetBooksAsync( string? search, 
        string? genre, 
        string? author, 
        decimal? minPrice, 
        decimal? maxPrice, 
        string? sortBy, 
        bool descending, 
        int page, 
        int pageSize);

    }
}