using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.Classes;
using System.Timers;
using System.IO;
using ILOG.Concert;
using ILOG.CPLEX;

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
        public static int branchNumber = 0;
        static int Bound;
        static List<int> maxClique;
        static string graphFile = "";
        static DateTime startTime;
        static void Main(string[] args)
        {
            try
            {
                graphFile = args[0];
                int timeLimit = Convert.ToInt32(args[1]);
                Timer myTimer = new Timer();
                myTimer.Start();
                myTimer.Interval = timeLimit * 1000;
                startTime = DateTime.Now;
                myTimer.Elapsed += MyTimer_Elapsed;
                MyGraph graph = new MyGraph(graphFile, "DIMACS");
                int maxTime = 100;
                int targetCliqueSize = graph.NumberNodes;
                //Эвристически найдем хотя бы что-то похожее на максимальную клику (в пределах 5% ошибки - чтобы вернуть, если не успеет обсчитаться основной обход)
                maxClique = FindMaxClique(graph, maxTime,  targetCliqueSize);
                int ub = maxClique.Count;
                Bound = ub;
                List<List<int>> clique = new List<List<int>>();
                //Сортируем вершины по числу соседей, будем вызывать алгоритм для тех вершин, у которых количество соседей наибольшее
                Dictionary<int, int> nodeAndNeighbors = new Dictionary<int, int>();
                for (int i = 0; i < graph.NumberNodes; ++i)
                {
                    int numberNeighbors = graph.NumberNeighbors(i);
                    nodeAndNeighbors.Add(i, numberNeighbors);
                }
                //Сортируем вершины по уменьшению количества соседей
                nodeAndNeighbors = nodeAndNeighbors.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                List<int> colors = new List<int>() { 1 };
                //Раскраска графа
                int top = (from v in nodeAndNeighbors.Keys.ToList() select v).ToList<int>()[0];
                Dictionary<int, int> colorizedGraph = new Dictionary<int, int>();
                //Раскрасим граф
                colorizedGraph = colorize(nodeAndNeighbors.Keys.ToList<int>(), graph);
                int cntr = 0;
                //Зададим базовую модель
                Cplex cplex = new Cplex();
                IRange[][] rng = new IRange[1][];
                INumVar[][] var = new INumVar[1][];
                rng[0] = new IRange[graph.NumberNodes * graph.NumberNodes];
                // add the objective function
                double[] objvals = new double[graph.NumberNodes];
                string[] varname = new string[graph.NumberNodes];
                for (int i = 0; i < graph.NumberNodes; i++)
                {
                    objvals[i] = 1.0;
                    varname[i] = "x" + (i + 1);
                }
                INumVar[] x = cplex.NumVarArray(graph.NumberNodes, 0.0, 1.0, varname);
                var[0] = x;
                //Ограничение, что х лежит от нуля до единицы задали при инициализации
                cplex.AddMaximize(cplex.ScalProd(x, objvals));
                //Получим номер максимального цвета = это количество цветов, в которые окрашен граф
                //Будем иметь в виду, что количество цветов - это верхняя оценка на размер клики, а найденная эвристически клика на первом этапе - нижняя оценка.
                int colorCount = colorizedGraph.Values.Max();
                List<int> colorizedNodes = new List<int>();
                int pointer = 1;
                //Добавим ограничение, что вершины, входящие в один цветовой класс, не связаны между собой
                for (int i = 1; i <= colorCount; ++i)
                {
                    colorizedNodes = (from t in colorizedGraph where t.Value == i select t.Key).ToList<int>();
                    if (colorizedNodes.Count() != 1)
                    {
                        INumExpr[] constraint = new INumExpr[colorizedNodes.Count()];
                        int counter = 0;
                        colorizedNodes.ForEach(node =>
                        {
                            constraint[counter] = cplex.Prod(1.0, x[node]);
                            counter++;
                        });
                        rng[0][pointer] = cplex.AddLe(cplex.Sum(constraint), 1.0, "c" + (pointer));
                        pointer++;
                    }
                }
                for (int i =0; i<graph.NumberNodes; i++)
                {
                    for (int j = i+1; j<graph.NumberNodes; j++)
                    {
                        if (!graph.AreAdjacent(i, j))
                        {
                            rng[0][pointer] = cplex.AddLe(cplex.Sum(cplex.Prod(1.0, x[i]), cplex.Prod(1.0, x[j])), 1.0, "c" + (pointer));
                            pointer++;
                        }
                    }
                }

                //------------------------------------------------------------------------
                //-----Пробуем решать задачу ровно до тех пор, пока не получим клику------
                //-----Помним про ограничения на размер клики-----------------------------
                int countOfConstraint = colorCount;
                globalClick = maxClique;
                Branching(cplex, x);
                cplex.End();
                ////Максимальная клика, которую можно найти для вершины - это количество различных цветов, в которые окрашены все ее соседи плюс она  сама
                //foreach (KeyValuePair<int,int> pair in nodeAndNeighbors)
                //{
                //        List<int> neighbors = graph.GetNeighbors(pair.Key);
                //        neighbors.Add(pair.Key);
                //        var cols = (from n in colorizedGraph where neighbors.Exists(t => t == n.Key) select n.Value).Distinct().ToList<int>();
                //        if (cols.Count() >= Bound && cols.Count() >= globalClick.Count())
                //        {
                //            clique.Add(new List<int>());
                //            List<int> cur = new List<int>();
                //            RecursiveSearch(pair.Key, ref cur, ref neighbors, graph);
                //            clique[cntr] = cur;
                //            cntr++;
                //        }
                //}
                TimeSpan time = (DateTime.Now - startTime);
                Console.WriteLine("Time to find " + time);
                Console.WriteLine(globalClick.Count());
                Console.WriteLine(String.Join(" ", globalClick));
                WriteResults(time, globalClick, false);

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Fatal: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadKey();
            }
        }
        private static Dictionary<int, int> colorize(List<int> nodes, MyGraph graph)
        {
            //Раскраска графа
            List<int> colors = new List<int>() { 1 };
            Dictionary<int, int> colorizedGraph = new Dictionary<int, int>();
            int count = 0;
            nodes.ForEach(n =>
            {
                //Обработаем первую вершину из списка - покрасим ее в 1 цвет
                if (count == 0)
                {
                    colorizedGraph.Add(n, colors[0]);
                    count++;
                }
                else
                {
                    //Получить список соседей для данной вершины
                    List<int> neighbors = graph.GetNeighbors(n).Intersect(nodes).ToList<int>();
                    //Получить минимальный цвет из списка цветов, в который не окрашен ни один из соседей
                    neighbors = neighbors.Intersect(colorizedGraph.Keys.ToList<int>()).ToList<int>();//Список окрашенных соседей
                    var cols = (from c in colorizedGraph where neighbors.Exists(t => t == c.Key) select c.Value).Distinct().ToList<int>();//Список различных цветов окрашенных соседей
                    var avaliable = colors.Except(cols).ToList<int>();
                    //Если такого цвета нет - создать и добавить в список доступных цветов
                    if (avaliable.Count() == 0)
                    {
                        int newColor = colors.Max() + 1;
                        colors.Add(newColor);
                        colorizedGraph.Add(n, newColor);
                    }
                    else
                    {
                        int color = avaliable.Min();
                        colorizedGraph.Add(n, color);
                    }
                }

            });
            return colorizedGraph;   
        }
        private static int Branching( Cplex cplex, INumVar[] x)
        {
            cplex.Solve();
            double[] res = cplex.GetValues(x);
            double[] dj = cplex.GetReducedCosts(x);
            double solveResult = res.Sum();
            if (solveResult>globalClick.Count)
            {
                //Пытаемся выполнить ветвление
                //Если ветвиться не по чему - это наша искомая клика
                int? BranchNode = getNodeForBranching(res);
                if (BranchNode == null)
                {
                    var click = getClickFromSolution(res);
                    if (click.Count> globalClick.Count)
                    {
                        globalClick = click;
                    }
                    return click.Count();
                }
                else
                {
                    branchNumber++;
                    int currentBranchNumber = branchNumber;
                    var branchConstraint = cplex.AddEq(x[(int)BranchNode], 1.0, currentBranchNumber.ToString());
                    int For1Branch = Branching(cplex, x);
                    //Удалить ограничение
                    cplex.Remove(branchConstraint);
                    branchConstraint = cplex.AddEq(x[(int)BranchNode], 0.0, currentBranchNumber.ToString());
                    int For2Branch = Branching(cplex, x);
                    return For1Branch > For2Branch ? For1Branch : For2Branch;
                }
            }
            return 0;
        }
        private static int? getNodeForBranching(double[] res)
        {
            Dictionary<int, double> cuttedNodes = new Dictionary<int, double>();
            for (int j = 0; j < res.Length; ++j)
            {
                if (res[j] < 1 && res[j] > 0)
                {
                    cuttedNodes.Add(j, res[j]);
                }
            }
            int? result = (from r in cuttedNodes where r.Value == cuttedNodes.Values.Max() select r.Key).FirstOrDefault();
            return result;
        }
        private static List<int> getClickFromSolution(double[] res)
        {
            List<int> click = new List<int>();
            for (int j = 0; j < res.Length; ++j)
            {
                if (res[j] < 1 && res[j] > 0)
                {
                    click.Add(j);
                }
            }
            return click;
        }
        private static void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Time is out");
            TimeSpan time = DateTime.Now - startTime;
            if (globalClick.Count()> maxClique.Count())
            {
                Console.WriteLine(globalClick.Count());
                Console.WriteLine(String.Join(" ", globalClick));
                WriteResults(time, globalClick, true);
            }
            else
            {
                Console.WriteLine(maxClique.Count());
                Console.WriteLine(String.Join(" ", maxClique));
                WriteResults(time, maxClique, true);
            }
            Environment.Exit(0);
        }
        private static void WriteResults(TimeSpan time, List<int> clique, bool istimeLimitReached)
        {
            FileStream ifs = new FileStream(@".\Khrustaleva_N.xls", FileMode.Append);
            StreamWriter sw = new StreamWriter(ifs);
            string times = (time.Hours * 3600 + time.Minutes * 60 + time.Seconds).ToString();
            string result = graphFile + ";" + times + ";" + clique.Count() + ";" + istimeLimitReached + Environment.NewLine;
            sw.Write(result);
            sw.Close();
            ifs.Close();
        }
        static void RecursiveSearch(int v, ref List<int> clique, ref List<int> neighbours, MyGraph g)
        {
            //Получить соседей для вершины
            List<int> candidates = g.GetNeighbors(v);
            //Добавим саму вершину в список соседей
            candidates.Add(v);
            //Получить пересечение со списком всех вершин-соседей в клике - это общий список соседей клики при добавлении этой вершины
            candidates = candidates.Intersect(neighbours).ToList<int>();
            clique.Add(v);
            //Получить список вершин из общего списка соседей клики, которые еще не были добавлены в клику
            List<int> cut_n = candidates.Except(clique).ToList<int>();
           
            //Если есть еще нерассмотренные вершины из общего списка соседей клики
            if (cut_n.Count() != 0)
            {
                //Найдем вершину, максимально связанную с прочими нерассмотренными вершинами
                int node = GetNodeToAdd(g, cut_n, clique);
                if (node.Equals(-1))
                {
                    return;
                }
                RecursiveSearch(node, ref clique, ref candidates, g);
                if (globalClick.Count() < clique.Count())
                {
                    globalClick.Clear();
                    globalClick.AddRange(clique);
                }
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// Эвристический поиск максимальной клики
        /// </summary>
        /// <param name="graph">Граф</param>
        /// <param name="maxTime">Ограничение на количество запусков поиска</param>
        /// <param name="targetCliqueSize">Верхнее ограничение на размер клики</param>
        /// <returns></returns>
        static List<int> FindMaxClique(MyGraph graph, int maxTime, int targetCliqueSize)
        {
            List<int> clique = new List<int>();
            random = new Random(1);
            int time = 0;
            int timeBestClique = 0;
            int timeRestart = 0;
            int nodeToAdd = -1;
            int nodeToDrop = -1;
            //Выберем случайную вершину и добавим ее в клику
            int randomNode = random.Next(0, graph.NumberNodes);
            clique.Add(randomNode);
            //Сохраним полученную клику как возможное лучшее решение
            List<int> bestClique = new List<int>();
            bestClique.AddRange(clique);
            //Вычислим возможный лучший размер клики
            int bestSize = bestClique.Count;
            timeBestClique = time;
            //Получить список возможных вершин на добавление
            List<int> possibleAdd = MakePossibleAdd(graph, clique);
            //Получить список возможных вершин на удаление
            List<int> oneMissing = MakeOneMissing(graph, clique);
            while (time < maxTime && bestSize < targetCliqueSize)
            {
                ++time;
                bool cliqueChanged = false;
                if (possibleAdd.Count > 0)
                {
                    //Пробуем добавить вершину
                    nodeToAdd = GetNodeToAdd(graph, possibleAdd);
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
                //Если клика не улучшилась (не удалось добавить вершину) - попробуем удалить одну из вершин клики
                if (cliqueChanged == false)
                {
                    if (clique.Count > 0)
                    {
                        nodeToDrop = GetNodeToDrop(graph, clique, oneMissing);
                        clique.Remove(nodeToDrop);
                        clique.Sort();
                        cliqueChanged = true;
                    }
                } // удаление
                int restart = 2 * bestSize;
                //Если еще не достигнут предел по количеству операций - начинаем поиск заново
                if (time - timeBestClique > restart &&
                  time - timeRestart > restart)
                {
                    timeRestart = time;
                    int seedNode = random.Next(0, graph.NumberNodes);
                    clique.Clear();
                    clique.Add(seedNode);
                } // перезапуск
                possibleAdd = MakePossibleAdd(graph, clique);
                oneMissing = MakeOneMissing(graph, clique);
            } // цикл
            return bestClique;
        }
        /// <summary>
        /// Получить список возможных вершин на добавление к существующей клике
        /// </summary>
        /// <param name="graph">Граф</param>
        /// <param name="clique">Клика</param>
        /// <returns></returns>
        static List<int> MakePossibleAdd(MyGraph graph, List<int> clique)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < graph.NumberNodes; ++i)
            {
                //Если вершина не входит в клику - проверим, связана ли она со свеми вершинами в клике
                if (!clique.Exists(x => x == i))
                {
                    //Если вершина связана со всеми вершинами в клике - добавим ее в список кандидатов
                    bool isLinked = clique.Intersect(graph.GetNeighbors(i)).Count().Equals(clique.Count()) ? true : false;
                    if (isLinked)
                        result.Add(i);
                }
            }
            return result;
        }
        /// <summary>
        /// Проверить, что добавление данной вершины формирует клику большего размера
        /// </summary>
        /// <param name="graph">Граф</param>
        /// <param name="clique">Клика</param>
        /// <param name="node">Вершина на добавление</param>
        /// <returns></returns>
        static bool FormsALargerClique(MyGraph graph, List<int> clique, int node)
        {
            bool result = clique.Intersect(graph.GetNeighbors(node)).Count().Equals(clique.Count()) ? true : false;
            return result;
        }
        /// <summary>
        /// Получить вершину на добавление к клике
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="possibleAdd">Список возможных вершин на добавление</param>
        /// <returns></returns>
        static int GetNodeToAdd(MyGraph graph, List<int> possibleAdd)
        {
            //Если возможная вершина всего одна - ее и вернем
            if (possibleAdd.Count == 1)
                return possibleAdd[0];
            int maxDegree = 0;
            List<int> candidates = new List<int>();
            possibleAdd.ForEach(c =>
            {
               int degreeOfCurrentNode = 0;
                //Вычислим, со сколькими вершинами из списка возможных связана текущая выбранная вершина
                possibleAdd.ForEach(v =>
                {
                   if (graph.AreAdjacent(c, v) == true)
                       ++degreeOfCurrentNode;
                });
                //Таким образом получим максимальное значение связанности для вершин из списка возможных
                if (degreeOfCurrentNode > maxDegree)
                   maxDegree = degreeOfCurrentNode;
            });
            //Пройдем по списку возможных вершин и добавим в список кандидатов те из них, для которых степень связанности равна максимальной связанности в данном списке
            possibleAdd.ForEach(c =>
            {
               int degreeOfCurrentNode = 0;
                possibleAdd.ForEach(v =>
                {
                   if (graph.AreAdjacent(c, v) == true)
                       ++degreeOfCurrentNode;
                });
               if (degreeOfCurrentNode == maxDegree) 
                   candidates.Add(c);
            });
            //Вернем случайную вершину из кандидатов на добавление
            return candidates[random.Next(0, candidates.Count)];
        }
        /// <summary>
        /// Получить вершину на добавление к клике
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="possibleAdd">Список возможных вершин на добавление</param>
        /// <returns></returns>
        static int GetNodeToAdd(MyGraph graph, List<int> possibleAdd, List<int> clique)
        {
            //Если возможная вершина всего одна - ее и вернем
            if (possibleAdd.Count == 1)
                return possibleAdd[0];
            int maxDegree = 0;
            List<int> candidates = new List<int>();
            possibleAdd.ForEach(c =>
            {
                int degreeOfCurrentNode = 0;
                //Вычислим, со сколькими вершинами из списка возможных связана текущая выбранная вершина
                possibleAdd.ForEach(v =>
                {
                    if (graph.AreAdjacent(c, v) == true)
                    {
                        ++degreeOfCurrentNode;
                    }
                });
                //Таким образом получим максимальное значение связанности для вершин из списка возможных
                if (degreeOfCurrentNode > maxDegree)
                    maxDegree = degreeOfCurrentNode;
            });
            //Пройдем по списку возможных вершин и добавим в список кандидатов те из них, для которых степень связанности равна максимальной связанности в данном списке
            possibleAdd.ForEach(c =>
            {
                int degreeOfCurrentNode = 0;
                possibleAdd.ForEach(v =>
                {
                    if (graph.AreAdjacent(c, v) == true)
                        ++degreeOfCurrentNode;
                });
                if ((degreeOfCurrentNode == maxDegree) && !(clique.Exists(x => x==c)))
                    candidates.Add(c);
            });
            if (candidates.Count().Equals(0))
            {
                return -1;
            }
            //Вернем случайную вершину из кандидатов на добавление
            return candidates[random.Next(0, candidates.Count)];
        }
        static List<int> MakeOneMissing( MyGraph graph, List<int> clique)
        {
            int count;
            List<int> result = new List<int>();
            for (int i = 0; i < graph.NumberNodes; ++i)
            {
                count = 0;
                //Если число соседей данной вершины меньше размера клики - перейдем к рассмотрению следующей вершины
                if (graph.NumberNeighbors(i) < clique.Count - 1) continue;
                //Если данная вершина уже есть в клике - перейдем к следующей вершине
                if (clique.BinarySearch(i) >= 0) continue;
                //Подсчитаем количество связей данной вершины с вершинами клики
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
        /// <summary>
        /// Получить вершину на удаление из клики
        /// </summary>
        /// <param name="graph">Граф</param>
        /// <param name="clique">Клика</param>
        /// <param name="oneMissing">Список возможных вершин на удаление</param>
        /// <returns></returns>
        static int GetNodeToDrop(MyGraph graph, List<int> clique,  List<int> oneMissing)
        {
            //Если в клике всего одна вершина - ее и вернем
            if (clique.Count == 1)
                return clique[0];
            int maxCount = 0;
            for (int i = 0; i < clique.Count; ++i)
            {
                int currCliqueNode = clique[i];
                int countNotAdjacent = 0;
                //Подсчитаем количество не связанных между собой вершин
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
                //Если количество пропусков ребер у данной вершины равно максимуму по списку вершин на удаление - добавим эту вершину в список кандидатов на удаление
                if (countNotAdjacent == maxCount)
                    candidates.Add(currCliqueNode);
            }
            //Вернем случайную вершину из списка кандидатов на удаление
            return candidates[random.Next(0, candidates.Count)];
        } // GetNodeToDrop
    }
}
