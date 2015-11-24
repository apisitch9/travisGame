using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TravisGame
{
    public partial class Form1 : Form
    {
        public Game game;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            game = new Game(panel1);
            game.startGame(10, 10, 2);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
        }
    }
}
