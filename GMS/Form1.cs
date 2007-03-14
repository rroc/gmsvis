using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Gav.Data;
using Gav.Graphics;

namespace GMS
{
    public partial class Form1 : Form
    {
        GMSDocument doc;
        Renderer renderer;
        ParallelPlotCountries pcCountries;
        MapPlot mapPlot;
        TreeMap iTreeMap;

        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string dir = Directory.GetCurrentDirectory();
            string iDataPath = "\\..\\..\\..\\data\\";
            string iDBFileName = "db.bin";

            renderer = new Renderer(this);
            doc = new GMSDocument();
            doc.ReadDB(dir + iDataPath + iDBFileName);
            doc.SetupFilteredData("countries_acronyms_europe.txt");
            doc.SetupSortedData();

            pcCountries = new ParallelPlotCountries(  doc.GetFilteredDataCube()
                                                    , doc.GetFilteredCountryNames()
                                                    , PCMapSplitContainer.Panel2
                                                    , renderer
                                                    , doc.iFilteredColorMap
                                                    );

            //mapPlot     = new MapPlot( doc.GetFilteredDataCube()
            //                          , PCMapSplitContainer.Panel1
            //                          , renderer
            //                          , doc.iFilteredColorMap 
            //                          , pcCountries.pcPlot
            //                          );

            // Initialize Tree Map
            iTreeMap = new TreeMap(mainSplitContainer.Panel2.Width, mainSplitContainer.Panel2.Height);
            iTreeMap.UpdateScale(mainSplitContainer.Panel2.Width, mainSplitContainer.Panel2.Height);
            iTreeMap.ColorMap = doc.iFilteredColorMap;

        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox combo = (ToolStripComboBox)sender;
            string clusters = (string)combo.SelectedItem;
            pcCountries.ToggleFilter(clusters);
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = mainSplitContainer.Panel2.CreateGraphics();
            iTreeMap.DrawTree(g);
            g.Dispose();
        }

        private void splitContainer1_Panel2_SizeChanged(object sender, EventArgs e)
        {
            if (iTreeMap != null)
            {
                iTreeMap.UpdateData(mainSplitContainer.Panel2.Width, mainSplitContainer.Panel2.Height);

                iTreeMap.UpdateScale(mainSplitContainer.Panel2.Width, mainSplitContainer.Panel2.Height);
                mainSplitContainer.Panel2.Invalidate();
            }
        }
    }
}
