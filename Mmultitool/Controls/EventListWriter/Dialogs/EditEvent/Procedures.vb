Imports Mmultitool.EventListWriter
Partial Public Class DlgEditEvent


    Private Sub FillDialog()

        lblTime.Content = ConvertTimeToString(TrevX.Time, Evliwr.EvliTPQ, Evliwr.DesiredTimeFormat)
        lblTrk.Content = TrevX.TrackNumber
        lblChn.Content = TrevX.Channel
        lblTypeX.Content = TrevX.TypeX
        lblStatus.Content = ConvertStatusByte(TrevX.Status, Evliwr.DesiredStatusFormat)
        lblData1.Content = TrevX.Data1
        lblData2.Content = TrevX.Data2
        lblDataStr.Content = TrevX.DataStr






    End Sub

End Class
