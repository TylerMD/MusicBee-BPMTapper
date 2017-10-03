'-----------------------------------------------------------------------------------------------------------------------------
' AUTHOR:	T. Decaussin
' TITLE:	mb_BPMTapper - Opens a PopUp for Tapping the BPM
' DATE:		October 3, 2017 (Last Update)
' MB VERSION: V3.1.6466

'DESCRIPTION:
'  MATH: (<Taps created> - 1) / (<Time Interval in ms>) * (60000 ms / 1 min) = BPM
'  Calculates BPM using counting the Taps per given time interval.
'  Is the exact same stratedgy as averaging the distance between Taps.
'  This method always tends toward exactness. Typically ~30 taps for ~180 BPM range
'  Note: I've tested this with a Metronome for various speeds, it is accurate to the precision of the user. -Oct23,14T
'
'-----------------------------------------------------------------------------------------------------------------------------
'
' What's New:
'
'   V1.2    (Oct 3 2017)
'   - Changed the Lyricist tag save action into a checkbox option
'   - Tapper now automatically resets if Difference after 2nd Tap is greater than 75.
'       + An improvement would be to us a proprotionaly, like +/- 50%, but this works well enough
'
'   V1.1    (Oct 23 2014)
'   - When the Now Playin song changes, the pop-up is automatically reset
'       - Had to use the methodInvoker for safe cross-threading.
'   - The "Tap" in the Menu will reset and open the pop-up if it isn't visible
'   - Added the ability to save "0" value, which clears the BPM tag
'   - Shortened the Plugin Description to fit the Preferences' window
'   - Reoragnized the buttons to be "Tap | Save | Reset" instead of "Tap | Reset | Save"
'       - Makes more intuitive sense, especially when using Hotkeys
'
'   V1.0    (Oct 8 2014)
'   - Added Hotkey Attachments to Tap, Reset, and Save
'       - Now user can save using hotkeys, even while popup is hidden
'   - Now values are erased when a song is changed
'   - Fixed bug where count wasn't reset after a save (kept incrementing)
'   - Specified the VS Project to Only copy the .dll into MusicBee\Plugins folder.
'       - Was copying over the entire directory, which put junk inside \Plugins. Looked messy.
'
'   V1.0    (Sep 13 2014)
'   - Created 1st Version
'
'-----------------------------------------------------------------------------------------------------------------------------
' WISHLIST:
'   + BUG: On some PCs, Holding down Ctrl+1 keeps pressing Tap into the 1000s.
'   + Add uninstaller
'   + Remove the Titlebar to reduce footprint.
'
'-----------------------------------------------------------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Windows.Forms

Public Class Plugin
    Private mbApiInterface As New MusicBeeApiInterface
    Private about As New PluginInfo

    Public writeLyric As New Boolean
    Public sLyric As CheckBox   'I don't like how this is required

    Public Shared strBPM As String = ""
    Public Shared strLyricist As String = ""

    Private sourceUrl As String                  'The path for the Now Playing Song (when pop is created)
    Public WithEvents BPMForm As New Form1()      'The BPM Tapper 
    'Dim WithEvents myForm As New Form1()        'The BPM Tapper **Works, but can't reset -Oct8,14T



    Private Function createMenuItem() As String
        ' Creates the SubMenu with Menu Items in Tool Menu
        ' The Submenu process was discovered following this adivce from Stephen:
        '   + https://getmusicbee.com/forum/index.php?topic=21902.0        
        Dim itm As New ToolStripMenuItem()
        itm = CType(mbApiInterface.MB_AddMenuItem("mnuTools/BPM", "", AddressOf TapClicked), ToolStripMenuItem)
        mbApiInterface.MB_AddMenuItem("mnuTools/BPM: Open Pop-up \ Tap Button", "BPM: Open Pop-up \ Tap Button", AddressOf TapClicked)
        mbApiInterface.MB_AddMenuItem("mnuTools/BPM: Save Button", "BPM: Save Button", AddressOf SaveClicked)
        mbApiInterface.MB_AddMenuItem("mnuTools/BPM: Reset Button", "BPM: Reset Button", AddressOf ResetClicked)

        Return Nothing
    End Function

    Sub TapClicked(ByVal sender As Object, ByVal e As EventArgs)
        ' User clicked on "Tap" Button inside Menu
        'Debug.Print("Hotkey Tap")
        If (BPMForm.Visible) Then                'IF Form is already Open
            BPMForm.Btn_Tap()
        Else
            BPMForm.Btn_Reset()                  'Reset its values (If used, THEN Closed it retains values)
            BPMForm.Show()                       'Show the Form
        End If

    End Sub

    Sub SaveClicked(ByVal sender As Object, ByVal e As EventArgs)
        ' User clicked on "Save" Button inside Menu
        BPMForm.Btn_Save()
    End Sub

    Sub ResetClicked(ByVal sender As Object, ByVal e As EventArgs)
        ' User clicked on "Reset" Button inside Menu
        BPMForm.Btn_Reset()
    End Sub

    Sub MenuClicked(ByVal sender As Object, ByVal e As EventArgs)
        ' User clicked on Menu Item
        ' No longer used. -Oct23,14T

        ' Open the Form
        sourceUrl = mbApiInterface.NowPlaying_GetFileUrl()
        strBPM = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.BeatsPerMin)     'Get the Current BPM Value. TagValues: 65=Title / 41=BPM / 62 = Lyricist
        strLyricist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Lyricist)   'Get the Current Lyricist Tag.
        'Debug.Print("URL is " + sourceUrl)

        If (BPMForm.Visible) Then                 'IF Form is already Open
            Debug.Print("Refocusing on Form")
            BPMForm.Activate()                       'Put in focus **maybe not necessary -Oct8,14T
            BPMForm.Btn_Reset()                      'Reset its values
        Else
            BPMForm.Show()                           'Show the Form
        End If

        'System.Windows.Forms.Application.Run(New Form1())      'aka "Form1 myForm = new Form1(); myForm.Show();"
    End Sub

    Private Sub myForm_tagSaveBPM(ByVal oldValue As String, ByVal newValue As String) Handles BPMForm.tagSaveBPM
        ' Saves the new BPM to the Now Playing Song's Tag
        '
        Debug.Print("BPM is = " + strBPM)

        'Convert Textbox value into a Number
        Dim decBPM As Decimal = 0
        Try
            decBPM = Convert.ToDecimal(strBPM)
        Catch ex As Exception               'Catch Format Error when converting the Textbox String to Decimal
            decBPM = -1                     'IF content was NaN, THEN set value to -1 
        End Try


        If (decBPM > 0) Then                'IF Textbox's BPM is > 0, THEN Save to Tag

            If (writeLyric) Then                'IF Setting Is Selected to Save Lyricis Tag
                If (strLyricist = "") Then      'IF Current Lyricist is non empty, THEN don't modify it
                    mbApiInterface.Library_SetFileTag(sourceUrl, MetaDataType.Lyricist, "#BPMManual")  'My way of knowing that I manually set this song's BPM
                End If
            End If

            mbApiInterface.Library_SetFileTag(sourceUrl, MetaDataType.BeatsPerMin, strBPM)
            mbApiInterface.Library_CommitTagsToFile(sourceUrl)
            mbApiInterface.MB_RefreshPanels()


        ElseIf (decBPM = 0) Then            'ELSE IF Textbox's BPM is 0, THEN Erase from Tag


            If (strLyricist = "#BPMManual") Then    'IF Current Lyricist is "#BPMManual", THEN Clear it
                mbApiInterface.Library_SetFileTag(sourceUrl, MetaDataType.Lyricist, "")  'Clear the Lyricist tag
            End If

            mbApiInterface.Library_SetFileTag(sourceUrl, MetaDataType.BeatsPerMin, "")   'Clear the BPM tag
            mbApiInterface.Library_CommitTagsToFile(sourceUrl)                           'Save updated tags to the File
            mbApiInterface.MB_RefreshPanels()                                            'Refresh MB's panels (the slowest operation)
        End If
    End Sub


    Public Function Initialise(ByVal apiInterfacePtr As IntPtr) As PluginInfo
        CopyMemory(mbApiInterface, apiInterfacePtr, 4)
        If mbApiInterface.MusicBeeVersion = MusicBeeVersion.v2_0 Then
            ' MusicBee version 2.0 - Api methods > revision 25 are not available
            CopyMemory(mbApiInterface, apiInterfacePtr, 456)
        ElseIf mbApiInterface.MusicBeeVersion = MusicBeeVersion.v2_1 Then
            CopyMemory(mbApiInterface, apiInterfacePtr, 516)
        ElseIf mbApiInterface.MusicBeeVersion = MusicBeeVersion.v2_2 Then
            CopyMemory(mbApiInterface, apiInterfacePtr, 584)
        ElseIf mbApiInterface.MusicBeeVersion = MusicBeeVersion.v2_3 Then
            CopyMemory(mbApiInterface, apiInterfacePtr, 596)
        ElseIf mbApiInterface.MusicBeeVersion = MusicBeeVersion.v2_4 Then
            CopyMemory(mbApiInterface, apiInterfacePtr, 604)
        ElseIf mbApiInterface.MusicBeeVersion = MusicBeeVersion.v2_5 Then
            CopyMemory(mbApiInterface, apiInterfacePtr, 648)
        Else
            CopyMemory(mbApiInterface, apiInterfacePtr, Marshal.SizeOf(mbApiInterface))
        End If

        'videoPanel = New Panel
        'videoPanel.Bounds = mbApiInterface.MB_GetPanelBounds(PluginPanelDock.ApplicationWindow)
        'mbApiInterface.MB_AddPanel(videoPanel, PluginPanelDock.ApplicationWindow)

        createMenuItem() ' Create Menu Item in Tools
        about.PluginInfoVersion = PluginInfoVersion
        about.Name = "BPM Tapper"
        about.Description = "Opens pop-up to manually tap and save to the BPM tag."
        about.Author = "T. de Caussin"
        about.TargetApplication = ""        ' Current only applies to artwork, lyrics or instant messenger name that appears in the provider drop down selector or target Instant Messenger
        about.Type = PluginType.General
        about.VersionMajor = 1              ' Current Plugin Version
        about.VersionMinor = 2
        about.Revision = 1
        about.MinInterfaceVersion = MinInterfaceVersion
        about.MinApiRevision = MinApiRevision
        about.ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents
        about.ConfigurationPanelHeight = 100  ' Height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function
        Return about
    End Function

    Public Function Configure(ByVal panelHandle As IntPtr) As Boolean
        ' save any persistent settings in a sub-folder of this path
        Dim dataPath As String = mbApiInterface.Setting_GetPersistentStoragePath()
        ' panelHandle will only be set if you set about.ConfigurationPanelHeight to a non-zero value
        ' keep in mind the panel width is scaled according to the font the user has selected
        ' if about.ConfigurationPanelHeight is set to 0, you can display your own popup window
        If panelHandle <> IntPtr.Zero Then

            Dim configPanel As Panel = DirectCast(Panel.FromHandle(panelHandle), Panel)

            sLyric = New CheckBox           'Start a new Checkbox
            'prompt.Location = New Point(0, 0)
            sLyric.Text = "Write '#BPMManual' to the Lyricist tag." + Environment.NewLine + _
                          "Recommended as a marker for which files have been tapped manually." + Environment.NewLine + _
                          "Both BPM and Lyricist tags are saved to file." + Environment.NewLine + _
                          "This will not override any valid Lyricist tags."
            sLyric.CheckAlign = ContentAlignment.TopLeft  ' Puts the check in the Top Left Corner
            sLyric.AutoSize = True

            'Debug.Print(writeLyric.ToString)    'check what it's default uninit state is
            If (writeLyric) Then
                sLyric.CheckState = CheckState.Checked
            Else
                sLyric.CheckState = CheckState.Unchecked
            End If

            '' Set the appropriate location of a second item.
            'info1.Location = New System.Drawing.Point( _
            'sLyric.Location.X, _
            'sLyric.Location.Y + sLyric.Height)
            configPanel.Controls.AddRange(New Control() {sLyric})

        End If
        Return True


    End Function


    ' called by MusicBee when the user clicks Apply or Save in the MusicBee Preferences screen.
    ' its up to you to figure out whether anything has changed and needs updating
    Public Sub SaveSettings()
        ' save any persistent settings in a sub-folder of this path
        Dim dataPath As String = mbApiInterface.Setting_GetPersistentStoragePath()
        Dim fileReader As String ' Settings File Content

        '----
        'NOTES:
        '   + The only way I could access sLyrics here was by making the Control Global.
        '       + I wish I didn't have to do that, but all other attempts failed. -Sep22,17T
        writeLyric = sLyric.Checked          'Saved the checkbox state.
        'Pickup with Added an ISTHERE check to prevent Reading a nonexistant file
        '+https://docs.microsoft.com/en-us/dotnet/visual-basic/developing-apps/programming/drives-directories-files/how-to-read-from-text-files
        '+https://docs.microsoft.com/en-us/dotnet/visual-basic/developing-apps/programming/drives-directories-files/how-to-write-text-to-files
        If (My.Computer.FileSystem.FileExists(dataPath + "\BPMTapperSaved.info")) Then  'IF File already exists, THEN Read it
            fileReader = My.Computer.FileSystem.ReadAllText(dataPath + "\BPMTapperSaved.info")
        Else
            'Debug.Print("Settings File Not Found")
            fileReader = ""
        End If

        If (fileReader <> writeLyric.ToString()) Then       'IF File is different from Checkbox State
            'Debug.Print("Writing a Change")
            My.Computer.FileSystem.WriteAllText(dataPath + "\BPMTapperSaved.info", writeLyric.ToString(), False)    'Update the Settings File
        End If

    End Sub

    ' MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
    Public Sub Close(ByVal reason As PluginCloseReason)
    End Sub

    ' uninstall this plugin - clean up any persisted files
    Public Sub Uninstall()
    End Sub

    ' receive event notifications from MusicBee
    ' you need to set about.ReceiveNotificationFlags = PlayerEvents to receive all notifications, and not just the startup event
    Public Sub ReceiveNotification(ByVal sourceFileUrl As String, ByVal type As NotificationType)
        ' perform some action depending on the notification type
        Select Case type
            Case NotificationType.PluginStartup
                ' perform startup initialisation
                Debug.Print("Initialising BPM Plugin")
                mbApiInterface.MB_SendNotification.Invoke(CallbackType.StorageReady)


                '--- Read Settings from File ---'
                'DEFAULT: True
                Dim dataPath As String = mbApiInterface.Setting_GetPersistentStoragePath()
                Dim fileReader As String ' Settings File Content

                writeLyric = True           'Assume it is true.
                If (My.Computer.FileSystem.FileExists(dataPath + "\BPMTapperSaved.info")) Then  'IF File already exists, THEN Read it
                    Debug.Print("Settings File Found.")
                    fileReader = My.Computer.FileSystem.ReadAllText(dataPath + "\BPMTapperSaved.info")
                    If (String.Equals(fileReader, "False")) Then                                'IF File is FALSE, THEN Set checkbox state to match
                        writeLyric = False
                    End If
                End If


                Select Case mbApiInterface.Player_GetPlayState()
                    Case PlayState.Playing, PlayState.Paused
                        ' ...
                End Select
            Case NotificationType.TrackChanged
                Dim artist As String = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist)
                ' ...
                Debug.Print("BPM: Track has Changed")

                sourceUrl = mbApiInterface.NowPlaying_GetFileUrl()
                strBPM = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.BeatsPerMin)     'Get the Current BPM Value. TagValues: 65=Title / 41=BPM / 62 = Lyricist
                strLyricist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Lyricist)   'Get the Current Lyricist Tag.

                BPMForm.Btn_Reset()               'Reset its values
        End Select
    End Sub

    ' return an array of lyric or artwork provider names this plugin supports
    ' the providers will be iterated through one by one and passed to the RetrieveLyrics/ RetrieveArtwork function in order set by the user in the MusicBee Tags(2) preferences screen until a match is found
    Public Function GetProviders() As String()
        Return New String() {}
    End Function

    ' return lyrics for the requested artist/title from the requested provider
    ' only required if PluginType = LyricsRetrieval
    ' return Nothing if no lyrics are found
    Public Function RetrieveLyrics(ByVal sourceFileUrl As String, ByVal artist As String, ByVal trackTitle As String, ByVal album As String, ByVal synchronisedPreferred As Boolean, ByVal provider As String) As String
        Return Nothing
    End Function

    ' return Base64 string representation of the artwork binary data from the requested provider
    ' only required if PluginType = ArtworkRetrieval
    ' return Nothing if no artwork is found
    Public Function RetrieveArtwork(ByVal sourceFileUrl As String, ByVal albumArtist As String, ByVal album As String, ByVal provider As String) As String
        'Return Convert.ToBase64String(artworkBinaryData)
        Return Nothing
    End Function
End Class
