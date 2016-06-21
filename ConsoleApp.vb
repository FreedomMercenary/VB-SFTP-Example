#region "License, Terms and Author(s)"
'
' VB-SFTP-Example
' Copyright (c) 2016 Chris Reynoso. All rights reserved.
'
'  Author(s):
'
'      Chris Reynoso, https://github.com/freedommercenary
'
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
'
'    http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
'
#End Region

Imports System.IO
Imports WinSCP

''' <summary>
''' ConsoleApp - SFTP Utility Example Class
''' </summary>
Module ConsoleApp

    Dim _sftpService As SftpService

    Sub Main()
        ' SFTP client requires full path, this is ugly to store in source, so instead
        ' get current directory of .exe and reference .ppk from there
        Dim currentDirectory = Directory.GetCurrentDirectory()
        Const keyFileName = "key.ppk"
        ' Alternate example with unprotected keyfile
        'Const keyFileName = "key-nopass.ppk"
        Dim keyFileLocation = currentDirectory + "\" + keyFileName

        ' Create service reference to do the transfer work
        ' In this example, we will pass a password for the .ppk to use it as it is protected
        _sftpService = New SftpService("sftp.myserver.com", "username", keyFileLocation, "ThisIsMyP@ssword")
        ' Alternate example, we will pass Nothing for the .ppk to use it as this .ppk was saved unprotected
        '_sftpService = New SftpService("sftp.myserver.com", "username", keyFileLocation, Nothing)

        ' Begin console logic for example
        Dim input = ""
        While IsNothing(input) or Trim(input) <> "4"

            Console.WriteLine("1 - Upload file")
            Console.WriteLine("2 - List files on server")
            Console.WriteLine("3 - Delete file")
            Console.WriteLine("4 - Exit")
            Console.Write("Please select an option: ")
            
            input = Console.ReadLine()
            If Not IsNothing(input)
              input = Trim(input)
            End If

            Select Case input
                Case "1"
                    Console.WriteLine("Uploading file to server...")
                    UploadFile()
                Case "2"
                    Console.WriteLine("Getting files...")
                    ListFiles()
                Case "3"
                    Console.WriteLine("Deleting file...")
                    DeleteFile()
                Case "4"
                    ' Do nothing here, will exit
                Case Else
                    Console.WriteLine("Please select a valid option")
                End Select
            
            Console.WriteLine()
        End While

    End Sub

    Private Sub DeleteFile()
        ' Attempt delete
        Dim result = _sftpService.DeleteFile("README.md")

        If result
            Console.WriteLine("File successfully deleted")
        Else 
            Console.WriteLine("Something went wrong and an error was probably printed to the console")
        End If
    End Sub

    Private Sub ListFiles()
        ' Get list of files
        Dim files As RemoteFileInfoCollection = _sftpService.ListFiles()

        If files Is Nothing
            Console.WriteLine("Something went wrong (authentication probably failed) and an error was probably printed to the console")
        Else 
            For Each fileInfo In files
                Console.WriteLine("{0} with size {1}, permissions {2} and last modification at {3}", _
                    fileInfo.Name, fileInfo.Length, fileInfo.FilePermissions, fileInfo.LastWriteTime)
            Next
        End If
    End Sub
    
    Private Sub UploadFile()
        ' Pass either a relative path here or an absolute path here
        ' Relative path will be relative to the location of the .exe output file
        ' Absolute path must be accessible by account running the .exe (same as well for relative)

        ' Absolute example
        'Dim result = sftpService.UploadFile("C:\Users\Chris\Documents\Visual Studio 2015\Projects\VB-SFTP-Example\README.md")
        ' Relative example - File does not have "Properties -> Copy to Output Directory = Copy ..."
        ' Without copy specified in configuration, file must be manually added there (or by another process)
        ' Instead, we know for DEBUG will be two folders up (app/debug/[app].exe), so we jump up two to app level
        Dim result = _sftpService.UploadFile("..\..\README.md")
        
        If result
            Console.WriteLine("File successfully uploaded")
        Else 
            Console.WriteLine("Something went wrong and an error was probably printed to the console")
        End If
    End Sub

End Module
