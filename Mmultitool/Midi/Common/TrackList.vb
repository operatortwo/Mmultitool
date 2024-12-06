Public Module TracklistModule
    Public Class Tracklist
        Public Tracks As New List(Of Track)

    End Class

    Public Class Track
        Public Name As String = ""
        Public Channel As Byte
        Public EventList As New List(Of TrackEventX)
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


        Next

        ' split multichannel track to singlechannel tracks


        Return trklist
    End Function


End Module
