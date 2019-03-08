using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace SlideAutomation.Models
{
    public class DbInitializer : DropCreateDatabaseAlways<SlideContext>
    {
        protected override void Seed(SlideContext context)
        {
            base.Seed(context);
        }
    }
}