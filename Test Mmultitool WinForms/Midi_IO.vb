Partial Public Class Form1

    Public WithEvents MIO As New Midi_IO.Midi_IO

    Public hMidiIn As UInteger              ' for Midi-In
    Public hMidiOut As UInteger            ' for Midi-Out   

    Friend MidiOut_Selected As String = ""

    Private Sub OpenMidiOutPort()
        Dim MidiOutName As String
        MidiOutName = SelectMidiOut()

        If MidiOutName <> "" Then
            If MIO.OpenMidiOutPort(MidiOutName, hMidiOut, 0) = True Then
                lblMidiOutPort.Text = MidiOutName
                MidiOut_Selected = MidiOutName
            Else
                lblMidiOutPort.Text = "[ no Midi Out ]"
            End If
        Else
            lblMidiOutPort.Text = "[ no Midi Out ]"
        End If
    End Sub

    Private Function SelectMidiOut() As String
        Dim MidiOutName As String = ""

        MidiOutName = "MyMidiDevice"                        ' preferred
        'MidiOutName = My.Settings.PreferredMidiOut
        If MidiOutName <> "" Then
            If MIO.MidiOutPorts.Exists(Function(x) x.portName = MidiOutName) = True Then
                Return MidiOutName
            End If
        End If

        ' first existing
        If MIO.MidiOutPorts.Count > 0 Then
            MidiOutName = MIO.MidiOutPorts(0).portName
        End If

        Return MidiOutName
    End Function



    Public Sub MidiOutShortMsg(status As Byte, data1 As Byte, data2 As Byte)
        If hMidiOut <> 0 Then
            MIO.OutShortMsg(hMidiOut, status, data1, data2)
        End If
    End Sub

    Public Sub MidiOutLongMsg(SysExData As Byte())
        If hMidiOut <> 0 Then
            'MIO.OutShortMsg(hMidiOut0, status, data1, data2)
            MIO.OutLongMsg(hMidiOut, SysExData)
        End If
    End Sub

    Private Sub MidiInData(hmi As UInteger, dwInstance As UInteger, status As Byte, data1 As Byte, data2 As Byte, dwTimestamp As UInteger) Handles MIO.MidiInData

    End Sub

    Private Sub MidiInLongdata(hmi As UInteger, dwInstance As UInteger, buffer As Byte(), dwTimestamp As UInteger) Handles MIO.MidiInLongdata

    End Sub

End Class
