using Microsoft.EntityFrameworkCore;
using accapp001.Properties.Models;

namespace accapp001.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Employee entity
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);
                    
                entity.Property(e => e.Department)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Seed data
            modelBuilder.Entity<Employee>().HasData(
                new Employee 
                { 
                    Id = 1, 
                    Name = "John Doe", 
                    Email = "john.doe@company.com", 
                    Department = "Information Technology",
                    CreatedDate = new DateTime(2024, 1, 15),
                    IsActive = true
                },
                new Employee 
                { 
                    Id = 2, 
                    Name = "Jane Smith", 
                    Email = "jane.smith@company.com", 
                    Department = "Human Resources",
                    CreatedDate = new DateTime(2024, 2, 10),
                    IsActive = true
                },
                new Employee 
                { 
                    Id = 3, 
                    Name = "Mike Johnson", 
                    Email = "mike.johnson@company.com", 
                    Department = "Finance",
                    CreatedDate = new DateTime(2024, 3, 22),
                    IsActive = true
                }
            );
        }
    }
}