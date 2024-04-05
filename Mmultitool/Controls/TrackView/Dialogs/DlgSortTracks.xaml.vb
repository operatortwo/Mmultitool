
Imports System.Collections.ObjectModel

Public Class DlgSortTracks

    Public Ret_Tracks As List(Of Track)                        ' sortable List of Tracks for UI

    'Private Trackview As TrackView

    Private Property TrackCollection As New ObservableCollection(Of Track)       ' local

    Public Sub New(ByRef UI_Tracks As List(Of Track))
        ' required for the Designer
        InitializeComponent()
        TrackCollection = New ObservableCollection(Of Track)(UI_Tracks)
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        'LvTracks.ItemsSource = Tracks
        LvTracks.ItemsSource = TrackCollection
    End Sub

    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        DialogResult = True
        Ret_Tracks = TrackCollection.ToList
        Close()
    End Sub

    Private Sub btnSortUp_Click(sender As Object, e As RoutedEventArgs) Handles btnSortUp.Click
        Dim source = TryCast(LvTracks.SelectedItem, Track)
        If source IsNot Nothing Then
            Dim SourceIndex = TrackCollection.IndexOf(source)
            Dim DestinationIndex As Integer
            If SourceIndex <> -1 Then
                If SourceIndex <> 0 Then
                    DestinationIndex = SourceIndex - 1
                    TrackCollection.RemoveAt(SourceIndex)
                    TrackCollection.Insert(DestinationIndex, source)
                    LvTracks.SelectedItem = source
                    LvTracks.ScrollIntoView(source)
                End If
            End If
        End If
    End Sub

    Private Sub btnSortDown_Click(sender As Object, e As RoutedEventArgs) Handles btnSortDown.Click
        Dim source = TryCast(LvTracks.SelectedItem, Track)
        If source IsNot Nothing Then
            Dim SourceIndex = TrackCollection.IndexOf(source)
            Dim DestinationIndex As Integer
            If SourceIndex <> -1 Then
                If SourceIndex + 1 < LvTracks.Items.Count Then
                    DestinationIndex = SourceIndex + 1
                    TrackCollection.RemoveAt(SourceIndex)
                    TrackCollection.Insert(DestinationIndex, source)
                    LvTracks.SelectedItem = source
                    LvTracks.ScrollIntoView(source)
                End If
            End If
        End If
    End Sub
End Class
