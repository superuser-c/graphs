using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grafy {
    public partial class Form1 : Form {
        Graph graph;

        public Form1() {
            InitializeComponent();
            button1_Click(null, null);
        }

        private void button1_Click(object sender, EventArgs e) {
            if (Int32.TryParse(textBox1.Text, out int x) && Int32.TryParse(textBox4.Text, out int r)) {
                graph = Graph.Random(x, checkBox1.Checked, r);
                button2_Click(null, null);
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            graph.Render(pictureBox1.CreateGraphics(), Math.Min(pictureBox1.Width, pictureBox1.Height));
        }

        private void button3_Click(object sender, EventArgs e) {
            if (Int32.TryParse(textBox2.Text, out int i) && Int32.TryParse(textBox3.Text, out int j)) {
                Tuple<float, List<int>> pathdata = graph.FindPath(i, j);
                int[] path = pathdata.Item2?.ToArray();
                richTextBox1.Text = "";
                if (path == null) {
                    richTextBox1.Text = "404 Path not found.";
                } else {
                    foreach (var v in path) {
                        richTextBox1.Text += v + " ";
                    }
                    richTextBox1.Text += "> " + pathdata.Item1.ToString();
                    graph.Render(pictureBox1.CreateGraphics(),
                        Math.Min(pictureBox1.Width, pictureBox1.Height), path);
                }
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e) {
            button2_Click(null, null);
        }
    }
}
