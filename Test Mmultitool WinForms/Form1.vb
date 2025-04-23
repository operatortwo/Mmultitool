Imports System.Windows
Imports System.Windows.Forms.Integration
Imports System.Windows.Threading
Imports Mmultitool

Public Class Form1

    Public Shared ScreenRefreshTimer As New Timers.Timer(50)       ' 80 ms Screen Timer (= 12.5 FPS)

    Private mifir As New MidifileRead

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If mifir.ReadMidiFile("Echoes1.mid") = True Then
            ShowMidifileInfo()
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            EventLister1.SetEventListContainer(evlic)
        End If

        AddHandler Player.MidiOutShortMsg, AddressOf MidiOutShortMsg
        AddHandler Player.MidiOutLongMsg, AddressOf MidiOutLongMsg
        AddHandler ScreenRefreshTimer.Elapsed, AddressOf ScreenRefreshTimer_Tick

        OpenMidiOutPort()

        ScreenRefreshTimer.Start()
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ScreenRefreshTimer.Stop()
        WinForms_Mmultitool_end()                           ' stop Player Timer if running
        MIO._End()                                          ' close all MIDI-Ports
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click

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
            Player.StopSequencePlayer()
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            EventLister1.SetEventListContainer(evlic)
        End If
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        tbEvListerMessage.Clear()
    End Sub

    Private Sub ShowMidifileInfo()
        EvListerMessage("File loaded: " & mifir.MidiName)
        EvListerMessage("Format: " & mifir.SmfFormat & " - " & "Time division: " & mifir.TPQ)
        EvListerMessage("Tracks count: " & mifir.NumberOfTracks)
    End Sub

    Private Sub EvListerMessage(str As String)

        If tbEvListerMessage.Lines.Count > 50 Then tbEvListerMessage.Clear()
        tbEvListerMessage.AppendText(str & vbCrLf)
        tbEvListerMessage.ScrollToCaret()
    End Sub

    Private Sub ScreenRefreshTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Invoke(New ScreenRefresh_Delegate(AddressOf ScreenRefresh))
        Catch
            ' sometimes Tick after the Timer was stopped and Form1 disposed
        End Try
    End Sub

    Public Delegate Sub ScreenRefresh_Delegate()
    Private Sub ScreenRefresh()


        'If EventLister1.IsVisible Then
        Dim time As Long = Player.SequencePlayerTime
        lblSequencePlayerPosition.Text = TimeTo_MBT(time, PlayerTPQ)
        'If SequencePlayerBPM <> ssldSequencePlayerBPM.Value Then
        '    ssldSequencePlayerBPM.SetValueSilent(Math.Round(SequencePlayerBPM, 0))
        'End If

    End Sub

    Private Sub btnStartSequencer_Click(sender As Object, e As EventArgs) Handles btnStartSequencer.Click
        StartSequencePlayer()
    End Sub

    Private Sub btnStopSequencer_Click(sender As Object, e As EventArgs) Handles btnStopSequencer.Click
        StopSequencePlayer()
    End Sub

    Private Sub btnRestartSequencer_Click(sender As Object, e As EventArgs) Handles btnRestartSequencer.Click
        Set_SequencePlayerTime(0)
    End Sub

    Private Sub btnPlaySelected_Click(sender As Object, e As EventArgs) Handles btnPlaySelected.Click
        If EventLister1.GetSelectedItems.Count = 0 Then
            EventLister1.SelectAll()
        End If

        EventLister1.PlaySelectedItems(cbDoLoop.Checked)
    End Sub

    Private Sub Mi_File_Exit_Click(sender As Object, e As EventArgs) Handles Mi_File_Exit.Click
        Close()
    End Sub
End Class
