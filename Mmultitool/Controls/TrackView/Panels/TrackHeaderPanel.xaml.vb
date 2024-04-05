Public Class TrackHeaderPanel
    Friend TrackScaleY As Double = 1.0

    Friend TrackPanel As TrackPanel                     ' Parent, set in TrackPanel _Loaded

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        'TrackPanel.TrackData
        If TrackPanel IsNot Nothing Then
            tblkViewType.Text = TrackPanel.KeyPanel.SelectedView.ToString()
            If TrackPanel.TrackData IsNot Nothing Then
                tblkTrackName.Text = GetTrackName(TrackPanel.TrackData.EventList)
            End If
        End If
    End Sub

    Private Sub sldTrackScaleY_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldTrackScaleY.ValueChanged
        TrackScaleY = e.NewValue
        If TrackPanel IsNot Nothing Then
            TrackPanel.KeyPanel.UpdateView()
        End If
    End Sub

    Private Sub btnSetView_Click(sender As Object, e As RoutedEventArgs) Handles btnSetView.Click
        Dim win As New DlgSetKeyList(TrackPanel)
        win.Owner = Application.Current.MainWindow
        win.ShowDialog()
    End Sub

    Private Sub btnSwitchMode_Click(sender As Object, e As RoutedEventArgs) Handles btnSwitchMode.Click
        Select Case TrackPanel.KeyPanel.SelectedView
            Case KeyPanel.ViewType.FullRangeList
                TrackPanel.KeyPanel.SelectedView = KeyPanel.ViewType.RangeList
                tblkViewType.Text = TrackPanel.KeyPanel.SelectedView.ToString()
                TrackPanel.KeyPanel.UpdateView()
            Case KeyPanel.ViewType.RangeList
                TrackPanel.KeyPanel.SelectedView = KeyPanel.ViewType.RandomList
                tblkViewType.Text = TrackPanel.KeyPanel.SelectedView.ToString()
                TrackPanel.KeyPanel.UpdateView()
            Case KeyPanel.ViewType.RandomList
                TrackPanel.KeyPanel.SelectedView = KeyPanel.ViewType.FullRangeList
                tblkViewType.Text = TrackPanel.KeyPanel.SelectedView.ToString()
                TrackPanel.KeyPanel.UpdateView()
        End Select
    End Sub

    Private Sub cbDrumView_Click(sender As Object, e As RoutedEventArgs) Handles cbDrumView.Click
        TrackPanel.KeyPanel.IsDrumView = cbDrumView.IsChecked
        TrackPanel.KeyPanel.UpdateView()
    End Sub


End Class
