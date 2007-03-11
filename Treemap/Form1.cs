using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Treemap
{
    public partial class Form1 : Form
    {
        private TreeMap iTreeMap;

        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            iTreeMap = new TreeMap();
            iTreeMap.UpdateScale(treemapPanel.Width, treemapPanel.Height);
        }

        private void treemapPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = treemapPanel.CreateGraphics();
            //iTreeMap.UpdateScale(treemapPanel.Width, treemapPanel.Height);
            iTreeMap.DrawTree( g );
        }

        private void treemapPanel_SizeChanged(object sender, EventArgs e)
        {
            iTreeMap.UpdateScale(treemapPanel.Width, treemapPanel.Height);
            treemapPanel.Invalidate();
        }
    }
}