Imports System.IO

Partial Public Class MidiFileWrite


    Private Function ConvertTrackEvent(trevx As TrackEventX) As TrackEvent
        Dim trev As New TrackEvent

        Select Case trevx.TypeX
                'MidiEvent = 1        ' channel message
            Case EventTypeX.NoteOffEvent
                CopyMidiEvent(trevx, trev)
            Case EventTypeX.NoteOnEvent
                CopyMidiEvent(trevx, trev)
            Case EventTypeX.PolyKeyPressure
                CopyMidiEvent(trevx, trev)
            Case EventTypeX.ControlChange
                CopyMidiEvent(trevx, trev)
            Case EventTypeX.ProgramChange
                CopyMidiEvent(trevx, trev)
            Case EventTypeX.ChannelPressure
                CopyMidiEvent(trevx, trev)
            Case EventTypeX.PitchBend
                CopyMidiEvent(trevx, trev)

            Case EventTypeX.F0SysExEvent       ' &HF0 / 240     normal sysx
                ' F0 <length> <bytes to be transmitted after F0>               
                trev.Type = EventType.F0SysxEvent
                trev.TrackNumber = trevx.TrackNumber
                trev.Time = trevx.Time
                trev.Status = EventType.F0SysxEvent
                trev.DataX = CopyByteArray(trevx.DataX)

            Case EventTypeX.F7SysExEvent        ' &HF7 / 247     escape sysx
                ' F7 <length> <all bytes to be transmitted>
                trev.Type = EventType.F7SysxEvent
                trev.TrackNumber = trevx.TrackNumber
                trev.Time = trevx.Time
                trev.Status = EventType.F7SysxEvent
                trev.DataX = CopyByteArray(trevx.DataX)

                'MetaEvent = &HFF00         ' &HFF / 255    
            Case EventTypeX.SequenceNumber
                'FF 00 02 ssss (16bit)
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 0                          ' = MetaEventType.SequenceNumber
                trev.Data2 = 2
            Case EventTypeX.TextEvent
                ' FF 01 len text
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 1
            Case EventTypeX.CopyrightNotice
                ' FF 02 len text
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 2
            Case EventTypeX.SequenceOrTrackName
                ' FF 03 len text
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 3
            Case EventTypeX.InstrumentName
                'FF 04 len text
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 4
            Case EventTypeX.Lyric
                'FF 05 len text
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 5
            Case EventTypeX.Marker
                ' FF 06 len text
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 6
            Case EventTypeX.CuePoint
                ' FF 07 len text
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 7
            Case EventTypeX.ProgramName
                ' FF 08 len text   (RP-019 1999 MMA)
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 8
            Case EventTypeX.DeviceName
                ' FF 09 len text   (RP-019 1999 MMA)
                CopyMetaEvent(trevx, trev)
                trev.Data1 = 9
            Case EventTypeX.MIDIChannelPrefix
                ' FF 20 01 cc
                CopyMetaEvent(trevx, trev)
                trev.Data1 = &H20
            Case EventTypeX.MIDIPortPrefix
                ' FF 21 01 pp               obsolete, but implemented for compatibility with old midifiles
                CopyMetaEvent(trevx, trev)
                trev.Data1 = &H21
            Case EventTypeX.EndOfTrack
                ' FF 2F 00
                CopyMetaEvent(trevx, trev)
                trev.Data1 = &H2F
                trev.Data2 = 0
            Case EventTypeX.SetTempo
                ' FF 51 03 tttttt (3 bytes)
                CopyMetaEvent(trevx, trev)
                trev.Data1 = &H51
                trev.Data2 = 3
            Case EventTypeX.SMPTEOffset
                ' FF 54 05 hr mn se fr ff
                CopyMetaEvent(trevx, trev)
                trev.Data1 = &H54
                trev.Data2 = 6
            Case EventTypeX.TimeSignature
                ' FF 58 04 nn dd cc bb
                CopyMetaEvent(trevx, trev)
                trev.Data1 = &H58
                trev.Data2 = 4
            Case EventTypeX.KeySignature
                ' FF 59 02 sf mi
                CopyMetaEvent(trevx, trev)
                trev.Data1 = &H59
                trev.Data2 = 2
            Case EventTypeX.SequencerSpecific
                ' FF 7F len data
                CopyMetaEvent(trevx, trev)
                trev.Data1 = &H7F
            Case Else
                ' unknown
                trev.Time = trevx.Time
                trev.Type = EventType.Unkown
        End Select

        Return trev
    End Function


    Private Sub CopyMidiEvent(trevx As TrackEventX, ByRef ret As TrackEvent)
        ret.Type = EventType.MidiEvent
        ret.Time = trevx.Time
        ret.Duration = trevx.Duration
        ret.Data1 = trevx.Data1
        ret.Data2 = trevx.Data2
        ret.Status = trevx.Status Or trevx.Channel
        ret.TrackNumber = trevx.TrackNumber
    End Sub

    Private Sub CopyMetaEvent(trevx As TrackEventX, ByRef ret As TrackEvent)
        ret.Type = EventType.MetaEvent
        ret.Time = trevx.Time
        ret.Duration = trevx.Duration
        ' Data1 specifies MetaEventType
        ' Data2 is used for some MetaEvents        
        ret.Status = &HFF
        ret.DataX = CopyByteArray(trevx.DataX)
    End Sub


    Private Sub CompleteNoteOffs(ByRef evlist As List(Of TrackEventX))
        If evlist Is Nothing Then Exit Sub

        Dim evlist2 As New List(Of TrackEventX)

        For Each ev In evlist
            If ev.TypeX <> EventTypeX.NoteOffEvent Then
                evlist2.Add(ev)
            End If
        Next

        Dim noff As TrackEventX
        For Each ev In evlist
            If ev.TypeX = EventTypeX.NoteOnEvent Then
                noff = New TrackEventX
                noff.TypeX = EventTypeX.NoteOffEvent
                noff.Type = EventType.MidiEvent
                noff.Status = ev.Status
                noff.Data1 = ev.Data1
                noff.Data2 = 0
                noff.Time = ev.Time + ev.Duration
                noff.Channel = ev.Channel
                noff.Port = ev.Port
                noff.TrackNumber = ev.TrackNumber
                evlist2.Add(noff)
            End If
        Next

        Dim sc As New TrackEventX
        evlist2.Sort(sc)
        evlist = evlist2

    End Sub


    ''' <summary>
    ''' A helper class for writing Standard MidiFile variable-length values
    ''' </summary>
    Private Class SmfVariableLength
        Private Const MaxValue = &HFFFFFFF
        Public ReadOnly Property ByteCount As Byte
        Public ReadOnly Property Buffer As Byte() = New Byte(3) {}

        ''' <summary>
        ''' Converts Input to variable-length bytes. Writes the bytes into the Buffer property in Big-Endian
        ''' format and the ByteCount property is set to the number of written bytes in the range of 1 - 4.
        ''' </summary>
        ''' <param name="Value">The input value</param>
        Public Sub UIntToVarlen(Value As UInteger)
            Dim invalue As UInteger
            Dim outvalue As UInteger
            Dim auxbyte As Byte

            If Value > MaxValue Then Value = MaxValue           ' limit input value to max allowed value

            '--- at least 1 byte, even when value is 0 ---
            outvalue = CUInt(Value And &H7F)
            _ByteCount = 1

            invalue = Value
            Do
                invalue >>= 7
                If invalue = 0 Then Exit Do

                outvalue <<= 8
                auxbyte = CByte(invalue And &H7F)
                auxbyte = CByte(auxbyte Or &H80)                ' set highest bit
                outvalue = outvalue Or auxbyte

                _ByteCount = CByte(ByteCount + 1)
            Loop

            '--- clear unused bytes in buffer ---
            For i = Buffer.Length To ByteCount Step -1
                _Buffer(i - 1) = 0
            Next

            '--- fill buffer in Big-Endian format
            For i = 1 To ByteCount
                _Buffer(i - 1) = CByte(outvalue And &HFF)       ' lowest 8 bit
                outvalue >>= 8
            Next
        End Sub

        ''' <summary>
        ''' Writes the input value as variable-length bytes to a stream using the BinaryWriter
        ''' </summary>
        ''' <param name="Value">the input value</param>
        ''' <param name="Writer"></param>
        Public Sub Write(Value As UInteger, Writer As BinaryWriter)
            If Writer Is Nothing Then Exit Sub
            UIntToVarlen(Value)                         ' convert value to variable-length
            For i = 1 To ByteCount
                Writer.Write(Buffer(i - 1))
            Next
        End Sub

        ''' <summary>
        ''' Returns buffer-bytes in hexadecimal format
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return BitConverter.ToString(Buffer, 0, ByteCount)
        End Function

    End Class

    ''' <summary>
    ''' Conversion from Little-Endian to Big-Endian format. 4 bytes to UInteger
    ''' </summary>
    ''' <param name="LittleEndian"></param>
    ''' <returns></returns>
    Private Function UIntToBe(LittleEndian As UInteger) As UInteger

        Dim ret As UInteger

        Dim b1 As Byte
        Dim b2 As Byte
        Dim b3 As Byte
        Dim b4 As Byte

        b1 = CByte(LittleEndian >> 24 And &HFF)
        b2 = CByte(LittleEndian >> 16 And &HFF)
        b3 = CByte(LittleEndian >> 8 And &HFF)
        b4 = CByte(LittleEndian And &HFF)

        ret = b4
        ret = (ret << 8) Or b3
        ret = (ret << 8) Or b2
        ret = (ret << 8) Or b1

        Return ret
    End Function

    ''' <summary>
    '''  Conversion from Little-Endian to Big-Endian format. 4 bytes to UInteger
    ''' </summary>
    ''' <param name="LittleEndian"></param>
    ''' <returns></returns>
    Private Function UShortToBe(LittleEndian As UShort) As UShort

        Dim ret As UShort

        Dim b1 As Byte
        Dim b2 As Byte

        b1 = CByte(LittleEndian >> 8 And &HFF)
        b2 = CByte(LittleEndian And &HFF)

        ret = b2
        ret = (ret << 8) Or b1

        Return ret
    End Function

    Private Function CopyByteArray(src As Byte()) As Byte()
        If src IsNot Nothing Then
            If src.Count > 0 Then
                Dim dx2 As Byte() = New Byte(src.Count - 1) {}        ' byte(upperBound)
                For i = 1 To src.Count
                    dx2(i - 1) = src(i - 1)
                Next
                Return dx2
            End If
        End If
        Return Nothing
    End Function

    Public Enum ErrorNum
        NoError = 0
        PrepareException                ' exception thrown while Prepare
        WriteException                  ' exception thrown while Write
    End Enum

End Class

