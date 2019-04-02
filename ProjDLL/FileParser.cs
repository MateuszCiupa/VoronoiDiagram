using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjZesp
{
    public class FileParser
    {
        public List<KeyPoint> KeyPoints { get; set; }
        public List<FacilityType> FacilityTypes { get; set; }
        public List<Point> OutlinePoints { get; set; }
        public List<Edge> OutlineLines { get; set; }
        public string ConsoleLine { get; set; }

        public FileParser()
        {
            KeyPoints = new List<KeyPoint>();
            FacilityTypes = new List<FacilityType>();
            OutlinePoints = new List<Point>();
            OutlineLines = new List<Edge>();
        }

        public int ReadFile(string fileName)
        {
            {
                ClearParserData();
            }
            try
            {
                StreamReader reader = File.OpenText(fileName);
                string line;
                int lineCounter = 0;
                int headerCounter = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineCounter++;
                    string[] elements = FixQuotedElements(line.Split(' '));
                    if (elements.Length == 1 && elements[0] == "")
                    {
                        ;
                    }
                    else if (elements.Length < 3 || (elements.Length == 3 && elements[2] == ""))
                    {
                        ConsoleLine = "Dla punktu zareklarowanego w linii " + lineCounter + " pliku zabrakło danych o koordynatach.";
                        return -1;
                    }
                    else if (elements[0] == "#")
                    {
                        headerCounter++;
                    }
                    else
                    {
                        if (headerCounter == 1) //kontury terenu
                        {
                            Point readOutlinePoint = new Point(double.Parse(elements[1].Replace(".", ",")), double.Parse(elements[2].Replace(".", ",")));
                            OutlinePoints.Add(readOutlinePoint);
                        }

                        else if (headerCounter == 2) // punkty kluczowe
                        {
                            KeyPoint readKeyPoint = new KeyPoint(double.Parse(elements[1].Replace(".", ",")), double.Parse(elements[2].Replace(".", ",")), elements[3]);
                            KeyPoints.Add(readKeyPoint);
                        }

                        else if (headerCounter == 3) // definicje obiektów
                        {
                            FacilityType readFacilityType = new FacilityType
                            {
                                Name = elements[1]
                            };

                            int coordinateCount = 0;

                            int elementsIterator = 2;
                            while (elements.Length >= elementsIterator + 2)
                            {
                                string fieldName = elements[elementsIterator];
                                if (fieldName == "X" || fieldName == "x" || fieldName == "Y" || fieldName == "y")
                                {
                                    coordinateCount++;
                                }
                                try
                                {
                                    readFacilityType.FacilityFields.Add(elements[elementsIterator], elements[elementsIterator + 1]);
                                }
                                catch (System.ArgumentException)
                                {
                                    ConsoleLine = "Powtórzenie deklaracji koordynatów dla typu " + elements[1] + " w " + lineCounter + " linii pliku.";
                                    return -1;
                                }
                                elementsIterator += 2;
                            }
                            if (coordinateCount < 2)
                            {
                                ConsoleLine = "Niewystarczająca liczba deklaracji koordynatów dla typu " + elements[1] + " w " + lineCounter + " linii pliku.";
                                return -1;
                            }
                            else
                            {
                                FacilityTypes.Add(readFacilityType);
                            }
                        }

                        else// obiekty
                        {
                            Facility readFacility = new Facility();

                            string readFacilityName = elements[1];
                            FacilityType tmpType = GetType(readFacilityName);
                            readFacility.Type = tmpType;

                            int elementsIterator = 2;
                            while (elements.Length >= elementsIterator + 1)
                            {
                                if (elements[elementsIterator] != null)
                                {
                                    readFacility.FieldValues.Add(elements[elementsIterator]);
                                }
                                elementsIterator++;
                            }
                            tmpType.Facilities.Add(readFacility);
                        }
                    }
                }
            }
            catch (Exception)
            {
                ConsoleLine = "Nie udało się otworzyć pliku.";
                return 1;
            }
            if (!IsOutlineCorrect() || !AreKeyPointsCorrect() || !MoveAndCheckCoordinates())
            {
                return 1;
            }
            ConsoleLine = "Poprawnie wczytano plik wejściowy.";
            return 0;
        }

        private void ClearParserData()
        {
            OutlinePoints.Clear();
            OutlineLines.Clear();
            KeyPoints.Clear();
            FacilityTypes.Clear();
        }

        private bool MoveAndCheckCoordinates()
        {
            foreach (FacilityType type in FacilityTypes)
            {
                int xIndex = 0, yIndex = 0;
                int i = 0;
                while (i < type.FacilityFields.Count)
                {
                    var field = type.FacilityFields.ElementAt(i);
                    if (field.Key == "X" || field.Key == "x")
                    {
                        xIndex = i;
                    }
                    if (field.Key == "Y" || field.Key == "y")
                    {
                        yIndex = i;
                    }
                    i++;
                }
                if (xIndex > yIndex)
                {
                    type.FacilityFields.Remove(type.FacilityFields.ElementAt(xIndex).Key);
                    type.FacilityFields.Remove(type.FacilityFields.ElementAt(yIndex).Key);
                }
                else
                {
                    type.FacilityFields.Remove(type.FacilityFields.ElementAt(yIndex).Key);
                    type.FacilityFields.Remove(type.FacilityFields.ElementAt(xIndex).Key);
                }
                foreach (Facility facility in type.Facilities)
                {
                    facility.X = double.Parse(facility.FieldValues[xIndex]);
                    facility.Y = double.Parse(facility.FieldValues[yIndex]);
                    if (xIndex > yIndex)
                    {
                        facility.FieldValues.RemoveAt(xIndex);
                        facility.FieldValues.RemoveAt(yIndex);
                    }
                    else
                    {
                        facility.FieldValues.RemoveAt(yIndex);
                        facility.FieldValues.RemoveAt(xIndex);
                    }
                    if (!PointUtils.IsPointInPolygon(new Point(facility.X, facility.Y), OutlinePoints))
                    {
                        ConsoleLine = "Obiekt typu " + facility.Type.Name + " ( " + facility.X + " , " + facility.Y + " ) leży poza granicą obszaru.";
                        return false;
                    }
                }
            }
            return true;
        }

        public string PrintLists()
        {
            StringBuilder printedLists = new StringBuilder("");

            foreach (Point outlinePoint in OutlinePoints)
            {
                printedLists.Append(outlinePoint.ToString()).Append("\n");
            }
            printedLists.Append("----------\n");
            foreach (KeyPoint keyPoint in KeyPoints)
            {
                printedLists.Append(keyPoint.ToString()).Append("\n");
            }
            printedLists.Append("----------\n");
            foreach (FacilityType facilityType in FacilityTypes)
            {
                printedLists.Append(facilityType.ToString()).Append("\n");
            }
            return printedLists.ToString();
        }

        private string[] FixQuotedElements(string[] elements)
        {
            string[] validatedElements = new string[elements.Length];
            int validatedElementsIndex = 0;
            bool isBusy = false;
            int i = 0;
            StringBuilder element = new StringBuilder("");
            while (i < elements.Length)
            {
                if (!isBusy) // początek składania
                {
                    if (elements[i].Contains("\""))
                    {
                        isBusy = true;
                        element.Append(elements[i]).Append(" ");
                        element.Remove(0, 1);
                        i++;
                    }
                    else
                    {
                        validatedElements[validatedElementsIndex] = elements[i];
                        validatedElementsIndex++;
                        i++;
                    }
                }
                else
                {
                    if (elements[i].Contains("\""))
                    {
                        isBusy = false;
                        element.Append(elements[i]);
                        element.Remove(element.Length - 1, 1);
                        validatedElements[validatedElementsIndex] = element.ToString();
                        element.Clear();
                        validatedElementsIndex++;
                    }
                    else
                    {
                        element.Append(elements[i]).Append(" ");
                    }
                    i++;
                }

            }
            return validatedElements;
        }

        private FacilityType GetType(string name)
        {
            for (int i = 0; i < FacilityTypes.Count; i++)
            {
                if (FacilityTypes[i].Name == name)
                {
                    return FacilityTypes[i];
                }
            }
            return null;
        }

        public void MakeOutlineLines()
        {
            if (OutlineLines.Count != 0)
            {
                OutlineLines.Clear();
            }
            for (int i = 0; i < OutlinePoints.Count - 1; i++)
            {
                Edge outlineLine = new Edge(OutlinePoints[i], OutlinePoints[i + 1]);
                OutlineLines.Add(outlineLine);
                if (i == OutlinePoints.Count - 2)
                {
                    outlineLine = new Edge(OutlinePoints[i + 1], OutlinePoints[0]);
                    OutlineLines.Add(outlineLine);
                }
            }
        }

        public bool IsOutlineCorrect()
        {
            bool isOutlineCorrect = true;
            if (OutlinePoints.Count < 4)
            {
                ConsoleLine = "Obszar musi mieć co najmniej 3 punkty konturu.";
                isOutlineCorrect = false;
            }
            else
            {
                for (int i = 0; i < (OutlineLines.Count / 2); i++)
                {
                    for (int j = i + 2; j < (i == 0 ? OutlineLines.Count - 1 : OutlineLines.Count); j++)
                    {
                        if (PointUtils.FindIntersectionPoint(OutlineLines[i], OutlineLines[j], true) != null)
                        {
                            ConsoleLine = "Granice przecinają się.";
                            isOutlineCorrect = false;
                        }
                    }
                }
            }
            return isOutlineCorrect;
        }

        public bool AreKeyPointsCorrect()
        {
            foreach (Point keyPoint in KeyPoints)
            {
                if (!PointUtils.IsPointInPolygon(keyPoint, OutlinePoints))
                {
                    ConsoleLine = "Punkt kluczowy ( " + keyPoint.X + " , " + keyPoint.Y + " ) leży poza granicą obszaru.";
                    return false;
                }
            }
            return true;
        }
    }

    public static class PointUtils
    {
        public static bool IsPointOnSection(Point point, Edge section)
        {
            double ax = section.A.X, ay = section.A.Y;
            double bx = section.B.X, by = section.B.Y;
            double px = point.X, py = point.Y;
            double det = ax * by + bx * py + px * ay - px * by - ax * py - bx * ay;
            if (Math.Round(det) != 0)
            {
                return false;
            }
            else
            {
                if ((Math.Min(ax, bx) <= px) && (px <= Math.Max(ax, bx)) &&
                (Math.Min(ay, by) <= py) && (py <= Math.Max(ay, by)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static Line FindLineEquasion(Edge line)
        {
            Line lineEquasion = new Line();
            lineEquasion.A = line.B.Y - line.A.Y;
            lineEquasion.B = line.A.X - line.B.X;
            lineEquasion.C = lineEquasion.A * (line.A.X) + lineEquasion.B * (line.A.Y);
            return lineEquasion;
        }

        public static Point FindIntersectionPoint(Edge line1, Edge line2, bool isSection)
        {
            Line line1Equasion = FindLineEquasion(line1);
            Line line2Equasion = FindLineEquasion(line2);
            double delta = (line1Equasion.A * line2Equasion.B) - (line2Equasion.A * line1Equasion.B);
            if (delta == 0) //linie rownolegle
            {
                return null;
            }
            else
            {
                Point intersectionPoint = new Point();
                intersectionPoint.X = ((line2Equasion.B * line1Equasion.C) - (line1Equasion.B * line2Equasion.C)) / delta;
                intersectionPoint.Y = ((line1Equasion.A * line2Equasion.C) - (line2Equasion.A * line1Equasion.C)) / delta;
                if (!isSection)
                {
                    return intersectionPoint;
                }
                else
                {
                    if (IsPointOnSection(intersectionPoint, line1))
                    {
                        return intersectionPoint;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public static bool IsPointInPolygon(Point point, List<Point> polygon)
    {
        bool result = false;
        int j = polygon.Count() - 1;
        for (int i = 0; i < polygon.Count(); i++)
        {
            if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
            {
                if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                {
                    result = !result;
                }
            }
            j = i;
        }
        return result;
    }
    }
}


