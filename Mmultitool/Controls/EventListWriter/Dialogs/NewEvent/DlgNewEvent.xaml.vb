Imports System.Collections.ObjectModel
Imports System.Text



Public Class DlgNewEvent
        Private Evliw As EventListWriter
        ''' <summary>
        ''' When starting, this is the selected item which is used to make a suggestion for 
        ''' time, track and channel of the new item. Nothing if no item is selected.
        ''' </summary>
        Public StartReferenceItem As TrackEventX
        ''' <summary>
        ''' The last inserted TrackEventX which can be used for SetFocusToSelectedRow
        ''' </summary>
        Public LastInsertedEvent As TrackEventX
        ''' <summary>
        ''' Dialog to insert one ore more new TrackEvnetX. When DialogResult is set to TRUE, LastInsertedEvent 
        ''' can be used to cjange the selected row in the DataGrid.
        ''' </summary>
        ''' <param name="instance">Instance of the calling EventListWriter</param>    
        Public Sub New(instance As EventListWriter)
            InitializeComponent()                   ' required for the Designer
            Evliw = instance
        End Sub

        Private CurrentEvent As TrackEventX



    Public Property CountryCollection As New ObservableCollection(Of Country)

        Public Class Country
            Public Property Name As String
            Public Property Code As String
        End Class

        Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            MBT_Editor1.TPQ = Evliw.EvliTPQ
            MBT_Editor2_Step.TPQ = Evliw.EvliTPQ
            MBT_InputBox1.TPQ = Evliw.EvliTPQ
            lblTPQ.Content = Evliw.EvliTPQ

            If Evliw.DesiredTimeFormat = EventListWriter.TimeFormat.MBT_1_based Then
                MBT_Editor1.IsMBT_Base1 = True
                MBT_Editor2_Step.IsMBT_Base1 = True
                MBT_InputBox1.IsMBT_Base1 = True
                lblMBT_Base.Content = "1"
            Else
                lblMBT_Base.Content = "0"
            End If

            CurrentEvent = New TrackEventX With {.TypeX = EventTypeX.Unkown}

            If StartReferenceItem IsNot Nothing Then
                CurrentEvent.Time = StartReferenceItem.Time
                CurrentEvent.Channel = StartReferenceItem.Channel
                CurrentEvent.TrackNumber = StartReferenceItem.TrackNumber
            End If

            cmbEventTypeX.ItemsSource = [Enum].GetValues(GetType(EventTypeX))
            cmbEventTypeX.SelectedIndex = 2

            cmbEventTypeX.Focus()
        End Sub

        Private evtinfo As EventTypeX_Information

        Private Sub cmbEventTypeX_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbEventTypeX.SelectionChanged
            Dim sel = cmbEventTypeX.SelectedItem
            If sel IsNot Nothing Then
                evtinfo = GetEventTypeX_Info(sel)
                InitializeEvent(sel)
                SetControlDefaultValues()
                ShowAvailableControls()
                UpdateData()
            End If
        End Sub

        Private buffer() As Byte = {}
        Private Sub InitializeEvent(tp As EventTypeX)
            CurrentEvent.TypeX = tp
            CurrentEvent.Type = GetEventType(CurrentEvent.TypeX)

            CurrentEvent.Data2 = 0
            CurrentEvent.Duration = 0

            If evtinfo.IsMidiEvent = True Then
                CurrentEvent.Status = CurrentEvent.TypeX And &HFF
                CurrentEvent.DataX = Nothing
            Else
                CurrentEvent.Status = 0
                CurrentEvent.DataX = buffer
                ' Data1 contains Meta and SysEx type number
                Dim val As Integer = CurrentEvent.TypeX
                val = val And &HFF
                CurrentEvent.Data1 = val
                tbEditText.Text = ""
            End If


        End Sub

        Private Sub SetControlDefaultValues()
            If evtinfo Is Nothing Then Exit Sub
            If evtinfo.enumtype = EventTypeX.NoteOffEvent Then
                nudMidiData2.Value = &H40                           ' default value for NoteOff velocity
                nudDuration.Value = 0
            End If
        End Sub

#Region "Visibility"
        ''' <summary>
        ''' depending on the type of the new Event
        ''' </summary>
        Private Sub ShowAvailableControls()
            tblEventInfo.Text = evtinfo.Info


            If evtinfo.IsMidiEvent Then
                GroupBoxMidiEvent.Visibility = Visibility.Visible
                ShowAvailableControls_Midi()
            Else
                GroupBoxMidiEvent.Visibility = Visibility.Hidden
            End If


            If evtinfo.IsTextEvent Then
                tbEditText.Visibility = Visibility.Visible
            Else
                tbEditText.Visibility = Visibility.Hidden
            End If

        End Sub

        Private Sub ShowAvailableControls_Midi()

            If evtinfo.enumtype = EventTypeX.NoteOnEvent OrElse evtinfo.enumtype = EventTypeX.NoteOffEvent Then
                nudDuration.Visibility = Visibility.Visible
            Else
                nudDuration.Visibility = Visibility.Hidden
            End If

            If evtinfo.DataBytes = 2 Then
                nudMidiData2.Visibility = Visibility.Visible
            Else
                nudMidiData2.Visibility = Visibility.Hidden
            End If



        End Sub


#End Region



        Private Sub MBT_InputBox1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles MBT_InputBox1.ValueChanged
            MBT_Editor1.ChangeCurrentValue(e.NewValue)
        End Sub

        Private Sub btnShowResult_Click(sender As Object, e As RoutedEventArgs) Handles btnShowResult.Click
            Dim ev = CurrentEvent




            tblResult.Text = GetData(CurrentEvent)
        End Sub



#Region "Control Changed"

        Private Sub nudMidiData1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudMidiData1.ValueChanged
            UpdateData()
        End Sub

        Private Sub nudMidiData2_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudMidiData2.ValueChanged
            UpdateData()
        End Sub

        Private Sub nudDuration_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles nudDuration.ValueChanged
            UpdateData()
        End Sub

        Private Sub MBT_Editor1_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles MBT_Editor1.ValueChanged
            ' time is updated before inserting
        End Sub

        Private Sub UpdateData()
            If CurrentEvent IsNot Nothing Then
                If evtinfo.IsMidiEvent Then
                    CurrentEvent.Data1 = nudMidiData1.Value
                    CurrentEvent.Data2 = nudMidiData2.Value
                    If evtinfo.enumtype = EventTypeX.NoteOnEvent Then
                        CurrentEvent.Duration = nudDuration.Value
                    ElseIf evtinfo.enumtype = EventTypeX.NoteOffEvent Then
                        CurrentEvent.Duration = 0
                    End If
                End If
                tblResult.Text = GetData(CurrentEvent)
            End If
        End Sub




#End Region

        Private InsertCounter As Integer
        Private ReadOnly ASCIIenc As New ASCIIEncoding

        Private Sub btnInsertEvent_Click(sender As Object, e As RoutedEventArgs) Handles btnInsertEvent.Click
            CurrentEvent.Time = MBT_Editor1.NewValue


            If evtinfo.IsTextEvent Then
                CurrentEvent.DataX = ASCIIenc.GetBytes(tbEditText.Text)
            End If

            CurrentEvent.DataStr = GetData(CurrentEvent)

            If Evliw.InsertEvent(CurrentEvent.Copy(True)) = True Then
                InsertCounter += 1
                lblInsertCount.Content = InsertCounter
                If cbIncreaseByStep.IsChecked = True Then
                    MBT_Editor1.ChangeCurrentValue(MBT_Editor1.NewValue + MBT_Editor2_Step.NewValue)
                End If
            Else
                Beep()      ' failed to insert
            End If



        End Sub

    Private Sub NoteDurationHelper_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of UInteger)) Handles cmbNoteDuration.ValueChanged
        lblDurationValue.Content = e.NewValue
    End Sub

    Private Sub btnSetDuration_Click(sender As Object, e As RoutedEventArgs) Handles btnSetDuration.Click
        nudDuration.Value = cmbNoteDuration.Value
    End Sub
End Class

