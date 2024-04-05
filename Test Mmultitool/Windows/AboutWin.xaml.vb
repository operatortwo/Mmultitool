Public Class AboutWin


    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Dim appver As String = My.Application.Info.Version.ToString
        Dim libname As Reflection.AssemblyName = GetType(Mmultitool.EventLister).Assembly.GetName
        Dim libver As String = libname.Version.ToString

        tbDescription.Text = My.Application.Info.Description
        tbAppVersion.Text = appver
        tbLibVersion.Text = libver
        tbCopyright.Text = My.Application.Info.Copyright
    End Sub
End Class
