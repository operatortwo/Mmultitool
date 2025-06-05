Public Module TracklistModule
    Public Class Tracklist
        Public Tracks As New List(Of Track)

        Public Const TPQ = 480

        Public MaxLength As UInteger
    End Class

    Public Class Track
        Public Property Name As String = ""
        Public Mute As Boolean
        Public Transpose As Short
        Public EventListPtr As Integer                      ' ptr to EventList, f.e. ptr to next event while playing
        Public EventList As New List(Of TrackEventX)

        ' for UI
        Public ChannelUpdate As Boolean                     ' True if Updated
        Public ProgramChangeUpdate As Boolean
        Public ChannelVolumeUpdate As Boolean
        Public PanUpdate As Boolean
        Public VU_VelocityUpdate As Boolean

        Public ChannelValue As Byte                         ' value if Update = True
        Public ProgramChangeValue As Byte
        Public ChannelVolumeValue As Byte
        Public PanValue As Byte
        Public VU_Velocity As Byte


        Public Class UsedNotesInfo
            Public ListOfUsedNotes As New List(Of Byte)
            Public NoteRangeStart As Byte = 0
            Public NoteRangeEnd As Byte = 127
        End Class

        ''' <summary>
        ''' Returns NoteRangeStart, NoteRangeEnd and a list of the note numbers used.
        ''' </summary>        
        ''' <returns></returns>
        Public Function GetUsedNotesInfo() As UsedNotesInfo
            Dim info As New UsedNotesInfo
            If EventList.Count = 0 Then Return info

            For Each ev In EventList

                If ev.TypeX = EventTypeX.NoteOnEvent Then

                    If Not info.ListOfUsedNotes.Contains(ev.Data1) Then
                        info.ListOfUsedNotes.Add(ev.Data1)
                    End If

                End If
            Next

            info.ListOfUsedNotes.Sort()
            info.NoteRangeStart = info.ListOfUsedNotes.FirstOrDefault
            info.NoteRangeEnd = info.ListOfUsedNotes.LastOrDefault

            Return info
        End Function


        Public Function FindNoteEvent(time As UInteger, NoteNumber As Byte) As TrackEventX
            If NoteNumber > 127 Then Return Nothing
            If EventList Is Nothing Then Return Nothing

            For Each trev In EventList
                If trev.TypeX = EventTypeX.NoteOnEvent Then
                    If trev.Data1 = NoteNumber Then
                        'If trev.Time > time + trev.Duration Then Return Nothing             ' early out
                        If time >= trev.Time AndAlso time <= trev.Time + trev.Duration Then
                            Return trev
                        End If
                    End If
                    End If
            Next

            Return Nothing
        End Function


    End Class



    ''' <summary>
    ''' Create a Tracklist from midifile tracks.
    ''' </summary>
    ''' <param name="MTracks">Tracklist in midifile</param>
    ''' <param name="TPQsource">TicksPerQuarterNote in midifile</param>
    ''' <returns></returns>
    Public Function CreateTracklist(Mtracks As List(Of TrackChunk), TPQsource As Integer) As Tracklist
        Dim trklist As New Tracklist

        If Mtracks Is Nothing Then Return trklist          ' no data

        Dim trknum As Integer
        Dim ntrk As Track
        Dim ntrev As TrackEventX

        For Each mtrack In Mtracks

            ntrk = New Track

            For Each trev In mtrack.EventList

                ntrev = New TrackEventX With {
                        .Time = trev.Time,
                        .Status = trev.Status,
                        .Data1 = trev.Data1,
                        .Data2 = trev.Data2,
                        .DataX = trev.DataX,
                        .Duration = trev.Duration,
                        .TrackNumber = trev.TrackNumber,
                        .Type = trev.Type,
                        .TypeX = GetEventTypeX(trev),
                        .Channel = trev.Status And &HF,
                        .DataStr = MDecode.GetData(trev)
}

                ntrk.EventList.Add(ntrev)
            Next
            trklist.Tracks.Add(ntrk)
            ntrk.Name = GetTrackName(trknum, ntrk.EventList)
            trknum += 1
        Next

        '--- convert time and duration to TPQ 480 based if necessary.

        If TPQsource <> Tracklist.TPQ Then
            For Each trk In trklist.Tracks
                For Each trev In trk.EventList
                    'Newtime = Time * DestinationTPQ / SourceTPQ                    
                    trev.Time = trev.Time * Tracklist.TPQ / TPQsource
                    If trev.Duration > 0 Then
                        trev.Duration = trev.Duration * Tracklist.TPQ / TPQsource      ' scale up duration
                    End If
                Next
            Next
        End If

        '--- get length (ticks)

        Dim ttrev As TrackEventX
        Dim lasttime As UInteger

        For Each trk In trklist.Tracks
            ttrev = trk.EventList.LastOrDefault
            If ttrev.Time > lasttime Then
                lasttime = ttrev.Time
            End If
        Next

        lasttime = MModule1.GetTimeOfNextBeat(lasttime, Tracklist.TPQ)          ' round to beat
        trklist.MaxLength = lasttime

        ' split multichannel track to singlechannel tracks

        Return trklist
    End Function


End Module
