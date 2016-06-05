using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.ViewModels
{
    public class ChartViewModel
    {
        public List<float> Values { get; set; }
        public List<string> Labels { get; set; }
        public string Type{ get; set; }
        public string Title { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        private string _chartID = Guid.NewGuid().ToString();
        public string ChartID { get { return _chartID; } }
    }
}