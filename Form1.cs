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

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            Form2 a = new Form2();
            a.Show();
            a.setHostForm(this);
            game = new Game();
            game.startGame(10, 10, panel1);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Visible = false;

        }
    }
}
