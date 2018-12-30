# File-manager
File-manager is a simple web API and single page application for browsing and managing directories on a web server. This is the initial version of the project and it should be used in a test environment. 

## Installation
Clone the repository and build & run with Visual Studio 2017.
  
## Currrent features
* Navigate to directories by URL or from UI
* Search for files
* Upload, download, move, copy & delete files
* UI displays folder / file counts and sizes for current directory
* UI runs in a dialog box that can be opened from an existing web page
* Use of browser back / forward buttons is supported

## Screenshots
  ![manager screenshot](/screenshots/manager.PNG?raw=true)
  ![transfer screenshot](/screenshots/transfer.PNG?raw=true)
  
## Usage
__Root Folder__

A root directory for the api can be set in file-manager/Models/Settings.cs. The directory will be created if it does not already exist.

__API__

| Method        | Endpoint                                | Usage                                                |
| :------------ | :-------------------------------------- | :--------------------------------------------------- |
| GET   	      | /api/{directoryname}/{directoryname}... | Get directories and files for a path                 |
| GET           | /api/{directoryname}?query=searchterm   | Get files that have a name that contains search term |
| POST          | /api/MoveItems/{items}                  | Move items to destination                            |
| POST          | /api/CopyItems/{items}	                | Copy items to destination                            |
| DELETE        | /api/DeleteItems/{items}                | Delete items                                         |

## Javascript libraries
File-manager uses Knockout and jQuery UI
