Imports Mmultitool

Partial Public Class MainWindow

    Public WithEvents MIO As New Midi_IO.Midi_IO

    Public hMidiIn As UInteger              ' for Midi-In
    Public hMidiOut As UInteger            ' for Midi-Out   

    Friend MidiOut_Selected As String = ""

    Private Sub OpenMidiOutPort()
        Dim MidiOutName As String
        MidiOutName = SelectMidiOut()

        If MidiOutName <> "" Then
            If MIO.OpenMidiOutPort(MidiOutName, hMidiOut, 0) = True Then
                lblMidiOutPort.Content = MidiOutName
                MidiOut_Selected = MidiOutName
            Else
                lblMidiOutPort.Content = "[ no Midi Out ]"
            End If
        Else
            lblMidiOutPort.Content = "[ no Midi Out ]"
        End If
    End Sub

    Private Function SelectMidiOut() As String
        Dim MidiOutName As String = ""

        If MIO.MidiOutPorts.Count = 0 Then Return MidiOutName

        MidiOutName = My.Settings.PreferredMidiOut
        If MidiOutName <> "" Then
            If MIO.MidiOutPorts.Exists(Function(x) x.portName = MidiOutName) = True Then
                Return MidiOutName
            End If
        End If

        MidiOutName = My.Settings.LastMidiOut
        If MidiOutName <> "" Then
            If MIO.MidiOutPorts.Exists(Function(x) x.portName = MidiOutName) = True Then
                Return MidiOutName
            End If
        End If

        MidiOutName = My.Settings.LastAlternativeMidiOut
        If MidiOutName <> "" Then
            If MidiOutName = "first available" = False Then
                If MIO.MidiOutPorts.Exists(Function(x) x.portName = MidiOutName) = True Then
                    Return MidiOutName
                End If
            End If
        End If

        MidiOutName = MIO.MidiOutPorts(0).portName

        Return MidiOutName
    End Function

    Friend Sub UpdateMidiOut(MidiOutName As String)

        If hMidiOut <> 0 Then
            MIO.CloseMidiOutPort(hMidiOut)
            hMidiOut = 0
        End If

        If MidiOutName <> "" Then
            If MIO.OpenMidiOutPort(MidiOutName, hMidiOut, 0) = True Then
                lblMidiOutPort.Content = MidiOutName
                MidiOut_Selected = MidiOutName
            Else
                lblMidiOutPort.Content = "[ no Midi Out ]"
            End If
        Else
            lblMidiOutPort.Content = "[ no Midi Out ]"
        End If

    End Sub

    Private DbgMidiOutList As New List(Of TrackEventX)

    Public Sub MidiOutShortMsg(status As Byte, data1 As Byte, data2 As Byte)
        If hMidiOut <> 0 Then
            MIO.OutShortMsg(hMidiOut, status, data1, data2)
            If DbgLogMidiOut = True Then
                If DbgMidiOutList.Count < 1000 Then
                    Dim trev As TrackEventX
                    trev = CreateTrackEventX(status, data1, data2)
                    trev.Time = PatternPlayerTime
                    DbgMidiOutList.Add(trev)
                End If
            End If
        End If
    End Sub

    Public Sub MidiOutLongMsg(SysExData As Byte())
        If hMidiOut <> 0 Then
            MIO.OutLongMsg(hMidiOut, SysExData)
        End If
    End Sub

    Private Sub MidiInData(hmi As UInteger, dwInstance As UInteger, status As Byte, data1 As Byte, data2 As Byte, dwTimestamp As UInteger) Handles MIO.MidiInData

    End Sub

    Private Sub MidiInLongdata(hmi As UInteger, dwInstance As UInteger, buffer As Byte(), dwTimestamp As UInteger) Handles MIO.MidiInLongdata

    End Sub

End Class
