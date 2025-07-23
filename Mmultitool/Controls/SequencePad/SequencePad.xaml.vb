Public Class SequencePad

    Public Sub ScreenRefresh()
        For Each element In MainPad.Children
            If element.GetType = GetType(SequenceBox) Then
                Dim sqb As SequenceBox
                sqb = TryCast(element, SequenceBox)
                If sqb IsNot Nothing Then
                    sqb.ScreeRefresh()
                End If
            End If
        Next
    End Sub

End Class
