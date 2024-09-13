Imports System.Windows.Controls.Primitives
Imports Mmultitool.EventListWriter
Public Class DlgEditEvent

    Private TrevX As TrackEventX
    Private Evliw As EventListWriter

    Private CurrentEvent As TrackEventX


    Public Property LocalTPQ As Integer = 1

    Public Sub New(trev As TrackEventX, instance As EventListWriter)
        ' required for the designer


        TrevX = trev
        Evliw = instance

        LocalTPQ = Evliw.EvliTPQ

        InitializeComponent()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        'Evliw.CollectionView.MoveCurrentTo(TrevX)
        CurrentIndex = Evliw.CollectionView.CurrentPosition

        'GetCurrentEvent()
        'CurrentEvent = Evliw.CollectionView.CurrentItem
        ShowEventInfo()


        lblPosition.Content = CurrentIndex
        lblPositionMax.Content = Evliw.CollectionView.Count - 1

        ScrollBar1.Maximum = Evliw.CollectionView.Count - 1
        ScrollBar1.Value = Evliw.CollectionView.CurrentPosition




        'TrackEvents.Item(1)
    End Sub


    Private _CurrentIndex As Integer
    Private Property CurrentIndex As Integer
        Get
            Return _CurrentIndex
        End Get
        Set(value As Integer)
            _CurrentIndex = value
            GetCurrentEvent()
            ShowEventInfo()

            SetEditTab()
            SetEditData
        End Set
    End Property

    Private Sub GetCurrentEvent()
        CurrentEvent = Nothing
        If Evliw Is Nothing Then Exit Sub
        If CurrentIndex < 0 Then Exit Sub
        If CurrentIndex >= Evliw.CollectionView.Count Then Exit Sub
        CurrentEvent = Evliw.CollectionView.GetItemAt(CurrentIndex)
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim lbl = lblPositionMax.Content

        Beep()
        Me.Close()
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

    Private Sub MainGrid_KeyDown(sender As Object, e As KeyEventArgs) Handles MainGrid.KeyDown
        If e.Key = Key.Down Then
            ScrollBar1.Value = ScrollBar1.Value + 1
            e.Handled = True
        ElseIf e.Key = Key.Up Then
            ScrollBar1.Value = ScrollBar1.Value - 1
            e.Handled = True
        ElseIf e.Key = Key.Home Then
            ScrollBar1.Value = 0
            e.Handled = True
        ElseIf e.Key = Key.End Then
            ScrollBar1.Value = Evliw.TrackEvents.Count - 1
            e.Handled = True
        ElseIf e.Key = Key.PageUp Then
            ScrollBar1.Value = ScrollBar1.Value - ScrollBar1.LargeChange
            e.Handled = True
        ElseIf e.Key = Key.PageDown Then
            ScrollBar1.Value = ScrollBar1.Value + ScrollBar1.LargeChange
            e.Handled = True
        End If
    End Sub

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

    Private Sub SetEditData()
        Select Case CurrentEvent.TypeX
            Case EventTypeX.NoteOffEvent

                'If radNoteOff_90h.IsChecked = True Then
                '    CurrentEvent.Status = &H90
                '    CurrentEvent.Data1 = nudNoteOffData1.Value
                'Else
                '    CurrentEvent.Status = &H80
                '    CurrentEvent.Data1 = nudNoteOffData1.Value
                '    CurrentEvent.Data2 = nudNoteOffData2.Value
                'End If

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
                ' not yet                
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                'CurrentEvent.DataX = New Byte() {}
            Case EventTypeX.F7SysExEvent
                ' not yet
                If HasMetaData(CurrentEvent.DataX, 0) = False Then Exit Select
                'CurrentEvent.DataX = New Byte() {}
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
                nudKeySignature.Value = barr(0)
                If barr(1) = 0 Then
                    rbtnKeySignatureMajor.IsChecked = True
                Else
                    rbtnKeySignatureMajor.IsChecked = False
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

End Class
