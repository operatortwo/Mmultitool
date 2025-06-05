Public Class LoopStrip
    Public Sub New()
        InitializeComponent()                           ' required for the designer
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        RenderOptions.SetEdgeMode(Me, EdgeMode.Aliased)            ' sharp edges
        InitializeLoopAdorner()
    End Sub

    Protected Overrides Sub OnRender(dc As DrawingContext)
        Dim renderBrush As New SolidColorBrush(Colors.AliceBlue)

        Dim rect As New Rect
        rect.Location = New Point(0, 0)
        rect.Size = RenderSize

        dc.DrawRectangle(renderBrush, Nothing, rect)

    End Sub

    Private Sub UserControl_MouseMove(sender As Object, e As MouseEventArgs)
        LoopPositionAdorner1.InvalidateVisual()
    End Sub

    Private Sub UserControl_MouseLeave(sender As Object, e As MouseEventArgs)
        LoopPositionAdorner1.InvalidateVisual()
    End Sub

#Region "Adorner"


    Public LoopStripAdornerLayer As AdornerLayer
    Public LoopPositionAdorner1 As LoopPositionAdorner


    Private Sub InitializeLoopAdorner()

        LoopStripAdornerLayer = AdornerLayer.GetAdornerLayer(Me)
        'MeasureStripAdornerLayer = AdornerLayer.GetAdornerLayer(TrackView.MeasureStrip1)
        If LoopStripAdornerLayer IsNot Nothing Then
            If LoopPositionAdorner1 Is Nothing Then         ' avoid multiple add's (when returning from other tab)
                LoopPositionAdorner1 = New LoopPositionAdorner(Me)
                'PlayPositionAdorner1 = New PlayPositionAdorner(TrackView.MeasureStrip1)
                LoopStripAdornerLayer.Add(LoopPositionAdorner1)
                'LoopPositionAdorner1.TrackView = TrackView

            Else
                If LoopPositionAdorner1.Parent Is Nothing Then
                    LoopStripAdornerLayer.Remove(LoopPositionAdorner1)
                    LoopStripAdornerLayer.Add(LoopPositionAdorner1)
                End If

            End If
        End If

    End Sub

    Public Class LoopPositionAdorner
        Inherits Adorner

        Public Sub New(adornedElement As UIElement)
            MyBase.New(adornedElement)
            IsHitTestVisible = False            ' important to prevent flicker!        
            'IsClipEnabled = True               ' not recommended -> using soft clip
            RenderOptions.SetEdgeMode(Me, EdgeMode.Aliased)            ' sharp edges
        End Sub


        Protected Overrides Sub OnRender(dc As DrawingContext)
            MyBase.OnRender(dc)

            Dim adornedElementRect As New Rect(AdornedElement.RenderSize)

            '---- Draw current Loop Select Cursor

            Dim pen As New Pen(Brushes.Blue, 1)

            If AdornedElement.IsMouseOver Then
                Dim pt As Point
                pt = Mouse.GetPosition(AdornedElement)
                'pt.X = RoundToBeat(pt.X)
                'pt.X = RoundToBeat(pt.X, SeqPanel.TracksHeader.ScaleX)

                'drawingContext.DrawLine(pen, New Point(MousePosition.X, 0), New Point(MousePosition.X, adornedElementRect.Height))
                dc.DrawLine(pen, New Point(pt.X, 0), New Point(pt.X, adornedElementRect.Height))
            End If




        End Sub


    End Class







#End Region


End Class
