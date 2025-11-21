using Entities;
using Microsoft.EntityFrameworkCore;

namespace RestAPI
{
    public class AppDbContext : DbContext
    {

        private readonly IConfiguration _configuration;


        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ToDoTask> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToDoTask>()
                .HasOne(task => task.User)
                .WithMany(user => user.Tasks)
                .HasForeignKey(task => task.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
