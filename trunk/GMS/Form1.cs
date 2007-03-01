using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Gav.Data;

namespace GMS
{
    public partial class Form1 : Form
    {
        GMSDocument doc;
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

            doc = new GMSDocument(splitContainer1.Panel1, this);
            doc.ReadDB(dir + iDataPath + iDBFileName);
            
            List<string> headers = new List<string>();
            headers.Add("Country");
            headers.Add("Median Age");
            headers.Add("Number of Releases");
            headers.Add("Unemployment Rate");
            headers.Add("GDB Per Capita");

            //pcCountries = new ParallelPlotCountries(doc, splitContainer1.Panel1, )

//            doc.FillDummieCube(headers);
            DataCube dc = doc.CreateDataCube();

            headers.Add("Clusters");
            KMeansFilter kMeansFilter = new KMeansFilter(4);
            kMeansFilter.Input = dc;


            doc.ShowData(headers, kMeansFilter);
        }
    }
}