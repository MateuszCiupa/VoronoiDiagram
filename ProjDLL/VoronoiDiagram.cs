using System.Collections.Generic;
using System;

namespace ProjZesp
{
    public class Edge
    {
        public Point A { get; set; }
        public Point B { get; set; }

        public Edge(Point a, Point b)
        {
            A = a;
            B = b;
        }

        public Edge()
        {
            A = null;
            B = null;
        }

        public double XVectorAB()
        {
            return B.X - A.X;
        }

        public double YVectorAB()
        {
            return B.Y - A.Y;
        }

        public double XVectorBA()
        {
            return A.X - B.X;
        }

        public double YVectorBA()
        {
            return A.Y - B.Y;
        }

        public void AddPoint(Point P)
        {
            if(A == null)
            {
                A = P;
            } else if(B == null)
            {
                B = P;
            }
        }

        public bool DoesPointExist(Point P)
        {
            if(A != null && A.X == P.X && A.Y == P.Y || B != null && B.X == P.X && B.Y == P.Y)
            {
                return true;
            }

            return false;
        }
    }

    public class HalfEdge
    {
        public Edge ToBeChanged { get; set; }
        public Edge ToBeChangedWith { get; set; }

        public HalfEdge() { }

        public HalfEdge(Edge toBeChanged)
        {
            ToBeChanged = toBeChanged;
            ToBeChangedWith = new Edge();
        }

        public void Change()
        {
            if(ToBeChanged != null)
            {
                if(ToBeChangedWith.A != null && ToBeChangedWith.B != null)
                {
                    ToBeChanged.A = ToBeChangedWith.A;
                    ToBeChanged.B = ToBeChangedWith.B;
                }
            }
        }

        public void SetToBeChangedWith(Point A, Point B)
        {
            ToBeChangedWith.A = A;
            ToBeChangedWith.B = B;
        }

        public bool IsToBeChangedEqual(Edge edge)
        {
            if(ToBeChanged.DoesPointExist(edge.A) && ToBeChanged.DoesPointExist(edge.B))
            {
                return true;
            }

            return false;
        }
    }

    public class Cell
    {
        public Point Site { get; private set; }
        public List<Edge> Edges { get; private set; }

        public Cell(Point site)
        {
            Site = site;
            Edges = new List<Edge>();
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
        }

        public void RemoveEdge(Edge edge)
        {
            Edges.Remove(edge);
        }
    }

    public class Line
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }

        public Line() { }

        public Line(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Line(Point A, Point B)
        {
            if(A == null || B == null)
            {
                this.A = 0;
                this.B = 0;
                this.C = 0;
            } else
            {
                this.A = A.Y - B.Y;
                this.B = B.X - A.X;
                C = -this.A * A.X - this.B * A.Y;
            }
        }

        public void BuildPerpendicularBisector(Point A, Point B)
        {
            if(A == null || B == null)
            {
                this.A = 0;
                this.B = 0;
                this.C = 0;

                return;
            }

            this.A = A.X - B.X;
            this.B = A.Y - B.Y;
            C = -this.A * (A.X + B.X) / 2 - this.B * (A.Y + B.Y) / 2;
        }

        public Point GetEdgeIntersection(Edge edge)
        {
            if(edge == null)
            {
                return null;
            }

            double a = edge.YVectorBA();
            double b = edge.XVectorAB();
            double c = -a * edge.A.X - b * edge.A.Y;

            double x = (this.C * b - c * this.B) / (a * this.B - this.A * b);
            double y;

            if(this.B == 0)
            {
                y = -(a * x + c) / b;
            } else
            {
                y = -(this.A * x + this.C) / this.B;
            }


            return new Point(x, y);
        }

        public int DoesEdgeIntersect(Point A, Point B)
        {
            // Krawędź nie przecina się z prostą.
            if((this.A * A.X + this.B * A.Y + C) * (this.A * B.X + this.B * B.Y + C) > 0) { return 0; }
            // Krawędź przecina się z prostą (ale nie tak, jak w przypadku trzecim).
            else if((this.A * A.X + this.B * A.Y + C) * (this.A * B.X + this.B * B.Y + C) < 0) { return 1; }
            // Jeden z punktów granicznych krawędzi (A lub B) przecina się z prostą.
            else { return 2; }
        }

        public double GetDistanceFromPoint(double X, double Y)
        {
            return Math.Abs(X * A + Y * B + C) / Math.Sqrt(A * A + B * B);
        }

        public bool AreLinesEqual(Line line)
        {
            if(this.A == line.A && this.B == line.B && this.C == line.C)
            {
                return true;
            }

            return false;
        }
    }

    public class VoronoiDiagram
    {
        public List<Cell> Cells { get; private set; }
        public List<KeyPoint> Sites { get; private set; }
        public List<Edge> Edges { get; private set; }
        public List<Edge> Boundary { get; private set; }


        public VoronoiDiagram(List<KeyPoint> sites, List<Edge> boundary)
        {
            this.Sites = sites;
            this.Boundary = boundary;
            Cells = new List<Cell>();
            Edges = new List<Edge>();
        }

        public void GenerateVoronoi()
        {
            foreach(Point site in Sites)
            {
                AddNewSite(site);
            }
        }

        public void AddNewSite(Point site)
        {
            Cell cell = new Cell(site);
            List<HalfEdge> halfEdges = new List<HalfEdge>();
            List<Edge> toBeRemoved = new List<Edge>();

            foreach(Cell voronoiCell in Cells)
            {
                Line bisector = new Line();
                bisector.BuildPerpendicularBisector(site, voronoiCell.Site);
                Edge newCellEdge = GetEdgeFromBoundary(bisector);

                int edgesIntersected = 0;
                bool wasAnyEdgeRemoved = false;
                bool isThereEdgeBetweenCellPointAndSite = false;
            
                foreach(Edge cellEdge in voronoiCell.Edges)
                {
                    bool isEdgeBetweenBisectorAndPoint = IsEdgeBetweenBisectorAndPoint(cellEdge, site, voronoiCell.Site);

                    if(isEdgeBetweenBisectorAndPoint)
                    {
                        isThereEdgeBetweenCellPointAndSite = true;
                    }

                    int spacialRelationship = bisector.DoesEdgeIntersect(cellEdge.A, cellEdge.B);

                    if(spacialRelationship == 1) // cellEdge intersects bisector
                    {
                        HalfEdge halfEdge = new HalfEdge(cellEdge);
                        Point newPoint = bisector.GetEdgeIntersection(cellEdge);

                        if(bisector.DoesEdgeIntersect(site, cellEdge.A) == 0)
                        {
                            halfEdge.SetToBeChangedWith(newPoint, cellEdge.B);
                        } else
                        {
                            halfEdge.SetToBeChangedWith(cellEdge.A, newPoint);
                        }

                        if(!halfEdges.Contains(halfEdge))
                        {
                            halfEdges.Add(halfEdge);
                        }

                        if(edgesIntersected == 0)
                        {
                            if(GetVectorProduct(newPoint, site, newCellEdge.A) < GetVectorProduct(newPoint, site, newCellEdge.B))
                            {
                                if(isThereEdgeBetweenCellPointAndSite == false)
                                {
                                    newCellEdge.A = newPoint;
                                } else
                                {
                                    newCellEdge.B = newCellEdge.A;
                                    newCellEdge.A = newPoint;
                                }
                            } else
                            {
                                if(isThereEdgeBetweenCellPointAndSite == false)
                                {
                                    newCellEdge.B = newCellEdge.A;
                                    newCellEdge.A = newPoint;
                                } else
                                {
                                    newCellEdge.A = newPoint;
                                }
                            }
                        } else // edgesIntersected == 1
                        {
                            newCellEdge.B = newPoint;
                        }
                        

                        edgesIntersected++;
                    } else if(spacialRelationship == 2) // only cellEdge.A or cellEdge.B intersects bisector
                    {
                        isThereEdgeBetweenCellPointAndSite = true;
                        edgesIntersected = 3;
                    } else // cellEdge doesn't intersect bisector
                    {
                        if(bisector.DoesEdgeIntersect(site, cellEdge.A) == 0)
                        {
                            wasAnyEdgeRemoved = true;

                            if(!toBeRemoved.Contains(cellEdge))
                            {
                                toBeRemoved.Add(cellEdge);
                            }
                        }
                    }
                }

                foreach(Edge edge in toBeRemoved)
                {
                    Edges.Remove(edge);
                    voronoiCell.Edges.Remove(edge);
                }

                if(edgesIntersected == 0)
                {
                    if(wasAnyEdgeRemoved || !isThereEdgeBetweenCellPointAndSite)
                    {
                        Edges.Add(newCellEdge);
                        cell.AddEdge(newCellEdge);
                        voronoiCell.AddEdge(newCellEdge);
                    }
                } else if(edgesIntersected == 1 || edgesIntersected == 2)
                {
                    cell.AddEdge(newCellEdge);
                    Edges.Add(newCellEdge);
                    voronoiCell.AddEdge(newCellEdge);
                }
            }

            foreach(HalfEdge halfEdge in halfEdges)
            {
                halfEdge.Change();
            }

            Cells.Add(cell);
        }

        private bool WasEdgeRemoved(Edge edge)
        {
            if(Edges.Contains(edge))
            {
                return false;
            }

            return true;
        }

        private Edge GetEdgeFromBoundary(Line bisector)
        {
            Edge newEdge = new Edge();

            foreach(Edge boundaryEdge in Boundary)
            {
                if(bisector.DoesEdgeIntersect(boundaryEdge.A, boundaryEdge.B) == 1)
                {
                    newEdge.AddPoint(bisector.GetEdgeIntersection(boundaryEdge));
                } else if(bisector.DoesEdgeIntersect(boundaryEdge.A, boundaryEdge.B) == 2)
                {
                    Point P = bisector.GetEdgeIntersection(boundaryEdge);

                    if(!newEdge.DoesPointExist(P))
                    {
                        newEdge.AddPoint(P);
                    }
                }
            }

            return newEdge;
        }

        private bool IsEdgeBetweenBisectorAndPoint(Edge edge, Point site, Point point)
        {
            Line line = new Line(site, point);

            if(line.DoesEdgeIntersect(edge.A, edge.B) == 0)
            {
                return false;
            }

            Point A = line.GetEdgeIntersection(edge);
            Point middlePoint = new Point((site.X + point.X) / 2, (site.Y + point.Y) / 2);
          
            if(GetEdgeLength(middlePoint, point) > GetEdgeLength(A, point) && GetEdgeLength(middlePoint, point) > GetEdgeLength(middlePoint, A))
            {
                return true;
            }
            
            return false;
        }

        public bool DeleteBoundaryPoint(Point boundaryPoint)
        {
            bool isThereBoundaryPoint = false;
            Point A = null;

            foreach(Edge boundaryEdge in Boundary)
            {
                if(boundaryEdge.DoesPointExist(boundaryPoint))
                {
                    if(A == null)
                    {
                        isThereBoundaryPoint = true;

                        if(boundaryEdge.A == boundaryPoint)
                        {
                            A = boundaryEdge.B;
                        } else
                        {
                            A = boundaryEdge.A;
                        }

                        Boundary.Remove(boundaryEdge);
                    } else
                    {
                        if(boundaryEdge.A == boundaryPoint)
                        {
                            boundaryEdge.A = A;
                        } else
                        {
                            boundaryEdge.B = A;
                        }
                    }
                }
            }

            if(!isThereBoundaryPoint)
            {
                return false;
            }

            Edges.Clear();
            Cells.Clear();

            foreach(Point site in Sites)
            {
                AddNewSite(site);
            }

            return isThereBoundaryPoint;
        }

        public void WriteEdges()
        {
            foreach(Edge edge in Edges)
            {
                Console.WriteLine("Ax: {0}, Ay: {1} || Bx: {2}, By: {3}", edge.A.X, edge.A.Y, edge.B.X, edge.B.Y);
            }

            Console.WriteLine("{0}", Edges.Count);
        }

        public void WriteCells()
        {
            foreach(Cell cell in Cells)
            {
                Console.WriteLine("{0} {1}", cell.Site.X, cell.Site.Y);

                foreach(Edge edge in cell.Edges)
                {
                    Console.WriteLine("Ax: {0}, Ay: {1} || Bx: {2}, By: {3}", edge.A.X, edge.A.Y, edge.B.X, edge.B.Y);
                }

                Console.WriteLine();
            }
        }

        private double GetVectorProduct(Point A, Point B, Point C)
        { 
            return -1 * (GetEdgeLength(B, C) * GetEdgeLength(B, C) - GetEdgeLength(A, C) * GetEdgeLength(A, C) - GetEdgeLength(B, A) * GetEdgeLength(B, A)) / (2 * GetEdgeLength(B, A) * GetEdgeLength(A, C));
        }

        private double GetEdgeLength(Point A, Point B)
        {
            return Math.Sqrt((A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y));
        }
    }
}
