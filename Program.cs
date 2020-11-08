using System;
using System.IO;
using System.Configuration;  
using System.Collections.Generic;

/* 
* TODO:
*     1.  List of search keywords in a file. Go through each search item.
*     2.  Supports two minimum input.
*          a.  Search keyword file. // NO INPUT from user.
*              ./Config/CodeScan_Keywords.txt
*          b.  Search folder. // Folder that contains one or more file.
*
*     3.  Outputs:
*         a.  Writes a CSV file in a result folder. 
*             i.  Search keyword.
*            ii.  Filename with path.
*           iii.  Linenumber.
*            iv.  Text
*          File:  ./Results/CodeScan_Results_<TIMESTAMP>.csv
*/

namespace searchApp
{
    class Search
    {
        public List<string> Do(string startFolder, List<string> fileExtensions, string searchKeyword)
        {
            string sourceFolder = startFolder;
            string searchWord = searchKeyword;

            // Create a list to hold the files to search.
            List<string> allFiles = new List<string>();

            // Create a list to hold the results to return.
            List<string> allResults = new List<string>();

            // Call program to add files to the list.
            AddFileNamesToList(sourceFolder, allFiles, fileExtensions);

            // Now we have the list ready. Go through one-by-one.
            foreach (string fileName in allFiles)
            {
                int lineNum = 0;
                foreach (string line in File.ReadLines(fileName))
                {
                    lineNum ++;
                    if (line.Contains(searchWord))
                    {
                        //Keyword, Filename, Line#, Text
                        allResults.Add( searchWord + "," + fileName + "," + lineNum + "," + line);
                    }
                }
            }

            return allResults;
        }

        public static void AddFileNamesToList(string sourceDir, List<string> allFiles, List<string> fileExts)
        {
            string[] fileEntries = Directory.GetFiles(sourceDir);
            foreach (string fileName in fileEntries)
            {
                // Add files only if the extension.
                if (fileExts.Contains(Path.GetExtension(fileName).ToUpper()))
                {
                    allFiles.Add(fileName);
                }
            }

            //Recursion
            string[] subdirectoryEntries = Directory.GetDirectories(sourceDir);
            foreach (string item in subdirectoryEntries)
            {
                // Avoid "reparse points"
                if ((File.GetAttributes(item) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    AddFileNamesToList(item, allFiles, fileExts);
                }
            }
        }
    }

    class SearchWrapper{
        static void Main(string[] args){

            string searchFileName = ReadSetting("SEARCH_KEYWORD_FILEPATH");
            string searchFolder = ReadSetting("SEARCH_PATH");
            string searchFileExtStr = ReadSetting("CODESCAN_FILE_EXTENSIONS");

            List<string> searchFileExtensions = new List<string>(searchFileExtStr.Split(','));

            string searchResultFile = ReadSetting("SEARCH_RESULT_FILEPATH");
            System.IO.File.WriteAllText(searchResultFile, "");

            foreach (string line in File.ReadLines(searchFileName))
            {
                Scan(searchFolder, searchFileExtensions, line);
            }
        }
        static string ReadSetting(string key)  
        {  
            try  
            {  
                var appSettings = ConfigurationManager.AppSettings;  
                return appSettings[key] ?? "Not Found";
            }  
            catch (ConfigurationErrorsException)  
            {  
                return "ERROR";
            }
        }
        public static void Scan(string searchFolder, List<string> searchFileExt, string searchKey){
            string searchResultFile = ReadSetting("SEARCH_RESULT_FILEPATH");

            Search searchObj = new Search();
            //Loop through the file.
            System.IO.File.AppendAllLines(searchResultFile, searchObj.Do(searchFolder, searchFileExt, searchKey));
        }
    }
}
