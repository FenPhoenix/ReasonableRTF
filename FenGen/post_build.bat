echo ------------------------- START OF post_build.bat

rem ~ strips surrounded quotes if they exist
rem no spaces can exist around = sign for these lines
set ConfigurationName=%~1
set TargetDir=%~2
set ProjectDir=%~3
set SolutionDir=%~4
set PlatformName=%~5
set TargetFramework=%~6

echo Running code generator...

rem Autogenerate code
rem ---
rem vars with spaces in the value must be entirely in quotes
set "FenGen=%SolutionDir%FenGen\bin\Release\net48\FenGen.exe"

%FenGen%

echo Done!
