using System;
using Library.API.Auth;
using Library.API.Controllers;
using Library.API.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var u = new User
            {
                Id = new Guid().ToString(),
                UserName = "Marko",
                Password = "password"
            };

            var now = DateTime.Now;
            var expiresIn = now + TokenAuthOptions.ExpiresSpan;
            var rValue = UsersController.GenerateToken(u, expiresIn);
        }
    }

    public static class asd
    {
        public static string GenerateToken(User user, DateTime expiresIn)
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