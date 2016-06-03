using OwnBI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.ViewModels
{
    public class HomeViewModel
    {
        public TagCloudViewModel NameTagCloud { get; set; }
        public TagCloudViewModel DocTypeTagCloud { get; set; }
        public TagCloudViewModel OrtTagCloud { get; set; }
        public TagCloudViewModel KategorieTagCloud { get; set; }
        public TagCloudViewModel HandelTagCloud { get; set; }

        public double SummeAusgaben { get; set; }

        public double SummeEinnahmen { get; set; }
    }
}