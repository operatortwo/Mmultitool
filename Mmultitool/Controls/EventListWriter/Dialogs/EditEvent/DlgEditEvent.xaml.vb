Imports Mmultitool.EventListWriter
Public Class DlgEditEvent

    Private TrevX As TrackEventX
    Private Evliwr As EventListWriter

    Public Sub New(trev As TrackEventX, instance As EventListWriter)
        ' required for the designer
        InitializeComponent()

        TrevX = trev
        Evliwr = instance

    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        FillDialog()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Beep()
        Me.Close()
    End Sub
End Class
