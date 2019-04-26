using ASD.Graphs;
using System;

namespace ASD
{
    public class ConstrainedFlows : System.MarshalByRefObject
    {
        // testy, dla których ma być generowany obrazek
        // graf w ostatnim teście ma bardzo dużo wierzchołków, więc lepiej go nie wyświetlać
        public static int[] circulationToDisplay = { };
        public static int[] constrainedFlowToDisplay = { };

        /// <summary>
        /// Metoda znajdująca cyrkulację w grafie, z określonymi żądaniami wierzchołków.
        /// Żądania opisane są w tablicy demands. Szukamy funkcji, która dla każdego wierzchołka będzie spełniała warunek:
        /// suma wartości na krawędziach wchodzących - suma wartości na krawędziach wychodzących = demands[v]
        /// </summary>
        /// <param name="G">Graf wejściowy, wagi krawędzi oznaczają przepustowości</param>
        /// <param name="demands">Żądania wierzchołków</param>
        /// <returns>Graf reprezentujący wynikową cyrkulację.
        /// Reprezentacja cyrkulacji jest analogiczna, jak reprezentacja przepływu w innych funkcjach w bibliotece.
        /// Należy zwrócić kopię grafu G, gdzie wagi krawędzi odpowiadają przepływom na tych krawędziach.
        /// Zwróć uwagę na rozróżnienie sytuacji, kiedy mamy zerowy przeływ na krawędzi (czyli istnieje
        /// krawędź z wagą 0) od sytuacji braku krawędzi.
        /// Jeśli żądana cyrkulacja nie istnieje, zwróć null.
        /// </returns>
        /// <remarks>
        /// Nie można modyfikować danych wejściowych!
        /// Złożoność metody powinna być asymptotycznie równa złożoności metody znajdującej największy przeływ (z biblioteki).
        /// </remarks>
        public Graph FindCirculation(Graph G, double[] demands)
        {
            Graph Network = new AdjacencyMatrixGraph(true, G.VerticesCount + 2);
            int n = G.VerticesCount;
            //g.VerticesCount - zrodlo, g.VerticesCount + 1 - ujscie
            //zrodlo z ujemnymi, dodatnie z ujsciem

            double flows = 0;
            for (int i = 0; i < demands.Length; i++)
            {
                flows += demands[i];
            }
            if (flows != 0) return null;

            for (int i = 0; i < demands.Length; i++)
            {
                if (demands[i] < 0)
                {
                    Network.AddEdge(new Edge(n, i, Math.Abs(demands[i])));
                }
                else
                {
                    Network.AddEdge(new Edge(i, n + 1, demands[i]));
                }

                foreach(Edge e in G.OutEdges(i))
                {
                    Network.Add(e);
                }
            }

            (double max_flow, Graph Flow) = MaxFlowGraphExtender.FordFulkersonDinicMaxFlow(Network, n, n + 1, MaxFlowGraphExtender.DFSBlockingFlow);

            Graph Circulation = new AdjacencyMatrixGraph(true, n);
            double[] degrees = new double[n];

            for(int i = 0; i < n; i++)
            {
                foreach(Edge e in Flow.OutEdges(i))
                {
                    if(e.To != n + 1)
                    {
                        Circulation.Add(e);
                        degrees[i] -= e.Weight;
                        degrees[e.To] += e.Weight;
                    }
                }
            }

            for(int i = 0; i < n; i++)
            {
                if (degrees[i] != demands[i])
                {
                    return null;
                }
            }

            return Circulation;
        }

        /// <summary>
        /// Funkcja zwracająca przepływ z ograniczeniami, czyli przepływ, który dla każdej z krawędzi
        /// ma wartość pomiędzy dolnym ograniczeniem a górnym ograniczeniem.
        /// Zwróć uwagę, że interesuje nas *jakikolwiek* przepływ spełniający te ograniczenia.
        /// </summary>
        /// <param name="source">źródło</param>
        /// <param name="sink">ujście</param>
        /// <param name="G">graf wejściowy, wagi krawędzi oznaczają przepustowości (górne ograniczenia)</param>
        /// <param name="lowerBounds">kopia grafu G, wagi krawędzi oznaczają dolne ograniczenia przepływu</param>
        /// <returns>Graf reprezentujący wynikowy przepływ (analogicznie do poprzedniej funkcji i do reprezentacji
        /// przepływu w funkcjach z biblioteki.
        /// Jeśli żądany przepływ nie istnieje, zwróć null.
        /// </returns>
        /// <remarks>
        /// Nie można modyfikować danych wejściowych!
        /// Złożoność metody powinna być asymptotycznie równa złożoności metody znajdującej największy przeływ (z biblioteki).
        /// </remarks>
        /// <hint>Wykorzystaj poprzednią część zadania.
        /// </hint>
        public Graph FindConstrainedFlow(int source, int sink, Graph G, Graph lowerBounds)
        {
            Graph Network = new AdjacencyMatrixGraph(true, G.VerticesCount);
            int n = G.VerticesCount;
            double[] demands = new double[n];
            bool was = false;
            for (int i = 0; i < n; i++)
            {
                demands[i] = 0;
            }

            for (int i = 0; i < n; i++)
            {
                foreach (Edge e in lowerBounds.OutEdges(i))
                {
                    demands[i] += e.Weight;
                    demands[e.To] -= e.Weight;
                    Network.AddEdge(new Edge(i, e.To, G.GetEdgeWeight(i, e.To) - e.Weight));
                    if (i == sink && e.To == source) was = true;
                }
            }
            
            if (!was)
            {
                Network.AddEdge(new Edge(sink, source, double.PositiveInfinity));
            }
            else
            {
                Network.ModifyEdgeWeight(sink, source, double.PositiveInfinity);
            }

            Graph Circulation = FindCirculation(Network, demands);
            if (Circulation == null) return null;

            Circulation.DelEdge(sink, source);

            for (int i = 0; i < n; i++)
            {
                foreach (Edge e in lowerBounds.OutEdges(i))
                {
                    Circulation.ModifyEdgeWeight(e.From, e.To, e.Weight);
                }
            }

            if (was)
            {
                Circulation.AddEdge(new Edge(sink, source, G.GetEdgeWeight(sink, source)));
            }

            return Circulation;
        }

    }
}