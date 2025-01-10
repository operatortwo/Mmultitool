
Public Class NotePanel

    Friend TrackPanel As TrackPanel                     ' Parent, set in TrackPanel _Loaded

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        NoteCanvas.TrackPanel = TrackPanel
        NoteCanvas.NotePanel = Me
    End Sub

    Public Sub UpdateView()

        NoteCanvas.Height = TrackPanel.KeyPanel.KeyCanvas.Height

        DebugDraw()
    End Sub


    Private Sub DebugDraw()
        'NotePanel.Children.Clear()


        'InsertText(NotePanel, "Hello", 10, 100)
    End Sub


    Private Sub InsertText(panel As Canvas, text As String, left As Double, top As Double)
        Dim tb As New TextBlock
        tb.Text = text
        Canvas.SetLeft(tb, left)
        Canvas.SetTop(tb, top)
        tb.IsHitTestVisible = False
        panel.Children.Add(tb)
    End Sub

    Private Sub NotePanelScroll_ScrollChanged(sender As Object, e As ScrollChangedEventArgs) Handles NotePanelScroll.ScrollChanged
        If e.VerticalChange <> 0 Then
            TrackPanel.KeyPanel.KeyPanelVScroll.ScrollToVerticalOffset(NotePanelScroll.VerticalOffset)
        End If
    End Sub


End Class

