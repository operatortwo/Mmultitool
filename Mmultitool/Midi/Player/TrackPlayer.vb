Imports System.ComponentModel

Partial Public Module Player

    Private Tracklist1 As Tracklist

#Region "Play"
    Private Sub PlayTrackList()                         ' called from Timer TickCallback
        If Tracklist1 Is Nothing Then Exit Sub
        Dim CurrenTime As Long = TrackPlayerTime
        For Each trk In Tracklist1.Tracks
            PlayThisTrack(trk, CurrenTime)
        Next
    End Sub

    Private Sub PlayThisTrack(trk As Track, CurrentTime As Long)
        If trk Is Nothing Then Exit Sub
        If trk.EventListPtr >= trk.EventList.Count Then Exit Sub

        Dim trev As TrackEventX
        Dim mute As Boolean = trk.Mute
        Dim transp As Short = trk.Transpose

        trev = trk.EventList(trk.EventListPtr)

        While trev.Time <= CurrentTime
            TrackPlayer.PlayEvent(CurrentTime, trev.Time, trev, mute, transp)
            Set_Track_UI_Notifications(trev, trk)
            trk.EventListPtr += 1
            If trk.EventListPtr >= trk.EventList.Count Then Exit While
            trev = trk.EventList(trk.EventListPtr)
        End While

    End Sub


#End Region

#Region "Set Tracklist"

    Public Sub PlayTracklist(tracklist As Tracklist)
        SetTracklist(tracklist)
        StartTrackPlayer()
    End Sub

    ''' <summary>
    ''' Assign a Tracklist to the Player. To to visualize it's also necessary to call Player.SetTracklist.
    ''' The player itself also works without Trackview.
    ''' </summary>
    ''' <param name="tracklist"></param>
    Public Sub SetTracklist(tracklist As Tracklist)
        If IsTrackPlayerRunning = True Then
            StopTrackPlayer()
        End If

        Tracklist1 = tracklist

        '--- reset ptr to start --

        If tracklist IsNot Nothing Then
            For Each trk In tracklist.Tracks
                trk.EventListPtr = 0
            Next
        End If

        For Each trk In tracklist.Tracks
            Reset_UI_Notification(trk)
        Next

        Set_TrackPlayerTime(0)
    End Sub

    Public Sub Set_TrackPlayerTime(newTime As Double)
        Dim isrunning As Boolean = IsTrackPlayerRunning
        If IsTrackPlayerRunning = True Then
            If newTime < TrackPlayerTime Then
                TrackPlayer.AllRunningNotesOff()
            End If
        End If

        If isrunning = True Then StopTrackPlayer()

        Move_TrackPlayerPosition(newTime)
        _TrackPlayerTime = newTime

        If isrunning = True Then StartTrackPlayer()

    End Sub

#End Region

#Region "Move Trackplayer position"

    Private Sub Move_TrackPlayerPosition(newTime As Double)
        If newTime = TrackPlayerTime Then Exit Sub

        If newTime = 0 Then
            MoveTrackPlayerPosition_ToZero()
        ElseIf newTime > TrackPlayerTime Then
            PB_Tracker.Clear()
            Ctrl_Tracker.Clear()
            MoveTrackPlayerPosition_Forward(newTime)
            PB_Tracker.Send_PB_Messages()
            Ctrl_Tracker.Send_Ctrl_Messages()
            'dbg_show_values()
        Else
            PB_Tracker.Clear()
            Ctrl_Tracker.Clear()
            MoveTrackPlayerPosition_Backward(newTime)
            PB_Tracker.Send_PB_Messages()
            Ctrl_Tracker.Send_Ctrl_Messages()
            'dbg_show_values()
        End If

    End Sub

    Private dbg_prog As Integer
    Private dbg_ctrl As Integer
    Private dbg_PB As Integer

    Private Sub dbg_show_values()
        Console.WriteLine("Prog: " & dbg_prog)
        Console.WriteLine("Ctrl: " & dbg_ctrl)
        Console.WriteLine("PB: " & dbg_PB)
        Console.WriteLine()
    End Sub


    Private Sub MoveTrackPlayerPosition_Forward(newTime As Double)
        ' assuming: EventListPtrs are correctly set to current item, we can go forward from here
        ' Set_TracklistEventPointers(newTime)
        dbg_prog = 0
        dbg_ctrl = 0
        dbg_PB = 0

        Dim trev As TrackEventX

        If Tracklist1 IsNot Nothing Then
            For Each trk In Tracklist1.Tracks

                Do Until trk.EventListPtr >= trk.EventList.Count        ' maximum until end of list
                    trev = trk.EventList(trk.EventListPtr)

                    If trev.Time >= newTime Then
                        Exit Do
                    End If

                    ProcessMoveStep(trev, trk)
                    ProcessMove_UI_Notifications(trev, trk)

                    trk.EventListPtr += 1
                Loop
            Next
        End If

    End Sub
    Private Sub MoveTrackPlayerPosition_Backward(newTime As Double)
        ' EventListPtrs are set to current position, we go to zero and processing the events from beginning
        MoveTrackPlayerPosition_ToZero()
        dbg_prog = 0
        dbg_ctrl = 0
        dbg_PB = 0

        If Tracklist1 IsNot Nothing Then
            For Each trk In Tracklist1.Tracks
                For Each trev In trk.EventList
                    If trev.Time >= newTime Then
                        Exit For
                    End If

                    ProcessMoveStep(trev, trk)
                    ProcessMove_UI_Notifications(trev, trk)

                    trk.EventListPtr += 1
                Next
            Next
        End If

    End Sub

    Private Sub MoveTrackPlayerPosition_ToZero()
        If Tracklist1 IsNot Nothing Then
            For Each trk In Tracklist1.Tracks
                trk.EventListPtr = 0
            Next
        End If
    End Sub

    Private Sub ProcessMoveStep(trev As TrackEventX, trk As Track)
        ' 8 no Note Off
        ' 9 no Note On
        ' A no PolyKeyPressure
        ' B yes ControlChange (only last 1, 7, 10, 64, Modulation, Channel Volume MSB, Pan MSB, Damper Pedal)
        ' C yes ProgramChange
        ' D no ChannelPressure
        ' E send only last Pitchbend (per Channel)

        If trev.Type = EventType.MidiEvent Then
            If trev.TypeX = EventTypeX.ControlChange Then
                If Ctrl_Tracker.TrySetValue(trev.Status, trev.Data1, trev.Data2) = False Then   ' cache
                    RaiseEvent MidiOutShortMsg(trev.Status, trev.Data1, trev.Data2)             ' send direct
                    dbg_ctrl += 1                                                               ' count sent
                End If
            ElseIf trev.TypeX = EventTypeX.ProgramChange Then
                RaiseEvent MidiOutShortMsg(trev.Status, trev.Data1, trev.Data2)
                dbg_prog += 1
            ElseIf trev.TypeX = EventTypeX.PitchBend Then
                PB_Tracker.SetValue(trev.Status, trev.Data1, trev.Data2)
            End If
        ElseIf trev.TypeX = EventTypeX.SetTempo Then
            ' send directly to player
            If trev.DataX IsNot Nothing Then                 ' check for invalid SetTempo
                If trev.DataX.Count >= 3 Then
                    Dim micros As Integer
                    micros = trev.DataX(0) * 65536 + trev.DataX(1) * 256 + trev.DataX(2)
                    TrackPlayerBPM = (CSng(Math.Round(60 * 1000 * 1000 / micros, 2)))
                    TrackPlayer.BPMUpdate = True            ' UI Notification
                End If
            End If
        End If

    End Sub

    Private Sub ProcessMove_UI_Notifications(trev As TrackEventX, trk As Track)
        Dim status_high As Byte = trev.Status And &HF0
        Dim channel As Byte = trev.Status And &HF

        If status_high >= &H90 AndAlso status_high <= &HE0 Then

            '--- channel ---

            If trk.ChannelValue <> channel Then
                trk.ChannelUpdate = True
                trk.ChannelValue = channel
            End If

            ' &hA0, &hB0, &hC0, &hD0, &hE0,     PolyKeyPress, CtrlChg, ProgChg, ChPress, PitchBend
            If status_high = MidiEventType.ProgramChange Then
                trk.ProgramChangeUpdate = True
                trk.ProgramChangeValue = trev.Data1
            ElseIf status_high = MidiEventType.ControlChange Then
                If trev.Data1 = 7 Then                                   ' Channel volume coarse (MSB)                    
                    trk.ChannelVolumeUpdate = True
                    trk.ChannelVolumeValue = trev.Data2
                ElseIf trev.Data1 = 10 Then                              ' Panorama MSB
                    trk.PanUpdate = True
                    trk.PanValue = trev.Data2
                End If
            End If
        End If

    End Sub

    Private Sub Reset_all_UI_Notifications()
        If Tracklist1 IsNot Nothing Then
            For Each trk In Tracklist1.Tracks
                Reset_UI_Notification(trk)
            Next
        End If
    End Sub

#Region "Tracking to reduce the amount of messages to be sent when moving Position"

    ' Some midi files can contain hundreds of pitchbend and controller events.
    ' This is not a problem when played in real time, but when trying to jump forward or backward,
    ' a large amount of data is generated and everything would have to be sent at once
    ' If you omit the messages, some values ​​for pitchbend and controller may be incorrect at the new position.
    ' The current solution is that the pitch-bend events are buffered per channel (while moving the EventPointers)
    ' and only the last values ​​are sent at the end of MovePosition. (Maximum 16 events)
    ' The procedure for controller events is similar, but since 128 controller numbers are possible,
    ' the effort would be disproportionate. Therefore, only a few frequently used controllers,
    ' such as modulation, channel volume, and pan, are tracked.
    ' Where necessary, this may mean reducing the number of events actually required
    ' from, for example, 1,000 to 50.


    ''' <summary>
    ''' Helps tracking PitchBend per Channel to reduce the amount of data to be sent
    ''' </summary>
    Private PB_Tracker As New PB_Tracking
    Private Class PB_Tracking
        Private ReadOnly PB_Value As List(Of TwoBytes) = New TwoBytes(0 To 15) {}.ToList         ' 2 data bytes
        Private ReadOnly PB_Value_updated As List(Of Boolean) = New Boolean(0 To 15) {}.ToList   ' 16 Channels

        ''' <summary>
        ''' There is a PitchBend event (2 Databytes), save temporarily
        ''' </summary>
        ''' <param name="status">Status byte including channel. The caller has checked that it is PB</param>
        ''' <param name="data1">Databyte 1</param>
        ''' <param name="data2">Databyte 2</param>
        Public Sub SetValue(status As Byte, data1 As Byte, data2 As Byte)
            Dim channel As Byte
            channel = status And &HF
            Dim tb As TwoBytes
            tb.Byte1 = data1
            tb.Byte2 = data2
            PB_Value(channel) = tb
            PB_Value_updated(channel) = True
        End Sub

        ''' <summary>
        ''' Send the Message with the latest (current) Value (for each updated channel)
        ''' </summary>
        Public Sub Send_PB_Messages()
            Dim tb As TwoBytes
            For i = 0 To 15
                If PB_Value_updated(i) = True Then
                    tb = PB_Value(i)
                    RaiseEvent MidiOutShortMsg(&HE0 Or i, tb.Byte1, tb.Byte2)
                    dbg_PB += 1
                End If
            Next
        End Sub

        ''' <summary>
        ''' Prepare PB_Tracker for a new round
        ''' </summary>
        Public Sub Clear()
            Dim tb As TwoBytes
            For i = 0 To 15
                PB_Value(i) = tb
                PB_Value_updated(i) = False
            Next
        End Sub

    End Class


    ' Ctrl 1	Modulation MSB
    ' Ctrl 6	Data Entry MSB
    ' Ctrl 7	Channel Volume MSB
    ' Ctrl 10	Pan MSB
    ' Ctrl 64   Damper Pedal

    ''' <summary>
    ''' Helps tracking some ControlChange events per Channel to reduce the amount of data to be sent
    ''' </summary>
    Private Ctrl_Tracker As New Ctrl_Tracking
    Private Class Ctrl_Tracking
        ' Ctrl 1	Modulation MSB
        Private ReadOnly Mod_Value As List(Of Byte) = New Byte(0 To 15) {}.ToList                   ' 1 data byte
        Private ReadOnly Mod_Value_updated As List(Of Boolean) = New Boolean(0 To 15) {}.ToList     ' 16 Channels
        ' Ctrl 6	Data Entry MSB
        Private ReadOnly Dat_Value As List(Of Byte) = New Byte(0 To 15) {}.ToList                   ' 1 data byte
        Private ReadOnly Dat_Value_updated As List(Of Boolean) = New Boolean(0 To 15) {}.ToList     ' 16 Channels
        ' Ctrl 7	Channel Volume MSB
        Private ReadOnly Vol_Value As List(Of Byte) = New Byte(0 To 15) {}.ToList                   ' 1 data byte
        Private ReadOnly Vol_Value_updated As List(Of Boolean) = New Boolean(0 To 15) {}.ToList     ' 16 Channels
        ' Ctrl 10	Pan MSB
        Private ReadOnly Pan_Value As List(Of Byte) = New Byte(0 To 15) {}.ToList                   ' 1 data byte
        Private ReadOnly Pan_Value_updated As List(Of Boolean) = New Boolean(0 To 15) {}.ToList     ' 16 Channels
        ' Ctrl 64   Damper Pedal (Sustain) keep original data, not interpreted as a switch
        Private ReadOnly Dmp_Value As List(Of Byte) = New Byte(0 To 15) {}.ToList                   ' 1 data byte
        Private ReadOnly Dmp_Value_updated As List(Of Boolean) = New Boolean(0 To 15) {}.ToList     ' 16 Channels

        ''' <summary>
        ''' There is a Control event, check if it is a tracked controller number, if so, 
        ''' load the data into the cache and return True.
        ''' </summary>
        ''' <param name="status">Status byte including channel. The caller has checked that it is ControlChange</param>
        ''' <param name="data1">Databyte 1, this is the Controller number</param>
        ''' <param name="data2">Databyte 2, this is the Controller value</param>
        ''' <returns>True if it was a Tracked Controller, False if was not handled and the caller must send it himself</returns>
        Public Function TrySetValue(status As Byte, data1 As Byte, data2 As Byte) As Boolean
            Dim channel As Byte = status And &HF

            Select Case data1
                Case CtrlNum.Modulation_MSB
                    Mod_Value(channel) = data2
                    Mod_Value_updated(channel) = True
                Case CtrlNum.DataEntry_MSB
                    Dat_Value(channel) = data2
                    Dat_Value_updated(channel) = True
                Case CtrlNum.ChannelVolume_MSB
                    Vol_Value(channel) = data2
                    Vol_Value_updated(channel) = True
                Case CtrlNum.Pan_MSB
                    Pan_Value(channel) = data2
                    Pan_Value_updated(channel) = True
                Case CtrlNum.DamperPedal
                    Dmp_Value(channel) = data2
                    Dmp_Value_updated(channel) = True
                Case Else
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>
        ''' Send the Message with the latest (current) Value (for each updated channel)
        ''' </summary>
        Public Sub Send_Ctrl_Messages()
            For i = 0 To 15
                If Mod_Value_updated(i) = True Then
                    RaiseEvent MidiOutShortMsg(&HB0 Or i, CtrlNum.Modulation_MSB, Mod_Value(i))
                    dbg_ctrl += 1
                End If

                If Dat_Value_updated(i) = True Then
                    RaiseEvent MidiOutShortMsg(&HB0 Or i, CtrlNum.DataEntry_MSB, Dat_Value(i))
                    dbg_ctrl += 1
                End If

                If Vol_Value_updated(i) = True Then
                    RaiseEvent MidiOutShortMsg(&HB0 Or i, CtrlNum.ChannelVolume_MSB, Vol_Value(i))
                    dbg_ctrl += 1
                End If

                If Pan_Value_updated(i) = True Then
                    RaiseEvent MidiOutShortMsg(&HB0 Or i, CtrlNum.Pan_MSB, Pan_Value(i))
                    dbg_ctrl += 1
                End If

                If Dmp_Value_updated(i) = True Then
                    RaiseEvent MidiOutShortMsg(&HB0 Or i, CtrlNum.DamperPedal, Dmp_Value(i))
                    dbg_ctrl += 1
                End If
            Next
        End Sub

        ''' <summary>
        ''' Prepare Ctrl_Tracker for a new round
        ''' </summary>
        Public Sub Clear()
            For i = 0 To 15
                Mod_Value(i) = 0
                Mod_Value_updated(i) = False

                Dat_Value(i) = 0
                Dat_Value_updated(i) = False

                Vol_Value(i) = 0
                Vol_Value_updated(i) = False

                Pan_Value(i) = 0
                Pan_Value_updated(i) = False

                Dmp_Value(i) = 0
                Dmp_Value_updated(i) = False
            Next
        End Sub

        Private Enum CtrlNum
            Modulation_MSB = 1
            DataEntry_MSB = 6
            ChannelVolume_MSB = 7
            Pan_MSB = 10
            DamperPedal = 64
        End Enum

    End Class

#End Region

#End Region

End Module
