using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Dtos
{
    public record ResetPasswordDto
    {
        public string Email { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
        
        
    }
}