using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Auth;
using Library.API.Entities;
using Library.API.Model;
using Library.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Library.API.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {
        private ILibraryRepository _libraryRepository;

        private RoleManager<ApplicationRole> _roleManager;
        private UserManager<User> _userManager;

        public UsersController(ILibraryRepository libraryRepository,
            UserManager<User> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _libraryRepository = libraryRepository;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        //asdasd
        //this is a test
        //Returns 200 OK and an auth token if uName and pass exist in db, else returns 422 Unprocessable entity with a message that it's not valid u/p combination
        [Consumes("application/json")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto user)
        {
            var v = user?.UserName == null || user.Email == null || user.Password == null ||
                    user.ApplicationRoleId == null;
            if (v)
            {
                return BadRequest("You have to provide all info on user");
            }
            var userForStore = Mapper.Map<User>(user);
            if (userForStore != null)
            {
                try
                {
                    var result = await _userManager.CreateAsync(userForStore, user.Password);

                    if (result.Succeeded)
                    {
                        var applicationRole = await _roleManager.FindByIdAsync(user.ApplicationRoleId);
                        if (applicationRole != null)
                        {
                            var roleResult = await _userManager.AddToRoleAsync(userForStore, applicationRole.Name);
                            if (roleResult.Succeeded)
                            {
                                return Created("blabla", userForStore);
                            }
                        }
                        return BadRequest("Application role doesnt exist!");
                    }
                    return BadRequest(result.Errors);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("THIS IS EXC: " + e);
                    return BadRequest(e);
                }
            }
            return BadRequest("Something went wrong");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserForLogInDto user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            //var userFromDb = _libraryRepository.GetUser(user.UserName, user.Password);
            var userFromDb = await _userManager.FindByNameAsync(user.UserName);
            if (userFromDb != null && await _userManager.CheckPasswordAsync(userFromDb, user.Password))
            {
                return StatusCode(422, "Username or password is invalid");
            }
            var requestedAt = DateTime.Now;
            var expiresIn = requestedAt + TokenAuthOptions.ExpiresSpan;
            var token = GenerateToken(userFromDb, expiresIn);
            return Ok(JsonConvert.SerializeObject(new
            {
                requertAt = requestedAt,
                expiresIn = TokenAuthOptions.ExpiresSpan.TotalSeconds,
                accessToken = token
            }));
        }

        private static string GenerateToken(User user, DateTime expiresIn)
        {
            var handler = new JwtSecurityTokenHandler();
            var identity = new ClaimsIdentity(
                new GenericIdentity(user.UserName, "TokenAuth"),
                new[]
                {
                    new Claim("ID", user.Id)
                }
            );
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = TokenAuthOptions.Issuer,
                Audience = TokenAuthOptions.Audience,
                SigningCredentials = TokenAuthOptions.SigningCredentials,
                Subject = identity,
                Expires = expiresIn
            });
            return handler.WriteToken(securityToken);
        }
    }
}