Public Class SequencePad

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        WiringGrid1.SequencePad = Me
    End Sub

    Public Sub ScreenRefresh()
        For Each element In MainPad.Children
            If element.GetType = GetType(SequenceBox) Then
                Dim sqb As SequenceBox
                sqb = TryCast(element, SequenceBox)
                If sqb IsNot Nothing Then
                    sqb.ScreeRefresh()
                End If
            End If
        Next
    End Sub

    Private ContextMenuClickPoint As New Point
    Private Sub MainPad_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles MainPad.ContextMenuOpening
        ContextMenuClickPoint = Mouse.GetPosition(MainPad)
    End Sub

    Private Sub CtxMi_AddSequenceVoicePanel_Click(sender As Object, e As RoutedEventArgs) Handles CtxMi_AddSequenceVoicePanel.Click
        Dim vpan As New SequenceVoicePanel
        vpan.HorizontalAlignment = HorizontalAlignment.Left
        vpan.VerticalAlignment = VerticalAlignment.Top
        Dim marg As Thickness
        marg.Left = ContextMenuClickPoint.X
        marg.Top = ContextMenuClickPoint.Y
        vpan.Margin = marg
        MainPad.Children.Add(vpan)
    End Sub

    Private Sub CtxMi_AddSequenceBox_Click(sender As Object, e As RoutedEventArgs) Handles CtxMi_AddSequenceBox.Click
        Dim seqbox As New SequenceBox
        seqbox.HorizontalAlignment = HorizontalAlignment.Left
        seqbox.VerticalAlignment = VerticalAlignment.Top
        Dim marg As Thickness
        marg.Left = ContextMenuClickPoint.X
        marg.Top = ContextMenuClickPoint.Y
        seqbox.Margin = marg
        MainPad.Children.Add(seqbox)
    End Sub

    Private Sub BtnStopAll_Click(sender As Object, e As RoutedEventArgs) Handles BtnStopAll.Click
        Dim sqb As SequenceBox
        For Each elem In MainPad.Children
            sqb = TryCast(elem, SequenceBox)
            If sqb IsNot Nothing Then
                sqb.StopPlaying()
            End If
        Next
    End Sub

    Private Sub BtnSyncStop_Click(sender As Object, e As RoutedEventArgs) Handles BtnSyncStop.Click
        Dim sqb As SequenceBox
        For Each elem In MainPad.Children
            sqb = TryCast(elem, SequenceBox)
            If sqb IsNot Nothing Then
                If sqb.TgbtnSync.IsChecked = True Then
                    sqb.StopPlaying()
                End If
            End If
        Next
    End Sub

    Private Sub BtnSyncStart_Click(sender As Object, e As RoutedEventArgs) Handles BtnSyncStart.Click
        Dim sqb As SequenceBox
        For Each elem In MainPad.Children
            sqb = TryCast(elem, SequenceBox)
            If sqb IsNot Nothing Then
                If sqb.TgbtnSync.IsChecked = True Then
                    sqb.StartPlaying()
                End If
            End If
        Next
    End Sub


End Class
