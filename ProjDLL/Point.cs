using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ProjZesp {
    public class Point {
        public double X { get; set; }
        public double Y { get; set; }

        public Point() { }

        public Point(double x, double y) {
            X = x;
            Y = y;
        }

        public override string ToString() {
            StringBuilder toString = new StringBuilder("");
            toString.Append("Klasa: " + this.GetType().Name).Append("\n");
            toString.Append("X: " + X).Append("\n");
            toString.Append("Y: " + Y).Append("\n");
            return toString.ToString();
        }
    }

    public class KeyPoint : Point {
        public string Name { get; set; }

        public KeyPoint(double x, double y, string name) : base(x, y) {
            Name = name;
        }

        public override string ToString() {
            StringBuilder toString = new StringBuilder("");
            toString.Append(base.ToString());
            toString.Append("Nazwa: " + Name).Append("\n");
            return toString.ToString();
        }
    }

    public class Site : Point {
        public int SiteNumb { get; set; }

        public Site(double x, double y, int siteNumb) : base(x, y) {
            SiteNumb = siteNumb;
        }
    }

    public class Facility : Point {
        public List<string> FieldValues { get; set; }
        public FacilityType Type { get; set; }

        public Facility() {
            FieldValues = new List<string>();
        }

        public Facility(double x, double y) : base(x, y) { }

        public override string ToString() {
            StringBuilder toString = new StringBuilder("");
            toString.Append("Typ: " + Type.Name).Append("\n");
            toString.Append(base.ToString());
            for(int i = 0; i < FieldValues.Count; i++) {
                var field = Type.FacilityFields.ElementAt(i);
                var fieldKey = field.Key;
                var fieldValue = field.Value;
                toString.Append("" + fieldValue + " " + fieldKey + ": " + FieldValues[i]).Append("\n");
            }
            return toString.ToString();
        }
    }

    public class FacilityType {
        public string Name { get; set; }
        public Dictionary<string, string> FacilityFields { get; set; }
        public List<Facility> Facilities { get; set; }

        public FacilityType() {
            FacilityFields = new Dictionary<string, string>();
            Facilities = new List<Facility>();
        }

        public override string ToString() {
            StringBuilder toString = new StringBuilder("");
            toString.Append("Typ: " + Name).Append("\n");
            toString.Append("Klasa: " + this.GetType().Name).Append("\n");

            for(int i = 0; i < FacilityFields.Count; i++) {
                var field = FacilityFields.ElementAt(i);
                var fieldKey = field.Key;
                var fieldValue = field.Value;
                toString.Append("" + fieldValue + " " + fieldKey).Append("\n");
            }
            toString.Append("\n{\n\n");
            foreach(Facility facility in Facilities) {
                toString.Append(facility.ToString()).Append("\n");
            }
            toString.Append("}\n");
            return toString.ToString();
        }
    }

}
