using System;
using System.IO;
using System.Collections.Generic;

namespace FileCollectionMod
{
    class FileCollection
    {
        private string m_directoryToSearch = "";
        private string m_searchPattern = "";
        
        public string DirectoryToSearch { get => m_directoryToSearch; set => m_directoryToSearch = value; }
        public string SearchPattern { get => m_searchPattern; set => m_searchPattern = value; }

        public FileCollection(string aDirectory, string aSearchPattern)
        {
            m_directoryToSearch = aDirectory;
            m_searchPattern = aSearchPattern;
        }

        public List<String> BuildFileCollection()
        {
            string[] patternsToSearch = m_searchPattern.Split(";");
            List<String> matchingFiles = new List<String>();

            foreach (string patt in patternsToSearch)
            {
                foreach (string file in Directory.EnumerateFiles(DirectoryToSearch, patt))
                {
                    matchingFiles.Add(file.ToString());
                }
            }
            return matchingFiles;
        }
    }
}