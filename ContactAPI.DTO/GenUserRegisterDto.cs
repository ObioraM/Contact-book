using System;
using System.ComponentModel.DataAnnotations;

namespace ContactAPI.DTO
{
    public class GenUserRegisterDto
    {
        [Required]
        [MinLength(1),MaxLength(20)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(1), MaxLength(20)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
