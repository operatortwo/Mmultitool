Imports System.IO

Public Class MidifileRead

    Public ReadOnly Property MidiFullname As String = ""
    Public ReadOnly Property MidiName As String

    ''' <summary>
    ''' TRUE if Midi-File loaded without errors and Tracks in Tracklist are filled with Track-Events
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property FileLoaded As Boolean
    Public Property ErrorCode As Integer
    Public Property ErrorText As String = ""

    Private Const MIN_MIDIFILE_SIZE = 14                    ' minimal size of a Midi file (Header)
    Private Const MAX_MIDIFILE_SIZE = 10 * 1024 * 1024      ' maximal size of a Midi file (myLimit)
    Private Const MinHeaderDataLenght = 6                   ' MinDataLength of Midi-File Header (normal = 6)
    Private Const MAX_NUMBER_OF_TRACKS = 120                ' maximal number of tracks (myLimit)

    '--- Header ---

    Public ReadOnly Property FileSize As Long               ' size on disk
    Public ReadOnly Property SmfFormat As Integer           ' 0 or 1 or 2
    Public ReadOnly Property NumberOfTracks As Integer      ' Track count (all Tracks, including unknown)
    Public ReadOnly Property TPQ As Integer                 ' Ticks per QuaterNote

    'Private HeaderDateLength As Integer                     ' for calculating the next Track-Position

    '---

    Public ReadOnly Property TimeSignature_Num As Integer = 4           ' Numerator 4 (default TiSig.4/4)
    Public ReadOnly Property TimeSignature_Denom As Integer = 4         ' Nenner 4 (default TiSig.4/4)
    Public ReadOnly Property BPM As Single = 120

    Public ReadOnly Property LastTick As UInteger       ' greatest event time of the last event of all tracks

    Public ReadOnly Property HasMultichannelTrack As Boolean        ' 1 or more tracks uses more the 1 channel
    '---

    Public TrackList As New List(Of TrackChunk)                    ' only Tracks with "MTrk"-Sig.

    'Public Class TrackChunk
    '    ''' <summary>
    '    ''' Position of Datablock in SourceStream
    '    ''' </summary>
    '    Public DataPosition As Integer                              ' Position of Datablock in SourceStream
    '    ''' <summary>
    '    ''' Number of bytes in Datablock
    '    ''' </summary>
    '    Public DataLength As Integer                                ' Number of bytes in Datablock     
    '    ''' <summary>
    '    ''' List of TrackEvents
    '    ''' </summary>
    '    Public EventList As New List(Of TrackEvent)
    '    ''' <summary>
    '    ''' for Player (current Event)
    '    ''' </summary>
    '    Public EventPtr As Integer                                  ' for Player (current Event)
    '    ''' <summary>
    '    ''' for Player (True if end reached)
    '    ''' </summary>
    '    Public EndOfTrack As Boolean                                ' for Player (True if end reached)
    '    ''' <summary>
    '    ''' If True: skip NoteOn-Events in Player
    '    ''' </summary>
    '    Public Mute As Boolean                                      ' True = skip NoteOn-Events in Player
    '    ''' <summary>
    '    ''' Track Filter for Aux-Operations
    '    ''' </summary>
    '    Public XSelect As Boolean                                   ' Track Filter for Aux-Operations
    '   ''' <summary>
    '   ''' 0 or 1 for SingleChannelTrack, > 1 for MultichannelTracks   
    '   ''' </summary>
    '    Public NumberOfChannels As Byte          ' > 1 = Multichannel Tracks in SMF 0 or SMF 1 Format (SMF2 ?)
    'End Class


    Private RunningStatusByte As Byte                                   ' cache

    Public Function ReadMidiFile(fullname As String) As Boolean

        Initialize()                                                   ' load defaults

        If fullname Is Nothing Or fullname = "" Then
            ErrorCode = MiErr_EmptyName
            ErrorText = "No filename"
            Return False
        End If

        '--- verify header

        If Check_MidiFileHeader(fullname) = False Then
            If ErrorCode = MiErr_NoError Then
                ErrorCode = MiErr_Unknown
            End If
            Return False
        End If

        '--- verify file (data lengths)

        If Check_MidiFile(fullname) = False Then
            If ErrorCode = MiErr_NoError Then
                ErrorCode = MiErr_Unknown
            End If
            Return False
        End If

        '--- create TrackList (TrackChunks with "MTrk")

        createTrackList(fullname)

        createEventLists(fullname)

        DeltaTimeToAbs()                    ' Delta time to continuous time

        '--- calculate durations
        ' try to calculate the duration of note-on's from 'time' (for GUI)

        CalculateDurations()                ' unsuccessful events: Duration = 0


        '--- get numer of used channels on each track, we want to avoid Multichannel tracks

        GetNumberOfUsedChannels()
        For Each trk In TrackList
            If trk.NumberOfChannels > 1 Then
                _HasMultichannelTrack = True
                Exit For
            End If
        Next

        '--- Reset Player-vars

        For i = 1 To TrackList.Count
            TrackList(i - 1).EventPtr = 0
            TrackList(i - 1).EndOfTrack = False
        Next

        '--- find LastTick

        Dim time As UInteger

        For i = 1 To TrackList.Count

            If TrackList(i - 1).EventList.Count > 0 Then
                time = TrackList(i - 1).EventList.Last().Time
                If time > LastTick Then
                    _LastTick = time
                End If
            End If
        Next

        '--- successfully loaded

        _MidiFullname = fullname
        _MidiName = Path.GetFileName(fullname)
        '_MidiName = Path.GetFileNameWithoutExtension(fullname)
        _FileLoaded = True

        Return True
    End Function

    Private Sub Initialize()
        _FileLoaded = False
        _MidiFullname = ""
        ErrorText = ""
        ErrorCode = 0
        _TimeSignature_Num = 4
        _TimeSignature_Denom = 4
        _BPM = 120
        _LastTick = 0
        _HasMultichannelTrack = False
    End Sub

    Private Sub DeltaTimeToAbs()

        Dim time As UInteger

        For t = 1 To TrackList.Count

            time = 0
            For e = 1 To TrackList(t - 1).EventList.Count
                time += TrackList(t - 1).EventList(e - 1).Time
                TrackList(t - 1).EventList(e - 1).Time = time
            Next

        Next
    End Sub

    Private Sub CalculateDurations()

        Dim ev As TrackEvent
        Dim stat As Byte

        For t = 1 To TrackList.Count

            For e = 1 To TrackList(t - 1).EventList.Count
                ev = TrackList(t - 1).EventList(e - 1)
                If ev.Type = EventType.MidiEvent Then
                    stat = ev.Status
                    If stat < &HA0 Then
                        If stat > &H8F Then
                            If ev.Data2 > 0 Then
                                ev.Duration = FindNoteOff(TrackList(t - 1).EventList, (e - 1))
                            End If
                        End If
                    End If
                End If
            Next
        Next

    End Sub

    Private Function FindNoteOff(elist As List(Of TrackEvent), position As Integer) As UInteger

        ' assume: current Event is Note-On

        Dim ev As TrackEvent

        Dim stat As Byte                        ' assuming 9nh
        Dim channel As Byte
        Dim noteNum As Byte
        Dim start_time As UInteger

        ' find 1: 

        '--- current event

        ev = elist(position)

        stat = ev.Status
        channel = CByte(ev.Status And &HF)
        noteNum = ev.Data1
        start_time = ev.Time

        '--- find note-off

        For e = position + 2 To elist.Count
            ev = elist(e - 1)

            If ev.Status = stat Then                        ' 9nh
                If ev.Data1 = noteNum Then                  ' same NoteNumber
                    If ev.Data2 = 0 Then                    ' velocity = 0
                        Return ev.Time - start_time
                    End If
                End If
            ElseIf ev.Status = stat - &H10 Then             ' 8nh
                If ev.Data1 = noteNum Then                  ' same NoteNumber
                    Return ev.Time - start_time
                End If
            End If

            '  ev.stat = stat , velo = 0            9nh
            '  ev.stat = stat -10h                  8nh
        Next

        Return 0                                    ' not found
    End Function

    Private Sub GetNumberOfUsedChannels()
        Dim chlist As New List(Of Byte)(16)
        Dim status_high As Byte
        Dim channel As Byte

        For Each trk In TrackList
            chlist.Clear()

            For Each ev In trk.EventList
                If ev.Type = MModule1.EventType.MidiEvent Then
                    status_high = ev.Status And &HF0
                    channel = ev.Status And &HF
                    If status_high >= &H80 AndAlso status_high < &HF0 Then
                        If Not chlist.Contains(channel) Then
                            chlist.Add(channel)
                        End If
                    End If
                End If
            Next
            trk.NumberOfChannels = chlist.Count

        Next

    End Sub

    Private Sub createEventLists(fullname As String)

        Dim fi As New FileInfo(fullname)
        Dim reader As New BinaryReader(fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read))

        Dim el As List(Of TrackEvent)
        Dim ev As TrackEvent
        Dim endOfTrack As Boolean

        For i = 1 To TrackList.Count

            TrackList(i - 1).EventList.Clear()
            el = TrackList(i - 1).EventList
            reader.BaseStream.Position = TrackList(i - 1).DataPosition

            Do
                ev = New TrackEvent
                ev.Time = ReadDeltaTime(reader)
                endOfTrack = ReadEvent(reader, ev)      ' return TRUE if EndOfTrack reached

                If i <= 256 Then
                    ev.TrackNumber = CByte(i - 1)
                Else
                    ev.TrackNumber = 255
                End If

                el.Add(ev)                              ' add event to list

            Loop Until endOfTrack = True
        Next

        reader.Close()                                  ' release unmanaged resources (or using) 

    End Sub

    Private Function ReadEvent(reader As BinaryReader, ByRef ev As TrackEvent) As Boolean

        Dim Data0 As Byte
        Data0 = reader.ReadByte
        Dim length As Integer

        If Data0 < &HF0 Then                             ' channel event ( 8,9,A,B,C,D,E)
            ev.Type = EventType.MidiEvent

            If Data0 >= &H80 Then
                RunningStatusByte = Data0
                ev.Status = Data0
            Else                                        ' running state
                ev.Status = RunningStatusByte
                reader.BaseStream.Position -= 1         ' back to first Datenbyte
            End If

            ev.Data1 = reader.ReadByte                  ' first Databyte fo all

            Dim dat As Byte = CByte(Data0 And &HF0)     ' C + D = 1 Databyte
            If Not (dat = &HC0 Or dat = &HD0) Then
                ev.Data2 = reader.ReadByte              ' others = 2 Databytes
            End If

        ElseIf Data0 = EventType.MetaEvent Then          ' FF
            ev.Type = EventType.MetaEvent
            ev.Data1 = reader.ReadByte                  ' MetaEventType
            length = ReadVariableLength(reader)         ' Data length (can be 0)            
            ev.DataX = reader.ReadBytes(length)          ' read to Byte()

            If ev.Data1 = &H2F Then                     ' FF 2F 00 = End of Track
                Return True
            End If

        ElseIf Data0 = EventType.F0SysxEvent Then        ' F0
            ev.Type = EventType.F0SysxEvent
            ' the sequence in file is: F0 <Length> <bytes after F0 including F7>
            length = ReadVariableLength(reader)         ' Data length (after F0)                        
            If length > 0 Then
                Dim barr() As Byte
                barr = reader.ReadBytes(length)
                ev.DataX = New Byte(length) {}              ' (upper bound)
                ev.DataX(0) = &HF0
                Buffer.BlockCopy(barr, 0, ev.DataX, 1, length)
            Else
                ev.DataX = New Byte() {}                ' empty byte() instead of Nothing
            End If

        ElseIf Data0 = EventType.F7SysxEvent Then       ' F7            
            ' the sequence in file is: F7 <Length> <bytes to be transmitted>
            ev.Type = EventType.F7SysxEvent
                length = ReadVariableLength(reader)         ' Data length             
            If length > 0 Then
                Dim barr() As Byte
                barr = reader.ReadBytes(length)
                ev.DataX = New Byte(length) {}              ' (upper bound)
                ev.DataX(0) = &HF0
                Buffer.BlockCopy(barr, 0, ev.DataX, 1, length)
            Else
                ev.DataX = New Byte() {}                ' empty byte() instead of Nothing
            End If

        Else
                ev.Type = EventType.Unkown                  ' neither MidiEvent nor MetaEvent nor SysxEvent
            Return False
        End If

        Return False
    End Function

    Private Function ReadDeltaTime(reader As BinaryReader) As UInteger

        Dim time As UInteger
        Dim b1 As Byte

        Try
            For i = 1 To 4
                b1 = reader.ReadByte
                time = time << 7
                time = CUInt(time Or (b1 And &H7F))
                If b1 < &H80 Then Exit Try
            Next
        Catch
            Return 0
        End Try

        Return time
    End Function

    Private Function ReadVariableLength(reader As BinaryReader) As Integer

        Dim length As Integer
        Dim b1 As Byte

        Try
            For i = 1 To 4
                b1 = reader.ReadByte
                length = length << 7
                length = length Or (b1 And &H7F)
                If b1 < &H80 Then Exit Try
            Next
        Catch
            Return 0
        End Try

        Return length
    End Function

    Private Sub createTrackList(fullname As String)
        'Header und File are already checked

        TrackList.Clear()

        Dim fi As New FileInfo(fullname)

        Dim reader As New BinaryReader(fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read))

        Dim chunkType As Char()
        Dim DataLength As UInteger

        ' Position = 0

        '--- Header
        chunkType = reader.ReadChars(4)
        DataLength = BeToUInt(reader.ReadUInt32)
        reader.BaseStream.Position += DataLength        ' HeaderSig(4) + DataLen(4) + DataLength

        '--- Tracks
        For i = 1 To NumberOfTracks
            chunkType = reader.ReadChars(4)
            DataLength = BeToUInt(reader.ReadUInt32)
            If chunkType = "MTrk" Then
                TrackList.Add(New TrackChunk With
                              {.DataLength = CInt(DataLength), .DataPosition = CInt(reader.BaseStream.Position)})
            End If
            reader.BaseStream.Position += DataLength    ' HeaderSig(4) + DataLen(4) + DataLength
        Next

        reader.Close()                                  ' release unmanaged resources (or using) 

    End Sub

    Private Function Check_MidiFileHeader(fullname As String) As Boolean

        Dim fi As New FileInfo(fullname)

        If fi.Exists = False Then
            ErrorCode = MiErr_FileNotExists
            ErrorText = "file not found"
            Return False
        End If

        If fi.Length < MIN_MIDIFILE_SIZE Then
            ErrorCode = MiErr_FileTooShort
            ErrorText = "invalid file size  " & CStr(fi.Length)
            Return False
        End If

        If fi.Length > MAX_MIDIFILE_SIZE Then
            ErrorCode = MiErr_FileTooLong
            ErrorText = "File is too big (application restriction)  "
            Return False
        End If


        _FileSize = fi.Length                    ' Length of the file

        '----

        Dim chunkType As Char()
        Dim length As UInteger
        Dim format As UShort
        Dim ntracks As UShort
        Dim division As UShort

        Dim reader As New BinaryReader(fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read))

        chunkType = reader.ReadChars(4)
        length = BeToUInt(reader.ReadUInt32)
        format = BeToUShort(reader.ReadUInt16)
        ntracks = BeToUShort(reader.ReadUInt16)
        division = BeToUShort(reader.ReadUInt16)

        reader.Close()                                  ' release unmanaged resources

        If chunkType <> "MThd" Then
            ErrorCode = MiErr_HeaderChunkSignature
            ErrorText = "Invalid header-chunk-type signature"
            Return False
        End If

        If length < MinHeaderDataLenght Then                ' normally = 6 (00 00 00 06)
            '                                               ' could get bigger in the future
            ErrorCode = MiErr_MinHeaderDataLength
            ErrorText = "Invalid header length"
            Return False
        End If

        If (division >> 15) > 0 Then
            ErrorCode = MiErr_TimeFormat_SMPTE
            ErrorText = "SMPTE format is currently not supported"
            Return False
        End If

        If format > 2 Then
            ErrorCode = MiErr_InvalidMidiFormat
            ErrorText = "Invalid midi format number"
            Return False
        End If

        If ntracks = 0 Then
            ErrorCode = MiErr_HeaderNoTracks
            ErrorText = "Number of tracks in the header is 0"
            Return False
        End If

        If format = 0 Then
            If ntracks > 1 Then
                ErrorCode = MiErr_Format0_MoreThanOneTrack
                ErrorText = "Format 0 cannot have more than 1 track"
                Return False
            End If
        ElseIf format = 1 Then
            If ntracks > MAX_NUMBER_OF_TRACKS Then
                ErrorCode = MiErr_Format1_TooManyTracks
                ErrorText = "Format 1 has too many tracks for this application"
            End If

        ElseIf format = 2 Then
            If ntracks > 1 Then
                ErrorCode = MiErr_Format0_MoreThanOneTrack
                ErrorText = "Format 2 cannot have more than 1 track"
                Return False
            End If
        End If

        If division = 0 Then
            ErrorCode = MiErr_DivisionIsNull
            ErrorText = "Division (TPQ) is null"
            Return False
        End If

        ' Header ist valid

        _SmfFormat = format
        _NumberOfTracks = ntracks
        _TPQ = division

        Return True
    End Function


    Private Function Check_MidiFile(fullname As String) As Boolean
        ' Header ist already checked

        Dim fi As New FileInfo(fullname)

        Dim reader As New BinaryReader(fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read))

        Dim chunkType As Char()
        Dim DataLength As UInteger
        Dim ret As Boolean = True                       ' return value

        ' Position = 0

        Try
            '--- Header
            chunkType = reader.ReadChars(4)
            DataLength = BeToUInt(reader.ReadUInt32)
            reader.BaseStream.Position += DataLength        ' HeaderSig(4) + DataLen(4) + DataLength

            '--- Tracks
            For i = 1 To NumberOfTracks
                chunkType = reader.ReadChars(4)
                DataLength = BeToUInt(reader.ReadUInt32)
                reader.BaseStream.Position += DataLength    ' HeaderSig(4) + DataLen(4) + DataLength
            Next
        Catch
            ret = False                                 ' error
            ErrorCode = MiErr_ReadingChunkChain
            ErrorText = "Exception while reading Chunk-Chain"
        End Try

        'If Not reader.BaseStream.Position = reader.BaseStream.Length Then
        '    ret = False
        '    ErrorCode = MiErr_ReadingChunkChain
        '    ErrorText = "Wrong data-length in last Chunk"
        'End If

        reader.Close()                                  ' release unmanaged resources (or using)

        Return ret
    End Function

#Region "Convert Multichannel Tracks"
    ''' <summary>
    ''' Convert Multichannel Tracks to Singlechannel Tracks. Replace current tracklist.
    ''' </summary>
    ''' <returns>True if if successfully converted. False if nothing to convert or failed to convert.</returns>
    Public Function ConvertMultichannelTracks() As Boolean
        If HasMultichannelTrack = False Then Return False
        If TrackList.Count = 0 Then Return False

        Dim tlist2 As New List(Of TrackChunk)

        Try
            For Each trk In TrackList

                If trk.NumberOfChannels <= 1 Then
                    tlist2.Add(trk)                                 ' copy unchanged
                Else
                    '--- split 
                    Dim src As TrackChunk
                    src = trk
                    Dim dst As New List(Of TrackChunk)
                    For di = 1 To 16                                ' all 16 midi channels
                        dst.Add(New TrackChunk)
                    Next

                    Dim chn As Byte
                    For Each srcev In src.EventList
                        chn = GetChannelNumberOfEvent(srcev)
                        dst(chn).EventList.Add(srcev)
                    Next

                    '- remove unused TracksChunks
                    For t = 15 To 0 Step -1
                        If dst(t).EventList.Count = 0 Then
                            dst.RemoveAt(t)
                        End If
                    Next

                    '- add each track to tl2
                    For Each tc In dst
                        tlist2.Add(tc)
                    Next

                End If

            Next

            '--- insert track number to each event ---

            Dim newtrknum As Byte = 0

            For Each trk In tlist2
                For Each ev In trk.EventList
                    ev.TrackNumber = newtrknum
                Next

                If newtrknum < 255 Then
                    newtrknum += 1
                End If
            Next

        Catch ex As Exception
            Return False                ' tracklist is unchanged
        End Try


        TrackList = tlist2
        _NumberOfTracks = TrackList.Count
        _HasMultichannelTrack = False
        Return True
    End Function

    Private Function GetChannelNumberOfEvent(trev As TrackEvent) As Byte
        Dim stat As Byte
        Dim channel As Byte

        stat = trev.Status And &HF0
        If stat >= &H80 And stat < &HF0 Then        ' exclude F0, F7    (choose channel 0)
            channel = trev.Status And &HF
        End If

        Return channel
    End Function

#End Region


#Region "Converter Constants and Enums"

    ''' <summary>
    ''' Conversion from Big-Endian to Little-Endian Format. 4 Bytes to UInteger
    ''' </summary>
    ''' <param name="BigEndian"></param>
    ''' <returns></returns>
    Private Function BeToUInt(BigEndian As UInteger) As UInteger

        Dim ret As UInteger

        Dim b1 As Byte
        Dim b2 As Byte
        Dim b3 As Byte
        Dim b4 As Byte

        b1 = CByte(BigEndian >> 24 And &HFF)
        b2 = CByte(BigEndian >> 16 And &HFF)
        b3 = CByte(BigEndian >> 8 And &HFF)
        b4 = CByte(BigEndian And &HFF)

        ret = b4
        ret = (ret << 8) Or b3
        ret = (ret << 8) Or b2
        ret = (ret << 8) Or b1

        Return ret
    End Function

    Private Function BeToUShort(BigEndian As UShort) As UShort

        Dim ret As UShort

        Dim b1 As Byte
        Dim b2 As Byte

        b1 = CByte(BigEndian >> 8 And &HFF)
        b2 = CByte(BigEndian And &HFF)

        ret = b2
        ret = (ret << 8) Or b1

        Return ret
    End Function


    Friend Const MiErr_NoError = 0

    Friend Const MiErr_EmptyName = 11                  ' fullname is nothing or empty
    Friend Const MiErr_FileNotExists = 12              ' fullname not exists
    Friend Const MiErr_FileTooShort = 13               ' file is < 14 bytes
    Friend Const MiErr_FileTooLong = 14                ' file is > Max. bytes
    Friend Const MiErr_HeaderChunkSignature = 15       ' Header-Chunk <> "MThd"
    Friend Const MiErr_MinHeaderDataLength = 16        ' Length < 6
    Friend Const MiErr_TimeFormat_SMPTE = 17           ' TimeFormat SMPTE is not supported by this application
    Friend Const MiErr_InvalidMidiFormat = 18          ' Format not 0 or 1 or 2
    Friend Const MiErr_HeaderNoTracks = 19             ' Number of Track in Header = 0
    Friend Const MiErr_Format0_MoreThanOneTrack = 20   ' Format 0 can only have 1 track
    Friend Const MiErr_Format1_TooManyTracks = 21      ' Format 1 has application-defined limits for the number of tracks
    Friend Const MiErr_Format2_MoreThanOneTrack = 22   ' Format 2 can only have 1 track
    Friend Const MiErr_DivisionIsNull = 23             ' Division / Ticks per QuaterNote can not be 0
    Friend Const MiErr_ReadingChunkChain = 24          ' ChunkHeader + DataLen points to next Chunk until eof

    Friend Const MiErr_Unknown = &H10001


    Public Enum EventType
        Unkown = 0
        MidiEvent = 1                   ' channel message
        MetaEvent = &HFF                ' MetaEvent
        F0SysxEvent = &HF0              ' normal sysx
        F7SysxEvent = &HF7              ' escape sysx
    End Enum


    '--- unused ? (0 references) ---
    Public Enum MidiEventType
        NoteOffEvent = &H80
        NoteOnEvent = &H90
        PolyKeyPressure = &HA0
        ControlChange = &HB0
        ProgramChange = &HC0
        ChannelPressure = &HD0
        PitchBend = &HE0
    End Enum

    '--- unused ? (0 references) ---
    Public Enum MetaEventType
        SequenceNumber = 0
        TextEvent = 1
        CopyrightNotice = 2
        SequenceOrTrackName = 3
        InstrumentName = 4
        Lyric = 5
        Marker = 6
        CuePoint = 7
        ProgramName = 8                     ' (RP-019 1999 MMA) 
        DeviceName = 9                      ' (RP-019 1999 MMA)
        MIDIChannelPrefix = &H20
        MIDIPortPrefix = &H21               ' obsolete, for read only, use DeviceName instead
        EndOfTrack = &H2F
        SetTempo = &H51
        SMPTEOffset = &H54
        TimeSignature = &H58
        KeySignature = &H59
        SequencerSpecific = &H7F
    End Enum

#End Region

End Class
