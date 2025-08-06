Imports System.ComponentModel
Imports DailyUserControls

Public Class SequenceBox

    Friend SequencePad As SequencePad                       ' Parent set at _Loaded

    Friend Sequence1 As Sequence

    Private Const MaxTrigOutWires = 5                       ' maximum of wires at Trigger Out
    Friend NextSequenceBox As New List(Of SequenceBox)      ' for Trigger Out
    Friend PreviousSequenceBox As SequenceBox               ' for backtracking  (Trigger In can have only 1 wire)

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        ' in DesignMode ProgressValue is > 0 to see example of RingFill Brush
        If DesignerProperties.GetIsInDesignMode(Me) = False Then
            ProgressRing1.ProgressValue = 0                         ' reset value
        End If

        SequencePad = FindLogicalParent(Me, GetType(SequencePad))
    End Sub

    Friend Sub ScreeRefresh()
        If Sequence1 IsNot Nothing Then
            Dim reltime As Integer = (SequencePlayerTime - Sequence1.StartTime) Mod Sequence1.Length
            If Sequence1.ID <> 0 Then
                ProgressRing1.ProgressValue = reltime
            Else
                If TgbtnPlay.IsChecked = True Then
                    If Sequence1.Ended = True Then
                        'wait until entire time is elapsed
                        If SequencePlayerTime > (Sequence1.StartTime + Sequence1.Duration) Then
                            TgbtnPlay.IsChecked = False
                            If ProgressRing1.ProgressValue <> 0 Then
                                ProgressRing1.ProgressValue = 0
                            End If
                        Else
                            ' keep animation running, even if Seqence1 played all events
                            ProgressRing1.ProgressValue = reltime
                        End If
                        CheckTriggerNext()
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub CheckTriggerNext()
        If NextSequenceBox.Count > 0 Then
            If SequencePlayerTime > (Sequence1.StartTime + Sequence1.Duration - PlayerTPQ) Then
                For Each nextsqb In NextSequenceBox
                    nextsqb.TgbtnPlay.IsChecked = True
                Next
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
        '---
        If SequencePad IsNot Nothing Then
            SequencePad.WiringGrid1.InvalidateVisual()
        End If
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
        If TgbtnPlay.IsChecked = True Then
            TgbtnPlay.IsChecked = False
        End If

        Dim win As New DlgImportMidiPattern
        win.Owner = Application.Current.MainWindow
        If win.ShowDialog() = True Then
            Sequence1 = win.RetSequence
            LblName.Content = win.RetSequence.Name
            lblBeatcount.Content = Math.Round(Sequence1.Length / PlayerTPQ, 2)
            ProgressRing1.MaximumValue = Sequence1.Length
        End If
    End Sub

    Private Sub CtxMi_RemoveMe_Click(sender As Object, e As RoutedEventArgs) Handles CtxMi_RemoveMe.Click
        If TgbtnPlay.IsChecked = True Then
            TgbtnPlay.IsChecked = False               ' stop playing
        End If

        '--- remove wires ---
        Dim redraw As Boolean

        For i = NextSequenceBox.Count - 1 To 0 Step -1
            NextSequenceBox(i).PreviousSequenceBox = Nothing
            NextSequenceBox.RemoveAt(i)
            redraw = True
        Next

        If PreviousSequenceBox IsNot Nothing Then
            PreviousSequenceBox.NextSequenceBox.Remove(Me)
            PreviousSequenceBox = Nothing
            redraw = True
        End If

        If redraw = True Then
            SequencePad.WiringGrid1.InvalidateVisual()
        End If

        '--- remove panel ---

        Dim par As Panel = Me.Parent
        If par IsNot Nothing Then
            Dim coll As UIElementCollection = par.Children
            par.Children.Remove(Me)
        End If
    End Sub

#End Region

#Region "Public Sub's"
    ''' <summary>
    ''' If Sequence is playing, stop it and set UI appearance to stop
    ''' </summary>
    Friend Sub StopPlaying()
        If TgbtnPlay.IsChecked = True Then
            TgbtnPlay.IsChecked = False
        End If
    End Sub

    ''' <summary>
    ''' If Sequence is not playing, start it and set UI appearance to running
    ''' </summary>
    Friend Sub StartPlaying()
        If TgbtnPlay.IsChecked = False Then
            TgbtnPlay.IsChecked = True
        End If
    End Sub


#End Region

    Private Sub TgbtnLoop_Checked(sender As Object, e As RoutedEventArgs) Handles TgbtnLoop.Checked
        NudDuration.IsEnabled = False
        If Sequence1 IsNot Nothing Then
            Sequence1.DoLoop = True
        End If
    End Sub

    Private Sub TgbtnLoop_Unchecked(sender As Object, e As RoutedEventArgs) Handles TgbtnLoop.Unchecked
        NudDuration.IsEnabled = True
        If Sequence1 IsNot Nothing Then
            Sequence1.DoLoop = False
        End If
    End Sub

    Private Sub TgbtnPlay_Checked(sender As Object, e As RoutedEventArgs) Handles TgbtnPlay.Checked
        If Sequence1 IsNot Nothing Then
            If Sequence1.ID = 0 Then
                Sequence1.ForceToChannel = CbForceToChannel.IsChecked
                Sequence1.DestinationChannel = NudChannel.Value
                If NudDuration.IsEnabled = True Then
                    Sequence1.Duration = Sequence1.Length * NudDuration.Value
                End If
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
                If ProgressRing1.ProgressValue <> 0 Then
                    ProgressRing1.ProgressValue = 0
                End If
            End If
        End If
    End Sub

    Private Sub CbForceToChannel_Checked(sender As Object, e As RoutedEventArgs) Handles CbForceToChannel.Checked
        NudChannel.IsEnabled = True
        If Sequence1 IsNot Nothing Then
            Sequence1.ForceToChannel = True
        End If
    End Sub

    Private Sub CbForceToChannel_Unchecked(sender As Object, e As RoutedEventArgs) Handles CbForceToChannel.Unchecked
        NudChannel.IsEnabled = False
        If Sequence1 IsNot Nothing Then
            Sequence1.ForceToChannel = False
        End If
    End Sub

    Private Sub NudChannel_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles NudChannel.ValueChanged
        If Sequence1 IsNot Nothing Then
            Sequence1.DestinationChannel = NudChannel.Value
        End If
    End Sub

    Private Sub NudDuration_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles NudDuration.ValueChanged
        If Sequence1 IsNot Nothing Then
            Sequence1.Duration = Sequence1.Length * NudDuration.Value
        End If
    End Sub

    Private Sub MainGrid_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles MainGrid.ContextMenuOpening
        '--- Controls with ContextMenu are allowed ---
        If e.Source Is LblTriggerIn Then Exit Sub
        If e.Source Is LblTriggerOut Then Exit Sub

        '--- prevent from opening the ContextMenu from parent
        e.Handled = True
    End Sub


#Region "Wire TriggerOut"

    Private Sub LblTriggerout_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles LblTriggerOut.MouseLeftButtonDown
        If NextSequenceBox.Count > 5 Then
            MessageWindow.Show("Trigger Out is limited to a maximum of 5 wires")
        End If
    End Sub

    Private Sub LblTriggerout_MouseMove(sender As Object, e As MouseEventArgs) Handles LblTriggerout.MouseMove
        If e.LeftButton = MouseButtonState.Pressed Then
            If NextSequenceBox.Count <= 5 Then
                Dim dataObject As New DataObject
                dataObject.SetData(GetType(SequenceBox), Me)     ' format as type
                DragDrop.DoDragDrop(Me, dataObject, DragDropEffects.Copy)
            End If
        End If
    End Sub

    Private Sub LblTriggerIn_DragOver(sender As Object, e As DragEventArgs) Handles LblTriggerIn.DragOver
        If e.Data.GetDataPresent(GetType(SequenceBox)) Then
            Dim seqbox As SequenceBox = e.Data.GetData(GetType(SequenceBox))
            If Not seqbox.Equals(Me) Then           ' is traget a different seqbox (do not connect to itself)
                If PreviousSequenceBox Is Nothing Then
                    e.Effects = DragDropEffects.Copy
                Else
                    e.Effects = DragDropEffects.None                ' if input is already wired
                End If
            Else
                    e.Effects = DragDropEffects.None
            End If
        End If
    End Sub

    Private Sub LblTriggerIn_Drop(sender As Object, e As DragEventArgs) Handles LblTriggerIn.Drop
        If e.Data.GetDataPresent(GetType(SequenceBox)) Then
            Dim seqbox As SequenceBox = e.Data.GetData(GetType(SequenceBox))    ' Source
            ' me to source.Nextobject
            seqbox.NextSequenceBox.Add(Me)
            Me.PreviousSequenceBox = seqbox
            SequencePad.WiringGrid1.InvalidateVisual()
        End If
    End Sub

    Private Sub MiLblTrigIn_RemoveWire_Click(sender As Object, e As RoutedEventArgs) Handles MiLblTrigIn_RemoveWire.Click
        PreviousSequenceBox.NextSequenceBox.Remove(Me)
        PreviousSequenceBox = Nothing
        SequencePad.WiringGrid1.InvalidateVisual()
    End Sub

    Private Sub LblTriggerIn_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles LblTriggerIn.ContextMenuOpening
        If PreviousSequenceBox Is Nothing Then
            e.Handled = True
        End If
    End Sub

    Private Sub MiLblTrigOut_RemoveWire_Click(sender As Object, e As RoutedEventArgs) Handles MiLblTrigOut_RemoveWire.Click
        For i = NextSequenceBox.Count - 1 To 0 Step -1
            NextSequenceBox(i).PreviousSequenceBox = Nothing
            NextSequenceBox.RemoveAt(i)
        Next
        SequencePad.WiringGrid1.InvalidateVisual()
    End Sub

    Private Sub LblTriggerOut_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles LblTriggerOut.ContextMenuOpening
        If NextSequenceBox.Count = 0 Then
            e.Handled = True
        End If
    End Sub










#End Region
End Class
