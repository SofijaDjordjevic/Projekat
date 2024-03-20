using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TagValue> Values { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<AlarmValue> Alarms { get; set; }
    }
}