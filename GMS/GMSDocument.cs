using System;
using System.Collections.Generic;
using System.Text;
using MusicDataminer;

namespace GMS
{
    class GMSDocument
    {
        public void ReadDB(string filename)
        {
            DB db;
            bool loaded = MusicDBLoader.LoadDB(filename, out db);

            if( ! loaded )
            {
                // TODO: something :P
            }
        }
    }
}
