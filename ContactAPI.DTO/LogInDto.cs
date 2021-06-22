using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ContactAPI.DTO
{
    public class LogInDto
    {
        
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
