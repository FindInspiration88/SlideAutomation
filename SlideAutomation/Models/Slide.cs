using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace SlideAutomation.Models
{
    [DataContract]
    public class Slide
    {
        [DataMember]
        public string PathToBackgroundPicture { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Text { get; set; }

    }
}