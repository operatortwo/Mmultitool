Imports Mmultitool.TrackView

Public Class MeasureStrip

    Friend TrackView As TrackView

    Private Typeface As Typeface
    Private GlyphTypeface As GlyphTypeface

    Public Sub New()
        InitializeComponent()                           ' required for the designer

        Typeface = New Typeface(FontFamily, FontStyle, FontWeight, FontStretch)
        Typeface.TryGetGlyphTypeface(GlyphTypeface)
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        RenderOptions.SetEdgeMode(Me, EdgeMode.Aliased)            ' sharp edges
        InitializePlayPositionAdorner()
    End Sub

    Private Const BeatMarkHeight = 5
    Private Const BarMarkHeight = 10
    Private Const GlyphOffsetY = 24         ' 25

    Protected Overrides Sub OnRender(dc As DrawingContext)
        MyBase.OnRender(dc)

        If TrackView Is Nothing Then Exit Sub
        If TrackView.MeasureStrip1 Is Nothing Then Exit Sub

        Dim ScaleX As Double = Math.Round(TrackView.sldScaleX.Value, 1)
        Dim tpq As Integer = TrackView.TPQ

        Dim str As String
        Dim glyphrun As GlyphRun
        Dim Pen1 As New Pen(Brushes.Blue, 2.0)              ' need at least thickness 2.0 for full color
        Dim Pen2 As New Pen(Brushes.Red, 2.0)

        Dim scb = TrackView.MasterHScroll

        Dim psx As Integer
        Dim len As Integer
        psx = tpq * TicksToPixelFactor * ScaleX         ' at 2. beat
        len = tpq * TicksToPixelFactor * ScaleX          ' 1 beat long

        '--- draw Measure ---

        Dim StartTick As Integer = scb.Value / ScaleX * PixelToTicksFactor
        Dim TickOffset As Integer = RoundToStep(StartTick, tpq) - StartTick
        Dim CurrentTick As Integer = RoundToStep(StartTick, tpq)
        Dim TickStep As Integer = tpq
        Dim NumberOfMarks As Integer = (scb.ViewportSize / ScaleX * PixelToTicksFactor / tpq) + 1
        Dim posx As Integer
        Dim ys As Integer = 1

        Dim BeatPen As New Pen(Brushes.MediumBlue, 1.0)
        Dim BarPen As New Pen(Brushes.MediumBlue, 2.0)

        Dim tpq4 As Integer = tpq * 4

        For i = 0 To NumberOfMarks

            posx = (CurrentTick - StartTick) * TicksToPixelFactor * ScaleX

            If IsMeasure(CurrentTick) Then
                'If i Mod 4 = 0 Then

                dc.DrawLine(BarPen, New Point(posx, ys), New Point(posx, ys + BarMarkHeight))

                If ScaleX <= 0.2 AndAlso ((i Mod 8) = 0) Then
                Else
                    str = CurrentTick \ tpq4
                    glyphrun = CreateGlyphRun(str, GlyphTypeface, 12, New Point(posx + 2, ys + GlyphOffsetY))
                    dc.DrawGlyphRun(Brushes.MediumBlue, glyphrun)
                End If

            Else
                ' is not measure
                dc.DrawLine(BeatPen, New Point(posx, ys), New Point(posx, ys + BeatMarkHeight))
            End If

            CurrentTick += TickStep
        Next

    End Sub


    Private Function RoundToStep(value As Double, stepvalue As Integer) As Double
        If stepvalue = 0 Then stepvalue = 1
        Dim steps As Double = Fix(value / stepvalue)

        Dim smod As Double = Math.Abs(value Mod stepvalue)  ' remainder
        If smod >= stepvalue / 2 Then       ' round up if necessary
            steps += 1
        End If

        Return steps * stepvalue
    End Function

    Private Function IsMeasure(time As UInteger) As Boolean
        If time Mod 4 * TPQ = 0 Then
            Return True
        Else
            Return False
        End If
    End Function


    Private Sub UserControl_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        Dim pt As Point = e.GetPosition(Me)
        Dim ScaleX As Double = 1.0
        ScaleX = Math.Round(TrackView.sldScaleX.Value, 1)

        Dim scb = TrackView.MasterHScroll
        Dim TickAtPtX As Integer = (scb.Value + pt.X) / ScaleX * TrackView.PixelToTicksFactor
        Dim posx As String = TimeTo_MBT(TickAtPtX, TrackView.TPQ)

        Player.Set_TrackPlayerTime(TickAtPtX)
    End Sub

End Class
