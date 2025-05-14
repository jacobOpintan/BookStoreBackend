using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using System.Reflection.Metadata.Ecma335;
using BookStoreAPI.Repositories;
using BookStoreAPI.Models;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Endpoints;

public static class BookEndpoints
{

    //this is an extension method that maps the endpoints to the /book route
    //this method will be called in the Program.cs file
    public static RouteGroupBuilder MapBooksEndPoint(this IEndpointRouteBuilder routes)
    {
        // this will map the endpoint to the /book route
        var group = routes.MapGroup("/book");

        //Map the GET /book endpoint to the GetAllAsync method
        group.MapGet("/", async (IBookRepository repository) => await repository.GetAllAsync()).RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, IBookRepository repository) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest("Invalid Book Id");
            }
            var book = await repository.GetByIdAsync(id);
            return book is not null ? Results.Ok(book) : Results.NotFound();
        }
         ).RequireAuthorization();

        // endpoint to allow users to make requests using either the id or Title or author
        group.MapGet("/{identifier}", async (IBookRepository repository, string identifier) =>
        {
            string searchTerm = identifier.ToLower();

            //try to check if identifier is a number
            if (int.TryParse(identifier, out int bookId))
            {
                var bookById = await repository.GetByIdAsync(bookId);
                return bookById is not null ? Results.Ok(bookById) : Results.NotFound();
            }

            //performs case insensitive serch and partial matching for title amd Author
            var book = await repository.SearchBookAsync(searchTerm);
            return book.Any() ? Results.Ok(book) : Results.NotFound(new { message = "No book found" });



        }).RequireAuthorization();


        //to create a new book
        group.MapPost("/", async (Book createBook, IBookRepository repository) =>
        {
            if (string.IsNullOrWhiteSpace(createBook.Title) || createBook.Title.Length > 50)
            {
                return Results.BadRequest("Title cannot be empty and it is requred to be less than 50 characters");
            }
            if (string.IsNullOrWhiteSpace(createBook.Author) || createBook.Author.Length > 25)
            {
                return Results.BadRequest("Author cannot be empty and it is required to be less than 25 characters");
            }
            if (string.IsNullOrWhiteSpace(createBook.Genre) || createBook.Genre.Length > 15)
            {
                return Results.BadRequest("Genre cannot be empty and it is required to be less than 15 characters");
            }
            if (createBook.Price <= 0 || createBook.Price > 1000)
            {
                return Results.BadRequest("Price must be between 1 and 1000");
            }
            await repository.AddAsync(createBook);
            return Results.Created($"/book/{createBook.Id}", createBook);

        }).RequireAuthorization("Admin");

        group.MapPut("/{id:int}", async (int id, Book updateBook, IBookRepository repo) =>
           {
               if (id <= 0) return Results.BadRequest("Invalid book ID.");

               var existingBook = await repo.GetByIdAsync(id);
               if (existingBook is null) return Results.NotFound();

               if (string.IsNullOrWhiteSpace(updateBook.Title) || updateBook.Title.Length > 100)
                   return Results.BadRequest("Title is required and must not exceed 100 characters.");

               if (string.IsNullOrWhiteSpace(updateBook.Author) || updateBook.Author.Length > 50)
                   return Results.BadRequest("Author is required and must not exceed 50 characters.");

               if (updateBook.Price <= 0)
                   return Results.BadRequest("Price must be greater than 0.");

               existingBook.Title = updateBook.Title;
               existingBook.Author = updateBook.Author;
               existingBook.Price = updateBook.Price;

               await repo.UpdateAsync(existingBook);
               return Results.NoContent();
           }).RequireAuthorization("Admin");


        
        group.MapDelete("/{id:int}", async (int id, IBookRepository repo) =>
        {
            if (id <= 0) return Results.BadRequest("Invalid book ID.");

            await repo.DeleteAsync(id);
            return Results.NoContent();
        }).RequireAuthorization("Admin");


        // endpoint to allow users to filter and sort the books
        group.MapGet("/books", async (
            IBookRepository bookRepository,
            string? title,
            string? author,
            string? genre,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy) =>
        {
            var books = await bookRepository.GetFilteredBooksAsync(title, author, genre, minPrice, maxPrice, sortBy);
            return books.Any() ? Results.Ok(books) : Results.NotFound(new { message = "No books found" });
        });


        // merging the search and sorting and filter endpoints
        group.MapGet("/api/books", async (
        IBookRepository bookRepository, 
        string? search, 
        string? genre, 
        string? author, 
        decimal? minPrice, 
        decimal? maxPrice, 
        string? sortBy, 
        bool descending = false, 
        int page = 1, 
        int pageSize = 10) =>
        {
            var (books, totalBooks) = await bookRepository.GetBooksAsync(
                search, genre, author, minPrice, maxPrice, sortBy, descending, page, pageSize);
            
            var response = new
            {
                TotalBooks = totalBooks,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalBooks / pageSize),
                Data = books
            };

            return Results.Ok(response);
    });


        // return group
        return group;

    }



}