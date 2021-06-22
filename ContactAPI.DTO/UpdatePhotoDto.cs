using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContactAPI.DTO
{
    public class UpdatePhotoDto
    {
        public IFormFile PhotoUrl { get; set; }
    }
}
