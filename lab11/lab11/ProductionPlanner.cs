using System;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class ProductionPlanner : MarshalByRefObject
    {
        /// <summary>
        /// Flaga pozwalająca na włączenie wypisywania szczegółów skonstruowanego planu na konsolę.
        /// Wartość <code>true</code> spoeoduje wypisanie planu.
        /// </summary>
        public bool ShowDebug { get; } = false;
        
        /// <summary>
        /// Część 1. zadania - zaplanowanie produkcji telewizorów dla pojedynczego kontrahenta.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających maksymalną produkcję i zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się maksymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateSimplePlan(PlanData[] production, PlanData[] sales, PlanData storageInfo, 
            out SimpleWeeklyPlan[] weeklyPlan)
        {
            bool boolean = true;
            int n = production.Length;
            int can = storageInfo.Quantity;
            double price = storageInfo.Value;
            if (n != sales.Length) throw new ArgumentException();
            if (n <= 0 || can < 0 || price < 0) boolean = false;
            for (int i = 0; i < n; i++)
            {
                if (production[i].Quantity < 0 || production[i].Value < 0 || sales[i].Quantity < 0 || sales[i].Value < 0)
                {
                    boolean = false;
                    break;
                }
            }
            if (!boolean) throw new ArgumentException();
            weeklyPlan = new SimpleWeeklyPlan[n];
            Graph Network = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n + 2);
            Graph NetworkCost = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n + 2);
            for (int i = 0; i < n; i++)
            {
                Network.AddEdge(new Edge(n, i, production[i].Quantity));
                NetworkCost.AddEdge(new Edge(n, i, production[i].Value));
                Network.AddEdge(new Edge(i, n + 1, sales[i].Quantity));
                NetworkCost.AddEdge(new Edge(i, n + 1, -sales[i].Value));
                if (i < n - 1)
                {
                    Network.AddEdge(new Edge(i, i + 1, can));
                    NetworkCost.AddEdge(new Edge(i, i + 1, price));
                }
            }
            (double value, double cost, Graph flow) = MinCostFlowGraphExtender.MinCostFlow(Network, NetworkCost, n, n + 1, false, MaxFlowGraphExtender.PushRelabelMaxFlow, null, false);
            for (int i = 0; i < n; i++)
            {
                weeklyPlan[i].UnitsProduced = (int)flow.GetEdgeWeight(n, i);
                weeklyPlan[i].UnitsSold = (int)flow.GetEdgeWeight(i, n + 1);
                weeklyPlan[i].UnitsStored = (int)flow.GetEdgeWeight(i, i + 1);
            }
            weeklyPlan[n - 1].UnitsStored = 0;
            return new PlanData {Quantity = (int)value, Value = -cost};
        }

        /// <summary>
        /// Część 2. zadania - zaplanowanie produkcji telewizorów dla wielu kontrahentów.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających produkcję dającą maksymalny zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Dwuwymiarowa tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Pierwszy wymiar tablicy jest równy liczbie kontrahentów, zaś drugi - liczbie tygodni w planie.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// Każdy wiersz tablicy odpowiada jednemu kontrachentowi.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się optymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateComplexPlan(PlanData[] production, PlanData[,] sales, PlanData storageInfo,
            out WeeklyPlan[] weeklyPlan)
        {
            bool boolean = true;
            int n = production.Length;
            int m = sales.GetLength(0);
            if (n != sales.GetLength(1)) throw new ArgumentException();
            int[] sum = new int[m];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    sum[i] += sales[i, j].Quantity;
                    if (sales[i, j].Quantity < 0 || sales[i, j].Value < 0)
                    {
                        boolean = false;
                    }
                }
            }
            int can = storageInfo.Quantity;
            double price = storageInfo.Value;
            if (m <= 0 || n <= 0 || can < 0 || price < 0) boolean = false;
            for (int i = 0; i < n; i++)
            {
                if (production[i].Quantity < 0 || production[i].Value < 0)
                {
                    boolean = false;
                    break;
                }
            }
            if (!boolean) throw new ArgumentException();
            Graph Network = new AdjacencyListsGraph<SimpleAdjacencyList>(true, 1 + n + n + m + 1);
            Graph NetworkCost = new AdjacencyListsGraph<SimpleAdjacencyList>(true, 1 + n + n + m + 1);
            for (int i = 0; i < n; i++)
            {
                Network.AddEdge(new Edge(0, i + 1, production[i].Quantity));
                NetworkCost.AddEdge(new Edge(0, i + 1, 0));
                Network.AddEdge(new Edge(i + 1, n + i + 1, production[i].Quantity));
                NetworkCost.AddEdge(new Edge(i + 1, n + i + 1, production[i].Value));
                Network.AddEdge(new Edge(i + 1, n + n + m + 1, production[i].Quantity));
                NetworkCost.AddEdge(new Edge(i + 1, n + n + m + 1, 0));
                if (i < n - 1)
                {
                    Network.AddEdge(new Edge(n + i + 1, n + i + 2, can));
                    NetworkCost.AddEdge(new Edge(n + i + 1, n + i + 2, price));
                }
                for (int j = 0; j < m; j++)
                {
                    Network.AddEdge(new Edge(n + i + 1, n + n + 1 + j, sales[j, i].Quantity));
                    NetworkCost.AddEdge(new Edge(n + i + 1, n + n + 1 + j, -sales[j, i].Value));
                }
            }
            for (int i = 0; i < m; i++)
            {
                Network.AddEdge(new Edge(n + n + 1 + i, n + n + m + 1, sum[i]));
                NetworkCost.AddEdge(new Edge(n + n + 1 + i, n + n + m + 1, 0));
            }
            (double value, double cost, Graph flow) = MinCostFlowGraphExtender.MinCostFlow(Network, NetworkCost, 0, n + n + m + 1);
            for (int i = 0; i < n; i++)
            {
                value -= flow.GetEdgeWeight(i + 1, n + n + m + 1);
            }
            weeklyPlan = new WeeklyPlan[n];
            for (int i = 0; i < n; i++)
            {
                weeklyPlan[i].UnitsProduced = (int)flow.GetEdgeWeight(i + 1, n + i + 1);
                weeklyPlan[i].UnitsSold = new int[m];
                for (int j = 0; j < m; j++)
                {
                    weeklyPlan[i].UnitsSold[j] = (int)flow.GetEdgeWeight(n + i + 1, n + n + 1 + j);
                }
                weeklyPlan[i].UnitsStored = (int)flow.GetEdgeWeight(n + i + 1, n + i + 2);
            }
            weeklyPlan[n - 1].UnitsStored = 0;
            return new PlanData {Quantity = (int)value, Value = -cost};
        }

    }
}