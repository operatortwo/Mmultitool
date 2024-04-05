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

        If TrackPanel Is Nothing Then Exit Sub
        If TrackPanel.TrackView Is Nothing Then Exit Sub
        TrackView = TrackPanel.TrackView                                ' shortcut

        '---

        ScaleX = Math.Round(TrackPanel.TrackView.sldScaleX.Value, 1)
        ScaleY = Math.Round(TrackPanel.TrackHeaderPanel.sldTrackScaleY.Value, 1)

        scb = TrackView.MasterHScroll
        FirstTick = scb.Value / ScaleX * TrackView.PixelToTicksFactor

        NumberOfTicks = scb.ViewportSize / ScaleX * TrackView.PixelToTicksFactor
        LastTick = FirstTick + NumberOfTicks

        '---

        DrawGrid(dc)

        CreateLookup()
        DrawNotes(dc)

    End Sub


    Private ScaleX As Double = 1.0
    Private ScaleY As Double = 1.0

    Private TrackView As TrackView
    Private scb As Primitives.ScrollBar             ' MasterHScroll

    Private FirstTick As Integer
    Private NumberOfTicks As Integer
    Private LastTick As Integer

    Private NoteBrush As New SolidColorBrush(Color.FromRgb(158, 194, 254))
    Private NoteBrushVelocity As New SolidColorBrush(Color.FromRgb(0, 122, 204))

    Private Gridpen As New Pen(Brushes.Gray, 1.0)

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
            nrect.Y = kitem.StartPosition * ScaleY
            nrect.Height = kitem.Height * ScaleY

            'If ScaleX > 0.2 Then
            '    dc.DrawRectangle(NoteBrush, Nothing, nrect)
            'End If

            '--- velocity rect
            Dim velheight As Integer
            Dim veloffset As Integer

            vrect = nrect
            velheight = nrect.Height / 127 * trev.Data2
            veloffset = nrect.Height - velheight
            vrect.Height = velheight
            vrect.Y += veloffset

            '---
            If ScaleX > 0.2 Then
                dc.DrawRectangle(NoteBrush, Nothing, nrect)
                dc.DrawRectangle(NoteBrushVelocity, Nothing, vrect)
            Else
                ' improve perfomance when ScaleX is small
                dc.DrawRectangle(NoteBrushVelocity, Nothing, nrect)         ' fulle size with velocity brush
            End If

        Next

    End Sub

    Private Sub CreateLookup()
        Lookup.Clear()

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

    Private Sub DrawGrid(dc As DrawingContext)
        'First Tick
        'NumberOfTicks
        'LastTick

        Dim stepvalue As Integer = RoundToStep(4 * 480 / ScaleX, 480)
        'Dim stepvalue As Integer = RoundToStep(4 * 480 / ScaleX, 960)


        Dim pts As New Point
        Dim pte As New Point

        Dim first As Integer
        first = RoundToNextStep(FirstTick, stepvalue)

        pts.X = (first - FirstTick) * TrackView.TicksToPixelFactor * ScaleX
        pts.Y = 0
        pte.X = pts.X
        pte.Y = Me.ActualHeight

        Dim NumberOfSteps As Integer = 1

        If stepvalue > 0 Then
            NumberOfSteps = NumberOfTicks / stepvalue
        End If

        For i = 1 To NumberOfSteps

            dc.DrawLine(Gridpen, pts, pte)

            pts.X += stepvalue * TrackView.TicksToPixelFactor * ScaleX
            pte.X += stepvalue * TrackView.TicksToPixelFactor * ScaleX

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
