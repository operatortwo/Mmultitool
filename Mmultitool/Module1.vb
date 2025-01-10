Module Module1
    Public Function FindLogicalParent(base As FrameworkElement, targetType As Type) As Object

        Dim current As DependencyObject = base.Parent

        While current IsNot Nothing
            If current.[GetType]() = targetType Then
                Return current
            End If
            current = LogicalTreeHelper.GetParent(current)
        End While
        Return Nothing
    End Function
End Module
