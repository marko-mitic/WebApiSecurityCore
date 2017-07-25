using System;
using System.Threading.Tasks;
using Library.API.Entities;
using Library.API.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/roles")]
    public class ApplicationRoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ApplicationRoleController(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }


        [HttpPost]
        public async Task<IActionResult> CreateApplicationRole([FromBody] ApplicationRoleDto applicationRole)
        {
            if (applicationRole?.RoleName == null)
            {
                return BadRequest();
            }
            var applicationRoleToCreate = new ApplicationRole
            {
                CreatedDate = DateTime.Now,
                Name = applicationRole.RoleName,
                Description = applicationRole.Description ?? "",
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
            };
            var result = await _roleManager.CreateAsync(applicationRoleToCreate);
            if (result.Succeeded)
            {
                return Created("", applicationRoleToCreate);
            }
            return BadRequest(result.Errors);
        }
    }
}