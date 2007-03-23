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
using System.Collections;
using MusicDataminer;

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
            string iDBFileName = "dbEurope.bin";

            renderer = new Renderer(this);
            doc = new GMSDocument();
            doc.ReadDB(dir + iDataPath + iDBFileName);
            doc.SetupFilteredData("countries_acronyms_europe.txt");
            doc.SetupSortedData();

            pcCountries = new ParallelPlotCountries(  doc.GetFilteredDataCube()
                                                    , doc.GetFilteredCountryNames()
                                                    , panel1
                                                    , renderer
                                                    , doc.iFilteredColorMap
                                                    , doc
                                                    );

            mapPlot = new MapPlot(doc.GetFilteredDataCube()
                                      , PCMapSplitContainer.Panel1
                                      , renderer
                                      , doc.iFilteredColorMap
                                      , pcCountries.pcPlot
                                      , doc
                                      );


            // Initialize Tree Map
            iTreeMap = new TreeMap(mainSplitContainer.Panel2);

            int quantitativeDataIndex, ordinalDataIndex, idIndex, leafNodeIndex;
            List<GMSToolTipComponent> toolTipComponents = new List<GMSToolTipComponent>();

            // Build the TreeMap Data << Countries releases per Styles >>
            object[, ,] data = doc.BuildStylesAreasTree(out quantitativeDataIndex,
                out ordinalDataIndex, out idIndex, out leafNodeIndex, toolTipComponents);

            //// Build the TreeMap Data << UnemploymentRates per Styles >>
            //object[, ,] data = doc.BuildStylesUnemploymentRateAreaTree(out quantitativeDataIndex,
            //    out ordinalDataIndex, out idIndex, out leafNodeIndex, toolTipComponents);

            iTreeMap.SetData(data, quantitativeDataIndex, ordinalDataIndex, idIndex, 
                leafNodeIndex, toolTipComponents);
            
            iTreeMap.UpdateScale();
            iTreeMap.ColorMap = doc.iFilteredColorMap;
            pcCountries.pcPlot.FilterChanged += new EventHandler(pcPlot_FilterChanged);
        }

        void pcPlot_FilterChanged(object sender, EventArgs e)
        {
            iTreeMap.Invalidate();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox combo = (ToolStripComboBox)sender;
            string clusters = (string)combo.SelectedItem;
            pcCountries.ToggleFilter(clusters);
            mainSplitContainer.Panel2.Invalidate();
            PCMapSplitContainer.Panel1.Invalidate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
