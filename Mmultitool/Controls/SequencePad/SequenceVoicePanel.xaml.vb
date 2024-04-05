Public Class SequenceVoicePanel

    Private Sub CtxMi_RemoveMe_Click(sender As Object, e As RoutedEventArgs) Handles CtxMi_RemoveMe.Click
        Dim par As Panel = Me.Parent
        If par IsNot Nothing Then
            Dim coll As UIElementCollection = par.Children
            par.Children.Remove(Me)
        End If
    End Sub

    Private mtrev As New TrackEventX With {.Type = EventType.MidiEvent}         ' local trev for manual send

    Private Sub btnTap_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles btnTap.PreviewMouseLeftButtonDown
        mtrev.Status = &H90 Or nudMidiChannel.Value
        mtrev.Data1 = &H40                              ' note number
        mtrev.Data2 = 100                               ' velocity
        Play_Manually(mtrev)
    End Sub

    Private Sub btnTap_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles btnTap.PreviewMouseLeftButtonUp
        mtrev.Status = &H90 Or nudMidiChannel.Value
        mtrev.Data1 = &H40                              ' note number
        mtrev.Data2 = 0                                 ' 0 = note off
        Play_Manually(mtrev)

    End Sub

    Private Sub nudMidiChannel_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudMidiChannel.ValueChanged
        Show_VoiceName_or_PatchName()
    End Sub

    Private Sub nudGmVoice_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudGmVoice.ValueChanged
        mtrev.Status = &HC0 Or nudMidiChannel.Value     ' program change
        mtrev.Data1 = nudGmVoice.Value
        mtrev.Data2 = 0
        Play_Manually(mtrev)

        Show_VoiceName_or_PatchName()
    End Sub

    Private Sub Show_VoiceName_or_PatchName()
        If nudMidiChannel.Value <> 9 Then
            LblVoicename.Content = Get_GM_VoiceName(nudGmVoice.Value)         ' it's a voice name
        Else
            Dim str As String = Get_GS_DrumPatchName(nudGmVoice.Value)      ' drum: maybe it's a known GS patch number
            If str = "" Then str = "Drum"                                   ' else 'Drum' is used
            LblVoicename.Content = str
        End If
    End Sub

    Private Sub ssldVolume_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldVolume.ValueChanged
        mtrev.Status = &HB0 Or nudMidiChannel.Value     ' control change
        mtrev.Data1 = 7                                 ' Channel volume coarse
        mtrev.Data2 = e.NewValue
        Play_Manually(mtrev)
    End Sub

    Private Sub ssldPan_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles ssldPan.ValueChanged
        mtrev.Status = &HB0 Or nudMidiChannel.Value     ' control change
        mtrev.Data1 = 10                                ' Panorama MSB
        mtrev.Data2 = e.NewValue
        Play_Manually(mtrev)
    End Sub

#Region "Move Panel"

    Private IsMoveMode As Boolean
    Private MouseBasePosition As Point              ' relative to this control
    Private MouseLastPosition As Point              ' relative to SequencePad
    Private MouseNewPosition As Point
    Private MousePositionDiff As Point

    Private Sub LblVoicename_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles LblVoicename.MouseLeftButtonDown
        LblVoicename.Focus()                ' Workaround for some debug situations
        ' capture mouse
        Dim el As UIElement = CType(sender, UIElement)
        el.CaptureMouse()
        MouseBasePosition = e.GetPosition(Me)
        MouseLastPosition = e.GetPosition(Me.Parent)
        Cursor = Cursors.SizeAll
        IsMoveMode = True
    End Sub

    Private Sub LblVoicename_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles LblVoicename.MouseLeftButtonUp
        IsMoveMode = False
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then
            el.ReleaseMouseCapture()
        End If
        Cursor = Cursors.Arrow
    End Sub

    Private Sub LblVoicename_MouseMove(sender As Object, e As MouseEventArgs) Handles LblVoicename.MouseMove
        If IsMoveMode = True Then
            MouseNewPosition = e.GetPosition(Me.Parent)

            ' Margin Left and Top should not be less than 0 / 0
            If MouseNewPosition.X < MouseBasePosition.X Then MouseNewPosition.X = MouseBasePosition.X
            If MouseNewPosition.Y < MouseBasePosition.Y Then MouseNewPosition.Y = MouseBasePosition.Y

            MousePositionDiff.X = MouseNewPosition.X - MouseLastPosition.X
            MousePositionDiff.Y = MouseNewPosition.Y - MouseLastPosition.Y

            MouseLastPosition = MouseNewPosition

            Dim marg As New Thickness
            marg.Left = Margin.Left + MousePositionDiff.X
            marg.Top = Margin.Top + MousePositionDiff.Y
            marg.Right = Margin.Right
            marg.Bottom = Margin.Bottom

            Margin = marg
        End If
    End Sub

    Private Sub LblVoicename_LostFocus(sender As Object, e As RoutedEventArgs) Handles LblVoicename.LostFocus
        ' Workaround for some debug situations
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then
            el.ReleaseMouseCapture()
        End If
        Cursor = Cursors.Arrow
        IsMoveMode = False
    End Sub

    Private Sub MainGrid_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles MainGrid.ContextMenuOpening
        '--- Controls with ContextMenu are allowed ---
        'If e.Source Is nudGmVoice Then Exit Sub

        '--- prevent from opening the ContextMenu from parent
        e.Handled = True
    End Sub



#End Region



End Class
