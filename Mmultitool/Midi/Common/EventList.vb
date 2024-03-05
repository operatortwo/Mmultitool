Public Module EventList



    '''' <summary>
    '''' Create an eventlist from midifile tracks.
    '''' </summary>
    '''' <param name="Tracks">Tracklist in midifile</param>
    '''' <param name="TPQsource">TicksPerQuarterNote in midifile</param>
    '''' <param name="Evlic">The resulting EventListContainer</param>
    '''' <returns></returns>
    ''' <summary>
    ''' Create an eventlist from midifile tracks.
    ''' </summary>
    ''' <param name="Tracks">Tracklist in midifile</param>
    ''' <param name="TPQsource">TicksPerQuarterNote in midifile</param>
    ''' <returns>Container including EventList. The result is returned in the CreateResult field.</returns>
    Public Function CreateEventListContainer(Tracks As List(Of TrackChunk), TPQsource As Integer) As EventListContainer
        Dim evlic As New EventListContainer

        If Tracks Is Nothing Then Return evlic          ' no data

        Dim ntrev As TrackEventX

        For Each track In Tracks
            For Each trev In track.EventList

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

                Evlic.EventList.Add(ntrev)
            Next
        Next

        Dim sc As New TrackEventX
        Evlic.EventList.Sort(sc)

        '---

        evlic.TPQ = TPQsource                   ' is needed to correctly interpret the Time property of TrackEventX
        evlic.CreateResult = True               ' mark as 'create was successful'
        Return evlic
    End Function


    Public Function CreateEventListContainer(Tracks As List(Of TrackChunk), Evlic As EventListContainer) As Boolean
        Return False
    End Function

    Public Function CreateEventListContainer(Track As TrackChunk, TPQ As Integer, Evli As EventListContainer) As Boolean
        Return False
    End Function

    Public Function CreateEventListContainer(Track As TrackChunk, Evli As EventListContainer) As Boolean
        Return False
    End Function




    Public Class EventListContainer
        Public EventList As New List(Of TrackEventX)
        Private _TPQ As Integer = 1
        Public Property TPQ As Integer
            Get
                Return _TPQ
            End Get
            Set(value As Integer)
                If value < 1 Then value = 1         ' prevent zero and minus value
                _TPQ = value
            End Set
        End Property
        Public NumberOfTracks As Integer            ' info (?)
        Public CreateResult As Boolean              ' Result of Create, TRUE = successful
    End Class
End Module
