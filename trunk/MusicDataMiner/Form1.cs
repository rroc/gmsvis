using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using musicbrainz;

namespace MusicDataminer
{
    public partial class Form1 : Form
    {
        private MusicDBParser iParser;
        public bool queryOnGoing;

        private Hashtable iThreadList; 
        
        delegate void SetTextCallback(string text);

        public Form1()
        {
            InitializeComponent();

            //set all the styles to selected
            this.styleCheckBoxList.SetItemChecked(0, true);
            this.styleCheckBoxList.SetItemChecked(1, true);
            this.styleCheckBoxList.SetItemChecked(2, true);
            this.styleCheckBoxList.SetItemChecked(3, true);
            this.styleCheckBoxList.SetItemChecked(4, true);
            this.styleCheckBoxList.SetItemChecked(5, true);
            this.styleCheckBoxList.SetItemChecked(6, true);
            this.styleCheckBoxList.SetItemChecked(7, true);
            this.styleCheckBoxList.SetItemChecked(8, true);
            this.styleCheckBoxList.SetItemChecked(9, true);
            this.styleCheckBoxList.SetItemChecked(10, true);
            this.outputBox.Text = "Ready.\r\n";

            iThreadList = new Hashtable();
            iParser = new MusicDBParser( this );
            queryOnGoing = false;
        }


        //------------------------------------------------------------
        // BUTTONS:
        //------------------------------------------------------------
        void startQueryButton_Click(object sender, System.EventArgs e)
        {
            //Begin queries according to the style
            if( this.startQueryButton.Text.Equals("Start Queries") )
            {
                this.startQueryButton.Text = "Stop Query";
                queryOnGoing = true;
                PrintLine( "Restart Querying according to styles:\r\n-----------------------------------\r\n");
                foreach (string style in this.styleCheckBoxList.CheckedItems)
                {
//                    PrintLine("Style: " + style);
                    this.QueryByStyle(style);
                }
            }
            //Pause Querying
            else
            {
                PrintLine("Pausing please wait...");
                //foreach(Thread thr in iThreadList) 
                //{
                //    thr.Interrupt();
                //}
                foreach (string style in this.styleCheckBoxList.CheckedItems)
                {
                    PrintLine( "("+style + ")\tStopping..." );
                    if (((Thread)iThreadList[style]).IsAlive) 
                    {
                        ((Thread)iThreadList[style]).Abort();
                    }
                }
                iThreadList.Clear();
                queryOnGoing = false;
                this.startQueryButton.Text = "Start Queries";
            }
            this.startQueryButton.Invalidate();
        }

        void clearLogButton_Click(object sender, EventArgs e)
        {
            if (queryOnGoing)
            {
                MessageBox.Show("Please stop the ongoing query first.");
                //PrintLine("Please stop the ongoing query first.");
            }
            else
            {
                foreach (string style in this.styleCheckBoxList.CheckedItems)
                {
                    iParser.ClearLog(style);
                }
            }
        }




        //------------------------------------------------------------
        // QUERYING:
        //------------------------------------------------------------
        public int QueryByStyle(string aStyle)
        {
/*            MusicBrainz o;

            // Create the musicbrainz object, which will be needed for subsequent calls
            o = new MusicBrainz();

            // Set the proper server to use. Defaults to mm.musicbrainz.org:80
            if (Environment.GetEnvironmentVariable("MB_SERVER") != null)
                o.SetServer(Environment.GetEnvironmentVariable("MB_SERVER"), 80);
            // Check to see if the debug env var has been set 
            if (Environment.GetEnvironmentVariable("MB_DEBUG") != null)
                o.SetDebug(Environment.GetEnvironmentVariable("MB_DEBUG") != "0");
            // Tell the server to only return 4 levels of data, unless the MB_DEPTH env var is set
            if (Environment.GetEnvironmentVariable("MB_DEPTH") != null)
                o.SetDepth(int.Parse(Environment.GetEnvironmentVariable("MB_DEPTH")));
            else
                o.SetDepth(4);
*/
            iThreadList[aStyle] = iParser.AsyncParseByStyle(aStyle);
            return 0;
        }

        void outputBox_TextChanged(object sender, EventArgs e)
        {
            this.outputBox.SelectionStart = this.outputBox.TextLength;
            this.outputBox.Focus();
            this.outputBox.ScrollToCaret();
        }

        public void PrintLine(string aText )
        {
            if (this.outputBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback( PrintLine );
                this.Invoke(d, new object[] { aText });
            }
            else
            {
                this.outputBox.AppendText( aText + "\r\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"D:\Linkoping University\Information Visualization\Project\data\";
            openFileDialog1.Title = "Select two files";
            openFileDialog1.Filter = "Database Files|*.bin";

            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                string[] filenames = openFileDialog1.FileNames;
                if (filenames.Length > 1 && saveFileDialog1.ShowDialog() != DialogResult.Cancel)
                {
                    saveFileDialog1.Filter = "Database Files|*.bin";
                    string outputFilename = saveFileDialog1.FileName;
                    bool done = MusicDBParser.SynchronizeDBs(filenames[0], filenames[1], outputFilename);
                    if( done )
                    {
                        MessageBox.Show("Databases merged succesfully (I hope :P)");
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong :S !!!");
                    }
                }
            }
        }
    }
}