using Microsoft.EntityFrameworkCore;

namespace poc_amax_attachments.Data
{
    public class AppDbContext: DbContext
    {
        public DbSet<tblSupplemental> tblSupplementals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure the context to use SQL Server and specify the connection string
            optionsBuilder.UseSqlServer("server=.;Initial Catalog=IP_AMAX_Local;User ID=sa;Password=123;Connection Timeout=0;TrustServerCertificate=true;");
        }

    }
}
