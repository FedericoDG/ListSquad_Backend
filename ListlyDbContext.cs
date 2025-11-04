using Microsoft.EntityFrameworkCore;
using listly.Features.User;
using listly.Features.Subscription;
using listly.Features.List;
using listly.Features.Item;
using listly.Features.Invitation;
using listly.Features.Setting;

namespace listly
{
  public class ListlyDbContext(DbContextOptions<ListlyDbContext> options) : DbContext(options)
  {
    public DbSet<User> User { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<PaymentMapping> PaymentMappings { get; set; }
    public DbSet<List> Lists { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ListUsers> ListUsers { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Setting> Settings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // User
      modelBuilder.Entity<User>();

      // Setting
      modelBuilder.Entity<Setting>();

      // Subscription
      modelBuilder.Entity<Subscription>();

      // UserSubscription
      modelBuilder.Entity<UserSubscription>()
        .HasKey(us => new { us.UId, us.SubscriptionId });

      modelBuilder.Entity<UserSubscription>()
        .HasOne(us => us.User)
        .WithMany(u => u.UserSubscriptions)
        .HasForeignKey(us => us.UId)
        .HasConstraintName("FK_UserSubscription_User");

      modelBuilder.Entity<UserSubscription>()
        .HasOne(us => us.Subscription)
        .WithMany(s => s.UserSubscriptions)
        .HasForeignKey(us => us.SubscriptionId)
        .HasConstraintName("FK_UserSubscription_Subscription");

      // List - relacion con Owner
      modelBuilder.Entity<List>()
        .HasOne(l => l.Owner)
        .WithMany()
        .HasForeignKey(l => l.OwnerUid)
        .HasPrincipalKey(u => u.UId)
        .OnDelete(DeleteBehavior.Restrict);

      // UserList - relacion muchos a muchos
      modelBuilder.Entity<ListUsers>()
        .HasKey(ul => new { ul.UId, ul.ListId });

      // Relaciones de UserList
      modelBuilder.Entity<ListUsers>()
        .HasOne(ul => ul.User)
        .WithMany(u => u.ListUsers)
        .HasForeignKey(ul => ul.UId)
        .HasPrincipalKey(u => u.UId);

      modelBuilder.Entity<ListUsers>()
        .HasOne(ul => ul.List)
        .WithMany(l => l.ListUsers)
        .HasForeignKey(ul => ul.ListId)
        .OnDelete(DeleteBehavior.Cascade);

      // Item relaciones
      modelBuilder.Entity<Item>()
        .HasOne(i => i.List)
        .WithMany(l => l.Items)
        .HasForeignKey(i => i.ListId)
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Item>()
        .HasOne(i => i.CheckedUser)
        .WithMany()
        .HasForeignKey(i => i.CheckedBy)
        .HasPrincipalKey(u => u.UId)
        .OnDelete(DeleteBehavior.SetNull);

      // Invitation relaciones
      modelBuilder.Entity<Invitation>()
        .HasOne(i => i.List)
        .WithMany()
        .HasForeignKey(i => i.ListId)
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Invitation>()
        .HasOne(i => i.FromUser)
        .WithMany()
        .HasForeignKey(i => i.FromUserId)
        .HasPrincipalKey(u => u.UId)
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Invitation>()
        .HasOne(i => i.ToUser)
        .WithMany()
        .HasForeignKey(i => i.ToUserId)
        .HasPrincipalKey(u => u.UId)
        .OnDelete(DeleteBehavior.Restrict);

      // Setting relaciones (uno a uno con User)
      modelBuilder.Entity<Setting>()
        .HasOne(s => s.User)
        .WithOne(u => u.Setting)
        .HasForeignKey<Setting>(s => s.UserUid)
        .HasPrincipalKey<User>(u => u.UId)
        .OnDelete(DeleteBehavior.Cascade);

      // Seeders
      SeedUsers(modelBuilder);
      SeedSettings(modelBuilder);
      SeedSubscriptions(modelBuilder);
      SeedUsersSubscriptions(modelBuilder);
      SeedLists(modelBuilder);
      SeedListUsers(modelBuilder);
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<User>().HasData(
        new User
        {
          UId = "57cV0RNcC9bnCKzbcNiBp1Tzr822",
          Email = "alambratore@gmail.com",
          DisplayName = "Federico González",
          PhotoUrl = "https://lh3.googleusercontent.com/a/ACg8ocJPzlf1C9crnRGlPqM8B-86Xm68mJmqQ-2oGr7kW11g4apmgPg=s96-c",
          FcmToken = ""
        },
        new User
        {
          UId = "hQZnBgjuOwTTGIOVWgGHsE95OOX2",
          Email = "steamsecundario@gmail.com",
          DisplayName = "Steam Secundario",
          PhotoUrl = "https://lh3.googleusercontent.com/a/ACg8ocK35d11bNqi6k1mqlcxakZ7QFmnPHbkbYs4KZ5k47XwxC8YuA=s96-c",
          FcmToken = ""
        },

        new User
        {
          UId = "IMImLkBokLWuwjRGy7Jm7wnBWAB3",
          Email = "fogondehugo@gmail.com",
          DisplayName = "Fogón de Hugo",
          PhotoUrl = "https://lh3.googleusercontent.com/a/ACg8ocKdkpsOMhRxy-XTt2CyCGKNLs9sQLThXYsv4ZJpQYCWx-qMwg=s96-c",
          FcmToken = ""
        }
        );
    }

    private static void SeedSubscriptions(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Subscription>().HasData(
        new Subscription
        {
          SubscriptionId = 1,
          Name = "Free",
          Description = "Plan gratuito con funciones básicas",
          Price = 0.0m
        },
        new Subscription
        {
          SubscriptionId = 2,
          Name = "Premium",
          Description = "Plan premium con todas las funciones",
          Price = 2.00m
        }
      );
    }

    private static void SeedUsersSubscriptions(ModelBuilder modelBuilder)
    {
      var today = DateTime.Today;
      var endDate = today.AddDays(30);

      modelBuilder.Entity<UserSubscription>().HasData(
        new UserSubscription
        {
          UId = "57cV0RNcC9bnCKzbcNiBp1Tzr822",
          SubscriptionId = 1,
          StartDate = today,
          EndDate = endDate
        },
        new UserSubscription
        {
          UId = "hQZnBgjuOwTTGIOVWgGHsE95OOX2",
          SubscriptionId = 1,
          StartDate = today,
          EndDate = endDate
        },
        new UserSubscription
        {
          UId = "IMImLkBokLWuwjRGy7Jm7wnBWAB3",
          SubscriptionId = 1,
          StartDate = today,
          EndDate = endDate
        }
      );
    }

    private static void SeedLists(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<List>().HasData(
        new List
        {
          ListId = 1,
          Title = "Lista de Compras",
          Description = "Lista para la compra semanal",
          Icon = "🛒",
          OwnerUid = "57cV0RNcC9bnCKzbcNiBp1Tzr822",
        }
      );
    }

    private static void SeedListUsers(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<ListUsers>().HasData(
        new ListUsers
        {
          ListId = 1,
          UId = "57cV0RNcC9bnCKzbcNiBp1Tzr822"
        }
      );
    }

    private static void SeedSettings(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Setting>().HasData(
        new Setting
        {
          SettingId = 1,
          UserUid = "57cV0RNcC9bnCKzbcNiBp1Tzr822",
          ReceiveInvitationNotifications = true,
          ReceiveItemAddedNotifications = true,
          ReceiveItemStatusChangedNotifications = true,
          ReceiveItemDeletedNotifications = true
        },
        new Setting
        {
          SettingId = 2,
          UserUid = "hQZnBgjuOwTTGIOVWgGHsE95OOX2",
          ReceiveInvitationNotifications = true,
          ReceiveItemAddedNotifications = true,
          ReceiveItemStatusChangedNotifications = true,
          ReceiveItemDeletedNotifications = true
        },
        new Setting
        {
          SettingId = 3,
          UserUid = "IMImLkBokLWuwjRGy7Jm7wnBWAB3",
          ReceiveInvitationNotifications = true,
          ReceiveItemAddedNotifications = true,
          ReceiveItemStatusChangedNotifications = true,
          ReceiveItemDeletedNotifications = true
        }
      );
    }
  }
}
