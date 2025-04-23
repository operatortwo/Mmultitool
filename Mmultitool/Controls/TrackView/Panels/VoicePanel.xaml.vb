Public Class VoicePanel

    Friend TrackPanel As TrackPanel                     ' Parent, set in TrackPanel _Loaded

    Private mtrev As New TrackEventX With {.Type = EventType.MidiEvent}         ' local trev for manual send

    Private Sub btnTap_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles btnTap.PreviewMouseLeftButtonDown
        mtrev.Status = &H90 Or nudMidiChannel.Value
        mtrev.Data1 = &H40                              ' note number
        mtrev.Data2 = 100                               ' velocity
        Play_Manually(mtrev)
    End Sub

    Private Sub btnTap_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles btnTap.PreviewMouseLeftButtonUp
        mtrev.Status = &H90 Or nudMidiChannel.Value
        mtrev.Data1 = &H40                              ' note number
        mtrev.Data2 = 0                                 ' 0 = note off
        Play_Manually(mtrev)
    End Sub

    Private Sub nudMidiChannel_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudMidiChannel.ValueChanged
        Show_VoiceName_or_PatchName()
    End Sub

    Private Sub nudGmVoice_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudGmVoice.ValueChanged
        mtrev.Status = &HC0 Or nudMidiChannel.Value     ' program change
        mtrev.Data1 = nudGmVoice.Value
        mtrev.Data2 = 0
        Play_Manually(mtrev)

        Show_VoiceName_or_PatchName()
    End Sub

    Private Sub Show_VoiceName_or_PatchName()
        If nudMidiChannel.Value <> 9 Then
            tbGmVoiceName.Text = Get_GM_VoiceName(nudGmVoice.Value)         ' it's a voice name
        Else
            Dim str As String = Get_GS_DrumPatchName(nudGmVoice.Value)      ' drum: maybe it's a known GS patch number
            If str = "" Then str = "Drum"
            tbGmVoiceName.Text = str
        End If
    End Sub

    Private Sub ssldVolume_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldVolume.ValueChanged
        mtrev.Status = &HB0 Or nudMidiChannel.Value     ' control change
        mtrev.Data1 = 7                                 ' Channel volume coarse
        mtrev.Data2 = e.NewValue
        Play_Manually(mtrev)
    End Sub

    Private Sub ssldPan_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldPan.ValueChanged
        mtrev.Status = &HB0 Or nudMidiChannel.Value     ' control change
        mtrev.Data1 = 10                                ' Panorama MSB
        mtrev.Data2 = e.NewValue
        Play_Manually(mtrev)
    End Sub

    Private Sub tgbtnMute_Checked(sender As Object, e As RoutedEventArgs) Handles tgbtnMute.Checked
        TrackPanel.TrackData.Mute = True
    End Sub

    Private Sub tgbtnMute_Unchecked(sender As Object, e As RoutedEventArgs) Handles tgbtnMute.Unchecked
        TrackPanel.TrackData.Mute = False
    End Sub

    Private Sub nudNTransp_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudNTransp.ValueChanged
        TrackPanel.TrackData.Transpose = nudNTransp.Value
    End Sub
End Class
