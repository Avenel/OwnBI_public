using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.ViewModels
{
    public class TagCloudViewModel
    {
        public string Title { get; set; }
        public Dictionary<string, float> Tags { get; set; }
    }
}