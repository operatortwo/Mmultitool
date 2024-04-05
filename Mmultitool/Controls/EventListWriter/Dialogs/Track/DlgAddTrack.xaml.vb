Public Class DlgAddTrack

    Private Evliw As EventListWriter

    Public Sub New(instance As EventListWriter)
        InitializeComponent()           ' required for the designer

        Evliw = instance
        MBTE1.TPQ = Evliw.EvliTPQ
        MBTE1.OriginalValue = Evliw.EvliTPQ / 3         ' initial position for TrackName event

    End Sub

    Private Sub ImageButton_Click(sender As Object, e As RoutedEventArgs)
        Beep()
    End Sub
End Class
