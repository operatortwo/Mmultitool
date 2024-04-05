Imports System.Windows.Controls.Primitives
Imports Mmultitool.EventListWriter
Public Class DlgEditEvent

    Private TrevX As TrackEventX
    Private Evliw As EventListWriter

    Private CurrentEvent As TrackEventX


    Public Property LocalTPQ As Integer = 1

    Public Sub New(trev As TrackEventX, instance As EventListWriter)
        ' required for the designer


        TrevX = trev
        Evliw = instance

        LocalTPQ = Evliw.EvliTPQ

        InitializeComponent()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        'Evliw.CollectionView.MoveCurrentTo(TrevX)
        CurrentIndex = Evliw.CollectionView.CurrentPosition

        'GetCurrentEvent()
        'CurrentEvent = Evliw.CollectionView.CurrentItem
        ShowEventInfo()


        lblPosition.Content = CurrentIndex
        lblPositionMax.Content = Evliw.CollectionView.Count - 1

        ScrollBar1.Maximum = Evliw.CollectionView.Count - 1
        ScrollBar1.Value = Evliw.CollectionView.CurrentPosition

        cb1.Focus()


        'TrackEvents.Item(1)
    End Sub


    Private _CurrentIndex As Integer
    Private Property CurrentIndex As Integer
        Get
            Return _CurrentIndex
        End Get
        Set(value As Integer)
            _CurrentIndex = value
            GetCurrentEvent()
            ShowEventInfo()
        End Set
    End Property

    Private Sub GetCurrentEvent()
        CurrentEvent = Nothing
        If Evliw Is Nothing Then Exit Sub
        If CurrentIndex < 0 Then Exit Sub
        If CurrentIndex >= Evliw.CollectionView.Count Then Exit Sub
        CurrentEvent = Evliw.CollectionView.GetItemAt(CurrentIndex)
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim lbl = lblPositionMax.Content

        Beep()
        Me.Close()
    End Sub


    Private Sub ScrollBar1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim scrollBar = CType(sender, ScrollBar)
        Dim newvalue As Double = Math.Round(e.NewValue, 0)
        If newvalue > scrollBar.Maximum Then
            newvalue = scrollBar.Maximum
        End If
        scrollBar.Value = newvalue
        CurrentIndex = newvalue
        If lblPosition IsNot Nothing Then
            lblPosition.Content = ScrollBar1.Value
        End If
    End Sub

    Private Sub MainGrid_KeyDown(sender As Object, e As KeyEventArgs) Handles MainGrid.KeyDown
        If e.Key = Key.Down Then
            ScrollBar1.Value = ScrollBar1.Value + 1
            e.Handled = True
        ElseIf e.Key = Key.Up Then
            ScrollBar1.Value = ScrollBar1.Value - 1
            e.Handled = True
        ElseIf e.Key = Key.Home Then
            ScrollBar1.Value = 0
            e.Handled = True
        ElseIf e.Key = Key.End Then
            ScrollBar1.Value = Evliw.TrackEvents.Count - 1
            e.Handled = True
        ElseIf e.Key = Key.PageUp Then
            ScrollBar1.Value = ScrollBar1.Value - ScrollBar1.LargeChange
            e.Handled = True
        ElseIf e.Key = Key.PageDown Then
            ScrollBar1.Value = ScrollBar1.Value + ScrollBar1.LargeChange
            e.Handled = True
        End If
    End Sub

    Private Sub MBT_InputBox1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles MBT_InputBox1.ValueChanged
        MBT_Editor1.ChangeCurrentValue(e.NewValue)
    End Sub

    Private Sub btnResetTime_Click(sender As Object, e As RoutedEventArgs) Handles btnResetTime.Click
        MBT_Editor1.ResetToOriginalValue()
    End Sub

    Private Sub NavigateGrid_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles NavigateGrid.MouseDown
        NavigateGrid.Focus()
    End Sub
End Class
