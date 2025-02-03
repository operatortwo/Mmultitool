Public Class TrackView

    Public Property TrackList As New Tracklist

    'Public Const TicksToPixelFactor = 0.03125       ' ( 1 / 32 )
    'Public Const PixelToTicksFactor = 32            ' ( 1 / TicksToPixelFactor )

    Friend Const TicksToPixelFactor = 0.0625        ' ( 1 / 16 )
    Friend Const PixelToTicksFactor = 16            ' ( 1 / TicksToPixelFactor )

    Friend Const TicksPerBeat = 480               ' = TPQ 480
    Friend Const TPQ = 480


    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        MeasureStrip.TrackView = Me
    End Sub

    ''' <summary>
    ''' Assign a Tracklist to the TrackView and convert the Event times to TPQ 480 based if necessary.
    ''' </summary>
    ''' <param name="trklist"></param>
    Public Sub SetTracklist(trklist As Tracklist)
        TrackList = trklist
        TrackPanelStack.Children.Clear()
        lblNumberOfBeats.Content = ""
        If TrackList Is Nothing Then Exit Sub

        If trklist.TPQ <> TPQ Then
            For Each trk In trklist.Tracks
                For Each trev In trk.EventList
                    'Newtime = Time * DestinationTPQ / SourceTPQ
                    trev.Time = trev.Time * TPQ / trklist.TPQ
                Next
            Next
            trklist.MaxLength = trklist.MaxLength * TPQ / trklist.TPQ
            trklist.TPQ = TPQ
        End If

        Dim beatlen As Single = trklist.MaxLength / trklist.TPQ
        lblNumberOfBeats.Content = beatlen & "  " & TimeTo_MBT_0(trklist.MaxLength, TPQ)

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

        '--- convert Evets to 480 TPQ base ---



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

    Private Function TimeToPx(Time As Double) As Double
        Return Time * TicksToPixelFactor * sldScaleX.Value
    End Function

#Region "HScroll"
    Private Sub MasterHScroll_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles MasterHScroll.SizeChanged
        SetHScrollValues()
    End Sub

    Private Sub SetHScrollValues()
        If TrackList IsNot Nothing Then
            'TrackList.MaxLength
            Dim CanvasLength As Double
            Dim Maximum As Double
            Dim ActualWidth As Double

            CanvasLength = TrackList.MaxLength * TicksToPixelFactor * sldScaleX.Value
            ActualWidth = MasterHScroll.ActualWidth
            Maximum = CanvasLength - ActualWidth
            If Maximum < 0 Then Maximum = 0

            MasterHScroll.Maximum = CanvasLength - ActualWidth
            MasterHScroll.ViewportSize = ActualWidth

            MasterHScroll.SmallChange = Math.Round(0.02 * Maximum, 0)
            MasterHScroll.LargeChange = Math.Round(0.1 * Maximum, 0)

        End If
    End Sub

    Private Sub sldScaleX_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldScaleX.ValueChanged
        If MasterHScroll IsNot Nothing Then SetHScrollValues()
        MeasureStrip.InvalidateVisual()
    End Sub

    Private Sub MasterHScroll_Scroll(sender As Object, e As Primitives.ScrollEventArgs) Handles MasterHScroll.Scroll
        MeasureStrip.InvalidateVisual()
    End Sub

#End Region
End Class
