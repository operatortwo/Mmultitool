Imports System.Diagnostics.Eventing.Reader
Imports System.Text
Imports Mmultitool.EventListWriter
Partial Public Class DlgEditEvent

    Private ASCIIenc As New ASCIIEncoding

    Private Sub ShowEventInfo()

        If CurrentEvent IsNot Nothing Then

            lblTime.Content = ConvertTimeToString(CurrentEvent.Time, Evliw.EvliTPQ, Evliw.DesiredTimeFormat)

            MBT_Editor1.OriginalValue = CurrentEvent.Time


            lblTrk.Content = CurrentEvent.TrackNumber
            lblChn.Content = CurrentEvent.Channel
            lblTypeX.Content = CurrentEvent.TypeX
            lblStatus.Content = ConvertStatusByte(CurrentEvent.Status, Evliw.DesiredStatusFormat)
            lblData1.Content = CurrentEvent.Data1
            lblData2.Content = CurrentEvent.Data2
            lblDataStr.Content = CurrentEvent.DataStr
            lblEventType.Content = CurrentEvent.Type            ' MidiEvent or MetaEvent
            If CurrentEvent.Type = EventType.MetaEvent Then
                If CurrentEvent.Data1 > 9 Then
                    lblMetaEventType.Content = Hex(CurrentEvent.Data1) & " hex"
                Else
                    lblMetaEventType.Content = Hex(CurrentEvent.Data1)
                End If
            Else
                    lblMetaEventType.Content = ""
            End If

            If IsEventTypeText(CurrentEvent) = True Then
                If CurrentEvent.DataX IsNot Nothing Then

                    tbEditText.Text = ASCIIenc.GetString(CurrentEvent.DataX)
                Else
                    tbEditText.Text = ""
                End If
            Else
                tbEditText.Text = ""
            End If


            If CurrentEvent.DataX IsNot Nothing Then
                    lblDataXcount.Content = CurrentEvent.DataX.Count
                    lblDataX.Content = Bytes_to_hex_str(CurrentEvent.DataX)
                Else
                    lblDataXcount.Content = ""
                    lblDataX.Content = ""
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
            lblEventType.Content = ""
            lblMetaEventType.Content = ""

            lblDataXcount.Content = ""
            lblDataX.Content = ""
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


End Class
