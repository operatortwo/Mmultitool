Partial Public Module Player

    Private Tracklist1 As Tracklist

#Region "Play"
    Private Sub PlayTrackList()                         ' called from Timer TickCallback
        If Tracklist1 Is Nothing Then Exit Sub
        Dim CurrenTime As Long = TrackPlayerTime
        For Each trk In Tracklist1.Tracks
            PlayThisTrack(trk, CurrenTime)
        Next
    End Sub

    Private Sub PlayThisTrack(trk As Track, CurrentTime As Long)
        If trk Is Nothing Then Exit Sub
        If trk.EventListPtr >= trk.EventList.Count Then Exit Sub

        Dim tev As TrackEventX

        tev = trk.EventList(trk.EventListPtr)

        While tev.Time <= CurrentTime
            TrackPlayer.PlayEvent(CurrentTime, tev.Time, tev)
            trk.EventListPtr += 1
            If trk.EventListPtr >= trk.EventList.Count Then Exit While
            tev = trk.EventList(trk.EventListPtr)
        End While

    End Sub


#End Region

#Region "Set Tracklist"

    Public Sub PlayTracklist(tracklist As Tracklist)
        Tracklist1 = tracklist

        If IsTrackPlayerRunning = True Then
            StopTrackPlayer()
        End If

        '--- reset ptr to start --
        If tracklist IsNot Nothing Then
            For Each trk In tracklist.Tracks
                trk.EventListPtr = 0
            Next
        End If

        Set_TrackPlayerTime(0)
        StartTrackPlayer()

    End Sub

#End Region

End Module
