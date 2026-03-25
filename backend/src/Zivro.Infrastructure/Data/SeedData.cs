using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Zivro.Infrastructure.Data
{
    /// <summary>
    /// Database seeding class for initial data population.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Seeds the database with initial data including a power user.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="passwordHasher">The password hasher for securing passwords.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SeedAsync(ZivroDbContext context, IPasswordHasher passwordHasher)
        {
            try
            {
                // Check if power user already exists
                var existingUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == "poweruser@zivro.com");

                if (existingUser != null)
                {
                    Console.WriteLine("✓ Power user already exists. Skipping seed.");
                    return;
                }

                // Create power user
                var powerUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Name = "Power User",
                    Email = "poweruser@zivro.com",
                    PasswordHash = passwordHasher.Hash("PowerUser@123456"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(powerUser);
                await context.SaveChangesAsync();

                Console.WriteLine("✓ Power user created successfully.");
                Console.WriteLine($"  Email: {powerUser.Email}");
                Console.WriteLine($"  Password: PowerUser@123456");
                Console.WriteLine($"  ID: {powerUser.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error seeding database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Seeds the database with multiple test users (development only).
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="passwordHasher">The password hasher for securing passwords.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SeedTestUsersAsync(ZivroDbContext context, IPasswordHasher passwordHasher)
        {
            try
            {
                var userCount = await context.Users.CountAsync();
                if (userCount > 1) // More than just power user
                {
                    Console.WriteLine("✓ Test users already exist. Skipping seed.");
                    return;
                }

                var testUsers = new List<User>
                {
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Name = "Regular User",
                        Email = "user@zivro.com",
                        PasswordHash = passwordHasher.Hash("RegularUser@123"),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test User",
                        Email = "test@zivro.com",
                        PasswordHash = passwordHasher.Hash("TestUser@123"),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Name = "Demo User",
                        Email = "demo@zivro.com",
                        PasswordHash = passwordHasher.Hash("DemoUser@123"),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Users.AddRangeAsync(testUsers);
                await context.SaveChangesAsync();

                Console.WriteLine($"✓ {testUsers.Count} test users created successfully.");
                foreach (var user in testUsers)
                {
                    Console.WriteLine($"  • {user.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error seeding test users: {ex.Message}");
                throw;
            }
        }
    }
}
