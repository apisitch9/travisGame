using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TravisGame
{

    public class Game
    {
        Random rand;
        Panel scene;
        Panel hostScene;
        public Field field;
        public int centerX, centerY;
        public int offsetX = 100, offsetY = 15;
        private Graphics g;
        private Bitmap buff;
        private NumericUpDown rowSpinner, colSpinner;
        private ScrollBar vScroll;
        private PictureBox pic;
        private Button startBtn;
        private int player;
        public int turn = 0;//0 first player : 1 for other player and on and on
        int[] point;
        Color[] playerColor;
        public Game(Panel hostScene)
        {
            rand = new Random();
            this.hostScene = hostScene;
            scene = new Panel();
            scene.Paint += Scene_Paint;
            scene.Click += Scene_Click;
            scene.MouseMove += Scene_MouseMove;
            scene.Resize += Scene_Resize;
            hostScene.Dock = DockStyle.Fill;
            hostScene.Controls.Add(scene);
            setPanelControl();
            buff = new Bitmap(scene.Size.Width, scene.Size.Height);
        }
        public void setPanelControl()
        {
            scene.Controls.Clear();
            rowSpinner = new NumericUpDown();
            colSpinner = new NumericUpDown();
            colSpinner.Minimum = rowSpinner.Minimum = 5;
            colSpinner.Maximum = rowSpinner.Maximum = 32;
            scene.Controls.Add(rowSpinner);
            scene.Controls.Add(colSpinner);
            scene.Controls.Add(pic = new PictureBox());
            scene.Controls.Add(startBtn = new Button());
            pic.Image = TravisGame.Properties.Resources.TravisGame;
            pic.SizeMode = PictureBoxSizeMode.StretchImage;
            startBtn.Click += restartClick;
            hostScene.AutoScroll = true;
        }

        private void restartClick(object sender, EventArgs e)
        {
            startGame((int)rowSpinner.Value, (int)colSpinner.Value, 2);
        }

        public enum Direction
        {
            NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3
        };
        public void startGame(int row, int col, int player)
        {
            this.player = player;
            field = new Field(row, col);
            point = new int[player];
            center();
            playerColor = new Color[player];
            for (int i = 0; i < player; ++i)
                playerColor[i] = Color.FromArgb(rand.Next() ^ ~playerColor[i > 0 ? i - 1 : 0].ToArgb() | (0xFF << 24) | (0x080808));
            scene.Invalidate();
        }
        private void center()
        {
            centerX = scene.Width / 2 + offsetX;
            centerY = scene.Height / 2 + offsetY;
        }
        private void Scene_Resize(object sender, EventArgs e)
        {
            buff = new Bitmap(scene.Size.Width, scene.Size.Height);
            center();
            redraw_buff();
            redraw();
        }

        private void Scene_MouseMove(object sender, MouseEventArgs e)
        {
            if (g == null) return;
            redraw_buff();
            MouseEventArgs me = (MouseEventArgs)e;
            int mx = me.X;
            int my = me.Y;
            Field.Hover h = field.findHover(mx - centerX, my - centerY);
            if (h.hoverType == Field.Hover.HoverType.EDGE)
            {
                strokeLine(new Pen(Color.DarkGray, 4.0f), h.x, h.y, h.direction);
                redraw();
            }
            else if (h.hoverType == Field.Hover.HoverType.BOX)
            {
                redraw();
            }
            else redraw();
        }

        private void Scene_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            int mx = me.X;
            int my = me.Y;
            Field.Hover h = field.findHover(mx - centerX, my - centerY);
            if (h.hoverType == Field.Hover.HoverType.EDGE)
            {
                if (field.getEdgeOwner(h) != 0) return;
                field.setEdgeOwner(h, turn + 1);
                List<Point> nc = field.checkBox();
                if (nc.Count > 0)
                {
                    point[turn]++;
                    if (field.isEnd())
                    {
                        MessageBox.Show("Game ended");
                    }
                }
                else
                changeTurn();
            }
        }
        public void changeTurn()
        {
            turn++;
            turn %= player;
        }
        private void Scene_Paint(object sender, PaintEventArgs e)
        {
            redraw_buff();
            redraw();
        }
        private void redraw_buff()
        {
            if (field == null) return;
            g = Graphics.FromImage(buff);
            g.Clear(Color.DimGray);
            drawField();
            drawControl();
            drawStatus();
        }
        private void redraw()
        {
            scene.CreateGraphics().DrawImage(buff, new Point(0, 0));
        }
        public void drawField()
        {

            int n = field.getRow(), m = field.getCol();
            int sx = centerX - field.getFieldWidth() / 2;
            int sy = centerY - field.getFieldHeight() / 2;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    foreach (Direction d in Enum.GetValues(typeof(Direction)))
                    {
                        int owner = field.getEdgeOwner(j, i, d);
                        strokeLine(owner == 0 ? Color.SteelBlue: playerColor[owner - 1], j, i, d);
                    }
                }
            }
            scene.MinimumSize = hostScene.Size;
            scene.Size= new Size(field.getFieldWidth() + offsetX * 2, field.getFieldHeight() + offsetY * 2);
        }
        public Point[] findLine(int x, int y, Direction direction)
        {
            Point[] line = new Point[2];
            x = centerX - field.getFieldWidth() / 2 + x * field.boxWidth;
            y = centerY - field.getFieldHeight() / 2 + y * field.boxHeight;
            int[] gen = { x, x + field.boxWidth, y, y + field.boxHeight };
            int[,] indices =
            {
                        {0,2,1,2 },
                        {1,2,1,3 },
                        {0,3,1,3 },
                        {0,2,0,3 }
                    };
            int d = (int)direction;
            line[0].X = gen[indices[d, 0]];
            line[0].Y = gen[indices[d, 1]];
            line[1].X = gen[indices[d, 2]];
            line[1].Y = gen[indices[d, 3]];
            return line;
        }
        public void strokeLine(Color col, int x, int y, Direction direction)
        {
            strokeLine(new Pen(col, 3.0f), x, y, direction);
        }
        public void strokeLine(Pen p, int x, int y, Direction direction)
        {
            Point[] line = findLine(x, y, direction);
            g.DrawLine(p, line[0], line[1]);
        }
        public void drawStatus()
        {
            int sx = centerX - field.getFieldWidth() / 2, sy = centerY - field.getFieldHeight() / 2 - offsetY*2;
            String statusString = "Turn : Player " + (turn + 1) + " Score [" + point[0];
            for(int i=1;i< player; i++)
                statusString += ":" + point[i];
            statusString += "]";
            g.DrawString(statusString,new Font("Helvetica", 15.0f), Brushes.WhiteSmoke, new Point(sx , sy));
        }
        public void drawControl()
        {
            int statusWidth = offsetX * 2;
            int padding = 30;
            int rowHeight = 30;
            int sx = centerX - field.getFieldWidth() / 2 - statusWidth, sy = centerY - field.getFieldHeight() / 2;
            pic.Left = sx + padding;
            pic.Top = sy;
            pic.Width = statusWidth - padding * 2;
            pic.Height = pic.Width;
            sy += pic.Height+20;
            g.DrawString("Columns", new Font("Helvetica", 15.0f),Brushes.WhiteSmoke, new Point(sx+padding, sy));
            sy += rowHeight;
            colSpinner.Top = sy;
            colSpinner.Left = sx + padding;
            colSpinner.Width = statusWidth - padding * 2;
            sy += rowHeight;
            g.DrawString("Rows", new Font("Helvetica", 15.0f),Brushes.WhiteSmoke, new Point(sx+padding, sy));
            sy += rowHeight;
            rowSpinner.Top = sy;
            rowSpinner.Left = sx + padding;
            rowSpinner.Width = statusWidth - padding * 2;
            sy += rowHeight*2;
            startBtn.Left = sx + padding;
            startBtn.Width = statusWidth - padding * 2;
            startBtn.Top = sy;
            startBtn.Text = "Restart";
        }
        public class Field
        {
            public int[,] edgeStroke_hor;
            public int[,] edgeStroke_ver;
            public int[,] conqueredBox;
            float EdgeThreshold = 0.4f;
            private int row, col;
            private int conqueredCount = 0;
            public int boxHeight = 40, boxWidth = 40;
            public int getRow()
            {
                return row;
            }
            public int getCol()
            {
                return col;
            }
            public int getFieldWidth()
            {
                return boxWidth * col;
            }
            public int getFieldHeight()
            {
                return boxHeight * row;
            }
            public int getEdgeOwner(Hover h)
            {
                return getEdgeOwner(h.x, h.y, h.direction);
            }
            public int getEdgeOwner(int x, int y, Direction direction)
            {
                if (direction == Direction.WEST || direction == Direction.EAST)
                    return edgeStroke_ver[y, direction == Direction.WEST ? x : x + 1];
                else
                    return edgeStroke_hor[direction == Direction.NORTH ? y : y + 1, x];
            }
            public void setEdgeOwner(Hover h, int owner)
            {
                setEdgeOwner(h.x, h.y, h.direction, owner);
            }
            public void setEdgeOwner(int x, int y, Direction direction, int owner)
            {
                if (direction == Direction.WEST || direction == Direction.EAST)
                    edgeStroke_ver[y, direction == Direction.WEST ? x : x + 1] = owner;
                else
                    edgeStroke_hor[direction == Direction.NORTH ? y : y + 1, x] = owner;
            }
            public bool isEnd()
            {
                return conqueredCount == row * col;
            }
            public List<Point> checkBox()
            {
                List<Point> newConquered = new List<Point>();
                for(int i=0;i< row; i++) {
                    for(int j=0;j< col; j++)
                    {
                        if(isBoxed(j,i)&&conqueredBox[j,i] == 0)
                        {
                            newConquered.Add(new Point(j, i));
                            conqueredBox[j, i] = 1;
                            conqueredCount++;
                        }
                    }
                }
                return newConquered;
            }
            public bool isBoxed(int x,int y)
            {
                return edgeStroke_hor[y, x] > 0 && edgeStroke_hor[y + 1, x] > 0 &&
                    edgeStroke_ver[y, x] > 0 && edgeStroke_ver[y, x + 1] > 0;
            }
            public class Hover
            {
                public int x, y;
                public Direction direction;
                public enum HoverType
                {
                    BOX, EDGE, NONE
                };
                public HoverType hoverType;
            }

            public Field(int rowCount, int columnCount)
            {
                row = rowCount;
                col = columnCount;
                edgeStroke_hor = new int[row + 1, col];
                edgeStroke_ver = new int[row, col + 1];
                conqueredBox = new int[row, col];
            }
            public Hover findHover(int rx, int ry)
            {
                int fieldWidth = getFieldWidth();
                int fieldHeight = getFieldHeight();
                rx += fieldWidth / 2;
                ry += fieldHeight / 2;
                Hover h = new Hover();
                int edgeWidth = (int)(EdgeThreshold * boxWidth);
                int edgeHeight = (int)(EdgeThreshold * boxHeight);
                int dx = rx + edgeWidth / 2, dy = ry + edgeHeight / 2;
                if (dx < 0 || dx > fieldWidth + edgeWidth
                    || dy < 0 || dy > fieldHeight + edgeHeight)
                {
                    h.hoverType = Hover.HoverType.NONE;
                    return h;
                }
                dx %= boxWidth;
                dy %= boxHeight;
                if (Math.Abs(dx) > edgeWidth && Math.Abs(dy) > edgeHeight)
                {
                    h.hoverType = Hover.HoverType.BOX;
                    h.x = rx / boxWidth;
                    h.y = ry / boxHeight;
                    return h;
                }
                h.hoverType = Hover.HoverType.EDGE;
                h.x = (rx / boxWidth);
                h.y = (ry / boxHeight);
                dx = (rx) % boxWidth;
                dy = (ry) % boxHeight;
                dy = (int)(dy * boxWidth * 1.0f / boxHeight);
                dx -= boxWidth / 2;
                dy -= boxWidth / 2;
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    h.direction = dx < 0 ? Direction.WEST : Direction.EAST;
                    if (rx >= fieldWidth)
                        h.direction = Direction.EAST;
                }
                else
                {
                    h.direction = dy < 0 ? Direction.NORTH : Direction.SOUTH;
                    if (ry >= fieldHeight)
                        h.direction = Direction.SOUTH;
                }
                if (h.x >= col)
                    h.x--;
                if (h.y >= row)
                    h.y--;
                return h;
            }
        }
    }

}
