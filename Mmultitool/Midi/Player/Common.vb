Partial Public Module Player

#Region "Play Manually"

    Public Sub Play_Manually(tev As TrackEventX)

        If tev.Type = EventType.MidiEvent Then
            RaiseEvent MidiOutShortMsg(tev.Status, tev.Data1, tev.Data2)
        Else
            If tev.DataX.Count > 0 Then
                Dim sysex(tev.DataX.Count) As Byte
                sysex(0) = tev.Status
                tev.DataX.CopyTo(sysex, 1)
                RaiseEvent MidiOutLongMsg(sysex)
            End If
        End If

    End Sub


#End Region

    ''' <summary>
    ''' Note-Durations in SequencePlayer related to fixed TPQ of 480
    ''' </summary>
    Public Enum NoteDuration
        Whole = 1920
        Half = 960
        Quarter = 480
        Eight = 240
        Sixteenth = 120
        Thirty_Second = 60
        Tri_Eight = 160
        Tri_Sixteenth = 80
        Tri_Thirty_Second = 40
    End Enum


End Module
