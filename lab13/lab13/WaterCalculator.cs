using System;
using System.Collections.Generic;

namespace ASD
{
    public class WaterCalculator : MarshalByRefObject
    {

        /*
         * Metoda sprawdza, czy przechodząc p1->p2->p3 skręcamy w lewo 
         * (jeżeli idziemy prosto, zwracany jest fałsz).
         */
        private bool leftTurn(Point p1, Point p2, Point p3)
        {
            Point w1 = new Point(p2.x - p1.x, p2.y - p1.y);
            Point w2 = new Point(p3.x - p2.x, p3.y - p2.y);
            double vectProduct = w1.x * w2.y - w2.x * w1.y;
            return vectProduct > 0;
        }


        /*
         * Metoda wyznacza punkt na odcinku p1-p2 o zadanej współrzędnej y.
         * Jeżeli taki punkt nie istnieje (bo cały odcinek jest wyżej lub niżej), zgłaszany jest wyjątek ArgumentException.
         */
        private Point getPointAtY(Point p1, Point p2, double y)
        {
            if (p1.y != p2.y)
            {
                double newX = p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y);
                if ((newX - p1.x) * (newX - p2.x) > 0)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point(p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y), y);
            }
            else
            {
                if (p1.y != y)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point((p1.x + p2.x) / 2, y);
            }
        }


        /// <summary>
        /// Funkcja zwraca tablice t taką, że t[i] jest głębokością, na jakiej znajduje się punkt points[i].
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double[] PointDepths(Point[] points)
        {
            int n = points.Length;
            double[] ans = new double[n];
            double[] left = new double[n];
            double[] right = new double[n];
            for (int i = 0; i < n; i++)
            {
                left[i] = points[i].y;
                right[i] = points[i].y;
            }
            for (int i = 1; i < n; i++)
            {
                if (points[i].x < points[i - 1].x) continue;
                left[i] = Math.Max(left[i], left[i - 1]);
            }
            for (int i = n - 2; i >= 0; i--)
            {
                if (points[i].x > points[i + 1].x) continue;
                right[i] = Math.Max(right[i], right[i + 1]);
            }
            for (int i = 0; i < n; i++)
            {
                ans[i] = Math.Min(left[i], right[i]) - points[i].y;
            }
            return ans;
        }

        /// <summary>
        /// Funkcja zwraca objętość wody, jaka zatrzyma się w górach.
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double WaterVolume(Point[] points)
        {
            double sum = 0;
            int n = points.Length;
            double[] depth = PointDepths(points);
            for (int i = 0; i < n - 1; i++)
            {
                double mid = (depth[i] + depth[i + 1]) / 2;
                double leg = points[i + 1].x - points[i].x;
                if (depth[i] + points[i].y < points[i + 1].y)
                {
                    double height = points[i + 1].y - depth[i] - points[i].y;
                    leg = leg * depth[i] / (depth[i] + height);
                }
                if (depth[i + 1] + points[i + 1].y < points[i].y)
                {
                    double height = points[i].y - depth[i + 1] - points[i + 1].y;
                    leg = leg * depth[i + 1] / (depth[i + 1] + height);
                }
                sum += mid * leg;
            }
            return sum;
        }
    }

    [Serializable]
    public struct Point
    {
        public double x, y;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
