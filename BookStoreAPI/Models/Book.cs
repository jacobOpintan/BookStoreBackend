using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
//applying the data annotations to the properties of the Book class

    [Required]
    [StringLength(maximumLength:50,MinimumLength =3,ErrorMessage="Title cannot be more than 50 characters")]
    [NotNull]
        public string Title { get; set; } =string.Empty;


        [Required]
        [StringLength(maximumLength:25,ErrorMessage="Author cannot be more than 25 characters",MinimumLength =5)]
        public string Author { get; set; }= string.Empty;

        [Required]
        [StringLength(maximumLength:15,ErrorMessage="Genre cannot be more than 15 characters",MinimumLength =3)]
        public string Genre {get ; set;}= string.Empty;

        // specify the [Column(TypeName = "decimal(18,2)")] data annotation to specify the precision of the price
        [Required]
        [Range(1,1000,ErrorMessage="Price must be between 1 and 1000")]
        public decimal Price { get; set; }

        
        public int Stock { get; set; }
        
        
    }
}