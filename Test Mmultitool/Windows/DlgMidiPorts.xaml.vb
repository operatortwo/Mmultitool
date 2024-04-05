Public Class DlgMidiPorts

    Private MainWin As MainWindow
    Private outportlist As New List(Of String)

    Public Sub New(MainWindow As MainWindow)
        InitializeComponent()
        MainWin = MainWindow
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        For i = 1 To MainWin.MIO.MidiOutPorts.Count
            If MainWin.MIO.MidiOutPorts(i - 1).invalidPort = False Then          'list only valid devices
                outportlist.Add(MainWin.MIO.MidiOutPorts(i - 1).portName)
            End If
        Next

        cmbOutPort.Items.Add("")
        For Each item In outportlist
            cmbOutPort.Items.Add(item)
        Next
        cmbOutPort.SelectedItem = MainWin.MidiOut_Selected

        cmbAlternativeOutPort.Items.Add("")
        cmbAlternativeOutPort.Items.Add("first available")
        For Each item In outportlist
            cmbAlternativeOutPort.Items.Add(item)
        Next
        cmbAlternativeOutPort.SelectedItem = My.Settings.LastAlternativeMidiOut

        tbPreferredOutPort.Text = My.Settings.PreferredMidiOut
    End Sub
    Private Sub btnOutPort_asPreferred_Click(sender As Object, e As RoutedEventArgs) Handles btnOutPort_asPreferred.Click
        tbPreferredOutPort.Text = CStr(cmbOutPort.SelectedItem)
    End Sub


    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click

        If MainWin.MidiOut_Selected <> CStr(cmbOutPort.SelectedItem) Then
            MainWin.UpdateMidiOut(cmbOutPort.SelectedItem)
        End If

        My.Settings.PreferredMidiOut = tbPreferredOutPort.Text
        My.Settings.LastAlternativeMidiOut = cmbAlternativeOutPort.SelectedItem
        My.Settings.LastMidiOut = MainWin.MidiOut_Selected
        My.Settings.Save()

        Close()
    End Sub


End Class
