Imports System.Collections.Specialized
Imports System.Reflection

Partial Public Class EventListWriter

    Public Class ModifiedObservableCollection(Of T)
        Inherits System.Collections.ObjectModel.ObservableCollection(Of T)

        Private IsBulkOperation As Boolean


        ''' <summary>
        ''' Start a bulk operation. Further calls of insert and remove will bypass Notifications. 
        ''' When the operation is completed, BulkOperationEnd must be called.
        ''' </summary>
        Public Sub BulkOperationStart()
            IsBulkOperation = True
        End Sub
        ''' <summary>
        ''' Terminates a bulk operation and send the required Notifications.
        ''' </summary>
        Public Sub BulkOperationEnd()
            IsBulkOperation = False
            NotifyCollectionChanged_Reset()
        End Sub

        Public Overloads Sub Insert(index As Integer, item As T)
            If IsBulkOperation = True Then
                BulkInsert(index, item)
            Else
                MyBase.Insert(index, item)
            End If
        End Sub
        Private Sub BulkInsert(index As Integer, item As T)
            Items.Insert(index, item)
        End Sub

        Public Overloads Sub Remove(item As T)
            If IsBulkOperation = True Then
                BulkRemove(item)
            Else
                MyBase.Remove(item)
            End If
        End Sub

        Private Sub BulkRemove(item As T)
            Items.Remove(item)
        End Sub

        ''' <summary>
        ''' Add multiple items at once. (Append)
        ''' </summary>
        ''' <param name="collection">Items to add</param>
        Public Sub AddRange(ByVal collection As IEnumerable(Of T))
            For Each i In collection
                Items.Add(i)
            Next
            NotifyCollectionChanged_Reset()
        End Sub
        ''' <summary>
        ''' Remove multiple items at once.
        ''' </summary>
        ''' <param name="collection">Items to remove</param>
        Public Sub RemoveRange(collection As IEnumerable(Of T))
            For Each i In collection
                Items.Remove(i)
            Next
            NotifyCollectionChanged_Reset()
        End Sub

        ''' <summary>
        ''' Raises PropertyChanged event for 'Count' and 'Item[]' and raises the CollectionChanged event 
        ''' specifying the Reset action.
        ''' </summary>
        Private Sub NotifyCollectionChanged_Reset()
            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs("Count"))
            OnPropertyChanged(New ComponentModel.PropertyChangedEventArgs("Item[]"))
            OnCollectionChanged(New NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))
        End Sub


    End Class


End Class
