using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GMS
{
    public partial class Form1 : Form
    {
        GMSDocument doc;

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

            doc.FillDummieCube(headers);
            doc.ShowData(headers);
        }
    }
}