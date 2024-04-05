Public Class VoicePanel

    Friend TrackPanel As TrackPanel                     ' Parent, set in TrackPanel _Loaded

    Private Sub btnTap_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles btnTap.PreviewMouseLeftButtonDown
        Dim trev As New TrackEventX

        trev.Type = EventType.MidiEvent
        trev.Status = &H90 Or nudMidiChannel.Value
        trev.Data1 = &H40
        trev.Data2 = 100

        Play_Manually(trev)
    End Sub

    Private Sub btnTap_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles btnTap.PreviewMouseLeftButtonUp
        Dim trev As New TrackEventX

        trev.Type = EventType.MidiEvent
        trev.Status = &H90 Or nudMidiChannel.Value
        trev.Data1 = &H40
        trev.Data2 = 0

        Play_Manually(trev)
    End Sub

    Private Sub nudGmVoice_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudGmVoice.ValueChanged
        Dim trev As New TrackEventX

        trev.Type = EventType.MidiEvent
        trev.Status = &HC0 Or nudMidiChannel.Value
        trev.Data1 = nudGmVoice.Value
        trev.Data2 = 0

        Play_Manually(trev)
    End Sub
End Class
