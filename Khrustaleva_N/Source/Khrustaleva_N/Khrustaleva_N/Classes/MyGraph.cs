using System;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.Classes;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApp1.Classes
{
    public class MyGraph
    {
        private BitMatrix data;
        private int numberNodes;
        private int numberEdges;
        private int[] numberNeighbors;

        public MyGraph(string graphFile, string fileFormat)
        {
            if (fileFormat.ToUpper() == "DIMACS")
                LoadDimacsFormatGraph(graphFile);
            else
                throw new Exception("Format " + fileFormat + " not supported");
        }
        /// <summary>
        /// Считать граф из файла (в формате записи информации о графе DIMACS
        /// </summary>
        /// <param name="graphFile">Путь к файлу</param>
        private void LoadDimacsFormatGraph(string graphFile)
        {
            FileStream ifs = new FileStream(graphFile, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            string line = "";
            string[] tokens = null;
            line = sr.ReadLine();
            line = line.Trim();
            while (line != null && line.StartsWith("p") == false)
            {
                line = sr.ReadLine();
                line = line.Trim();
            }
            tokens = line.Split(' ');
            int numNodes = int.Parse(tokens[2]);
            int numEdges = int.Parse(tokens[3]);
            ifs.Close();
            sr.Close(); 
            this.data = new BitMatrix(numNodes);
            ifs = new FileStream(graphFile, FileMode.Open);
            sr = new StreamReader(ifs);
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("e") == true)
                {
                    tokens = line.Split(' ');
                    int nodeA = int.Parse(tokens[1]) - 1;
                    int nodeB = int.Parse(tokens[2]) - 1;
                    data.SetValue(nodeA, nodeB, true);
                    data.SetValue(nodeB, nodeA, true);
                }
            }
            sr.Close(); ifs.Close();
            this.numberNeighbors = new int[numNodes];
            for (int row = 0; row < numNodes; ++row)
            {
                int count = 0;
                for (int col = 0; col < numNodes; ++col)
                {
                    if (data.GetValue(row, col) == true) ++count;
                }
                numberNeighbors[row] = count;
            }
            this.numberNodes = numNodes;
            this.numberEdges = numEdges;
            return;
        }
        /// <summary>
        /// Количество вершин в графе
        /// </summary>
        public int NumberNodes
        {
            get { return this.numberNodes; }
        }
        /// <summary>
        /// Количество ребер в графе
        /// </summary>
        public int NumberEdges
        {
            get { return this.numberEdges; }
        }
        /// <summary>
        /// Получить количество соседей для вершины
        /// </summary>
        /// <param name="node">Номер вершины</param>
        /// <returns></returns>
        public int NumberNeighbors(int node)
        {
            return this.numberNeighbors[node];
        }
        public string getMatrix()
        {
            return this.data.ToString();
        }
        /// <summary>
        /// Проверить, что вершины связаны ребром
        /// </summary>
        /// <param name="nodeA">Вершина А</param>
        /// <param name="nodeB">Вершина В</param>
        /// <returns></returns>
        public bool AreAdjacent(int nodeA, int nodeB)
        {
            if (this.data.GetValue(nodeA, nodeB) == true)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Переопределенный метод для представления связей вершин графа
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < this.data.Dim; ++i)
            {
                s += i + ": ";
                for (int j = 0; j < this.data.Dim; ++j)
                {
                    if (this.data.GetValue(i, j) == true)
                        s += j + " ";
                }
                s += Environment.NewLine;
            }
            return s;
        }
        /// <summary>
        /// Получение списка соседей для вершины
        /// </summary>
        /// <param name="vers"> Номер вершины</param>
        /// <returns></returns>
        public List<int> GetNeighbors(int vers)
        {
            List<int> neighbours = new List<int>();
            for (int j = 0; j < this.data.Dim; ++j)
            {
                if (this.data.GetValue(vers, j) == true)
                    neighbours.Add(j);
            }
            return neighbours;
        }

        public static void ValidateGraphFile(string graphFile, string fileFormat)
        {
            if (fileFormat.ToUpper() == "DIMACS")
                ValidateDimacsGraphFile(graphFile);
            else
                throw new Exception("Format " + fileFormat + " not supported");
        }

        public static void ValidateDimacsGraphFile(string graphFile)
        {
            // Сюда помещается код  
        }

        public void ValidateGraph()
        {
            // Сюда помещается код   
        }

        // -------------------------------------------------------------------
        
        // -------------------------------------------------------------------

    } // класс MyGraph
}
