Imports System.IO
Imports System.Text

Public Class MidiFileWrite

    '--- Information for caller ---
    Public ReadOnly Property BytesWritten As Long
    Public ReadOnly Property ErrorCode As ErrorNum                  ' NoError if ok, ErrorNumber if failed
    Public ReadOnly Property ErrorMessage As String = ""            ' additional information about the error

    '--- Header ---
    Private HeaderChunkType As Byte() = Encoding.UTF8.GetBytes("MThd")
    '--- the following members need to be converted to big-endian before write
    Private HeaderLength As UInteger = 6     ' =6 (number of following bytes)
    Private SmfFormat As UShort = 1         ' format 0, contains a single multi-channel track
    '                                         format 1, vertically one-dimensional form (one ore more tracks)
    '                                         format 2, horizontally one-dimensional form (one ore more tracks)
    Private SmfNumberOfTracks As UShort = 1 ' number of tracks
    Private SmfDivision As UShort = 96      ' ticks per quarter-note or SMPTE (if bit 15 = 1)

    '--- Track chunk ---
    Private Class TrackChunk
        Public TrackChunkType As Byte() = Encoding.UTF8.GetBytes("MTrk")
        Public DataLength As UInteger
        Public EventList As New List(Of TrackEvent)                 ' track data (delta time, event)
    End Class

    '--- Tracks ---
    Private TrackList As New List(Of TrackChunk)

    '--- Auxiliary ---
    Private TrackNumberList As New List(Of Byte)


    ''' <summary>
    ''' Write Midifile Format 0 or 1
    ''' </summary>
    ''' <param name="evlic"></param>
    ''' <param name="destinationTPQ"></param>
    ''' <param name="fullname">path, filename and extension (.mid)</param>
    ''' <param name="desired_SMF_Format">0 or 1, otherwise Format 1 is selected</param>
    ''' <returns></returns>
    Public Function WriteMidiFile(evlic As EventListContainer, destinationTPQ As Integer, fullname As String, desired_SMF_Format As Byte) As Boolean
        If desired_SMF_Format = 0 Then
            Return WriteMidiFile_F0(evlic, destinationTPQ, fullname)
        Else
            Return WriteMidiFile_F1(evlic, destinationTPQ, fullname)
        End If
    End Function

    ''' <summary>
    ''' Write Midifile Format 1
    ''' </summary>
    ''' <param name="evlic"></param>
    ''' <param name="destinationTPQ"></param>
    ''' <param name="fullname">path, filename and extension (.mid)</param>
    ''' <returns></returns>
    Public Function WriteMidiFile(evlic As EventListContainer, destinationTPQ As Integer, fullname As String) As Boolean
        Return WriteMidiFile_F1(evlic, destinationTPQ, fullname)
    End Function


#Region "Format 0"

    ''' <summary>
    ''' Format 0
    ''' </summary>    
    ''' <returns>True if ok, False if failed</returns>
    Private Function WriteMidiFile_F0(evlic As EventListContainer, destinationTPQ As Integer, fullname As String) As Boolean
        If evlic Is Nothing Then Return False
        If fullname Is Nothing Then Return False
        If fullname = "" Then Return False

        '--- reset return informations ---
        _BytesWritten = 0
        _ErrorCode = ErrorNum.NoError
        _ErrorMessage = ""

        '---

        Try
            Prepare_F0(evlic, destinationTPQ)
        Catch ex As Exception
            _ErrorCode = ErrorNum.PrepareException
            _ErrorMessage = ex.Message
            Return False
        End Try

        '--- write to file ---
        Try
            WriteMidiFile(File.Create(fullname))
        Catch ex As Exception
            _ErrorCode = ErrorNum.WriteException
            _ErrorMessage = ex.Message
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Format 0
    ''' </summary>    
    Private Sub Prepare_F0(evlic As EventListContainer, destinationTPQ As Integer)
        ' assume evlic IsNot Nothing

        '--- remove End of Track for induvidual Tracks in source ---

        For i = evlic.EventList.Count - 1 To 0 Step -1
            If evlic.EventList(i).TypeX = EventTypeX.EndOfTrack Then
                evlic.EventList.RemoveAt(i)
            End If
        Next

        '--- make sure all NoteOff events are present ---
        CompleteNoteOffs(evlic.EventList)

        '--- to desired TPQ ---

        For Each ev In evlic.EventList
            ev.Time = ConvertTime(ev.Time, evlic.TPQ, destinationTPQ)
            ev.Duration = ConvertTime(ev.Duration, evlic.TPQ, destinationTPQ)
        Next

        '--- list of track numbers (don't have to be a list of continous numbers)
        For Each ev In evlic.EventList
            If TrackNumberList.Contains(ev.TrackNumber) = False Then
                TrackNumberList.Add(ev.TrackNumber)
            End If
        Next

        If TrackNumberList.Count = 0 Then Exit Sub

        '--- create list of tracks ---
        Dim TrackChunk As TrackChunk

        'For Each trknum In TrackNumberList
        TrackChunk = New TrackChunk
        For Each ev In evlic.EventList
            'If trknum = ev.TrackNumber Then
            TrackChunk.EventList.Add(ConvertTrackEvent(ev))
            If TrackChunk.EventList.Last.Type = EventType.Unkown Then
                Dim i As Integer = ev.Type
            End If
            'End If
        Next
        TrackList.Add(TrackChunk)
        'Next


        Dim dbgunkcount As Integer

        For Each trk In TrackList
            For Each ev In trk.EventList
                If ev.Type = EventType.Unkown Then
                    dbgunkcount += 1
                End If
            Next
        Next


        '--- insert End of Track if necessary
        Dim lastev As TrackEvent
        Dim eot As TrackEvent

        For Each trk In TrackList
            If trk.EventList.Count > 0 Then
                lastev = trk.EventList(trk.EventList.Count - 1)
                If lastev.Type <> EventType.MetaEvent Then
                    If lastev.Data1 <> &H2F Then
                        eot = New TrackEvent With {.Status = &HFF, .Data1 = &H2F, .Data2 = 0, .Type = EventType.MetaEvent}
                        eot.Time = trk.EventList(trk.EventList.Count - 1).Time
                        trk.EventList.Add(eot)
                    End If
                End If
            Else
                eot = New TrackEvent With {.Status = &HFF, .Data1 = &H2F, .Data2 = 0, .Type = EventType.MetaEvent}
                trk.EventList.Add(eot)
            End If
        Next

        '--- absoulute time to delta time

        Dim delta As UInteger
        Dim lasttime As UInteger

        Dim dbgTrkcount As Integer
        Dim dbgEvcount As Integer

        For Each trk In TrackList
            lasttime = 0
            dbgEvcount = 0
            For Each ev In trk.EventList
                delta = ev.Time - lasttime
                lasttime = ev.Time
                ev.Time = delta
                dbgEvcount += 1
            Next
            dbgTrkcount += 1
        Next

        '--- set Header vars ---
        SmfFormat = 0
        SmfNumberOfTracks = TrackList.Count
        SmfDivision = destinationTPQ

    End Sub

#End Region

#Region "Format 1"

    ''' <summary>
    ''' Format 1
    ''' </summary>    
    ''' <returns>True if ok, False if failed</returns>
    Private Function WriteMidiFile_F1(evlic As EventListContainer, destinationTPQ As Integer, fullname As String) As Boolean
        If evlic Is Nothing Then Return False
        If fullname Is Nothing Then Return False
        If fullname = "" Then Return False

        '--- reset return informations ---
        _BytesWritten = 0
        _ErrorCode = ErrorNum.NoError
        _ErrorMessage = ""

        '---

        Try
            Prepare_F1(evlic, destinationTPQ)
        Catch ex As Exception
            _ErrorCode = ErrorNum.PrepareException
            _ErrorMessage = ex.Message
            Return False
        End Try

        '--- write to file ---
        Try
            WriteMidiFile(File.Create(fullname))              ' TestMidi.mid
        Catch ex As Exception
            _ErrorCode = ErrorNum.WriteException
            _ErrorMessage = ex.Message
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Format 1
    ''' </summary>    
    Private Sub Prepare_F1(evlic As EventListContainer, destinationTPQ As Integer)
        ' assume evlic IsNot Nothing
        '--- make sure all NoteOff events as present ---
        CompleteNoteOffs(evlic.EventList)

        '--- to desired TPQ ---

        For Each ev In evlic.EventList
            ev.Time = ConvertTime(ev.Time, evlic.TPQ, destinationTPQ)
            ev.Duration = ConvertTime(ev.Duration, evlic.TPQ, destinationTPQ)
        Next

        '--- list of track numbers (don't have to be a list of continous numbers)
        For Each ev In evlic.EventList
            If TrackNumberList.Contains(ev.TrackNumber) = False Then
                TrackNumberList.Add(ev.TrackNumber)
            End If
        Next

        If TrackNumberList.Count = 0 Then Exit Sub

        '--- create list of tracks ---
        Dim TrackChunk As TrackChunk

        For Each trknum In TrackNumberList
            TrackChunk = New TrackChunk
            For Each ev In evlic.EventList
                If trknum = ev.TrackNumber Then
                    TrackChunk.EventList.Add(ConvertTrackEvent(ev))
                    If TrackChunk.EventList.Last.Type = EventType.Unkown Then
                        Dim i As Integer = ev.Type
                    End If
                End If
            Next
            TrackList.Add(TrackChunk)
        Next


        Dim dbgunkcount As Integer

        For Each trk In TrackList
            For Each ev In trk.EventList
                If ev.Type = EventType.Unkown Then
                    dbgunkcount += 1
                End If
            Next
        Next

        '--- insert End of Track if necessary
        Dim lastev As TrackEvent
        Dim eot As TrackEvent

        For Each trk In TrackList
            If trk.EventList.Count > 0 Then
                lastev = trk.EventList(trk.EventList.Count - 1)
                If lastev.Type <> EventType.MetaEvent Then
                    If lastev.Data1 <> &H2F Then
                        eot = New TrackEvent With {.Status = &HFF, .Data1 = &H2F, .Data2 = 0, .Type = EventType.MetaEvent}
                        eot.Time = trk.EventList(trk.EventList.Count - 1).Time
                        trk.EventList.Add(eot)
                    End If
                End If
            Else
                eot = New TrackEvent With {.Status = &HFF, .Data1 = &H2F, .Data2 = 0, .Type = EventType.MetaEvent}
                trk.EventList.Add(eot)
            End If
        Next

        '--- absoulute time to delta time

        Dim delta As UInteger
        Dim lasttime As UInteger

        Dim dbgTrkcount As Integer
        Dim dbgEvcount As Integer

        For Each trk In TrackList
            lasttime = 0
            dbgEvcount = 0
            For Each ev In trk.EventList
                delta = ev.Time - lasttime
                lasttime = ev.Time
                ev.Time = delta
                dbgEvcount += 1
            Next
            dbgTrkcount += 1
        Next

        '--- set Header vars ---
        SmfFormat = 1
        SmfNumberOfTracks = TrackList.Count
        SmfDivision = destinationTPQ

    End Sub

#End Region

#Region "Writer"
    ''' <summary>
    ''' Instance for writing Variable-Length data
    ''' </summary>
    Private varlen As New SmfVariableLength

    ''' <summary>
    ''' Primarily for writing to a file. By using the abstract Stream class, other results can also be achieved,
    ''' such as writing to a memory stream.
    ''' </summary>
    ''' <param name="strm">Stream used by BinaryWriter. This can be a file stream, a memory stream
    ''' or another stream type.</param>
    Private Sub WriteMidiFile(strm As Stream)
        Using writer = New BinaryWriter(strm)
            writer.Write(HeaderChunkType)
            writer.Write(UIntToBe(HeaderLength))
            writer.Write(UShortToBe(SmfFormat))
            writer.Write(UShortToBe(SmfNumberOfTracks))
            writer.Write(UShortToBe(SmfDivision))


            Dim lastStatus As Byte
            Dim stat As Byte

            For Each trk In TrackList
                writer.Write(trk.TrackChunkType)
                Dim trackSizePosition = writer.BaseStream.Position
                writer.Write(UIntToBe(trk.DataLength))                  ' 0 at this time
                lastStatus = 0

                For Each ev In trk.EventList

                    Select Case ev.Type
                        Case EventType.MidiEvent
                            varlen.Write(ev.Time, writer)
                            'WriteMidiEvent(ev, writer)

                            If lastStatus <> ev.Status Then             ' skip if runnung state
                                writer.Write(ev.Status)                 ' first byte
                                lastStatus = ev.Status
                            End If

                            writer.Write(ev.Data1)                      ' second byte

                            stat = CByte(ev.Status And &HF0)
                            If Not (stat = &HC0 Or stat = &HD0) Then    ' ProgramChange and Aftertouch have a length of 2 bytes
                                writer.Write(ev.Data2)                  ' write third byte for all except &HC0 and &HD0
                            End If

                        Case EventType.MetaEvent
                            WriteMetaEvent(ev, writer)
                            lastStatus = 0

                        Case EventType.F0SysxEvent
                            varlen.Write(ev.Time, writer)                   ' time
                            ' the sequence in file is: F0 <Length> <bytes after F0 including F7>                            
                            WriteByte(writer, EventType.F0SysxEvent)        ' F0   
                            If ev.DataX.Count > 0 Then
                                varlen.Write(ev.DataX.Count - 1, writer)    ' length
                                WriteDataBytes(ev.DataX, 1, writer)         ' bytes after F0
                            Else
                                varlen.Write(0, writer)                     ' length = 0
                            End If
                            lastStatus = 0

                        Case EventType.F7SysxEvent
                            varlen.Write(ev.Time, writer)                   ' time
                            ' the sequence in file is: F7 <Length> <bytes to be transmitted>
                            WriteByte(writer, EventType.F7SysxEvent)        ' F7  
                            If ev.DataX.Count > 0 Then
                                varlen.Write(ev.DataX.Count - 1, writer)    ' length
                                WriteDataBytes(ev.DataX, 1, writer)         ' bytes to be transmitted
                            Else
                                varlen.Write(0, writer)                     ' length = 0
                            End If
                            lastStatus = 0

                        Case EventType.Unkown

                    End Select

                Next

                Dim trackChunkLength As UInteger = CUInt(writer.BaseStream.Position - trackSizePosition) - 4
                writer.BaseStream.Position = trackSizePosition
                writer.Write(UIntToBe(trackChunkLength))
                writer.BaseStream.Position += trackChunkLength
            Next

            _BytesWritten = writer.BaseStream.Length

        End Using
    End Sub

    Private Sub WriteMidiEvent(ev As TrackEvent, writer As BinaryWriter)

        writer.Write(ev.Status)                         ' first byte
        writer.Write(ev.Data1)                          ' second byte

        Dim stat As Byte = CByte(ev.Status And &HF0)
        If Not (stat = &HC0 Or stat = &HD0) Then        ' ProgramChange and Aftertouch have a length of 2 bytes
            writer.Write(ev.Data2)                      ' write third byte for all except &HC0 and &HD0
        End If

    End Sub

    Private Sub WriteMetaEvent(ev As TrackEvent, writer As BinaryWriter)

        Select Case ev.Data1

            Case MetaEventType.SequenceNumber                           ' FF 00 02 ssss
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.SequenceNumber)         ' 00
                WriteByte(writer, 2)                                    ' 02    (length=2)
                WriteByte(writer, ev.DataX(0))                          ' Sequence Number MSB
                WriteByte(writer, ev.DataX(1))                          ' Sequence Number LSB

            Case MetaEventType.TextEvent                                ' FF 01 len text
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.TextEvent)              ' 01
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.CopyrightNotice                          ' FF 02 len text
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.CopyrightNotice)        ' 02
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.SequenceOrTrackName                      ' FF 03 len text
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.SequenceOrTrackName)    ' 03
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.InstrumentName                           ' FF 04 len text
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.InstrumentName)         ' 04
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.Lyric                                    ' FF 05 len text
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.Lyric)                  ' 05
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.Marker                                   ' FF 06 len text
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.Marker)                 ' 06
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.CuePoint                                 ' FF 07 len text
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.CuePoint)               ' 07
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.ProgramName                              ' FF 08 len text   (RP-019 1999 MMA)
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.ProgramName)            ' 08
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.DeviceName                               ' FF 09 len text   (RP-019 1999 MMA)
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.DeviceName)             ' 09
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' text

            Case MetaEventType.MIDIChannelPrefix                        ' FF 20 01 cc
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.MIDIChannelPrefix)      ' &H20
                WriteByte(writer, 1)                                    ' 01    (length=1)
                WriteByte(writer, ev.DataX(0))                          ' cc

            ' MIDIPortPrefix is obsolete and should not be used anymore, instead DeviceName should be used.
            ' It is implemented for compatibility with old MidiFiles which may still contain it
            Case MetaEventType.MIDIPortPrefix                           ' FF 21 01 pp
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.MIDIPortPrefix)         ' &H21
                WriteByte(writer, 1)                                    ' 01    (length=1)
                WriteByte(writer, ev.DataX(0))                          ' pp

            Case MetaEventType.EndOfTrack                               ' FF 2F 00
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.EndOfTrack)             ' 2F    (Databyte 1)
                WriteByte(writer, 0)                                    ' 00    (Databyte 2)

            Case MetaEventType.SetTempo                                 ' FF 51 03 tttttt
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.SetTempo)               ' &H51
                WriteByte(writer, 3)                                    ' 03    (length=3)
                WriteDataBytes(ev.DataX, writer, 3)

            Case MetaEventType.SMPTEOffset                              ' FF 54 05 hr mn se fr ff
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.SMPTEOffset)             ' &H54
                WriteByte(writer, 5)                                    ' 05    (length=5)
                WriteDataBytes(ev.DataX, writer, 5)

            Case MetaEventType.TimeSignature                            ' FF 58 04 nn dd cc bb
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.TimeSignature)          ' &H58
                WriteByte(writer, 4)                                    ' 04    (length=4)
                WriteDataBytes(ev.DataX, writer, 4)

            Case MetaEventType.KeySignature                             ' FF 59 02 sf mi
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.KeySignature)           ' &H59
                WriteByte(writer, 2)                                    ' 02    (length=2)
                WriteDataBytes(ev.DataX, writer, 2)

            Case MetaEventType.SequencerSpecific                        ' FF 7F len data
                varlen.Write(ev.Time, writer)                           ' time
                WriteByte(writer, EventType.MetaEvent)                  ' FF    (Status)
                WriteByte(writer, MetaEventType.SequencerSpecific)      ' &H7F               
                WriteDataByteCount(ev.DataX, writer)                    ' length
                WriteDataBytes(ev.DataX, writer)                        ' data

        End Select


    End Sub

    Private Sub WriteByte(writer As BinaryWriter, value As Byte)
        writer.Write(value)
    End Sub

    ''' <summary>
    ''' Writes all bytes contained in the buffer.
    ''' Works also when no data is present and therefore buffer is Nothing.
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <param name="writer"></param>
    Private Sub WriteDataBytes(buffer As Byte(), writer As BinaryWriter)
        If buffer Is Nothing Then Exit Sub
        For i = 1 To buffer.Count
            writer.Write(buffer(i - 1))
        Next
    End Sub

    ''' <summary>
    ''' Writes all bytes contained in the buffer.
    ''' Works also when no data is present and therefore buffer is Nothing.
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <param name="srcOffset">Zero-based byte offset in buffer. Start writing from this position</param>
    ''' <param name="writer"></param>
    Private Sub WriteDataBytes(buffer As Byte(), srcOffset As Integer, writer As BinaryWriter)
        If buffer Is Nothing Then Exit Sub
        For i = srcOffset + 1 To buffer.Count
            writer.Write(buffer(i - 1))
        Next
    End Sub

    ''' <summary>
    ''' Writes the number of bytes in the buffer in variable-length format.
    ''' Uses normally buffer.count, but works also when no data is present and therefore buffer is Nothing.
    '''  In this case, a value of 0 is written.
    ''' </summary>    
    Private Sub WriteDataByteCount(buffer As Byte(), writer As BinaryWriter)
        Dim bytecount As Integer
        If buffer IsNot Nothing Then
            bytecount = buffer.Count
        End If
        varlen.Write(bytecount, writer)
    End Sub

    ''' <summary>
    ''' Writes a nuber of data bytes. Assumes that buffer is not Nothing and contains at least [count] bytes.
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <param name="writer"></param>
    ''' <param name="count">The number of bytes to write.</param>
    Private Sub WriteDataBytes(buffer As Byte(), writer As BinaryWriter, count As Integer)
        For i = 1 To count
            writer.Write(buffer(i - 1))
        Next
    End Sub

#End Region

End Class

