using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Dtos
{
    public record ForgotPasswordDto
    {

        public string Email {get; set;}
        
    }
}