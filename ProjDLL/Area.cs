using System.Collections.Generic;

namespace ProjZesp
{
    public class Area
    {
        public List<Facility> Facilities { get; set; }
        public List<Edge> Border { get; set; }

        public Area()
        {
            Facilities = new List<Facility>();
            Border = new List<Edge>();
        }

        public void CalcStatistics() { }
    }
}
