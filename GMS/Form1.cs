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
        CountriesTreeMapForm iCountriesTreeMap;
        AboutBox iAboutBox;

        public Form1()
        {
            InitializeComponent();

            iAboutBox = new AboutBox();
            this.glyphCheckBoxes.SetItemChecked(1, true);
            this.glyphCheckBoxes.SetItemChecked(3, true);
            this.glyphPanel.Hide();
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
                                                    , GetGroupBox()
                                                    );

            mapPlot = new MapPlot(doc.GetFilteredDataCube()
                                      , PCMapSplitContainer.Panel1
                                      , renderer
                                      , doc.iFilteredColorMap
                                      , pcCountries.iPcPlot
                                      , doc
                                      );


            // Initialize Tree Map
            iTreeMap = new TreeMap(mainSplitContainer.Panel2, doc);

            int quantitativeDataIndex, ordinalDataIndex, idIndex, leafNodeIndex;
            List<GMSToolTipComponent> toolTipComponents = new List<GMSToolTipComponent>();

            // Build the TreeMap Data << Countries releases per Styles >>
            object[, ,] data = doc.BuildStylesAreasTree(out quantitativeDataIndex,
                out ordinalDataIndex, out idIndex, out leafNodeIndex, toolTipComponents);

            iTreeMap.SetData(data, quantitativeDataIndex, ordinalDataIndex, idIndex, 
                leafNodeIndex, toolTipComponents);
            
            iTreeMap.UpdateScale();
            iTreeMap.ColorMap = doc.iFilteredColorMap;
            pcCountries.iPcPlot.FilterChanged += new EventHandler(pcPlot_FilterChanged);

            this.glyphPanel.Show();
            this.ShowInformationBox();           
        }

        void ShowInformationBox()
        {
            iAboutBox.ShowDialog( this );
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
            iTreeMap.Invalidate();
            mapPlot.Invalidate();
        }

        private void glyphCheckBoxes_SelectedIndexChanged(object sender, EventArgs e)
        {
            mapPlot.Invalidate(); //PCMapSplitContainer.Panel1.Invalidate();
        }

        private void helpButtonClicked(object sender, EventArgs e)
        {
            ShowInformationBox();
        }

        private void treeMapComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox combo = (ToolStripComboBox)sender;
            string treeMapType = (string)combo.SelectedItem;

            if (combo.SelectedIndex != 0 && 
                (iCountriesTreeMap == null || 
                iCountriesTreeMap.IsDisposed))
            {
                iCountriesTreeMap = new CountriesTreeMapForm(doc, treeMapType, combo, doc.iFilteredColorMap);
                iCountriesTreeMap.Show();
            }
        }

        public System.Windows.Forms.ToolStripComboBox GetGroupBox()
        {
            return groupingComboBox;
        }

    }
}
