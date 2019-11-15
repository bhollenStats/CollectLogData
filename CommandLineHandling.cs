using System;
using System.Collections.Generic;

namespace CommandLineHandling
{
    class Processor    
    {
        public Dictionary<String, String> Process(string[] inputArguments, Dictionary<String, Boolean> programArguments)
        {
            Dictionary<String, String> outputArguments = new Dictionary<String, String>();
            
            long requiredArguments = 0;
            foreach (KeyValuePair<String, Boolean> entry in programArguments)
            {
                if (entry.Value == true)
                    requiredArguments++;
            }

            if (inputArguments.Length < requiredArguments)
            {
                outputArguments.Add("ParseStatus", false.ToString());
                outputArguments.Add("ParseMessage", "  Unable to find all required arguments!");
                return outputArguments;
            }

            bool cmdArgumentsAreValid = true;
            for(int i = 0; i < inputArguments.Length; i++) {
                string[] tempArg = inputArguments[i].Split("=");
                if (programArguments.ContainsKey(tempArg[0])) {
                    outputArguments.Add(tempArg[0], tempArg[1]);
                } else {
                    outputArguments.Add(tempArg[0], "Unsupported Argument");
                    string errMessage = "  Unsupported argument found on command line: [" + tempArg[0].ToString() + "]";
                    outputArguments.Add("ParseMessage", errMessage);
                    cmdArgumentsAreValid = false;
                }
            }
                
            outputArguments.Add("ParseStatus", cmdArgumentsAreValid.ToString());

            return outputArguments;
        }
    }
}