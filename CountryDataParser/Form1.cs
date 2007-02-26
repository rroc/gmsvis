using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace CountryDataParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string projectFolder = Directory.GetCurrentDirectory();
            openFileDialog1.InitialDirectory = @projectFolder + "\\data\\geodata";
            openFileDialog1.Title = "Select any number of files to be parsed";
            openFileDialog1.Filter = "Countries Files|*.txt";
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                Parser parser = new Parser();

                string[] filenames = openFileDialog1.FileNames;

                foreach (string filename in filenames)
                {
                    parser.Parse(filename);
                }

                MessageBox.Show("Finished parsing files");
            }
        }
    }
}