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

        Dim linepen As New Pen(Brushes.Gray, 1.0)
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
        Dim kitem As KeyItem = KeyPanel.GetKeyItem(CInt(pt.Y / ScaleY))
        If kitem IsNot Nothing Then
            If KeyPanel.IsDrumView = False Then
                TrackPanel.TrackView.lbl_MousePosition_Key.Content = kitem.NoteName
            Else
                TrackPanel.TrackView.lbl_MousePosition_Key.Content = kitem.DrumName
            End If
        Else
            TrackPanel.TrackView.lbl_MousePosition_Key.Content = "Nothing"
        End If

        '---

        If kitem IsNot Nothing Then
            If e.LeftButton = MouseButtonState.Pressed Then

                If kitem.NoteNumber <> NoteNumberPlaying Then
                    PlayTrev.Status = &H90 Or TrackPanel.VoicePanel.nudMidiChannel.Value
                    PlayTrev.Data1 = NoteNumberPlaying
                    PlayTrev.Data2 = 0
                    Play_Manually(PlayTrev)                         ' previous note off
                    '--- new note on
                    PlayTrev.Data1 = kitem.NoteNumber
                    PlayTrev.Data2 = 100
                    Play_Manually(PlayTrev)
                    NoteNumberPlaying = kitem.NoteNumber
                End If
            End If
        End If

    End Sub


    Private NoteNumberPlaying As Byte = 128
    Private PlayTrev As New TrackEventX With {.Type = EventType.MidiEvent}

    Private Sub UserControl_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        Dim pt As Point = e.GetPosition(Me)
        Dim kitem As KeyItem = KeyPanel.GetKeyItem(CInt(pt.Y / ScaleY))
        If kitem IsNot Nothing Then
            PlayTrev.Status = &H90 Or TrackPanel.VoicePanel.nudMidiChannel.Value
            PlayTrev.Data1 = kitem.NoteNumber
            PlayTrev.Data2 = 100
            Play_Manually(PlayTrev)
            NoteNumberPlaying = kitem.NoteNumber
        End If

    End Sub

    Private Sub UserControl_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        If NoteNumberPlaying < 128 Then
            PlayTrev.Status = &H90 Or TrackPanel.VoicePanel.nudMidiChannel.Value
            PlayTrev.Data1 = NoteNumberPlaying
            PlayTrev.Data2 = 0
            Play_Manually(PlayTrev)
            NoteNumberPlaying = 128
        End If
    End Sub

    Private Sub UserControl_MouseLeave(sender As Object, e As MouseEventArgs)
        TrackPanel.TrackView.lbl_MousePosition_Key.Content = "KC Leave"

        If NoteNumberPlaying < 128 Then
            PlayTrev.Status = &H90 Or TrackPanel.VoicePanel.nudMidiChannel.Value
            PlayTrev.Data1 = NoteNumberPlaying
            PlayTrev.Data2 = 0
            Play_Manually(PlayTrev)
            NoteNumberPlaying = 128
        End If
    End Sub


End Class
