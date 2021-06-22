using System;
using System.Collections.Generic;
using System.Text;

namespace ContactAPI.DTO
{
    public class ContactToReturnDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PhotoPath { get; set; }
        public string Email { get; set; }
    }
}
