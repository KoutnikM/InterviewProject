# InterviewProject
Synchronisation of two folders
	1. Checks size
	2. Checks timestap
	3. Checks hash using MD5


### Command to run:
```
dotnet run -- --source "<Source folder>" --target "<Target folder>" --interval <Interval in seconds> --log "<Log file path>"
```

### Dependencies:
- [NLog](https://nlog-project.org/) - Logging library
- [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) - Commandline parser

### Notes
- Synchronisation can be cancled by `Ctrl + C`
- Errors and informations are logged into file