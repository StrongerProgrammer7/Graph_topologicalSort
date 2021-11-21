using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LB15
{
    public class Graph
    {
        public class Node
        {
            public int id { get; set; } // id#
            public int active { get; set; } //status обработки узла
            public int prev { get; set; }
            public int chk { get; set; } // для нерек DFS
            public int x { get; set; } // coordinate
            public int y { get; set; }
            public string name { get; set; }
            public int count_ancestors { get; set; } // для топологической сортировки
            public List<int> edges;

            public void addEdge(int id)
            {
                if (!edges.Contains(id))
                    edges.Add(id); // добавить узел если не входит
            }

            public void removeEdge(int id)
            {
                edges.Remove(id);
            }

            public Node Clone()
            {
                List<int> clone = new List<int>(edges.Count);
                foreach (var i in edges)
                    clone.Add(i);
                return new Node
                {
                    id = this.id,
                    active = this.active,
                    prev = this.prev,
                    chk = this.chk,
                    x = this.x,
                    y = this.y,
                    name = this.name,
                    count_ancestors = this.count_ancestors,
                    edges = clone

                };
            }
        };

        public List<Node> nodes = new List<Node>(); // узлы графа
        private int maxid = 0;
        //для цикла в графе
        private List<int> list_cycle = new List<int>();
        public List<int> Get_list_cycle
        {
            get
            {
                return list_cycle;
            }
        }
        //-----------------Цикл в графе
        
        // Для отрисовки координаты
        public int x = 0;
        public int y = 0;
        public int size = 32;
        //-----------------------------


        //для топологической сортировки
        public bool bypass_topologicalsort_complete = false;
        //---------------------

        public int MaxID
        {
            get { return maxid; }
        }

        
        //Unoriented graph-----------------
        public void AddNode(string name)
        {
            bool exist = false;//уже существует
           foreach(Node vert in nodes)
           {
              if (vert.name == name)
                {
                  exist = true;
                  break;
               }
            }
            
            if(exist==false)
            {
                int id = Convert.ToInt32(name);
                if (maxid <= id)
                    maxid = id + 1;
                Node v = new Node();
                v.id = id;
                v.active = 0;
                v.prev = -1;
                v.chk = -1;
                v.x = x;
                v.y = y;
                if (name != "")
                    v.name = name;
                else
                    v.name = id.ToString();

                v.edges = new List<int>();// пустой список смежности
                nodes.Add(v);
                nodes.Sort((x, y) => x.id.CompareTo(y.id)); // сортировка по id для оптимизации
               
            }
           
        }

        //Добавление----------------------
        public void AddNode(int id,string name,List<int> edge,bool oriented)
        {
            bool exist = false;//уже существует
                foreach (Node vert in nodes)
                {
                    if (vert.id == id)
                    {
                        exist = true;
                        for (int j = 1; j < edge.Count; j++)
                            vert.addEdge(edge[j]);

                        edge.RemoveAt(0);
                        if(oriented==false)
                            unoriented_graph(id, edge);
                        else
                            oriented_graph(edge);
                    break;
                    }
                }

            if(exist==false)
            {
                if(maxid<=id)
                    maxid = id + 1;
                Node v = new Node();
                v.id = id;
                v.active = 0;
                v.prev = -1;
                v.chk = -1;
                v.x = 4*id*id+x;
                v.y = 4*id*id+y;
                if (name != "")
                    v.name = name;
                else
                    v.name = id.ToString();
                edge.RemoveAt(0);
                v.edges = edge;// пустой список смежности
                nodes.Add(v);
                nodes.Sort((x, y) => x.id.CompareTo(y.id)); // сортировка по id для оптимизации
                if (oriented == false)
                    unoriented_graph(id, edge);
                else
                    oriented_graph(edge);
            }
                    
        }
        /// <summary>
        /// Метод добавляет новые листы , чтобы граф был неориентированным
        /// № 2 4 6 , 2 смеж с 4 и 6 => 4 смеж с 2 и 6 смеж с 2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="edge"></param>
        private void unoriented_graph(int id,List<int> edge)//для неориентированного графа
        {
            
            for (int i = 0; i < edge.Count; i++)
            {
                List<int> alllist = new List<int>();
                alllist.Add(edge[i]);
                alllist.Add(id);
                int id1 = alllist[0];
                bool exist = false;//уже существует
                foreach (Node vert in nodes)
                {
                    if (vert.id == id1)
                    {
                        exist = true;
                        for (int j = 1; j < alllist.Count; j++)
                            vert.addEdge(alllist[j]);
                        break;
                    }
                }

                if (exist == false)
                {
                    if (maxid <= id1)
                        maxid = id1 + 1;
                    Node v = new Node();
                    v.id = id1;
                    v.active = 0;
                    v.prev = -1;
                    v.chk = -1;
                    v.x = 4*id1+x;
                    v.y = 4*id1+y;
                    v.name = id1.ToString();
                    alllist.RemoveAt(0);
                    v.edges = alllist;// пустой список смежности
                    nodes.Add(v);
                    nodes.Sort((x, y) => x.id.CompareTo(y.id)); // сортировка по id для оптимизации

                }
            }
        }
        //----------------------------------------------------------------

       //Ориентированное
        private void oriented_graph(List<int> edge)
        {
            for(int i=0;i<edge.Count;i++)
            {
                int id = edge[i];
                bool exist = false;//уже существует
                foreach (Node vert in nodes)
                {
                    if (vert.id == id)
                    {
                        vert.count_ancestors++;
                        exist = true;
                        break;
                    }
                }

                if (exist == false)
                {
                    if (maxid <= id)
                        maxid = id + 1;
                    Node v = new Node();
                    v.id = id;
                    v.active = 0;
                    v.prev = -1;
                    v.chk = -1;
                    Random rnd = new Random();
                    int dist = rnd.Next(0, 80);
                    int disty = rnd.Next(-95, 0);
                    v.x = dist+x;
                    v.y = disty+y;
                    v.count_ancestors++;
                    v.name = id.ToString();
                    
                    v.edges = new List<int>();// пустой список смежности
                    nodes.Add(v);
                    nodes.Sort((x, y) => x.id.CompareTo(y.id)); // сортировка по id для оптимизации

                }
            }
        }
        //---------------------------Добавление---------------------

        public void removeNode(int id)
        {
            Node n = null;
            foreach(Node vert in nodes)
            {
                vert.edges.Remove(id);
                if (vert.id == id)
                    n = vert;
            }
            nodes.Remove(n);
        }

     

        //Загрузка из файла
        public void loadgraph(int id,int x, int y, string name, List<int> edge)
        {
            Node v = new Node();
            if (maxid <= id)
                maxid = id + 1;
            v.id = id;
            v.active = 0;
            v.prev = - 1;
            v.chk = -1;
            v.x = x;
            v.y = y;
            if (name != "")
                v.name = name;
            else
                v.name = id.ToString();
            v.edges = edge;
            //все параметры необх для созд узла передются в функцию, вкл сп смеж
            nodes.Add(v);
            nodes.Sort((xx, yy) => xx.id.CompareTo(yy.id));
        }

        public object Clone()
        {
            Graph g = (Graph)this.MemberwiseClone();
            g.nodes = new List<Node>(nodes.Count);
            foreach (Node v in nodes)
                g.nodes.Add(v.Clone());
          /*  for (int i = 0; i < g.nodes.Count; i++)
                g.nodes.Add(this.nodes[i]);*/
            return g;
         /*   List<Node> nodes2 = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
                nodes2.Add(nodes[i]);
            return new Graph
            {
                bypass_topologicalsort_complete = this.bypass_topologicalsort_complete,
                maxid = this.maxid,
                nodes = nodes2,
                size = this.size,
                x = this.x,
                y=this.y
              
            };*/
        }
        public void clear()
        {
            maxid = 0;
            x = 0;
            y = 0;
            size = 32;
            bypass_topologicalsort_complete = false;
        }

        //Топлогическая сортировка для теста------------------
        public void topological_ForTest(ref List<int> top)
        {
            foreach (Node v in nodes)
            {
                v.active = 0;
                v.prev = -1;
                v.chk = -1;
            }
            if (nodes.Count > 0)
            {
                nodes[0].active = 1;

            }
            for (int i=0;i<nodes.Count*2;i++)
            {
                dfs(ref top);
            }

        }

        private void dfs(ref List<int> top)
        {
            bool activeNode = false; // если в графе есть активные узлы
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].active == 1) // найти активный узел
                {
                    if (nodes[i].chk == -1)
                    {
                        //do something
                        //(данный узел впервые обрабатывается, т.е. при повторной обработке(напр яв-я корнем) сюда не попадет)
                    }
                    bool unprocessedNode = false; // есть ли необработанные узлы в списке смежности
                    //перебрать все узлы в списке
                    while (!unprocessedNode && (nodes[i].chk < nodes[i].edges.Count - 1))
                    {
                        nodes[i].chk++;//менять  проверяемый элемент списка смежности
                        foreach (Node ed in nodes)
                        {
                            if (ed.id == nodes[i].edges[nodes[i].chk])//найти проверяемый узел
                            {
                                if (ed.active == 0) //если он не активный
                                {
                                    ed.active = 1;
                                    ed.prev = nodes[i].id;//указать себя в качестве родительского узла
                                    nodes[i].active = 3;//запомнить
                                    unprocessedNode = true;//в списке смежности есть необработанные узлы
                                    break;
                                }
                            }
                        }
                    }
                    if (!(nodes[i].chk < nodes[i].edges.Count - 1)) // если список смежности закончился
                    {
                        bool notfindActoveNode = true;//в списке смежности нет активных узлов
                        foreach (Node ed in nodes)
                        {
                            if (nodes[i].edges.Contains(ed.id) && ed.active == 1) // найти активный узел из списка 
                                notfindActoveNode = false; // найден активный узел
                        }
                        if (notfindActoveNode) // если в списке больше нет активных узлов
                        {
                            top.Add(nodes[i].id);
                            //do somthing
                            //(вызов при выходе из узла, когда он был уже полностью обработан)
                            //когда полностью обработан
                            nodes[i].active = 2;//узел был обработан
                            if (nodes[i].prev != -1) // если родитель
                            {
                                foreach (Node ed in nodes)
                                    if (ed.id == nodes[i].prev)//найти его
                                        ed.active = 1;//активировать предыдущий узел
                            }
                        }
                    }
                    activeNode = true;//активный узел найден
                    break;
                }
            }
            if (!activeNode) // если нет активных узлов
            {
                activeNode = false;//в графе все еще есть необработанные узлы
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].active == 0) // попытаться найти все еще необработанные узлы на случай если в гр есть узлы не связ с первым
                    {
                        nodes[i].active = 1;
                        activeNode = true;
                        break;
                    }
                }
                if (!activeNode) // если 100% узлы обр
                {
                    foreach (Node vert in nodes)
                        vert.active = 0;//вернуться в обычное состояние
                }
            }
        }//---------------------------------Тест-------------------------------

       
        // для поиска цикла
        public int dfs(int id,int cycle,int cycle_start)
        {
           
            if (cycle == 0)
            {
                foreach (Node v in nodes)
                {
                    if (v.id == id)
                    {
                        if (v.edges.Count > 0)
                        {
                            list_cycle.Add(v.id);
                        }else
                        {
                            int id_new = df(v.id);
                            return 0+ dfs(id_new, cycle, cycle_start);
                        }
                        
                        for (int i = 0; i < v.edges.Count; i++)
                        {
                            if (v.edges[i] == cycle_start)
                            {
                                cycle = 1;
                                return 1 + dfs(v.edges[i],cycle, cycle_start);
                                
                            }
                             return 0 + dfs(v.edges[i],cycle, cycle_start);
                        }
                        
                    }
                       
                }
                return 0;
            }
            else
                return 1;
        }

        private int df(int id)
        {
            foreach(Node g in nodes)
            {
                if(g.id>id)
                {
                    return g.id;
                }
            }
            return 0;
        }
    }//конец класса Graph

    public static class Match
    {
        public static double degtorad(double deg)//градусы в радианы
        {
            return deg * Math.PI / 180;
        }

        public static double radtodeg(double rad)//радианы в градусы
        {
            return rad / Math.PI * 180;
        }

        public static double lengthdir_x(double len, double dir)//расстояние по X при передвижении по направлению
        {
            return len * Math.Cos(degtorad(dir));
        }

        public static double lengthdir_y(double len, double dir)//расстояние по Y при передвижении по направлению
        {
            return len * Math.Sin(degtorad(dir)) * (-1);
        }

        public static double point_direction(int x1, int y1, int x2, int y2)//угол направления между двумя точками 
        {
            return 180 - radtodeg(Math.Atan2(y1 - y2, x1 - x2));
        }

        public static double point_distance(int x1, int y1, int x2, int y2)//расстояние между двумя точками
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }
    }
}

/*во множество вершин и во множество ребер. Способ представления ребер зависит от варианта индивидуальной задачи (списки смежности, матрица смежности, список ребер). 
 * Реализовать графический интерфейс с двойной буферизацией, в котором элементы графа можно свободно перемещать по области рисования.
2.	Разработать метод класса для обработки данных графа согласно индивидуальному заданию из табл. 16.
Результат обработки отражать графически и выводить посредством элементов управления TextBox согласно индивидуальному заданию из табл. 16.
3.	Операции загрузки, сохранения и обработки графа инициировать посредством элементов управления Button.
4.	Разработать модульный тест для метода обработки графа. При проверке читать граф из файла G.grf.
5.	При программировании задачи выполнять обработку исключительных ситуаций.
*/

/*Топологическая сортировка.
Вывести последовательно названия вершин в TextBox через запятую.	Н	Списки смежности	DFS
*/