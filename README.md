# CollectLogData

This is a .NET CORE 3.0 application that will search through a directory to find lines
in the file that fall within the date and time window specified by the command line arguments.  

With this tool you can create extracted log files from a directory source with only
the messages that match the date and time stamp leading up to an event date and time 
specified by a time window, e.g. backwards from 2019-11-11T12:34:56 for 2 hours with the 
arguments "--eventDate=2019-11-11 --eventTime=12:34:56 --timeWindow=2[h]"

This tool could then help to reduce the time needed to manually trim log files so that
the focus of the investigation can quickly concentrate only on the messages printed
that led up to the event to be analyzed. 

CollectLogData requires two arguments to indicate the event date and time
in order to search the log files to retrieve the log file history from the event.

    --eventDate=yyyy-mm-dd
        Specifies the date of the event

    --eventTime=hh:mm:ss
        Specifies the time of the event

OPTIONAL ARGUMENTS

    --directory=path
        Specifies the directory to search for relevant log messages.
        Default value is the current directory '.'

    --timeWindow=x[d|h|m]
        Specifies the time window in which to search for relevant log messages.
        Default value is '1h' for a history of one hour backward and forward from the event
        The user can specify time in days[d], hours[h], or minutes[m]
        Default value is 1 hour, or '1[h]'

    --outputFilename=filename
        This option will open and write the output from the program to the location specified
        in the value for the argument, e.g. 'CollectLogFile.out
        Default value is 'Output.log'

    --searchDepth=[shallow|deep]
        This option will allow for either a shallow or deep search of the log files contained
        within the targeted search directory.  A deep search will search all files whereas a 
        shallow search will look for log messages from only a select number of log files
        Default value is 'shallow'.

EXAMPLES

    CollectLogData --eventDate=2019-10-10 --eventTime=23:33:02
        Shallow collection of relevant log files from the current directory plus/minus one hour

    CollectLogData --eventDate=2019-11-04 --eventTime=03:33:21
        Collects relevant log files from the 'd:/log' directory plus/minus one hour

    CollectLogData --eventDate=2019-08-12 --eventTime=09:05:44 --directory=../. --timeWindow=30[m]
        Collects relevant log files from the '../.' directory plus/minus thirty minutes

    CollectLogData --eventDate=2019-08-12 --eventTime=09:05:44 --directory=../. --searchDepth=deep
        Collects relevant log files from the '../.' directory with a deep search for the default time window


