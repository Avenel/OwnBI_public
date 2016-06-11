using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OwnBI.ViewModels
{
    public class ChartViewModel
    {
        public List<List<float>> Values { get; set; }
        public List<string> Categories { get; set; }
        public List<string> SeriesNames{ get; set; }
        public string Type{ get; set; }
        public string Title { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        private string _chartID = Guid.NewGuid().ToString();
        public string ChartID { get { return _chartID; } }

        public bool HidePanel { get; set; }

        public List<string> BackGroundColors
        {
            get
            {
                return new List<string>()
                {
                    "rgba(255, 99, 132, 0.2)",
                    "rgba(54, 162, 235, 0.2)",
                    "rgba(255, 206, 86, 0.2)",
                    "rgba(75, 192, 192, 0.2)",
                    "rgba(153, 102, 255, 0.2)",
                    "rgba(255, 159, 64, 0.2)"
                };
            }
        }

        public List<string> BorderColors
        {
            get
            {
                return new List<string>()
                {
                    "rgba(255, 99, 132, 1.0)",
                    "rgba(54, 162, 235, 1.0)",
                    "rgba(255, 206, 86, 1.0)",
                    "rgba(75, 192, 192, 1.0)",
                    "rgba(153, 102, 255, 1.0)",
                    "rgba(255, 159, 64, 1.0)"
                };
            }
        }
    }
}