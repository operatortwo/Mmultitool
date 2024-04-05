Imports Mmultitool.KeyPanel
Imports System.Collections.ObjectModel
Imports System.ComponentModel

Public Class DlgSetKeyList

    Private TrackPanel As TrackPanel
    Private KeyPanel As KeyPanel

    Public Property AllKeys As New ObservableCollection(Of KeyItem)
    Public Property SelectedKeys As New ObservableCollection(Of KeyItem)
    Public Property SelectedKeysView As New CollectionViewSource()
    Private SortDescending As SortDescription = New SortDescription("NoteNumber", ListSortDirection.Descending)

    Private UsedNotesTabEntered As Boolean                   ' as marker for IsChanged

    '--- defined again with Properties for WPF ---
    Public Class KeyItem
        Public KeyType As KeyType = KeyType.Note
        Public Property NoteNumber As Byte                          ' 0-127
        Public Property NoteName As String = ""                     ' like "A4"
        Public Property DrumName As String = ""                     ' like "Acoustic Snare"
        Public IsBlackKey As Boolean
        Public Height As Integer = KeyItemDefaultHeight ' default = 20
        Public StartPosition As Integer
    End Class

    Public Sub New(parent As TrackPanel)
        InitializeComponent()                       ' required for the designer
        TrackPanel = parent                         ' -> TrackPanel.TrackData, TrackPanel.KeyPanel1
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        If TrackPanel IsNot Nothing Then
            If TrackPanel.KeyPanel IsNot Nothing Then
                KeyPanel = TrackPanel.KeyPanel
                SetInitialValues()
            End If
        End If

        FillAllKeysList()

        btnUsedNotes_Click(Me, New RoutedEventArgs)
        SelectedKeysView.Source = SelectedKeys
        SelectedKeysView.SortDescriptions.Add(SortDescending)
        lvSelectedKeys.ItemsSource = SelectedKeysView.View
    End Sub

    Private Sub tiList_GotFocus(sender As Object, e As RoutedEventArgs) Handles tiList.GotFocus
        lvAllKeys.ScrollIntoView(AllKeys.Item(57))
        UsedNotesTabEntered = True
    End Sub



    Private Sub SetInitialValues()
        nudFullRangeFirstNote.Value = KeyPanel.FullRangeFirstNote
        nudFullRangeNumNotes.Value = KeyPanel.FullRangeNumberOfNotes
        nudFullRangeLastNote.Value = KeyPanel.FullRangeFirstNote + KeyPanel.FullRangeNumberOfNotes - 1

        nudRangeFirstNote.Value = KeyPanel.RangeFirstNote
        nudRangeNumNotes.Value = KeyPanel.RangeNumberOfNotes
        nudRangeLastNote.Value = KeyPanel.RangeFirstNote + KeyPanel.RangeNumberOfNotes - 1

        ' used notes KeyPanel.UsedNotes
    End Sub


#Region "Full Range"
    Private Sub nudFullRangeLastNote_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudFullRangeLastNote.ValueChanged
        lblFullRangeLastNoteName.Content = NoteNumber_to_NoteName(e.NewValue)
    End Sub
    Private Sub nudFullRangeNumNotes_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudFullRangeNumNotes.ValueChanged
        If IsLoaded = False Then Exit Sub
        nudFullRangeLastNote.Value = nudFullRangeFirstNote.Value + e.NewValue - 1
    End Sub
    Private Sub nudFullRangeFirstNote_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudFullRangeFirstNote.ValueChanged
        lblFullRangeFirstNoteName.Content = NoteNumber_to_NoteName(e.NewValue)
        nudFullRangeLastNote.Value = nudFullRangeNumNotes.Value + e.NewValue - 1
    End Sub

    '--- Standard Keyboard Ranges 61,76,88 keys ---
    Private Sub btnFullRange61Keys_Click(sender As Object, e As RoutedEventArgs) Handles btnFullRange61Keys.Click
        nudFullRangeNumNotes.SetValueSilent(61)
        nudFullRangeFirstNote.Value = 36
    End Sub
    Private Sub btnFullRange76Keys_Click(sender As Object, e As RoutedEventArgs) Handles btnFullRange76Keys.Click
        nudFullRangeNumNotes.SetValueSilent(76)
        nudFullRangeFirstNote.Value = 28
    End Sub
    Private Sub btnFullRange88Keys_Click(sender As Object, e As RoutedEventArgs) Handles btnFullRange88Keys.Click
        nudFullRangeNumNotes.SetValueSilent(88)
        nudFullRangeFirstNote.Value = 21
    End Sub

#End Region

#Region "Range"

    Private Sub nudRangeLastNote_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudRangeLastNote.ValueChanged
        lblRangeLastNoteName.Content = NoteNumber_to_NoteName(e.NewValue)
    End Sub

    Private Sub nudRangeNumNotes_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudRangeNumNotes.ValueChanged
        If IsLoaded = False Then Exit Sub
        nudRangeLastNote.Value = nudRangeFirstNote.Value + e.NewValue - 1
    End Sub

    Private Sub nudRangeFirstNote_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudRangeFirstNote.ValueChanged
        lblRangeFirstNoteName.Content = NoteNumber_to_NoteName(e.NewValue)
        nudRangeLastNote.Value = nudRangeNumNotes.Value + e.NewValue - 1
    End Sub

    Private Sub btnRangeC2_25_Click(sender As Object, e As RoutedEventArgs) Handles btnRangeC2_25.Click
        nudRangeFirstNote.Value = 36                ' 36 (C2)
        nudRangeNumNotes.Value = 25
    End Sub

    Private Sub btnRange_UsedRange_Click(sender As Object, e As RoutedEventArgs) Handles btnRange_UsedRange.Click
        GetUsedRange()
    End Sub

    Private Sub GetUsedRange()
        tbRangeMessage.Clear()
        If TrackPanel Is Nothing Then
            tbRangeMessage.AppendText("TrackPanel is Nothing")
            Exit Sub
        End If
        If TrackPanel.TrackData Is Nothing Then
            tbRangeMessage.AppendText("TrackData is Nothing")
            Exit Sub
        End If
        If TrackPanel.TrackData.EventList.Count = 0 Then
            tbRangeMessage.AppendText("EventList is empty")
            Exit Sub
        End If

        tbRangeMessage.AppendText(TrackPanel.TrackData.EventList.Count & " Events")


        Dim nfo As Track.UsedNotesInfo = TrackPanel.TrackData.GetUsedNotesInfo


        nudRangeFirstNote.Value = nfo.NoteRangeStart
        nudRangeNumNotes.Value = nfo.NoteRangeEnd - nfo.NoteRangeStart + 1

    End Sub



#End Region

#Region "List"

    Private Sub FillAllKeysList()
        For i = 127 To 0 Step -1
            AddKeyItem(AllKeys, i)
        Next
    End Sub

    Private Sub btnUsedNotes_Click(sender As Object, e As RoutedEventArgs) Handles btnUsedNotes.Click
        GetUsedNotes()
    End Sub

    Private Sub GetUsedNotes()

        Dim nfo As Track.UsedNotesInfo = TrackPanel.TrackData.GetUsedNotesInfo

        SelectedKeys.Clear()

        For Each num In nfo.ListOfUsedNotes
            AddKeyItem(SelectedKeys, num)
        Next


    End Sub

    Private Sub AddKeyItem(List As ObservableCollection(Of KeyItem), Note As Byte)
        Dim item As New KeyItem
        item.NoteNumber = Note
        item.NoteName = GetNoteNameFromNoteNumber(Note)
        item.DrumName = Get_GM_DrumVoiceName(Note)
        List.Add(item)
    End Sub


    Private Sub lvAllKeys_MouseMove(sender As Object, e As MouseEventArgs) Handles lvAllKeys.MouseMove
        If e.LeftButton = MouseButtonState.Pressed Then
            If TypeOf e.OriginalSource IsNot Primitives.Thumb Then      ' allow dragging thumb
                Dim listview = TryCast(sender, ListView)
                If listview IsNot Nothing Then
                    If listview.SelectedItem IsNot Nothing Then
                        Dim dataObject As New DataObject
                        dataObject.SetData(GetType(KeyItem), listview.SelectedItem)     ' format as type
                        'DoDragDrop is a blocking function, returns when the operation ended
                        DragDrop.DoDragDrop(listview, dataObject, DragDropEffects.Copy)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub lvSelectedKeys_Drop(sender As Object, e As DragEventArgs) Handles lvSelectedKeys.Drop
        '--- try to get index at drop point ---
        Dim index As Integer = -1
        Dim listview = TryCast(sender, ListView)
        If listview IsNot Nothing Then
            Dim listViewItem As ListViewItem = FindVisualParent(e.OriginalSource, GetType(ListViewItem))
            If listViewItem IsNot Nothing Then
                index = listview.ItemContainerGenerator.IndexFromContainer(listViewItem)
            End If
        End If

        If e.Data.GetDataPresent(GetType(KeyItem)) Then
            Dim item As KeyItem = e.Data.GetData(GetType(KeyItem))
            If item IsNot Nothing Then
                '--- check if not already isted
                If SelectedKeys.FirstOrDefault(Function(x) x.NoteNumber = item.NoteNumber) Is Nothing Then
                    If index = -1 Then
                        SelectedKeys.Add(item)
                    Else
                        SelectedKeys.Insert(index, item)
                    End If
                Else
                    '--- don't insert duplicates
                    Beep()
                End If
            End If
        End If
    End Sub

    Public Function FindVisualParent(base As DependencyObject, targetType As Type) As Object

        'Dim current As DependencyObject = VisualTreeHelper.GetParent(base)
        Dim current As DependencyObject = base

        While current IsNot Nothing
            If current.GetType = targetType Then
                Return current
            End If
            current = VisualTreeHelper.GetParent(current)
        End While
        Return Nothing
    End Function

#End Region


#Region "Dialog"
    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        Dim btn = btnOk
        DialogResult = True
        UpdateKeys()
        Close()
    End Sub

    Private Sub UpdateKeys()
        If KeyPanel Is Nothing Then Exit Sub
        '--- check if something was changed ---
        If KeyPanel.FullRangeFirstNote = nudFullRangeFirstNote.Value Then
            If KeyPanel.FullRangeNumberOfNotes = nudFullRangeNumNotes.Value Then
                If KeyPanel.RangeFirstNote = nudRangeFirstNote.Value Then
                    If KeyPanel.RangeNumberOfNotes = nudRangeNumNotes.Value Then
                        '--- used notes
                        If UsedNotesTabEntered = False Then
                            Exit Sub                                ' nothing eas changed exit
                        End If
                    End If
                    End If
            End If
        End If

        '--- update values ---

        KeyPanel.FullRangeFirstNote = nudFullRangeFirstNote.Value
        KeyPanel.FullRangeNumberOfNotes = nudFullRangeNumNotes.Value
        KeyPanel.RangeFirstNote = nudRangeFirstNote.Value
        KeyPanel.RangeNumberOfNotes = nudRangeNumNotes.Value

        If UsedNotesTabEntered = True Then
            KeyPanel.UsedNotes.Clear()

            For Each item In SelectedKeys
                KeyPanel.UsedNotes.Add(item.NoteNumber)
            Next

        End If

        KeyPanel.FillKeyLists()
        KeyPanel.UpdateView()

    End Sub

    Private Sub cbSortDescending_Click(sender As Object, e As RoutedEventArgs) Handles cbSortDescending.Click
        If cbSortDescending.IsChecked = True Then
            SelectedKeysView.SortDescriptions.Add(SortDescending)
            btnSortUp.IsEnabled = False
            btnSortDown.IsEnabled = False
        Else
            SelectedKeysView.SortDescriptions.Clear()
            btnSortUp.IsEnabled = True
            btnSortDown.IsEnabled = True
        End If
    End Sub

    Private Sub btnSortUp_Click(sender As Object, e As RoutedEventArgs) Handles btnSortUp.Click
        Dim source = TryCast(lvSelectedKeys.SelectedItem, KeyItem)
        If source IsNot Nothing Then
            Dim SourceIndex = SelectedKeys.IndexOf(source)
            Dim DestinationIndex As Integer
            If SourceIndex <> -1 Then
                If SourceIndex <> 0 Then
                    DestinationIndex = SourceIndex - 1
                    SelectedKeys.RemoveAt(SourceIndex)
                    SelectedKeys.Insert(DestinationIndex, source)
                    lvSelectedKeys.SelectedItem = source
                    lvSelectedKeys.ScrollIntoView(source)
                End If
            End If
        End If
    End Sub

    Private Sub btnSortDown_Click(sender As Object, e As RoutedEventArgs) Handles btnSortDown.Click
        Dim source = TryCast(lvSelectedKeys.SelectedItem, KeyItem)
        If source IsNot Nothing Then
            Dim SourceIndex = SelectedKeys.IndexOf(source)
            Dim DestinationIndex As Integer
            If SourceIndex <> -1 Then
                If SourceIndex + 1 < lvSelectedKeys.Items.Count Then
                    DestinationIndex = SourceIndex + 1
                    SelectedKeys.RemoveAt(SourceIndex)
                    SelectedKeys.Insert(DestinationIndex, source)
                    lvSelectedKeys.SelectedItem = source
                    lvSelectedKeys.ScrollIntoView(source)
                End If
            End If
        End If
    End Sub













#End Region

End Class
