Partial Public Module Sequencer

    Public Function CreateSequence(items As IList, TPQsource As Integer) As Sequence
        Dim seq As New Sequence
        If items Is Nothing Then Return seq             ' no data
        If items.Count = 0 Then Return seq              ' no data

        '--- copy the events and adjust time and duration to sequencerTPQ (960)
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


    Public Class Sequence                               ' Fixed TPQ: always 960
        Public Property Name As String = ""             ' need Property for WPF DataBinding
        Public StartTime As UInteger                    ' in Ticks 
        Public StartOffset As UInteger                  ' for repeated play
        Public Length As UInteger                       ' in ticks (1 beat = 960), for the sequence itself
        Public Duration As UInteger                     ' while
        Public DoLoop As Boolean                        ' restart at end (higher priority than Duration)
        Public Ended As Boolean                         ' needed ?
        Public EventListPtr As Integer                  ' ptr to next item in EventList
        Public EventList As New List(Of TrackEventX)
        Public StartValues As New SequenceStartValues   ' optional
    End Class


    Public Class SequenceStartValues
        Public Tempo As Single
        Public ChannelStates As New List(Of SequenceChannelState)       ' only the used channels
    End Class

    Public Class SequenceChannelState                           ' state of 1 channel
        Public ChannelNumber As Byte                            ' 0 - 15
        Public Program As Byte                                  ' ProgramChange (C0)
        Public PitchBending(1) As Byte                          ' upperBound (2 bytes)  PitchBending (E0) 14 bit
        Public ControlList As New Dictionary(Of Byte, Byte)     ' ControlNumber, ControlValue
    End Class

    ''' <summary>
    ''' Get time of next beat mark. Time base is SequencerTPQ.
    ''' </summary>
    ''' <param name="Time">Time 1</param>
    ''' <returns>Time 1 rounded up to next beat mark</returns>
    Public Function GetTimeOfNextBeat(Time As UInteger) As UInteger
        Dim Newtime As UInteger
        Dim TicksPerUnit As Integer = SequencerTPQ          ' here a unit is a beat = 1 quaterNote Length
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
        Dim TicksPerUnit As UInteger = SequencerTPQ           ' here a unit is a beat = 1 quaterNote Length
        Dim ElapsedTicks As UInteger                        ' in this unit

        ElapsedTicks = Time Mod TicksPerUnit
        Newtime = Time - ElapsedTicks                       ' round down to start of this unit

        Return Newtime
    End Function


End Module
