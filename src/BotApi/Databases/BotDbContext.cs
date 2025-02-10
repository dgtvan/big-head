using BotApi.Databases.Enums;
using Microsoft.EntityFrameworkCore;
using BotApi.Databases.Models;
using File = BotApi.Databases.Models.File;
using Thread = BotApi.Databases.Models.Thread;

namespace BotApi.Databases
{
    public class BotDbContext : DbContext
    {
        // Constructor
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Thread> Threads { get; set; } = null!;
        public DbSet<File> Files { get; set; } = null!;
        public DbSet<AIThread> AiThreads { get; set; } = null!;
        public DbSet<AIAssistant> AiAssistants { get; set; } = null!;
        public DbSet<AIMessage> AiMessages { get; set; } = null!;
        public DbSet<AiPrompt> AiUserMessagePrompts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Author Entity Configuration
            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable(nameof(Author));
                entity.HasKey(a => a.Id);
                entity.Property(a => a.ReferenceId).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            });

            // Thread Entity Configuration
            modelBuilder.Entity<Thread>(entity =>
            {
                entity.ToTable(nameof(Thread));
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ReferenceId).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Name).HasMaxLength(200);
                entity.Property(t => t.Type).IsRequired()
                    .HasConversion(
                        v => v.ToString(),  // Enum to string
                        v => Enum.Parse<ThreadType>(v) // String to enum (generic Enum.Parse)
                    );

                entity.HasOne(t => t.AiThread)
                      .WithMany(at => at.Threads)
                      .HasForeignKey(t => t.AiThreadId);
            });

            // Message Entity Configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable(nameof(Message));
                entity.HasKey(m => m.Id);
                entity.Property(m => m.ReferenceId).IsRequired().HasMaxLength(200);
                entity.Property(m => m.Text).IsRequired().HasMaxLength(2048);
                entity.Property(m => m.Timestamp).IsRequired();

                entity.HasOne(m => m.Author)
                      .WithMany(a => a.Messages)
                      .HasForeignKey(m => m.AuthorId);

                entity.HasOne(m => m.Thread)
                      .WithMany(t => t.Messages)
                      .HasForeignKey(m => m.ThreadId);

                entity.HasOne(m => m.AiMessage)
                      .WithMany(am => am.Messages)
                      .HasForeignKey(m => m.AiMessageId);
            });

            // File Entity Configuration
            modelBuilder.Entity<File>(entity =>
            {
                entity.ToTable(nameof(File));
                entity.HasKey(f => f.Id);
                entity.Property(f => f.ReferenceId).IsRequired().HasMaxLength(200);
                entity.Property(f => f.FileName).IsRequired().HasMaxLength(1000);
                entity.Property(f => f.FileHashSha512).IsRequired().HasMaxLength(256);
            });

            // AIThread Entity Configuration
            modelBuilder.Entity<AIThread>(entity =>
            {
                entity.ToTable(nameof(AIThread));
                entity.HasKey(at => at.Id);
                entity.Property(at => at.ReferenceId).IsRequired().HasMaxLength(200);

                entity.HasOne(at => at.AiAssistant)
                      .WithMany(aa => aa.AIThreads)
                      .HasForeignKey(at => at.AiAssistantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AIAssistant Entity Configuration
            modelBuilder.Entity<AIAssistant>(entity =>
            {
                entity.ToTable(nameof(AIAssistant));
                entity.HasKey(aa => aa.Id);
                entity.Property(aa => aa.ReferenceId).IsRequired().HasMaxLength(200);
            });

            // AIMessage Entity Configuration
            modelBuilder.Entity<AIMessage>(entity =>
            {
                entity.ToTable(nameof(AIMessage));
                entity.HasKey(am => am.Id);
                entity.Property(am => am.ReferenceId).IsRequired().HasMaxLength(200);
                entity.Property(am => am.Text).IsRequired().HasMaxLength(2048);
            });

            // AIUserMessagePrompt Entity Configuration
            modelBuilder.Entity<AiPrompt>(entity =>
            {
                entity.ToTable(nameof(AiPrompt));
                entity.HasKey(ump => ump.Id);
                entity.Property(ump => ump.Prompt).IsRequired();
            });
        }
    }
}