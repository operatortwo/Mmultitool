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

    Protected Overrides Sub OnRender(dc As DrawingContext)
        MyBase.OnRender(dc)

        If TrackView Is Nothing Then Exit Sub
        If TrackView.MeasureStrip Is Nothing Then Exit Sub


        Dim ScaleX As Double = TrackView.sldScaleX.Value
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

        'dc.DrawLine(Pen1, New Point(psx, 30), New Point(psx + len, 30))         ' sample quarter line

        '--- draw Measure ---

        Dim StartTick As Integer = scb.Value / ScaleX * PixelToTicksFactor
        Dim TickOffset As Integer = RoundToStep(StartTick, tpq) - StartTick
        Dim CurrentTick As Integer = RoundToStep(StartTick, tpq)
        Dim TickStep As Integer = tpq
        Dim NumberOfMarks As Integer = (scb.ViewportSize / ScaleX * PixelToTicksFactor / tpq) + 1
        Dim posx As Integer
        Dim ys As Integer = 1

        Dim BeatPen As New Pen(Brushes.MediumBlue, 2.0)
        Dim BarPen As New Pen(Brushes.MediumBlue, 2.0)
        Dim BeatMarkHeight = 10
        Dim BarMarkHeight = 20

        For i = 0 To NumberOfMarks
            posx = (CurrentTick - StartTick) * TicksToPixelFactor * ScaleX

            If IsMeasure(CurrentTick) Then
                dc.DrawLine(BarPen, New Point(posx, ys), New Point(posx, ys + BarMarkHeight))

                str = CurrentTick / tpq
                glyphrun = CreateGlyphRun(str, GlyphTypeface, 12, New Point(posx + 5, ys + 25))
                dc.DrawGlyphRun(Brushes.MediumBlue, glyphrun)
            Else
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


    Private Function CreateGlyphRun(text As String, glyphTypeface As GlyphTypeface, emSize As Double, baselineOrigin As Point) As GlyphRun
        If text Is Nothing Then Return Nothing
        If text.Length = 0 Then Return Nothing
        If glyphTypeface Is Nothing Then Return Nothing

        Dim glyphIndices As UShort() = New UShort(text.Length - 1) {}
        Dim advanceWidths As Double() = New Double(text.Length - 1) {}

        For i = 0 To text.Length - 1
            Dim glyphIndex As UShort
            glyphTypeface.CharacterToGlyphMap.TryGetValue(AscW(text(i)), glyphIndex)
            glyphIndices(i) = glyphIndex
            advanceWidths(i) = glyphTypeface.AdvanceWidths(glyphIndex) * emSize
        Next

        Return New GlyphRun(glyphTypeface, 0, False, emSize, glyphIndices, baselineOrigin,
            advanceWidths, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
    End Function

End Class
