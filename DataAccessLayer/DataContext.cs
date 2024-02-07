﻿using Microsoft.EntityFrameworkCore;
using BusinessObject;

namespace DataAccessLayer
{
    public class DataContext : DbContext
    {
        public DataContext() { }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Friend> Friend { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Participants> Participants { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=(local);uid=sa;pwd=12345;database=ChatSystem;\nTrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure primary key for Friend entity
            modelBuilder.Entity<Friend>()
                .HasKey(f => f.RequestId);

            //one user can send friend request to many users
            //when delete this user, also delete the friend request
            modelBuilder.Entity<Friend>()
                .HasOne(s => s.SenderUser)
                .WithMany(l => l.SentUsers)
                .HasForeignKey(s => s.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            //one user can be sent by many users
            modelBuilder.Entity<Friend>()
                .HasOne(s => s.RecipientUser)
                .WithMany(l => l.SentByUsers)
                .HasForeignKey(s => s.RecipientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(u => u.Conversation)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Participants>()
                .HasKey(f => f.ConversationId);

            modelBuilder.Entity<Participants>()
                .HasOne(p => p.Conversation)
                .WithMany()
                .HasForeignKey(p => p.ConversationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Participants>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
