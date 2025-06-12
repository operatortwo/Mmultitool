﻿Imports System.Text

Public Module EventList



    ''' <summary>
    ''' Create an eventlist from midifile tracks.
    ''' </summary>
    ''' <param name="Tracks">Tracklist in midifile</param>
    ''' <param name="TPQsource">TicksPerQuarterNote in midifile</param>    
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

                evlic.EventList.Add(ntrev)
            Next
        Next

        Dim sc As New TrackEventX
        evlic.EventList.Sort(sc)

        evlic.TPQ = TPQsource                   ' is needed to correctly interpret the Time property of TrackEventX

        Return evlic
    End Function

    ''' <summary>
    ''' includes sort
    ''' </summary>   
    Public Function CreateEventListContainer(items As IList, TPQsource As Integer) As EventListContainer
        Dim evlic As New EventListContainer
        evlic.TPQ = TPQsource
        If items Is Nothing Then Return evlic           ' no data
        If items.Count = 0 Then Return evlic            ' no data

        Dim selx As New List(Of TrackEventX)
        Dim tev As TrackEventX
        Dim tev2 As TrackEventX
        For Each item In items
            tev = TryCast(item, TrackEventX)
            If tev IsNot Nothing Then
                tev2 = tev.Copy(False)
                evlic.EventList.Add(tev2)
            End If
        Next
        Dim sc As New TrackEventX
        evlic.EventList.Sort(sc)

        Return evlic
    End Function

    <Serializable>                                  ' needed for copy and paste
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

        ''' <summary>
        ''' Make a deep copy af this EventListContainer
        ''' </summary>
        ''' <returns></returns>
        Public Function Copy() As EventListContainer
            Dim evlic2 As New EventListContainer
            evlic2.TPQ = TPQ
            Dim trev As TrackEventX

            For Each ev In EventList
                trev = ev.Copy
                evlic2.EventList.Add(trev)
            Next

            Return evlic2
        End Function

        ''' <summary>
        ''' Get number of tracks in EventList
        ''' </summary>
        ''' <returns></returns>
        Public Function GetNumberOfTracks() As Integer
            Dim trklist As New List(Of Byte)

            For Each ev In EventList
                If Not trklist.Contains(ev.TrackNumber) Then
                    trklist.Add(ev.TrackNumber)
                End If
            Next

            Return trklist.Count
        End Function

    End Class

    Public Function GetTrackName(TrackNumber As Byte, Eventlist As List(Of TrackEventX)) As String
        Dim retstr As String = Nothing
        If Eventlist Is Nothing Then Return ""

        For Each ev In Eventlist
            If ev.TrackNumber = TrackNumber Then
                If ev.TypeX = EventTypeX.SequenceOrTrackName Or ev.TypeX = EventTypeX.TextEvent Then
                    Dim ascii As Encoding = Encoding.ASCII
                    retstr = TrackNumber & "  " & ascii.GetChars(ev.DataX)
                    Exit For
                End If
            End If
        Next

        '--- If no SequenceOrTrackName or TextEvent try Program Change ---
        Dim progstr As String

        If retstr Is Nothing Then
            For Each ev In Eventlist
                If ev.TrackNumber = TrackNumber Then
                    If ev.TypeX = EventTypeX.ProgramChange Then
                        If ev.Channel <> 9 Then
                            progstr = Get_GM_VoiceName(ev.Data1)         ' it's a voice name
                            retstr = TrackNumber & "  " & progstr
                            Exit For
                        Else
                            Dim str As String = Get_GS_DrumPatchName(ev.Data1)      ' drum: maybe it's a known GS patch number
                            If str = "" Then str = "Drum"                                   ' else 'Drum' is used
                            progstr = str
                            retstr = TrackNumber & "  " & progstr
                            Exit For
                        End If
                    End If
                End If
            Next
        End If

        '--- if still nothing found set 'Drum' for channel 9 ---

        If retstr Is Nothing Then
            For Each ev In Eventlist
                If ev.TrackNumber = TrackNumber Then
                    If ev.Channel = 9 Then
                        retstr = TrackNumber & "  " & "Drum"
                        Exit For
                    End If
                End If
            Next
        End If

        '--- can be Track 0 ---
        If retstr Is Nothing Then
            retstr = TrackNumber
        End If


        Return retstr
    End Function

    ''' <summary>
    ''' For Single Track EventList
    ''' </summary>    
    Public Function GetTrackName(Eventlist As List(Of TrackEventX)) As String
        Dim retstr As String = ""
        If Eventlist Is Nothing Then Return retstr

        For Each ev In Eventlist
            If ev.TypeX = EventTypeX.SequenceOrTrackName Or ev.TypeX = EventTypeX.TextEvent Then
                Dim ascii As Encoding = Encoding.ASCII
                retstr = retstr & "  " & ascii.GetChars(ev.DataX)
            End If
        Next

        Return retstr
    End Function

End Module
