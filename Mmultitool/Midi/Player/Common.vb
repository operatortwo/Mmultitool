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



End Module
