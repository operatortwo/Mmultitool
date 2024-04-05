Imports System.IO
Imports System.Text

Module MDecode
    Public Function GetSubType(ev As TrackEvent) As String

        If ev.Type = EventType.MidiEvent Then
            Return GetSubType_Midi(ev)
        ElseIf ev.Type = EventType.MetaEvent Then
            Return CType(ev.Data1, MetaEventType).ToString()
        ElseIf ev.Type = EventType.F0SysxEvent Then
            Return "F0"
        ElseIf ev.Type = EventType.F7SysxEvent Then
            Return "F7"
        ElseIf ev.Type = EventType.Unkown Then
            Return "unknown EventType"
        End If

        Return "not found"
    End Function

    Private Function GetSubType_Midi(ev As TrackEvent) As String
        Dim stat As Byte = CByte(ev.Status And &HF0)
        If (stat = &H90 And ev.Data2 = 0) OrElse (stat = &H80) Then Return MidiEventType.NoteOffEvent.ToString
        Return CType(stat, MidiEventType).ToString()
    End Function

    Public Function GetChannel(ev As TrackEvent) As String
        If ev.Type = EventType.MidiEvent Then
            Return Hex(ev.Status And &HF)
        Else
            Return Hex(ev.Status And &HF)
            'Return ""
        End If
    End Function

    Public Function GetData(evx As TrackEventX) As String
        Dim ev As New TrackEvent
        ev.Type = evx.Type
        ev.Status = evx.Status
        ev.Data1 = evx.Data1
        ev.Data2 = evx.Data2
        ev.DataX = evx.DataX
        Return GetData(ev)
    End Function

    Public Function GetData(ev As TrackEvent) As String

        Select Case ev.Type
            Case EventType.MidiEvent
                Return GetData_Midi(ev)
            Case EventType.MetaEvent
                Return GetData_Meta(ev)
            Case EventType.F0SysxEvent
                ' Universal Sysx (7F=Realtime,
                ' Universal Sysx (7E=NonRealTime), 7F=Channel, 09=SubID=GM System,01=Enable
                Return Bytes_to_hex_str(ev.DataX)        ' other Sysx
            Case EventType.F7SysxEvent
                Return Bytes_to_hex_str(ev.DataX)
            Case EventType.Unkown
        End Select

        Return ""
    End Function

    Private Function GetData_Midi(ev As TrackEvent) As String
        Dim stat As Byte = CByte(ev.Status And &HF0)

        If stat = MidiEventType.NoteOffEvent Then
            Return Hex(ev.Status) & " " & Hex(ev.Data1) & " " & Hex(ev.Data2) & "  -  " & NoteNr_to_NoteName(ev.Data1)

        ElseIf stat = MidiEventType.NoteOnEvent Then
            Return Hex(ev.Status) & " " & Hex(ev.Data1) & " " & Hex(ev.Data2) & "  -  " & NoteNr_to_NoteName(ev.Data1)

        ElseIf stat = MidiEventType.PolyKeyPressure Then

        ElseIf stat = MidiEventType.ControlChange Then
            Dim str As String
            str = "Ctrl: " & ev.Data1 & "  val: " & ev.Data2 & "  -  "
            str = str & GetControllerName(ev.Data1)
            Return str
        ElseIf stat = MidiEventType.ProgramChange Then
            Return CStr(ev.Data1) & " - " & GetVoiceName(ev.Data1)
        ElseIf stat = MidiEventType.ChannelPressure Then

        ElseIf stat = MidiEventType.PitchBend Then
            Return CStr(ev.Data2 * 128 + ev.Data1 - 8192)              ' center = 8192
            ' byte 2 * 128 + byte 1
        End If

        Return ""
    End Function

    Private Function GetData_Meta(ev As TrackEvent) As String

        Dim type As Byte = ev.Data1

        If type = MetaEventType.EndOfTrack Then Return ""        ' no additional data required

        If ev.DataX Is Nothing Then Return "Nothing"

        'Dim ascii As Encoding = Encoding.ASCII

        If type = MetaEventType.SequenceNumber Then
            If ev.DataX.Length > 0 Then
                Return Bytes_to_hex_str(ev.DataX)
            Else
                Return "len = 0"
            End If
        ElseIf type = MetaEventType.TextEvent Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.CopyrightNotice Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.SequenceOrTrackName Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.InstrumentName Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.Lyric Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.Marker Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.CuePoint Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.ProgramName Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.DeviceName Then
            Return GetMetaEventText(ev)
        ElseIf type = MetaEventType.MIDIChannelPrefix Then
            If ev.DataX.Length > 0 Then                          ' should be 1
                Return CStr(ev.DataX(0))                         ' channel (0-15)
            Else
                Return ""
            End If
        ElseIf type = MetaEventType.MIDIPortPrefix Then
            If ev.DataX.Length > 0 Then                          ' should be 1
                Return CStr(ev.DataX(0))                         ' port (0-15)
            Else
                Return ""
            End If
        ElseIf type = MetaEventType.SetTempo Then
            Dim micros As Integer
            If ev.DataX.Length >= 3 Then
                micros = ev.DataX(0) * 65536 + ev.DataX(1) * 256 + ev.DataX(2)
                Return micros.ToString & "   " & Math.Round(60 * 1000 * 1000 / micros, 2)   ' 2 Decimal places
            Else
                Return "len = 0"
            End If
        ElseIf type = MetaEventType.SMPTEOffset Then
            If ev.DataX.Length > 0 Then
                ' FF 54 05 hr mn se fr ff
                Return Bytes_to_hex_str(ev.DataX)
            Else
                Return "len = 0"
            End If
        ElseIf type = MetaEventType.TimeSignature Then
            If ev.DataX.Length >= 4 Then
                Dim num As Byte = ev.DataX(0)                        ' numerator
                Dim denom As Byte = CByte(2 ^ ev.DataX(1))           ' denominator (denominator, 2 ^ denominator
                Dim mclocks_metronclick As Byte = ev.DataX(2)        ' clocks per metronome click
                Dim num32perquarter As Byte = ev.DataX(3)            ' num of 32/notes in a QuarterNote (24)            
                Return num & "/" & denom & " " & mclocks_metronclick & " " & num32perquarter
            Else
                Return "len = 0"
            End If
        ElseIf type = MetaEventType.KeySignature Then
            ' C  min/maj
            If ev.DataX.Length > 0 Then
                Return Bytes_to_hex_str(ev.DataX)
            Else
                Return "len = 0"
            End If
        ElseIf type = MetaEventType.SequencerSpecific Then
            If ev.DataX.Length > 0 Then
                Return Bytes_to_hex_str(ev.DataX)
            Else
                Return "len = 0"
            End If
        Else
            If ev.DataX.Length > 0 Then
                Return "unknown, " & Bytes_to_hex_str(ev.DataX)
            Else
                Return "unkown, len = 0"
            End If

        End If
        Return ""

    End Function

    Private Function GetMetaEventText_old(ev As TrackEvent) As String
        Dim ascii As Encoding = Encoding.ASCII
        Dim chr As Char()

        If ev.DataX.Length > 0 Then
            chr = ascii.GetChars(ev.DataX)
            Return chr
        Else
            Return "len = 0"
        End If
    End Function

    Private ReadOnly ASCIIenc As New ASCIIEncoding
    Private Function GetMetaEventText(ev As TrackEvent) As String
        If ev.DataX.Length > 0 Then
            Return ASCIIenc.GetString(ev.DataX)
        Else
            Return "len = 0"
        End If
    End Function


    Public Function GetDuration(ev As TrackEvent) As String

        Return CStr(ev.Duration)

        Return ""
    End Function

    Private Class C_octave_key
        Public Property offset As Integer
        Public Property name As String          ' key name
        Public Property white As Boolean        ' is it a white key
        Public Property shape As Integer        ' shape-type (for painting)
        Public Property xbase As Integer        ' pos-x base for painting (nr of white keys from octave-start)
    End Class

    Private octave_keys As New List(Of C_octave_key) From
        {
        New C_octave_key With {.offset = 0, .name = "C", .white = True, .shape = shp_w_r, .xbase = 0},
        New C_octave_key With {.offset = 1, .name = "C#", .white = False, .shape = shp_blk, .xbase = 1},
        New C_octave_key With {.offset = 2, .name = "D", .white = True, .shape = shp_w_lr, .xbase = 1},
        New C_octave_key With {.offset = 3, .name = "D#", .white = False, .shape = shp_blk, .xbase = 2},
        New C_octave_key With {.offset = 4, .name = "E", .white = True, .shape = shp_w_l, .xbase = 2},
        New C_octave_key With {.offset = 5, .name = "F", .white = True, .shape = shp_w_r, .xbase = 3},
        New C_octave_key With {.offset = 6, .name = "F#", .white = False, .shape = shp_blk, .xbase = 4},
        New C_octave_key With {.offset = 7, .name = "G", .white = True, .shape = shp_w_lr, .xbase = 4},
        New C_octave_key With {.offset = 8, .name = "G#", .white = False, .shape = shp_blk, .xbase = 5},
        New C_octave_key With {.offset = 9, .name = "A", .white = True, .shape = shp_w_lr, .xbase = 5},
        New C_octave_key With {.offset = 10, .name = "A#", .white = False, .shape = shp_blk, .xbase = 6},
        New C_octave_key With {.offset = 11, .name = "B", .white = True, .shape = shp_w_l, .xbase = 6}
        }

    'possible key-shapes
    Private Const shp_blk = 0           ' black key                                 5*
    Private Const shp_w_lr = 1          ' white key, black on left and right        3*
    Private Const shp_w_l = 2           ' white key, black on left                  2*
    Private Const shp_w_r = 3           ' white key, black on right                 2*

    ''' <summary>
    ''' Note-number to Note Name (f.e. 60 to 'C 4')
    ''' </summary>
    ''' <param name="NoteNr"></param>
    ''' <returns></returns>
    Private Function NoteNr_to_NoteName(noteNr As Integer) As String

        If noteNr > 127 Then
            Return ""                   ' return empty String if noteNr is invalid
        End If

        Return octave_keys(noteNr Mod 12).name & " " & (noteNr \ 12) - 1

        ' A4 = 440Hz = NoteNr: 69 (Def: Middle C = C4 = NoteNr: 60)
    End Function
    ''' <summary>
    ''' Note-number to Note Name (f.e. 60 to 'C 4')
    ''' </summary>    
    Public Function GetNoteNameFromNoteNumber(NoteNumber As Integer) As String
        Return NoteNr_to_NoteName(NoteNumber)
    End Function

    Public Function IsBlackKey(NoteNumber As Integer) As Boolean
        If NoteNumber > 127 Then Return False
        Return Not octave_keys(NoteNumber Mod 12).white
    End Function


    ''' <summary>
    ''' Converts a byte array to hex string. f.e. (A1, B2, C3, 54) to 'A1 B2 C3 54'
    ''' </summary>
    ''' <param name="src">Byte()</param>
    ''' <returns>Result as hex string. Empty string if source is Nothing or empty.</returns>
    Public Function Bytes_to_hex_str(ByRef src As Byte()) As String
        Dim str As String = ""

        Dim x As Byte() = {&HA1, &HB2, &HC3}

        If src Is Nothing Then Return str
        If src.Count = 0 Then Return str                    ' return empty string

        Dim arraysize As Integer = (src.Count * 3) - 1      ' each byte has (2 digits + 1 space) no space at the end
        Dim i As Integer                                    ' source index
        Dim d As Integer                                    ' destination index

        Dim array(arraysize - 1) As Byte                    ' upper bound !

        For i = 0 To src.Count - 1
            array(d) = CByte(Asc(Hex(src(i) >> 4)))         ' high nibble to first digit
            array(d + 1) = CByte(Asc(Hex(src(i) And &HF)))  ' low nibble to second digit
            If (d + 2) >= arraysize Then Exit For           ' early exit if at the end (avoid trailing space)
            array(d + 2) = &H20
            d += 3
        Next

        str = System.Text.Encoding.ASCII.GetString(array)

        ' or:
        'str = BitConverter.ToString(Buffer, BufferOffset, BytesPerRow)
        'str = str.Replace("-", " ")

        Return str
    End Function


    ''' <summary>
    ''' Returns the GM VoiceName from GM VoiceNumber
    ''' </summary>
    ''' <param name="VoiceNum"></param>
    ''' <returns></returns>
    Public Function GetVoiceName(VoiceNum As Byte) As String

        Dim str As String = ""

        If GM_VoiceNames.TryGetValue(VoiceNum, str) = True Then
            Return str
        Else

            Return VoiceNum & " - unknown Voice"
        End If

    End Function

    Public Function GetControllerName(CtrlNum As Byte) As String

        Dim str As String = ""

        If ControllerNames.TryGetValue(CtrlNum, str) = True Then
            Return str
        Else

            Return CtrlNum & " - unknown Controller"
        End If

    End Function

    ''' <summary>
    ''' GemeralMidi VoiceNames sorted by VoiceNumber
    ''' </summary>
    Public ReadOnly GM_VoiceNames As New SortedList(Of Integer, String) From
          {
{0, "Acoustic Grand Piano"},
{1, "Bright Acoustic Piano"},
{2, "Electric Grand Piano"},
{3, "Honky-tonk Piano"},
{4, "Electric Piano 1"},
{5, "Electric Piano 2"},
{6, "Harpsichord"},
{7, "Clavi"},
{8, "Celesta"},
{9, "Glockenspiel"},
{10, "Music Box"},
{11, "Vibraphone"},
{12, "Marimba"},
{13, "Xylophone"},
{14, "Tubular Bells"},
{15, "Dulcimer"},
{16, "Drawbar Organ"},
{17, "Percussive Organ"},
{18, "Rock Organ"},
{19, "Church Organ"},
{20, "Reed Organ"},
{21, "Accordion"},
{22, "Harmonica"},
{23, "Tango Accordion"},
{24, "Acoustic Guitar (nylon)"},
{25, "Acoustic Guitar (steel)"},
{26, "Electric Guitar (jazz)"},
{27, "Electric Guitar (clean)"},
{28, "Electric Guitar (muted"},
{29, "Overdriven Guitar"},
{30, "Distortion Guitar"},
{31, "Guitar harmonics"},
{32, "Acoustic Bass"},
{33, "Electric Bass (finger)"},
{34, "Electric Bass (pick)"},
{35, "Fretless Bass"},
{36, "Slap Bass 1"},
{37, "Slap Bass 2"},
{38, "Synth Bass 1"},
{39, "Synth Bass 2"},
{40, "Violin"},
{41, "Viola"},
{42, "Cello"},
{43, "Contrabass"},
{44, "Tremolo Strings"},
{45, "Pizzicato Strings"},
{46, "Orchestral Harp"},
{47, "Timpani"},
{48, "String Ensemble 1"},
{49, "String Ensemble 2"},
{50, "SynthStrings 1"},
{51, "SynthStrings 2"},
{52, "Choir Aahs"},
{53, "Voice Oohs"},
{54, "Synth Voice"},
{55, "Orchestra Hit"},
{56, "Trumpet"},
{57, "Trombone"},
{58, "Tuba"},
{59, "Muted Trumpet"},
{60, "French Horn"},
{61, "Brass Section"},
{62, "SynthBrass 1"},
{63, "SynthBrass 2"},
{64, "Soprano Sax"},
{65, "Alto Sax"},
{66, "Tenor Sax"},
{67, "Baritone Sax"},
{68, "Oboe"},
{69, "English Horn"},
{70, "Bassoon"},
{71, "Clarinet"},
{72, "Piccolo"},
{73, "Flute"},
{74, "Recorder"},
{75, "Pan Flute"},
{76, "Blown Bottle"},
{77, "Shakuhachi"},
{78, "Whistle"},
{79, "Ocarina"},
{80, "Lead 1 (square)"},
{81, "Lead 2 (sawtooth)"},
{82, "Lead 3 (calliope)"},
{83, "Lead 4 (chiff)"},
{84, "Lead 5 (charang)"},
{85, "Lead 6 (voice)"},
{86, "Lead 7 (fifths)"},
{87, "Lead 8 (bass + lead)"},
{88, "Pad 1 (New age)"},
{89, "Pad 2 (warm)"},
{90, "Pad 3 (polysynth)"},
{91, "Pad 4 (choir)"},
{92, "Pad 5 (bowed)"},
{93, "Pad 6 (metallic)"},
{94, "Pad 7 (halo)"},
{95, "Pad 8 (sweep)"},
{96, "FX 1 (rain)"},
{97, "FX 2 (soundtrack)"},
{98, "FX 3 (crystal)"},
{99, "FX 4 (atmosphere)"},
{100, "FX 5 (brightness)"},
{101, "FX 6 (goblins)"},
{102, "FX 7 (echoes)"},
{103, "FX 8 (sci-fi)"},
{104, "Sitar"},
{105, "Banjo"},
{106, "Shamisen"},
{107, "Koto"},
{108, "Kalimba"},
{109, "Bag pipe"},
{110, "Fiddle"},
{111, "Shanai"},
{112, "Tinkle Bell"},
{113, "Agogo"},
{114, "Steel Drums"},
{115, "Woodblock"},
{116, "Taiko Drum"},
{117, "Melodic Tom"},
{118, "Synth Drum"},
{119, "Reverse Cymbal"},
{120, "Guitar Fret Noise"},
{121, "Breath Noise"},
{122, "Seashore"},
{123, "Bird Tweet"},
{124, "Telephone Ring"},
{125, "Helicopter"},
{126, "Applause"},
{127, "Gunshot"}
}

    ''' <summary>
    ''' Midi ControllerNames sorted by ContollerNumber
    ''' </summary>
    Public ReadOnly ControllerNames As New SortedList(Of Integer, String) From
        {
{0, "Bank Select MSB"},
{1, "Modulation MSB"},           ' wheel or lever
{2, "Breath Controller MSB"},
{3, "Undefined"},
{4, "Foot Controller  MSB"},
{5, "Portamento Time  MSB"},
{6, "Data entry MSB"},
{7, "Channel Volume  MSB"},
{8, "Balance  MSB"},
{9, "Undefined"},
{10, "Pan  MSB"},
{11, "Expression Controller  MSB"},
{12, "Effect Control 1  MSB"},
{13, "Effect Control 2  MSB"},
{14, "Undefined"},
{15, "Undefined"},
{16, "General Purpose Controller 1  MSB"},
{17, "General Purpose Controller 2  MSB"},
{18, "General Purpose Controller 3  MSB"},
{19, "General Purpose Controller 4  MSB"},
{20, "Undefined"},
{21, "Undefined"},
{22, "Undefined"},
{23, "Undefined"},
{24, "Undefined"},
{25, "Undefined"},
{26, "Undefined"},
{27, "Undefined"},
{28, "Undefined"},
{29, "Undefined"},
{30, "Undefined"},
{31, "Undefined"},
{32, "Bank Select LSB"},
{33, "Modulation LSB"},      ' wheel or lever
{34, "Breath Controller LSB"},
{35, "Undefined"},
{36, "Foot Controller LSB"},
{37, "Portamento Time  LSB"},
{38, "Data entry LSB"},
{39, "Channel Volume  LSB"},
{40, "Balance  LSB"},
{41, "Undefined"},
{42, "Pan  LSB"},
{43, "Expression Controller  LSB"},
{44, "Effect Control 1  LSB"},
{45, "Effect Control 2  LSB"},
{46, "Undefined"},
{47, "Undefined"},
{48, "General Purpose Controller 1  LSB"},
{49, "General Purpose Controller 2  LSB"},
{50, "General Purpose Controller 3  LSB"},
{51, "General Purpose Controller 4  LSB"},
{52, "Undefined"},
{53, "Undefined"},
{54, "Undefined"},
{55, "Undefined"},
{56, "Undefined"},
{57, "Undefined"},
{58, "Undefined"},
{59, "Undefined"},
{60, "Undefined"},
{61, "Undefined"},
{62, "Undefined"},
{63, "Undefined"},
{64, "Damper Pedal (sustain)"},
{65, "Portamento On/Off"},
{66, "Sostenuto"},
{67, "Soft pedal"},
{68, "Legato Footswitch"},
{69, "Hold 2"},
{70, "Sound Controller 1"},
{71, "Sound Controller 2"},
{72, "Sound Controller 3"},
{73, "Sound Controller 4"},
{74, "Sound Controller 5"},
{75, "Sound Controller 6"},
{76, "Sound Controller 7"},
{77, "Sound Controller 8"},
{78, "Sound Controller 9"},
{79, "Sound Controller 10"},
{80, "General Purpose Controller 5"},
{81, "General Purpose Controller 6"},
{82, "General Purpose Controller 7"},
{83, "General Purpose Controller 8"},
{84, "Portamento Control"},
{85, "Undefined"},
{86, "Undefined"},
{87, "Undefined"},
{88, "Undefined"},
{89, "Undefined"},
{90, "Undefined"},
{91, "Effects 1 Depth"},
{92, "Effects 2 Depth"},
{93, "Effects 3 Depth"},
{94, "Effects 4 Depth"},
{95, "Effects 5 Depth"},
{96, "Data increment"},
{97, "Data decrement"},
{98, "Non-Registered Parameter Number LSB"},
{99, "Non-Registered Parameter Number MSB"},
{100, "Registered Parameter Number LSB"},
{101, "Registered Parameter Number MSB"},
{102, "Undefined"},
{103, "Undefined"},
{104, "Undefined"},
{105, "Undefined"},
{106, "Undefined"},
{107, "Undefined"},
{108, "Undefined"},
{109, "Undefined"},
{110, "Undefined"},
{111, "Undefined"},
{112, "Undefined"},
{113, "Undefined"},
{114, "Undefined"},
{115, "Undefined"},
{116, "Undefined"},
{117, "Undefined"},
{118, "Undefined"},
{119, "Undefined"},
{120, "All Sounds Off"},
{121, "Controller Reset"},
{122, "Local Control On/Off"},
{123, "All Notes Off"},
{124, "Omni Off"},
{125, "Omni On"},
{126, "mono on / poly off"},
{127, "poly on / mono off"}
    }

End Module
