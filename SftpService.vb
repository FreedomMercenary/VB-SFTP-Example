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
Imports System.Runtime.InteropServices
Imports System.Security
Imports WinSCP

''' <summary>
''' SftpService - SFTP Utility Class
''' </summary>
Public Class SftpService

    Private ReadOnly Dim _hostName As String
    Private ReadOnly _password As SecureString
    Private ReadOnly _privateKeyPath As String
    Private ReadOnly _sessionOptions As SessionOptions
    Private ReadOnly Dim _username As String

    ''' <summary>Standard constructor for username/password authentication</summary>
    ''' <param name="hostName">SFTP hostname</param>
    ''' <param name="username">SFTP account username</param>
    ''' <param name="password">SFTP account password</param>
    Public Sub New(hostName As String, username As String, password As String)
        _hostName = HostName
        _password = New SecureString
        For Each character As Char In password.ToCharArray
            _password.AppendChar(character)
        Next
        _password.MakeReadOnly()
        _username = username
        _sessionOptions = CreateSessionOptions()
    End Sub

    ''' <summary>Standard constructor for username/certificate authentication</summary>
    ''' <param name="hostName">SFTP hostname</param>
    ''' <param name="username">SFTP account username</param>
    ''' <param name="privateKeyPassword">Private key password, <c>String</c> if password needed,
    ''' <c>Nothing</c> if not password protected</param>
    Public Sub New(hostName As String, username As String, privateKeyPath As String, privateKeyPassword As String)
        _hostName = HostName
        _password = New SecureString()
        If Not IsNothing(privateKeyPassword)
            For Each character As Char In privateKeyPassword.ToCharArray
                _password.AppendChar(character)
            Next
        End If
        _password.MakeReadOnly()
        _privateKeyPath = privateKeyPath
        _username = username
        _sessionOptions = CreateSessionOptions()
    End Sub

    ''' <summary>Utility method to get string from SecureString</summary>
    ''' <param name="secureString">SecureString to convert to unsecured String</param>
    ''' <returns>Unsecured String</returns>
    Private Shared Function ConvertToUnsecureString(secureString As SecureString) As String
        If IsNothing(secureString)
            throw new ArgumentNullException("secureString")
        End If

        Dim unmanagedString = ""
        Dim bstr = Marshal.SecureStringToBSTR(secureString)
        Try
            unmanagedString = Marshal.PtrToStringBSTR(bstr)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            Marshal.FreeBSTR(bstr)
        End Try

        Return unmanagedString
    End Function

    ''' <summary>Shared function to configure connection (to be called once only)</summary>
    ''' <returns>Populated and configured <c>SessionOptions</c> object</returns>
    Private Function CreateSessionOptions() As SessionOptions
        Dim sessionOptions As New SessionOptions

        ' Setup private key authentication
        If Not String.IsNullOrWhiteSpace(_privateKeyPath)
            With sessionOptions
                .Protocol = Protocol.Sftp
                .HostName = _hostName
                .UserName = _username
                .SshPrivateKeyPath = _privateKeyPath
                .SshPrivateKeyPassphrase = ConvertToUnsecureString(_password)
                .GiveUpSecurityAndAcceptAnySshHostKey = True
            End With
            ' No private key - use username & password
        Else
            With sessionOptions
                .Protocol = Protocol.Sftp
                .HostName = _hostName
                .UserName = _username
                .Password = ConvertToUnsecureString(_password)
                .GiveUpSecurityAndAcceptAnySshHostKey = True
            End With
        End If

        Return sessionOptions
    End Function

    ''' <summary>Delete a specific file</summary>
    ''' <param name="filename">The remote filename to delete</param>
    ''' <returns><c>True</c> if successfully deleted or <c>False</c> otherwise</returns>
    Public Function DeleteFile(filename As String) As Boolean
        Dim result = false

        Try
            ' Setup session options
            Using session As New Session
                ' Connect
                session.Open(_sessionOptions)

                Dim removeResult = session.RemoveFiles(filename)

                result = removeResult.IsSuccess

                If Not result
                    Console.WriteLine("There was an error deleting the file")
                    Console.WriteLine(removeResult.Failures.First().Message)
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return result
    End Function

    ''' <summary>List files from current directory</summary>
    ''' <returns><c>RemoteFileInfoCollection</c> with result and list of files</returns>
    Public Function ListFiles() As RemoteFileInfoCollection
        Return ListFiles(Nothing)
    End Function

    ''' <summary><para>List files from specific directory (usually complete starting at root
    ''' /home/user/folder, etc)</para></summary>
    ''' <param name="path">Remote server path or Nothing to list current directory</param>
    ''' <returns><c>RemoteFileInfoCollection</c> with result and list of files</returns>
    Public Function ListFiles(path As String) As RemoteFileInfoCollection
        Dim remoteFiles As RemoteFileInfoCollection = Nothing
        Try
            ' Setup session options
            Using session As New Session
                ' Connect
                session.Open(_sessionOptions)

                Dim directory As RemoteDirectoryInfo = session.ListDirectory(If(String.IsNullOrWhiteSpace(path), ".", path))

                remoteFiles = directory.Files
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return remoteFiles
    End Function

    ''' <summary>
    ''' Upload a file to the remote server
    ''' </summary>
    ''' <param name="filePath">Local fully qualified path to the file to upload</param>
    ''' <returns><c>True</c> if successful, <c>False</c> otherwise</returns>
    Public Function UploadFile(filePath As String) As Boolean
        If IsNothing(filePath)
            Throw new ArgumentNullException("filePath")
        End If
        ' Get filename only here by searching for directory separator
        Dim backslashIndex = filePath.LastIndexOf("\", StringComparison.InvariantCultureIgnoreCase)
        Dim forwardSlashIndex = filePath.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase)
        ' Take substring for filename, use 0 as default in case of filename only others will be -1
        Dim filename = filePath.Substring(Math.Max(Math.Max(backslashIndex, forwardSlashIndex), 0))
        
        Return UploadFile(filePath, filename)
    End Function

    ''' <summary>
    ''' Upload a file to the remote server with a specified remote path
    ''' </summary>
    ''' <param name="filePath">Local fully qualified path to the file to upload</param>
    ''' <param name="remoteFilePath">Remote path to put file or different name to use for upload</param>
    ''' <returns><c>True</c> if successful, <c>False</c> otherwise</returns>
    Public Function UploadFile(filePath As String, remoteFilePath As String) As Boolean
        Dim result = false
        Dim resolvedFilePath = Path.GetFullPath(filePath)

        Try
            ' Setup session options
            Using session As New Session
                ' Connect
                session.Open(_sessionOptions)

                Dim uploadResult = session.PutFiles(resolvedFilePath, remoteFilePath)

                result = uploadResult.IsSuccess

                If Not result
                    Console.WriteLine("There was an error uploading the file")
                    Console.WriteLine(uploadResult.Failures.First().Message)
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return result

    End Function

End Class
