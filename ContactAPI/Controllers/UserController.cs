using ContactAPI.Common;
using ContactAPI.DTO;
using ContactAPI.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactAPI.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<GenUser> _userManager;
        private readonly SignInManager<GenUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenGeneration _tokenGen;

        public UserController(UserManager<GenUser> userManager,
            SignInManager<GenUser> signInManager, RoleManager<IdentityRole> roleManager,
            ITokenGeneration tokenGen)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenGen = tokenGen;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] GenUserRegisterDto user)
        {
            // check if user exists.
            var exists = await _userManager.FindByEmailAsync(user.Email);

            if (exists != null)
            {
                return BadRequest("Email already exists.");
            }

            var model = new GenUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.Email
            };

            if (await _roleManager.FindByNameAsync("Regular") == null)
            {
                await _roleManager.CreateAsync(new IdentityRole("Regular"));
            }

            var attempt = await _userManager.CreateAsync(model, user.Password);


            if (!attempt.Succeeded)
                return BadRequest("Not succeeded");


            await _userManager.AddToRoleAsync(model, "Regular");

            return Ok("Registration successful");
        }

        [HttpPost]
        [Route("mkadmin/{id}")]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            var exist = await _userManager.FindByIdAsync(id);

            if (exist == null)
            {
                return NotFound("User does not exist");
            }

            if (await _roleManager.FindByNameAsync("Admin") == null)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var attempt = await _userManager.AddToRoleAsync(exist, "Admin");

            if (!attempt.Succeeded)
                return StatusCode(500,"Attempt not successful");

            return Ok("Upgrade to Admin successful");
        }

        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LogIn([FromBody]LogInDto logInDetail)
        {
            var exist = await _userManager.FindByEmailAsync(logInDetail.Email);

            if (exist == null)
            {
                return NotFound("Email does not exist.");
            }

            var response = await _signInManager.PasswordSignInAsync(exist, logInDetail.Password, false, false);

            if (!response.Succeeded)
            {
                return BadRequest("Invalid login details");
            }
            var getToken = await _tokenGen.GenerateToken(exist);

            var token = new LogInResponseDto
            {
                Token = getToken,
            };

            return Ok(token);

        }


    }
}
