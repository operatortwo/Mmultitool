Imports System.IO

Partial Public Module Player

    Friend TrackPlayer As New NoteOffProcessor
    Friend SequencePlayer As New NoteOffProcessor

    ' TrackPlayer and SequencePlayer have their own Time, BPM, NoteOffList.
    ' However, the events are sent to the same MIDI device and are only separated by the MIDI channel

    ' For example, if the SequencePlayer sends a ProgramChange to Midi channel 2,
    ' then all notes sent by TrackPlayer on channel 2 will also be played with this sound.

    Friend Class NoteOffProcessor
        ''' <summary>
        ''' Play a TrackEventX
        ''' </summary>
        ''' <param name="CurrentTime">Sequencer Time</param>
        ''' <param name="PlannedTime">StartTime + StartOffset + TevTime</param>
        ''' <param name="tev">Event data</param>
        Public Sub PlayEvent(CurrentTime As UInteger, PlannedTime As UInteger, tev As TrackEventX)
            Dim status As Byte = tev.Status And &HF0            ' status without channel

            If (status >= &H80) And (status < &HF0) Then        ' corresponds to MidiEvent

                If (status = &H90) And (tev.Data2 > 0) Then
                    ' is NoteOn, excluding NoteOn with Velocity 0 (NoteOff), --> ignore &h9x NoteOff
                    ' remove this note if already running (?)                
                    PlayNote(CurrentTime, PlannedTime, tev.Channel, tev.Data1, tev.Data2, tev.Duration)       ' Note On + Duration
                Else
                    If status <> &H80 Then                                  ' --> ignore &h8x NoteOff
                        ' &hA0, &hB0, &hC0, &hD0, &hE0,     PolyKeyPress, CtrlChg, ProgChg, ChPress, PitchBend
                        RaiseEvent MidiOutShortMsg(tev.Status, tev.Data1, tev.Data2)
                    End If
                End If

                'ElseIf tev.Status

            ElseIf tev.TypeX = EventTypeX.SetTempo Then
                If tev.DataX IsNot Nothing Then                 ' check for invalid SetTempo
                    If tev.DataX.Count >= 3 Then
                        Dim micros As Integer
                        micros = tev.DataX(0) * 65536 + tev.DataX(1) * 256 + tev.DataX(2)
                        SequencePlayerBPM = CSng(Math.Round(60 * 1000 * 1000 / micros, 2))       ' 2 Decimal places
                    End If
                End If

            ElseIf (tev.Status = &HF0) Or (tev.Status = &HF7) Then
                If tev.DataX.Count > 0 Then
                    Dim sysex(tev.DataX.Count) As Byte
                    sysex(0) = tev.Status
                    tev.DataX.CopyTo(sysex, 1)
                    RaiseEvent MidiOutLongMsg(sysex)
                End If

            End If

            '--- Update values for sequencer panel (UI) ---

            'PropertyUpdates(tev)

        End Sub


        Private Sub PlayNote(CurrentTime As UInteger, PlannedTime As UInteger, Channel As Byte, NoteNumber As Byte, Velocity As Byte, Duration As UInteger)
            Dim status As Byte = &H90 Or (Channel And &HF)       ' NoteOn & Channel

            ' check if this note is already running
            Dim noffli As NoteOffList = NoteOffListCollection(Channel)  ' shortcut to the corresponding list
            Dim ndx As Integer
            ndx = noffli.Items.FindIndex(Function(x) x.Data1 = NoteNumber)
            If ndx >= 0 Then
                Dim noevX As NoteOffEvent
                noevX = noffli.Items(ndx)
                RaiseEvent MidiOutShortMsg(noevX.Status, noevX.Data1, noevX.Data2)         ' play Note Off
                noffli.Items.RemoveAt(ndx)                                              ' remove from list
            End If

            RaiseEvent MidiOutShortMsg(status, NoteNumber, Velocity)
            Insert_NoteOff(Channel, PlannedTime + Duration, NoteNumber, Velocity)
        End Sub

        ''' <summary>
        ''' Insert a Note to the NoteOff List
        ''' </summary>
        ''' <param name="Channel">0-15</param>
        ''' <param name="OffTime">Time of the NoteOn event + Duration</param>    
        ''' <param name="NoteNumber">Data1 in MidiMsg</param>    
        Private Sub Insert_NoteOff(Channel As Byte, OffTime As UInteger, NoteNumber As Byte, Velocity As Byte)
            If Channel <= &HF Then                          ' only Channel Numbers 0-15 are allowed
                If NoteNumber <= 127 Then                   ' only Note Numbers 0-127 are allowed

                    ' preparing the NoteOff event for inserting to the list
                    Dim noev As New NoteOffEvent With
                        {.OffTime = OffTime, .Status = &H90 Or Channel, .Data1 = NoteNumber, .Data2 = 0}

                    Dim noffli As NoteOffList = NoteOffListCollection(Channel)  ' shortcut to the corresponding list

                    ' if the only one item (easy and fast)
                    If noffli.Items.Count = 0 Then
                        noffli.Items.Add(noev)
                        Exit Sub
                    End If

                    ' check if list if full
                    If noffli.Items.Count = NoteOffListCapacity Then
                        Dim noevX As NoteOffEvent
                        noevX = noffli.Items.Last
                        RaiseEvent MidiOutShortMsg(noevX.Status, noevX.Data1, noevX.Data2)      ' play Note Off
                        noffli.Items.RemoveAt(NoteOffListCapacity - 1)                          ' remove last item
                        'Debug.WriteLine("Full -> Removed")
                    End If

                    ' check if the new item belongs to the end of the list
                    If OffTime > noffli.Items(noffli.Items.Count - 1).OffTime Then
                        noffli.Items.Add(noev)
                        Exit Sub
                    End If

                    ' check if the new item belongs to the beginning of the list
                    If OffTime < noffli.Items(0).OffTime Then
                        noffli.Items.Insert(0, noev)
                        Exit Sub
                    End If

                    ' now search where the new item must be inserted
                    Dim ndx As Integer
                    ndx = noffli.Items.FindLastIndex(Function(x) x.OffTime <= noev.OffTime)
                    If ndx = -1 Then
                        Exit Sub
                    Else
                        noffli.Items.Insert(ndx + 1, noev)
                    End If

                End If
            End If

        End Sub


        ''' <summary>
        ''' Checks the Note Off list for all channels to see whether the time for running notes has expired.
        ''' If so, a NoteOff event is raised and the corresponding entry is removed from the list.
        ''' This action can also occur multiple times if several matching entries are found.
        ''' </summary>
        ''' <param name="CurrentTime">Current Sequencer time</param>
        Public Sub Do_TimedNoteOff(CurrentTime As Double)
            ' check if it is time for NoteOff
            Dim noev As NoteOffEvent
            For Each list In NoteOffListCollection
                While list.Items.Count > 0
                    noev = list.Items(0)
                    If noev.OffTime <= CurrentTime Then
                        RaiseEvent MidiOutShortMsg(noev.Status, noev.Data1, noev.Data2)     ' play Note Off
                        list.Items.RemoveAt(0)
                    Else
                        Exit While
                    End If
                End While
            Next
        End Sub

        ''' <summary>
        ''' Immediately turn off all playing notes on all channels.
        ''' This is done by reading the NoteOff lists and raising a MidiOutShortMsg event for each item found.
        ''' It also clears all NoteOffLists.
        ''' </summary>
        Public Sub AllRunningNotesOff()
            Dim noev As NoteOffEvent
            For Each list In NoteOffListCollection
                For i = 1 To list.Items.Count
                    noev = list.Items(i - 1)
                    RaiseEvent MidiOutShortMsg(noev.Status, noev.Data1, noev.Data2)     ' play Note Off
                Next
                list.Items.Clear()                  ' clears NoteOffList
            Next
        End Sub

        Private ReadOnly NoteOffListCollection As New List(Of NoteOffList) From
           {
            {New NoteOffList},                      ' Channel  0
            {New NoteOffList},                      ' Channel  1
            {New NoteOffList},                      ' Channel  2
            {New NoteOffList},                      ' Channel  3
            {New NoteOffList},                      ' Channel  4
            {New NoteOffList},                      ' Channel  5
            {New NoteOffList},                      ' Channel  6
            {New NoteOffList},                      ' Channel  7
            {New NoteOffList},                      ' Channel  8
            {New NoteOffList},                      ' Channel  9
            {New NoteOffList},                      ' Channel 10
            {New NoteOffList},                      ' Channel 11
            {New NoteOffList},                      ' Channel 12
            {New NoteOffList},                      ' Channel 13
            {New NoteOffList},                      ' Channel 14
            {New NoteOffList}                       ' Channel 15         
           }

        Private Const NoteOffListCapacity = 32                                  ' 32 on each channel
        Private Class NoteOffList
            Public Items As New List(Of NoteOffEvent)(NoteOffListCapacity)
        End Class

        Private Class NoteOffEvent
            Public OffTime As UInteger
            ' prepared for sendig as MidiOutShortMsg, using NoteOff as NoteOn with velocity 0
            Public Status As Byte                   ' &h9x & channel
            Public Data1 As Byte                    ' Note number
            Public Data2 As Byte                    ' Velocity (always 0)
        End Class

    End Class

End Module

