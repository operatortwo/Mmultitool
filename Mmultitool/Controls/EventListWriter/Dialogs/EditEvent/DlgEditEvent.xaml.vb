Imports System.Windows.Controls.Primitives
Imports DailyUserControls
Imports Mmultitool.EventListWriter
Public Class DlgEditEvent

    Private TrevX As TrackEventX
    Private Evliw As EventListWriter

    Private CurrentEvent As TrackEventX
    Private EditedEvent As TrackEventX

    Private ChangedBgBrush As Brush
    Private UnChangedBgBrush As Brush

    Public Property LocalTPQ As Integer = 1

    Public Sub New(trev As TrackEventX, instance As EventListWriter)
        TrevX = trev
        Evliw = instance

        LocalTPQ = Evliw.EvliTPQ

        InitializeComponent()               ' required for the designer
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        'Evliw.CollectionView.MoveCurrentTo(TrevX)
        CurrentIndex = Evliw.CollectionView.CurrentPosition

        'GetCurrentEvent()
        'CurrentEvent = Evliw.CollectionView.CurrentItem
        'ShowEventInfo()


        lblPosition.Content = CurrentIndex
        lblPositionMax.Content = Evliw.CollectionView.Count - 1

        ScrollBar1.Maximum = Evliw.CollectionView.Count - 1
        ScrollBar1.Value = Evliw.CollectionView.CurrentPosition

        ChangedBgBrush = TryCast(Me.Resources("ChangedBgBrush"), Brush)           ' defined in xaml
        UnChangedBgBrush = TryCast(Me.Resources("UnChangedBgBrush"), Brush)           ' defined in xaml


        If Evliw.DesiredTimeFormat = EventListWriter.TimeFormat.MBT_1_based Then
            MBT_Editor1.IsMBT_Base1 = True
            MBT_InputBox1.IsMBT_Base1 = True
        End If

    End Sub

    Private evtinfo As EventTypeX_Information

    Private _CurrentIndex As Integer
    Private Property CurrentIndex As Integer
        Get
            Return _CurrentIndex
        End Get
        Set(value As Integer)
            _CurrentIndex = value
            GetCurrentEvent()
            EditedEvent = CurrentEvent.Copy
            EditCompare()

            evtinfo = GetEventTypeX_Info(CurrentEvent.TypeX)
            tblEventInfo.Text = evtinfo.Info                    ' show Info for the event
            ShowEventInfo()

            SetEditTab()
            SetEditData()
        End Set
    End Property

    Private Sub GetCurrentEvent()
        CurrentEvent = Nothing
        If Evliw Is Nothing Then Exit Sub
        If CurrentIndex < 0 Then Exit Sub
        If CurrentIndex >= Evliw.CollectionView.Count Then Exit Sub
        CurrentEvent = Evliw.CollectionView.GetItemAt(CurrentIndex)
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If HasChanges = True Then
            Dim result As QuestionWindowResult
            result = QuestionWindow.Show(Me, "Do you want to save the changes ?", "Closing Dialog", QuestionWindowButton.YesNoCancel, Brushes.WhiteSmoke)
            If result = QuestionWindowResult.Yes Then
                SaveChanges()
            ElseIf result = QuestionWindowResult.No Then

            ElseIf result = QuestionWindowResult.Cancel Then
                e.Cancel = True
            End If
        End If
    End Sub

#Region "Navigation"

    Private Sub ScrollBar1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim scrollBar = CType(sender, ScrollBar)
        Dim newvalue As Double = Math.Round(e.NewValue, 0)
        If newvalue > scrollBar.Maximum Then
            newvalue = scrollBar.Maximum
        End If
        scrollBar.Value = newvalue
        CurrentIndex = newvalue
        If lblPosition IsNot Nothing Then
            lblPosition.Content = ScrollBar1.Value
        End If

    End Sub

    Private Sub ScrollBar1_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles ScrollBar1.PreviewMouseLeftButtonDown
        ScrollBar1.Focus()
    End Sub

    Private Sub Navigate_Click(sender As Object, e As RoutedEventArgs) Handles Navigate.Click
        ScrollBar1.Focus()
    End Sub

    Private NavigateFocusBrush As New SolidColorBrush(Color.FromArgb(&HFF, &H56, &H9D, &HE5))
    Private Sub ScrollBar1_GotFocus(sender As Object, e As RoutedEventArgs) Handles ScrollBar1.GotFocus
        ScrollFocusRectangle.Stroke = NavigateFocusBrush
    End Sub

    Private Sub ScrollBar1_LostFocus(sender As Object, e As RoutedEventArgs) Handles ScrollBar1.LostFocus
        ScrollFocusRectangle.Stroke = Nothing
    End Sub

    'Private Sub Navigate_KeyDown(sender As Object, e As KeyEventArgs) Handles Navigate.KeyDown

    '    If e.Key = Key.Down Then
    '        ScrollBar1.Value = ScrollBar1.Value + 1
    '        e.Handled = True
    '    ElseIf e.Key = Key.Up Then
    '        ScrollBar1.Value = ScrollBar1.Value - 1
    '        e.Handled = True
    '    ElseIf e.Key = Key.Home Then
    '        ScrollBar1.Value = 0
    '        e.Handled = True
    '    ElseIf e.Key = Key.End Then
    '        ScrollBar1.Value = Evliw.TrackEvents.Count - 1
    '        e.Handled = True
    '    ElseIf e.Key = Key.PageUp Then
    '        ScrollBar1.Value = ScrollBar1.Value - ScrollBar1.LargeChange
    '        e.Handled = True
    '    ElseIf e.Key = Key.PageDown Then
    '        ScrollBar1.Value = ScrollBar1.Value + ScrollBar1.LargeChange
    '        e.Handled = True
    '    End If
    'End Sub

    'Private NavigateFocusBrush As New SolidColorBrush(Color.FromArgb(&HFF, &H56, &H9D, &HE5))
    'Private Sub Navigate_GotFocus(sender As Object, e As RoutedEventArgs) Handles Navigate.GotFocus
    '    Navigate.BorderBrush = NavigateFocusBrush
    'End Sub

    'Private Sub Navigate_LostFocus(sender As Object, e As RoutedEventArgs) Handles Navigate.LostFocus
    '    Navigate.BorderBrush = Nothing
    'End Sub


    Private Sub MBT_InputBox1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles MBT_InputBox1.ValueChanged
        MBT_Editor1.ChangeCurrentValue(e.NewValue)
    End Sub

    Private Sub btnResetTime_Click(sender As Object, e As RoutedEventArgs) Handles btnResetTime.Click
        MBT_Editor1.ResetToOriginalValue()
    End Sub

#End Region

    Private Sub SetEditTab()

        Select Case CurrentEvent.TypeX
            Case EventTypeX.NoteOffEvent
                TabControlEdit.SelectedItem = Ti_NoteOff
            Case EventTypeX.NoteOnEvent
                TabControlEdit.SelectedItem = Ti_NoteOn
            Case EventTypeX.PolyKeyPressure
                TabControlEdit.SelectedItem = Ti_PolyKeyPressure
            Case EventTypeX.ControlChange
                TabControlEdit.SelectedItem = Ti_ControlChange
            Case EventTypeX.ProgramChange
                TabControlEdit.SelectedItem = Ti_ProgramChange
            Case EventTypeX.ChannelPressure
                TabControlEdit.SelectedItem = Ti_ChannelPressure
            Case EventTypeX.PitchBend
                TabControlEdit.SelectedItem = Ti_PitchBend
            Case EventTypeX.F0SysExEvent
                TabControlEdit.SelectedItem = Ti_F0SysEx
            Case EventTypeX.F7SysExEvent
                TabControlEdit.SelectedItem = Ti_F7SysEx
            Case EventTypeX.SequenceNumber
                TabControlEdit.SelectedItem = Ti_SequenceNumber
            Case EventTypeX.TextEvent
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Text"
            Case EventTypeX.CopyrightNotice
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Copyright Notice"
            Case EventTypeX.SequenceOrTrackName
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Sequence- or Track-Name"
            Case EventTypeX.InstrumentName
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Instrument Name"
            Case EventTypeX.Lyric
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Lyric"
            Case EventTypeX.Marker
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Marker"
            Case EventTypeX.CuePoint
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Cue Point"
            Case EventTypeX.ProgramName
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Program Name"
            Case EventTypeX.DeviceName
                TabControlEdit.SelectedItem = Ti_MetaText
                GroupBox_MetaText.Header = "Device Name"
            Case EventTypeX.MIDIChannelPrefix
                TabControlEdit.SelectedItem = Ti_MidiChannelPrefix
            Case EventTypeX.MIDIPortPrefix
                TabControlEdit.SelectedItem = Ti_MidiPortPrefix
            Case EventTypeX.EndOfTrack
                TabControlEdit.SelectedItem = Ti_EndOfTrack
            Case EventTypeX.SetTempo
                TabControlEdit.SelectedItem = Ti_SetTempo
            Case EventTypeX.SMPTEOffset
                TabControlEdit.SelectedItem = Ti_SMPTEOffset
            Case EventTypeX.TimeSignature
                TabControlEdit.SelectedItem = Ti_TimeSignature
            Case EventTypeX.KeySignature
                TabControlEdit.SelectedItem = Ti_KeySignature
            Case EventTypeX.SequencerSpecific
                TabControlEdit.SelectedItem = Ti_SequencerSpecific
            Case EventTypeX.Unkown
                TabControlEdit.SelectedItem = Ti_Unknown
            Case Else
                ' not listed --> unknown
                TabControlEdit.SelectedItem = Ti_Unknown
        End Select

    End Sub

    Private Sub SetEditData()
        nudTrack.MaximumValue = Evliw.TrackList.Count - 1
        nudTrack.Value = CurrentEvent.TrackNumber

        nudChannel.Value = CurrentEvent.Channel

        Select Case CurrentEvent.TypeX
            Case EventTypeX.NoteOffEvent
                nudNoteOffData1.Value = CurrentEvent.Data1
                nudNoteOffData2.Value = CurrentEvent.Data2
            Case EventTypeX.NoteOnEvent
                nudNoteOnData1.Value = CurrentEvent.Data1
                nudNoteOnData2.Value = CurrentEvent.Data2
                nudNoteOnDuration.Value = CurrentEvent.Duration
            Case EventTypeX.PolyKeyPressure
                nudPolyKeyPressureData1.Value = CurrentEvent.Data1
                nudPolyKeyPressureData2.Value = CurrentEvent.Data2
            Case EventTypeX.ControlChange
                nudControlChangeData1.Value = CurrentEvent.Data1
                nudControlChangeData2.Value = CurrentEvent.Data2
            Case EventTypeX.ProgramChange
                nudProgramChangeData1.Value = CurrentEvent.Data1
            Case EventTypeX.ChannelPressure
                nudChannelPressureData1.Value = CurrentEvent.Data1
            Case EventTypeX.PitchBend
                PitchBendSlider.Value = PitchBendDataToValue(CurrentEvent.Data1, CurrentEvent.Data2)
            Case EventTypeX.F0SysExEvent
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbF0SysEx.Text = Bytes_to_hex_str(CurrentEvent.DataX)
            Case EventTypeX.F7SysExEvent
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbF7SysEx.Text = Bytes_to_hex_str(CurrentEvent.DataX)
            Case EventTypeX.SequenceNumber
                ' Meta 2 bytes
                If HasMetaData(CurrentEvent.DataX, 2) = False Then Exit Select
                Dim val As Integer
                val = CurrentEvent.DataX(0) * 256
                val += CurrentEvent.DataX(1)
                nudSequenceNumber.Value = val
            Case EventTypeX.TextEvent
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.CopyrightNotice
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.SequenceOrTrackName
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.InstrumentName
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.Lyric
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.Marker
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.CuePoint
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.ProgramName
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.DeviceName
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                tbEditMetaText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
            Case EventTypeX.MIDIChannelPrefix
                ' Meta 1 byte
                If HasMetaData(CurrentEvent.DataX, 1) = False Then Exit Select
                nudChannelPrefix.Value = CurrentEvent.DataX(0)
            Case EventTypeX.MIDIPortPrefix
                ' Meta 1 byte
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                nudPortPrefix.Value = CurrentEvent.DataX(0)
            Case EventTypeX.EndOfTrack
                ' no parameter needed
            Case EventTypeX.SetTempo
                ' Meta 3 bytes
                If HasMetaData(CurrentEvent.DataX, 3) = False Then Exit Select
                Dim barr As Byte() = CurrentEvent.DataX
                TempoSlider.Value = Math.Round(TempoDataToBPM(barr(0), barr(1), barr(2)), 0)
            Case EventTypeX.SMPTEOffset
                ' not yet
                ' Meta 5 bytes
                If HasMetaData(CurrentEvent.DataX, 5) = False Then Exit Select
            Case EventTypeX.TimeSignature
                ' Meta 4 bytes
                If HasMetaData(CurrentEvent.DataX, 4) = False Then Exit Select
                Dim barr As Byte() = CurrentEvent.DataX
                nudTimeSignatureNom.Value = barr(0)
                cmbTimeSignatureDenom.SelectedItem = barr(1)
                nudTimeSignatureClocks.Value = barr(2)
                nudTimeSignature32perQuarter.Value = barr(3)
            Case EventTypeX.KeySignature
                ' Meta 2 bytes
                If HasMetaData(CurrentEvent.DataX, 2) = False Then Exit Select
                Dim barr As Byte() = CurrentEvent.DataX
                nudKeySignature.Value = ByteToSByte(barr(0))
                If barr(1) = 0 Then
                    rbtnKeySignatureMajor.IsChecked = True
                Else
                    rbtnKeySignatureMinor.IsChecked = True
                End If
            Case EventTypeX.SequencerSpecific
                ' example with 3 data-bytes
                ' Format is defined by the sequecner, here we make a test example with 3 bytes
                If HasMetaData(CurrentEvent.DataX, 3) = False Then Exit Select
                ssldSequencerSpecific1.Value = CurrentEvent.DataX(0)
                ssldSequencerSpecific2.Value = CurrentEvent.DataX(1)
                ssldSequencerSpecific3.Value = CurrentEvent.DataX(2)
            Case EventTypeX.Unkown

            Case Else
                ' not listed --> unknown

        End Select
    End Sub

    ''' <summary>
    ''' Check if the buffer is not Nothing and holds the required number of bytes
    ''' </summary>
    ''' <param name="buffer">byte array</param>
    ''' <param name="requiredBytes">the number of expected bytes</param>
    ''' <returns>True if the buffer exists and holds at least the specified number of bytes</returns>
    Private Function HasMetaData(buffer As Byte(), requiredBytes As Integer) As Boolean
        If buffer Is Nothing Then Return False
        If buffer.Count < requiredBytes Then Return False
        Return True
    End Function

    Private Sub cmbNoteOnDuration_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles cmbNoteOnDuration.ValueChanged
        lblNoteOnDurationValue.Content = e.NewValue
    End Sub

    Private Sub btnNoteOnSetDuration_Click(sender As Object, e As RoutedEventArgs) Handles btnNoteOnSetDuration.Click
        nudNoteOnDuration.Value = cmbNoteOnDuration.Value
    End Sub

#Region "PitchBend"
    Private Sub PitchBendSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles PitchBendSlider.ValueChanged
        Dim ret As TwoBytes
        ret = PitchBendValueToData(e.NewValue)
        txblPitchBendData.Text = ret.Byte1 & "   " & ret.Byte2
    End Sub

    Private Sub PitchBendZero_Click(sender As Object, e As RoutedEventArgs) Handles PitchBendZero.Click
        PitchBendSlider.Value = 0
    End Sub

    Private Sub PitchBend2000_Click(sender As Object, e As RoutedEventArgs) Handles PitchBend2000.Click
        PitchBendSlider.Value = 2000
    End Sub

    Private Sub PitchBend4000_Click(sender As Object, e As RoutedEventArgs) Handles PitchBend4000.Click
        PitchBendSlider.Value = 4000
    End Sub

    Private Sub PitchBend6000_Click(sender As Object, e As RoutedEventArgs) Handles PitchBend6000.Click
        PitchBendSlider.Value = 6000
    End Sub

    Private Sub PitchBendMinus2000_Click(sender As Object, e As RoutedEventArgs) Handles PitchBendMinus2000.Click
        PitchBendSlider.Value = -2000
    End Sub

    Private Sub PitchBendMinus4000_Click(sender As Object, e As RoutedEventArgs) Handles PitchBendMinus4000.Click
        PitchBendSlider.Value = -4000
    End Sub

    Private Sub PitchBendMinus6000_Click(sender As Object, e As RoutedEventArgs) Handles PitchBendMinus6000.Click
        PitchBendSlider.Value = -6000
    End Sub





#End Region

    Private Sub TempoSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles TempoSlider.ValueChanged

        Dim bpm As Integer = e.NewValue

        Dim micros As Integer
        Dim tdata As ThreeBytes
        micros = BPM_ToMicroseconds(bpm)
        tdata = BPM_ToTempoData(bpm)

        txblSetTempoData.Text = micros.ToString("N0")
        txblSetTempoData2.Text = tdata.Byte1 & "   " & tdata.Byte2 & "   " & tdata.Byte3

    End Sub

    Private Sub nudNoteOnData1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudNoteOnData1.ValueChanged
        txblNoteOnName.Text = NoteNumber_to_NoteName(e.NewValue)
        txblNoteOnDrumName.Text = NoteNames.Get_GM_DrumVoiceName(e.NewValue)
    End Sub
    Private Sub radNoteOff_90h_Checked(sender As Object, e As RoutedEventArgs) Handles radNoteOff_90h.Checked
        If IsLoaded = False Then Exit Sub
        nudNoteOffData2.IsEnabled = False
        nudNoteOffData1.Value = CurrentEvent.Data1
    End Sub
    Private Sub radNoteOff_80h_Checked(sender As Object, e As RoutedEventArgs) Handles radNoteOff_80h.Checked
        If IsLoaded = False Then Exit Sub
        nudNoteOffData2.IsEnabled = True
        nudNoteOffData1.Value = CurrentEvent.Data1
        nudNoteOffData2.Value = CurrentEvent.Data2
    End Sub

    Private Sub nudKeySignature_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudKeySignature.ValueChanged
        Dim sfval As Integer = e.NewValue
        Dim str As String

        If sfval = 0 Then
            str = "key of C"
        Else
            Dim val As Integer = Math.Abs(sfval)
            str = val & " "
            If sfval < 0 Then
                If val = 1 Then
                    str = str & "flat"
                Else
                    str = str & "flats"
                End If
            Else
                If val = 1 Then
                    str = str & "sharp"
                Else
                    str = str & "sharps"
                End If
            End If
        End If

        txblKeySignatureSf.Text = str
    End Sub

    Private Sub btnTimeSignatureDefault_Click(sender As Object, e As RoutedEventArgs) Handles btnTimeSignatureDefault.Click
        cmbTimeSignatureDenom.SelectedIndex = 1
        nudTimeSignatureNom.Value = 4
        nudTimeSignatureClocks.Value = 24
        nudTimeSignature32perQuarter.Value = 8
    End Sub

    Private Sub nudProgramChangeData1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudProgramChangeData1.ValueChanged
        txblProgramChangeProgName.Text = NoteNames.Get_GM_VoiceName(e.NewValue)
    End Sub

    Private Sub nudControlChangeData1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudControlChangeData1.ValueChanged
        txblControlChangeCtrlName.Text = MDecode.GetControllerName(e.NewValue)
    End Sub

    Private Sub nudPolyKeyPressureData1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudPolyKeyPressureData1.ValueChanged
        txblPolyKeyPressureNoteName.Text = NoteNumber_to_NoteName(e.NewValue)
    End Sub

#Region "Edit Event"

    Private Sub btnSaveChanges_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveChanges.Click
        If HasChanges = False Then Exit Sub
        SaveChanges()
    End Sub

    Private Sub SaveChanges()

        '--- update data's in CurrentEvent ---

        CurrentEvent.TrackNumber = EditedEvent.TrackNumber
        CurrentEvent.Channel = EditedEvent.Channel
        CurrentEvent.TypeX = EditedEvent.TypeX
        CurrentEvent.Status = EditedEvent.Status
        CurrentEvent.Data1 = EditedEvent.Data1
        CurrentEvent.Data2 = EditedEvent.Data2
        CurrentEvent.Duration = EditedEvent.Duration

        If ByteArrayCompare(CurrentEvent.DataX, EditedEvent.DataX) = False Then
            CurrentEvent.DataX = EditedEvent.DataX
        End If

        If CurrentEvent.DataStr <> EditedEvent.DataStr Then
            CurrentEvent.DataStr = EditedEvent.DataStr
        End If

        '--- if Time was changed ---

        If CurrentEvent.Time <> EditedEvent.Time Then
            CurrentEvent.Time = EditedEvent.Time                    ' set new Time
            Evliw.CollectionView.Remove(CurrentEvent)               ' remove at current position
            If Evliw.InsertEvent(CurrentEvent) = False Then         ' insert to new position
                MessageWindow.Show(Me, "InsertEvent failed", "Time was changed", MessageIcon.Error)     ' for debug
            End If

            MBT_Editor1.OriginalValue = CurrentEvent.Time           ' update to remove changed color
        End If

        '--- update view ---

        Evliw.CollectionView.Refresh()

        ShowEventInfo()                             ' show the the updated data's in row 1
        EditCompare()                               ' reset background in row 2 and set HasChanges to FALSE
    End Sub


    Private Sub MBT_Editor1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles MBT_Editor1.ValueChanged
        EditedEvent.Time = e.NewValue
        TimeChanged
    End Sub

    Private Sub TempoSlider_ValueChanged_1(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles TempoSlider.ValueChanged
        If IsLoaded = True Then
            Dim barr = New Byte(2) {}
            Dim dat As ThreeBytes = BPM_ToTempoData(TempoSlider.Value)
            barr(0) = dat.Byte1
            barr(1) = dat.Byte2
            barr(2) = dat.Byte3
            EditedEvent.DataX = barr
            DataXChanged()
        End If
    End Sub

    Private Sub nudProgramChangeData1_ValueChanged_1(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudProgramChangeData1.ValueChanged
        EditedEvent.Data1 = e.NewValue
        Data1Changed()
    End Sub

    Private Sub nudControlChangeData1_ValueChanged_1(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudControlChangeData1.ValueChanged
        If IsLoaded = True Then
            EditedEvent.Data1 = e.NewValue
            Data1Changed()
        End If
    End Sub

    Private Sub nudControlChangeData2_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudControlChangeData2.ValueChanged
        If IsLoaded = True Then
            EditedEvent.Data2 = e.NewValue
            Data2Changed()
        End If
    End Sub

    Private Sub PitchBendSlider_ValueChanged_1(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles PitchBendSlider.ValueChanged
        Dim dat As TwoBytes = PitchBendValueToData(PitchBendSlider.Value)
        EditedEvent.Data1 = dat.Byte1
        EditedEvent.Data2 = dat.Byte2
        Data1Changed()
        Data2Changed()
    End Sub

    Private Sub nudNoteOnData1_ValueChanged_1(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudNoteOnData1.ValueChanged
        If IsLoaded = True Then
            EditedEvent.Data1 = e.NewValue
            Data1Changed()
        End If
    End Sub

    Private Sub nudNoteOnData2_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudNoteOnData2.ValueChanged
        If IsLoaded = True Then
            EditedEvent.Data2 = e.NewValue
            Data2Changed()
        End If
    End Sub

    Private Sub nudNoteOnDuration_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudNoteOnDuration.ValueChanged
        If IsLoaded = True Then
            EditedEvent.Duration = e.NewValue
            DurationChanged()
        End If
    End Sub

    Private Sub nudNoteOffData1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudNoteOffData1.ValueChanged
        If IsLoaded = True Then
            EditedEvent.Data1 = e.NewValue
            Data1Changed()
        End If
    End Sub

    Private Sub nudNoteOffData2_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudNoteOffData2.ValueChanged
        If IsLoaded = True Then
            EditedEvent.Data2 = e.NewValue
            Data2Changed()
        End If
    End Sub

    Private Sub radNoteOff_90h_Checked_1(sender As Object, e As RoutedEventArgs) Handles radNoteOff_90h.Checked
        If IsLoaded = True Then
            EditedEvent.Status = &H90 Or (CurrentEvent.Channel And &HF)
            StatusChanged()
            If EditedEvent.Data2 <> 0 Then
                nudNoteOffData2.Value = 0
            End If
        End If
    End Sub

    Private Sub radNoteOff_80h_Checked_1(sender As Object, e As RoutedEventArgs) Handles radNoteOff_80h.Checked
        If IsLoaded = True Then
            EditedEvent.Status = &H80 Or (CurrentEvent.Channel And &HF)
            StatusChanged()
        End If
    End Sub

    Private Sub tbEditMetaText_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbEditMetaText.TextChanged
        If IsLoaded = True Then
            EditedEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            DataXChanged()
        End If
    End Sub

    Private Sub nudKeySignature_ValueChanged_1(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudKeySignature.ValueChanged
        ' Meta 2 bytes
        EditedEvent.DataX(0) = CByte(nudKeySignature.Value And &HFF)        ' limit value to signed byte range
        DataXChanged()
    End Sub

    Private Sub rbtnKeySignatureMajor_Checked(sender As Object, e As RoutedEventArgs) Handles rbtnKeySignatureMajor.Checked
        If IsLoaded = False Then Exit Sub
        ' major = 0
        If EditedEvent.DataX(1) <> 0 Then
            EditedEvent.DataX(1) = 0
            DataXChanged()
        End If
    End Sub

    Private Sub rbtnKeySignatureMinor_Checked(sender As Object, e As RoutedEventArgs) Handles rbtnKeySignatureMinor.Checked
        If IsLoaded = False Then Exit Sub
        ' minor = 1
        If EditedEvent.DataX(1) <> 1 Then
            EditedEvent.DataX(1) = 1
            DataXChanged()
        End If
    End Sub

    Private Sub nudTimeSignatureNom_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudTimeSignatureNom.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.DataX(0) = nudTimeSignatureNom.Value
        DataXChanged()
    End Sub

    Private Sub cmbTimeSignatureDenom_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbTimeSignatureDenom.SelectionChanged
        If IsLoaded = False Then Exit Sub
        Dim ci As ComboBoxItem = cmbTimeSignatureDenom.SelectedItem
        EditedEvent.DataX(1) = Math.Sqrt(ci.Content)
        DataXChanged()
    End Sub

    Private Sub nudTimeSignatureClocks_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudTimeSignatureClocks.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.DataX(2) = nudTimeSignatureClocks.Value
        DataXChanged()
    End Sub

    Private Sub nudTimeSignature32perQuarter_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudTimeSignature32perQuarter.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.DataX(3) = nudTimeSignature32perQuarter.Value
        DataXChanged()
    End Sub

    Private Sub nudPolyKeyPressureData1_ValueChanged_1(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudPolyKeyPressureData1.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.Data1 = e.NewValue
        Data1Changed()
    End Sub

    Private Sub nudPolyKeyPressureData2_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudPolyKeyPressureData2.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.Data2 = e.NewValue
        Data2Changed()
    End Sub

    Private Sub nudChannelPressureData1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudChannelPressureData1.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.Data1 = e.NewValue
        Data1Changed()
    End Sub

    Private Sub nudSequenceNumber_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudSequenceNumber.ValueChanged
        If IsLoaded = False Then Exit Sub
        Dim val As Integer = e.NewValue
        val = val And &HFFFF
        EditedEvent.DataX(0) = val >> 8
        EditedEvent.DataX(1) = val And &HFF
        DataXChanged()
    End Sub

    Private Sub nudChannelPrefix_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudChannelPrefix.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.DataX(0) = e.NewValue
        DataXChanged()
    End Sub

    Private Sub nudPortPrefix_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudPortPrefix.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.DataX(0) = e.NewValue
        DataXChanged()
    End Sub

    Private Sub ssldSequencerSpecific1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldSequencerSpecific1.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.DataX(0) = e.NewValue
        DataXChanged()
    End Sub

    Private Sub ssldSequencerSpecific2_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldSequencerSpecific2.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.DataX(1) = e.NewValue
        DataXChanged()
    End Sub

    Private Sub ssldSequencerSpecific3_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldSequencerSpecific3.ValueChanged
        If IsLoaded = False Then Exit Sub
        EditedEvent.DataX(2) = e.NewValue
        DataXChanged()
    End Sub

    Private ReadOnly F0SysExRegexPattern As String = "[Ff][0][ ]([0-7][\da-fA-F]{1}[ ]{1})+?[Ff][7]"
    Private F0SysExRegex As New Text.RegularExpressions.Regex(F0SysExRegexPattern)

    Private Sub tbF0SysEx_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbF0SysEx.TextChanged
        If F0SysExRegex.IsMatch(tbF0SysEx.Text) Then
            btnF0SysExSet.IsEnabled = True
            F0SysExValidator.BorderBrush = Brushes.Green
        Else
            btnF0SysExSet.IsEnabled = False
            F0SysExValidator.BorderBrush = Brushes.Red
        End If
    End Sub

    Private Sub btnF0SysExSet_Click(sender As Object, e As RoutedEventArgs) Handles btnF0SysExSet.Click
        Dim str As String
        str = F0SysExRegex.Match(tbF0SysEx.Text).ToString()         ' remove leading and trailing chars

        '--- convert to byte array --- 
        Dim arr As String() = str.Split(CChar(" "))
        Dim SysExMsg As Byte() = New Byte(arr.Length - 1) {}

        For i = 1 To arr.Length
            SysExMsg(i - 1) = Convert.ToByte(arr(i - 1), 16)
        Next

        '--- set ---
        EditedEvent.DataX = SysExMsg
        DataXChanged()
    End Sub

    ' The F7 Part is not tested
    Private ReadOnly F7SysExRegexPattern As String = "([Ff][7])([ ][0-7][\da-fA-F])+([ ][Ff][7])?"
    Private F7SysExRegex As New Text.RegularExpressions.Regex(F7SysExRegexPattern)

    Private Sub tbF7SysEx_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbF7SysEx.TextChanged
        If F7SysExRegex.IsMatch(tbF7SysEx.Text) Then
            btnF7SysExSet.IsEnabled = True
            F7SysExValidator.BorderBrush = Brushes.Green
        Else
            btnF7SysExSet.IsEnabled = False
            F7SysExValidator.BorderBrush = Brushes.Red
        End If
    End Sub

    Private Sub btnF7SysExSet_Click(sender As Object, e As RoutedEventArgs) Handles btnF7SysExSet.Click
        Dim str As String
        str = F7SysExRegex.Match(tbF7SysEx.Text).ToString()         ' remove leading and trailing chars

        '--- convert to byte array --- 
        Dim arr As String() = str.Split(CChar(" "))
        Dim SysExMsg As Byte() = New Byte(arr.Length - 1) {}

        For i = 1 To arr.Length
            SysExMsg(i - 1) = Convert.ToByte(arr(i - 1), 16)
        Next

        '--- set ---
        EditedEvent.DataX = SysExMsg
        DataXChanged()
    End Sub

    Private Sub nudTrack_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudTrack.ValueChanged
        'If IsLoaded = False Then Exit Sub
        EditedEvent.TrackNumber = e.NewValue
        TrackChanged()
    End Sub

    Private Sub nudChannel_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudChannel.ValueChanged
        'If IsLoaded = False Then Exit Sub        
        Dim newChannel As Byte = e.NewValue And &HF
        EditedEvent.Channel = newChannel
        ChannelChanged()
        ' change also status if Midi-Event
        If evtinfo.IsMidiEvent = True Then
            EditedEvent.Status = (CurrentEvent.Status And &HF0) Or newChannel
            StatusChanged()
        End If

    End Sub



#End Region

End Class
