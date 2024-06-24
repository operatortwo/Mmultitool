Imports System.Diagnostics.Eventing.Reader

Partial Public Class EventListWriter

    Private Function InsertNewEvent(index As Integer) As TrackEventX
        Dim trev As New TrackEventX With {.TypeX = EventTypeX.Unkown}
        Dim tri As TrackEventX = GetEventAt(index)

        If tri IsNot Nothing Then
            trev.Time = tri.Time
            trev.Channel = tri.Channel
            trev.TrackNumber = tri.TrackNumber
        Else
            UpdateChannelList(trev)
            UpdateTrackList(trev)
        End If

        trev.DataStr = GetData(trev)
        index += 1
        If index < 0 Then index = 0
        If index > TrackEvents.Count Then index = TrackEvents.Count
        TrackEvents.Insert(index, trev)
        Return trev
    End Function

    Private Function GetEventAt(index As Integer) As TrackEventX
        If index < 0 Then Return Nothing
        If index >= TrackEvents.Count Then Return Nothing
        Return TrackEvents(index)
    End Function

    ''' <summary>
    ''' Paste EventList at the end (Append)
    ''' </summary>
    ''' <param name="EvlicToPaste"></param>
    ''' <param name="Insert">True: insert events. False: overwrite events</param>
    Private Sub PasteEventList(EvlicToPaste As EventListContainer, Insert As Boolean)
        PasteEventList(EvlicToPaste, 0, Insert, True)
    End Sub

    ''' <summary>
    ''' Paste EventList at the specified position
    ''' </summary>
    ''' <param name="EvlicToPaste"></param>
    ''' <param name="InsertTime">Based on datagid TPQ</param>
    ''' <param name="Insert">True: insert events. False: overwrite events</param>
    Private Sub PasteEventList(EvlicToPaste As EventListContainer, InsertTime As UInteger, Insert As Boolean)
        PasteEventList(EvlicToPaste, InsertTime, Insert, False)
    End Sub

    ''' <summary>
    ''' Paste EventList
    ''' </summary>
    ''' <param name="EvlicToPaste"></param>
    ''' <param name="InsertTime">Based on datagid TPQ. Ignored when Append = TRUE</param>
    ''' <param name="Insert">True: insert events. False: overwrite events</param>
    ''' <param name="Append">append events at the end</param>
    Private Sub PasteEventList(EvlicToPaste As EventListContainer, InsertTime As UInteger, Insert As Boolean, Append As Boolean)
        If EvlicToPaste Is Nothing Then Exit Sub
        If EvlicToPaste.EventList.Count = 0 Then Exit Sub

        Dim evlic As EventListContainer = EvlicToPaste.Copy         ' need an independent object

        '--- adjust times and durations to destination TPQ if necessary
        If evlic.TPQ <> EvliTPQ Then
            For Each ev In evlic.EventList
                ev.Time = ConvertTime(ev.Time, evlic.TPQ, EvliTPQ)
                ev.Duration = ConvertTime(ev.Duration, evlic.TPQ, EvliTPQ)
            Next
            evlic.TPQ = EvliTPQ
        End If

        '--- extend TrackList if necessary
        UpdateTrackList(evlic)

        '--- extend ChannelList if necessary
        UpdateChannelList(evlic)

        '--- insert data string
        For Each ev In evlic.EventList
            ev.DataStr = GetData(ev)
        Next

        '--- time of insert-list to relative time
        Dim TimeOffset As UInteger = evlic.EventList(0).Time
        'If Align = True Then
        TimeOffset = MModule1.GetTimeOfPreviousBeat(TimeOffset, evlic.TPQ)      ' keep relative time within beat
        ' f.e. if the first event is at 3:1:24 it will be shifted to 0:0:24
        'End If
        For Each tev In evlic.EventList
            tev.Time -= TimeOffset
        Next

        '---
        Dim listLength As UInteger = evlic.EventList.Last.Time
        Dim MoveOffset As UInteger = MModule1.GetTimeOfNextBeat(listLength, evlic.TPQ)

        '--- shift time of insert-list to desired position
        Dim InsertTime2 As UInteger
        If Append = True Then
            If TrackEvents.Count > 0 Then
                InsertTime2 = TrackEvents.Last.Time
            End If
            For Each tev In evlic.EventList
                tev.Time += InsertTime2
            Next
        Else
            InsertTime2 = InsertTime
            For Each tev In evlic.EventList
                tev.Time += InsertTime2
            Next
        End If

        '--- append
        If Append = True Then
            TrackEvents.AddRange(evlic.EventList)
            Exit Sub
        End If

        '--- insert
        Dim ndx As Integer

        '--- find start index
        For i = 1 To TrackEvents.Count
            If TrackEvents(i - 1).Time >= InsertTime2 Then Exit For
            ndx += 1
        Next

        '--- move following events

        If Insert = False Then
            Dim offs As Integer

            While ndx + offs < TrackEvents.Count
                TrackEvents(ndx + offs).Time += MoveOffset
                offs += 1
            End While

        End If

        '---

        TrackEvents.BulkOperationStart()

        For Each ev In evlic.EventList

            While ndx < TrackEvents.Count
                If ev.Compare(ev, TrackEvents(ndx)) >= -1 Then
                    Exit While                                      ' equal or greater
                Else
                    ndx += 1
                End If
            End While

            TrackEvents.Insert(ndx, ev)     ' if ndx = count then item is added to the end                

            If ndx < TrackEvents.Count Then
                ndx += 1
            End If
        Next
        TrackEvents.BulkOperationEnd()

    End Sub

    ''' <summary>
    ''' Create a temporary channel list of the specified object and extend the channel list of 
    ''' this control if necessary
    ''' </summary>    
    Private Sub UpdateTrackList(evlic As EventListContainer)
        If evlic Is Nothing Then Exit Sub
        If evlic.EventList.Count = 0 Then Exit Sub

        Dim trknumlist2 As New List(Of Byte)
        Dim trk As Byte

        '--- create tmp list
        For Each ev In evlic.EventList
            trk = ev.TrackNumber
            If trknumlist2.Contains(trk) = False Then
                trknumlist2.Add(trk)
            End If
        Next

        Dim ntrk As NamedTrack

        For Each trnum In trknumlist2
            If TrackList.Exists(Function(x) x.TrackNumber = trnum) = False Then
                ntrk = New NamedTrack
                ntrk.TrackNumber = trnum
                ntrk.TrackName = GetTrackName(trnum, evlic.EventList)
                TrackList.Add(ntrk)
            End If
        Next

        TrackList.Sort(New NamedTrack)
        cbflistTrack.ItemListUpdate()
    End Sub

    ''' <summary>
    ''' Create a temporary channel list of the specified object and extend the channel list of 
    ''' this control if necessary
    ''' </summary>    
    Private Sub UpdateChannelList(evlic As EventListContainer)
        If evlic Is Nothing Then Exit Sub
        If evlic.EventList.Count = 0 Then Exit Sub

        Dim ChannelList2 As New List(Of Byte)
        Dim chan As Byte

        '--- create tmp list
        For Each ev In evlic.EventList
            chan = ev.Channel
            If ChannelList2.Contains(chan) = False Then
                ChannelList2.Add(chan)
            End If
        Next

        For Each chan In ChannelList2
            If ChannelList.Contains(chan) = False Then
                ChannelList.Add(chan)
            End If
        Next

        ChannelList.Sort()
        cbflistChannel.ItemListUpdate()
    End Sub

    ''' <summary>
    ''' When adding a new TrackEventX, it checks if TrackEventX.Channel is already contained in the ChannelList.
    ''' Otherwise the ChannelList will be expanded.
    ''' </summary>    
    Private Sub UpdateTrackList(trev As TrackEventX)
        If trev Is Nothing Then Exit Sub
        Dim trnum As Byte = trev.TrackNumber
        If TrackList.Exists(Function(x) x.TrackNumber = trnum) = False Then
            Dim ntrk = New NamedTrack
            ntrk.TrackNumber = trnum
            TrackList.Add(ntrk)
            TrackList.Sort(New NamedTrack)
            cbflistTrack.ItemListUpdate()
        End If

    End Sub
    ''' <summary>
    ''' When adding a new TrackEventX, it checks if TrackEventX.TrackNumber is already contained in the TrackList.
    ''' Otherwise the ChannelList will be expanded.
    ''' </summary>    
    Private Sub UpdateChannelList(trev As TrackEventX)
        If trev Is Nothing Then Exit Sub
        Dim chan As Byte = trev.Channel
        If ChannelList.Contains(chan) = False Then
            ChannelList.Add(chan)
            ChannelList.Sort()
            cbflistChannel.ItemListUpdate()
        End If
    End Sub

End Class
