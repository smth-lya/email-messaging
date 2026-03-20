using Homework.EfCoreInAction.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homework.EfCoreInAction.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { }

    public DbSet<Book> Books => Set<Book>();
}