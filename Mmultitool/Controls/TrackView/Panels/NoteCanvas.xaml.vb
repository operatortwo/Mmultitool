Public Class NoteCanvas
    Friend TrackPanel As TrackPanel                 ' Parent of Parent, set in NotePanel _Loaded
    Friend NotePanel As NotePanel                   ' Parent, set in NotePanel _Loaded

    Friend Lookup As New List(Of TrackEventX)

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

        'Dim glyphrun As GlyphRun = CreateGlyphRun("This is NoteCanvas", GlyphTypeface, 18, New Point(100, 40))
        'dc.DrawGlyphRun(Brushes.Red, glyphrun)

        '---

        If TrackPanel Is Nothing Then Exit Sub
        If TrackPanel.TrackView Is Nothing Then Exit Sub

        TrackView = TrackPanel.TrackView
        ScaleX = Math.Round(TrackPanel.TrackView.sldScaleX.Value, 1)
        scb = TrackView.MasterHScroll
        FirstTick = scb.Value / ScaleX * TrackView.PixelToTicksFactor

        ScaleY = TrackPanel.TrackHeaderPanel.sldTrackScaleY.Value

        CreateLookup()
        DrawNotes(dc)

    End Sub


    Private ScaleX As Double = 1.0
    Private ScaleY As Double = 1.0

    Private TrackView As TrackView
    Private scb As Primitives.ScrollBar             ' MasterHScroll

    Private FirstTick As Integer

    'Private NoteBrush As New SolidColorBrush(Colors.LightGray)
    Private NoteBrush As New SolidColorBrush(Color.FromRgb(158, 194, 254))
    Private NoteBrushVelocity As New SolidColorBrush(Color.FromRgb(0, 122, 204))


    Private Sub DrawNotes(dc As DrawingContext)
        Dim nrect As New Rect                       ' Note Rect
        Dim vrect As New Rect                       ' Velocity Rect
        Dim kitem As KeyPanel.KeyItem


        nrect.X = 30
        nrect.Y = 20
        nrect.Width = 480 * TrackView.TicksToPixelFactor * ScaleX
        nrect.Height = 20


        For Each trev In Lookup

            nrect.X = (trev.Time - FirstTick) * TrackView.TicksToPixelFactor * ScaleX
            nrect.Width = trev.Duration * TrackView.TicksToPixelFactor * ScaleX

            kitem = TrackPanel.KeyPanel.GetKeyItem(trev.Data1)
            'nrect.Y = 20 * ScaleY
            nrect.Y = kitem.StartPosition * ScaleY

            'nrect.Height = 20 * ScaleY
            nrect.Height = kitem.Height * ScaleY

            dc.DrawRectangle(NoteBrush, Nothing, nrect)

            '--- velocity rect
            Dim velheight As Integer
            Dim veloffset As Integer

            vrect = nrect
            velheight = nrect.Height / 127 * trev.Data2
            veloffset = nrect.Height - velheight
            vrect.Height = velheight
            vrect.Y += veloffset

            dc.DrawRectangle(NoteBrushVelocity, Nothing, vrect)

        Next



    End Sub


    Private Sub CreateLookup()
        Lookup.Clear()


        Dim NumberOfTicks As Integer = scb.ViewportSize / ScaleX * TrackView.PixelToTicksFactor
        Dim LastTick As Integer = FirstTick + NumberOfTicks

        Dim evlist As List(Of TrackEventX) = TrackPanel.TrackData.EventList

        For Each ev In evlist
            If ev.TypeX = EventTypeX.NoteOnEvent Then

                If ev.Time >= FirstTick AndAlso ev.Time <= LastTick Then
                    Lookup.Add(ev)
                    ' notes that starts in aperture
                ElseIf ev.Time < FirstTick AndAlso ev.Time + ev.Duration > FirstTick Then
                    Lookup.Add(ev)
                    ' notes that starts before an overlap into the aperture
                End If

            End If
        Next

    End Sub

    Private Sub UserControl_MouseLeave(sender As Object, e As MouseEventArgs)
        TrackView.lbl_MousePosition.Content = "Leave"
    End Sub

    Private Sub UserControl_MouseMove(sender As Object, e As MouseEventArgs)
        Dim pt As Point = e.GetPosition(Me)
        TrackView.lbl_MousePosition.Content = Math.Round(pt.X, 2) & " " & Math.Round(pt.Y, 2)
    End Sub
End Class
