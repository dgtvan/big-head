using BotApi.Databases.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Thread = BotApi.Databases.Models.Thread;

public class BotDbContext : DbContext
{
    public BotDbContext(DbContextOptions<BotDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Thread> Threads { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Author entity
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable(nameof(Author));
            entity.HasKey(a => a.Id);
        });

        // Configure Message entity
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable(nameof(Message));
            entity.HasKey(m => m.Id);

            entity.HasOne(m => m.Author)
                  .WithMany()
                  .HasForeignKey(m => m.AuthorId);

            entity.HasOne(m => m.Thread)
                  .WithMany()
                  .HasForeignKey(m => m.ThreadId);
        });

        // Configure Thread entity
        modelBuilder.Entity<Thread>(entity =>
        {
            entity.ToTable(nameof(Thread));
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Type).HasConversion(new EnumToStringConverter<ThreadType>());
        });
    }
}
