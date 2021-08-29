using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<
        AppUser,
        AppRole, 
        int, 
        IdentityUserClaim<int>, 
        AppUserRole, 
        IdentityUserLogin<int>, 
        IdentityRoleClaim<int>,
        IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) 
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasMany(q => q.UserRoles)
                .WithOne(q => q.User)
                .HasForeignKey(q => q.UserId)
                .IsRequired();

            builder.Entity<AppRole>()
                .HasMany(q => q.UserRoles)
                .WithOne(q => q.Role)
                .HasForeignKey(q => q.RoleId)
                .IsRequired();

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
