﻿Public Module MModule1


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
        ''' Comparer for sorting the EventList by Time and Track
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

            Return 0
        End Function
    End Class



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
                    Case MetaEventType.MIDIChannelPrefix
                        Return EventTypeX.MIDIChannelPrefix
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
        MIDIChannelPrefix = &H20
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
        'MidiEvent = 1        ' channel message
        NoteOffEvent = &H180
        NoteOnEvent = &H190
        PolyKeyPressure = &H1A0
        ControlChange = &H1B0
        ProgramChange = &H1C0
        ChannelPressure = &H1D0
        PitchBend = &H1E0

        F0SysExEvent = &HF000       ' &HF0 / 240     normal sysx
        F7SysExEvent = &HF700       ' &HF7 / 247     escape sysx
        'MetaEvent = &HFF00         ' &HFF / 255    

        SequenceNumber = &HFF00
        TextEvent = &HFF01
        CopyrightNotice = &HFF02
        SequenceOrTrackName = &HFF03
        InstrumentName = &HFF04
        Lyric = &HFF05
        Marker = &HFF06
        CuePoint = &HFF07
        MIDIChannelPrefix = &HFF20
        EndOfTrack = &HFF2F
        SetTempo = &HFF51
        SMPTEOffset = &HFF54
        TimeSignature = &HFF58
        KeySignature = &HFF59
        SequencerSpecific = &HFF7F
    End Enum

End Module
