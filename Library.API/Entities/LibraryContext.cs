using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Entities
{
    public sealed class LibraryContext : IdentityDbContext<User, ApplicationRole, string>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Author> Authors { get; set; }

        public DbSet<Book> Books { get; set; }
        //  public DbSet<User> Users { get; set; }
    }
}