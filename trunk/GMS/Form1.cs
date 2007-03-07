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

            //pcCountries = new ParallelPlotCountries(doc.GetSortedDataCube(), doc.GetSortedCountryNames(), pcSplitContainer.Panel1, renderer, doc.iSortedColorMap);
            pcCountries = new ParallelPlotCountries(doc.GetFilteredDataCube(), doc.GetFilteredCountryNames(), pcSplitContainer.Panel1, renderer, doc.iFilteredColorMap);
            mapPlot = new MapPlot(doc.GetFilteredDataCube(), mainSplitContainer.Panel1, renderer, doc.iFilteredColorMap );

        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox combo = (ToolStripComboBox)sender;
            string clusters = (string)combo.SelectedItem;
            pcCountries.ToggleFilter(clusters);
        }
    }
}