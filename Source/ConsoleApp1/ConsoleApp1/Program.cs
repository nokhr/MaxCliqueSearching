using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.Classes;

namespace ConsoleApp1
{
    class AllRating
    {
       public int candidate { get; set; }
       public int child { get; set; }
       public int Rating { get; set; }
    }



    class Program
    {
        static Random random = null;
        public static List<int> globalClick = new List<int>();
        static int Bound;
        static void Main(string[] args)
        {
            try
            {
                string graphFile = @"E:\Work\c125.9.clq";
                Console.WriteLine("Loading graph into memory");
                Console.WriteLine("Graph loaded and validated\n");
                MyGraph graph = new MyGraph(graphFile, "DIMACS");
                int maxTime = 1000;
                int targetCliqueSize = graph.NumberNodes;

                List<int> maxClique = FindMaxClique(graph, maxTime,
                  targetCliqueSize);
                Console.WriteLine("\nMaximum time reached");
                Console.WriteLine("\nSize of best clique found = " +
                  maxClique.Count);
                int ub = maxClique.Count;
                Bound = ub;
                //Список вершин, у которых больше соседей, чем найденный размер оценки на макс. клику
                //Этот список избыточен, сократим его - будем добавлять номер минимального соседа для кажой вершины (сама вершина будет соседней сама себе)

                List<int> candidates = new List<int>();
                candidates = maxClique;
                //for (int i = 0; i < graph.NumberNodes; i++)
                //{
                //    int neighbors = graph.NumberNeighbors(i);
                //    ++neighbors;
                //    Console.WriteLine("neighbors");
                //    Console.WriteLine(neighbors);
                //    //if (neighbors >= ub)
                //    //{
                //        //int min_n = graph.GetNeighbors(i).Min();
                //        //candidates.Add(i>min_n ? min_n:i
                //        candidates.Add(i);
                //    //}
                //}
                    //    Console.WriteLine("candidates");
                    //    Console.WriteLine(candidates.Count);
                    //}
                    //Console.WriteLine("candidates.Intersect(maxClique).ToList<int>().Count");
                    //Console.WriteLine(candidates.Intersect(maxClique).ToList<int>().Count);
                    //candidates = (from c in candidates select c).Distinct().ToList<int>();

                    // List<AllRating> totalRating = new List<AllRating>();

                    // candidates.ForEach(c =>
                    //{
                    //    List<int> candidatesChilds = new List<int>();
                    //    candidatesChilds = graph.GetNeighbors(c);
                    //    //Console.WriteLine("candidatesChilds");
                    //    //Console.WriteLine(candidatesChilds.Count());

                    //    candidatesChilds.ForEach(z =>
                    //    {
                    //        int Rating = 0;
                    //        List<int> zChilds = new List<int>();
                    //        zChilds = graph.GetNeighbors(z);

                    //        //Console.WriteLine("zChilds");
                    //        //Console.WriteLine(zChilds.Count());

                    //        zChilds.ForEach(x =>
                    //        {
                    //             if(candidatesChilds.Contains(x))
                    //            {
                    //                ++Rating;
                    //            }
                    //        });
                    //        AllRating item = new AllRating();
                    //        item.candidate = c;
                    //        item.child = z;
                    //        item.Rating = Rating;
                    //        totalRating.Add(item);
                    //    });
                    //});
                    // int maxRating = totalRating.Min(x => x.Rating);
                    // Console.WriteLine(maxRating);
                    //clique.ForEach(Console.WriteLine);
                    //List<int> cut_neighbors = (from c in candidates select c).Distinct().ToList<int>();
                    //cut_neighbors.ForEach(Console.WriteLine);
                    //Это список точек входа в независимые подграфы исходного графа
                    //Пройдем по ним рекурсивным обходом, сохраняя список посещенных вершин
                    //Для каждой добавленной в клику вершины проверим список соседей, пересчитаем посещенные и щзапустим рекурсию по измененному списку вершин
                    //Для упрошения будем запускать рекурсивный обход только по тем вершинам, у которых число соседей больше оценки на макс. клику
                    // cut_neighbors.ForEach(v =>
                    //{
                    //    List<int> unvisited = graph.GetNeighbors(v);
                    //});
                    //List<int> unvisited = graph.GetNeighbors(cut_neighbors[0]);
                    //int count = 1;
                    //List<int> visited = new List<int>() { cut_neighbors[0] };
                    //Console.WriteLine(count);

                    List<List<int>> clique = new List<List<int>>();
                //Console.WriteLine(graph.ToString());
                //Console.ReadLine();

               for (int i =0; i<candidates.Count(); ++i)
               {
                    clique.Add(new List<int>());
                    List<int> cur = new List<int>();
                    List<int> neibours = graph.GetNeighbors(candidates[i]);
                    neibours.Add(candidates[i]);
                    //Console.WriteLine(String.Join(" ", neibours));
                    RecursiveSearch(candidates[i], ref cur, ref neibours, graph);
                    clique[i] = cur;
                    break;
                    //Console.WriteLine("Candidates"+candidates[i]);
                };
                int cnt = 0;
                clique.ForEach( c=>
                {
                    cnt++;
 //                   Console.WriteLine("Clique" + cnt);
                    c.ForEach(cc =>
                    {

                    });
//                    Console.WriteLine(c.Count());
                    });
                //Console.WriteLine("==MAX==");
                //Console.WriteLine(clique.Max(x=>x.Count()));
                //Console.WriteLine("==MIN==");
                //Console.WriteLine(clique.Min(x => x.Count()));
                Console.WriteLine("Global");
                Console.WriteLine(globalClick.Count());
                Console.WriteLine(String.Join(" ", globalClick));
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal: " + ex.Message);
            }
        }
        static void RecursiveSearch(int v, ref List<int> clique, ref List<int> neighbours, MyGraph g)
        {
            //Получить соседей для вершины
            List<int> candidates = g.GetNeighbors(v);
            candidates.Add(v);
            //Получить пересечение со списком всех вершин-соседей в клике;
            candidates = candidates.Intersect(neighbours).ToList<int>();

            //Console.WriteLine(String.Join(" ", candidates));
            //Получить список непомеченных вершин-соседей
            List<int> cut_n = candidates.Except(clique).ToList<int>();
            clique.Add(v);

            if (cut_n.Count() != 0)
            {

                for (int i =0; i< cut_n.Count(); ++i)
                {
                    int curV = cut_n[i];
                    var cliqueCopy = new List<int>(clique);

                    if(!(clique.Contains(curV)))
                    { 

                    RecursiveSearch(curV, ref clique, ref candidates, g);
                    //Console.WriteLine("WAIT!!!!! " + " globalClick.Count()-"+ globalClick.Count() + " clique-" + clique.Count());
                    if (globalClick.Count()<clique.Count())
                    {
                        var cp = globalClick;
                        globalClick.Clear();
                        globalClick.AddRange(clique);
                        Console.WriteLine(globalClick.Count());
                    }
                    clique.Clear();
                    clique.AddRange(cliqueCopy);
                    }
                }

               
            }
            else
            {
                return;
            }
        }
        static List<int> FindMaxClique(MyGraph graph, int maxTime, int targetCliqueSize)
        {
            List<int> clique = new List<int>();
            random = new Random(1);
            int time = 0;
            int timeBestClique = 0;
            int timeRestart = 0;
            int nodeToAdd = -1;
            int nodeToDrop = -1;
            int randomNode = random.Next(0, graph.NumberNodes);
           // Console.WriteLine("Adding node " + randomNode);
            clique.Add(randomNode);
            List<int> bestClique = new List<int>();
            bestClique.AddRange(clique);
            int bestSize = bestClique.Count;
            timeBestClique = time;
            List<int> possibleAdd = MakePossibleAdd(graph, clique);
            List<int> oneMissing = MakeOneMissing(graph, clique);
            while (time < maxTime && bestSize < targetCliqueSize)
            {
                ++time;
                bool cliqueChanged = false;
                if (possibleAdd.Count > 0)
                {
                    nodeToAdd = GetNodeToAdd(graph, possibleAdd);
                    //Console.WriteLine("Adding node " + nodeToAdd);
                    clique.Add(nodeToAdd);
                    clique.Sort();
                    cliqueChanged = true;
                    if (clique.Count > bestSize)
                    {
                        bestSize = clique.Count;
                        bestClique.Clear();
                        bestClique.AddRange(clique);
                    }
                }
                if (cliqueChanged == false)
                {
                    if (clique.Count > 0)
                    {
                        nodeToDrop = GetNodeToDrop(graph, clique, oneMissing);
                        //Console.WriteLine("Dropping node " + nodeToDrop);
                        clique.Remove(nodeToDrop);
                        clique.Sort();
                        cliqueChanged = true;
                    }
                } // удаление
                int restart = 2 * bestSize;
                if (time - timeBestClique > restart &&
                  time - timeRestart > restart)
                {
                    //Console.WriteLine("\nRestarting\n");
                    timeRestart = time;
                    int seedNode = random.Next(0, graph.NumberNodes);
                    clique.Clear();
                    //Console.WriteLine("Adding node " + seedNode);
                    clique.Add(seedNode);
                } // перезапуск
                possibleAdd = MakePossibleAdd(graph, clique);
                oneMissing = MakeOneMissing(graph, clique);
            } // цикл
            return bestClique;
        }
        static List<int> MakePossibleAdd(MyGraph graph, List<int> clique)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < graph.NumberNodes; ++i)
            {
                if (FormsALargerClique(graph, clique, i) == true)
                    result.Add(i);
            }
            return result;
        }
        static bool FormsALargerClique(MyGraph graph, List<int> clique, int node)
        {
            for (int i = 0; i < clique.Count; ++i)
            {
                if (graph.AreAdjacent(clique[i], node) == false)
                    return false;
            }
            return true;
        }
        static int GetNodeToAdd(MyGraph graph, List<int> possibleAdd)
        {
            if (possibleAdd.Count == 1)
                return possibleAdd[0];
            int maxDegree = 0;
            for (int i = 0; i < possibleAdd.Count; ++i)
            {
                int currNode = possibleAdd[i];
                int degreeOfCurrentNode = 0;
                for (int j = 0; j < possibleAdd.Count; ++j)
                {
                    int otherNode = possibleAdd[j];
                    if (graph.AreAdjacent(currNode, otherNode) == true)
                        ++degreeOfCurrentNode;
                }
                if (degreeOfCurrentNode > maxDegree)
                    maxDegree = degreeOfCurrentNode;
            }
            List<int> candidates = new List<int>();
            for (int i = 0; i < possibleAdd.Count; ++i)
            {
                int currNode = possibleAdd[i];
                int degreeOfCurrentNode = 0;
                for (int j = 0; j < possibleAdd.Count; ++j)
                {
                    int otherNode = possibleAdd[j];
                    if (graph.AreAdjacent(currNode, otherNode) == true)
                        ++degreeOfCurrentNode;
                }
                if (degreeOfCurrentNode == maxDegree)
                    candidates.Add(currNode);
            }
            return candidates[random.Next(0, candidates.Count)];
        }
        static List<int> MakeOneMissing(
  MyGraph graph, List<int> clique)
        {
            int count;
            List<int> result = new List<int>();
            for (int i = 0; i < graph.NumberNodes; ++i)
            {
                count = 0;
                if (graph.NumberNeighbors(i) < clique.Count - 1) continue;
                if (clique.BinarySearch(i) >= 0) continue;
                for (int j = 0; j < clique.Count; ++j)
                {
                    if (graph.AreAdjacent(i, clique[j]))
                        ++count;
                }
                if (count == clique.Count - 1)
                    result.Add(i);
            }
            return result;
        }
        static int GetNodeToDrop(MyGraph graph, List<int> clique,
  List<int> oneMissing)
        {
            if (clique.Count == 1)
                return clique[0];
            int maxCount = 0;
            for (int i = 0; i < clique.Count; ++i)
            {
                int currCliqueNode = clique[i];
                int countNotAdjacent = 0;
                for (int j = 0; j < oneMissing.Count; ++j)
                {
                    int currOneMissingNode = oneMissing[j];
                    if (graph.AreAdjacent(currCliqueNode,
                      currOneMissingNode) == false)
                        ++countNotAdjacent;
                }
                if (countNotAdjacent > maxCount)
                    maxCount = countNotAdjacent;
            }
            List<int> candidates = new List<int>();
            for (int i = 0; i < clique.Count; ++i)
            {
                int currCliqueNode = clique[i];
                int countNotAdjacent = 0;
                for (int j = 0; j < oneMissing.Count; ++j)
                {
                    int currOneMissingNode = oneMissing[j];
                    if (graph.AreAdjacent(currCliqueNode,
                      currOneMissingNode) == false)
                        ++countNotAdjacent;
                }
                if (countNotAdjacent == maxCount)
                    candidates.Add(currCliqueNode);
            }
            return candidates[random.Next(0, candidates.Count)];
        } // GetNodeToDrop
    }
}
