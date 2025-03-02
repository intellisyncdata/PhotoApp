using DemoPhotoBooth.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Version = DemoPhotoBooth.Models.Entities.Version;

namespace DemoPhotoBooth.DataContext
{
    public class CommonDbDataContext : DbContext
    {
        public CommonDbDataContext()
        {

        }

        public DbSet<PhotoApp> PhotoApps { get; set; }
        public DbSet<CommonData> CommonDatas { get; set; }
        public DbSet<LayoutApp> LayoutApp { get; set; }
        public DbSet<SvgInfor> SvgInfors { get; set; }
        public DbSet<SvgRectTag> SvgRectTags { get; set; }
        public DbSet<Version> Versions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Version>().HasData(new Version
            {
                Id = 1,
                VersionNo = "0.0.0",
                PackageUrl = string.Empty
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dic = Directory.GetCurrentDirectory();
            var dbFile = $"{dic}/Data/db.db";
            var connectionString = $"Data source={dbFile};";

            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
