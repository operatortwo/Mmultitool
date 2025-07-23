
Imports DailyUserControls
Imports Microsoft.VisualBasic.Devices
Imports Mmultitool

Class MainWindow

    Public Shared ScreenRefreshTimer As New Timers.Timer(50)       ' 50 ms Screen Timer (= 20 FPS)

    Private mifir As New MidifileRead

    Public Const HelpUrl As String = "Mmultitool Help.chm"

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        If mifir.ReadMidiFile("Echoes1.mid") = True Then
            tbEvListerFilename.Text = mifir.MidiName
            tbEvListerMessage.Text = GetMidiFileInfo(False)
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            EventLister1.SetEventListContainer(evlic)
        End If

        AddHandler Player.MidiOutShortMsg, AddressOf MidiOutShortMsg
        AddHandler Player.MidiOutLongMsg, AddressOf MidiOutLongMsg
        AddHandler ScreenRefreshTimer.Elapsed, AddressOf ScreenRefreshTimer_Tick

        '--- in case the Application Version has changed, Upgrade the settings ---
        If My.Settings.UpgradeRequired = True Then
            My.Settings.Upgrade()
            My.Settings.UpgradeRequired = False
            My.Settings.Save()
        End If

        Mi_Prefer_MBT_1_1_0.IsChecked = My.Settings.Prefer_MBT_1_1_0
        Prefer_MBT_1_1_0 = Mi_Prefer_MBT_1_1_0.IsChecked

        Dim mcact As Byte = My.Settings.MultichannelConvertAction
        If mcact = 0 Then
            rbPrefer_Multichan_Nothing.IsChecked = True
        ElseIf mcact = 1 Then
            rbPrefer_Multichan_Ask.IsChecked = True
        Else
            rbPrefer_Multichan_Convert.IsChecked = True
        End If


        OpenMidiOutPort()

        TabControl1.SelectedIndex = My.Settings.LastTabIndex

        ScreenRefreshTimer.Start()

        '--- for Developmnt ---

        'Dim win As New DlgNewEvent(EventListWriter1)
        'win.Owner = Application.Current.MainWindow
        'win.StartReferenceItem = New TrackEventX
        'win.ShowDialog()

        Dim mifir2 As New MidifileRead
        If mifir2.ReadMidiFile("Most Events.mid") = True Then
            Dim evlic2 As EventListContainer
            evlic2 = CreateEventListContainer(mifir2.TrackList, mifir2.TPQ)
            EventListWriter1.SetEventListContainer(evlic2)
        End If

        Dim mifir3 As New MidifileRead
        If mifir3.ReadMidiFile("Echoes1.mid") = True Then
            tbTrackViewFilename.Text = mifir.MidiName

            Dim trklist As New Tracklist
            trklist = CreateTracklist(mifir3.TrackList, mifir3.TPQ)

            TrackView1.SetTracklist(trklist)        ' Visualisation / UI
            'TrackView1.CollapseAllTracks()
            Player.SetTracklist(trklist)            ' Player. Can also be used without TrackView.
            ' wait for: StartTrackPlayer()
        End If

        '---

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        StopTrackPlayer()
        StopSequencePlayer()
        ScreenRefreshTimer.Stop()

        My.Settings.LastMidiOut = MidiOut_Selected
        My.Settings.LastTabIndex = TabControl1.SelectedIndex
        My.Settings.Prefer_MBT_1_1_0 = Mi_Prefer_MBT_1_1_0.IsChecked
        If rbPrefer_Multichan_Nothing.IsChecked = True Then
            My.Settings.MultichannelConvertAction = 0
        ElseIf rbPrefer_Multichan_Ask.IsChecked = True Then
            My.Settings.MultichannelConvertAction = 1
        ElseIf rbPrefer_Multichan_Convert.IsChecked = True Then
            My.Settings.MultichannelConvertAction = 2
        End If
        My.Settings.Save()



        MIO._End()                          ' close all MIDI-Ports
    End Sub

#Region "Menu Items"
    Private Sub Mi_File_Exit_Click(sender As Object, e As RoutedEventArgs) Handles Mi_File_Exit.Click
        Close()
    End Sub
    Private Sub Mi_MidiPorts_Click(sender As Object, e As RoutedEventArgs) Handles Mi_MidiPorts.Click
        If IsSequencePlayerRunning = True Then StopTrackPlayer()
        If IsSequencePlayerRunning = True Then StopSequencePlayer()
        Dim dlg As New DlgMidiPorts(Me)
        dlg.Owner = Me
        dlg.ShowDialog()
    End Sub

    Private Sub Mi_Send_GM_On_Click(sender As Object, e As RoutedEventArgs) Handles Mi_Send_GM_On.Click
        ' Universal Non-Real Time SysEx message  F0 7E, device ID 7F (All), Sub ID = 9, GM On = 1        
        ' Turn GM On
        ' Some devices need to be in a certain mode to turn GM on
        Dim GM_ON_sysx As Byte() = {&HF0, &H7E, &H7F, &H9, &H1, &HF7}
        MIO.OutLongMsg(hMidiOut, GM_ON_sysx)
    End Sub

    Private Sub Mi_ResetSound_Click(sender As Object, e As RoutedEventArgs) Handles Mi_ResetSound.Click
        ' All Notes Off, All Sound Off, Reset All Controllers --> Port 0
        Dim stat As Byte

        '-- all notes off 123 (7B)
        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(stat, &H7B, 0)           ' All Notes Off (Bx, 7B, 0)     
        Next

        '-- all sounds off 120 (78h)
        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(stat, &H78, 0)           ' All Sounds Off (Bx, 78, 0)     
        Next

        '-- reset all controllers 121 (79h)
        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(stat, &H79, 0)           ' Controller Reset (Bx, 79, 0)
        Next
    End Sub

    Private Sub Mi_Help_Click(sender As Object, e As RoutedEventArgs) Handles Mi_Help.Click
        Forms.Help.ShowHelp(Nothing, HelpUrl)
    End Sub

    ''' <summary>
    ''' allows calling help from SequencerUI
    ''' </summary>
    ''' <param name="keyword"></param>
    Public Sub SubShowHelp(keyword As String)
        Forms.Help.ShowHelp(Nothing, HelpUrl, Forms.HelpNavigator.KeywordIndex, "Trackview")
        'Forms.Help.ShowHelp(Nothing, HelpUrl, keyword)
    End Sub

    Private Sub Mi_About_Click(sender As Object, e As RoutedEventArgs) Handles Mi_About.Click
        Dim win As New AboutWin
        win.Owner = Me
        win.ShowDialog()
    End Sub

#End Region

#Region "Tab EventLister"
    Private Sub btnEvListerOpenFile_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListerOpenFile.Click
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        ofd.Filter = "Midi files|*.mid"
        If ofd.ShowDialog() = False Then Exit Sub
        Dim ret As Boolean

        Try
            ret = mifir.ReadMidiFile(ofd.FileName)
            tbEvListerFilename.Text = ofd.SafeFileName
            tbEvListerMessage.Clear()
            If ret = True Then
                Dim converted As Boolean = ConditionalMultichannelConversion(mifir)
                tbEvListerMessage.Text = GetMidiFileInfo(converted)
            Else
                tbEvListerMessage.Text = "Errortext:" & mifir.ErrorText
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message & mifir.ErrorText, "Error reading Midi-File")
        End Try

        If ret = True Then
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            EventLister1.SetEventListContainer(evlic)
        End If
    End Sub

    Private Sub btnTbEvListerMessageClear_Click(sender As Object, e As RoutedEventArgs) Handles btnTbEvListerMessageClear.Click
        tbEvListerMessage.Clear()
    End Sub

    Private Sub ShowMidifileInfo()
        EvListerMessage("File loaded: " & mifir.MidiName)
        EvListerMessage("Format: " & mifir.SmfFormat & " - " & "Time division: " & mifir.TPQ)
        EvListerMessage("Tracks count: " & mifir.NumberOfTracks)
    End Sub

    Private Function GetMidiFileInfo(converted As Boolean) As String
        Dim str As String
        str = "File loaded: " & mifir.MidiName & vbCrLf
        str = str & "Format: " & mifir.SmfFormat & " - " & "Time division: " & mifir.TPQ & vbCrLf

        If converted = False Then
            If mifir.HasMultichannelTrack = False Then
                str = str & "Tracks count: " & mifir.NumberOfTracks & vbCrLf
            Else
                str = str & "Tracks count: " & mifir.NumberOfTracks & "  { Multichannel }" & vbCrLf
            End If
        Else
            str = str & "Tracks count: " & mifir.NumberOfTracks & "  { Converted }" & vbCrLf
        End If

        Return str
    End Function

    Private Sub EvListerMessage(str As String)
        If tbEvListerMessage.LineCount > 50 Then tbEvListerMessage.Clear()
        tbEvListerMessage.AppendText(str & vbCrLf)
        tbEvListerMessage.ScrollToEnd()
    End Sub

    ''' <summary>
    ''' If the tracklist contains multichannel tracks, the MultichannelConvertAction settings are checked 
    ''' and converted if desired.
    ''' </summary>
    ''' <param name="mifir"></param>
    ''' <returns>True if a conversion has occurred and was successful.</returns>
    Private Function ConditionalMultichannelConversion(mifir As MidifileRead) As Boolean
        If mifir Is Nothing Then Return False
        If mifir.HasMultichannelTrack = False Then Return False
        If rbPrefer_Multichan_Nothing.IsChecked = True Then Return False

        If rbPrefer_Multichan_Convert.IsChecked = True Then
            Return mifir.ConvertMultichannelTracks()
        End If

        If rbPrefer_Multichan_Ask.IsChecked = True Then

            Dim ret As QuestionWindowResult
            ret = QuestionWindow.Show(Me, "Multichannel Tracks detected. Convert to Singlechannel Tracks ?",
                                "Midifile", QuestionWindowButton.YesNoCancel)

            If ret = QuestionWindowResult.Yes Then
                Return mifir.ConvertMultichannelTracks()
            Else
                Return False
            End If

        End If

        Return False
    End Function


#End Region

    Private Sub ScreenRefreshTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        Dispatcher.Invoke(New ScreenRefresh_Delegate(AddressOf ScreenRefresh))
    End Sub

    Public Delegate Sub ScreenRefresh_Delegate()
    Private Sub ScreenRefresh()


        If EventLister1.IsVisible Then
            Dim time As Long = Player.SequencePlayerTime
            lblEvlSequencePlayerPosition.Content = TimeTo_MBT(time, PlayerTPQ)
            If SequencePlayerBPM <> ssldEvlSequencePlayerBPM.Value Then
                ssldEvlSequencePlayerBPM.SetValueSilent(Math.Round(SequencePlayerBPM, 0))
            End If

        ElseIf EventListWriter1.IsVisible Then
            Dim time As Long = Player.SequencePlayerTime
            lblEvlwrSequencePlayerPosition.Content = TimeTo_MBT(time, PlayerTPQ)
            If SequencePlayerBPM <> ssldEvlwrSequencePlayerBPM.Value Then
                ssldEvlwrSequencePlayerBPM.SetValueSilent(Math.Round(SequencePlayerBPM, 0))
            End If

        ElseIf TrackView1.IsVisible Then
            TrackView1.ScreenRefresh()
        End If

        If DebugTab.IsVisible Then
            DebugTab_ScreenRefresh()
        End If

        If SequencePad1.IsVisible Then
            SequencePad1.Screenrefresh()
        End If

    End Sub

#Region "SequencePlayer"
    Private Sub btnEvlStartSequencePlayer_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlStartSequencePlayer.Click
        StartSequencePlayer()
    End Sub
    Private Sub btnEvlStopSequencePlayer_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlStopSequencePlayer.Click
        StopSequencePlayer()
    End Sub
    Private Sub btnEvlRestartSequencePlayer_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlRestartSequencePlayer.Click
        Restart_SequencePlayer()
    End Sub

    Private Sub btnEvlwrStartSequencePlayer_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlwrStartSequencePlayer.Click
        StartSequencePlayer()
    End Sub
    Private Sub btnEvlistWrStop_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlistWrStop.Click
        StopSequencePlayer()
        'Set_SequencerTime(0)
    End Sub
    Private Sub btnEvlwrRestartSequencePlayer_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlwrRestartSequencePlayer.Click
        Restart_SequencePlayer()
    End Sub
    Private Sub btnEvlistWrStopSequence_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlistWrStopSequence.Click
        EventListWriter1.StopCurrentSequence()
    End Sub
    Private Sub EvlBpmSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldEvlSequencePlayerBPM.ValueChanged
        SequencePlayerBPM = ssldEvlSequencePlayerBPM.Value
    End Sub
    Private Sub EvlwrBpmSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldEvlwrSequencePlayerBPM.ValueChanged
        SequencePlayerBPM = ssldEvlwrSequencePlayerBPM.Value
    End Sub


#End Region

    Private Sub MainVolumeSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles MainVolumeSlider.ValueChanged
        ' Master Volume for GM Instruments, Universal Real Time SysEx
        'F0 7F 7F 04 01 ll mm F7        ' ll = volume LSB, mm = volume MSB

        Dim MasterVolume_sysex As Byte() = {&HF0, &H7F, &H7F, &H4, &H1, &H0, &H64, &HF7}

        MasterVolume_sysex(6) = CByte(MainVolumeSlider.Value)
        MidiOutLongMsg(MasterVolume_sysex)
    End Sub

    Private Sub btnEvListerSelectAll_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListerSelectAll.Click
        EventLister1.SelectAll()
    End Sub

    Private Sub btnEvListerPlaySelected_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListerPlaySelected.Click
        If EventLister1.GetSelectedItems.Count = 0 Then
            EventLister1.SelectAll()
        End If
        EventLister1.StopCurrentSequence()
        EventLister1.PlaySelectedItems(tgbtnEvListerLoop.IsChecked)
    End Sub

    Private Sub btnEvlStopSequence_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlStopSequence.Click
        EventLister1.StopCurrentSequence()
    End Sub

    Private Sub btnEvListWrSelectAll_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListWrSelectAll.Click
        EventListWriter1.SelectAll()
    End Sub

    Private Sub btnEvListWrPlaySelected_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListWrPlaySelected.Click
        If EventListWriter1.GetSelectedItems.Count = 0 Then
            EventListWriter1.SelectAll()
        End If
        EventListWriter1.StopCurrentSequence()
        EventListWriter1.PlaySelectedItems(tgbtnEvListWrLoop.IsChecked)
    End Sub

    Private Sub btnSaveAll_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveListed.Click
        Dim evlic As EventListContainer
        Dim tpq As Integer = EventListWriter1.EvliTPQ
        'evlic = CreateEventListContainer(EventListWriter1.TrackEvents, tpq)        ' All events
        ' Events in DataGrid (All events filtered)
        evlic = CreateEventListContainer(EventListWriter1.GetListedItems, EventListWriter1.EvliTPQ)
        SaveToMidifile(evlic)
    End Sub

    Private Sub btnSaveSelected_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveSelected.Click
        Dim evlic As EventListContainer
        Dim tpq As Integer = EventListWriter1.EvliTPQ
        evlic = CreateEventListContainer(EventListWriter1.GetSelectedItems, tpq)
        SaveToMidifile(evlic)
    End Sub

    Private Sub SaveToMidifile(evlic As EventListContainer)

        Dim dstTpq As Integer = 120
        Dim dstFormat As Byte = 1

        '--- Write Options (TPQ, Format) ---

        Dim win As New DlgMidiFileWriteOptions
        win.Owner = Me
        win.SrcNumberOfEvents = evlic.EventList.Count
        win.SrcNumberOfTracks = evlic.GetNumberOfTracks
        win.ShowDialog()
        If win.DialogResult = True Then
            dstTpq = win.RetTPQ
            dstFormat = win.RetFormat
        Else
            Exit Sub
        End If

        '--- SaveFileDialog ---

        Dim sfd As New Microsoft.Win32.SaveFileDialog
        'sfd.InitialDirectory = CompositionsDirectory
        sfd.Filter = "MIDI files|*.mid"
        sfd.DefaultExt = ".mid"

        Dim ret As Boolean?
        ret = sfd.ShowDialog()
        If ret = False Then Exit Sub

        '--- Write MidiFile ---

        Dim mifiw As New MidiFileWrite
        If mifiw.WriteMidiFile(evlic, dstTpq, sfd.FileName, dstFormat) = True Then
            Dim msg As String = mifiw.BytesWritten.ToString("N0") & " bytes written to:" & vbCrLf & sfd.FileName
            Dim bg As New SolidColorBrush(Color.FromRgb(&HE0, &HEE, &HE0))
            MessageWindow.Show(Me, msg, "Create MidiFile", MessageIcon.StatusOk, bg)
        Else
            ' show error
            Dim errstr As String = mifiw.ErrorCode.ToString()
            Dim bg As New SolidColorBrush(Color.FromRgb(&HFF, &HE5, &HE5))
            MessageWindow.Show(Me, errstr & vbCrLf & mifiw.ErrorMessage,
                               "MidiFile Write Error", MessageIcon.Error, bg)
        End If

    End Sub

    Private Sub btnEvListWrOpenFile_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListWrOpenFile.Click
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        ofd.Filter = "Midi files|*.mid"
        If ofd.ShowDialog() = False Then Exit Sub
        Dim ret As Boolean

        Try
            ret = mifir.ReadMidiFile(ofd.FileName)
            tbEvListWriterFilename.Text = ofd.SafeFileName
            tbEvListWriterMessage.Clear()
            If ret = True Then
                Dim converted As Boolean = ConditionalMultichannelConversion(mifir)
                tbEvListWriterMessage.Text = GetMidiFileInfo(converted)
            Else
                tbEvListWriterMessage.Text = "Errortext:" & mifir.ErrorText
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message & mifir.ErrorText, "Error reading Midi-File")
        End Try

        If ret = True Then
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            EventListWriter1.SetEventListContainer(evlic)
        End If

    End Sub


    Private Sub btnTrackViewOpenFile_Click(sender As Object, e As RoutedEventArgs) Handles btnTrackViewOpenFile.Click
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        ofd.Filter = "Midi files|*.mid"
        If ofd.ShowDialog() = False Then Exit Sub
        Dim ret As Boolean

        Try
            ret = mifir.ReadMidiFile(ofd.FileName)
            tbTrackViewFilename.Text = ofd.SafeFileName
            tbTrackViewMessage.Clear()
            If ret = True Then
                tbTrackViewFilename.Text = mifir.MidiName
                Dim converted As Boolean = ConditionalMultichannelConversion(mifir)
                tbTrackViewMessage.Text = GetMidiFileInfo(converted)
            Else
                tbTrackViewMessage.Text = "Errortext:" & mifir.ErrorText
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message & mifir.ErrorText, "Error reading Midi-File")
        End Try


        Dim trklist As New Tracklist
        'trklist.Tracks.Add(New Track)
        'trklist.Tracks(0).Events.Add(New TrackEventX)
        If ret = True Then
            trklist = CreateTracklist(mifir.TrackList, mifir.TPQ)
            ' set trklist to trklist view

            TrackView1.SetTracklist(trklist)        ' Visualisation / UI
            'TrackView1.CollapseAllTracks()
            Player.SetTracklist(trklist)            ' Player. Can also be used without TrackView.
            ' wait for: StartTrackPlayer()

        End If
    End Sub

    Private Sub BtnTrackviewHelp_Click(sender As Object, e As RoutedEventArgs) Handles BtnTrackviewHelp.Click
        Forms.Help.ShowHelp(Nothing, HelpUrl, Forms.HelpNavigator.KeywordIndex, "Trackview")
    End Sub

    Private Sub Mi_Prefer_MBT_1_1_0_Click(sender As Object, e As RoutedEventArgs) Handles Mi_Prefer_MBT_1_1_0.Click
        If Mi_Prefer_MBT_1_1_0.IsChecked = True Then
            Prefer_MBT_1_1_0 = True
        Else
            Prefer_MBT_1_1_0 = False            ' Measure:Beat:Ticks starts at 0:0:0 in TrackView
        End If


    End Sub

#Region "Debug"

    Private Sub DebugTab_ScreenRefresh()
        Dim time As Long = Player.SequencePlayerTime
        Dim cont As String = TimeTo_MBT(time, PlayerTPQ)
        If LblDbgSequencePlayerPosition.Content <> cont Then
            LblDbgSequencePlayerPosition.Content = TimeTo_MBT(time, PlayerTPQ)
        End If
        If SequencePlayerBPM <> SsldDbgSequencePlayerBPM.Value Then
            SsldDbgSequencePlayerBPM.SetValueSilent(Math.Round(SequencePlayerBPM, 0))
        End If

        LblDbgSeqListCount.Content = Player.SequenceList.Count


    End Sub

    Private Sub BtnShowClipboard_Click(sender As Object, e As RoutedEventArgs) Handles BtnShowClipboard.Click
        Dim contevlic As Boolean
        Dim tp As Type = GetType(EventListContainer)
        contevlic = Clipboard.ContainsData(tp.FullName)

        If contevlic = False Then
            MessageWindow.Show("The Clipboard does not contain an EventList", "Show Clipboard")
            Exit Sub
        End If

        Dim evlic As EventListContainer = Clipboard.GetData(tp.FullName)
        Dim win As New DlgClipboardContent
        win.Owner = Me
        win.EventLister1.SetEventListContainer(evlic)
        If Prefer_MBT_1_1_0 Then
            win.EventLister1.DesiredTimeFormat = EventLister.TimeFormat.MBT_1_based
        End If
        win.ShowDialog()
    End Sub

    Private Sub BtnDbgStartSequencePlayer_Click(sender As Object, e As RoutedEventArgs) Handles BtnDbgStartSequencePlayer.Click
        StartSequencePlayer()
    End Sub

    Private Sub BtnDbgStopSequencePlayer_Click(sender As Object, e As RoutedEventArgs) Handles BtnDbgStopSequencePlayer.Click
        StopSequencePlayer()
    End Sub

    Private Sub BtnDbgRestartSequencePlayer_Click(sender As Object, e As RoutedEventArgs) Handles BtnDbgRestartSequencePlayer.Click
        Restart_SequencePlayer()
    End Sub

    Private Sub SsldDbgSequencePlayerBPM_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles SsldDbgSequencePlayerBPM.ValueChanged
        SequencePlayerBPM = SsldDbgSequencePlayerBPM.Value
    End Sub



#End Region


End Class
