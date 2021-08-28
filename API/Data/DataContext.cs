using API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) 
        {
            base.OnModelCreating(builder);

            builder.Entity<UserLike>()
                .HasKey(q => new {q.SourceUserId, q.LikedUserId});

            builder.Entity<UserLike>()
                .HasOne(q => q.SourceUser)
                .WithMany(q => q.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(q => q.LikedUser)
                .WithMany(q => q.LikedByUsers)
                .HasForeignKey(s => s.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Message>()
                .HasOne(q => q.Recipient)
                .WithMany(q => q.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
                            
            builder.Entity<Message>()
                .HasOne(q => q.Sender)
                .WithMany(q => q.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
