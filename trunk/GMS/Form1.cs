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
            
            pcCountries = new ParallelPlotCountries(doc.GetDatabase(), splitContainer1.Panel1, renderer);

            //headers.Add("Clusters");
            //KMeansFilter kMeansFilter = new KMeansFilter(3);
            //kMeansFilter.Input = dc;


            //doc.ShowData(headers, kMeansFilter);
        }
    }
}