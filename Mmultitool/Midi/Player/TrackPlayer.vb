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

        Dim trev As TrackEventX
        Dim mute As Boolean = trk.Mute

        trev = trk.EventList(trk.EventListPtr)

        While trev.Time <= CurrentTime
            TrackPlayer.PlayEvent(CurrentTime, trev.Time, trev, mute)
            Set_Track_UI_Notifications(trev, trk)
            trk.EventListPtr += 1
            If trk.EventListPtr >= trk.EventList.Count Then Exit While
            trev = trk.EventList(trk.EventListPtr)
        End While

    End Sub


#End Region

#Region "Set Tracklist"

    Public Sub PlayTracklist(tracklist As Tracklist)
        SetTracklist(tracklist)
        StartTrackPlayer()
    End Sub

    ''' <summary>
    ''' Assign a Tracklist to the Player. To to visualize it's also necessary to call Player.SetTracklist.
    ''' The player itself also works without Trackview.
    ''' </summary>
    ''' <param name="tracklist"></param>
    Public Sub SetTracklist(tracklist As Tracklist)
        If IsTrackPlayerRunning = True Then
            StopTrackPlayer()
        End If

        Tracklist1 = tracklist

        '--- reset ptr to start --

        If tracklist IsNot Nothing Then
            For Each trk In tracklist.Tracks
                trk.EventListPtr = 0
            Next
        End If

        For Each trk In tracklist.Tracks
            Reset_UI_Notification(trk)
        Next

        Set_TrackPlayerTime(0)
    End Sub



#End Region

End Module
