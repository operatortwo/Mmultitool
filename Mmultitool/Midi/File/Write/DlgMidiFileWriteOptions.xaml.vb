Public Class DlgMidiFileWriteOptions

    Public SrcNumberOfEvents As Integer
    Public SrcNumberOfTracks As Integer

    Public RetTPQ As Integer = 120
    Public RetFormat As Byte = 1


    Private TpqList() As Integer = {96, 120, 192, 240, 384, 480, 768, 960}

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        lblNumberOfEvents.Content = SrcNumberOfEvents.ToString("N0")
        lblNumberOfTracks.Content = SrcNumberOfTracks
        If SrcNumberOfTracks <= 1 Then
            lblTrackText.Content = "Track"
        End If

        cmbTPQ.ItemsSource = TpqList
        cmbTPQ.SelectedItem = 120
    End Sub

    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        RetTPQ = cmbTPQ.SelectedItem

        If rbFormat_0.IsChecked = True Then
            RetFormat = 0
        ElseIf rbFormat_1.IsChecked = True Then
            RetFormat = 1
        ElseIf rbFormat_1.IsChecked = True Then
            RetFormat = 2
        End If

        DialogResult = True
        Close()
    End Sub
End Class
