using Microsoft.EntityFrameworkCore;
using MessengerBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace MessengerBackend.Data;

public class MessengerDbContext : IdentityDbContext<User>
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<PhoneVerificationCode> PhoneVerificationCodes { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatParticipant> ChatParticipants { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<AIAnalysys> AIAnalysys { get; set; }
    public DbSet<AIAnalysysSeen> AIAnalysysSeen { get; set; }




    public MessengerDbContext(DbContextOptions<MessengerDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.User)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasOne(c => c.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.ContactUser)
                .WithMany()
                .HasForeignKey(c => c.ContactId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<AIAnalysys>()
            .HasKey(a => a.Id);
        modelBuilder.Entity<AIAnalysysSeen>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(az => az.Analysys)
                .WithMany()
                .HasForeignKey(az => az.AnalysysId)
                .OnDelete(DeleteBehavior.NoAction);
        });

    }
}