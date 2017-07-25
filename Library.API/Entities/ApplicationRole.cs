using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Library.API.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string IpAddress { get; set; }
    }
}