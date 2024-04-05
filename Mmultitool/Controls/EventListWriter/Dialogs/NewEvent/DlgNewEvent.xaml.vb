Imports System.Collections.ObjectModel
Imports System.Text
Imports DailyUserControls



Public Class DlgNewEvent
    Private Evliw As EventListWriter
    ''' <summary>
    ''' When starting, this is the selected item which is used to make a suggestion for 
    ''' time, track and channel of the new item. Nothing if no item is selected.
    ''' </summary>
    Public StartReferenceItem As TrackEventX
    ''' <summary>
    ''' The last inserted TrackEventX which can be used for SetFocusToSelectedRow
    ''' </summary>
    Public LastInsertedEvent As TrackEventX
    ''' <summary>
    ''' Dialog to insert one ore more new TrackEvnetX. When DialogResult is set to TRUE, LastInsertedEvent 
    ''' can be used to cjange the selected row in the DataGrid.
    ''' </summary>
    ''' <param name="instance">Instance of the calling EventListWriter</param>    
    Public Sub New(instance As EventListWriter)
        InitializeComponent()                   ' required for the Designer
        Evliw = instance
    End Sub

    Private ReadOnly Property localTPQ As Integer
    Private CurrentEvent As TrackEventX

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        _localTPQ = Evliw.EvliTPQ
        MBT_Editor1.TPQ = localTPQ
        MBT_Editor2_Step.TPQ = localTPQ
        MBT_InputBox1.TPQ = localTPQ
        lblTPQ.Content = localTPQ

        If Evliw.DesiredTimeFormat = EventListWriter.TimeFormat.MBT_1_based Then
            MBT_Editor1.IsMBT_Base1 = True
            MBT_Editor2_Step.IsMBT_Base1 = True
            MBT_InputBox1.IsMBT_Base1 = True
            lblMBT_Base.Content = "1"
        Else
            lblMBT_Base.Content = "0"
        End If

        nudTrack.MaximumValue = Evliw.TrackList.Count - 1
        If StartReferenceItem IsNot Nothing Then
            nudTrack.Value = StartReferenceItem.TrackNumber
            nudChannel.Value = StartReferenceItem.Channel
        End If


        nudNoteOnDuration.MaximumValue = 4 * localTPQ

        CurrentEvent = New TrackEventX With {.TypeX = EventTypeX.Unkown}

        If StartReferenceItem IsNot Nothing Then
            CurrentEvent.Time = StartReferenceItem.Time
            MBT_Editor1.OriginalValue = StartReferenceItem.Time
            CurrentEvent.Channel = StartReferenceItem.Channel
            CurrentEvent.TrackNumber = StartReferenceItem.TrackNumber
        End If

        cmbEventTypeX.ItemsSource = [Enum].GetValues(GetType(EventTypeX))
        cmbEventTypeX.SelectedIndex = 2

        cmbEventTypeX.Focus()
    End Sub

    Private evtinfo As EventTypeX_Information

    Private Sub cmbEventTypeX_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbEventTypeX.SelectionChanged
        Dim sel = cmbEventTypeX.SelectedItem
        If sel IsNot Nothing Then
            evtinfo = GetEventTypeX_Info(sel)               ' get Info for selected TypeX
            InitializeEvent(sel)

            tblEventInfo.Text = evtinfo.Info                    ' show Info for the event
            'ShowAvailableControls()                             ' old
            SetEditTab()

        End If
    End Sub

    Private buffer() As Byte = {}
    Private Sub InitializeEvent(tp As EventTypeX)
        CurrentEvent.TypeX = tp
        CurrentEvent.Type = GetEventType(CurrentEvent.TypeX)

        CurrentEvent.Data2 = 0
        CurrentEvent.Duration = 0

        If evtinfo.IsMidiEvent = True Then
            CurrentEvent.Status = (CurrentEvent.TypeX And &HF0) Or (CurrentEvent.Channel And &HF)
            CurrentEvent.DataX = Nothing
        Else
            CurrentEvent.Status = 0
            CurrentEvent.DataX = buffer
            ' Data1 contains Meta and SysEx type number
            Dim val As Integer = CurrentEvent.TypeX
            val = val And &HFF
            CurrentEvent.Data1 = val
            tbEditMetaText.Text = ""
        End If


    End Sub



#Region "Visibility"

    Private Sub SetEditTab()

        Select Case evtinfo.enumtype
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
            Case EventTypeX.CopyrightNotice
                TabControlEdit.SelectedItem = Ti_MetaText
            Case EventTypeX.SequenceOrTrackName
                TabControlEdit.SelectedItem = Ti_MetaText
            Case EventTypeX.InstrumentName
                TabControlEdit.SelectedItem = Ti_MetaText
            Case EventTypeX.Lyric
                TabControlEdit.SelectedItem = Ti_MetaText
            Case EventTypeX.Marker
                TabControlEdit.SelectedItem = Ti_MetaText
            Case EventTypeX.CuePoint
                TabControlEdit.SelectedItem = Ti_MetaText
            Case EventTypeX.ProgramName
                TabControlEdit.SelectedItem = Ti_MetaText
            Case EventTypeX.DeviceName
                TabControlEdit.SelectedItem = Ti_MetaText
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






#End Region



    Private Sub MBT_InputBox1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles MBT_InputBox1.ValueChanged
        MBT_Editor1.ChangeCurrentValue(e.NewValue)
    End Sub


    Private Sub MBT_Editor1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles MBT_Editor1.ValueChanged
        ' time is updated before inserting
    End Sub


#Region "Insert Event"

    Private InsertCounter As Integer
    Private ReadOnly ASCIIenc As New ASCIIEncoding

    Private Sub btnInsertEvent_Click(sender As Object, e As RoutedEventArgs) Handles btnInsertEvent.Click
        CurrentEvent.Time = MBT_Editor1.NewValue
        CurrentEvent.TrackNumber = nudTrack.Value

        If evtinfo.IsSysExEvent = False Then
            CurrentEvent.Channel = nudChannel.Value
        End If

        If evtinfo.IsMidiEvent Then
            CurrentEvent.Status = CurrentEvent.Status Or nudChannel.Value
        End If


        If GetEditData() = False Then Exit Sub

        CurrentEvent.DataStr = GetData(CurrentEvent)

        If Evliw.InsertEvent(CurrentEvent.Copy(True)) = True Then
            InsertCounter += 1
            lblInsertCount.Content = InsertCounter
            If cbIncreaseByStep.IsChecked = True Then
                MBT_Editor1.ChangeCurrentValue(MBT_Editor1.NewValue + MBT_Editor2_Step.NewValue)
            End If
        Else
            Beep()      ' failed to insert
        End If

    End Sub


    Private Function GetEditData() As Boolean


        Select Case evtinfo.enumtype
            Case EventTypeX.NoteOffEvent
                If radNoteOff_90h.IsChecked = True Then
                    CurrentEvent.Status = &H90 Or nudChannel.Value
                    CurrentEvent.Data1 = nudNoteOffData1.Value
                Else
                    CurrentEvent.Status = &H80
                    CurrentEvent.Data1 = nudNoteOffData1.Value
                    CurrentEvent.Data2 = nudNoteOffData2.Value
                End If
            Case EventTypeX.NoteOnEvent
                CurrentEvent.Data1 = nudNoteOnData1.Value
                CurrentEvent.Data2 = nudNoteOnData2.Value
                CurrentEvent.Duration = nudNoteOnDuration.Value
            Case EventTypeX.PolyKeyPressure
                CurrentEvent.Data1 = nudPolyKeyPressureData1.Value
                CurrentEvent.Data2 = nudPolyKeyPressureData2.Value
            Case EventTypeX.ControlChange
                CurrentEvent.Data1 = nudControlChangeData1.Value
                CurrentEvent.Data2 = nudControlChangeData2.Value
            Case EventTypeX.ProgramChange
                CurrentEvent.Data1 = nudProgramChangeData1.Value
            Case EventTypeX.ChannelPressure
                CurrentEvent.Data1 = nudChannelPressureData1.Value
            Case EventTypeX.PitchBend
                Dim dat As TwoBytes = PitchBendValueToData(PitchBendSlider.Value)
                CurrentEvent.Data1 = dat.Byte1
                CurrentEvent.Data2 = dat.Byte2
            Case EventTypeX.F0SysExEvent
                If F0SysExRegex.IsMatch(tbF0SysEx.Text) Then
                    GetEditData_F0()
                Else
                    MessageWindow.Show(Me, "The SysEx string is invalid.", "New TrackEvent", MessageIcon.Warning)
                    Return False
                End If
            Case EventTypeX.F7SysExEvent
                If F7SysExRegex.IsMatch(tbF7SysEx.Text) Then
                    GetEditData_F7()
                Else
                    MessageWindow.Show(Me, "The SysEx string is invalid.", "New TrackEvent", MessageIcon.Warning)
                    Return False
                End If
            Case EventTypeX.SequenceNumber
                ' Meta 2 bytes
                Dim barr = New Byte(1) {}
                barr(0) = nudSequenceNumber.Value \ 256
                barr(1) = nudSequenceNumber.Value Mod 256
                CurrentEvent.DataX = barr
            Case EventTypeX.TextEvent
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.CopyrightNotice
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.SequenceOrTrackName
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.InstrumentName
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.Lyric
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.Marker
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.CuePoint
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.ProgramName
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.DeviceName
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditMetaText.Text)
            Case EventTypeX.MIDIChannelPrefix
                ' Meta 1 byte                
                Dim barr = New Byte(0) {}               ' byte(upperBound)
                barr(0) = nudChannelPrefix.Value
                CurrentEvent.DataX = barr
            Case EventTypeX.MIDIPortPrefix
                ' Meta 1 byte
                Dim barr = New Byte(0) {}
                barr(0) = nudPortPrefix.Value
                CurrentEvent.DataX = barr
            Case EventTypeX.EndOfTrack
                ' no parameter needed
            Case EventTypeX.SetTempo
                ' Meta 3 bytes
                Dim barr = New Byte(2) {}
                Dim dat As ThreeBytes = BPM_ToTempoData(TempoSlider.Value)
                barr(0) = dat.Byte1
                barr(1) = dat.Byte2
                barr(2) = dat.Byte3
                CurrentEvent.DataX = barr
            Case EventTypeX.SMPTEOffset
                ' not yet
                ' Meta 5 bytes
            Case EventTypeX.TimeSignature
                ' Meta 4 bytes
                Dim barr = New Byte(3) {}
                barr(0) = nudTimeSignatureNom.Value
                Dim ci As ComboBoxItem = cmbTimeSignatureDenom.SelectedItem
                barr(1) = Math.Sqrt(ci.Content)
                barr(2) = nudTimeSignatureClocks.Value
                barr(3) = nudTimeSignature32perQuarter.Value
                CurrentEvent.DataX = barr
            Case EventTypeX.KeySignature
                ' Meta 2 bytes
                Dim barr = New Byte(1) {}
                barr(0) = CByte(nudKeySignature.Value And &HFF)         ' limit value to signed byte range
                If rbtnKeySignatureMajor.IsChecked = True Then
                    barr(1) = 0
                Else
                    barr(1) = 1
                End If
                CurrentEvent.DataX = barr
            Case EventTypeX.SequencerSpecific
                ' example with 3 data-bytes
                Dim barr = New Byte(2) {}
                barr(0) = ssldSequencerSpecific1.Value
                barr(1) = ssldSequencerSpecific2.Value
                barr(2) = ssldSequencerSpecific3.Value
                CurrentEvent.DataX = barr
            Case EventTypeX.Unkown

            Case Else
                ' not listed --> unknown

        End Select

        Return True
    End Function

    Private Sub GetEditData_F0()
        Dim str As String
        str = F0SysExRegex.Match(tbF0SysEx.Text).ToString()         ' remove leading and trailing chars

        '--- convert to byte array --- 
        Dim arr As String() = str.Split(CChar(" "))
        Dim SysExMsg As Byte() = New Byte(arr.Length - 1) {}

        For i = 1 To arr.Length
            SysExMsg(i - 1) = Convert.ToByte(arr(i - 1), 16)
        Next

        CurrentEvent.DataX = SysExMsg
    End Sub

    Private Sub GetEditData_F7()
        Dim str As String
        str = F7SysExRegex.Match(tbF7SysEx.Text).ToString()         ' remove leading and trailing chars

        '--- convert to byte array --- 
        Dim arr As String() = str.Split(CChar(" "))
        Dim SysExMsg As Byte() = New Byte(arr.Length - 1) {}

        For i = 1 To arr.Length
            SysExMsg(i - 1) = Convert.ToByte(arr(i - 1), 16)
        Next

        CurrentEvent.DataX = SysExMsg
    End Sub

#End Region

    Private Sub NoteDurationHelper_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles cmbNoteOnDuration.ValueChanged
        lblNoteOnDurationValue.Content = e.NewValue
    End Sub

    Private Sub btnSetDuration_Click(sender As Object, e As RoutedEventArgs) Handles btnNoteOnSetDuration.Click
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
        nudNoteOffData2.IsEnabled = False
    End Sub

    Private Sub radNoteOff_80h_Checked(sender As Object, e As RoutedEventArgs) Handles radNoteOff_80h.Checked
        nudNoteOffData2.IsEnabled = True
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

    Private ReadOnly F0SysExRegexPattern As String = "[Ff][0][ ]([0-7][\da-fA-F]{1}[ ]{1})+?[Ff][7]"
    Private F0SysExRegex As New Text.RegularExpressions.Regex(F0SysExRegexPattern)
    Private Sub tbF0SysEx_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbF0SysEx.TextChanged
        If F0SysExRegex.IsMatch(tbF0SysEx.Text) Then
            F0SysExValidator.BorderBrush = Brushes.Green
        Else
            F0SysExValidator.BorderBrush = Brushes.Red
        End If
    End Sub

    Private ReadOnly F7SysExRegexPattern As String = "([Ff][7])([ ][0-7][\da-fA-F])+([ ][Ff][7])?"
    Private F7SysExRegex As New Text.RegularExpressions.Regex(F7SysExRegexPattern)
    Private Sub tbF7SysEx_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbF7SysEx.TextChanged
        If F7SysExRegex.IsMatch(tbF7SysEx.Text) Then
            F7SysExValidator.BorderBrush = Brushes.Green
        Else
            F7SysExValidator.BorderBrush = Brushes.Red
        End If
    End Sub
End Class

