using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testAstar
{
    public class Player  //【类】定义对单个机器人的一类寻路操作流程
    {
        public int ID;
        public int X;  //X,Y表示Player当前位置坐标
        public int Y;
        public int TargetX;
        public int TargetY;
        public List<AstarNode> Paths;
        public int PathIndex;  //指示 List<AstarNode> Paths 中元素编号（index“指标”）
        public Color PathColor;
        public Color ShowColor;

        public void delete(Astar astar)  //【方法】删除机器人后使能当前点
        {
            astar.enableMapDataPixel(X, Y, -2);
        }

        public void update(Astar astar)  //【方法】寻路及再次自更新
        {
            if (Paths != null)  //有路
            {
                if (PathIndex < Paths.Count)  //如果没走完。（PathIndex初始为0）
                {
                    if (astar.checkAround(X, Y) <= 0)  //周围无机器人（0），或者有编号小于自己的机器人（-1），则自己正常走if(astar.checkAround(X, Y) <= 0)
                    {
                        int tx = Paths[PathIndex].X;
                        int ty = Paths[PathIndex].Y;
                        
                        if (astar.canWalkPixel(tx, ty))  //判断下一步点为可走
                        {
                            astar.enableMapDataPixel(X, Y, -2);  //走离当前点，当前点置为可用
                            X = tx;  //走到下一点
                            Y = ty;
                            astar.enableMapDataPixel(X, Y, ID);  //新走到的点置为不可用.编号为自己的编号
                            PathIndex++;

                            if(astar.canWalkPixel(TargetX, TargetY))  //只在终点可用时进行动态刷新，防止因重点不可用带来的崩溃。
                            {
                                Paths = astar.findPathPixel(X, Y, TargetX, TargetY); //【动态规划】每一步都重新刷新轨迹。
                                PathIndex = 1;
                            }
                        }
                        else  //无可走点则刷新
                        {
                            Paths = astar.findPathPixel(X, Y, TargetX, TargetY);
                            PathIndex = 1;
                        }
                    }
                    else  //周围有编号比自己大的机器人，则暂停行动，且重新刷新路径
                    {
                        Paths = astar.findPathPixel(X, Y, TargetX, TargetY);
                        PathIndex = 1;
                    }
                }
                else  //列表计数指标到了（当前目标全走完了）则刷新
                {
                    astar.enableMapDataPixel(X, Y, -1);  //原为true
                    Paths = null;
                    PathIndex = 0;
                }
            }
            else  //已经停止行进(Paths==null)
            {
                astar.enableMapDataPixel(X, Y, -1);  //机器人静止则赋予当前位置优先级最低编号，保证其他优先级低于当前位置的机器人能正常在周围移动
            }
        }

        public void render(Astar astar, Graphics g)  //【方法】路径着色及机器人编号标记
        {
            if (Paths != null)
            {
                for (int i = PathIndex; i < Paths.Count; ++i)
                {
                    g.FillRectangle(new SolidBrush(PathColor), new Rectangle(Paths[i].X, Paths[i].Y, astar.PixelFormat, astar.PixelFormat));  //路径着色
                }
            }

            g.FillRectangle(new SolidBrush(ShowColor), new Rectangle(X, Y, astar.PixelFormat, astar.PixelFormat));  //当前位置(X,Y)着色

            g.DrawString(ID.ToString(), new Font("楷体", 14, FontStyle.Bold), Brushes.Black, X, Y);
        }
    }

    public partial class Form1 : Form  //【类】GUI
    {
        public static int MapWidth = 640;  //原640
        public static int MapHeight = 480;  //原480

        public static Random rand = new Random((int)DateTime.Now.Ticks);

        private Astar astar = new Astar();
        private Bitmap surface = null;
        private Graphics g = null;

        private int NewTargetX;
        private int NewTargetY;

        private List<Player> players = new List<Player>();

        private bool[] keys = new bool[256];

        private void init()  //【方法】 初始化，更改方块大小
        {
            pictureBox1.Location = Point.Empty;  //一种GDI控件pictureBox
            pictureBox1.ClientSize = new System.Drawing.Size(MapWidth, MapHeight);

            surface = new Bitmap(MapWidth, MapHeight);
            g = Graphics.FromImage(surface);

            astar.initMapData(MapWidth, MapHeight, 16);  //此处修改最后一个参数pixelFormat(原：16)可修改方块大小

            for (int i = 0; i < keys.Length; ++i)
            {
                keys[i] = false;
            }
        }

        private void update()  //【方法】全机器人更新
        {
            foreach (Player p in players)
            {
                p.update(astar);
            }
        }

        private void render()  //【方法】着色
        {
            g.Clear(Color.White);

            int[,] mapData = astar.MapData;

            for (int w = 0; w < astar.MapWidth; ++w)  //障碍着色
            {
                for (int h = 0; h < astar.MapHeight; ++h)
                {
                    if (mapData[w, h]==0)
                    {
                        g.FillRectangle(Brushes.Black, new Rectangle(w * astar.PixelFormat, h * astar.PixelFormat, astar.PixelFormat, astar.PixelFormat));
                    }
                }
            }

            foreach (Player p in players)  //机器人着色
            {
                p.render(astar, g);
            }

            pictureBox1.Image = surface;
        }

        public Form1()  //【方法】初始化GUI界面
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)  //【方法】窗口标签设置，更改行进速度
        {
            this.Text = "多机器人协作演示 A:增加 D:减少 左键:障碍设置 右键+数字键:对应编号物体的寻路 右键+0键:指定任务分配坐标点";
            init();

            Timer gameTimer = new Timer();  //新建计时器gameTimer
            gameTimer.Tick += gameTimer_Tick;
            gameTimer.Interval = 500;  //改此数值可以改变行进速度（即刷新速度）
            gameTimer.Start();
        }

        void gameTimer_Tick(object sender, EventArgs e)  //【方法】刷新事件，间隔gameTimer.Interval = 100（ms），见168行
        {
            update();
            render();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)  //【方法】鼠标
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)  //鼠标左键
            {
                astar.enableMapDataPixel(e.X, e.Y, 0);  //设置为障碍
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)  //鼠标右键
            {
                /*endX = e.X;
                endY = e.Y;
                paths = astar.findPathPixel(px, py, endX, endY);
                pathIndex = 0;*/

                int pi = 0;
                for (int i = 0; i < 256; ++i)
                {
                    if (keys[i])  //keys布尔一维数组
                    {
                        pi = i - (int)Keys.D1;

                        if (pi < 0 || pi >= players.Count)
                        {
                            if(pi == -1)  //0键+鼠标右键指定任务分配坐标点
                            {
                                NewTargetX = e.X;
                                NewTargetY = e.Y;

                                int step = 999;
                                int stepEstimate = 999;  //用作路径步数计量。
                                int id = 0;
                                int j;
                                for ( j = 0; j < players.Count; j++)  //检索所有机器人
                                {
                                    if (astar.MapData[(players[j].X) / astar.PixelFormat, (players[j].Y) / astar.PixelFormat] == -1)  //发现闲置机器人
                                    {
                                        stepEstimate = astar.findPathPixel(players[j].X, players[j].Y, NewTargetX, NewTargetY).Count;  //以到达的步数为调用判据。
                                        if(stepEstimate < step)
                                        {
                                            step = stepEstimate;
                                            id = j;
                                        }
                                    }
                                }
                                players[id].TargetX = NewTargetX;
                                players[id].TargetY = NewTargetY;
                                players[id].Paths = astar.findPathPixel(players[id].X, players[id].Y, players[id].TargetX, players[id].TargetY);
                                players[id].PathIndex = 1;
                            }
                            return;
                        }

                        else
                        {
                            Player p = players[pi];

                            p.TargetX = e.X;
                            p.TargetY = e.Y;
                            p.Paths = astar.findPathPixel(players[pi].X, players[pi].Y, e.X, e.Y);
                            p.PathIndex = 1;  //原为0

                            players[pi] = p;

                            return;
                        }
                    }
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)  //【方法】键盘，更改初始位置
        {
            keys[e.KeyValue] = true;

            if (e.KeyCode == Keys.A)
            {
                Player p = new Player();
                p.ID = players.Count + 1;
                p.X = astar.PixelFormat * players.Count + 64;  //此处更改机器人初始位置（16： pixelFormat）
                p.Y = 64;
                p.TargetX = 0;
                p.TargetY = 0;
                p.Paths = null;
                p.PathIndex = 0;
                p.ShowColor = Color.FromArgb(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
                p.PathColor = Color.FromArgb(64, p.ShowColor);

                players.Add(p);  //在玩家列表中添加新建玩家
            }

            if (e.KeyCode == Keys.D)
            {
                if (players.Count > 0)
                {
                    players[players.Count - 1].delete(astar);  //调用方法使能当前点
                    players.RemoveAt(players.Count - 1);  //RemoveAt删除第(players.Count - 1)项
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)  //【方法】抬键无效
        {
            keys[e.KeyValue] = false;
        }
    }
}
