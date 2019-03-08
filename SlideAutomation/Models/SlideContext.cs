using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace SlideAutomation.Models
{
    public class SlideContext : DbContext
    {
        public DbSet<Slide> Slides { get; set; }
    }
}