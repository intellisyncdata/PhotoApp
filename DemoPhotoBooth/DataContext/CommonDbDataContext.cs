using DemoPhotoBooth.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
