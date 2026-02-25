using Microsoft.EntityFrameworkCore;
using CatalogoWeb.Models;

namespace CatalogoWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        );

        //Nota: deberia mejorar esto con un data seed y evitar problemas con HasData
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "admin",
                Username = "admin", 
                PasswordHash = "AQAAAAIAAYagAAAAEPSFJYDy5DDLgxP5xAP6peZIynbEaDAiiMN3uhjUqdIMvKMm0WGYYx3bdtHAzRgMLA==",
                RoleId = 1
            }
        );
    }
}