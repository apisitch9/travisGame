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
        Panel scene;
        public Field field;
        public Size fieldSize = new Size(10,10);
        private Graphics g;
        public Game()
        {
        }
        public void startGame(int row,int col,Panel scene)
        {
            this.scene = scene;
            field = new Field(row, col);
            scene.Paint += Scene_Paint;
            scene.Click += Scene_Click;
            scene.MouseMove += Scene_MouseMove;
        }

        private void Scene_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Scene_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            int mx = me.X;
            int my = me.Y;
            int centerX = scene.Width / 2;
            int centerY = scene.Height / 2;
            Field.Hover h = field.findHover(centerX - mx, centerY - my);
            Point bLoc = field.findXY(0, 0, centerX, centerY);
            field.box[0, 0].strokeLine(g, bLoc.X, bLoc.Y, field.boxWidth, field.boxHeight,Field.Box.Direction.EAST);
        }

        private void Scene_Paint(object sender, PaintEventArgs e)
        {
            int centerX = scene.Width / 2;
            int centerY = scene.Height / 2;
            g = Graphics.FromHwnd(scene.Handle);
            g.Clear(Color.White);
            field.drawField(g, centerX, centerY);
        }

        public class Field
        {
            public Box[,] box;
            float EdgeThreshold = 0.2f;
            private int row, col;
            public int boxHeight=40, boxWidth=40;
            public class Hover
            {
                public int x, y;
                public enum HoverType
                {
                    BOX, EDGE ,NONE
                };
                public HoverType hoverType;
            }
            
            public Field(int rowCount,int columnCount) {
                row = rowCount;
                col = columnCount;
                box = new Box[rowCount,columnCount];
                for(int i = 0; i < rowCount; i++) {
                    for(int j = 0; j < columnCount; j++)
                    {
                        box[i, j] = new Box();
                    }
                }

            }
            public Hover findHover(int rx,int ry) 
            {
                Hover h = new Hover();
                int fieldWidth = boxWidth * col;
                int fieldHeight = boxHeight * row;
                rx += fieldWidth / 2;
                ry += fieldHeight / 2;
                rx %= boxWidth;
                ry %= boxHeight;
                return h;
            }
            public Point findXY(int x,int y,int cx,int cy)
            {
                int fieldWidth = boxWidth * col;
                int fieldHeight = boxHeight * row;
                return new Point(cx -(fieldWidth/2) + x * boxWidth,cy - (fieldHeight/2) + y * boxHeight);
            }
            public void drawField(Graphics g,int x,int y)
            {
                int fieldWidth = boxWidth * col;
                int fieldHeight = boxHeight * row;

                for(int i = 0; i < row; i++) {
                    for(int j=0;j< col; j++)
                    {
                        int bx = x - (fieldWidth / 2)+ j*boxWidth;
                        int by = y - (fieldHeight / 2) + i *boxHeight;
                        box[i, j].drawBox(g, bx, by,boxWidth,boxHeight);
                    }
                }
            }
            
            public class Box
            {

                bool[] drawnSide = new bool[4];
                public enum Direction
                {
                    NORTH,WEST,SOUTH,EAST
                };
                public Box() {
                      
                }
                public void strokeLine(Graphics g,int x,int y,int boxWidth,int boxHeight,Direction direction)
                {
                    Pen p = new Pen(Brushes.Blue,3.0f);
                    int sx=0, sy = 0;
                    int ex = 0, ey = 0;
                    int[] gen = {x,x+boxWidth,y,y+boxHeight };
                    MessageBox.Show(gen[0] + " " + gen[1] + " " + gen[2]+ " " +gen[3]);
                    if(direction == Direction.NORTH)
                    {
                        sx = gen[0];
                        sy = gen[2];
                        ex = gen[1];
                        ey = gen[2];
                    }
                    if(direction == Direction.EAST)
                    {
                        sx = gen[1];
                        sy = gen[2];
                        ex = gen[1];
                        ey = gen[3];
                    }
                    if(direction == Direction.SOUTH)
                    {
                        sx = gen[0];
                        sy = gen[3];
                        ex = gen[1];
                        ey = gen[3];
                    }
                    if(direction == Direction.WEST)
                    {
                        sx = gen[0];
                        sy = gen[2];
                        ex = gen[0];
                        ey = gen[3];
                    }
                    g.DrawLine(p, sx, sy, ex, ey);
                }
                public void drawBox(Graphics g,int x,int y,int boxWidth,int boxHeight)
                {
                    g.DrawRectangle(Pens.Black, new Rectangle(x,y,boxWidth,boxHeight));
                }
            }
        }
    }       

}
