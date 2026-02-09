Partial Public Module Player


    Public ReadOnly PatternList As New List(Of Pattern)

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

    Private Sub PlayPatternList()                      ' called from Timer TickCallback
        Dim CurrenTime As Long = PatternPlayerTime
        For Each seq In PatternList
            PlayThisSequence(seq, CurrenTime)
        Next
        '--- remove ended sequences ---
        For i = PatternList.Count - 1 To 0 Step -1
            If PatternList(i).Ended = True Then
                PatternList(i).ID = 0
                PatternList.RemoveAt(i)
            End If
        Next
    End Sub

    Private Sub PlayThisSequence(seq As Pattern, CurrentTime As Long)
        If seq Is Nothing Then Exit Sub
        If seq.Ended = True Then Exit Sub
        If CurrentTime < seq.StartTime Then Exit Sub
        If seq.EventListPtr >= seq.EventList.Count Then Exit Sub

        Dim tev As TrackEventX
        tev = seq.EventList(seq.EventListPtr)

        While seq.StartTime + seq.StartOffset + tev.Time <= CurrentTime

            If seq.ForceToChannel = False Then
                PatternPlayer.PlayEvent(CurrentTime, seq.StartTime + seq.StartOffset + tev.Time, tev)
            Else
                PatternPlayer.PlayEvent(CurrentTime, seq.StartTime + seq.StartOffset + tev.Time, tev, seq.DestinationChannel)
            End If

            MoveEventListPtr(seq)
            If seq.EventListPtr >= seq.EventList.Count Then Exit While
            tev = seq.EventList(seq.EventListPtr)
        End While

    End Sub

    Private Sub MoveEventListPtr(seq As Pattern)
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

#Region "Set and remove Pattern"

    ''' <summary>
    ''' Start sequencer if necessary, and add Pattern to PatternList
    ''' </summary>
    ''' <param name="pat">pattern to play</param>
    ''' <param name="DoLoop">repeat pattern at the end ?</param>
    Public Function PlayPattern(pat As Pattern, DoLoop As Boolean) As Integer
        '--- set unique ID to stop selected sequences (sequences can be very long, just waiting for end is not an option ---
        pat.ID = ValueForNewSequenceID
        pat.EventListPtr = 0
        pat.Ended = False

        If IsPatternPlayerRunning = False Then StartPatternPlayer()
        '--- insert seq to sequence list
        'SequenceList.Clear()                                    ' remove old sequences
        pat.StartTime = GetTimeOfNextBeat(PatternPlayerTime)
        'seq.StartTime = GetTimeOfNextMeasure(SequencePlayerTime)
        pat.StartOffset = 0
        pat.DoLoop = DoLoop
        PatternList.Add(pat)

        Return pat.ID
    End Function

    ''' <summary>
    ''' Play a SingleTrackEventX immediately with SequencePlayer, base for Duration can be TPQ other than 480
    ''' </summary>
    ''' <param name="tev">TrackEventX with Channel, Status, Data1, Data2, Duration</param>
    ''' <param name="SourceTPQ">Duration need adjustment if SourceTPQ is other than 480</param>
    Public Sub PlaySingleEvent(tev As TrackEventX, SourceTPQ As Integer)
        If IsPatternPlayerRunning = False Then StartPatternPlayer()
        Dim tev2 As TrackEventX = tev.Copy(False)
        tev2.Duration = ToSeqTime(tev.Duration, SourceTPQ)
        PatternPlayer.PlayEvent(PatternPlayerTime, PatternPlayerTime, tev2)
    End Sub

    ''' <summary>
    ''' Play a SingleTrackEventX immediately with SequencePlayer, assumnig base for Duration is TPQ 480
    ''' </summary>
    ''' <param name="tev">TrackEventX with Channel, Status, Data1, Data2, Duration</param>
    Public Sub PlaySingleEvent(tev As TrackEventX)
        If IsPatternPlayerRunning = False Then StartPatternPlayer()
        PatternPlayer.PlayEvent(PatternPlayerTime, PatternPlayerTime, tev)
    End Sub


    ''' <summary>
    ''' Create a Pattern from a list of TrackEventX. Duration is time of last event, rounded up to next beat.
    ''' </summary>
    ''' <param name="items">Source Eventlist</param>
    ''' <param name="TPQsource">Source Eventlist</param>
    ''' <returns>Sequence with events related to PlayerTPQ</returns>
    Public Function CreatePattern(items As IList, TPQsource As Integer) As Pattern
        Return CreatePattern(items, TPQsource, 0)
    End Function
    ''' <summary>
    ''' Create a Sequence from a list of TrackEventX
    ''' </summary>
    ''' <param name="items">Source Eventlist</param>
    ''' <param name="TPQsource">Source Eventlist</param>
    ''' <param name="NumBeats">Desired length in beats (quaters). If Zero, duration is time of last event
    '''  rounded up to time of next beat. If the desired length is shorter than the event list, it will be ignored</param>
    ''' <returns>Sequence with events related to PlayerTPQ</returns>
    Public Function CreatePattern(items As IList, TPQsource As Integer, NumBeats As UShort) As Pattern
        Dim seq As New Pattern
        If items Is Nothing Then Return seq             ' no data
        'If items.Count = 0 Then Return seq              ' no data

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
        End If

        '--- set length an duration

        If NumBeats = 0 Then
            If seq.EventList.Count > 0 Then
                ' time of last event, round up to next beat (beat aligned sequence)
                seq.Length = GetTimeOfNextBeat(seq.EventList.Last.Time)
                seq.Duration = seq.Length
            Else
                ' if no events in list -> Length and Duration are 1 beat
                seq.Length = PlayerTPQ
                seq.Duration = seq.Length
            End If

        Else
            Dim len As UInteger
            If seq.EventList.Count > 0 Then
                len = GetTimeOfNextBeat(seq.EventList.Last.Time)
            End If
            ' desired length can be longer but not shorter than Eventlist
            If NumBeats * PlayerTPQ > len Then
                len = NumBeats * PlayerTPQ
            End If
            seq.Length = len
            seq.Duration = seq.Length
        End If

        Return seq
    End Function

    ''' <summary>
    ''' Remove a Pattern from the PatternList an reset Pattern.ID to 0
    ''' </summary>    
    Public Sub RemovePattern(pat As Pattern)
        If pat Is Nothing Then Exit Sub
        If pat.ID = 0 Then Exit Sub                     ' nothing to do

        For i = 0 To PatternList.Count - 1
            If PatternList(i).ID = pat.ID Then
                PatternList.RemoveAt(i)
                Exit For
            End If
        Next

        pat.ID = 0
    End Sub

#End Region


#Region "Pattern Builder"

    Public Function PatternBuilder(count As Byte, dur As NoteDuration) As Pattern
        Dim seq As New Pattern


        Dim time As UInteger

        For i = 1 To count
            Dim tev As New TrackEventX
            tev.Time = time
            tev.Status = &H90
            tev.Data1 = 41                  ' note number
            tev.Data2 = 100                 ' velocity
            tev.Duration = dur
            seq.EventList.Add(tev)

            time += dur
        Next


        'seq.EventList


        Return seq
    End Function



#End Region

    Public Class Pattern                               ' Fixed TPQ: always 480
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

    Public Function GetTimeOfNextMeasure(Time As UInteger) As UInteger
        Dim Newtime As UInteger
        Dim TicksPerUnit As Integer = 4 * PlayerTPQ          ' here a unit is a measure = 4 * quaterNote Length
        Dim ElapsedTicks As UInteger                        ' in this unit
        Dim RemainingTicks As UInteger                      ' in this unit     

        ElapsedTicks = CUInt(Time Mod TicksPerUnit)
        RemainingTicks = CUInt(TicksPerUnit - ElapsedTicks)
        Newtime = Time + RemainingTicks                     ' round up to end of this unit

        Return Newtime
    End Function


End Module
