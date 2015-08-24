﻿using System;
using System.Data.Common;
using System.Data.Entity;
using System.Diagnostics;

namespace Lyra.Models.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbConnection connection) : base(connection, true)
        {
            // Log
            this.Database.Log = s => Debug.WriteLine($"SQL Log => {Environment.NewLine}{s}");

            // Disable Migration by Entity Framework
            System.Data.Entity.Database.SetInitializer<AppDbContext>(null);
        }

        // -- Tracks --
        public DbSet<Track> Tracks { get; set; }

        public DbSet<Artist> Artists { get; set; }

        public DbSet<Album> Albums { get; set; }

        // -- WatchList --
        public DbSet<Location> Locations { get; set; }
    }
}