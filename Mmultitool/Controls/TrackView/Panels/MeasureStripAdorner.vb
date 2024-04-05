Partial Public Class MeasureStrip

    Public MeasureStripAdornerLayer As AdornerLayer
    Public PlayPositionAdorner1 As PlayPositionAdorner

    Private Sub InitializePlayPositionAdorner()

        MeasureStripAdornerLayer = AdornerLayer.GetAdornerLayer(Me)
        'MeasureStripAdornerLayer = AdornerLayer.GetAdornerLayer(TrackView.MeasureStrip1)
        If MeasureStripAdornerLayer IsNot Nothing Then
            If PlayPositionAdorner1 Is Nothing Then         ' avoid multiple add's (when returning from other tab)
                PlayPositionAdorner1 = New PlayPositionAdorner(Me)
                'PlayPositionAdorner1 = New PlayPositionAdorner(TrackView.MeasureStrip1)
                MeasureStripAdornerLayer.Add(PlayPositionAdorner1)
                PlayPositionAdorner1.TrackView = TrackView

            Else
                If PlayPositionAdorner1.Parent Is Nothing Then
                    MeasureStripAdornerLayer.Remove(PlayPositionAdorner1)
                    MeasureStripAdornerLayer.Add(PlayPositionAdorner1)
                End If

            End If
        End If

    End Sub

End Class

Public Class PlayPositionAdorner
    Inherits Adorner

    Public Sub New(adornedElement As UIElement)
        MyBase.New(adornedElement)
        IsHitTestVisible = False            ' important to prevent flicker!        
        'IsClipEnabled = True               ' not recommended -> using soft clip
        RenderOptions.SetEdgeMode(Me, EdgeMode.Aliased)            ' sharp edges
    End Sub

    Public TrackView As TrackView

    Protected Overrides Sub OnRender(dc As DrawingContext)
        MyBase.OnRender(dc)

        Dim adornedElementRect As New Rect(AdornedElement.RenderSize)

        Dim renderBrush As New SolidColorBrush(Colors.Green)
        renderBrush.Opacity = 0.2

        Dim pen As New Pen(Brushes.Red, 2)

        Dim rect As New Rect
        rect.Location = New Point(0, 0)
        rect.Size = AdornedElement.RenderSize

        'dc.DrawRectangle(renderBrush, Nothing, rect)

        If TrackView Is Nothing Then Exit Sub                               ' in design time
        Dim ScaleX As Double = Math.Round(TrackView.sldScaleX.Value, 1)
        Dim tpq As Integer = TrackView.TPQ
        Dim scb = TrackView.MasterHScroll

        'Dim StartTick As Integer = scb.Value / ScaleX * TrackView.PixelToTicksFactor

        Dim StartTick As Integer = scb.Value / ScaleX * TrackView.PixelToTicksFactor


        Dim posx As Integer = (TrackPlayerTime - StartTick) * ScaleX * TrackView.TicksToPixelFactor

        If posx > -1 AndAlso posx <= TrackView.MeasureStrip1.ActualWidth Then        ' soft clip
            dc.DrawLine(pen, New Point(posx, 2), New Point(posx, 14))               ' position mark height (12)
        End If

    End Sub


End Class