Imports System.IO
Imports Mmultitool.Player

Public Class DlgImportMidiPattern

    Public RetSequence As Sequence


    Private Shared ScreenRefreshTimer As New Timers.Timer(50)       ' 50 ms Screen Timer (= 20 FPS)

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        btnImport.IsEnabled = False
        IsPlayingMark1.Visibility = Visibility.Hidden
        IsPlayingMark2.Visibility = Visibility.Hidden
        AddHandler ScreenRefreshTimer.Elapsed, AddressOf ScreenRefreshTimer_Tick
        ScreenRefreshTimer.Start()
        BtnOpenMidifile_Click(Me, New RoutedEventArgs)
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        Player.RemoveSequence(Sequence1)
        Player.RemoveSequence(Sequence2)
    End Sub

    Private Sub btnImport_Click(sender As Object, e As RoutedEventArgs) Handles btnImport.Click
        CreateEventlist2()
        RetSequence = CreateSequence(evlic2.EventList, evlic2.TPQ, NudNumberOfBeats.Value)
        RetSequence.Name = Path.GetFileNameWithoutExtension(mifir.MidiName)
        DialogResult = True
        Close()
    End Sub

#Region "Screen Refresh"

    Private Sub ScreenRefreshTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        Dispatcher.Invoke(New ScreenRefresh_Delegate(AddressOf ScreenRefresh))
    End Sub

    Public Delegate Sub ScreenRefresh_Delegate()
    Private Sub ScreenRefresh()
        If SequencePlayerBPM <> SsldSequencePlayerBPM1.Value Then
            SsldSequencePlayerBPM1.SetValueSilent(Math.Round(SequencePlayerBPM, 0))
        End If
        If SequencePlayerBPM <> SsldSequencePlayerBPM2.Value Then
            SsldSequencePlayerBPM2.SetValueSilent(Math.Round(SequencePlayerBPM, 0))
        End If
        If Sequence1 IsNot Nothing Then
            If Sequence1.ID <> 0 Then
                IsPlayingMark1.Visibility = Visibility.Visible
            Else
                IsPlayingMark1.Visibility = Visibility.Hidden
            End If
        End If
        If Sequence2 IsNot Nothing Then
            If Sequence2.ID <> 0 Then
                IsPlayingMark2.Visibility = Visibility.Visible
            Else
                IsPlayingMark2.Visibility = Visibility.Hidden
            End If
        End If

    End Sub

#End Region

    Private mifir As New MidifileRead                   ' original file
    Private evlic1 As EventListContainer                ' original evc
    Private numbeats1 As Single                         ' number of beats    
    Private Sequence1 As Sequence                       ' play original evc

    Private evlic2 As EventListContainer                ' edited evc
    Private Sequence2 As Sequence                       ' play edited evc

    Private Sub BtnOpenMidifile_Click(sender As Object, e As RoutedEventArgs) Handles BtnOpenMidifile.Click
        Player.RemoveSequence(Sequence1)
        Player.RemoveSequence(Sequence2)

        btnImport.IsEnabled = False
        evlic1 = Nothing
        evlic2 = Nothing
        ClearInfoValues()

        Dim ofd As New Microsoft.Win32.OpenFileDialog

        ofd.Filter = "Midi files|*.mid"
        If ofd.ShowDialog() = False Then Exit Sub
        Dim ret As Boolean

        Try
            ret = mifir.ReadMidiFile(ofd.FileName)
            'tbTrackViewFilename.Text = ofd.SafeFileName
            'tbTrackViewMessage.Clear()
            LblFilename.Content = ofd.SafeFileName
            If ret = True Then
                LblOpenresult.Content = "Loaded"
            Else
                LblOpenresult.Content = "Error: " & mifir.ErrorText
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message & mifir.ErrorText, "Error reading Midi-File")
        End Try

        If ret = True Then
            evlic1 = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            Dim maxtime As UInteger = MModule1.GetTimeOfNextBeat(mifir.LastTick, mifir.TPQ)  ' round to beat
            numbeats1 = Math.Round(maxtime / mifir.TPQ, 2)
            NudNumberOfBeats.MaximumValue = Math.Round(numbeats1, 0)
            NudNumberOfBeats.Value = NudNumberOfBeats.MaximumValue
            ShowInfoValues()
            CreateEventlist2()

            btnImport.IsEnabled = True
        End If


    End Sub

    Private Sub ShowInfoValues()

        LblFormat.Content = "Fmt: " & mifir.SmfFormat
        LblNumberOfTracks.Content = "NumTracks: " & mifir.TrackList.Count

        LblNumberOfEvents.Content = "NumEvents: " & evlic1.EventList.Count

        LblSequenceName.Content = GetSequenceOrTrackName(evlic1.EventList)

        LblInstrumentName.Content = GetInstrumentName(evlic1.EventList)
        LblProgram.Content = GetFirstProgramChange(evlic1.EventList)
        Dim chan As Byte = GetChannelOfFirstMidiEvent(evlic1.EventList)
        If chan = 9 Then
            LblIsDrum.Content = True
        Else
            LblIsDrum.Content = False
        End If

        LblNumberOfBeats.Content = numbeats1
        LblTempo.Content = GetFirstTempo(evlic1.EventList)
        LblIsMultichannel.Content = mifir.HasMultichannelTrack
    End Sub

    Private Sub ClearInfoValues()
        LblFilename.Content = ""
        LblOpenresult.Content = ""
        LblFormat.Content = ""
        LblNumberOfTracks.Content = ""

        LblSequenceName.Content = ""
        LblInstrumentName.Content = ""
        LblProgram.Content = ""
        LblIsDrum.Content = ""
        LblNumberOfBeats.Content = ""
        LblTempo.Content = ""
        LblIsMultichannel.Content = ""
    End Sub

    Private Sub btnPlay1_Click(sender As Object, e As RoutedEventArgs) Handles BtnPlay1.Click
        StopPlaying()
        If evlic1 Is Nothing Then Exit Sub
        Sequence1 = CreateSequence(evlic1.EventList, evlic1.TPQ)
        Player.PlaySequence(Sequence1, TgbtnLoop1.IsChecked)
    End Sub

    Private Sub BtnStopSequence1_Click(sender As Object, e As RoutedEventArgs) Handles BtnStopSequence1.Click
        Player.RemoveSequence(Sequence1)
    End Sub

    Private Sub BtnPlay2_Click(sender As Object, e As RoutedEventArgs) Handles BtnPlay2.Click
        StopPlaying()
        CreateEventlist2()
        If evlic2 Is Nothing Then Exit Sub
        Sequence2 = CreateSequence(evlic2.EventList, evlic2.TPQ)
        Player.PlaySequence(Sequence2, TgbtnLoop2.IsChecked)
    End Sub

    Private Sub BtnStopSequence2_Click(sender As Object, e As RoutedEventArgs) Handles BtnStopSequence2.Click
        Player.RemoveSequence(Sequence2)
    End Sub

    Private Sub NudNumberOfBeats_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles NudNumberOfBeats.ValueChanged
        StopPlaying()
    End Sub

    Private Sub StopPlaying()
        Player.RemoveSequence(Sequence1)
        Player.RemoveSequence(Sequence2)
    End Sub


    Private Sub SsldSequencePlayerBPM_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles SsldSequencePlayerBPM1.ValueChanged
        If IsLoaded = True Then
            SequencePlayerBPM = SsldSequencePlayerBPM1.Value
        End If
    End Sub

    Private Sub SsldSequencePlayerBPM2_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles SsldSequencePlayerBPM2.ValueChanged
        If IsLoaded = True Then
            SequencePlayerBPM = SsldSequencePlayerBPM2.Value
        End If
    End Sub


    Private Sub BtnRemoveAll_Click(sender As Object, e As RoutedEventArgs) Handles BtnRemoveAll.Click
        CheckAllRemove()
    End Sub

    Private Sub BtnRemoveNone_Click(sender As Object, e As RoutedEventArgs) Handles BtnRemoveNone.Click
        UnCheckAllRemove()
    End Sub

    Private Sub CheckAllRemove()
        CbRemKeyAndTime.IsChecked = True
        CbRemTempo.IsChecked = True
        CbRemText.IsChecked = True
        CbRemPitchB.IsChecked = True
        CbRemProgChg.IsChecked = True
        CbRemCntrlChg.IsChecked = True
        CbRemNoteOff.IsChecked = True
        CbRemOtherMeta.IsChecked = True
        CbRemEndOfTrk.IsChecked = True
    End Sub

    Private Sub UnCheckAllRemove()
        CbRemKeyAndTime.IsChecked = False
        CbRemTempo.IsChecked = False
        CbRemText.IsChecked = False
        CbRemPitchB.IsChecked = False
        CbRemProgChg.IsChecked = False
        CbRemCntrlChg.IsChecked = False
        CbRemNoteOff.IsChecked = False
        CbRemOtherMeta.IsChecked = False
        CbRemEndOfTrk.IsChecked = False
    End Sub

    Private Sub CreateEventlist2()
        If evlic1 Is Nothing Then Exit Sub
        evlic2 = evlic1.Copy

        '---

        If CbRemKeyAndTime.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).TypeX = EventTypeX.KeySignature Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For                    ' element is removed, index is not valid anymore
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.TimeSignature Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        If CbRemTempo.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).TypeX = EventTypeX.SetTempo Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        If CbRemText.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).TypeX = EventTypeX.SequenceOrTrackName Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For                    ' element is removed, index is not valid anymore
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.TextEvent Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.InstrumentName Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.ProgramName Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.DeviceName Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.CopyrightNotice Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.Lyric Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        If CbRemPitchB.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).TypeX = EventTypeX.PitchBend Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        If CbRemProgChg.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).TypeX = EventTypeX.ProgramChange Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        If CbRemCntrlChg.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).TypeX = EventTypeX.ControlChange Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        If CbRemNoteOff.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).TypeX = EventTypeX.NoteOffEvent Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        If CbRemEndOfTrk.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).TypeX = EventTypeX.EndOfTrack Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        If CbRemOtherMeta.IsChecked = True Then
            For i = evlic2.EventList.Count - 1 To 0 Step -1
                If evlic2.EventList(i).Type = EventType.MetaEvent Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.F0SysExEvent Then
                    evlic2.EventList.RemoveAt(i)
                    Continue For
                End If
                If evlic2.EventList(i).TypeX = EventTypeX.F7SysExEvent Then
                    evlic2.EventList.RemoveAt(i)
                End If
            Next
        End If

        '--- if the sequence will be shortened ---

        If NudNumberOfBeats.Value < NudNumberOfBeats.MaximumValue Then

            Dim firsttime As UInteger
            Dim alignedFirst As UInteger

            If evlic2.EventList.Count > 0 Then
                firsttime = evlic2.EventList(0).Time
            End If

            alignedFirst = Player.GetTimeOfPreviousBeat(firsttime)

            '---

            Dim ndx As Integer
            Dim maxtime As UInteger = NudNumberOfBeats.Value * evlic2.TPQ
            maxtime += alignedFirst                     ' offset if empty beat(s) at the beginning

            For Each trev In evlic2.EventList
                If trev.Time >= maxtime Then Exit For
                ndx += 1
            Next

            evlic2.EventList.RemoveRange(ndx, evlic2.EventList.Count - ndx)

        End If


        '---

        LblNumberOfEvents2.Content = "NumEvents: " & evlic2.EventList.Count

    End Sub

End Class
