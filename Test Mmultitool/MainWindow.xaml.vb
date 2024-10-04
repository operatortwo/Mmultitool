Imports System.Data
Imports DailyUserControls
Imports Mmultitool

Class MainWindow

    Public Shared ScreenRefreshTimer As New Timers.Timer(50)       ' 80 ms Screen Timer (= 12.5 FPS)

    Private mifir As New MidifileRead



    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        If mifir.ReadMidiFile("Echoes1.mid") = True Then
            ShowMidifileInfo()
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            EventLister1.SetEventListContainer(evlic)
        End If

        AddHandler Sequencer.MidiOutShortMsg, AddressOf MidiOutShortMsg
        AddHandler Sequencer.MidiOutLongMsg, AddressOf MidiOutLongMsg
        AddHandler ScreenRefreshTimer.Elapsed, AddressOf ScreenRefreshTimer_Tick

        '--- in case the Application Version has changed, Upgrade the settings ---
        If My.Settings.UpgradeRequired = True Then
            My.Settings.Upgrade()
            My.Settings.UpgradeRequired = False
            My.Settings.Save()
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
        '---
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        StopSequencer()
        ScreenRefreshTimer.Stop()

        My.Settings.LastMidiOut = MidiOut_Selected
        My.Settings.LastTabIndex = TabControl1.SelectedIndex
        My.Settings.Save()

        MIO._End()                          ' close all MIDI-Ports
    End Sub

#Region "Menu Items"
    Private Sub Mi_File_Exit_Click(sender As Object, e As RoutedEventArgs) Handles Mi_File_Exit.Click
        Close()
    End Sub
    Private Sub Mi_MidiPorts_Click(sender As Object, e As RoutedEventArgs) Handles Mi_MidiPorts.Click
        If IsSequencerRunning = True Then StopSequencer()
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

        '-- all sound off 120 (78h)
        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(stat, &H78, 0)           ' All Notes Off (Bx, 78, 0)            
        Next

        '-- reset all controllers 121 (79h)
        For i = 0 To &HF
            stat = CByte(i Or &HB0)
            MidiOutShortMsg(stat, &H79, 0)           ' All Notes Off (Bx, 79, 0)            
        Next
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
                ShowMidifileInfo()
            Else
                EvListerMessage("Errortext:" & mifir.ErrorText)
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

    Private Sub EvListerMessage(str As String)
        If tbEvListerMessage.LineCount > 50 Then tbEvListerMessage.Clear()
        tbEvListerMessage.AppendText(str & vbCrLf)
        tbEvListerMessage.ScrollToEnd()
    End Sub

#End Region

    Private Sub ScreenRefreshTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        Dispatcher.Invoke(New ScreenRefresh_Delegate(AddressOf ScreenRefresh))
    End Sub

    Public Delegate Sub ScreenRefresh_Delegate()
    Private Sub ScreenRefresh()
        Dim time As Long = CLng(Sequencer.SequencerTime)
        lblSequencerPosition.Content = TimeTo_MBT_0(time)


        '--- check if BPM was changed by SetTempo event
        If SequencerBPM <> BpmSlider.Value Then
            ' it's more clear to have no decimal digits on the slider, even when some files
            ' can SetTempo to BPM with 2 decimal digits.
            ' (f.e 109.46 BPM, but most creators use Integer values)
            BpmSlider.SetValueSilent(Math.Round(SequencerBPM, 0))       ' update without ValueChanged event
        End If

    End Sub


    Private Sub btnRestartSequencer_Click(sender As Object, e As RoutedEventArgs) Handles btnRestartSequencer.Click
        Set_SequencerTime(0)
    End Sub
    Private Sub btnStopSequencer_Click(sender As Object, e As RoutedEventArgs) Handles btnStopSequencer.Click
        StopSequencer()
    End Sub
    Private Sub btnStartSequencer_Click(sender As Object, e As RoutedEventArgs) Handles btnStartSequencer.Click
        StartSequencer()
    End Sub

    Private Sub BpmSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles BpmSlider.ValueChanged
        SequencerBPM = BpmSlider.Value
    End Sub

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
        EventLister1.PlaySelectedItems(tgbtnEvListerLoop.IsChecked)
    End Sub

    Private Sub btnEvlisterStop_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlisterStop.Click
        StopSequencer()
        'Set_SequencerTime(0)
    End Sub

    Private Sub btnTestMidiWrite_Click(sender As Object, e As RoutedEventArgs) Handles btnTestMidiWrite.Click
        Dim evlic As EventListContainer
        evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
        Dim mifiw As New MidiFileWrite

        If mifiw.CreateMidiFile(evlic, 120, "TestMidi.mid") = True Then
            MessageBox.Show("The file 'TestMidi.mid' was written successfully", "Create MidiFile")
        End If

    End Sub

    Private Sub btnEvListWrSelectAll_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListWrSelectAll.Click
        EventListWriter1.SelectAll()
    End Sub

    Private Sub btnEvListWrPlaySelected_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListWrPlaySelected.Click
        EventListWriter1.PlaySelectedItems(tgbtnEvListWrLoop.IsChecked)
    End Sub

    Private Sub btnEvlistWrStop_Click(sender As Object, e As RoutedEventArgs) Handles btnEvlistWrStop.Click
        StopSequencer()
        'Set_SequencerTime(0)
    End Sub

    Private Sub btnSaveAs_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveAs.Click
        Dim evlic As EventListContainer
        Dim tpq As Integer = EventListWriter1.EvliTPQ
        evlic = CreateEventListContainer(EventListWriter1.TrackEvents, tpq)

        Dim sfd As New Microsoft.Win32.SaveFileDialog
        '--- Create folder if not exists
        'If Not IO.Directory.Exists(CompositionsDirectory) Then
        'IO.Directory.CreateDirectory(CompositionsDirectory)
        'End If
        '---

        'sfd.InitialDirectory = CompositionsDirectory
        sfd.Filter = "MIDI files|*.mid"
        sfd.DefaultExt = ".mid"

        Dim ret As Boolean?
        ret = sfd.ShowDialog()
        If ret = False Then Exit Sub


        Dim mifiw As New MidiFileWrite
        If mifiw.CreateMidiFile(evlic, tpq, sfd.FileName) = True Then
            MessageWindow.Show(Me, "The file" & vbCrLf & sfd.FileName & vbCrLf & "was written successfully",
                               "Create MidiFile", MessageIcon.StatusOk, Brushes.AliceBlue)
        End If


    End Sub

    Private Sub btnEvListWrOpenFile_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListWrOpenFile.Click
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        ofd.Filter = "Midi files|*.mid"
        If ofd.ShowDialog() = False Then Exit Sub
        Dim ret As Boolean

        Try
            ret = mifir.ReadMidiFile(ofd.FileName)
            tbEvListerFilename.Text = ofd.SafeFileName
            tbEvListerMessage.Clear()
            If ret = True Then
                ShowMidifileInfo()
            Else
                EvListerMessage("Errortext:" & mifir.ErrorText)
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
End Class
