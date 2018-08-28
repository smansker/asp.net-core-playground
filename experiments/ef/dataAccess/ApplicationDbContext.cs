using Microsoft.EntityFrameworkCore;

namespace dataAccess
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Movie> Movies { get; set; }
    }
}
