Imports Mmultitool.KeyPanel

Public Class KeyCanvas

    Friend TrackPanel As TrackPanel                 ' Parent of Parent, set in KeyPanel _Loaded
    Friend KeyPanel As KeyPanel                    ' Parent, set in KeyPanel _Loaded

    Private Typeface As Typeface
    Private GlyphTypeface As GlyphTypeface

    Public Sub New()
        InitializeComponent()                       ' required for the designer

        Typeface = New Typeface(FontFamily, FontStyle, FontWeight, FontStretch)
        Typeface.TryGetGlyphTypeface(GlyphTypeface)

    End Sub

    Protected Overrides Sub OnRender(dc As DrawingContext)
        MyBase.OnRender(dc)

        '--- Background ---
        dc.DrawRectangle(Brushes.LavenderBlush, Nothing, New Rect(0, 0, ActualWidth, ActualHeight))


        '--- Draw Keys ---
        If TrackPanel Is Nothing Then Exit Sub
        If KeyPanel Is Nothing Then Exit Sub

        ScaleY = TrackPanel.TrackHeaderPanel.sldTrackScaleY.Value

        If KeyPanel.SelectedView = ViewType.FullRangeList Then
            DrawKeys(KeyPanel.FullRangeList, dc)
        ElseIf KeyPanel.SelectedView = ViewType.RangeList Then
            DrawKeys(KeyPanel.RangeList, dc)
        ElseIf KeyPanel.SelectedView = ViewType.RandomList Then
            DrawKeys(KeyPanel.RandomList, dc)
        End If

    End Sub

    Private ScaleY As Double = 1.0

    Private Sub DrawKeys(List As List(Of KeyItem), dc As DrawingContext)

        Dim linepen As New Pen(Brushes.Gray, 0.5)
        Dim startpoint As New Point(5, 20)
        Dim endpoint As New Point(65, 20)
        Dim GlyphRun As GlyphRun
        Dim TextStart As New Point
        Dim rectangle As New Rect

        For Each item In List

            If KeyPanel.IsDrumView = False Then
                If item.IsBlackKey = True Then

                    '       InsertRectangle(KeyPanel, 1, item.StartPosition * ScaleY, 50, item.Height * ScaleY)
                    ' left top width height

                    rectangle.X = 1
                    rectangle.Y = item.StartPosition * ScaleY
                    rectangle.Width = 50
                    rectangle.Height = item.Height * ScaleY
                    dc.DrawRectangle(Brushes.LightGray, Nothing, rectangle)
                End If
            End If

            If KeyPanel.IsDrumView = False Then
                ' left top
                '  InsertText(KeyPanel, item.NoteName, 5, item.StartPosition * ScaleY)
                TextStart.X = 5
                TextStart.Y = (item.StartPosition + 12) * ScaleY
                GlyphRun = CreateGlyphRun(item.NoteName, GlyphTypeface, 12, TextStart)
                dc.DrawGlyphRun(Brushes.Black, GlyphRun)
            Else
                ' InsertText(KeyPanel, item.DrumName, 5, item.StartPosition * ScaleY)
                TextStart.X = 5
                TextStart.Y = (item.StartPosition + 12) * ScaleY
                GlyphRun = CreateGlyphRun(item.DrumName, GlyphTypeface, 12, TextStart)
                dc.DrawGlyphRun(Brushes.Black, GlyphRun)
            End If

            'InsertHorizontalLine(KeyPanel, 5, KeyPanelMaxWidth, (item.StartPosition + item.Height) * ScaleY)

            startpoint.Y = (item.StartPosition + item.Height) * ScaleY
            endpoint.Y = startpoint.Y
            dc.DrawLine(linepen, startpoint, endpoint)

        Next


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

    Private Sub UserControl_MouseMove(sender As Object, e As MouseEventArgs)
        Dim pt As Point = e.GetPosition(Me)
        TrackPanel.TrackView.lbl_MousePosition_Note.Content = Math.Round(pt.X, 2) & " " & Math.Round(pt.Y, 2)
    End Sub

    Private Sub UserControl_MouseLeave(sender As Object, e As MouseEventArgs)
        TrackPanel.TrackView.lbl_MousePosition_Note.Content = "Leave"
    End Sub
End Class
