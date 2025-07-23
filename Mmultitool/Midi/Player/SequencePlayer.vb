Partial Public Module Player


    Public ReadOnly SequenceList As New List(Of Sequence)

    Private _ValueForNewSequenceID As Integer
    ''' <summary>
    ''' Automatically incrementing unique value to identify sequence
    ''' </summary>
    ''' <returns></returns>
    Private ReadOnly Property ValueForNewSequenceID As Integer
        Get
            If _ValueForNewSequenceID = (2 ^ 31) - 1 Then _ValueForNewSequenceID = 0
            _ValueForNewSequenceID += 1
            Return _ValueForNewSequenceID
        End Get
    End Property

#Region "Play"

    Private Sub PlaySequenceList()                      ' called from Timer TickCallback
        Dim CurrenTime As Long = SequencePlayerTime
        For Each seq In SequenceList
            PlayThisSequence(seq, CurrenTime)
        Next
        '--- remove ended sequences ---
        For i = SequenceList.Count - 1 To 0 Step -1
            If SequenceList(i).Ended = True Then
                SequenceList(i).ID = 0
                SequenceList.RemoveAt(i)
            End If
        Next
    End Sub

    Private Sub PlayThisSequence(seq As Sequence, CurrentTime As Long)
        If seq Is Nothing Then Exit Sub
        If seq.Ended = True Then Exit Sub
        If CurrentTime < seq.StartTime Then Exit Sub
        If seq.EventListPtr >= seq.EventList.Count Then Exit Sub

        Dim tev As TrackEventX
        tev = seq.EventList(seq.EventListPtr)

        While seq.StartTime + seq.StartOffset + tev.Time <= CurrentTime

            If seq.ForceToChannel = False Then
                SequencePlayer.PlayEvent(CurrentTime, seq.StartTime + seq.StartOffset + tev.Time, tev)
            Else
                SequencePlayer.PlayEvent(CurrentTime, seq.StartTime + seq.StartOffset + tev.Time, tev, seq.DestinationChannel)
            End If

            MoveEventListPtr(seq)
            If seq.EventListPtr >= seq.EventList.Count Then Exit While
            tev = seq.EventList(seq.EventListPtr)
        End While

    End Sub

    Private Sub MoveEventListPtr(seq As Sequence)
        seq.EventListPtr += 1
        If seq.EventListPtr >= seq.EventList.Count Then
            ' if do loop -> restart sequence
            If seq.DoLoop = True Then
                seq.StartOffset += seq.Length
                seq.EventListPtr = 0
                Exit Sub
                ' if play multiple times
            ElseIf seq.Duration > seq.Length Then
                If (seq.StartOffset + seq.Length) < seq.Duration Then
                    seq.StartOffset += seq.Length
                    seq.EventListPtr = 0
                Else
                    seq.Ended = True
                End If
            Else
                ' sequence is at the end
                seq.Ended = True
            End If
        End If
    End Sub

#End Region

#Region "Set and remove Sequence"

    ''' <summary>
    ''' Start sequencer if necessary, and add sequence to SequeneList
    ''' </summary>
    ''' <param name="seq">sequence to play</param>
    ''' <param name="DoLoop">repeat sequence at the end ?</param>
    Public Function PlaySequence(seq As Sequence, DoLoop As Boolean) As Integer
        '--- set unique ID to stop selected sequences (sequences can be very long, just waiting for end is not an option ---
        seq.ID = ValueForNewSequenceID
        seq.EventListPtr = 0
        seq.Ended = False

        If IsSequencePlayerRunning = False Then StartSequencePlayer()
        '--- insert seq to sequence list
        'SequenceList.Clear()                                    ' remove old sequences
        seq.StartTime = GetTimeOfNextBeat(SequencePlayerTime)
        seq.StartOffset = 0
        seq.DoLoop = DoLoop
        SequenceList.Add(seq)

        Return seq.ID
    End Function

    Public Sub PlaySingleEvent(tev As TrackEventX, SourceTPQ As Integer)
        If IsSequencePlayerRunning = False Then StartSequencePlayer()
        Dim tev2 As TrackEventX = tev.Copy(False)
        tev2.Duration = ToSeqTime(tev.Duration, SourceTPQ)
        SequencePlayer.PlayEvent(SequencePlayerTime, SequencePlayerTime, tev2)
    End Sub

    Public Function CreateSequence(items As IList, TPQsource As Integer) As Sequence
        Dim seq As New Sequence
        If items Is Nothing Then Return seq             ' no data
        If items.Count = 0 Then Return seq              ' no data

        '--- copy the events and adjust time and duration to sequencerTPQ (480)
        Dim selx As New List(Of TrackEventX)
        Dim tev As TrackEventX                          ' source
        Dim tev2 As TrackEventX
        For Each item In items
            tev = TryCast(item, TrackEventX)
            If tev IsNot Nothing Then
                tev2 = tev.Copy(False)
                tev2.Time = ToSeqTime(tev2.Time, TPQsource)
                tev2.Duration = ToSeqTime(tev2.Duration, TPQsource)
                seq.EventList.Add(tev2)
            End If
        Next
        Dim sc As New TrackEventX
        seq.EventList.Sort(sc)                          ' items input is not always sorted -> do it here

        If seq.EventList.Count > 0 Then
            '--- absolute time to relative time, set sequence Length and Duration
            Dim TimeOffset As UInteger = seq.EventList(0).Time
            TimeOffset = GetTimeOfPreviousBeat(TimeOffset)      ' keep relative time within beat
            ' f.e. if the first event is at 3:1:24 it will be shifted to 0:0:24

            For Each tev In seq.EventList
                tev.Time -= TimeOffset
            Next

            ' time of last event, round up to next beat (beat aligned sequence)
            seq.Length = GetTimeOfNextBeat(seq.EventList.Last.Time)
            seq.Duration = seq.Length
        End If

        Return seq
    End Function

    ''' <summary>
    ''' Remove a Sequence from the SequenceList an reset Sequence.ID to 0
    ''' </summary>    
    Public Sub RemoveSequence(seq As Sequence)
        If seq Is Nothing Then Exit Sub
        If seq.ID = 0 Then Exit Sub                     ' nothing to do

        For i = 0 To SequenceList.Count - 1
            If SequenceList(i).ID = seq.ID Then
                SequenceList.RemoveAt(i)
                Exit For
            End If
        Next

        seq.ID = 0
    End Sub

#End Region

    Public Class Sequence                               ' Fixed TPQ: always 480
        Public Property Name As String = ""             ' need Property for WPF DataBinding
        Public ID As Integer                            ' unique value to identify the sequence, <> 0 if playing
        Public StartTime As UInteger                    ' in Ticks 
        Public StartOffset As UInteger                  ' for repeated play
        Public Length As UInteger                       ' in ticks (1 beat = 480), for the sequence itself
        Public Duration As UInteger                     ' while
        Public DoLoop As Boolean                        ' restart at end (higher priority than Duration)
        Public Ended As Boolean                         ' needed ?
        Public ForceToChannel As Boolean                ' False: status/channel in TrackEventX is used 
        Public DestinationChannel As Byte               ' Midi-channel if ForceToChannel is set to True
        Public EventListPtr As Integer                  ' ptr to next item in EventList
        Public EventList As New List(Of TrackEventX)
    End Class


    ''' <summary>
    ''' Get time of next beat mark. Time base is SequencerTPQ.
    ''' </summary>
    ''' <param name="Time">Time 1</param>
    ''' <returns>Time 1 rounded up to next beat mark</returns>
    Public Function GetTimeOfNextBeat(Time As UInteger) As UInteger
        Dim Newtime As UInteger
        Dim TicksPerUnit As Integer = PlayerTPQ             ' here a unit is a beat = 1 quaterNote Length
        Dim ElapsedTicks As UInteger                        ' in this unit
        Dim RemainingTicks As UInteger                      ' in this unit     

        ElapsedTicks = CUInt(Time Mod TicksPerUnit)
        RemainingTicks = CUInt(TicksPerUnit - ElapsedTicks)
        Newtime = Time + RemainingTicks                     ' round up to end of this unit

        Return Newtime
    End Function

    ''' <summary>
    ''' Get time of previous beat mark. Time base is SequencerTPQ.
    ''' </summary>
    ''' <param name="Time">Time 1</param>
    ''' <returns>Time 1 rounded down to previous beat mark</returns>
    Public Function GetTimeOfPreviousBeat(Time As UInteger) As UInteger
        Dim Newtime As UInteger
        Dim TicksPerUnit As UInteger = PlayerTPQ            ' here a unit is a beat = 1 quaterNote Length
        Dim ElapsedTicks As UInteger                        ' in this unit

        ElapsedTicks = Time Mod TicksPerUnit
        Newtime = Time - ElapsedTicks                       ' round down to start of this unit

        Return Newtime
    End Function


End Module
