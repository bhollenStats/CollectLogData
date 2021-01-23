using System;
using System.Collections.Generic;
using System.IO;

namespace CollectLogData
{
    class MainModule
    {
        private const string messageStartOfCollection = "Collecting Log Data [{3}] from {0} to {1} within the directory '{2}' into the file {4}";
        private static readonly string[] commandLineMessage = {
            @"",
            @"CollectLogData requires two arguments to indicate the event date and time",
            @"in order to search the log files to retrieve the log file history from the event.",
            @"",
            @"    --eventDate=yyyy-mm-dd",
            @"        Specifies the date of the event",
            @"",
            @"    --eventTime=hh:mm:ss",
            @"        Specifies the time of the event",
            @"",
            @"OPTIONAL ARGUMENTS",
            @"",
            @"    --directory=path",
            @"        Specifies the directory(s) to search for relevant log messages.",
            @"        Default value is the current directory '.'",
            @"        Value could be a semicolon-separated path, e.g. '.;./dirA;./dirB",
            @"",
            @"    --timeWindow=x[d|h|m]",
            @"        Specifies the time window in which to search for relevant log messages.",
            @"        Default value is '1h' for a history of one hour backward and forward from the event",
            @"        The user can specify time in days[d], hours[h], or minutes[m]",
            @"        Default value is 1 hour, or '1[h]'",
            @"",
            @"    --outputFilename=filename",
            @"        This option will open and write the output from the program to the location specified",
            @"        in the value for the argument, e.g. 'CollectLogFile.out",
            @"        Default value is constructed from command inputs or defaults,",
            @"        e.g. 20201231T112233_CLD_1h_shallow.csv",
            @"",
            @"    --searchDepth=[shallow|deep]",
            @"        This option will allow for either a shallow or deep search of the log files contained",
            @"        within the targeted search directory.  A deep search will search all files whereas a ",
            @"        shallow search will look for log messages from only a select number of log files",
            @"        Default value is 'shallow' or files matching the pattern 'mess*' or 'TBT*'",
            @"",
            @"EXAMPLES",
            @"",
            @"    CollectLogData --eventDate=2019-10-10 --eventTime=23:33:02",
            @"        Shallow collection of relevant log files from the current directory plus/minus one hour",
            @"",
            @"    CollectLogData --eventDate=2019-11-04 --eventTime=03:33:21",
            @"        Collects relevant log files from the current directory plus/minus one hour",
            @"",
            @"    CollectLogData --eventDate=2019-11-04 --eventTime=03:33:21 --directory=.;./dirA;./dirB",
            @"        Collects relevant log files from the directories provided for plus/minus one hour",
            @"",
            @"    CollectLogData --eventDate=2019-08-12 --eventTime=09:05:44 --directory=../. --timeWindow=30[m]",
            @"        Collects relevant log files from the '../.' directory plus/minus thirty minutes",
            @"",
            @"    CollectLogData --eventDate=2019-08-12 --eventTime=09:05:44 --directory=../. --searchDepth=deep",
            @"        Collects relevant log files from the '../.' directory with a deep search for the default time window"
        };
        private const string datetimeFormat = "yyyy-MM-dd'|'HH:mm:ss";
        private const string justDateFormat = "yyyy-MM-dd";
        private const string justTimeFormat = "HH:mm:ss";
        private const string shallowFilesToSearch = "messages.p*;TBT*";
        private const string fileContentFields = "Date|Time|SourceFile|LogMessage";

        static void Main(string[] args)
        {

            // Build a dictionary of command line arguments with flags indicating if they are required
            Dictionary<String, Boolean> cmdArguments = new Dictionary<String, Boolean>();
            cmdArguments.Add("--eventDate", true);
            cmdArguments.Add("--eventTime", true);
            cmdArguments.Add("--directory", false);
            cmdArguments.Add("--timeWindow", false);
            cmdArguments.Add("--outputFilename", false);
            cmdArguments.Add("--searchDepth", false);

            // Pass arguments to the command line processor component to build a list of individual arguments 
            // as well as an overall status is all the arguments are available
            CommandLineHandling.Processor clp = new CommandLineHandling.Processor();
            Dictionary<String, String> processArgs = clp.Process(args, cmdArguments);

            // Check to see if there was a problem to process and get all the command line arguments
            if ("False".CompareTo(processArgs["ParseStatus"]) == 0)
            {
                Console.WriteLine("Unable to process command line arguments!");
                if (processArgs.ContainsKey("ParseMessage"))
                    Console.WriteLine(processArgs["ParseMessage"].ToString());
                ShowCommandLineMessage();
                return;
            }

            // Unpack and process the processed command line arguments
            if (!processArgs.ContainsKey("--directory"))
                processArgs.Add("--directory", ".");
            if (!processArgs.ContainsKey("--timeWindow"))
                processArgs.Add("--timeWindow", "1[h]");
            if (!processArgs.ContainsKey("--searchDepth"))
                processArgs.Add("--searchDepth", "shallow");
            if (!processArgs.ContainsKey("--outputFilename"))
            {
                String generatedFilename = @"CLD_" + processArgs["--eventDate"].ToString().Replace("-","") + "T";
                generatedFilename += processArgs["--eventTime"].ToString().Replace(":","") + "_";
                generatedFilename += processArgs["--timeWindow"].ToString().Replace("[","").Replace("]","") + "_";
                generatedFilename += processArgs["--searchDepth"] + ".csv";
                processArgs.Add("--outputFilename", generatedFilename);
            }

            // ToDo: Validation checking
            string eventTime = processArgs["--eventTime"];
            string eventDate = processArgs["--eventDate"];
            string dirToSearch = processArgs["--directory"];
            string timeWindow = processArgs["--timeWindow"];
            string outputFilename = processArgs["--outputFilename"];
            string searchDepth = processArgs["--searchDepth"];

            // Determine the event date and time provided
            DateTime eventDateTime = DateTime.Parse(eventDate + "T" + eventTime);

            // Calculate the start time of the window from the timeWindow to search, e.g. 4[h], 30[m]
            DateTime startDateTime;
            string[] timeWindowValues = timeWindow.Split('[');
            int timeWindowValue = Int32.Parse(timeWindowValues[0]);
            string timeWindowUnit = timeWindowValues[1].Substring(0, 1);
            switch (timeWindowUnit.ToString().ToLower())
            {
                case "m":
                    startDateTime = eventDateTime.AddMinutes(-1 * timeWindowValue);
                    break;
                case "h":
                    startDateTime = eventDateTime.AddHours(-1 * timeWindowValue);
                    break;
                default:
                    startDateTime = eventDateTime.AddMinutes(-1 * timeWindowValue);
                    break;
            }

            // Determine if we have one or two dates to search for
            long numberOfDatesToSearch;
            if (eventDateTime == startDateTime)
                numberOfDatesToSearch = 1;
            else
                numberOfDatesToSearch = 2;

            // Determine the files to search for
            string searchPattern = "";
            if (searchDepth.CompareTo("deep") == 0)
                searchPattern = "*";
            else
                searchPattern = shallowFilesToSearch;

            // Build a list of all the matching files from the directory provided
            List<String> matchingFiles = new List<String>();

            if (dirToSearch.Contains(";")) 
            {
                string[] paths2Search = dirToSearch.Split(";");

                foreach (string p in paths2Search) 
                {
                    try
                    {
                        FileCollectionMod.FileCollection lfc = new FileCollectionMod.FileCollection(p, searchPattern);  
                        matchingFiles.AddRange(lfc.BuildFileCollection());  
                    }
                    catch(Exception)
                    {
                        Console.WriteLine(@"Unable to search {0}", p);
                    }
                
                }
            }
            else
            {
                FileCollectionMod.FileCollection lfc = new FileCollectionMod.FileCollection(dirToSearch, searchPattern);
                matchingFiles.AddRange(lfc.BuildFileCollection());
            }

            // Iterate through each of the matching files to find log messages within the 
            // time window from the startDateTime to the eventDateTime
            using (StreamWriter sW = new StreamWriter(outputFilename.ToString()))
            {
                Console.WriteLine(messageStartOfCollection, startDateTime.ToString(datetimeFormat), eventDateTime.ToString(datetimeFormat), dirToSearch, searchDepth, outputFilename.ToString());
                Console.WriteLine(fileContentFields);
                sW.WriteLine(messageStartOfCollection, startDateTime.ToString(datetimeFormat), eventDateTime.ToString(datetimeFormat), dirToSearch, searchDepth, outputFilename.ToString());
                sW.WriteLine(fileContentFields);
                bool roCopyToDelete = false;

                foreach (String s in matchingFiles)
                {
                    String file2Open = s.ToString();

                    try
                    {
                        StreamReader testReader = File.OpenText(file2Open);
                        testReader.Close();
                        roCopyToDelete = false;
                    }
                    catch (Exception)
                    {
                        File.Copy(file2Open, file2Open + ".roCopy");
                        file2Open += ".roCopy";
                        roCopyToDelete = true;
                    }

                    StreamReader reader = File.OpenText(file2Open);

                    string stringDT;
                    DateTime dateDT;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        // Look for a line that has the provided eventDateTime or startDateTime
                        int eventDateIndex = 0;
                        int startDateIndex = 0;
                        eventDateIndex = line.IndexOf(eventDateTime.ToString(justDateFormat));
                        if (numberOfDatesToSearch == 2)
                            startDateIndex = line.IndexOf(startDateTime.ToString(justDateFormat));
                        else
                            startDateIndex = 0;

                        // Was there a match from the line in the file?
                        if (startDateIndex > 0 || eventDateIndex > 0)
                        {
                            if (startDateIndex > 0)
                            {
                                try
                                {
                                    stringDT = line.Substring(startDateIndex, 19);
                                    if (stringDT.Substring(11).CompareTo(";") != 0)
                                        stringDT = stringDT.Replace(';', 'T');
                                    dateDT = DateTime.Parse(stringDT);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                try
                                {
                                    stringDT = line.Substring(eventDateIndex, 19);
                                    if (stringDT.Substring(11).CompareTo(";") != 0)
                                        stringDT = stringDT.Replace(';', 'T');
                                    dateDT = DateTime.Parse(stringDT);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }

                            if (dateDT >= startDateTime && dateDT <= eventDateTime)
                            {
                                Console.WriteLine("{0}|{1}|{2}", dateDT.ToString(datetimeFormat), s.ToString(), line);
                                sW.WriteLine("{0}|{1}|{2}", dateDT.ToString(datetimeFormat), s.ToString(), line);
                            }
                        }
                    }
                    reader.Close();
                    if (roCopyToDelete == true)
                    {
                        File.Delete(file2Open);
                    }
                }
            }
        }

        private static void ShowCommandLineMessage()
        {
            for (int i = 0; i < commandLineMessage.Length; i++)
                Console.WriteLine(commandLineMessage[i]);
        }
    }
}
