using OwnBI.Models;
using OwnBI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.ViewModels
{
    public class DocTypeViewModel
    {
        public DocType DocType { get; set; }
        public List<MetaTag> MetaTags {
            get { return MetaTagRepository.Index(); } 
        }
    }
}