using BotApi.Databases.Enums;
using Microsoft.EntityFrameworkCore;
using BotApi.Databases.Models;
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
        public DbSet<PrompTemplate> PromptTemplates { get; set; } = null!;
        public DbSet<OpenAiThread> OpenAiThreads { get; set; } = null!;
        public DbSet<OpenAiAssistant> OpenAiAssistants { get; set; } = null!;
        public DbSet<OpenAiMessage> OpenAiMessages { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Author Entity Configuration
            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable(nameof(Author));
                entity.HasKey(a => a.Id);
            });

            // Thread Entity Configuration
            modelBuilder.Entity<Thread>(entity =>
            {
                entity.ToTable(nameof(Thread));
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Type).IsRequired()
                    .HasConversion(
                        v => v.ToString(),  // Enum to string
                        v => Enum.Parse<ThreadType>(v) // String to enum (generic Enum.Parse)
                    );
            });

            // Message Entity Configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable(nameof(Message));
                entity.HasKey(m => m.Id);
            });
            
            // AIUserMessagePrompt Entity Configuration
            modelBuilder.Entity<PrompTemplate>(entity =>
            {
                entity.ToTable(nameof(PrompTemplate));
                entity.HasKey(ump => ump.Id);
            });

            // AIThread Entity Configuration
            modelBuilder.Entity<OpenAiThread>(entity =>
            {
                entity.ToTable(nameof(OpenAiThread));
                entity.HasKey(at => at.ThreadId);
            });

            // AIAssistant Entity Configuration
            modelBuilder.Entity<OpenAiAssistant>(entity =>
            {
                entity.ToTable(nameof(OpenAiAssistant));
                entity.HasKey(x => new { x.ThreadId, OpenAiThreadId = x.OpenAiAssistantId });
            });

            // AIMessage Entity Configuration
            modelBuilder.Entity<OpenAiMessage>(entity =>
            {
                entity.ToTable(nameof(OpenAiMessage));
                entity.HasKey(am => am.MessageId);
            });
        }
    }
}