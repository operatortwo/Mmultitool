Public Class DlgAddTrack


    Public RetAddTrackNameEvent As Boolean
    Public RetTrackName As String
    Public RetPosition As UInteger


    Private Evliw As EventListWriter

    Public Sub New(instance As EventListWriter)
        InitializeComponent()           ' required for the designer

        Evliw = instance
        MBTE1.TPQ = Evliw.EvliTPQ
        MBTE1.OriginalValue = Evliw.EvliTPQ / 3         ' initial position for TrackName event

    End Sub

    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        RetAddTrackNameEvent = cbAddTrackEvent.IsChecked
        RetTrackName = tbTrackName.Text
        RetPosition = MBTE1.NewValue
        DialogResult = True
        Close()
    End Sub
End Class
