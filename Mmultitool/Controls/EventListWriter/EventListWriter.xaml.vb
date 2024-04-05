Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Configuration
Imports System.Data
Imports System.Text
Imports DailyUserControls

Public Class EventListWriter
    Inherits UserControl

    Public Sub New()
        ' required for the designer
        InitializeComponent()

        '--- user can select time format ---
        cmbTimeFormat.ItemsSource = [Enum].GetValues(GetType(TimeFormat))
        cmbTimeFormat.SelectedIndex = 1

        '--- user can select status format ---
        cmbStatusFormat.ItemsSource = [Enum].GetValues(GetType(HexOrDec))
        cmbStatusFormat.SelectedIndex = 0

        '--- user can filter events ---
        cbflistTrack.ItemList = TrackList
        cbflistTrack.DisplayMember = "TrackName"
        cbflistChannel.ItemList = ChannelList
        cbflistEventType.ItemList = [Enum].GetValues(GetType(EventTypeX))

        '--- time and status use an internal converter ---
        Dim bind As Binding = TimeCol.Binding
        bind.Converter = TimeConvert                ' set converter for Time Column
        bind = StatusCol.Binding
        bind.Converter = StatusConvert              ' set converter for Status Column

        CollectionView.Filter = AddressOf FilterFunction

    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        If Prefer_MBT_1_1_0 = False Then
            cmbTimeFormat.SelectedIndex = 1
        Else
            cmbTimeFormat.SelectedIndex = 2
        End If
    End Sub


    '--- main data ---
    ''' <summary>
    ''' Collection of all Events, sorted by Time
    ''' </summary>    
    Public Property TrackEvents As New ModifiedObservableCollection(Of TrackEventX)         ' sorted by Time
    'Public Property TrackEvents As New ObservableCollection(Of TrackEventX)         ' sorted by Time
    Public Property CollectionView As New ListCollectionView(TrackEvents)           ' Filtered View

    '--- auxiliary lists ---
    Friend ReadOnly TrackList As New List(Of NamedTrack)
    Friend ReadOnly ChannelList As New List(Of Byte)

    Friend Class NamedTrack
        Implements IComparer(Of NamedTrack)
        Public Property TrackNumber As Byte
        Public Property TrackName As String = ""

        Public Function Compare(x As NamedTrack, y As NamedTrack) As Integer Implements IComparer(Of NamedTrack).Compare
            ' Return Value              Meaning
            '---------------------------------------------
            ' Less than zero    (-1)    x is less than y
            ' Zero              (0)     x equals y
            ' Greater than zero (1)     x is greater than y

            If x Is Nothing Then Return -1
            If y Is Nothing Then Return 1
            If x.TrackNumber < y.TrackNumber Then Return -1
            If x.TrackNumber > y.TrackNumber Then Return 1
            Return 0
        End Function
    End Class

#Region "Properties"

#Region "TPQ Property"

    Public Shared ReadOnly EvliTPQProperty As DependencyProperty = DependencyProperty.Register("EvliTPQ", GetType(Integer), GetType(EventListWriter),
            New FrameworkPropertyMetadata(120, New PropertyChangedCallback(AddressOf OnEvliTPQChanged),
            New CoerceValueCallback(AddressOf CoerceEvliTPQ)))

    <Description("Ticks per quarter note"), Category("EventListWriter")>   ' appears in VS property
    Public Property EvliTPQ() As Integer
        Get
            Return (GetValue(EvliTPQProperty))
        End Get
        Set(ByVal value As Integer)
            SetValue(EvliTPQProperty, value)
        End Set
    End Property

    Private Overloads Shared Function CoerceEvliTPQ(ByVal d As DependencyObject, ByVal value As Object) As Object
        Dim newValue As Integer = value
        If newValue < 1 Then
            Return 1
        ElseIf newValue > 2000 Then
            Return 2000
        End If
        Return newValue
    End Function

    Private Shared Sub OnEvliTPQChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As EventListWriter = CType(d, EventListWriter)
    End Sub

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

#End Region

    Public Sub SetEventListContainer(evlic As EventListContainer)
        lblTpq.Content = 1
        EvliTPQ = 1
        CollectionView.Filter = Nothing                     ' reset filter if any (= show all items)

        TrackEvents.Clear()

        If evlic Is Nothing Then Exit Sub
        If evlic.EventList Is Nothing Then Exit Sub

        lblTpq.Content = evlic.TPQ
        EvliTPQ = evlic.TPQ

        '--- copy source list to ObservableCollection ---

        For Each ev In evlic.EventList
            TrackEvents.Add(ev)
        Next

        '--- create TrackList (numbers + name) ---

        TrackList.Clear()

        Dim trknumlist As New List(Of Byte)
        Dim trk As Byte

        For Each ev In evlic.EventList
            trk = ev.TrackNumber
            If trknumlist.Contains(trk) = False Then
                trknumlist.Add(trk)
            End If
        Next

        Dim ntrk As NamedTrack

        For Each trnum In trknumlist
            ntrk = New NamedTrack
            ntrk.TrackNumber = trnum
            ntrk.TrackName = GetTrackName(trnum, evlic.EventList)
            TrackList.Add(ntrk)
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

        ChannelList.Sort()

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


#Region "Time and Status format"

    Public Shared ReadOnly DesiredTimeFormatProperty As DependencyProperty = DependencyProperty.Register("DesiredTimeFormat", GetType(TimeFormat), GetType(EventListWriter), New PropertyMetadata(TimeFormat.MBT_0_based))
    <Description("Format of the Time column"), Category("EventListWriter")>
    Public Property DesiredTimeFormat As TimeFormat
        Get
            Return GetValue(DesiredTimeFormatProperty)
        End Get
        Set(value As TimeFormat)
            SetValue(DesiredTimeFormatProperty, value)
        End Set
    End Property

    Public Enum TimeFormat
        Ticks
        MBT_0_based
        MBT_1_based
    End Enum

    Public Shared ReadOnly DesiredStatusFormatProperty As DependencyProperty = DependencyProperty.Register("DesiredStatusFormat", GetType(HexOrDec), GetType(EventListWriter), New PropertyMetadata(HexOrDec.Hex))
    <Description("Format of the Status column"), Category("EventListWriter")>
    Public Property DesiredStatusFormat As HexOrDec
        Get
            Return GetValue(DesiredStatusFormatProperty)
        End Get
        Set(value As HexOrDec)
            SetValue(DesiredStatusFormatProperty, value)
        End Set
    End Property

    Public Enum HexOrDec
        Hex
        Dec
    End Enum

#End Region

#Region "DataGrid Row Background"

    Public Shared ReadOnly DataGridRowBackgroundProperty As DependencyProperty = DependencyProperty.Register("DataGridRowBackground", GetType(Brush), GetType(EventListWriter))
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

#Region "DataGrid Border Focus"

    Private Shared DataGridNotFocusedDefaultBrush As New SolidColorBrush(Color.FromArgb(&HFF, &H68, &H8C, &HAF))

    Public Shared ReadOnly DataGridBorderBrushNotFocusedProperty As DependencyProperty = DependencyProperty.Register("DataGridBorderBrushNotFocused", GetType(Brush), GetType(EventListWriter), New UIPropertyMetadata(DataGridNotFocusedDefaultBrush))
    ' appears in code
    ''' <summary>
    ''' Border color when the button is pressed
    ''' </summary>
    <Description("BorderBrush when DataGrid is not focused")>
    Public Property DataGridBorderBrushNotFocused As Brush
        Get
            Return CType(GetValue(DataGridBorderBrushNotFocusedProperty), Brush)
        End Get
        Set(ByVal value As Brush)
            SetValue(DataGridBorderBrushNotFocusedProperty, value)
        End Set
    End Property

    Public Shared ReadOnly DataGridBorderBrushFocusedProperty As DependencyProperty = DependencyProperty.Register("DataGridBorderBrushFocused", GetType(Brush), GetType(EventListWriter), New UIPropertyMetadata(Brushes.Blue))
    ' appears in code
    ''' <summary>
    ''' Border color when the button is pressed
    ''' </summary>
    <Description("BorderBrush when DataGrid is focused")>
    Public Property DataGridBorderBrushFocused As Brush
        Get
            Return CType(GetValue(DataGridBorderBrushFocusedProperty), Brush)
        End Get
        Set(ByVal value As Brush)
            SetValue(DataGridBorderBrushFocusedProperty, value)
        End Set
    End Property

    Private Sub DataGrid1_GotKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs) Handles DataGrid1.GotKeyboardFocus
        DataGrid1.BorderBrush = DataGridBorderBrushFocused
        'SetFocusToSelectedRow()
    End Sub

    Private Sub DataGrid1_LostKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs) Handles DataGrid1.LostKeyboardFocus
        DataGrid1.BorderBrush = DataGridBorderBrushNotFocused
    End Sub

    Private Sub DataGrid1_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles DataGrid1.MouseDown
        DataGrid1.Focus()
        SetFocusToSelectedRow()
    End Sub

    Private Sub DataGrid1_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs) Handles DataGrid1.PreviewMouseDown
        ' handling MouseDown on SelectAll Button (top left rectangle)
        ' SelectAll works only when Datagrid is focused
        If DataGrid1.IsFocused = False Then
            DataGrid1.Focus()
        End If
    End Sub

#End Region

#End Region
    ' set eventlist

    Private Function FilterFunction(item As Object) As Boolean
        ' using early out, if a criteria is False then immediately return False.

        If cbflistTrack.SelectedNone = True Then Return False
        If cbflistChannel.SelectedNone = True Then Return False
        If cbflistEventType.SelectedNone = True Then Return False

        Dim trev As TrackEventX = TryCast(item, TrackEventX)
        If trev Is Nothing Then Return False

        '--- Track ---
        If cbflistTrack.SelectedAll = False Then
            Dim trkfound As Boolean
            Dim ntrk As NamedTrack
            For Each item In cbflistTrack.SelectedItems
                ntrk = TryCast(item, NamedTrack)
                If ntrk IsNot Nothing Then
                    If ntrk.TrackNumber = trev.TrackNumber Then
                        trkfound = True
                        Exit For
                    End If
                End If
            Next

            If trkfound = False Then Return False
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

    Public Sub SelectAll()
        DataGrid1.SelectAll()               ' a fast way to select all events in the DataGrid
    End Sub

    Private Sub cbflistTrack_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles cbflistTrack.SelectionChanged
        CollectionView.Refresh()
        ScrollToFirstSelectedItem()
    End Sub

    Private Sub cbflistChannel_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles cbflistChannel.SelectionChanged
        CollectionView.Refresh()
        ScrollToFirstSelectedItem()
    End Sub

    Private Sub cbflistEventType_SelectionChanged(sender As Object, e As RoutedEventArgs) Handles cbflistEventType.SelectionChanged
        CollectionView.Refresh()
        ScrollToFirstSelectedItem()
    End Sub

    Private Sub ScrollToFirstSelectedItem()
        If DataGrid1.Items.Count > 0 Then
            If DataGrid1.SelectedItem IsNot Nothing Then
                DataGrid1.ScrollIntoView(DataGrid1.SelectedItem)
            End If
        End If
    End Sub

    Public Sub PlaySelectedItems(DoLoop As Boolean)
        If DataGrid1.SelectedItems.Count > 0 Then
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(DataGrid1.SelectedItems, EvliTPQ)

            Dim seq As Sequence
            seq = CreateSequence(DataGrid1.SelectedItems, EvliTPQ)
            Player.PlaySequence(seq, DoLoop)
        End If
    End Sub

    Public Function GetSelectedItems() As IList
        Return DataGrid1.SelectedItems
    End Function

    Public Function GetListedItems() As ItemCollection
        Return DataGrid1.Items
    End Function

    Private Sub DataGrid1_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles DataGrid1.ContextMenuOpening
        ' enable / disable ContextMenu Items

        Dim dto As IDataObject = Clipboard.GetDataObject()
        If dto.GetDataPresent(GetType(EventListContainer)) = True Then
            ctxMi_Paste.IsEnabled = True
        Else
            ctxMi_Paste.IsEnabled = False
        End If

    End Sub

    'Private Sub ctxMi_PasteInsert_Click(sender As Object, e As RoutedEventArgs) Handles ctxMi_PasteInsert.Click
    '    Dim evlic As EventListContainer = GetEventListContainerFromClipboard()
    '    If evlic IsNot Nothing Then
    '        PasteEventList(evlic, GetSelectedItemTime, True)
    '    End If
    'End Sub

    Private Sub ctxMi_PasteOverwrite_Click(sender As Object, e As RoutedEventArgs) Handles ctxMi_PasteOverwrite.Click
        Dim evlic As EventListContainer = GetEventListContainerFromClipboard()
        If evlic IsNot Nothing Then
            PasteEventList(evlic, GetSelectedItemTime, True)
        End If
    End Sub

    Private Sub ctxMi_PasteAppend_Click(sender As Object, e As RoutedEventArgs) Handles ctxMi_PasteAppend.Click
        Dim evlic As EventListContainer = GetEventListContainerFromClipboard()
        If evlic IsNot Nothing Then
            PasteEventList(evlic, True)
        End If
    End Sub

    Private Function GetEventListContainerFromClipboard() As EventListContainer
        Dim dto As IDataObject = Clipboard.GetDataObject()
        Dim dobj = dto.GetData(GetType(EventListContainer))

        If dobj IsNot Nothing Then
            Dim evlic As EventListContainer = TryCast(dobj, EventListContainer)
            Return evlic
        Else
            Return Nothing
        End If
    End Function

    Private Function GetSelectedItemTime() As UInteger
        Dim InsertTime As UInteger
        Dim sel = DataGrid1.SelectedItem
        If sel IsNot Nothing Then
            Dim trev = TryCast(sel, TrackEventX)
            If trev IsNot Nothing Then
                InsertTime = trev.Time
            End If
        End If
        Return InsertTime
    End Function

    Private Sub DataGrid1_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DataGrid1.MouseDoubleClick
        OpenEditDialog()
    End Sub

    Private Sub OpenEditDialog()
        Dim sel = DataGrid1.SelectedItem
        If sel IsNot Nothing Then
            Dim trev = TryCast(sel, TrackEventX)
            If trev IsNot Nothing Then
                Dim win As New DlgEditEvent(trev, Me)
                win.Owner = Application.Current.MainWindow
                win.ShowDialog()

            End If
        End If
    End Sub

    Private Sub DeleteSelectedItems()
        Dim sel = DataGrid1.SelectedItems
        If sel IsNot Nothing Then
            Dim trevlist As New List(Of TrackEventX)
            Dim trev As TrackEventX
            For Each item In sel
                trev = TryCast(item, TrackEventX)
                If trev IsNot Nothing Then
                    trevlist.Add(trev)
                End If
            Next

            If trevlist.Count < 20 Then
                For Each trev In trevlist
                    TrackEvents.Remove(trev)
                Next
            Else
                TrackEvents.RemoveRange(trevlist)
            End If

            SetFocusToSelectedRow()
        End If
    End Sub

    Private Sub SetFocusToSelectedRow()
        If DataGrid1.SelectedItem IsNot Nothing Then
            DataGrid1.ScrollIntoView(DataGrid1.SelectedItem)
        End If

        DataGrid1.UpdateLayout()

        Dim selectedrow As Object

        If DataGrid1.SelectedIndex >= 0 Then
            selectedrow = CType(DataGrid1.ItemContainerGenerator.ContainerFromIndex(DataGrid1.SelectedIndex), DataGridRow)
            If selectedrow IsNot Nothing Then
                selectedrow.MoveFocus(New TraversalRequest(FocusNavigationDirection.Down))
            End If
        End If
    End Sub

    Private Sub ctxMi_CopyWithHeader_Click(sender As Object, e As RoutedEventArgs) Handles ctxMi_CopyWithHeader.Click
        CopySelectedItemsToClipboard(True)
    End Sub

    Private Sub CopySelectedItemsToClipboard(withHeader As Boolean)
        Dim sel = DataGrid1.SelectedItems                                           ' last added item is first        
        Dim sel2 As New List(Of TrackEventX)                ' need for full sorting (f.e. when all times are 0)

        If sel.Count > 0 Then
            For Each item In sel
                If item.GetType = GetType(TrackEventX) Then
                    sel2.Add(item)
                End If
            Next
            Dim sc As New TrackEventX
            sel2.Sort(sc)                                   ' full sorting (not only by Time)

            '--- To Clipboard ---
            Dim dao As New DataObject
            '--- First Data Format, for aplication who dont't know EventListContainer format ---
            dao.SetData("Text", TrackEventXListToText(sel2, withHeader))
            '--- second Data Format, for internal use --
            Dim evlic As EventListContainer
            evlic = CreateEventListContainer(sel, EvliTPQ)              ' includes full sorting
            dao.SetData(GetType(EventListContainer), evlic)             ' format as type
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
                sb.Append(ConvertTimeToString(trev.Time, EvliTPQ, DesiredTimeFormat)).Append(vbTab)
                sb.Append(trev.TrackNumber).Append(vbTab)
                sb.Append(trev.Channel).Append(vbTab)
                sb.Append(trev.TypeX.ToString).Append(vbTab)
                sb.Append(ConvertStatusByte(trev.Status, DesiredStatusFormat)).Append(vbTab)
                sb.Append(trev.Data1).Append(vbTab)
                sb.Append(trev.Data2).Append(vbTab)
                sb.Append(trev.Duration).Append(vbTab)
                sb.Append(trev.DataStr).Append(vbTab)
                sb.Append(vbCrLf)
            End If
        Next
        Return sb.ToString
    End Function

    Private Sub btnAddTrack_Click(sender As Object, e As RoutedEventArgs) Handles btnAddTrack.Click
        Dim win As New DlgAddTrack(Me)
        win.Owner = Application.Current.MainWindow
        win.ShowDialog()
        If win.DialogResult = True Then
            If win.RetAddTrackNameEvent = False Then
                AddTrack()
            Else
                AddTrack(win.RetTrackName, win.RetPosition)
            End If
        End If
    End Sub

    Private Sub btnRemoveTrack_Click(sender As Object, e As RoutedEventArgs) Handles btnRemoveTrack.Click
        Dim win As New DlgRemoveTrack(Me)
        win.Owner = Application.Current.MainWindow
        win.ShowDialog()
        If win.DialogResult = True Then
            RemoveTrack(win.RetRemoveList)
        End If

    End Sub


End Class