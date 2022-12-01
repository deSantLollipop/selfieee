using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Selfeee_API
{
    public class DBInteractor : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = Path.GetFullPath(@"\..\..\..\..\selfeee_db\user.db3");
            path = string.Format(@"Data Source = {0};", path);
            optionsBuilder.UseSqlite(path);
        }

        public DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().HasData(
            new Users() { Id = 1, UserName = "admin", Password = "admin", ImageProfile = null, ImagesPath = @"E:\selfeee_db\images\1_admin" }
            );
        }
    }
}
