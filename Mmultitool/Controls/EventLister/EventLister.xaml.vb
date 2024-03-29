﻿Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Text

Public Class EventLister
    Inherits UserControl

    Public Sub New()
        ' required for the designer
        InitializeComponent()

        cmbTimeFormat.ItemsSource = [Enum].GetValues(GetType(TimeFormat))
        cmbTimeFormat.SelectedIndex = 0

        cmbStatusFormat.ItemsSource = [Enum].GetValues(GetType(HexOrDec))
        cmbStatusFormat.SelectedIndex = 0

        cbflistTrack.ItemList = TrackList
        cbflistChannel.ItemList = ChannelList
        cbflistEventType.ItemList = [Enum].GetValues(GetType(EventTypeX))
    End Sub

    Private AllTrackEvents As New List(Of TrackEventX)                      ' input
    Public Property TrackEvents As New ObservableCollection(Of TrackEventX)          ' sorted by Time
    Public Property CollectionView As New ListCollectionView(TrackEvents)           ' Filtered View
    Public Property TicksPerQuarter As Integer

    Private TrackList As New List(Of Byte)
    Private ChannelList As New List(Of Byte)

#Region "Appearance"

    Public Shared ReadOnly DataGridRowBackgroundProperty As DependencyProperty = DependencyProperty.Register("DataGridRowBackground", GetType(Brush), GetType(EventLister))
    <Description("The default brush for all rows background")>
    Public Property DataGridRowBackground As Brush
        Get
            Return GetValue(DataGridRowBackgroundProperty)
        End Get
        Set(value As Brush)
            SetValue(DataGridRowBackgroundProperty, value)
        End Set
    End Property

#End Region


    Public Sub SetEventListContainer(evlic As EventListContainer)
        lblTpq.Content = 1
        TPQ = 1
        CollectionView.Filter = Nothing                     ' reset filter if any (= show all items)

        TrackEvents.Clear()

        If evlic Is Nothing Then Exit Sub
        If evlic.EventList Is Nothing Then Exit Sub

        lblTpq.Content = evlic.TPQ
        TPQ = evlic.TPQ

        '--- copy list to ObservableCollection

        For Each ev In evlic.EventList
            TrackEvents.Add(ev)
        Next

        '--- create TrackList (numbers)

        TrackList.Clear()
        Dim trk As Byte

        For Each ev In evlic.EventList
            trk = ev.TrackNumber
            If TrackList.Contains(trk) = False Then
                TrackList.Add(trk)
            End If
        Next

        '--- create ChannelList (numbers)

        ChannelList.Clear()
        Dim chan As Byte

        For Each ev In evlic.EventList
            chan = ev.Channel
            If ChannelList.Contains(chan) = False Then
                ChannelList.Add(chan)
            End If
        Next

        '--- update Filter Lists

        cbflistTrack.ItemListUpdate()
        cbflistChannel.ItemListUpdate()
        cbflistEventType.ItemListUpdate()             ' also updates Filter state to AllSelected

        CollectionView.Filter = AddressOf FilterFunction
        'CollectionView.Refresh()

        If DataGrid1.Items.Count > 0 Then
            DataGrid1.ScrollIntoView(DataGrid1.Items(0))
        End If
    End Sub

    Private Function FilterFunction(item As Object) As Boolean
        ' using early out, if a criteria is False then immediately return False.

        If cbflistTrack.SelectedNone = True Then Return False
        If cbflistChannel.SelectedNone = True Then Return False
        If cbflistEventType.SelectedNone = True Then Return False

        Dim trev As TrackEventX = TryCast(item, TrackEventX)
        If trev Is Nothing Then Return False

        '--- Track ---

        If cbflistTrack.SelectedAll = False Then
            If cbflistTrack.SelectedItems.Contains(trev.TrackNumber) = False Then
                Return False
            End If
        End If

        '--- Channel ---

        If cbflistChannel.SelectedAll = False Then
            If cbflistChannel.SelectedItems.Contains(trev.Channel) = False Then
                Return False
            End If
        End If

        '--- EventType ---

        If cbflistEventType.SelectedAll = False Then
            If cbflistEventType.SelectedItems.Contains(trev.TypeX) = False Then
                Return False
            End If
        End If

        Return True
    End Function

    Private Function FilterFunctionSingle(item As Object) As Boolean

        If cbflistTrack.SelectedAll = True Then Return True
        If cbflistTrack.SelectedNone = True Then Return False

        Dim trev As TrackEventX = TryCast(item, TrackEventX)

        If trev IsNot Nothing Then

            If cbflistTrack.SelectedItems.Contains(trev.TrackNumber) Then
                Return True
            Else
                Return False
            End If
        End If

        Return False
    End Function


    Private Function FilterFunctionExample(item As Object) As Boolean
        Dim emp As TrackEventX = TryCast(item, TrackEventX)

        If emp IsNot Nothing Then

            If emp.Channel = 2 Then
                Return True
            Else
                Return False
            End If

        End If

        Return False
    End Function





    Private Sub cmbTimeFormat_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbTimeFormat.SelectionChanged
        If cmbTimeFormat.SelectedItem IsNot Nothing Then
            If DesiredTimeFormat <> cmbTimeFormat.SelectedItem Then
                DesiredTimeFormat = cmbTimeFormat.SelectedItem
                CollectionView.Refresh()
            End If
        End If
    End Sub

    Private Sub cmbStatusFormat_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbStatusFormat.SelectionChanged
        If cmbStatusFormat.SelectedItem IsNot Nothing Then
            If DesiredStatusFormat <> cmbStatusFormat.SelectedItem Then
                DesiredStatusFormat = cmbStatusFormat.SelectedItem
                CollectionView.Refresh()
            End If
        End If
    End Sub

    Private Sub cbflistTrack_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles cbflistTrack.SelectionChanged
        CollectionView.Refresh()
    End Sub

    Private Sub cbflistChannel_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles cbflistChannel.SelectionChanged
        CollectionView.Refresh()
    End Sub

    Private Sub cbflistEventType_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles cbflistEventType.SelectionChanged
        CollectionView.Refresh()
    End Sub

    Private Sub ctxMi_Copy_Click(sender As Object, e As RoutedEventArgs) Handles ctxMi_Copy.Click
        CopySelectedItems(False)
    End Sub

    Private Sub ctxMi_CopyWithHeader_Click(sender As Object, e As RoutedEventArgs) Handles ctxMi_CopyWithHeader.Click
        CopySelectedItems(True)
    End Sub

    Private Sub CopySelectedItems(withHeader As Boolean)
        Dim sel = DataGrid1.SelectedItems
        If sel.Count > 0 Then
            Dim dao As New DataObject
            'dao.SetData(GetType(Text), Me.Pattern.Copy)
            dao.SetData("Text", TrackEventXListToText(sel, withHeader))
            Clipboard.SetDataObject(dao)
        End If

    End Sub

    Private Function TrackEventXListToText(lst As IList, withHeader As Boolean) As String
        If lst Is Nothing Then Return ""
        Dim sb As New StringBuilder

        If withHeader = True Then
            sb.Append("Time").Append(vbTab)
            sb.Append("Track").Append(vbTab)
            sb.Append("Channel").Append(vbTab)
            sb.Append("TypeX").Append(vbTab)
            sb.Append("Status").Append(vbTab)
            sb.Append("Data1").Append(vbTab)
            sb.Append("Data2").Append(vbTab)
            sb.Append("Duration").Append(vbTab)
            sb.Append("DataStr").Append(vbTab)
            sb.Append(vbCrLf)
        End If

        Dim trev As TrackEventX
        For Each item In lst
            trev = TryCast(item, TrackEventX)
            If trev IsNot Nothing Then
                ' Time and Status are converted depending on the selected Format (= as in the DataGrid)
                sb.Append(TimeToString_Converter.Convert(trev.Time)).Append(vbTab)
                sb.Append(trev.TrackNumber).Append(vbTab)
                sb.Append(trev.Channel).Append(vbTab)
                sb.Append(trev.TypeX.ToString).Append(vbTab)
                sb.Append(StatusByte_Converter.Convert(trev.Status)).Append(vbTab)
                sb.Append(trev.Data1).Append(vbTab)
                sb.Append(trev.Data2).Append(vbTab)
                sb.Append(trev.Duration).Append(vbTab)
                sb.Append(trev.DataStr).Append(vbTab)
                sb.Append(vbCrLf)
            End If
        Next
        Return sb.ToString
    End Function


End Class
