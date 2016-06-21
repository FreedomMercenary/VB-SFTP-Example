VB-SFTP-Example
============

This project is provided as an example of how to send files securely to an SFTP server using VB.NET and WinSCP.

This project is comprised of 3 main components that would need to replicated in a production software environment to work:

  * SFTP Binary Library (WinSCP)
  * SftpService.vb (SFTP Service/Wrapper Code)
  * Application logic (sample ConsoleApp.vb in project)

WinSCP
------------

WinSCP [.NET assembly / COM library](https://winscp.net/eng/download.php) offers the library to be used to facilitate connection over SFTP.  It is leveraged by the wrapper service for ease of use.
These files must be present in the project to work:

 1. WinSCP.exe
 2. WinSCPnet.dll

Installation instructions:

 1. Copy both files into project, make sure they are included in project
 2. Right click on References in Solutions Explorer -> click "Add Reference"
 3. Click on "Browse", locate .dll and add to project
 4. Right click on each file in solution explorer -> click "Properties"
 5. Edit "Copy to Output Directory" and set to "Copy if newer"

These files must be in the final output in the same directory,
you need to right click -> Properties, then set "Copy to Output Directory" to one of the "Copy ..." options.

SftpService.vb
------------

This is the service class to copy and use as-is to provide the SFTP functionality with WinSCP

##### Included Example Key
The included example key has this public thumbprint to set on your server for testing:
ssh-rsa AAAAB3NzaC1yc2EAAAABJQAAAQEAniLOkUwxAVBayxWbODbPrTZ7pU/dkIQDj6VRZeAbUT+vChbZs3tuDO7oMTKl35ALWIim8/uHU2T62+tyNxZo7thuAYg4VfX9EBgOb7G8/HZQ6yamiX/xADP4y4Tci9I9DoLqf62XRu8dq72GXDzZUG668053K7iJRvMbTwnE6Hx1iIhoK44whHJYvA2ub8SAzJ2gJbU3xY/OCN8HFJL2KnjBVXo4pu0O3Tnbi7vmkqyod9Y01oXHztfnbm/j1kcDEUkqVAY1rfzQ0xGLo9SnFPcB2kiX/xHOM+4INUP6qrjAMhOVziSQRM1nBrsTZhtF82M122vBShonTcMObm+kpQ== Example Key

### There are 3 types of connections supported by the service:

#### Certificate Authentication (passphrase secured certificate)

Path to certificate must be complete and app must have access to path/file

    Dim keyFileLocation = Directory.GetCurrentDirectory() + "\" + "key.ppk"

Certificate that requires passphrase:

    Dim sftpService = New SftpService("sftp.myserver.com", "username", keyFileLocation, "ThisIsMyP@ssword")

#### Certificate Authentication (unsecured certificate)

Path to certificate must be complete and app must have access to path/file

    Dim keyFileLocation = Directory.GetCurrentDirectory() + "\" + "key-nopass.ppk"

Certificate that does not have a passphrase:

    Dim sftpService = New SftpService("sftp.myserver.com", "username", keyFileLocation, Nothing)

#### Password Authentication

    Dim sftpService = New SftpService("sftp.myserver.com", "username", "ThisIsMyP@ssword")


#### Supported actions

Uploading file to server:

    Dim result = sftpService.UploadFile("..\..\README.md")

Listing files in current directory:

    Dim files As RemoteFileInfoCollection = sftpService.ListFiles()

    For Each fileInfo In files
        Console.WriteLine("{0} with size {1}, permissions {2} and last modification at {3}", _
            fileInfo.Name, fileInfo.Length, fileInfo.FilePermissions, fileInfo.LastWriteTime)
    Next

Listing files in specific directory:

    Dim files As RemoteFileInfoCollection = sftpService.ListFiles("/home/user")

    For Each fileInfo In files
        Console.WriteLine("{0} with size {1}, permissions {2} and last modification at {3}", _
            fileInfo.Name, fileInfo.Length, fileInfo.FilePermissions, fileInfo.LastWriteTime)
    Next

Deleting a file from the server:

    Dim result = sftpService.DeleteFile("README.md")

ConsoleApp.vb
------------

This shows a sample of how to interact with the SftpService class, it represents sample
control code you would instead have in a production application