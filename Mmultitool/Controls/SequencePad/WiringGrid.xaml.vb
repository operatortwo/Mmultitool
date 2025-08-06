Public Class WiringGrid

    Friend SequencePad As SequencePad                   ' Parent, set in SequencePad _Loaded

    Private LinePen As New Pen(Brushes.Red, 1.0)

    Protected Overrides Sub OnRender(dc As DrawingContext)
        MyBase.OnRender(dc)

        'IsHitTestVisible=False

        If SequencePad Is Nothing Then Exit Sub

        Dim pts As New Point
        Dim pte As New Point

        Dim TrigOutOffsetX As Double
        Dim TrigOutOffsetY As Double
        Dim TrigInOffsetX As Double
        Dim TrigInOffsetY As Double

        Dim sqb As SequenceBox
        For Each elem In SequencePad.MainPad.Children
            sqb = TryCast(elem, SequenceBox)                        ' Trigger Out
            If sqb IsNot Nothing Then

                For Each sqbnext In sqb.NextSequenceBox             ' Trigger In

                    TrigOutOffsetX = sqb.LblTriggerOut.Margin.Left
                    TrigOutOffsetY = sqb.LblTriggerOut.Margin.Top
                    TrigInOffsetX = sqb.LblTriggerIn.Margin.Left
                    TrigInOffsetY = sqb.LblTriggerIn.Margin.Top

                    pts.X = sqb.Margin.Left + TrigOutOffsetX + sqb.LblTriggerOut.Width
                    pts.Y = sqb.Margin.Top + TrigOutOffsetY + 30        ' Title + lblTrig /2

                    pte.X = sqbnext.Margin.Left + TrigInOffsetX
                    pte.Y = sqbnext.Margin.Top + TrigInOffsetY + 30

                    If sqbnext.Margin.Left > sqb.Margin.Left + sqb.ActualWidth Then
                        '--- if Next SequenceBox is on the right side ---
                        dc.DrawLine(LinePen, pts, pte)
                    Else
                        '--- if Next SequenceBox is on the left side ---
                        ' pts and pte are connecting points at output(pts) and input(pte)

                        Dim outup As Boolean            ' draw output flange upwards
                        Dim inup As Boolean             ' draw input flange upwards

                        If sqbnext.Margin.Top > sqb.Margin.Top + 50 Then inup = True
                        If sqb.Margin.Top > sqbnext.Margin.Top + 50 Then outup = True

                        Dim outpt As New Point          ' moving point for direct line at output
                        Dim inpt As New Point           ' moving point for direct line at input
                        Dim dstpt As New Point          ' draw to

                        outpt = pts
                        inpt = pte

                        '--- from out 20 to the right
                        MoveTo(pts.X, pts.Y)
                        DrawTo(dc, LinePen, pts.X + 20, pts.Y)
                        '--- up or down
                        If outup = True Then
                            DrawTo(dc, LinePen, DrawPoint.X, DrawPoint.Y - 20)        ' 20 up
                        Else
                            DrawTo(dc, LinePen, DrawPoint.X, DrawPoint.Y + 20)        ' 20 down
                        End If
                        outpt = DrawPoint       ' save this point

                        '--- from in 20 to the left
                        MoveTo(pte.X, pte.Y)
                        DrawTo(dc, LinePen, pte.X - 20, pte.Y)
                        '--- up or down
                        If inup = True Then
                            DrawTo(dc, LinePen, DrawPoint.X, DrawPoint.Y - 20)        ' 20 up
                        Else
                            DrawTo(dc, LinePen, DrawPoint.X, DrawPoint.Y + 20)        ' 20 down
                        End If
                        inpt = DrawPoint       ' save this point

                        '-- connection line
                        pts.X = pte.X
                        pts.Y = pte.Y
                        pte.X = sqbnext.Margin.Left + TrigInOffsetX - 20
                        pte.Y = sqbnext.Margin.Top + TrigInOffsetY + 30 + 20
                        dc.DrawLine(LinePen, outpt, inpt)
                    End If

                Next
            End If
        Next

    End Sub

    Private DrawPoint As New Point                      ' current draw position

    Private Sub MoveTo(x As Double, y As Double)
        DrawPoint.X = x
        DrawPoint.Y = y
    End Sub

    Private Sub DrawTo(dc As DrawingContext, pen As Pen, x As Double, y As Double)
        Dim pte As New Point(x, y)
        dc.DrawLine(pen, DrawPoint, pte)
        DrawPoint.X = x
        DrawPoint.Y = y
    End Sub
End Class
