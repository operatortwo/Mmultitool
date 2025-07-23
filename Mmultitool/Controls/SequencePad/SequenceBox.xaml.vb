Imports System.ComponentModel

Public Class SequenceBox

    Friend Sequence1 As Sequence

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        ' in DesignMode ProgressValue is > 0 to see example of RingFill Brush
        If DesignerProperties.GetIsInDesignMode(Me) = False Then
            ProgressRing1.ProgressValue = 0                         ' reset value
        End If
    End Sub

    Friend Sub ScreeRefresh()
        If Sequence1 IsNot Nothing Then
            ' If Sequence1.Ended = False Then
            ' assuming: IsPlaying                    
            Dim reltime As Integer = SequencePlayerTime - Sequence1.StartTime - Sequence1.StartOffset
            If reltime <= Sequence1.Length Then
                ProgressRing1.ProgressValue = reltime
            Else
                If TgbtnPlay.IsChecked = True Then
                    If Sequence1.Ended = True Then
                        TgbtnPlay.IsChecked = False
                        ProgressRing1.ProgressValue = 0
                    End If
                End If
            End If
        End If
    End Sub


#Region "Move Panel"

    Private IsMoveMode As Boolean
    Private MouseBasePosition As Point              ' relative to this control
    Private MouseLastPosition As Point              ' relative to SequencePad
    Private MouseNewPosition As Point
    Private MousePositionDiff As Point

    Private Sub LblName_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles LblName.MouseLeftButtonDown
        LblName.Focus()                ' Workaround for some debug situations
        ' capture mouse
        Dim el As UIElement = CType(sender, UIElement)
        el.CaptureMouse()
        MouseBasePosition = e.GetPosition(Me)
        MouseLastPosition = e.GetPosition(Me.Parent)
        Cursor = Cursors.SizeAll
        IsMoveMode = True
    End Sub

    Private Sub LblName_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles LblName.MouseLeftButtonUp
        IsMoveMode = False
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then
            el.ReleaseMouseCapture()
        End If
        Cursor = Cursors.Arrow
    End Sub

    Private Sub LblName_MouseMove(sender As Object, e As MouseEventArgs) Handles LblName.MouseMove
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

    Private Sub LblName_LostFocus(sender As Object, e As RoutedEventArgs) Handles LblName.LostFocus
        ' Workaround for some debug situations
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then
            el.ReleaseMouseCapture()
        End If
        Cursor = Cursors.Arrow
        IsMoveMode = False
    End Sub

#End Region

#Region "Context Menu"

    Private Sub CtxMi_ImportMidiPattern_Click(sender As Object, e As RoutedEventArgs) Handles CtxMi_ImportMidiPattern.Click
        Dim win As New DlgImportMidiPattern
        win.Owner = Application.Current.MainWindow
        If win.ShowDialog() = True Then
            Sequence1 = win.RetSequence
            LblName.Content = win.RetSequence.Name
            ProgressRing1.MaximumValue = Sequence1.Length
        End If

    End Sub

#End Region

    Private Sub TgbtnLoop_Checked(sender As Object, e As RoutedEventArgs) Handles TgbtnLoop.Checked
        NudRepeatCount.IsEnabled = False
        If Sequence1 IsNot Nothing Then
            Sequence1.DoLoop = True
        End If
    End Sub

    Private Sub TgbtnLoop_Unchecked(sender As Object, e As RoutedEventArgs) Handles TgbtnLoop.Unchecked
        NudRepeatCount.IsEnabled = True
        If Sequence1 IsNot Nothing Then
            Sequence1.DoLoop = False
        End If
    End Sub

    Private Sub TgbtnPlay_Checked(sender As Object, e As RoutedEventArgs) Handles TgbtnPlay.Checked
        If Sequence1 IsNot Nothing Then
            If Sequence1.ID = 0 Then
                PlaySequence(Sequence1, TgbtnLoop.IsChecked)
            End If
        Else
            TgbtnPlay.IsChecked = False
        End If
    End Sub

    Private Sub TgbtnPlay_Unchecked(sender As Object, e As RoutedEventArgs) Handles TgbtnPlay.Unchecked
        If Sequence1 IsNot Nothing Then
            If Sequence1.ID <> 0 Then
                RemoveSequence(Sequence1)
            End If
        End If
    End Sub
End Class
