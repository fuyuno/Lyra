﻿using System.Data.Entity;

namespace Lyra.Models.Database
{
    public class LocationDbContext : DbContext
    {
        public LocationDbContext() : base(DatabaseConnectionProvider.GetConnection(), true)
        {
            // Disable Migration by Entity Framework
            System.Data.Entity.Database.SetInitializer<LocationDbContext>(null);
        }

        // -- WatchList --
        public DbSet<Location> Locations { get; set; }
    }
}