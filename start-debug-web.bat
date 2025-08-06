@echo off
set timestamp=%date:~-4,4%%date:~-10,2%%date:~-7,2%-%time:~0,2%%time:~3,2%%time:~6,2%
set timestamp=%timestamp: =0%
cd /d "C:\programming\LegalEnvironment\Law4Hire"
dotnet run --project Law4Hire.Web > "DebugLogWeb-%timestamp%.txt" 2>&1