Imports DailyUserControls

Public Class DlgRemoveTrack

    Private Evliw As EventListWriter
    Public RetRemoveList As New List(Of Byte)

    Public Sub New(instance As EventListWriter)
        InitializeComponent()           ' required for the designer
        Evliw = instance
    End Sub


    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        ListView1.ItemsSource = Evliw.TrackList
        ListView1.Focus()
    End Sub

    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        Dim sel As EventListWriter.NamedTrack

        For Each item In ListView1.SelectedItems
            sel = TryCast(item, EventListWriter.NamedTrack)
            If sel IsNot Nothing Then
                RetRemoveList.Add(sel.TrackNumber)
            End If
        Next

        If RetRemoveList.Count = 0 Then
            Close()                             ' nothing selected, close
            Exit Sub
        End If

        Dim trkstr As String
        If RetRemoveList.Count = 1 Then
            trkstr = "this track ?"
        Else
            trkstr = "these " & RetRemoveList.Count & " tracks ?"
        End If

        '--- ask before remove --

        Dim qresult As QuestionWindowResult
        qresult = QuestionWindow.Show(Me, "Remove " & trkstr, "Remove Track", QuestionWindowButton.YesNo, Brushes.LightYellow)

        If qresult = QuestionWindowResult.Yes Then
            DialogResult = True
            Close()
        Else
            Close()
        End If
    End Sub


End Class
