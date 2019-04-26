using System;
using System.Collections.Generic;
using ASD.Graphs;

namespace asd2
{
    public class City : MarshalByRefObject
    {
        /// <summary>
        /// Sprawdza przecięcie zadanych ulic-odcinków. Zwraca liczbę punktów wspólnych.
        /// </summary>
        /// <returns>0 - odcinki rozłączne, 
        /// 1 - dokładnie jeden punkt wspólny, 
        /// int.MaxValue - odcinki częściowo pokrywają się (więcej niż 1 punkt wspólny)</returns>
        public int CheckIntersection(Street s1, Street s2)
        {
            Point P1 = s1.p1, Q1 = s1.p2, P2 = s2.p1, Q2 = s2.p2;
            int o1 = Orientation(P1, Q1, P2);
            int o2 = Orientation(P1, Q1, Q2);
            int o3 = Orientation(P2, Q2, P1);
            int o4 = Orientation(P2, Q2, Q1);

            if (o1 != o2 && o3 != o4)
                return 1;

            if (o1 == 0 && Contains(P1, P2, Q1))
            {
                if (P1 == P2 && !Contains(P1, Q2, Q1))
                    return 1;

                if (Q1 == P2 && !Contains(P1, Q2, Q1))
                    return 1;

                return int.MaxValue;
            }

            if (o2 == 0 && Contains(P1, Q2, Q1))
            {
                if (P1 == Q2 && !Contains(P1, P2, Q1))
                    return 1;

                if (Q1 == Q2 && !Contains(P1, P2, Q1))
                    return 1;

                return int.MaxValue;
            }

            if (o3 == 0 && Contains(P2, P1, Q2))
            {
                if (P2 == P1 && !Contains(Q2, Q1, P2))
                    return 1;

                if (Q2 == P1 && !Contains(Q2, Q1, P2))
                    return 1;

                return int.MaxValue;
            }

            if (o4 == 0 && Contains(P2, Q1, Q2))
            {
                if (P2 == Q1 && !Contains(Q2, P1, P2))
                    return 1;

                if (Q2 == Q1 && !Contains(Q2, P1, P2))
                    return 1;

                return int.MaxValue;
            }

            return 0;
        }

        public bool Contains(Point p, Point q, Point r)
        {
            if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
                q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
                return true;

            return false;
        }

        public int Orientation(Point p, Point q, Point r)
        {
            double val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
            if (val == 0) return 0; 
            return (val > 0) ? 1 : 2; 
        }

        /// <summary>
        /// Sprawdza czy dla podanych par ulic możliwy jest przejazd między nimi (z użyciem być może innych ulic). 
        /// </summary>
        /// <returns>Lista, w której na i-tym miejscu jest informacja czy przejazd między ulicami w i-tej parze z wejścia jest możliwy</returns>
        public bool[] CheckStreetsPairs(Street[] streets, int[] streetsToCheck1, int[] streetsToCheck2)
        {
            if (streetsToCheck1.Length == 0) throw new ArgumentException();
            int n = streets.Length;
            int[] par = new int[n];
            for (int i = 0; i < n; i++)
            {
                if (par[i] == 0) par[i] = i + 1;
                for (int j = 0; j < n; j++)
                {
                    if (CheckIntersection(streets[i], streets[j]) > 0)
                    {
                        if (par[j] != 0) par[i] = Math.Min(par[i], par[j]);
                        par[j] = par[i];
                    }
                }
            }
            bool[] ans = new bool[streetsToCheck1.Length];
            for (int i = 0; i < streetsToCheck1.Length; i++)
            {
                if (par[streetsToCheck1[i]] == par[streetsToCheck2[i]]) ans[i] = true;
                else ans[i] = false;
            }
            return ans;
        }


        /// <summary>
        /// Zwraca punkt przecięcia odcinków s1 i s2.
        /// W przypadku gdy nie ma jednoznacznego takiego punktu rzuć wyjątek ArgumentException
        /// </summary>
        public Point GetIntersectionPoint(Street s1, Street s2)
        {
            //znajdź współczynniki a i b prostych y=ax+b zawierających odcinki s1 i s2
            //uwaga na proste równoległe do osi y
            //uwaga na odcinki równoległe o wspólnych końcu
            //porównaj równania prostych, aby znaleźć ich punkt wspólny
            if (CheckIntersection(s1, s2) != 1) throw new ArgumentException();
            double x1 = s1.p1.x, y1 = s1.p1.y;
            double x2 = s1.p2.x, y2 = s1.p2.y;
            double X1 = s2.p1.x, Y1 = s2.p1.y;
            double X2 = s2.p2.x, Y2 = s2.p2.y;
            if (((X2 - X1) * (y2 - y1) - (x2 - x1) * (Y2 - Y1)) == 0)
            {
                if (s1.p1 == s2.p1 || s1.p1 == s2.p2) return s1.p1;
                if (s1.p2 == s2.p1 || s1.p2 == s2.p2) return s1.p2;
            }
            if (x2 == x1 && Y1 == Y2) return new Point(x1, Y1);
            if (y2 == y1 && X1 == X2) return new Point(X1, y1);
            if (X2 == X1 && y1 == y2) return new Point(X1, y1);
            if (Y2 == Y1 && x1 == x2) return new Point(x1, Y1);
            double X = ((x2 - x1) * (Y1 * X2 - X1 * Y2) - (X2 - X1) * (y1 * x2 - x1 * y2)) / ((X2 - X1) * (y2 - y1) - (x2 - x1) * (Y2 - Y1));
            double Y = ((y2 - y1) * X + y1 * x2 - x1 * y2) / (x2 - x1);
            return new Point(X, Y);
        }


        /// <summary>
        /// Sprawdza możliwość przejazdu między dzielnicami-wielokątami district1 i district2,
        /// tzn. istnieją para ulic, pomiędzy którymi jest przejazd 
        /// oraz fragment jednej ulicy należy do obszaru jednej z dzielnic i fragment drugiej należy do obszaru drugiej dzielnicy
        /// </summary>
        /// <returns>Informacja czy istnieje przejazd między dzielnicami</returns>
        public bool CheckDistricts(Street[] streets, Point[] district1, Point[] district2, out List<int> path, out List<Point> intersections)
        {
            int n = district1.Length, m = district2.Length;
            Street[] street1 = new Street[n];
            Street[] street2 = new Street[m];
            for (int i = 0; i < n; i++)
            {
                street1[i] = new Street(district1[i], district1[(i + 1) % n]);
            }
            for (int i = 0; i < m; i++)
            {
                street2[i] = new Street(district2[i], district2[(i + 1) % m]);
            }
            int size = streets.Length;
            List<int> left = new List<int>();
            List<int> right = new List<int>();
            for (int i = 0; i < size; i++)
            {
                bool boolean = false;
                for (int j = 0; j < n; j++)
                {
                    if (CheckIntersection(streets[i], street1[j]) > 0)
                    {
                        boolean = true;
                        break;
                    }
                }
                if (boolean)
                {
                    if (!left.Contains(i)) left.Add(i);
                }
                boolean = false;
                for (int j = 0; j < m; j++)
                {
                    if (CheckIntersection(streets[i], street2[j]) > 0)
                    {
                        boolean = true;
                        break;
                    }
                }
                if (boolean)
                {
                    if (!right.Contains(i)) right.Add(i);
                }
            }
            path = new List<int>();
            intersections = new List<Point>();
            if (right.Count == 0 || left.Count == 0) return false;
            Graph g = new AdjacencyListsGraph<SimpleAdjacencyList>(true, size);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i != j)
                    {
                        if (CheckIntersection(streets[i], streets[j]) > 0)
                        {
                            g.AddEdge(i, j);
                            g.AddEdge(j, i);
                        }
                    }
                }
            }
            PathsInfo[] info = new PathsInfo[size];
            int min = int.MaxValue;
            int idx = -1, idy = -1;
            for (int i = 0; i < left.Count; i++)
            {
                ShortestPathsGraphExtender.DijkstraShortestPaths(g, left[i], out info);
                for (int j = 0; j < right.Count; j++)
                {
                    int value = (int)info[right[j]].Dist;
                    if (value >= 0)
                    {
                        if (min > value)
                        {
                            min = value;
                            idx = left[i];
                            idy = right[j]; 
                        }
                    }
                }
            }
            if (idx == -1 && idy == -1) return false;
            ShortestPathsGraphExtender.DijkstraShortestPaths(g, idx, out info);
            Edge[] edge = PathsInfo.ConstructPath(idx, idy, info);
            path.Add(idx);
            for (int i = 0; i < edge.Length; i++)
            {
                path.Add(edge[i].To);
            }
            for (int i = 0; i < path.Count - 1; i++)
            {
                intersections.Add(GetIntersectionPoint(streets[path[i]], streets[path[i + 1]]));
            }
            return true;
        }
    }

    [Serializable]
    public struct Point
    {
        public double x;
        public double y;

        public Point(double px, double py) { x = px; y = py; }

        public static Point operator +(Point p1, Point p2) { return new Point(p1.x + p2.x, p1.y + p2.y); }

        public static Point operator -(Point p1, Point p2) { return new Point(p1.x - p2.x, p1.y - p2.y); }

        public static bool operator ==(Point p1, Point p2) { return p1.x == p2.x && p1.y == p2.y; }

        public static bool operator !=(Point p1, Point p2) { return !(p1 == p2); }

        public override bool Equals(object obj) { return base.Equals(obj); }

        public override int GetHashCode() { return base.GetHashCode(); }

        public static double CrossProduct(Point p1, Point p2) { return p1.x * p2.y - p2.x * p1.y; }

        public override string ToString() { return String.Format("({0},{1})", x, y); }
    }

    [Serializable]
    public struct Street
    {
        public Point p1;
        public Point p2;

        public Street(Point p1, Point p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}