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
        public Form1()
        {
            InitializeComponent();

            string dir = Directory.GetCurrentDirectory();
            string iDataPath              = "\\..\\..\\..\\data\\";
            string iDBFileName            = "db.bin";
            GMSDocument doc = new GMSDocument();
            doc.ReadDB(dir + iDataPath + iDBFileName);
            
        }
    }
}