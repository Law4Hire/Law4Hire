# Law4Hire Project Management Scripts

This directory contains automation scripts for managing Law4Hire project processes.

## Scripts

### ProjectManager.ps1
The main project management script with full functionality.

**Usage:**
```powershell
.\ProjectManager.ps1 -Project <ProjectName> -Action <start|stop|restart>
```

**Projects:**
- `Law4Hire.API` - API Server
- `Law4Hire.Web` - Web Application  
- `Law4Hire.GovScraper` - Government Visa Wizard Scraper
- `Law4Hire.Scraper` - Visa Data Scraper
- `Law4Hire.Mobile` - MAUI Mobile Application

**Examples:**
```powershell
.\ProjectManager.ps1 -Project "Law4Hire.API" -Action "start"
.\ProjectManager.ps1 -Project "Law4Hire.GovScraper" -Action "restart"
.\ProjectManager.ps1 -Project "Law4Hire.Web" -Action "stop"
```

### pm.ps1
Shorthand script for quick project management.

**Usage:**
```powershell
.\pm.ps1 <project> <action>
```

**Project Shortcuts:**
- `api` → Law4Hire.API
- `web` → Law4Hire.Web
- `scraper` → Law4Hire.Scraper
- `govscraper` or `gov` → Law4Hire.GovScraper
- `mobile` or `maui` → Law4Hire.Mobile

**Examples:**
```powershell
.\pm.ps1 api start
.\pm.ps1 web stop
.\pm.ps1 gov restart
.\pm.ps1 scraper start
```

## Features

- ✅ **Process Management**: Automatically finds and stops running processes
- ✅ **Build Validation**: Tests build before starting projects
- ✅ **Colored Logging**: Easy-to-read output with timestamps
- ✅ **Error Handling**: Comprehensive error checking and reporting
- ✅ **Path Resolution**: Automatically resolves project paths
- ✅ **Process Monitoring**: Provides PID information for tracking

## Usage from Root Directory

From the Law4Hire root directory:
```powershell
# Using full script
powershell.exe -ExecutionPolicy Bypass -File ".claude\ProjectManager.ps1" -Project "Law4Hire.API" -Action "start"

# Using shorthand
powershell.exe -ExecutionPolicy Bypass -File ".claude\pm.ps1" api start
```

## Notes

- Scripts automatically handle process cleanup between restarts
- Build validation prevents starting projects with compilation errors
- All projects are started in separate processes for independent monitoring
- Use `Get-Process -Name "Law4Hire*"` to see all running Law4Hire processes