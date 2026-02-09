Imports System.IO
Imports System.IO.Compression
Imports DailyUserControls

Partial Public Class SequencePad

    Private SeqPadData As SequencePadData

#Region "Load"
    Private Sub LoadSeqPad()
        Dim ofd As New Microsoft.Win32.OpenFileDialog

        If IO.Directory.Exists(SequencePadDataDirectory) Then
            ofd.InitialDirectory = SequencePadDataDirectory
        End If

        ofd.Title = "Load SequencePad Data"
        ofd.Filter = "SequencePadData files|*.spd"

        If ofd.ShowDialog() = True Then
            LoadSequencePadData(ofd.FileName)
        End If

    End Sub

    Private Sub LoadSequencePadData(Fullname As String)

        Try
            Using zipToOpen As FileStream = New FileStream(Fullname, FileMode.Open, FileAccess.Read)
                Using archive As ZipArchive = New ZipArchive(zipToOpen, ZipArchiveMode.Read)
                    Dim SpdEntry As ZipArchiveEntry = archive.GetEntry("SeqPadData.xml")
                    If SpdEntry Is Nothing Then
                        Throw New Exception("No 'SeqPadData.xml' entry in archive")
                    End If
                    Dim myDeSerializer As New Xml.Serialization.XmlSerializer(GetType(SequencePadData))
                    Dim spdstream As Stream
                    spdstream = SpdEntry.Open
                    SeqPadData = CType(myDeSerializer.Deserialize(spdstream), SequencePadData)
                End Using
            End Using
        Catch ex As Exception
            MessageWindow.Show(Application.Current.MainWindow, ex.Message, "Load SPD", MessageIcon.Error)
        End Try

        Setup_SequencePad()

    End Sub

    Private Sub Setup_SequencePad()
        MainPad.Children.Clear()
        If SeqPadData Is Nothing Then Exit Sub

        PatternPlayerBPM = SeqPadData.BPM

        Try
            For Each vpdata In SeqPadData.VoicePanelList
                Dim panel As New SequenceVoicePanel
                vpdata.GetData(panel)
                MainPad.Children.Add(panel)
            Next

            '=== SequenceBox ===

            Dim Seqboxlist As New List(Of SequenceBox)

            '--- create tmp list ---

            For Each sbdata In SeqPadData.SeqBoxList
                Dim panel As New SequenceBox
                sbdata.GetData(panel)
                Seqboxlist.Add(panel)
            Next

            '--- replace ID with Object reference (for wiring)

            Dim ref As SequenceBox

            For Each sqb In Seqboxlist
                For Each nid In sqb.ID_Next
                    ref = GetSqb_fromList(nid, Seqboxlist)
                    If ref IsNot Nothing Then
                        sqb.NextSequenceBox.Add(ref)
                    End If
                Next

                For Each pid In sqb.ID_Previous
                    ref = GetSqb_fromList(pid, Seqboxlist)
                    If ref IsNot Nothing Then
                        sqb.PreviousSequenceBox.Add(ref)
                    End If
                Next

            Next

            '--- UI ---

            For Each seqbox In Seqboxlist
                seqbox.ID_Next.Clear()
                seqbox.ID_Previous.Clear()
                MainPad.Children.Add(seqbox)
            Next

            WiringGrid1.InvalidateVisual()                  ' update wiring

        Catch ex As Exception
            MessageWindow.Show(Application.Current.MainWindow, ex.Message, "Error setting up SequencePad", MessageIcon.Error)
        End Try

    End Sub

    Private Function GetSqb_fromList(ID As Integer, list As List(Of SequenceBox)) As SequenceBox
        Dim sqb As SequenceBox
        sqb = list.Find(Function(x) x.ID = ID)
        Return sqb
    End Function

#End Region

#Region "Save"
    Private Sub SaveSeqPad()

        '--- Create folder if not exists
        If Not IO.Directory.Exists(SequencePadDataDirectory) Then
            IO.Directory.CreateDirectory(SequencePadDataDirectory)
        End If

        Dim sfd As New Microsoft.Win32.SaveFileDialog

        sfd.InitialDirectory = SequencePadDataDirectory
        sfd.Filter = "XML files|*.xml"
        sfd.DefaultExt = ".xml"


        If sfd.ShowDialog() = True Then
            CollectSeqPadData()
            SaveSequencePadData(sfd.FileName)
        End If

    End Sub


    Private Sub CollectSeqPadData()
        SeqPadData = New SequencePadData

        SeqPadData.BPM = PatternPlayerBPM

        '--- SequenceVoicePanels ---
        Dim vp As SequenceVoicePanel
        For Each elem In MainPad.Children
            vp = TryCast(elem, SequenceVoicePanel)
            If vp IsNot Nothing Then
                Dim vpd As New SequenceVoicePanelData
                vpd.SetData(vp)
                SeqPadData.VoicePanelList.Add(vpd)
            End If
        Next

        '=== SequenceBoxes ===
        Dim sqb As SequenceBox
        Dim Seqboxlist As New List(Of SequenceBox)

        '--- create seqbox list
        Dim sqbID As Integer = 1

        For Each elem In MainPad.Children
            sqb = TryCast(elem, SequenceBox)
            If sqb IsNot Nothing Then
                sqb.ID = sqbID
                sqbID += 1
                sqb.ID_Next.Clear()
                sqb.ID_Previous.Clear()
                Seqboxlist.Add(sqb)
            End If
        Next

        '--- Wiring ---
        '--- replace next- and previous object references with ID

        For Each sqb In Seqboxlist
            For Each nxb In sqb.NextSequenceBox
                Dim id As Integer
                id = GetSqbID_fromList(nxb, Seqboxlist)
                sqb.ID_Next.Add(id)
            Next

            For Each pxb In sqb.PreviousSequenceBox
                Dim id As Integer
                id = GetSqbID_fromList(pxb, Seqboxlist)
                sqb.ID_Previous.Add(id)
            Next

        Next

        '--- set data ---

        For Each sqb In Seqboxlist
            Dim sqbd As New SequenceBoxData
            sqbd.SetData(sqb)
            SeqPadData.SeqBoxList.Add(sqbd)
        Next


        'For Each elem In MainPad.Children
        '    sqb = TryCast(elem, SequenceBox)
        '    If sqb IsNot Nothing Then
        '        Dim sqbd As New SequenceBoxData
        '        sqbd.SetData(sqb)
        '        SeqPadData.SeqBoxList.Add(sqbd)
        '    End If
        'Next


    End Sub

    Private Function GetSqbID_fromList(seqbox As SequenceBox, list As List(Of SequenceBox)) As Integer
        Dim ndx As Integer
        ndx = list.IndexOf(seqbox)
        If ndx <> -1 Then
            Return list(ndx).ID
        Else
            Return 0
        End If
    End Function


    Private Sub SaveSequencePadData(Fullname As String)

        '--- as XML for debug ---

        Try
            Using fs As New FileStream(Fullname, IO.FileMode.Create)        'create or truncate / write
                'Dim seria As New Xml.Serialization.XmlSerializer(sqb.GetType)
                'seria.Serialize(fs, sqb)

                Dim seria As New Xml.Serialization.XmlSerializer(SeqPadData.GetType)
                seria.Serialize(fs, SeqPadData)

            End Using
        Catch ex As Exception
            MessageWindow.Show(Application.Current.MainWindow, ex.Message, "Save as XML", MessageIcon.Error)
        End Try

        '--- spd SequencePadData ---

        Dim FullnameSpd As String = Path.ChangeExtension(Fullname, "spd")
        Try
            Using ZipToSave As New FileStream(FullnameSpd, FileMode.Create)
                Using archive As New ZipArchive(ZipToSave, ZipArchiveMode.Update)
                    Dim entry1 As ZipArchiveEntry = archive.CreateEntry("SeqPadData.xml")
                    Dim seria As New Xml.Serialization.XmlSerializer(SeqPadData.GetType)
                    seria.Serialize(entry1.Open, SeqPadData)
                End Using
            End Using
        Catch ex As Exception
            MessageWindow.Show(Application.Current.MainWindow, ex.Message, "Save as SPD", MessageIcon.Error)
        End Try

    End Sub

#End Region


#Region "Classes"

    Public Class SequencePadData
        Public BPM As Single = 120
        Public VoicePanelList As New List(Of SequenceVoicePanelData)
        Public SeqBoxList As New List(Of SequenceBoxData)
    End Class

    Public Class SequenceVoicePanelData
        Public MarginLeft As Double
        Public MarginTop As Double
        Public Volume As Byte
        Public Pan As Byte
        Public Channel As Byte
        Public GmVoice As Byte


        Public Sub SetData(panel As SequenceVoicePanel)
            If panel Is Nothing Then Exit Sub

            MarginLeft = panel.Margin.Left
            MarginTop = panel.Margin.Top
            Volume = panel.ssldVolume.Value
            Pan = panel.ssldPan.Value
            Channel = panel.nudMidiChannel.Value
            GmVoice = panel.nudGmVoice.Value
        End Sub

        Public Sub GetData(panel As SequenceVoicePanel)
            If panel Is Nothing Then Exit Sub

            Dim marg As New Thickness
            marg.Left = MarginLeft
            marg.Top = MarginTop
            panel.Margin = marg

            Volume = panel.ssldVolume.Value = Volume
            panel.ssldPan.Value = Pan
            panel.nudMidiChannel.Value = Channel
            panel.nudGmVoice.Value = GmVoice
        End Sub


    End Class

    Public Class SequenceBoxData
        Public MarginLeft As Double
        Public MarginTop As Double
        Public Channel As Byte
        Public ForceToChannel As Boolean
        Public DoLoop As Boolean
        Public LoopCount As Byte
        Public Sync As Boolean

        '--- for wiring ---

        Public ID As Integer
        Public ID_Next As New List(Of Integer)
        Public ID_Previous As New List(Of Integer)

        '--- pattern data ---
        Public Sequence As Pattern


        Public Sub SetData(panel As SequenceBox)
            If panel Is Nothing Then Exit Sub

            MarginLeft = Math.Round(panel.Margin.Left, 0)
            MarginTop = Math.Round(panel.Margin.Top, 0)
            Channel = panel.NudChannel.Value
            ForceToChannel = panel.CbForceToChannel.IsChecked
            DoLoop = panel.TgbtnLoop.IsChecked
            LoopCount = panel.NudLoopCount.Value
            Sync = panel.TgbtnSync.IsChecked

            Sequence = panel.Sequence1

            If Sequence IsNot Nothing Then
                Sequence.StartTime = 0
                Sequence.StartOffset = 0
                Sequence.EventListPtr = 0
            End If

            '--- for wiring ---

            ID = panel.ID
            ID_Next = panel.ID_Next
            ID_Previous = panel.ID_Previous

        End Sub

        Public Sub GetData(panel As SequenceBox)
            If panel Is Nothing Then Exit Sub

            Dim marg As New Thickness
            marg.Left = MarginLeft
            marg.Top = MarginTop
            panel.Margin = marg

            panel.NudChannel.Value = Channel
            panel.CbForceToChannel.IsChecked = ForceToChannel
            panel.TgbtnLoop.IsChecked = DoLoop
            panel.NudLoopCount.Value = LoopCount
            panel.TgbtnSync.IsChecked = Sync

            panel.Sequence1 = Sequence

            If Sequence IsNot Nothing Then
                panel.LblName.Content = Sequence.Name
                panel.lblBeatcount.Content = Math.Round(Sequence.Length / PlayerTPQ, 2)
                panel.ProgressRing1.MaximumValue = Sequence.Length
            End If

            '--- for wiring ---

            panel.ID = ID
            panel.ID_Next = ID_Next
            panel.ID_Previous = ID_Previous

        End Sub


    End Class

#End Region





End Class
