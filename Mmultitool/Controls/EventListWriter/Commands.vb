Imports System.Data

Partial Public Class EventListWriter

    '--- need to overwrite the default Ctrl+C copy command, set tabbedText and EventListContainer ---
    ' for proper functionality the CopyCommand is attached at DataGrid.CommandBindings
    '  while other commands are attached at UserControl.CommandBindings

    Public Shared DataGridCopyCommand As New RoutedCommand("DataGridCopy", GetType(DataGrid),
                    New InputGestureCollection From {New KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C")})

    Private Sub DataGridCopyCommand_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub
    Private Sub DataGridCopyCommand_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        CopySelectedItemsToClipboard(False)
    End Sub

    '--- Paste Insert ---

    Public Shared DataGridPasteInsertCommand As New RoutedCommand("DataGridPasteInsert", GetType(DataGrid),
                    New InputGestureCollection From {New KeyGesture(Key.V, ModifierKeys.Control, "Ctrl+V")})

    Private Sub DataGridPasteInsertCommand_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        'Dim dto As IDataObject = Clipboard.GetDataObject()
        'Dim contains As Boolean = dto.GetDataPresent(GetType(EventListContainer))
        'e.CanExecute = contains
        e.CanExecute = True
    End Sub
    Private Sub DataGridPasteInsertCommand_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Dim evlic As EventListContainer = GetEventListContainerFromClipboard()
        If evlic IsNot Nothing Then
            Dim sel = DataGrid1.SelectedItem
            PasteEventList(evlic, GetSelectedItemTime, True)
            DataGrid1.SelectedItem = sel
            SetFocusToSelectedRow()
        End If
    End Sub

    '--- Insert New ---

    Public Shared DataGridInsertNewCommand As New RoutedCommand("DataGridInsertNew", GetType(DataGrid),
                    New InputGestureCollection From {New KeyGesture(Key.Insert, ModifierKeys.Control, "Ctrl+Insert")})

    Private Sub DataGridInsertNewCommand_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub
    Private Sub DataGridInsertNewCommand_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        Dim ndx As Integer
        Dim trev As TrackEventX
        Dim csel As DataGridCellInfo = DataGrid1.CurrentCell
        If csel.IsValid = True Then
            trev = TryCast(csel.Item, TrackEventX)
            If trev IsNot Nothing Then
                ndx = TrackEvents.IndexOf(trev)
                If ndx = -1 Then
                    ndx = 0
                End If
            End If
        End If
        'Dim ret = InsertNewEvent(ndx)                  ' old procedure, now a dialog is used

        Dim win As New DlgNewEvent(Me)
        win.Owner = Application.Current.MainWindow

#Disable Warning BC42104 ' Variable is used before it has been assigned a value
        win.StartReferenceItem = trev
#Enable Warning BC42104 ' Variable is used before it has been assigned a value

        win.ShowDialog()

        If win.DialogResult = True Then
            If win.LastInsertedEvent IsNot Nothing Then
                DataGrid1.SelectedItem = win.LastInsertedEvent
                SetFocusToSelectedRow()
            End If
        End If

    End Sub


    '--- Delete ---

    Public Shared DataGridDeleteCommand As New RoutedCommand("DataGridDelete", GetType(DataGrid),
                    New InputGestureCollection From {New KeyGesture(Key.Delete, ModifierKeys.Control, "Ctrl+Delete")})

    Private Sub DataGridDeleteCommand_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub
    Private Sub DataGridDeleteCommand_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        DeleteSelectedItems()
    End Sub

    '--- Play ---

    Public Shared DataGridPlayCommand As New RoutedCommand("DataGridPlay", GetType(DataGrid),
                    New InputGestureCollection From {New KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P")})

    Private Sub DataGridPlayCommand_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub
    Private Sub DataGridPlayCommand_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        PlaySelectedItems(False)
    End Sub

    '--- Play loop ---

    Public Shared DataGridPlayLoopCommand As New RoutedCommand("DataGridPlayLoop", GetType(DataGrid),
                    New InputGestureCollection From {New KeyGesture(Key.P, ModifierKeys.Shift + ModifierKeys.Control, "Shift+Ctrl+P")})

    Private Sub DataGridPlayLoopCommand_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = True
    End Sub
    Private Sub DataGridPlayLoopCommand_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        PlaySelectedItems(True)
    End Sub

    '--- Edit Events ---

    Public Shared DataGridEditCommand As New RoutedCommand("DataGridEdit", GetType(DataGrid),
                   New InputGestureCollection From {New KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E")})

    Private Sub DataGridEditCommand_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
        If TrackEvents.Count > 0 Then
            e.CanExecute = True
        Else
            e.CanExecute = False
        End If
    End Sub
    Private Sub DataGridEditCommand_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        OpenEditDialog()
    End Sub


End Class
