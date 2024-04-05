Partial Public Class EventLister

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

End Class
