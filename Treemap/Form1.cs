using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gav.Data;

namespace Treemap
{
    public partial class Form1 : Form
    {
        private TreeMap iTreeMap;

        public Form1()
        {
            InitializeComponent();

        }

        private ColorMap CreateColorMap()
        {
            ColorMap map = new ColorMap();
            LinearHSVColorMapPart hsvMap = new LinearHSVColorMapPart(0.0f, 180.0f);
            map.AddColorMapPart(hsvMap);
            hsvMap.Invalidate();
            map.Invalidate();
            return map;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            iTreeMap = new TreeMap(treemapPanel.Width, treemapPanel.Height);
            iTreeMap.UpdateScale(treemapPanel.Width, treemapPanel.Height);
            iTreeMap.ColorMap = CreateColorMap();

        }

        private void treemapPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = treemapPanel.CreateGraphics();
            iTreeMap.DrawTree( g );
            g.Dispose();
        }

        private void treemapPanel_SizeChanged(object sender, EventArgs e)
        {
            if (iTreeMap != null)
            {
                iTreeMap.UpdateData(treemapPanel.Width, treemapPanel.Height);

                iTreeMap.UpdateScale(treemapPanel.Width, treemapPanel.Height);
                //            iTreeMap.UpdateScale(treemapPanel.Width, treemapPanel.Height);
                treemapPanel.Invalidate();
            }
        }
    }
}