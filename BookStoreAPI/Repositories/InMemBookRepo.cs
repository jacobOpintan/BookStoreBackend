using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStoreAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreAPI.Repositories
{

    // this is an interface that will be implemented by the InMemBookRepo class
    //it was intentionally created to improve loose coupling
    //it was intentionally create in the same namepsace 
    public interface IInMemBookRepo
    {
        void Create(Book book);
        void delete(Book deletebook);
        Book Get(int id);
        List<Book> GetAll();
        void Update(Book book);
    }
// inte
    public class InMemBookRepo :  IInMemBookRepo
    {

        // creating a list of books 
        private readonly List<Book> books = new() {
            new Book () {Id=1 ,Title="The Journey to the West",Author ="Mimi Brown",Genre="Action",Price=20.00M ,Stock=3},
            new Book () {Id=2 ,Title="On My Way Home",Author ="Mimi Brown",Genre="Adventure",Price=45.00M ,Stock=4},
            new Book () {Id=3 ,Title="The Lost Kingdom",Author ="Mimi Brown",Genre="Mystery",Price=40.00M ,Stock=5},
            new Book () {Id=4 ,Title="At a Time",Author ="Mimi Brown",Genre="Romantic",Price=15.00M ,Stock=4},
            new Book () {Id=5 ,Title="Rich Dad Poor Dad",Author ="Mimi Brown",Genre="Inspirational",Price=25.00M ,Stock=7},
            new Book () {Id=6 ,Title="In the Zoo",Author ="Mimi Brown",Genre="Comedy",Price=40.00M ,Stock=9},
            new Book () {Id=7 ,Title="Cage ",Author ="Mimi Brown",Genre="Action",Price=30.00M ,Stock=2}

        };



        //Implementing IBookrepositoryto improve loose coupling
        public void Create(Book book)
        {
            // creating the id for the book
            book.Id = books.Max(b => b.Id) + 1;
            // adding the book to the list of books
            books.Add(book);

        }

        public void delete(Book deletebook)
        {

            //try searching for the book before deletion
            try
            {
                var index = books.FindIndex(book => book.Id == deletebook.Id);
                if (deletebook.Id == index)
                {
                    books.RemoveAt(index);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("The book to be deleted was not found " + e.Message);

            }



        }

        public Book Get(int id)
        {
            Book? book = books.Find(b => b.Id == id);

            if (book is null)
            {
                return (Book)Results.NotFound();
            }
            else
            {
                return (Book)Results.Ok(new { book.Id, book.Title, book.Author, book.Genre, book.Price, book.Stock });
            }

        }

        public List<Book> GetAll()
        {
            return books.ToList();

        }

        public void Update(Book book)
        {
            var index = books.FindIndex(b => b.Id == book.Id);
            //im now going to update the book
            books[index] = book;
        }


    }
}