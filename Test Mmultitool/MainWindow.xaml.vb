Imports Mmultitool

Class MainWindow

    Private mifir As New MidifileRead

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        If mifir.ReadMidiFile("Echoes1.mid") = True Then
            ShowMidifileInfo()
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            EventLister1.SetEventListContainer(evlic)
        End If
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)


    End Sub

#Region "Tab EventLister"
    Private Sub btnEvListerOpenFile_Click(sender As Object, e As RoutedEventArgs) Handles btnEvListerOpenFile.Click
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        ofd.Filter = "Midi files|*.mid"
        If ofd.ShowDialog() = False Then Exit Sub
        Dim ret As Boolean

        Try
            ret = mifir.ReadMidiFile(ofd.FileName)
            tbEvListerFilename.Text = ofd.SafeFileName
            tbEvListerMessage.Clear()
            If ret = True Then
                ShowMidifileInfo()
            Else
                EvListerMessage("Errortext:" & mifir.ErrorText)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message & mifir.ErrorText, "Error reading Midi-File")
        End Try

        If ret = True Then
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(mifir.TrackList, mifir.TPQ)
            EventLister1.SetEventListContainer(evlic)
        End If
    End Sub

    Private Sub btnTbEvListerMessageClear_Click(sender As Object, e As RoutedEventArgs) Handles btnTbEvListerMessageClear.Click
        tbEvListerMessage.Clear()
    End Sub

    Private Sub ShowMidifileInfo()
        EvListerMessage("File loaded: " & mifir.MidiName)
        EvListerMessage("Format: " & mifir.SmfFormat & " - " & "Time division: " & mifir.TPQ)
        EvListerMessage("Tracks count: " & mifir.NumberOfTracks)
    End Sub

    Private Sub EvListerMessage(str As String)
        If tbEvListerMessage.LineCount > 50 Then tbEvListerMessage.Clear()
        tbEvListerMessage.AppendText(str & vbCrLf)
        tbEvListerMessage.ScrollToEnd()
    End Sub

#End Region

End Class
