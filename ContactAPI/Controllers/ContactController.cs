using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ContactAPI.DTO;
using ContactAPI.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contactRepo;
        private readonly IConfiguration _configuration;
        private Cloudinary _cloudinary;

        public ContactController(IContactRepository contactRepo, IConfiguration configuration)
        {
            _contactRepo = contactRepo;
            _configuration = configuration;

            Account account = new Account
            {
                Cloud = _configuration.GetSection("CloudinarySettings:CloudName").Value,
                ApiKey = _configuration.GetSection("CloudinarySettings:ApiKey").Value,
                ApiSecret = _configuration.GetSection("CloudinarySettings:ApiSecret").Value
            };
            _cloudinary = new Cloudinary(account);
        }


        [HttpPost]
        [Route("add-new")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddContact([FromBody]ContactToAddDto contact)
        {
            var contactExists = _contactRepo.ContactExists(contact.PhoneNumber, contact.FirstName);

            if (contactExists)
            {
                return BadRequest("Contact already exists.");
            }

            var newContact = _contactRepo.CreateContact(contact.FirstName, contact.LastName, contact.Email, contact.Address, contact.PhoneNumber, contact.PhotoPath);

            bool addContactSuccess = await _contactRepo.AddContact(newContact);

            if (addContactSuccess)
            {
                return Ok("Successfully added");
            }

            return StatusCode(500, "Not successfully added");
            
        }

        [HttpGet]
        [Route("{email_or_id}")]
        [Authorize(Roles = ("Admin, Regular"))]
        public ActionResult GetContactbyEmailOrId(string email_or_id)
        {
            Contact fetchContact = _contactRepo.GetContactByQuery(email_or_id);

            if (fetchContact == null) return BadRequest("Contact does not exist");

            ContactToReturnDto contact = new ContactToReturnDto
            {
                Id = fetchContact.Id,
                FirstName = fetchContact.FirstName,
                LastName = fetchContact.LastName,
                Email = fetchContact.Email,
                Address = fetchContact.Address,
                PhoneNumber = fetchContact.PhoneNumber,
                PhotoPath = fetchContact.PhotoPath
            };

            return Ok(contact);

        }

        [HttpPut]
        [Route("update/{id}")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult> UpdateContact(string id,[FromBody]ContactToUpdateDto contactUpdate)
        {
            Contact fetchContact = _contactRepo.GetContactByQuery(id);

            if (fetchContact == null) return BadRequest("Contact does not exist");

            fetchContact.FirstName = contactUpdate.FirstName != null ? contactUpdate.FirstName : fetchContact.FirstName;
            fetchContact.LastName = contactUpdate.LastName != null ? contactUpdate.LastName : fetchContact.LastName;
            fetchContact.PhoneNumber = contactUpdate.PhoneNumber != null ? contactUpdate.PhoneNumber : fetchContact.PhoneNumber;
            fetchContact.PhotoPath = contactUpdate.PhotoPath != null ? contactUpdate.PhotoPath : fetchContact.PhotoPath;
            fetchContact.Email = contactUpdate.Email != null ? contactUpdate.Email : fetchContact.PhotoPath;
            fetchContact.Address = contactUpdate.Address != null ? contactUpdate.Address : fetchContact.Address;

            var isUpdated = await _contactRepo.UpdateContact(fetchContact);

            if (!isUpdated) return StatusCode(500, "Not successfully added.");

            Contact getUpdatedContact = _contactRepo.GetContactByQuery(fetchContact.Id);

            if (getUpdatedContact == null) return StatusCode(500, "Try again Later");

            var returnUpdatedContact = new ContactToReturnDto
            {
                FirstName = getUpdatedContact.FirstName,
                LastName = getUpdatedContact.LastName,
                Email = getUpdatedContact.Email,
                Address = getUpdatedContact.Address,
                PhotoPath = getUpdatedContact.PhotoPath,
                PhoneNumber = getUpdatedContact.PhoneNumber
            };

            return Ok(returnUpdatedContact);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize(Roles = ("Admin"))]
        public async Task<ActionResult> DeleteContactById(string id)
        {
            Contact fetchContact = _contactRepo.GetContactByQuery(id);

            if (fetchContact == null) return BadRequest("Contact does not exist");

            bool isDeleted = await _contactRepo.DeleteContact(fetchContact);

            if (isDeleted == false) return StatusCode(500, "Sorry, try again later");

            return Ok("Contact deleted");
        }


        [HttpPatch]
        [Route("photo/{id}")]
        [Authorize(Roles = ("Admin, Regular"))]
        public async Task<ActionResult> AddPatchPhoto(string id, [FromForm] UpdatePhotoDto photoUpdateDto)
        {
            var contactToUpdate = _contactRepo.GetContactByQuery(id);
            if (contactToUpdate == null)
            {
                return NotFound("Contact Not Found!");
            }

            var file = photoUpdateDto.PhotoUrl;
            if (file.Length <= 0) return BadRequest("Invalid File Size");
            var imageUploadResult = new ImageUploadResult();
            using (var fs = file.OpenReadStream())
            {
                var imageUploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, fs),
                    Transformation = new Transformation().Width(300).Height(300).Crop("fill").Gravity("face")
                };
                imageUploadResult = await _cloudinary.UploadAsync(imageUploadParams);
            }

            var publicId = imageUploadResult.PublicId;
            var Url = imageUploadResult.Url.ToString();
            contactToUpdate.PhotoPath = Url;
            var photoIsUpdatd = await _contactRepo.UpdateContact(contactToUpdate);
            if (!photoIsUpdatd)
            {
                return StatusCode(500, "Something went wrong, try again");
            }
            return Ok($"Photo Path Successfully Updated {publicId}");
        }

        

        [HttpGet]
        //[Route("search")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllContactbySearch([FromQuery] PagingDto model)
        {
            var contacts = _contactRepo.GetAllContacts().ToList();
            
            if (contacts.Count <= 0)
            {
                return NotFound("No Contacts Available in your phone book");
            }
            if (!string.IsNullOrEmpty(model.QuerySearch))
            {
                contacts = contacts.Where(x => x.FirstName.ToLower().Contains(model.QuerySearch.ToLower())
                || x.PhoneNumber.Contains(model.QuerySearch)).ToList();
            }
            var count = contacts.Count();
            var currentPage = model.PageNumber;
            var pageSize = model.PageSize;
            var totalCount = count;
            var totalPages = (int)Math.Ceiling(count / (double)pageSize);
            var items = contacts.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            var previousPage = currentPage > 1 ? "Yes" : "No";
            var nextPage = currentPage < totalPages ? "Yes" : "No";
            var pagination = new
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                previousPage,
                nextPage,
                QuerySearch = string.IsNullOrEmpty(model.QuerySearch) ? "No Paramater Passed" : model.QuerySearch
            };
            HttpContext.Response.Headers.Add("Pagin-Header", JsonConvert.SerializeObject(pagination));
            return Ok(items);
        }
    }
}
