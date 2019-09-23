using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChatApp.Models;

namespace ChatApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Party Tables

        public DbSet<User> ChatUsers { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserDevice> UserDevice { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<UserPerChat> UserPerChat { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasIndex(b => b.UserName);
            modelBuilder.Entity<UserSession>().HasIndex(b => b.UserName);
            modelBuilder.Entity<UserSession>().HasIndex(b => b.UserID);
            modelBuilder.Entity<UserDevice>().HasIndex(b => b.UserName);
            modelBuilder.Entity<UserDevice>().HasIndex(b => b.UserID);
            modelBuilder.Entity<UserDevice>().HasIndex(b => b.DeviceID);
            modelBuilder.Entity<UserPerChat>().HasIndex(b => b.UserID);
            modelBuilder.Entity<UserPerChat>().HasIndex(b => b.ChatID);
            modelBuilder.Entity<Message>().HasIndex(b => new { b.ChatID, b.Sent });

            Seed(modelBuilder);
        }

        protected void Seed(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<ProductType>().HasData(
            //            new ProductType { ID = 1,  Name = "Tarriff", Description = "Tarriff Type" }
            //            );
        }

    }
}
