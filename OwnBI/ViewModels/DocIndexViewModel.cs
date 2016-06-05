using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.ViewModels
{
    public class DocIndexViewModel
    {
        public List<DocViewModel> Docs { get; set; }

        public List<DocType> DocTypes { get; set; }

        public string Query { get; set; }

        public string Date { get; set; }

        public string DocType { get; set; }

    }
}