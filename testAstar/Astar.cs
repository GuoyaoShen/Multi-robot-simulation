using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testAstar
{
    public class AstarNode  //【类】定义点，查看属性
    {
        private AstarNode parent = null;
        private double g;
        private double h;
        private int x;
        private int y;

        public AstarNode Parent  //属性Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        public double G  //属性G
        {
            get
            {
                return g;
            }
            set
            {
                g = value;
            }
        }

        public double H  //属性H
        {
            get
            {
                return h;
            }
            set
            {
                h = value;
            }
        }

        public double F  //属性F
        {
            get
            {
                return g + h;
            }
        }

        public int X  //属性X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public int Y  //属性Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public AstarNode(int _x, int _y)  //自定义构造函数
        {
            this.x = _x;
            this.y = _y;
            this.parent = null;
            this.g = 0;
            this.h = 0;
        }
    }

    public class Astar  //【类】算法
    {
        private List<AstarNode> openList = new List<AstarNode>();
        private List<AstarNode> closeList = new List<AstarNode>();
        private int[,] mapData;
        private int pixelFormat = 16;
        private int mapWidth = 0;
        private int mapHeight = 0;
        private int endX = 0;
        private int endY = 0;

        public int[,] MapData  //【属性】MapData
        {
            get
            {
                return mapData;
            }
        }

        public int PixelFormat  //【属性】PixelFormat
        {
            get
            {
                return pixelFormat;
            }
        }

        public int MapWidth  //【属性】MapWidth
        {
            get
            {
                return mapWidth;
            }
        }

        public int MapHeight  //【属性】MapHeight
        {
            get
            {
                return mapHeight;
            }
        }

        public Astar()  //构造函数
        {
        }

        private bool isValid(int x, int y)  //【方法】判断点是否在地图中
        {
            if (x < 0 || x >= mapWidth)
            {
                return false;
            }

            if (y < 0 || y >= mapHeight)
            {
                return false;
            }

            return true;
        }

        private bool inList(List<AstarNode> list, int x, int y)  //【方法】判断点是否在列表中
        {
            foreach (AstarNode node in list)
            {
                if (node.X == x && node.Y == y)
                {
                    return true;
                }
            }

            return false;
        }

        private bool inOpenList(int x, int y)  //【方法】判断点是否在开启列表中
        {
            return inList(openList, x, y);
        }

        private bool inCloseList(int x, int y)  //【方法】判断点是否在关闭列表中
        {
            return inList(closeList, x, y);
        }

        private AstarNode getBestNodeFromOpenList()  //【方法】 取最优点(开启列表第一个元素)
        {
            if (openList.Count == 0)
            {
                return null;
            }

            return openList[0];  //取开启列表第一个元素
        }

        private void openToClose(AstarNode node)  //【方法】 开启列表 转到 关闭列表
        {
            openList.Remove(node);
            closeList.Add(node);
        }

        private AstarNode openToCloseWithBest()  //【方法】 将最优点由 开启列表 转到 关闭列表
        {
            AstarNode node = getBestNodeFromOpenList();

            if (node == null)
            {
                return null;
            }

            openToClose(node);
            return node;
        }

        private void addToOpenList(AstarNode parent, int x, int y)  //【方法】 将点加入开启列表（以parent为父节点在开启列表中加入点（x，y））【包含估价函数】
        {
            int absX = Math.Abs(endX - x);
            int absY = Math.Abs(endY - y);

            if (!isValid(x, y))  //是否在地图中
            {
                return;
            }

            if (inOpenList(x, y) || inCloseList(x, y))  //是否在列表中
            {
                return;
            }

            if (!canWalk(x, y) && parent != null)  //是否可走或有父节点
            {
                return;
            }

            AstarNode node = new AstarNode(x, y);
            node.Parent = parent;

            if (parent == null)
            {
                node.G = 0;
                node.H = 0;
            }
            else
            {
                if (Math.Abs(parent.X - x) + Math.Abs(parent.Y - y) == 2)
                {
                    node.G = 14;
                }
                else
                {
                    node.G = 10;
                }

                node.H = 10*((Math.Abs(endX - x)  + Math.Abs(endY - y)) +  (1.414 - 2)*Math.Min(absX, absY));  //预估H：切比雪夫距离
            }

            openList.Add(node);
            openList.Sort(delegate(AstarNode lhs, AstarNode rhs)  //方法 按 lhs比rhs大的 升序排列
            {
                if (lhs.F < rhs.F)
                {
                    return -1;
                }
                else if (lhs.F > rhs.F)
                {
                    return 1;
                }
                return 0;
            });
        }

        private void genAroundNode(AstarNode node)  //【方法】 加入相邻点，八点以及直角障碍判别
        {
            int x = node.X;
            int y = node.Y;
            addToOpenList(node, node.X - 1, node.Y);

            addToOpenList(node, node.X, node.Y - 1);
            addToOpenList(node, node.X, node.Y + 1);

            addToOpenList(node, node.X + 1, node.Y);
            
            
            if (!canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1))  //1
            {
                addToOpenList(node, node.X + 1, node.Y - 1);
                addToOpenList(node, node.X + 1, node.Y + 1);
            }
            else if (canWalk(x - 1, y) && !canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1))  //2
            {
                addToOpenList(node, node.X - 1, node.Y - 1);
                addToOpenList(node, node.X + 1, node.Y - 1);
            }
            else if (canWalk(x - 1, y) && canWalk(x, y + 1) && !canWalk(x + 1, y) && canWalk(x, y - 1))  //3
            {
                addToOpenList(node, node.X - 1, node.Y - 1);
                addToOpenList(node, node.X - 1, node.Y + 1);
            }
            else if (canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && !canWalk(x, y - 1))  //4
            {
                addToOpenList(node, node.X - 1, node.Y + 1);
                addToOpenList(node, node.X + 1, node.Y + 1);
            }
            else if (!canWalk(x - 1, y) && !canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1))  //5
            {
                addToOpenList(node, node.X + 1, node.Y - 1);
            }
            else if (canWalk(x - 1, y) && !canWalk(x, y + 1) && canWalk(x + 1, y) && !canWalk(x, y - 1))  //6
            {

            }
            else if (canWalk(x - 1, y) && !canWalk(x, y + 1) && !canWalk(x + 1, y) && canWalk(x, y - 1))  //7
            {
                addToOpenList(node, node.X - 1, node.Y - 1);
            }
            else if (!canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && !canWalk(x, y - 1))  //8
            {
                addToOpenList(node, node.X + 1, node.Y + 1);
            }
            else if (!canWalk(x - 1, y) && canWalk(x, y + 1) && !canWalk(x + 1, y) && canWalk(x, y - 1))  //9
            {

            }
            else if (canWalk(x - 1, y) && canWalk(x, y + 1) && !canWalk(x + 1, y) && !canWalk(x, y - 1))  //10
            {
                addToOpenList(node, node.X - 1, node.Y + 1);
            }
            else if (!canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1)) { }
            else if (!canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1)) { }
            else if (!canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1)) { }
            else if (!canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1)) { }
            else if (!canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1)) { }  //15  全是障碍
            else  //16  没有障碍
            {
                addToOpenList(node, node.X - 1, node.Y - 1);
                addToOpenList(node, node.X - 1, node.Y + 1);
                addToOpenList(node, node.X + 1, node.Y - 1);
                addToOpenList(node, node.X + 1, node.Y + 1);
            }
            //if (!canWalk(x - 1, y) && canWalk(x, y + 1) && canWalk(x + 1, y) && canWalk(x, y - 1)) { }
            
            

        }

        private AstarNode findNearPointFromList(List<AstarNode> list, int x, int y)  //【方法】找列表中到点（x，y）之切比雪夫距离最小点
        {
            
            AstarNode result = null;
            double minDistance = int.MaxValue;  //MaxValue 一个特大值

            foreach (AstarNode node in list)
            {
                //【下面距离公式用作开启列表节点筛选的引导】
                double dist = node.F;

                if (dist < minDistance)
                {
                    minDistance = dist;
                    result = node;
                }
            }

            return result;
        }

        public bool canWalk(int x, int y)  //【方法】判断点(x,y)是否可走
        {
            if (mapData[x, y] < -1)  //路为-2。判断为路，则可用
                return true;
            else  //其他，为固定障碍或机器人
                return false;
        }

        public bool canWalkPixel(int x, int y)  //【方法】判断像素点(x,y)是否可走
        {
            int px = x / pixelFormat;
            int py = y / pixelFormat;

            return canWalk(px, py);
        }

        public List<AstarNode> findPath(int _startX, int _startY, int _endX, int _endY)  //【方法】找路
        {
            this.endX = _endX;
            this.endY = _endY;
            this.openList.Clear();
            this.closeList.Clear();
            List<AstarNode> result = new List<AstarNode>();
            AstarNode currNode = null;
            bool findPathFlag = false;


            if (canWalk(endX, endY))  //添加。 防止因终点不可用造成崩溃
                                      //终点可用
            {
                addToOpenList(null, _startX, _startY);  //此时Parent==null

                while (openList.Count > 0)
                {
                    currNode = openToCloseWithBest();  //将最优点(开启列表第一个)由开启列表转到关闭列表

                    if (currNode == null)
                    {
                        break;
                    }

                    if (currNode.X == _endX && currNode.Y == _endY)
                    {
                        findPathFlag = true;
                        break;
                    }

                    genAroundNode(currNode);
                }

                if (!findPathFlag)
                {
                    currNode = findNearPointFromList(closeList, endX, endY);  //此为欧几里得距离判断，减少不必要点的判断
                }

                if (currNode == null)
                {
                    return null;
                }

                while (true)
                {
                    result.Add(currNode);

                    if (currNode.X == _startX && currNode.Y == _startY)
                    {
                        break;
                    }

                    currNode = currNode.Parent;
                }

                result.Reverse();

                return result;
            }
            else  //终点不可用
            {
                result = null;  //停止循迹
                return result;
            }

        }

        public List<AstarNode> findPathPixel(int startX, int startY, int endX, int endY)  //【方法】找路 像素
        {
            int sx = startX / pixelFormat;
            int sy = startY / pixelFormat;
            int ex = endX / pixelFormat;
            int ey = endY / pixelFormat;

            List<AstarNode> result = findPath(sx, sy, ex, ey);

            if (result == null)
            {
                return null;
            }

            for (int i = 0; i < result.Count; ++i)
            {
                result[i].X *= pixelFormat;
                result[i].Y *= pixelFormat;
            }

            return result;
        }

        public void enableMapData(int x, int y, int value)  //【方法】使能点(x,y)
        {
            mapData[x, y] = value;
        }

        public void enableMapDataPixel(int x, int y, int value)  //【方法】使能像素点(x,y)
        {
            int px = x / pixelFormat;
            int py = y / pixelFormat;

            enableMapData(px, py, value);
        }

        public void enableMapDataAll(int value)  //【方法】使能全地图点(x,y)
        {
            for (int w = 0; w < mapWidth; ++w)
            {
                for (int h = 0; h < mapHeight; ++h)
                {
                    mapData[w, h] = value;
                }
            }
        }

        public void initMapData(int _widthPixel, int _heightPixel, int _pixelFormat)  //【方法】初始化地图（全图可走）
        {
            int width = _widthPixel / _pixelFormat;
            int height = _heightPixel / _pixelFormat;

            pixelFormat = _pixelFormat;
            mapData = new int[width, height];
            mapWidth = width;
            mapHeight = height;

            enableMapDataAll(-2);  //全部初始化为可走点
        }

        public int checkAround(int x, int y)  //【方法】新添加方法。用于检查[x, y]周围八个点的情况。
        {
            int result;  //用于存放周围八个点的最大值
            result = -3;
            int cx = x / pixelFormat;
            int cy = y / pixelFormat;
            for (int i = cy - 1;i <= cy + 1; i++)
            {
                if (mapData[cx - 1, i] > result)
                   result = mapData[cx - 1, i];
            }
            for (int i = cy - 1; i <= cy + 1; i+=2)
            {
                if (mapData[cx, i] > result)
                   result = mapData[cx, i];
            }
            for (int i = cy - 1; i <= cy + 1; i ++)
            {
                if (mapData[cx + 1, i] > result)
                   result = mapData[cx + 1, i];
            }
            if (result <= 0)  //为路或者固定障碍或者闲置机器人
                return 0;
            else if ((result > 0) && (result < mapData[cx, cy]))  //有机器人但是编号比自己小（自己先走）
                return -1;
            else  //有机器人且编号比自己大（对方先走）
                return 1;
        }
    }
}
