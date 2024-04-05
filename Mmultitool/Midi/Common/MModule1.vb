Imports MS.Internal

Public Module MModule1

    ''' <summary>
    ''' Contains data of a midifile track-chunk
    ''' </summary>
    Public Class TrackChunk
        ''' <summary>
        ''' Position of Datablock in SourceStream
        ''' </summary>
        Public DataPosition As Integer                              ' Position of Datablock in SourceStream
        ''' <summary>
        ''' Number of bytes in Datablock
        ''' </summary>
        Public DataLength As Integer                                ' Number of bytes in Datablock     
        ''' <summary>
        ''' List of TrackEvents
        ''' </summary>
        Public EventList As New List(Of TrackEvent)
        ''' <summary>
        ''' for Player (current Event)
        ''' </summary>
        Public EventPtr As Integer                                  ' for Player (current Event)
        ''' <summary>
        ''' for Player (True if end reached)
        ''' </summary>
        Public EndOfTrack As Boolean                                ' for Player (True if end reached)
        ''' <summary>
        ''' If True: skip NoteOn-Events in Player
        ''' </summary>
        Public Mute As Boolean                                      ' True = skip NoteOn-Events in Player
        ''' <summary>
        ''' Track Filter for Aux-Operations
        ''' </summary>
        Public XSelect As Boolean                                   ' Track Filter for Aux-Operations
        ''' <summary>
        ''' 0 or 1 for SingleChannelTrack, > 1 for MultichannelTracks   
        ''' </summary>
        Public NumberOfChannels As Byte             ' > 1 = Multichannel Tracks in SMF 0 or SMF 1 Format (SMF2 ?)
    End Class

    ''' <summary>
    ''' Contains data used in EventList member of TrackChunk
    ''' </summary>
    Public Class TrackEvent
        ' For the WPF data binding, the members must be defined as properties (not as fields) 
        ' (otherwise nothing or only the type name is displayed in a ListView)
        ''' <summary>
        ''' [Ticks] for all Events
        ''' </summary>        
        Public Property Time As UInteger                 '                   for all Events        
        Public Property Type As EventType                '                   for all Events
        Public Property Status As Byte                   ' Status            for MidiEvents
        'Public Channel As Byte                  ' Channel (0-0Fh)   for MidiEvents  (needed ?)
        ''' <summary>
        ''' If MidiEvent: Data-Byte 1 / If MetaEvent: MetaEventType
        ''' </summary>
        Public Property Data1 As Byte                    ' Data 1            for MidiEvents and MetaEvents
        ''' <summary>
        ''' for MidiEvents
        ''' </summary>
        Public Property Data2 As Byte                    ' Data 2            for MidiEvents
        ''' <summary>
        ''' Data Array for MetaEvents and SysxEvents
        ''' </summary>
        Public Property DataX As Byte()                   ' Data Array        for MetaEvents and SysxEvents
        ''' <summary>
        ''' Aux for Note-On Events. Calcualted from Time until Note-off. 0 if no Note-Off.
        ''' Can be used for Graphical User Interface.
        ''' </summary>
        Public Property Duration As UInteger
        ''' <summary>
        ''' For filtering, muting, ... / TrackNumbers > 255 will be set to 255 / First Track = 0
        ''' </summary>
        Public Property TrackNumber As Byte
    End Class

    ''' <summary>
    ''' Contains data used in EventList (extended TrackEvent)
    ''' </summary>
    <Serializable>                                  ' needed for copy and paste
    Public Class TrackEventX
        Implements IComparer(Of TrackEventX)
        ''' <summary>
        ''' [Ticks] for all Events
        ''' </summary>
        ''' <returns></returns>
        Public Property Time As UInteger

        Public Property TypeX As EventTypeX

        Public Property Status As Byte

        ''' <summary>
        ''' If MidiEvent: Data-Byte 1 / If MetaEvent: MetaEventType
        ''' </summary>
        ''' <returns></returns>
        Public Property Data1 As Byte

        ''' <summary>
        ''' for MidiEvents
        ''' </summary>
        ''' <returns></returns>
        Public Property Data2 As Byte

        ''' <summary>
        ''' Data Array for MetaEvents and SysxEvents
        ''' </summary>
        ''' <returns></returns>
        Public Property DataX As Byte()

        ''' <summary>
        ''' [Ticks] for Note-On Events. Calculated from Time until Note-off. 0 if no Note-Off.
        ''' </summary>
        ''' <returns></returns>
        Public Property Duration As UInteger

        ''' <summary>
        ''' Reserved for future use, can define different devices, default = 0
        ''' </summary>
        ''' <returns></returns>
        Public Property Port As Byte
        ''' <summary>
        ''' For filtering, muting, ... / TrackNumbers > 255 will be set to 255 / First Track = 0
        ''' </summary>
        Public Property TrackNumber As Byte

        '--- additional Properties ---

        Public Property Channel As Byte


        Public Property DataStr As String

        Public Property Type As EventType

        ''' <summary>
        ''' Comparer for sorting the EventList by Time and Track. -1 means x is less than y
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Function Compare(x As TrackEventX, y As TrackEventX) As Integer Implements IComparer(Of TrackEventX).Compare
            ' Return Value              Meaning
            '---------------------------------------------
            ' Less than zero    (-1)    x is less than y
            ' Zero              (0)     x equals y
            ' Greater than zero (1)     x is greater than y

            If x Is Nothing Then Return -1
            If y Is Nothing Then Return 1

            If x.Time < y.Time Then Return -1
            If x.Time > y.Time Then Return 1

            '--- x.Time = y.Time --            
            If x.TrackNumber < y.TrackNumber Then Return -1
            If x.TrackNumber > y.TrackNumber Then Return 1

            '--- x.Time = y.Time AND x.Track = y.Track
            '--- when Time and Track are equal, all event types are sorted before EndOfTrack
            If x.TypeX = EventTypeX.EndOfTrack Then Return 1
            If y.TypeX = EventTypeX.EndOfTrack Then Return -1


            '--- when x and y are NoteOn or NoteOff on the same note on the same channel then NoteOff before NoteOn
            If x.Channel = y.Channel Then
                If x.TypeX = EventTypeX.NoteOnEvent OrElse x.TypeX = EventTypeX.NoteOffEvent Then
                    If y.TypeX = EventTypeX.NoteOnEvent OrElse y.TypeX = EventTypeX.NoteOffEvent Then
                        If x.TypeX <> y.TypeX Then
                            If x.Data1 = y.Data1 Then
                                If x.TypeX = EventTypeX.NoteOffEvent Then Return -1
                                If y.TypeX = EventTypeX.NoteOffEvent Then Return 1
                            End If
                        End If
                    End If
                End If
            End If

            '--- when x and y are control change, sort by controller number, needed for (BankSelect MSB, LSB, Prog chg.)

            If x.TypeX = EventTypeX.ControlChange Then
                If y.TypeX = EventTypeX.ControlChange Then
                    If x.Data1 < y.Data1 Then Return -1
                    If x.Data1 > y.Data1 Then Return 1
                    Return 0
                End If
            End If

            '--- at this point time, track and channel are equal

            Dim wx As Integer = GetEventSortWeight(x.TypeX)
            Dim wy As Integer = GetEventSortWeight(y.TypeX)

            If wx < wy Then Return -1
            If wx > wy Then Return 1

            Return 0                                                ' is equal
        End Function

#Region "EventTypeX Sort Weight"

        Private Function GetEventSortWeight(TypeX As EventTypeX) As Integer

            If TypeX < &HFF04 Then
                If TypeX < &H1E0 Then
                    Select Case TypeX
                        Case EventTypeX.Unkown
                            Return EventSortWeight.SW_unknown
                        Case EventTypeX.NoteOffEvent
                            Return EventSortWeight.SW_NoteOff
                        Case EventTypeX.NoteOnEvent
                            Return EventSortWeight.SW_NoteOn
                        Case EventTypeX.PolyKeyPressure
                            Return EventSortWeight.SW_PoyKeyPressure
                        Case EventTypeX.ControlChange
                            Return EventSortWeight.SW_ControlChange
                        Case EventTypeX.ProgramChange
                            Return EventSortWeight.SW_ProgramChange
                        Case EventTypeX.ChannelPressure
                            Return EventSortWeight.SW_ChannelPressure
                    End Select
                Else
                    Select Case TypeX
                        Case EventTypeX.PitchBend
                            Return EventSortWeight.SW_PitchBend
                        Case EventTypeX.F0SysExEvent
                            Return EventSortWeight.SW_F0SysExEvent
                        Case EventTypeX.F7SysExEvent
                            Return EventSortWeight.SW_F7SysExEvent
                        Case EventTypeX.SequenceNumber
                            Return EventSortWeight.SW_SequenceNumber
                        Case EventTypeX.TextEvent
                            Return EventSortWeight.SW_TextEvent
                        Case EventTypeX.CopyrightNotice
                            Return EventSortWeight.SW_CopyrightNotice
                        Case EventTypeX.SequenceOrTrackName
                            Return EventSortWeight.SW_SequenceOrTrackName
                    End Select
                End If
            Else
                If TypeX < &HFF21 Then
                    Select Case TypeX
                        Case EventTypeX.InstrumentName
                            Return EventSortWeight.SW_InstrumentName
                        Case EventTypeX.Lyric
                            Return EventSortWeight.SW_Lyric
                        Case EventTypeX.Marker
                            Return EventSortWeight.SW_Marker
                        Case EventTypeX.CuePoint
                            Return EventSortWeight.SW_CuePoint
                        Case EventTypeX.ProgramName
                            Return EventSortWeight.SW_ProgramName
                        Case EventTypeX.DeviceName
                            Return EventSortWeight.SW_DeviceName
                        Case EventTypeX.MIDIChannelPrefix
                            Return EventSortWeight.SW_MidiChannelPrefix
                    End Select
                Else
                    Select Case TypeX
                        Case EventTypeX.MIDIPortPrefix
                            Return EventSortWeight.SW_MidiPortPrefix
                        Case EventTypeX.EndOfTrack
                            Return EventSortWeight.SW_EndOfTrack
                        Case EventTypeX.SetTempo
                            Return EventSortWeight.SW_SetTempo
                        Case EventTypeX.SMPTEOffset
                            Return EventSortWeight.SW_SMPTEOffset
                        Case EventTypeX.TimeSignature
                            Return EventSortWeight.SW_TimeSignature
                        Case EventTypeX.KeySignature
                            Return EventSortWeight.SW_KeySignature
                        Case EventTypeX.SequencerSpecific
                            Return EventSortWeight.SW_SequencerSpecific
                    End Select
                End If
            End If

            Return 270
        End Function


        Private Enum EventSortWeight
            SW_SequenceNumber = 10
            SW_KeySignature = 20
            SW_TimeSignature = 30
            SW_SetTempo = 40
            SW_SMPTEOffset = 50
            SW_MidiChannelPrefix = 60
            SW_MidiPortPrefix = 70
            SW_CopyrightNotice = 80
            SW_SequenceOrTrackName = 90
            SW_InstrumentName = 100
            SW_DeviceName = 110
            SW_ProgramName = 120
            SW_Marker = 130
            SW_CuePoint = 140
            SW_TextEvent = 150
            SW_Lyric = 160
            SW_SequencerSpecific = 170
            SW_F0SysExEvent = 180
            SW_F7SysExEvent = 190
            SW_ControlChange = 200
            SW_ProgramChange = 210
            SW_PitchBend = 220
            SW_NoteOff = 230
            SW_NoteOn = 240
            SW_ChannelPressure = 250
            SW_PoyKeyPressure = 260
            SW_unknown = 270
            SW_EndOfTrack = 280
        End Enum


        ' Name		            Constant    Weight
        ' ----------------------------------------
        ' SequenceNumber	    &HFF00		10
        ' KeySignature		    &HFF59		20
        ' TimeSignature		    &HFF58		30
        ' SetTempo		        &HFF51		40
        ' SMPTEOffset		    &HFF54		50
        ' MidiChannelPrefix	    &HFF20		60
        ' MidiPortPrefix	    &HFF21		70
        ' CopyrightNotice	    &HFF02		80
        ' SequenceOrTrackName	&HFF03		90
        ' InstrumentName		&HFF04		100
        ' DeviceName		    &HFF09		110
        ' ProgramName		    &HFF08		120
        ' Marker		        &HFF06		130
        ' CuePoint		        &HFF07		140
        ' TextEvent		        &HFF01		150
        ' Lyric		            &HFF05		160
        ' SequencerSpecific		&HFF7F		170
        ' F0SysExEvent		    &HF000		180
        ' F7SysExEvent		    &HF700		190
        ' ProgramChange		    &H1C0		200
        ' PitchBend		        &H1E0		210
        ' ControlChange		    &H1B0		220
        ' NoteOff		        &H180		230
        ' NoteOn		        &H190		240
        ' ChannelPressure		&H1D0		250
        ' PoyKeyPressure		&H1A0		260
        ' unknown		        &H0		    270
        ' EndOfTrack		    &HFF2F		280

#End Region

        ''' <summary>
        ''' Make a deep copy af this TrackEventX.
        ''' </summary>
        ''' <returns></returns>
        Public Function Copy() As TrackEventX
            Dim tev2 As New TrackEventX
            tev2.Time = Time
            tev2.TypeX = TypeX
            tev2.Status = Status
            tev2.Data1 = Data1
            tev2.Data2 = Data2
            'Public Property DataX As Byte()
            If DataX IsNot Nothing Then
                'If DataX.Count > 0 Then
                Dim dx2 As Byte() = New Byte(DataX.Count - 1) {}        ' byte(upperBound)
                    For i = 1 To DataX.Count
                        dx2(i - 1) = DataX(i - 1)
                    Next
                    tev2.DataX = dx2
                'End If
            End If
            tev2.Duration = Duration
            tev2.Port = Port
            tev2.TrackNumber = TrackNumber
            tev2.Channel = Channel
            If DataStr IsNot Nothing Then
                tev2.DataStr = String.Copy(DataStr)
            End If
            tev2.Type = Type
            Return tev2
        End Function

        ''' <summary>
        ''' Make a deep copy af this TrackEventX.
        ''' </summary>
        ''' <param name="CopyDataStr">Determines whether DataStr should also be copied. 
        ''' Setting to False can save space when used for playing only.</param>
        ''' <returns></returns>
        Public Function Copy(CopyDataStr As Boolean) As TrackEventX
            Dim tev2 As New TrackEventX
            tev2.Time = Time
            tev2.TypeX = TypeX
            tev2.Status = Status
            tev2.Data1 = Data1
            tev2.Data2 = Data2
            'Public Property DataX As Byte()
            If DataX IsNot Nothing Then
                If DataX.Count > 0 Then
                    Dim dx2 As Byte() = New Byte(DataX.Count - 1) {}        ' byte(upperBound)
                    For i = 1 To DataX.Count
                        dx2(i - 1) = DataX(i - 1)
                    Next
                    tev2.DataX = dx2
                End If
            End If
            tev2.Duration = Duration
            tev2.Port = Port
            tev2.TrackNumber = TrackNumber
            tev2.Channel = Channel
            If CopyDataStr = True Then
                If DataStr IsNot Nothing Then
                    tev2.DataStr = String.Copy(DataStr)
                End If
            End If
                tev2.Type = Type
            Return tev2
        End Function


    End Class

    Public Function GetEventType(trev As EventTypeX) As EventType
        Dim num As Integer = trev
        num = num And &HFFFF
        num = num >> 8
        Return num                              ' low byte
    End Function

    ''' <summary>
    ''' Examines the specified TrackEvent and returns the equivalent TrackEventX type.
    ''' </summary>
    ''' <param name="trev"></param>
    ''' <returns></returns>
    Public Function GetEventTypeX(trev As TrackEvent) As EventTypeX
        If trev Is Nothing Then Return EventTypeX.Unkown

        Dim stat As Byte

        Select Case trev.Type
            Case EventType.MidiEvent
                stat = trev.Status And &HF0

                If (stat = &H90 And trev.Data2 = 0) OrElse (stat = &H80) Then Return EventTypeX.NoteOffEvent
                Select Case stat
                    Case MidiEventType.NoteOnEvent
                        Return EventTypeX.NoteOnEvent
                    Case MidiEventType.PolyKeyPressure
                        Return EventTypeX.PolyKeyPressure
                    Case MidiEventType.ControlChange
                        Return EventTypeX.ControlChange
                    Case MidiEventType.ProgramChange
                        Return EventTypeX.ProgramChange
                    Case MidiEventType.ChannelPressure
                        Return EventTypeX.ChannelPressure
                    Case MidiEventType.PitchBend
                        Return EventTypeX.PitchBend
                End Select

            Case EventType.F0SysxEvent
                Return EventTypeX.F0SysExEvent
            Case EventType.F7SysxEvent
                Return EventTypeX.F7SysExEvent

            Case EventType.MetaEvent
                Select Case trev.Data1
                    Case MetaEventType.SequenceNumber
                        Return EventTypeX.SequenceNumber
                    Case MetaEventType.TextEvent
                        Return EventTypeX.TextEvent
                    Case MetaEventType.CopyrightNotice
                        Return EventTypeX.CopyrightNotice
                    Case MetaEventType.SequenceOrTrackName
                        Return EventTypeX.SequenceOrTrackName
                    Case MetaEventType.InstrumentName
                        Return EventTypeX.InstrumentName
                    Case MetaEventType.Lyric
                        Return EventTypeX.Lyric
                    Case MetaEventType.Marker
                        Return EventTypeX.Marker
                    Case MetaEventType.CuePoint
                        Return EventTypeX.CuePoint
                    Case MetaEventType.ProgramName          ' (RP-019 1999 MMA) FF 08 len text
                        Return EventTypeX.ProgramName
                    Case MetaEventType.DeviceName           ' (RP-019 1999 MMA) FF 09 len text
                        Return EventTypeX.DeviceName
                    Case MetaEventType.MIDIChannelPrefix
                        Return EventTypeX.MIDIChannelPrefix
                    Case MetaEventType.MIDIPortPrefix       ' obsolete, for read only, use DeviceName instead
                        Return EventTypeX.MIDIPortPrefix
                    Case MetaEventType.EndOfTrack
                        Return EventTypeX.EndOfTrack
                    Case MetaEventType.SetTempo
                        Return EventTypeX.SetTempo
                    Case MetaEventType.SMPTEOffset
                        Return EventTypeX.SMPTEOffset
                    Case MetaEventType.TimeSignature
                        Return EventTypeX.TimeSignature
                    Case MetaEventType.KeySignature
                        Return EventTypeX.KeySignature
                    Case MetaEventType.SequencerSpecific
                        Return EventTypeX.SequencerSpecific
                End Select


        End Select

        Return EventTypeX.Unkown
    End Function

    ''' <summary>
    ''' According to standard midi file specification
    ''' </summary>
    Public Enum EventType
        Unkown = 0
        MidiEvent = 1           ' channel message
        F0SysxEvent = 240       ' &HF0 / 240    normal sysx
        F7SysxEvent = 247       ' &HF7 / 247    escape sysx
        MetaEvent = 255         ' &HFF / 255    
    End Enum

    ''' <summary>
    '''  According to standard midi file specification
    ''' </summary>
    Public Enum MidiEventType
        NoteOffEvent = &H80
        NoteOnEvent = &H90
        PolyKeyPressure = &HA0
        ControlChange = &HB0
        ProgramChange = &HC0
        ChannelPressure = &HD0
        PitchBend = &HE0
    End Enum

    ''' <summary>
    '''  According to standard midi file specification
    ''' </summary>
    Public Enum MetaEventType
        SequenceNumber = 0
        TextEvent = 1
        CopyrightNotice = 2
        SequenceOrTrackName = 3
        InstrumentName = 4
        Lyric = 5
        Marker = 6
        CuePoint = 7
        ProgramName = 8                     ' (RP-019 1999 MMA) FF 08 len text
        DeviceName = 9                      ' (RP-019 1999 MMA) FF 09 len text
        MIDIChannelPrefix = &H20
        MIDIPortPrefix = &H21               ' obsolete, for read only, use DeviceName instead   FF 21 01 pp 
        EndOfTrack = &H2F
        SetTempo = &H51
        SMPTEOffset = &H54
        TimeSignature = &H58
        KeySignature = &H59
        SequencerSpecific = &H7F
    End Enum

    ''' <summary>
    ''' Linear event types used in TrackEventX / EventList
    ''' </summary>
    Public Enum EventTypeX
        Unkown = 0
        'MidiEvent = 1              ' channel message
        NoteOffEvent = &H180
        NoteOnEvent = &H190
        PolyKeyPressure = &H1A0
        ControlChange = &H1B0
        ProgramChange = &H1C0
        ChannelPressure = &H1D0
        PitchBend = &H1E0

        F0SysExEvent = &HF000       ' &HF0 / 240     normal sysEx
        F7SysExEvent = &HF700       ' &HF7 / 247     escape sysEx

        'MetaEvent = &HFF00         ' &HFF / 255    
        SequenceNumber = &HFF00
        TextEvent = &HFF01
        CopyrightNotice = &HFF02
        SequenceOrTrackName = &HFF03
        InstrumentName = &HFF04
        Lyric = &HFF05
        Marker = &HFF06
        CuePoint = &HFF07
        ProgramName = &HFF08                ' (RP-019 1999 MMA) FF 08 len text
        DeviceName = &HFF09                 ' (RP-019 1999 MMA) FF 09 len text
        MIDIChannelPrefix = &HFF20
        MIDIPortPrefix = &HFF21             ' obsolete, for read only, use DeviceName instead   FF 21 01 pp 
        EndOfTrack = &HFF2F
        SetTempo = &HFF51
        SMPTEOffset = &HFF54
        TimeSignature = &HFF58
        KeySignature = &HFF59
        SequencerSpecific = &HFF7F
    End Enum


    Public Function TimeTo_MBT_0(time As Long) As String

        Dim meas As Long                            ' measure (assume: 4/4)
        Dim beat As Integer                         ' beat inside measure
        Dim ticks As Integer

        meas = time \ (4 * PlayerTPQ)            '  \ = returns an integer result        
        beat = CInt((time \ PlayerTPQ) Mod 4)
        ticks = CInt(time Mod PlayerTPQ)

        Return CStr(meas) & " : " & CStr(beat) & " : " & CStr(ticks)                ' base 0
    End Function

    Public Function TimeTo_MBT_0(time As Long, tpq As Integer) As String
        If tpq < 1 Then Return ""

        Dim meas As Long                            ' measure (assume: 4/4)
        Dim beat As Integer                         ' beat inside measure
        Dim ticks As Integer

        meas = time \ (4 * tpq)            '  \ = returns an integer result        
        beat = CInt((time \ tpq) Mod 4)
        ticks = CInt(time Mod tpq)

        Return CStr(meas) & " : " & CStr(beat) & " : " & CStr(ticks)                ' base 0
    End Function

    ''' <summary>
    ''' Convert time or duration according to the TPQ ratio.
    ''' </summary>
    ''' <param name="Time">Time or duration in ticks</param>
    ''' <param name="SourceTPQ"></param>
    ''' <param name="DestinationTPQ"></param>
    ''' <returns>Converted time in ticks according to destination TPQ</returns>
    Public Function ConvertTime(Time As UInteger, SourceTPQ As Integer, DestinationTPQ As Integer) As UInteger
        If SourceTPQ = DestinationTPQ Then Return Time
        If SourceTPQ = 0 Then Return Time
        Return Time * DestinationTPQ / SourceTPQ
    End Function
    ''' <summary>
    ''' Convert time or duration into sequencer time, which has a fixed TPQ of 960
    ''' </summary>
    ''' <param name="Time">Time or duration in ticks</param>
    ''' <param name="SourceTPQ"></param>
    ''' <returns>Converted time in sequencer ticks</returns>
    Public Function ToSeqTime(Time As UInteger, SourceTPQ As Integer) As UInteger
        If SourceTPQ = 0 Then Return Time
        If SourceTPQ = PlayerTPQ Then Return Time
        Return Time * PlayerTPQ / SourceTPQ
    End Function


    ''' <summary>
    ''' Get time of next beat mark
    ''' </summary>
    ''' <param name="Time">Time 1</param>
    ''' <returns>Time 1 rounded up to next beat mark</returns>
    Public Function GetTimeOfNextBeat(Time As UInteger, TPQ As Integer) As UInteger
        If TPQ < 1 Then Return Time
        Dim Newtime As UInteger
        Dim TicksPerUnit As Integer = TPQ                   ' here a unit is a beat = 1 quaterNote Length
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
    Public Function GetTimeOfPreviousBeat(Time As UInteger, TPQ As Integer) As UInteger
        If TPQ < 1 Then Return Time
        Dim Newtime As UInteger
        Dim TicksPerUnit As UInteger = TPQ                  ' here a unit is a beat = 1 quaterNote Length
        Dim ElapsedTicks As UInteger                        ' in this unit

        ElapsedTicks = Time Mod TicksPerUnit
        Newtime = Time - ElapsedTicks                       ' round down to start of this unit

        Return Newtime
    End Function


End Module
