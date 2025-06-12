Imports System.Windows.Media.Media3D

Public Class LoopStrip

    Friend TrackView As TrackView
    Public Sub New()
        InitializeComponent()                           ' required for the designer
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        RenderOptions.SetEdgeMode(Me, EdgeMode.Aliased)     ' sharp edges
        InitializeLoopAdorner()
        Visibility = Visibility.Hidden          ' default LoopMode is Off, for desigmode it is Visible until here
    End Sub

    Private Sub UserControl_MouseMove(sender As Object, e As MouseEventArgs)
        LoopPositionAdorner1.InvalidateVisual()             ' Adorner new position
    End Sub

    Private Sub UserControl_MouseLeave(sender As Object, e As MouseEventArgs)
        LoopPositionAdorner1.InvalidateVisual()             ' Adorner no position (remove)
    End Sub

    Private Sub UserControl_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        Dim pt As Point = e.GetPosition(Me)

        Try                                         ' if one of the referenced objects is Nothing
            Dim scb = TrackView.MasterHScroll
            Dim ScaleX As Double = Math.Round(TrackView.sldScaleX.Value, 1)
            Dim FirstTick As Integer = scb.Value / ScaleX * TrackView.PixelToTicksFactor
            Dim TickAtPtX As Integer = (scb.Value + pt.X) / ScaleX * TrackView.PixelToTicksFactor
            Dim RoundedTickPosition As Integer = RoundToStep(TickAtPtX, TrackView.TPQ)
            pt.X = (RoundedTickPosition - FirstTick) * TrackView.TicksToPixelFactor * ScaleX

            TrackView.TrackList.LoopStart = RoundedTickPosition
            Me.InvalidateVisual()
        Catch
        End Try

    End Sub

    Private Sub UserControl_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        Dim pt As Point = e.GetPosition(Me)

        Try                                         ' if one of the referenced objects is Nothing
            Dim scb = TrackView.MasterHScroll
            Dim ScaleX As Double = Math.Round(TrackView.sldScaleX.Value, 1)
            Dim FirstTick As Integer = scb.Value / ScaleX * TrackView.PixelToTicksFactor
            Dim TickAtPtX As Integer = (scb.Value + pt.X) / ScaleX * TrackView.PixelToTicksFactor
            Dim RoundedTickPosition As Integer = RoundToStep(TickAtPtX, TrackView.TPQ)
            pt.X = (RoundedTickPosition - FirstTick) * TrackView.TicksToPixelFactor * ScaleX

            TrackView.TrackList.LoopEnd = RoundedTickPosition
            Me.InvalidateVisual()
        Catch
        End Try

    End Sub

    Private LoopMarkerBrush As SolidColorBrush = Brushes.DodgerBlue
    Private LoopMarkerPen As New Pen(LoopMarkerBrush, 3)

    Private LoopMarkerWidth As Integer = 14
    Private LoopMarkerHeight As Integer = 14

    Private DisabledBackground As SolidColorBrush = New SolidColorBrush(Color.FromArgb(&HFF, &HE1, &HE6, &HFF))

    Protected Overrides Sub OnRender(dc As DrawingContext)
        Dim renderBrush As New SolidColorBrush(Colors.AliceBlue)

        Dim rect As New Rect
        rect.Location = New Point(0, 0)
        rect.Size = RenderSize

        '--- Background ---
        If IsEnabled = True Then
            dc.DrawRectangle(renderBrush, Nothing, rect)
        Else
            dc.DrawRectangle(DisabledBackground, Nothing, rect)
            Exit Sub
        End If


        If TrackView Is Nothing Then Exit Sub
        If TrackView.TrackList Is Nothing Then Exit Sub
        Dim trklist As Tracklist = TrackView.TrackList

        If trklist.LoopStart = trklist.LoopEnd Then Exit Sub
        If trklist.LoopStart > trklist.LoopEnd Then Exit Sub


        '--- Loop Start, End and Range ---
        Try                                   ' if one of the referenced objects is Nothing
            Dim pt As New Point(0, 0)
            Dim GreenPen As New Pen(Brushes.Green, 2)

            Dim StartPx As Integer              ' Pixel position of Loop Start (can be negative)
            Dim EndPx As Integer                ' Pixel position of Loop End (can be > LoopStrip.Width)

            Dim scb = TrackView.MasterHScroll
            Dim ScaleX As Double = Math.Round(TrackView.sldScaleX.Value, 1)
            Dim FirstTick As Integer = scb.Value / ScaleX * TrackView.PixelToTicksFactor

            StartPx = (trklist.LoopStart - FirstTick) * TrackView.TicksToPixelFactor * ScaleX
            EndPx = (trklist.LoopEnd - FirstTick) * TrackView.TicksToPixelFactor * ScaleX

            '--- Draw LoopRange line if inside viewport ---

            If StartPx < ActualWidth Then
                If EndPx >= 0 Then
                    Dim xs As Integer = StartPx
                    Dim xe As Integer = EndPx
                    If xs < 0 Then xs = 0
                    If xe > ActualWidth Then xe = ActualWidth
                    dc.DrawLine(LoopMarkerPen, New Point(xs, 2), New Point(xe, 2))
                End If
            End If

            '--- Draw StartLoop mark if not outside viewport ---
            If StartPx >= 0 Then
                If StartPx < Me.ActualWidth Then
                    Dim strg As New StreamGeometry
                    Using ctx As StreamGeometryContext = strg.Open()
                        ctx.BeginFigure(New Point(StartPx, 0), True, True) ' is closed  -  is filled 
                        ctx.LineTo(New Point(StartPx + LoopMarkerWidth, 0), True, True)
                        ctx.LineTo(New Point(StartPx, LoopMarkerHeight), True, True)
                    End Using
                    dc.DrawGeometry(LoopMarkerBrush, Nothing, strg)
                End If
            End If
            '--- Draw EndLoop mark if not outside viewport ---
            If EndPx >= 0 Then
                If EndPx < Me.ActualWidth Then
                    'dc.DrawLine(GreenPen, New Point(EndPx, 0), New Point(EndPx, Me.Height))

                    Dim strg As New StreamGeometry
                    Using ctx As StreamGeometryContext = strg.Open()
                        ctx.BeginFigure(New Point(EndPx, 0), True, True) ' is closed  -  is filled 
                        ctx.LineTo(New Point(EndPx - LoopMarkerWidth, 0), True, True)
                        ctx.LineTo(New Point(EndPx, LoopMarkerHeight), True, True)
                    End Using
                    dc.DrawGeometry(LoopMarkerBrush, Nothing, strg)
                End If
            End If

        Catch
        End Try

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

        Private LS1 As LoopStrip
        Private PositionPen As New Pen(Brushes.Blue, 2)

        Public Sub New(adornedElement As UIElement)
            MyBase.New(adornedElement)
            IsHitTestVisible = False            ' important to prevent flicker!        
            'IsClipEnabled = True               ' not recommended -> using soft clip
            RenderOptions.SetEdgeMode(Me, EdgeMode.Aliased)            ' sharp edges

            LS1 = TryCast(adornedElement, LoopStrip)
        End Sub

        Protected Overrides Sub OnRender(dc As DrawingContext)
            MyBase.OnRender(dc)

            '---- Draw current Loop Select Cursor
            If AdornedElement.IsMouseOver Then
                Try                                   ' if one of the referenced objects is Nothing
                    Dim pt As Point = Mouse.GetPosition(AdornedElement)
                    Dim adornedElementRect As New Rect(AdornedElement.RenderSize)
                    Dim scb = LS1.TrackView.MasterHScroll
                    Dim ScaleX As Double = Math.Round(LS1.TrackView.sldScaleX.Value, 1)
                    Dim FirstTick As Integer = scb.Value / ScaleX * TrackView.PixelToTicksFactor
                    Dim TickAtPtX As Integer = (scb.Value + pt.X) / ScaleX * TrackView.PixelToTicksFactor
                    Dim RoundedTickPosition As Integer = RoundToStep(TickAtPtX, TrackView.TPQ)
                    pt.X = (RoundedTickPosition - FirstTick) * TrackView.TicksToPixelFactor * ScaleX

                    dc.DrawLine(PositionPen, New Point(pt.X, 0), New Point(pt.X, adornedElementRect.Height))
                Catch
                End Try
            End If

        End Sub

    End Class

#End Region


    Public Shared Function TicksRoundToBeat(MousePosition As Double, ScaleX As Double) As Double
        'Dim TickPosition As Double = MousePosition * PixelToTicksFactor / SequencerPanel1.TracksHeader.ScaleX
        Dim TickPosition As Double = MousePosition * TrackView.PixelToTicksFactor / ScaleX
        Dim RoundedTickPosition As Double = RoundToStep(TickPosition, TrackView.TPQ)
        'Dim RoundedPosition As Double = RoundedTickPosition * TicksToPixelFactor * SequencerPanel1.TracksHeader.ScaleX
        Dim RoundedPosition As Double = RoundedTickPosition * TrackView.TicksToPixelFactor * ScaleX

        Return RoundedPosition
    End Function




End Class
