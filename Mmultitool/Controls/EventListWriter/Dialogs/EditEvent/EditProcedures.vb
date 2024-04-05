Imports System.Text
Imports Mmultitool.EventListWriter
Partial Public Class DlgEditEvent

    Private ASCIIenc As New ASCIIEncoding

    Private Sub ShowEventInfo()

        ShowEventTypeAndNumber()

        If CurrentEvent IsNot Nothing Then

            lblTime.Content = ConvertTimeToString(CurrentEvent.Time, Evliw.EvliTPQ, Evliw.DesiredTimeFormat)

            MBT_Editor1.OriginalValue = CurrentEvent.Time

            lblTrk.Content = CurrentEvent.TrackNumber
            lblChn.Content = CurrentEvent.Channel
            lblTypeX.Content = CurrentEvent.TypeX
            lblStatus.Content = ConvertStatusByte(CurrentEvent.Status, Evliw.DesiredStatusFormat)
            lblData1.Content = CurrentEvent.Data1
            lblData2.Content = CurrentEvent.Data2
            lblDuration.Content = CurrentEvent.Duration
            lblDataStr.Content = CurrentEvent.DataStr
            lblEventType.Content = CurrentEvent.Type            ' MidiEvent or MetaEvent


            If CurrentEvent.DataX IsNot Nothing Then
                '--- Grid ---
                lblDataX.Content = "( " & CurrentEvent.DataX.Count & " )"
                '--- Dump ---
                lblDataXcount.Content = CurrentEvent.DataX.Count
                tbDataXDump.Text = Bytes_to_hex_str(CurrentEvent.DataX)
            Else
                '--- Grid ---
                lblDataX.Content = "( N )"
                '--- Dump ---
                lblDataXcount.Content = "(N)"
                tbDataXDump.Text = "(Nothing)"
            End If
        Else
            lblTime.Content = ""
            lblTrk.Content = ""
            lblChn.Content = ""
            lblTypeX.Content = ""
            lblStatus.Content = ""
            lblData1.Content = ""
            lblData2.Content = ""
            lblDataStr.Content = ""

            lblMetaEventType.Content = ""

            lblDataXcount.Content = ""
            tbDataXDump.Clear()
        End If


    End Sub

    ''' <summary>
    ''' Is it a Midi- or a Meta-Event, show MetaEvent number in Hex and Dec
    ''' </summary>
    Private Sub ShowEventTypeAndNumber()
        If CurrentEvent IsNot Nothing Then
            If CurrentEvent.Type = EventType.MetaEvent Then
                If CurrentEvent.Data1 > 9 Then
                    lblMetaEventType.Content = Hex(CurrentEvent.Data1) & " hex" & " / " & CurrentEvent.Data1 & " dec"
                Else
                    lblMetaEventType.Content = Hex(CurrentEvent.Data1)
                End If
            Else
                lblMetaEventType.Content = ""
            End If
        Else
            lblEventType.Content = ""
            lblMetaEventType.Content = ""
        End If
    End Sub



    '--- (Meta)Events with Data in DataX

    '--- Text events ---
    'TextEvent		        FF 01 len Text	
    'CopyrightNotice		FF 02 len Text	
    'SequenceOrTrackName	FF 03 len Text	
    'InstrumentName		    FF 04 len Text	
    'Lyric		            FF 05 len Text	
    'Marker		            FF 06 len Text	
    'CuePoint		        FF 07 len Text	
    'ProgramName		    FF 08 len Text	
    'DeviceName		        FF 09 len Text	

    'SequenceNumber     2 bytes MSB LSB
    'MidiChannelPrefix  1 byte cc
    'MidiPortPrefix     1 byte pp   (obsolete)
    'SetTempo           3 byte tt tt tt
    'SMPTEOffset        5 byte hr mn se fr ff
    'TimeSignature      4 byte nn dd cc bb
    'KeySignature       2 byte sf mi
    'SequencerSpecific  x bytes

    Private Function IsEventTypeText(ev As TrackEventX) As Boolean
        If ev Is Nothing Then Return False
        Select Case ev.TypeX
            Case EventTypeX.TextEvent
                Return True
            Case EventTypeX.CopyrightNotice
                Return True
            Case EventTypeX.SequenceOrTrackName
                Return True
            Case EventTypeX.InstrumentName
                Return True
            Case EventTypeX.Lyric
                Return True
            Case EventTypeX.Marker
                Return True
            Case EventTypeX.CuePoint
                Return True
            Case EventTypeX.ProgramName
                Return True
            Case EventTypeX.DeviceName
                Return True
            Case Else
                Return False
        End Select
    End Function




#Region "Edit and Compare"
    Private Sub TimeChanged()
        CompareTime()
        CheckIfChanged()
    End Sub
    Private Sub TrackChanged()
        CompareTrack()
        CheckIfChanged()
    End Sub
    Private Sub ChannelChanged()
        CompareChannel()
        CheckIfChanged()
    End Sub
    Private Sub TypexChanged()
        CompareTypeX()
        CheckIfChanged()
    End Sub
    Private Sub StatusChanged()
        CompareStatus()
        CheckIfChanged()

        EditedEvent.DataStr = GetData(EditedEvent)
        CompareDataStr()
    End Sub
    Private Sub Data1Changed()
        CompareData1()
        CheckIfChanged()

        EditedEvent.DataStr = GetData(EditedEvent)
        CompareDataStr()
    End Sub
    Private Sub Data2Changed()
        CompareData2()
        CheckIfChanged()

        EditedEvent.DataStr = GetData(EditedEvent)
        CompareDataStr()
    End Sub
    Private Sub DurationChanged()
        CompareDuration()
        CheckIfChanged()
    End Sub
    Private Sub DataXChanged()
        CompareDataX()
        CheckIfChanged()

        EditedEvent.DataStr = GetData(EditedEvent)
        CompareDataStr()
    End Sub
    Private Sub DatastrChanged()
        CompareDataStr()
        CheckIfChanged()
    End Sub

    ''' <summary>
    ''' Compare CurrentEvent with EditedEvent and set Backgound in row 2 of CompareGrid.
    ''' Also set HasChanges Property accordingly whether equal or not.
    ''' </summary>
    Private Sub EditCompare()
        CompareTime()
        CompareTrack()
        CompareChannel()
        CompareTypeX()
        CompareStatus()
        CompareData1()
        CompareData2()
        CompareDuration()
        CompareDataX()
        CompareDataStr()

        CheckIfChanged()
    End Sub

    Private Sub CompareTime()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.Time <> EditedEvent.Time Then
                lblTimeNew.Content = ConvertTimeToString(EditedEvent.Time, Evliw.EvliTPQ, Evliw.DesiredTimeFormat)
                lblTimeNew.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblTimeNew.Content = ""
        lblTimeNew.Background = UnChangedBgBrush
    End Sub
    Private Sub CompareTrack()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.TrackNumber <> EditedEvent.TrackNumber Then
                lblTrkNew.Content = EditedEvent.TrackNumber
                lblTrkNew.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblTrkNew.Content = ""
        lblTrkNew.Background = UnChangedBgBrush
    End Sub
    Private Sub CompareChannel()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.Channel <> EditedEvent.Channel Then
                lblChnNew.Content = EditedEvent.Channel
                lblChnNew.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblChnNew.Content = ""
        lblChnNew.Background = UnChangedBgBrush
    End Sub
    Private Sub CompareTypeX()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.TypeX <> EditedEvent.TypeX Then
                lblTypeXNew.Content = EditedEvent.TypeX
                lblTypeXNew.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblTypeXNew.Content = ""
        lblTypeXNew.Background = UnChangedBgBrush
    End Sub
    Private Sub CompareStatus()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.Status <> EditedEvent.Status Then
                lblStatusNew.Content = ConvertStatusByte(EditedEvent.Status, Evliw.DesiredStatusFormat)
                lblStatusNew.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblStatusNew.Content = ""
        lblStatusNew.Background = UnChangedBgBrush
    End Sub
    Private Sub CompareData1()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.Data1 <> EditedEvent.Data1 Then
                lblData1New.Content = EditedEvent.Data1
                lblData1New.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblData1New.Content = ""
        lblData1New.Background = UnChangedBgBrush
    End Sub
    Private Sub CompareData2()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.Data2 <> EditedEvent.Data2 Then
                lblData2New.Content = EditedEvent.Data2
                lblData2New.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblData2New.Content = ""
        lblData2New.Background = UnChangedBgBrush
    End Sub
    Private Sub CompareDuration()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.Duration <> EditedEvent.Duration Then
                lblDurationNew.Content = EditedEvent.Duration
                lblDurationNew.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblDurationNew.Content = ""
        lblDurationNew.Background = UnChangedBgBrush
    End Sub
    Private Sub CompareDataX()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If ByteArrayCompare(CurrentEvent.DataX, EditedEvent.DataX) = False Then
                If EditedEvent.DataX IsNot Nothing Then
                    lblDataXNew.Content = "( " & EditedEvent.DataX.Count & " )"
                    lblDataXNew.Background = ChangedBgBrush
                    Exit Sub
                Else
                    lblDataXNew.Content = "( N )"
                End If
            End If
        End If
        lblDataXNew.Content = ""
        lblDataXNew.Background = UnChangedBgBrush
    End Sub

    Private Sub CompareDataStr()
        If CurrentEvent IsNot Nothing AndAlso EditedEvent IsNot Nothing Then
            If CurrentEvent.DataStr <> EditedEvent.DataStr Then
                lblDataStrNew.Content = EditedEvent.DataStr
                lblDataStrNew.Background = ChangedBgBrush
                Exit Sub
            End If
        End If
        lblDataStrNew.Content = ""
        lblDataStrNew.Background = UnChangedBgBrush
    End Sub

    Private Function ByteArrayCompare(a1 As Byte(), a2 As Byte()) As Boolean
        If a1 Is Nothing Then
            If a2 Is Nothing Then
                Return True             ' a1 and a2 are Nothing
            Else
                Return False            ' a1 Is Nothing but a2 IsNot Nothing
            End If
        ElseIf a2 Is Nothing Then
            Return False                ' a1 IsNot Nothing but a2 Is Nothing
        End If
        ' at this point a1 and a2 are not Nothing

        If a1.Count <> a2.Count Then Return False

        For i = 1 To a1.Count
            If a1(i - 1) <> a2(i - 1) Then Return False
        Next

        Return True
    End Function
#End Region
    Private _HasChanges As Boolean

    Private Property HasChanges
        Get
            Return _HasChanges
        End Get
        Set(value)
            If _HasChanges <> value Then
                btnSaveChanges.IsEnabled = value
            End If
            _HasChanges = value
        End Set
    End Property


    ''' <summary>
    ''' Compare CurrentEvent with EditedEvent set Property HasChanges to True if they are not equal
    ''' </summary>    
    Private Sub CheckIfChanged()
        Dim e1 As TrackEventX = CurrentEvent
        Dim e2 As TrackEventX = EditedEvent

        If e2 Is Nothing Then
            HasChanges = False                                  ' no object to make a change
            Exit Sub
        End If

        If e1 Is Nothing Then                           ' only object 2 exists -> change is possible
                HasChanges = True
                Exit Sub
            End If

        If e1.Time <> e2.Time Then
            HasChanges = True
            Exit Sub
        End If

        If e1.TrackNumber <> e2.TrackNumber Then
            HasChanges = True
            Exit Sub
        End If

        If e1.Channel <> e2.Channel Then
            HasChanges = True
            Exit Sub
        End If

        If e1.TypeX <> e2.TypeX Then
            HasChanges = True
            Exit Sub
        End If

        If e1.Status <> e2.Status Then
            HasChanges = True
            Exit Sub
        End If

        If e1.Data1 <> e2.Data1 Then
            HasChanges = True
            Exit Sub
        End If

        If e1.Data2 <> e2.Data2 Then
            HasChanges = True
            Exit Sub
        End If

        If e1.Duration <> e2.Duration Then
            HasChanges = True
            Exit Sub
        End If

        If ByteArrayCompare(CurrentEvent.DataX, EditedEvent.DataX) = False Then
            HasChanges = True
            Exit Sub
        End If

        'If e1.DataStr <> e2.DataStr Then Return True           ' this is only an information member

        HasChanges = False
    End Sub


End Class
