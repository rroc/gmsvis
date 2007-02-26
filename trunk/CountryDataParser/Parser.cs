using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using CarlosAg.ExcelXmlWriter;

namespace CountryDataParser
{
    class Parser
    {
        const string countriesXLSFilename = "countries_per_year.xls";

        public void Parse(string filename)
        {
            if( File.Exists( filename ) )
            {
                Workbook book = new Workbook();

                if (File.Exists(countriesXLSFilename))
                {
                    book.Load(countriesXLSFilename);
                }

                // get just the last name without the extension
                string[] tokens = filename.Split(new char[] { '\\', '.'});
                string sheetName = tokens[tokens.Length - 2];
                Worksheet sheet = book.Worksheets.Add(sheetName);

                StreamReader sr = File.OpenText(filename);
                string line = "";

                while ((line = sr.ReadLine()) != null)
                {
                    string country;
                    string value;
                    
                    // remove dollar signs
                    line = line.Replace("$", "");
                    GetData(line, out country, out value);
                    WorksheetRow row = sheet.Table.Rows.Add();
                    row.Cells.Add(country);
                    row.Cells.Add(value);
                }

                book.Save(countriesXLSFilename);
            }
        }

        private void GetData(string line, out string country, out string value)
        {
            Regex regex = new Regex(@"^[0-9]+\s+(?<country>[-()'a-z,A-Z]+(\s[-()'a-z,A-Z]+)*)\s*(?<value>([0-9,.]+|NA))$");
            MatchCollection matches = regex.Matches(line);
            country = "";
            value = "";

            foreach (Match match in matches)
            {
                country = match.Groups["country"].Value;
                value = match.Groups["value"].Value;
                value = value.Replace(",", "");

                break;
            }
        }
    }
}
