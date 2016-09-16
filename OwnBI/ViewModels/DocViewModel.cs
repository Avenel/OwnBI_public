using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.ViewModels
{
    public class DocViewModel
    {
        public Guid Id { get; set; }
        
        public string Type { get; set; }

        public Guid TypeId { get; set; }

        public string Name { get; set; }

        public List<MetaTag> MetaTags { get; set; }

        public Dictionary<string, string> Values { get; set; }

		public Dictionary<string, List<string>> AutoCompletes { get; set; }
    }
}