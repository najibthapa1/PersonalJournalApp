using PersonalJournal.Models;
using PersonalJournal.Models.Enums;
using Microsoft.EntityFrameworkCore;
namespace PersonalJournal.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Users> Users { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<Mood> Moods { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<EntryMood> EntryMoods { get; set; }
    public DbSet<EntryTag> EntryTags { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

     protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //User Configuration
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique(); // Username must be unique
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PinHash).IsRequired();
                entity.Property(e => e.Salt).IsRequired();
            });

            // Journal Entry Configuration
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Unique constraint: One entry per day per user
                entity.HasIndex(e => new { e.UserId, e.EntryDate }).IsUnique();
                
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();

                // Foreign Key: User -> JournalEntries (One-to-Many)
                entity.HasOne(e => e.Users)
                    .WithMany(u => u.JournalEntries)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade); 

                // Foreign Key: Category -> JournalEntries (One-to-Many)
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.JournalEntries)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null if category is deleted
            });

            // Mood Configuration
            modelBuilder.Entity<Mood>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            });

            // Tag Configuration
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                
                // Unique constraint: Tag name + UserId 
                entity.HasIndex(e => new { e.Name, e.UserId }).IsUnique();

                // Foreign Key: User -> Tags 
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Entry Mood Configuration
            modelBuilder.Entity<EntryMood>(entity =>
            {
                // Composite primary key
                entity.HasKey(e => new { e.JournalEntryId, e.MoodId });

                // Foreign Key: JournalEntry -> EntryMoods
                entity.HasOne(e => e.JournalEntry)
                    .WithMany(j => j.EntryMoods)
                    .HasForeignKey(e => e.JournalEntryId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign Key: Mood -> EntryMoods
                entity.HasOne(e => e.Mood)
                    .WithMany(m => m.EntryMoods)
                    .HasForeignKey(e => e.MoodId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Entry Tag Configuration
            modelBuilder.Entity<EntryTag>(entity =>
            {
                // Composite primary key
                entity.HasKey(e => new { e.JournalEntryId, e.TagId });

                // Foreign Key: JournalEntry -> EntryTags
                entity.HasOne(e => e.JournalEntry)
                    .WithMany(j => j.EntryTags)
                    .HasForeignKey(e => e.JournalEntryId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign Key: Tag -> EntryTags
                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.EntryTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User Setting Configuration
            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique(); // One settings record per user

                // Foreign Key: User -> UserSettings (One-to-One)
                entity.HasOne(e => e.User)
                    .WithOne(u => u.UserSettings)
                    .HasForeignKey<UserSettings>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed Data
            SeedMoods(modelBuilder);
            SeedTags(modelBuilder);
            SeedCategories(modelBuilder);
        }

        // Seed Mood
        private void SeedMoods(ModelBuilder modelBuilder)
        {
            var moods = new List<Mood>
            {
                // Positive Moods
                new Mood { Id = 1, Name = "Happy", Category = MoodCategory.Positive },
                new Mood { Id = 2, Name = "Excited", Category = MoodCategory.Positive},
                new Mood { Id = 3, Name = "Relaxed", Category = MoodCategory.Positive },
                new Mood { Id = 4, Name = "Grateful", Category = MoodCategory.Positive},
                new Mood { Id = 5, Name = "Confident", Category = MoodCategory.Positive},
                
                // Neutral Moods
                new Mood { Id = 6, Name = "Calm", Category = MoodCategory.Neutral},
                new Mood { Id = 7, Name = "Thoughtful", Category = MoodCategory.Neutral,},
                new Mood { Id = 8, Name = "Curious", Category = MoodCategory.Neutral},
                new Mood { Id = 9, Name = "Nostalgic", Category = MoodCategory.Neutral},
                new Mood { Id = 10, Name = "Bored", Category = MoodCategory.Neutral },
                
                // Negative Moods
                new Mood { Id = 11, Name = "Sad", Category = MoodCategory.Negative },
                new Mood { Id = 12, Name = "Angry", Category = MoodCategory.Negative },
                new Mood { Id = 13, Name = "Stressed", Category = MoodCategory.Negative },
                new Mood { Id = 14, Name = "Lonely", Category = MoodCategory.Negative},
                new Mood { Id = 15, Name = "Anxious", Category = MoodCategory.Negative }
            };

            modelBuilder.Entity<Mood>().HasData(moods);
        }

        // Seed Tag
        private void SeedTags(ModelBuilder modelBuilder)
        {
            var tags = new List<Tag>
            {
                new Tag { Id = 1, Name = "Work", IsCustom = false, UserId = null },
                new Tag { Id = 2, Name = "Career", IsCustom = false, UserId = null },
                new Tag { Id = 3, Name = "Studies", IsCustom = false, UserId = null },
                new Tag { Id = 4, Name = "Family", IsCustom = false, UserId = null },
                new Tag { Id = 5, Name = "Friends", IsCustom = false, UserId = null },
                new Tag { Id = 6, Name = "Relationships", IsCustom = false, UserId = null },
                new Tag { Id = 7, Name = "Health", IsCustom = false, UserId = null },
                new Tag { Id = 8, Name = "Fitness", IsCustom = false, UserId = null },
                new Tag { Id = 9, Name = "Personal Growth", IsCustom = false, UserId = null },
                new Tag { Id = 10, Name = "Self-care", IsCustom = false, UserId = null },
                new Tag { Id = 11, Name = "Hobbies", IsCustom = false, UserId = null },
                new Tag { Id = 12, Name = "Travel", IsCustom = false, UserId = null },
                new Tag { Id = 13, Name = "Nature", IsCustom = false, UserId = null },
                new Tag { Id = 14, Name = "Finance", IsCustom = false, UserId = null },
                new Tag { Id = 15, Name = "Spirituality", IsCustom = false, UserId = null },
                new Tag { Id = 16, Name = "Birthday", IsCustom = false, UserId = null },
                new Tag { Id = 17, Name = "Holiday", IsCustom = false, UserId = null },
                new Tag { Id = 18, Name = "Vacation", IsCustom = false, UserId = null },
                new Tag { Id = 19, Name = "Celebration", IsCustom = false, UserId = null },
                new Tag { Id = 20, Name = "Exercise", IsCustom = false, UserId = null },
                new Tag { Id = 21, Name = "Reading", IsCustom = false, UserId = null },
                new Tag { Id = 22, Name = "Writing", IsCustom = false, UserId = null },
                new Tag { Id = 23, Name = "Cooking", IsCustom = false, UserId = null },
                new Tag { Id = 24, Name = "Meditation", IsCustom = false, UserId = null },
                new Tag { Id = 25, Name = "Yoga", IsCustom = false, UserId = null },
                new Tag { Id = 26, Name = "Music", IsCustom = false, UserId = null },
                new Tag { Id = 27, Name = "Shopping", IsCustom = false, UserId = null },
                new Tag { Id = 28, Name = "Parenting", IsCustom = false, UserId = null },
                new Tag { Id = 29, Name = "Projects", IsCustom = false, UserId = null },
                new Tag { Id = 30, Name = "Planning", IsCustom = false, UserId = null },
                new Tag { Id = 31, Name = "Reflection", IsCustom = false, UserId = null }
            };

            modelBuilder.Entity<Tag>().HasData(tags);
        }

        // Seed Category
        private void SeedCategories(ModelBuilder modelBuilder)
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Personal" },
                new Category { Id = 2, Name = "Work" },
                new Category { Id = 3, Name = "Health" },
                new Category { Id = 4, Name = "Goals" },
                new Category { Id = 6, Name = "Dreams" },
            };

            modelBuilder.Entity<Category>().HasData(categories);
        }
    }
