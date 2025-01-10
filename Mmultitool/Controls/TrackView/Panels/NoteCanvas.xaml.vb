Public Class NoteCanvas
    Friend TrackPanel As TrackPanel                 ' Parent of Parent, set in NotePanel _Loaded
    Friend NotePanel As NotePanel                   ' Parent, set in NotePanel _Loaded

    Private Typeface As Typeface
    Private GlyphTypeface As GlyphTypeface


    Public Sub New()
        InitializeComponent()                       ' required for the designer

        Typeface = New Typeface(FontFamily, FontStyle, FontWeight, FontStretch)
        Typeface.TryGetGlyphTypeface(GlyphTypeface)
    End Sub

    Protected Overrides Sub OnRender(dc As DrawingContext)
        MyBase.OnRender(dc)

        ' Background
        dc.DrawRectangle(Brushes.LightGoldenrodYellow, Nothing, New Rect(0, 0, ActualWidth, ActualHeight))


        Dim glyphrun As GlyphRun = CreateGlyphRun("This is NoteCanvas", GlyphTypeface, 18, New Point(100, 40))
        dc.DrawGlyphRun(Brushes.Red, glyphrun)

    End Sub

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
