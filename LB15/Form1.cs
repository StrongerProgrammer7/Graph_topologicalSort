using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;



namespace LB15
{
    public partial class Form1 : Form
    {
        public Graph graph = new Graph();
        public Graph graph_topological;
        public List<int> topogicalSort = new List<int>(); //сортировка

        public int drag = -1;//перемещ узел
        public int drage = -1; // узел из которого происходит добавление/удаление ребер

        //координаты двух точек для отображения линии,
        public int dx1 = 0;
        public int dy1 = 0;
        //----------------------------
        //при добавлении и   удалении ребре
        public int dx2 = 0;
        public int dy2 = 0;
        //------------------------------
        //для второго графа
        //координаты двух точек для отображения линии,
        public int d2x1 = 0;
        public int d2y1 = 0;
        //----------------------------
        //при добавлении и   удалении ребре
        public int d2x2 = 0;
        public int d2y2 = 0;
        //------------------------------

        public bool action_bypass = false; // запущен ли обход графа
        public bool cycle = false; // цикла нет
        public bool searchCycle_complete = false; // поиск цикла не завершен
        public int length = 0; // для анимации при топологической сортировки
        public Form1()
        {
            InitializeComponent();
            checkBox3.Enabled = false;
            button3.Enabled = false;
        }
 
        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //устанавливаем размеры формы
            pictureBox1.Width = this.Width - 16;
            pictureBox1.Height = this.Height - pictureBox1.Location.Y - 39;
            //------------------------------------

            //уст координат появления новых узлов
            graph.x = pictureBox1.Width / 2;
            graph.y = pictureBox1.Height / 2;
            //----------------------------

            Bitmap buffer = new Bitmap(Width, Height); // extra buf
            Graphics paint_graph = Graphics.FromImage(buffer);//двойная буферизация gfx

            //Кисти и ручки
            SolidBrush brush_text = new SolidBrush(Color.Blue); //myBrush
            SolidBrush brush_animation = new SolidBrush(Color.White); //myBrush2
            Pen pen_node = new Pen(Color.Black);//myPen
            Pen pen_edge = new Pen(Color.Red);//myPen2
            //----------------------------------

            //настройки по умолчанию
            paint_graph.Clear(Color.Gray); // очистка поверхности
            pen_edge.Color = Color.Red;
            brush_animation.Color = Color.Red;

            foreach(Graph.Node v in graph.nodes)
            {
                foreach(int eg in v.edges) // check all edge
                {
                    foreach(Graph.Node v2 in graph.nodes) // найти все узлы находящиесе в сп смежности
                    {
                        if(v2.id == eg)
                        {
                            double direction = Match.point_direction(v.x, v.y, v2.x, v2.y); // направление между узлами
                            double distantion = Match.point_distance(v.x, v.y, v2.x, v2.y); // дистанция между узлами
                            paint_graph.DrawLine(pen_edge, point1(v.x, v.y, direction, distantion), point2(v.x, v.y, direction, distantion)); // рисовка ребра
                            paint_graph.FillEllipse(brush_animation, paint_endEdge(v.x, v.y, distantion, direction)); // рисовка конца ребра
                        }
                    }
                }
            }

            foreach(Graph.Node v in graph.nodes)
            {
                brush_animation.Color = Color.White; // обычное состояние узла
                if(v.active==1)
                    brush_animation.Color = Color.SteelBlue; // обрабатывается
                if(v.active==2)
                    brush_animation.Color = Color.Gray;//обработан 
                if(v.active==3)
                    brush_animation.Color = Color.Gold; // узел сохр(не обр в дан момент и до конца еще не обработан) DFS
                paint_graph.FillEllipse(brush_animation, new Rectangle(v.x - graph.size / 2, v.y - graph.size / 2, graph.size, graph.size));//фон
                paint_graph.DrawEllipse(pen_node, new Rectangle(v.x - graph.size / 2, v.y - graph.size / 2, graph.size, graph.size));// контур
                paint_graph.DrawString(v.name, new Font("Arial", 8, FontStyle.Italic), brush_text, new PointF(v.x - graph.size / 3, v.y - 10)); // название
            }

            if(drage!=-1)// в каком то узле доб/уд ребра
            {
                brush_animation.Color = Color.Red;
                if(checkBox2.Checked==true)// удалении
                {
                    pen_edge.Color = Color.Yellow;
                    brush_animation.Color = Color.Yellow;
                }

                double direction_mouse = Match.point_direction(dx1, dy1, dx2, dy2); // направление от узла к указателю мыши
                double distantion_mouse = Match.point_distance(dx1, dy1, dx2, dy2);// расстояние между узлом и указателем мыши
                paint_graph.DrawLine(pen_edge, point1_mouse(dx1, dy1, direction_mouse, distantion_mouse),point2_mouse(dx1,dy1,direction_mouse,distantion_mouse));
                paint_graph.FillEllipse(brush_animation, new Rectangle(dx1 + (int)Match.lengthdir_x(distantion_mouse, direction_mouse) - 4, dy1 + (int)Match.lengthdir_y(distantion_mouse, direction_mouse) - 4, 8, 8));
            }

            pictureBox1.Image = buffer; // убирает мерцание
            brush_animation.Dispose();
            brush_text.Dispose();
            pen_edge.Dispose();
            pen_node.Dispose();

            if (graph_topological != null && checkBox3.Checked==true)
            {

                //уст координат появления новых узлов
                graph_topological.x = pictureBox1.Width / 2;
                graph_topological.y = pictureBox1.Height / 2;
                //----------------------------

                Bitmap buffer2 = new Bitmap(Width, Height); // extra buf
                Graphics paint_graph_toplogical = Graphics.FromImage(buffer2);//двойная буферизация gfx

                //Кисти и ручки
                SolidBrush brush_text2 = new SolidBrush(Color.Blue); //myBrush
                SolidBrush brush_animation2 = new SolidBrush(Color.Yellow); //myBrush2
                Pen pen_node2 = new Pen(Color.Black);//myPen
                Pen pen_edge2 = new Pen(Color.Red);//myPen2
               //----------------------------------

                //настройки по умолчанию
                paint_graph_toplogical.Clear(Color.Gray); // очистка поверхности
                pen_edge2.Color = Color.Green;
                brush_animation2.Color = Color.Green;

                foreach (Graph.Node v in graph_topological.nodes)
                {
                    foreach (int eg in v.edges) // check all edge
                    {
                        foreach (Graph.Node v2 in graph_topological.nodes) // найти все узлы находящиесе в сп смежности
                        {
                            if (v2.id == eg)
                            {
                                double direction = Match.point_direction(v.x, v.y, v2.x, v2.y); // направление между узлами
                                double distantion = Match.point_distance(v.x, v.y, v2.x, v2.y); // дистанция между узлами
                                paint_graph_toplogical.DrawLine(pen_edge2, point1_topological(v.x, v.y, direction, distantion), point2_topological(v.x, v.y, direction, distantion)); // рисовка ребра
                                paint_graph_toplogical.FillEllipse(brush_animation2, paint_endEdge_topological(v.x, v.y, distantion, direction)); // рисовка конца ребра
                            }
                        }
                    }
                }

                foreach (Graph.Node v in graph_topological.nodes)
                {
                    brush_animation2.Color = Color.White; // обычное состояние узла
                    if (v.active == 1)
                        brush_animation2.Color = Color.SteelBlue; // обрабатывается
                    if (v.active == 2)
                        brush_animation2.Color = Color.Gray;//обработан 
                    if (v.active == 3)
                        brush_animation2.Color = Color.Gold; // узел сохр(не обр в дан момент и до конца еще не обработан) DFS
                    paint_graph_toplogical.FillEllipse(brush_animation2, new Rectangle(v.x - graph_topological.size / 2, v.y - graph_topological.size / 2, graph_topological.size, graph_topological.size));//фон
                    paint_graph_toplogical.DrawEllipse(pen_node2, new Rectangle(v.x - graph_topological.size / 2, v.y - graph_topological.size / 2, graph_topological.size, graph_topological.size));// контур
                    paint_graph_toplogical.DrawString(v.name, new Font("Arial", 8, FontStyle.Italic), brush_text2, new PointF(v.x - graph_topological.size / 3, v.y - 10)); // название
                }

                if (drage != -1)// в каком то узле доб/уд ребра
                {
                    brush_animation2.Color = Color.Red;
                    if (checkBox2.Checked == true)// удалении
                    {
                        pen_edge2.Color = Color.Yellow;
                        brush_animation2.Color = Color.Yellow;
                    }

                    double direction_mouse = Match.point_direction(d2x1, d2y1, d2x2, d2y2); // направление от узла к указателю мыши
                    double distantion_mouse = Match.point_distance(d2x1, d2y1, d2x2, d2y2);// расстояние между узлом и указателем мыши
                    paint_graph_toplogical.DrawLine(pen_edge2, point1_mouse_topological(d2x1, d2y1, direction_mouse, distantion_mouse), point2_mouse_topological(d2x1, d2y1, direction_mouse, distantion_mouse));
                    paint_graph_toplogical.FillEllipse(brush_animation2, new Rectangle(d2x1 + (int)Match.lengthdir_x(distantion_mouse, direction_mouse) - 4, d2y1 + (int)Match.lengthdir_y(distantion_mouse, direction_mouse) - 4, 8, 8));
                }

                pictureBox1.Image = buffer2; // убирает мерцание
                brush_animation2.Dispose();
                brush_text2.Dispose();
                pen_edge2.Dispose();
                pen_node2.Dispose();
            }
        }

        private Point point1(int x, int y, double direction,double distance)
        {
            Point p = new Point(x + (int)Match.lengthdir_x(graph.size / 2, direction), y + (int)Match.lengthdir_y(graph.size / 2, direction));
            return p;
        }

        private Point point2(int x, int y, double direction, double distance)
        {
            Point p = new Point(x + (int)Match.lengthdir_x(distance - (graph.size / 2), direction), y + (int)Match.lengthdir_y(distance - (graph.size / 2), direction));
            return p;
        }

        private Rectangle paint_endEdge(int x,int y, double distance,double direction)
        {
            Rectangle c = new Rectangle(x+(int)Match.lengthdir_x(distance-(graph.size/2), direction)-4, y+(int)Match.lengthdir_y(distance-(graph.size/2),direction)-4,8,8);
            return c;
        }

        private Point point1_mouse(int x,int y,double direction,double distance)
        {
            Point p = new Point(x + (int)Match.lengthdir_x(graph.size / 2, direction), y + (int)Match.lengthdir_y(graph.size / 2, direction));
            return p;
        }
        private Point point2_mouse(int x, int y, double direction, double distance)
        {
            Point p = new Point(x + (int)Match.lengthdir_x(distance,direction), y + (int)Match.lengthdir_y(distance, direction));
            return p;
        }

        private Point point1_topological(int x, int y, double direction, double distance)
        {
            Point p = new Point(x + (int)Match.lengthdir_x(graph_topological.size / 2, direction), y + (int)Match.lengthdir_y(graph_topological.size / 2, direction));
            return p;
        }

        private Point point2_topological(int x, int y, double direction, double distance)
        {
            Point p = new Point(x + (int)Match.lengthdir_x(distance - (graph_topological.size / 2), direction), y + (int)Match.lengthdir_y(distance - (graph_topological.size / 2), direction));
            return p;
        }

        private Rectangle paint_endEdge_topological(int x, int y, double distance, double direction)
        {
            Rectangle c = new Rectangle(x + (int)Match.lengthdir_x(distance - (graph_topological.size / 2), direction) - 4, y + (int)Match.lengthdir_y(distance - (graph_topological.size / 2), direction) - 4, 8, 8);
            return c;
        }

        private Point point1_mouse_topological(int x, int y, double direction, double distance)
        {
            Point p = new Point(x + (int)Match.lengthdir_x(graph_topological.size / 2, direction), y + (int)Match.lengthdir_y(graph_topological.size / 2, direction));
            return p;
        }
        private Point point2_mouse_topological(int x, int y, double direction, double distance)
        {
            Point p = new Point(x + (int)Match.lengthdir_x(distance, direction), y + (int)Match.lengthdir_y(distance, direction));
            return p;
        }

        //Добавление вершин в граф
        private void Button4_Click(object sender, EventArgs e) //add
        {
            if (action_bypass == false)
            {
                if(length!=0)
                    graph.size = graph.size-length;
                string[] sNums = textBox1.Text.Split(' ');
                if (textBox1.Text != null && sNums.Length == 1)
                {
                    if (graph.bypass_topologicalsort_complete == true)
                    {
                        graph.bypass_topologicalsort_complete = false;
                        
                        foreach (Graph.Node v in graph.nodes)
                        {
                            string temp = Convert.ToString(v.name[0]);
                            v.name = "" + temp;
                        }
                        cycle = false;
                        searchCycle_complete = false;
                        button5.Enabled = true;
                        button3.Enabled = false;
                    }

                    graph.AddNode(sNums[0]);
                }else
                {
                    if(sNums.Length>1)
                    {
                        if (checkBox1.Checked == false) //добавить неориентированное ребро 
                        {
                            List<int> L = new List<int>();
                            foreach (string eg in sNums)
                                L.Add(int.Parse(eg));
                            if (graph.bypass_topologicalsort_complete == true)
                            {
                                graph.bypass_topologicalsort_complete = false;
                                foreach (Graph.Node v in graph.nodes)
                                {
                                    string temp = Convert.ToString(v.name[0]);
                                    v.name = "" + temp;
                                }
                            }
                            cycle = true;
                            searchCycle_complete = true;
                            button5.Enabled = false;
                            button3.Enabled = false;
                            button6.Enabled = false;
                            checkBox1.Enabled = false;
                            label3.Text = "Добавлено неориентрованное ребро\n в этом случае это уже цикл\n. Для топологической сортировки очистите область\n";
                            graph.AddNode(L[0], "", L,false);

                        }
                        else //или ориентированное
                         {
                            List<int> L = new List<int>();
                            foreach (string eg in sNums)
                                L.Add(int.Parse(eg));
                            if (graph.bypass_topologicalsort_complete == true)
                            {
                                graph.bypass_topologicalsort_complete = false;
                                foreach (Graph.Node v in graph.nodes)
                                {
                                    string temp = Convert.ToString(v.name[0]);
                                    v.name = "" + temp;
                                }
                            }
                            cycle = false;
                            searchCycle_complete = false;
                            button5.Enabled = true;
                            button3.Enabled = false;
                            checkBox1.Enabled = false;
                            graph.AddNode(L[0], "", L,true);
                        }
                    }
                }
                  
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {

            if(action_bypass==false)
            {
                progressBar1.Visible = false;
                trackBar1.Enabled = false;
                button4.Enabled = false;
                button1.Enabled =false;
                button6.Text = "Stop";
            }
            else
            {
                button6.Text = "Обход графа";
                trackBar1.Enabled = true;
                button4.Enabled = true;
                progressBar1.Visible = true;
                button1.Enabled = true;
            }
            
            //недоступна кнопка сохранения в файл
            if (graph.nodes.Count == 0 || action_bypass == false)
                button2.Enabled = false;
            else
                button2.Enabled = true;

            graph.size = trackBar1.Value;//регулировка размера узлов для отрисовки

            if(checkBox3.Checked == false)
            {
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    for (int j = 0; j < graph.nodes.Count; j++)
                    {
                        if (i != j) // узел не выталкивает сам себя
                        {
                            double distance = Match.point_distance(graph.nodes[i].x, graph.nodes[i].y, graph.nodes[j].x, graph.nodes[j].y);
                            int dist_extra = 10;//доп расст между узлами
                            if (distance <= (graph.size + dist_extra))//если два разных узла оказались внутри друг друга => придадим случ расст
                            {
                                if (graph.nodes[i].x == graph.nodes[j].x)
                                {
                                    graph.nodes[i].x = extra_distantion(graph.nodes[i].x);
                                }

                                if (graph.nodes[i].y == graph.nodes[j].y)
                                {
                                    graph.nodes[i].y = extra_distantion(graph.nodes[i].y);
                                }

                                //узлы выталкивают в противоположных направлениях
                                if (graph.nodes[i].x < graph.nodes[j].x)
                                {
                                    graph.nodes[i].x -= (int)(graph.size + dist_extra - distance);
                                    graph.nodes[j].x += (int)(graph.size + dist_extra - distance);
                                }
                                else
                                {
                                    graph.nodes[i].x += (int)(graph.size + dist_extra - distance);
                                    graph.nodes[j].x -= (int)(graph.size + dist_extra - distance);
                                }
                                if (graph.nodes[i].y < graph.nodes[j].y)
                                {
                                    graph.nodes[i].y -= (int)(graph.size + dist_extra - distance);
                                    graph.nodes[j].y += (int)(graph.size + dist_extra - distance);
                                }
                                else
                                {
                                    graph.nodes[i].y += (int)(graph.size + dist_extra - distance);
                                    graph.nodes[j].y -= (int)(graph.size + dist_extra - distance);
                                }
                            }
                        }
                    }
                    //не позволит выйти за пределы экрана
                    go_off_screen(ref graph.nodes, i);
                }
            }
            


            if (graph_topological != null && checkBox3.Checked == true)
            {
                graph_topological.size = trackBar1.Value;//регулировка размера узлов для отрисовки

                for (int i = 0; i < graph_topological.nodes.Count; i++)
                {
                    for (int j = 0; j < graph_topological.nodes.Count; j++)
                    {
                        if (i != j) // узел не выталкивает сам себя
                        {
                            double distance = Match.point_distance(graph_topological.nodes[i].x, graph_topological.nodes[i].y, graph_topological.nodes[j].x, graph_topological.nodes[j].y);
                            int dist_extra = 10;//доп расст между узлами
                            if (distance <= (graph_topological.size + dist_extra))//если два разных узла оказались внутри друг друга => придадим случ расст
                            {
                                if (graph_topological.nodes[i].x == graph_topological.nodes[j].x)
                                {
                                    graph_topological.nodes[i].x = extra_distantion_topological(graph_topological.nodes[i].x);
                                }

                                if (graph_topological.nodes[i].y == graph_topological.nodes[j].y)
                                {
                                    graph_topological.nodes[i].y = extra_distantion_topological(graph_topological.nodes[i].y);
                                }

                                //узлы выталкивают в противоположных направлениях
                                if (graph_topological.nodes[i].x < graph_topological.nodes[j].x)
                                {
                                    graph_topological.nodes[i].x -= (int)(graph_topological.size + dist_extra - distance);
                                    graph_topological.nodes[j].x += (int)(graph_topological.size + dist_extra - distance);
                                }
                                else
                                {
                                    graph_topological.nodes[i].x += (int)(graph_topological.size + dist_extra - distance);
                                    graph_topological.nodes[j].x -= (int)(graph_topological.size + dist_extra - distance);
                                }
                                if (graph_topological.nodes[i].y < graph_topological.nodes[j].y)
                                {
                                    graph_topological.nodes[i].y -= (int)(graph_topological.size + dist_extra - distance);
                                    graph_topological.nodes[j].y += (int)(graph_topological.size + dist_extra - distance);
                                }
                                else
                                {
                                    graph_topological.nodes[i].y += (int)(graph_topological.size + dist_extra - distance);
                                    graph_topological.nodes[j].y -= (int)(graph_topological.size + dist_extra - distance);
                                }
                            }
                        }
                    }
                    //не позволит выйти за пределы экрана
                    go_off_screen_topological(ref graph_topological.nodes, i);
                }
               for (int i = 0; i < graph_topological.nodes.Count; i++)
                {
                    double distance = Match.point_distance(graph_topological.nodes[i].x, graph_topological.nodes[i].y, graph.nodes[i].x, graph.nodes[i].y);
                    int dist_extra = 10;//доп расст между узлами
                    if (distance <= (graph_topological.size + dist_extra))//если два разных узла оказались внутри друг друга => придадим случ расст
                    {
                        if (graph_topological.nodes[i].x == graph.nodes[i].x)
                        {
                            graph_topological.nodes[i].x = extra_distantion_topological(graph_topological.nodes[i].x);
                        }

                        if (graph_topological.nodes[i].y == graph.nodes[i].y)
                        {
                            graph_topological.nodes[i].y = extra_distantion_topological(graph_topological.nodes[i].y);
                        }

                        //узлы выталкивают в противоположных направлениях
                        if (graph_topological.nodes[i].x < graph.nodes[i].x)
                        {
                            graph_topological.nodes[i].x -= (int)(graph_topological.size + dist_extra - distance);
                            graph.nodes[i].x += (int)(graph.size + dist_extra - distance);
                        }
                        else
                        {
                            graph_topological.nodes[i].x += (int)(graph_topological.size + dist_extra - distance);
                            graph.nodes[i].x -= (int)(graph.size + dist_extra - distance);
                        }
                        if (graph_topological.nodes[i].y < graph.nodes[i].y)
                        {
                            graph_topological.nodes[i].y -= (int)(graph_topological.size + dist_extra - distance);
                            graph.nodes[i].y += (int)(graph.size + dist_extra - distance);
                        }
                        else
                        {
                            graph_topological.nodes[i].y += (int)(graph_topological.size + dist_extra - distance);
                            graph.nodes[i].y -= (int)(graph.size + dist_extra - distance);
                        }
                    }
                }
            }
            Refresh(); // перерисовка формы
        }

        private void go_off_screen(ref List<Graph.Node> gr,int i)
        {
            if (graph.nodes[i].x - graph.size / 2 < 0)
                graph.nodes[i].x = graph.size / 2;
            if (graph.nodes[i].y - graph.size / 2 < 0)
                graph.nodes[i].y = graph.size / 2;
            if (graph.nodes[i].x + graph.size / 2 > pictureBox1.Width)
                graph.nodes[i].x = pictureBox1.Width - graph.size / 2 - 1;
            if (graph.nodes[i].y + graph.size / 2 > pictureBox1.Height)
                graph.nodes[i].y = pictureBox1.Height - graph.size / 2 - 1;
        }

        private  int extra_distantion(int x_y)
        {
            var rand = new Random();
            if (rand.Next(2) == 1)
                return x_y += 1;
            else
                return x_y -= 1;
        }

        private void go_off_screen_topological(ref List<Graph.Node> gr, int i)
        {
            if (graph_topological.nodes[i].x - graph_topological.size / 2 < 0)
                graph_topological.nodes[i].x = graph_topological.size / 2;
            if (graph_topological.nodes[i].y - graph_topological.size / 2 < 0)
                graph_topological.nodes[i].y = graph_topological.size / 2;
            if (graph_topological.nodes[i].x + graph_topological.size / 2 > pictureBox1.Width)
                graph_topological.nodes[i].x = pictureBox1.Width - graph_topological.size / 2 - 1;
            if (graph_topological.nodes[i].y + graph_topological.size / 2 > pictureBox1.Height)
                graph_topological.nodes[i].y = pictureBox1.Height - graph_topological.size / 2 - 1;
        }

        private int extra_distantion_topological(int x_y)
        {
            var rand = new Random();
            if (rand.Next(2) == 1)
                return x_y += 1;
            else
                return x_y -= 1;
        }

        //Движение мышью----------------------------------
        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(checkBox3.Checked==false)
            {
                if (drag != -1) // если перемещается какой то узел
                {
                    foreach (Graph.Node v in graph.nodes)
                    {
                        if (drag == v.id)
                        {
                            //переместить узел по координам мыши
                            v.x = e.X;
                            v.y = e.Y;
                            break;
                        }
                    }
                }
                if (drage != -1) // если из какого узла происходит добавление/уд ребер
                {
                    foreach (Graph.Node v in graph.nodes)
                    {
                        if (drage == v.id)
                        {
                            //запомнить коорд узла и указ мыши
                            dx1 = v.x;
                            dy1 = v.y;
                            dx2 = e.X;
                            dy2 = e.Y;
                            break;
                        }
                    }
                }
            }
            
            if(graph_topological!=null && checkBox3.Checked==true)
            {
                if (drag != -1) // если перемещается какой то узел
                {
                    foreach (Graph.Node v in graph_topological.nodes)
                    {
                        if (drag == v.id)
                        {
                            //переместить узел по координам мыши
                            v.x = e.X;
                            v.y = e.Y;
                            break;
                        }
                    }
                }
                if (drage != -1) // если из какого узла происходит добавление/уд ребер
                {
                    foreach (Graph.Node v in graph_topological.nodes)
                    {
                        if (drage == v.id)
                        {
                            //запомнить коорд узла и указ мыши
                            d2x1 = v.x;
                            d2y1 = v.y;
                            d2x2 = e.X;
                            d2y2 = e.Y;
                            break;
                        }
                    }
                }
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if(checkBox3.Checked==false)
            {
                if (e.Button == MouseButtons.Left)
                {
                    drage = -1;//off add/del edge
                    if (drag == -1)
                    {
                        foreach (Graph.Node v in graph.nodes)
                        {
                            if (Match.point_distance(v.x, v.y, e.X, e.Y) < graph.size / 2) // найти узел на который нажали
                            {
                                drag = v.id; // захват
                                v.x = e.X; // переместить
                                v.y = e.Y; // по координатам мыши
                                break;
                            }
                        }
                    }
                }

                if (action_bypass == false)
                {
                    if (e.Button == MouseButtons.Middle)
                    {
                        drag = -1;//off move
                        drage = -1;//of add/del edge
                        foreach (Graph.Node v in graph.nodes)
                        {
                            if (Match.point_distance(v.x, v.y, e.X, e.Y) < graph.size / 2)
                            {
                                if (checkBox2.Checked == true)
                                    graph.removeNode(v.id); //del node
                                break;
                            }
                        }
                    }
                    if (e.Button == MouseButtons.Right)
                    {
                        //сброс координат
                        drag = -1;
                        dx1 = 0;
                        dy1 = 0;
                        dx2 = 0;
                        dy2 = 0;
                        //-----------------
                        foreach (Graph.Node v in graph.nodes)
                        {
                            if (Match.point_distance(v.x, v.y, e.X, e.Y) < graph.size / 2)
                            {
                                //начать добавление/уд ребер из него
                                drage = v.id;
                                dx1 = v.x;
                                dy1 = v.y;
                                dx2 = e.X;
                                dy2 = e.Y;
                                break;
                            }
                        }
                    }
                }
            }
            
            if (graph_topological != null && checkBox3.Checked==true)
            {
                if (e.Button == MouseButtons.Left)
                {
                    drage = -1;//off add/del edge
                    if (drag == -1)
                    {
                        foreach (Graph.Node v in graph_topological.nodes)
                        {
                            if (Match.point_distance(v.x, v.y, e.X, e.Y) < graph_topological.size / 2) // найти узел на который нажали
                            {
                                drag = v.id; // захват
                                v.x = e.X; // переместить
                                v.y = e.Y; // по координатам мыши
                                break;
                            }
                        }
                    }
                }

                if (action_bypass == false)
                {
                    if (e.Button == MouseButtons.Middle)
                    {
                        drag = -1;//off move
                        drage = -1;//of add/del edge
                        foreach (Graph.Node v in graph_topological.nodes)
                        {
                            if (Match.point_distance(v.x, v.y, e.X, e.Y) < graph_topological.size / 2)
                            {
                                if (checkBox2.Checked == true)
                                    graph_topological.removeNode(v.id); //del node
                                break;
                            }
                        }
                    }
                    if (e.Button == MouseButtons.Right)
                    {
                        //сброс координат
                        drag = -1;
                        d2x1 = 0;
                        d2y1 = 0;
                        d2x2 = 0;
                        d2y2 = 0;
                        //-----------------
                        foreach (Graph.Node v in graph_topological.nodes)
                        {
                            if (Match.point_distance(v.x, v.y, e.X, e.Y) < graph_topological.size / 2)
                            {
                                //начать добавление/уд ребер из него
                                drage = v.id;
                                d2x1 = v.x;
                                d2y1 = v.y;
                                d2x2 = e.X;
                                d2y2 = e.Y;
                                break;
                            }
                        }
                    }
                }
            }


        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if(checkBox3.Checked==false)
            {
                if (e.Button == MouseButtons.Left)
                    drag = -1;
                if (e.Button == MouseButtons.Right)
                {
                    if (drage != -1)//если из какого то узла начато добавление/удаление рёбер
                    {
                        foreach (Graph.Node v in graph.nodes)
                        {
                            if (Match.point_distance(v.x, v.y, e.X, e.Y) < graph.size / 2) // найти узел на котором отпустили кнопку
                            {
                                if (v.id != drage) // если это не тот же самый узел
                                {
                                    foreach (Graph.Node v2 in graph.nodes)
                                    {
                                        if (v2.id == drage) // Найти узел из которого было запущено добавление/уд ребер
                                        {
                                            if (checkBox2.Checked == true)//удаление
                                            {
                                                v2.removeEdge(v.id);
                                                if (checkBox1.Checked == false)
                                                    v.removeEdge(v2.id); // если неориент удалить в обоих узлах
                                            }
                                            else
                                            {
                                                v2.addEdge(v.id);
                                                if (checkBox1.Checked == false)
                                                    v.addEdge(v2.id);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    drage = -1;
                }
            }
            
            if (graph_topological != null && checkBox3.Checked==true)
            {
                if (e.Button == MouseButtons.Left)
                    drag = -1;
                if (e.Button == MouseButtons.Right)
                {
                    if (drage != -1)//если из какого то узла начато добавление/удаление рёбер
                    {
                        foreach (Graph.Node v in graph_topological.nodes)
                        {
                            if (Match.point_distance(v.x, v.y, e.X, e.Y) < graph_topological.size / 2) // найти узел на котором отпустили кнопку
                            {
                                if (v.id != drage) // если это не тот же самый узел
                                {
                                    foreach (Graph.Node v2 in graph_topological.nodes)
                                    {
                                        if (v2.id == drage) // Найти узел из которого было запущено добавление/уд ребер
                                        {
                                            if (checkBox2.Checked == true)//удаление
                                            {
                                                v2.removeEdge(v.id);
                                                if (checkBox1.Checked == false)
                                                    v.removeEdge(v2.id); // если неориент удалить в обоих узлах
                                            }
                                            else
                                            {
                                                v2.addEdge(v.id);
                                                if (checkBox1.Checked == false)
                                                    v.addEdge(v2.id);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    drage = -1;
                }
            }
        }
        //------------------------------Мышь----------------------

        private void Timer2_Tick(object sender, EventArgs e)
        {
            timer2.Interval = trackBar2.Value; // регулировка скорости

            //DFS---------
            DFS(false);
            //------------DFS----

            //обнов полосы прогресса в зависимости от кол обработ узлов
            int k = 0;
            foreach (Graph.Node v in graph.nodes)
                if (v.active == 2) k += 1;
            progressBar1.Maximum = Math.Abs(k-graph.nodes.Count);
            progressBar1.PerformStep();
            
        }
        public List<int> cycle_list = new List<int>();
        private void DFS(bool cycle)
        {
            bool activeNode = false; // если в графе есть активные узлы
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i].active == 1) // найти активный узел
                {
                    if (graph.nodes[i].chk == -1)
                    {
                        if(cycle==true)
                        {
                            if (cycle_list.Contains(graph.nodes[i].id) == false)
                                cycle_list.Add(graph.nodes[i].id);
                        }
 
                        //do something
                        //(данный узел впервые обрабатывается, т.е. при повторной обработке(напр яв-я корнем) сюда не попадет)
                    }
                    bool unprocessedNode = false; // есть ли необработанные узлы в списке смежности
                    //перебрать все узлы в списке
                    while (!unprocessedNode && (graph.nodes[i].chk < graph.nodes[i].edges.Count - 1))
                    {
                        graph.nodes[i].chk++;//менять  проверяемый элемент списка смежности
                        foreach (Graph.Node ed in graph.nodes)
                        {
                            if (ed.id == graph.nodes[i].edges[graph.nodes[i].chk])//найти проверяемый узел
                            {
                                if (ed.active == 0) //если он не активный
                                {
                                    ed.active = 1;
                                    ed.prev = graph.nodes[i].id;//указать себя в качестве родительского узла
                                    graph.nodes[i].active = 3;//запомнить
                                    unprocessedNode = true;//в списке смежности есть необработанные узлы
                                    break;
                                }
                            }
                        }
                    }
                    if (!(graph.nodes[i].chk < graph.nodes[i].edges.Count - 1)) // если список смежности закончился
                    {
                        bool notfindActoveNode = true;//в списке смежности нет активных узлов
                        foreach (Graph.Node ed in graph.nodes)
                        {
                            if (graph.nodes[i].edges.Contains(ed.id) && ed.active == 1) // найти активный узел из списка 
                                notfindActoveNode = false; // найден активный узел
                        }
                        if (notfindActoveNode) // если в списке больше нет активных узлов
                        {
                            if(cycle==false)
                            {
                                textBox2.Text += graph.nodes[i].id.ToString() + " ";
                                topogicalSort.Add(graph.nodes[i].id);
                            }else
                            {
                                bool complete = false; // данную вершину проверили на цикл
                                foreach (Graph.Node v in graph.nodes)
                                {
                                    if (v.edges.Count != 0)
                                    {
                                        for (int k = 0; k < cycle_list.Count; k++)
                                        {
                                            if (v.id == cycle_list[k]&& cycle_list != null)
                                            {
                                                for (int j = 0; j < v.edges.Count; j++)
                                                    if (v.edges[j] == cycle_list[0])
                                                    {
                                                        foreach(Graph.Node v2 in graph.nodes)
                                                        {
                                                            if (v2.id == cycle_list[0] && cycle_list != null)
                                                            {
                                                               
                                                                if (graph.dfs(v2.id, 0, cycle_list[0]) > 0)
                                                                {
                                                                    this.cycle = true;
                                                                    for (int h = 0; h < graph.Get_list_cycle.Count; h++)
                                                                        label3.Text += Convert.ToString(graph.Get_list_cycle[h]);
                                                                    graph.Get_list_cycle.Clear();
                                                                }
                                                                    
                                                                complete = true;
                                                            }
                                                        }
                                                        
                                                    }       
                                            }
                                        }
                                        if (cycle_list != null && complete == true)
                                        {
                                            complete = false;
                                            cycle_list.RemoveAt(0);
                                        }
                                    }
                                    else
                                        cycle_list.Remove(v.id);
                                }
                            }
                            
                            //do somthing
                            //(вызов при выходе из узла, когда он был уже полностью обработан)
                            //когда полностью обработан
                            graph.nodes[i].active = 2;//узел был обработан
                            if (graph.nodes[i].prev != -1) // если родитель
                            {
                                foreach (Graph.Node ed in graph.nodes)
                                    if (ed.id == graph.nodes[i].prev)//найти его
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
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    if (graph.nodes[i].active == 0) // попытаться найти все еще необработанные узлы на случай если в гр есть узлы не связ с первым
                    {
                        graph.nodes[i].active = 1;
                        activeNode = true;
                        break;
                    }
                }
                if (!activeNode) // если 100% узлы обр
                {
                    if(cycle==false)
                    {
                        foreach (Graph.Node vert in graph.nodes)
                            vert.active = 0;//вернуться в обычное состояние
                        timer2.Stop();
                    }else
                    {
                        foreach (Graph.Node vert in graph.nodes)
                            vert.active = 0;//вернуться в обычное состояние
                        searchCycle_complete = true;
                        timer3.Stop();
                    }
                   
                    action_bypass = false;
                }
            }
        }


        private void Button6_Click(object sender, EventArgs e) // обход
        {
            textBox2.Clear();
            if(graph.bypass_topologicalsort_complete==false)
            {
                if (trackBar2.Value == 0)
                {
                    trackBar2.Value = 1;
                    timer2.Interval = trackBar2.Value;
                }
                else
                    timer2.Interval = trackBar2.Value;
                drag = -1;
                drage = -1;
                foreach (Graph.Node v in graph.nodes)
                {
                    v.active = 0;
                    v.prev = -1;
                    v.chk = -1;
                }
                if (timer2.Enabled==false && action_bypass==false)
                {
                    if (graph.nodes.Count > 0)
                    {
                        graph.nodes[0].active = 1;
                        timer2.Start();
                        action_bypass = true;
                        progressBar1.Value = 0;
                    }
                }
                else
                {
                    timer2.Stop();
                    action_bypass = false;
                }
            }
           
        }

        //Топологическая сортировка
        private void Button3_Click(object sender, EventArgs e) //topological
        {
            try
            {
                if(topogicalSort != null)
                {
                    graph_topological = (Graph)graph.Clone();
                    checkBox3.Enabled = true;
                    graph.bypass_topologicalsort_complete = true;
                    if (topogicalSort != null)
                    {
                        topogicalSort.Reverse();

                        for (int i = 0; i < topogicalSort.Count; i++)
                        {
                            foreach (Graph.Node vert in graph.nodes)
                            {
                                if (vert.id == topogicalSort[i])
                                {
                                    vert.name = i.ToString() + "(" + vert.name + ")";
                                    if (length < vert.name.Length)
                                        length = vert.name.Length;
                                }
                            }
                        }
                        topogicalSort.Clear();

                        graph.size = graph.size + length;
                        int sizebool = graph.MaxID;
                        foreach (Graph.Node node in graph.nodes)
                        {
                            bool[] find = new bool[sizebool * 4 + 1];
                            for (int i = 0; i < node.edges.Count * 4; i++)
                                find[i] = false;
                            List<int> edg = new List<int>(node.edges.Count);
                            for (int i = 0; i < node.edges.Count; i++)
                            {
                                foreach (Graph.Node v in graph.nodes)
                                {
                                    string name = "";
                                    bool met_bracket = false;
                                    for (int j = 0; j < v.name.Length; j++)
                                    {
                                        if (met_bracket == false && v.name[j] == '(')
                                        {
                                            met_bracket = true;

                                        }
                                        if (met_bracket == true && ((v.name[j] >= '0' && v.name[j] <= '9') || (v.name[j] >= '0' && v.name[j] <= '9')))
                                        {
                                            name += v.name[j];
                                        }
                                        if (v.name[j] == ')')
                                            break;
                                    }
                                    if (node.edges[i] == Convert.ToInt32(Convert.ToString(name)))
                                    {
                                        string id_v = "";
                                        for (int j = 0; j < v.name.Length; j++)
                                        {
                                            if (v.name[j] == '(')
                                            {
                                                break;

                                            }
                                            if ((v.name[j] > '0' && v.name[j] < '9') || (v.name[j] > '0' && v.name[j] < '9'))
                                            {
                                                id_v += v.name[j];
                                            }
                                        }

                                        edg.Add(Convert.ToInt32(Convert.ToString(id_v)));
                                        find[Convert.ToInt32(Convert.ToString(id_v))] = true;
                                        break;
                                    }
                                }
                            }
                            node.edges.Clear();

                            string s = Convert.ToString(node.name[0]);
                            node.id = Convert.ToInt32(s);
                            for (int i = 0; i < edg.Count; i++)
                            {
                                node.edges.Add(edg[i]);
                            }
                        }

                    }
                    else
                        MessageBox.Show("Сначала выполните обход графа!!");
                }
                
                textBox2.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }


        /*
        private void topological_sort(bool[] used)
        {
            for (int i = 0; i < graph.nodes.Count;i++)
            {
                used[i] = false;
            }
            ans.Clear();
 
            foreach(Graph.Node vert in graph.nodes)
            {
                if (!used[vert.id])
                    dfs(vert.id, ref used); ;
            }
            ans.Reverse();
            string s = "";
            for (int i = 0; i < ans.Count; i++)
                s += "" + ans[i].ToString() + " ";
            textBox2.Text = s;            
        }

        /*private void dfs(int v,ref bool[]used)
        {
            used[v] = true;
            if(v<graph.nodes.Count)
            {
               
                if (v!=0)
                {
                    for (int i = graph.nodes[v - 1].edges.Count - 1; i >= 0; i--)
                    {
                        int nextv = graph.nodes[v - 1].edges[i];
                        if (!used[nextv])
                            dfs(nextv, ref used);
                    }
                }else
                {
                    for (int i = graph.nodes[v].edges.Count - 1; i >= 0; i--)
                    {
                        int nextv = graph.nodes[v].edges[i];
                        if (!used[nextv])
                            dfs(nextv, ref used);
                    }
                }
               

            }
            ans.Add(v);
        }*/

        //--------------------------Топ сортировка-------------------------------

        //Загрузка из файла----------------------------------
        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            bool noWord = true;
            graph.nodes.Clear();
            StreamReader file = File.OpenText(openFileDialog1.FileName);
            while(!file.EndOfStream)
            {
                noWord = true;
                string s = file.ReadLine();
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] >= 'a' && s[i] <= 'z')
                    {
                        noWord = false;
                        break;
                    }
                }
                if (noWord == false)
                    continue;
               
                string[] ss = s.Split(',');
                List<int> L = new List<int>();
                if(ss[3]!="")
                {
                    string[] sse = ss[3].Split(';');
                    foreach (string eg in sse)
                        L.Add(int.Parse(eg));
                }
                graph.loadgraph(id: int.Parse(ss[0]), x: int.Parse(ss[1]), y: int.Parse(ss[2]), name: ss[4], edge: L);
            }
            file.Close();
        }

        

        //Сохранение в файл
        private void Button2_Click(object sender, EventArgs e)//save
        {
            if (graph.nodes.Count != 0)
                saveFileDialog1.ShowDialog();
        }

        private void SaveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //перезапись файла
            if (File.Exists(saveFileDialog1.FileName))
                File.Delete(saveFileDialog1.FileName);

            FileStream file = File.OpenWrite(saveFileDialog1.FileName);
            
            string data = " id , x, y, v1;v2;...vn, name \n";
            byte[] info = new UTF8Encoding(true).GetBytes(data);
            file.Write(info, 0, info.Length);
            //id узел который связана с ; ; 
            // id, x, y, v1;v2..;vn,name
            foreach(Graph.Node vert in graph.nodes)
            {
                 data = "";
                data += vert.id.ToString() + ",";
                data += vert.x.ToString() + ",";
                data += vert.y.ToString() + ",";
                if(vert.edges.Count!=0)
                {
                    foreach (int edg in vert.edges)
                        data += edg.ToString()+";";
                    data = data.Remove(data.Length - 1, 1);
                }
                data += "," + vert.name + "\n";
                info = new UTF8Encoding(true).GetBytes(data);
                file.Write(info, 0, info.Length);
               
            }
            file.SetLength(file.Length - 1); // удалить последний \n
            file.Close(); 
        }
        //-----------------------------------------Save file--------------


        //Проверка на цикл-----------------------------
        private void Button5_Click(object sender, EventArgs e)
        {
            //FIXME:
            if (trackBar2.Value == 0)
            {
                trackBar2.Value = 1;
                timer3.Interval = trackBar2.Value;
            }
            else
                timer3.Interval = trackBar2.Value;
            drag = -1;
            drage = -1;
            foreach (Graph.Node v in graph.nodes)
            {
                v.active = 0;
                v.prev = -1;
                v.chk = -1;
            }
            if (timer3.Enabled == false && action_bypass == false)
            {
                if (graph.nodes.Count > 0)
                {
                    graph.nodes[0].active = 1;
                    timer3.Start();
                    action_bypass = true;
                    progressBar1.Value = 0;
                }
            }
            else
            {
                timer3.Stop();
                action_bypass = false;
            }
        }

        private void Timer3_Tick(object sender, EventArgs e)
        {
          
            timer3.Interval = trackBar2.Value; // регулировка скорости

            //DFS---------
            DFS(true);
            //------------DFS----

            //обнов полосы прогресса в зависимости от кол обработ узлов
            int k = 0;
            foreach (Graph.Node v in graph.nodes)
                if (v.active == 2) k += 1;
            progressBar1.Maximum = 0+k;
            progressBar1.PerformStep();

            if (this.cycle == false && searchCycle_complete == true)
           {
               button3.Enabled = true;
               button5.Enabled = false;
           }
           else
                if(searchCycle_complete == true)
                 MessageBox.Show("В графе есть цикл!");
           

        }//---------------------Cycle-----------------


        //Clear---------------------------------------
        private void Button7_Click(object sender, EventArgs e)
        {
            graph.nodes.Clear();
            graph.clear();
            checkBox1.Enabled = true;
            checkBox3.Enabled = false;
            button6.Enabled = true;
            button5.Enabled = true;
        }//--------------------------------------Clear----------------

    }//конец класса Form


}//конец пространства имен
