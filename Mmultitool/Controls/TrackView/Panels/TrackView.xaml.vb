Public Class TrackView

    Public Property TrackList As New Tracklist


    Public Sub SetTracklist(trklist As Tracklist)
        TrackList = trklist
        TrackPanelStack.Children.Clear()
        If TrackList Is Nothing Then Exit Sub


        For Each track In TrackList.Tracks
            Dim trkp As New TrackPanel
            trkp.TrackView = Me
            trkp.TrackData = track
            trkp.Height = 250
            'trkp.IsExpanded = False            
            trkp.KeyPanel.SelectedView = KeyPanel.ViewType.RangeList
            'trkp.KeyPanel.SelectedView = KeyPanel.ViewType.FullRangeList
            TrackPanelStack.Children.Add(trkp)
        Next

        Dim trkp0 As TrackPanel = TrackPanelStack.Children(0)
        trkp0.IsExpanded = False

        '--- Track 1 for development ---
        'If TrackList.Tracks.Count > 1 Then
        '    Dim trkp As New TrackPanel
        '    trkp.TrackData = TrackList.Tracks(1)
        '    TrackPanelStack.Children.Add(trkp)
        'End If


        '--- example
        For Each panel As TrackPanel In TrackPanelStack.Children

        Next

    End Sub

    Public Sub UpdateVoiceColumnWidth(newwidth As Double)
        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.VoiceColumn.Width = New GridLength(newwidth)
        Next
    End Sub

    Public Sub UpdateTrackHeaderColumnWidth(newwidth As Double)
        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.TrackHeaderColumn.Width = New GridLength(newwidth)
        Next
    End Sub

    Public Sub UpdateKeysColumnColumnWidth(newwidth As Double)
        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.KeysColumn.Width = New GridLength(newwidth)
        Next
    End Sub


End Class
