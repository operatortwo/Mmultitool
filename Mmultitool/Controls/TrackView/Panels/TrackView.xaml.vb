Imports System.Windows.Controls.Primitives
Imports Mmultitool.Player

Public Class TrackView

    Public Property TrackList As New Tracklist

    'Public Const TicksToPixelFactor = 0.03125       ' ( 1 / 32 )
    'Public Const PixelToTicksFactor = 32            ' ( 1 / TicksToPixelFactor )

    Friend Const TicksToPixelFactor = 0.0625        ' ( 1 / 16 )
    Friend Const PixelToTicksFactor = 16            ' ( 1 / TicksToPixelFactor )

    Friend Const TicksPerBeat = 480               ' = TPQ 480
    Friend Const TPQ = 480


    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        MeasureStrip1.TrackView = Me
    End Sub

    ''' <summary>
    ''' Assign a Tracklist to the TrackView. For playing it's also necessary to call Player.SetTracklist.
    ''' </summary>
    ''' <param name="trklist"></param>
    Public Sub SetTracklist(trklist As Tracklist)
        TrackList = trklist
        TrackPanelStack.Children.Clear()
        lblNumberOfBeats.Content = ""
        If TrackList Is Nothing Then Exit Sub

        Dim beatlen As Single = trklist.MaxLength / Tracklist.TPQ
        lblNumberOfBeats.Content = beatlen & "  " & TimeTo_MBT(trklist.MaxLength - 1, TPQ)

        MasterHScroll.Value = 0
        Dim e As New ScrollEventArgs(ScrollEventType.First, 0)
        MasterHScroll_Scroll(Me, e)

        For Each track In TrackList.Tracks
            Dim trkp As New TrackPanel
            trkp.TrackView = Me
            trkp.TrackData = track
            trkp.Height = 250
            'trkp.IsExpanded = False            
            trkp.KeyPanel.SelectedView = KeyPanel.ViewType.RangeList
            'trkp.KeyPanel.SelectedView = KeyPanel.ViewType.FullRangeList
            TrackPanelStack.Children.Add(trkp)
        Next

        'Dim trkp0 As TrackPanel = TrackPanelStack.Children(0)
        ' trkp0.IsExpanded = False

        btnMuteUnMuteAll.IsChecked = False

        '--- Track 1 for development ---
        'If TrackList.Tracks.Count > 1 Then
        '    Dim trkp As New TrackPanel
        '    trkp.TrackData = TrackList.Tracks(1)
        '    TrackPanelStack.Children.Add(trkp)
        'End If


        '--- example
        For Each panel As TrackPanel In TrackPanelStack.Children

        Next

    End Sub

    Public Sub ScreenRefresh()
        'If TrackPanelStack.CheckAccess = False Then Exit Sub   ' if the calling thread has no access to this object

        Dim trk As Track

        For Each panel As TrackPanel In TrackPanelStack.Children
            trk = panel.TrackData
            If trk IsNot Nothing Then

                If trk.ChannelUpdate = True Then
                    panel.VoicePanel.nudMidiChannel.SetValueSilent(trk.ChannelValue)
                    trk.ChannelUpdate = False
                End If

                If trk.ProgramChangeUpdate = True Then
                    panel.VoicePanel.nudGmVoice.SetValueSilent(trk.ProgramChangeValue)

                    If trk.ChannelValue <> 9 Then
                        panel.VoicePanel.tbGmVoiceName.Text = Get_GM_VoiceName(trk.ProgramChangeValue)
                    Else
                        ' drum: maybe it's a known GS patch number
                        Dim str As String = Get_GS_DrumPatchName(trk.ProgramChangeValue)
                        If str = "" Then str = "Drum"
                        panel.VoicePanel.tbGmVoiceName.Text = str
                    End If

                    trk.ProgramChangeUpdate = False
                End If

                If trk.ChannelVolumeUpdate = True Then
                    panel.VoicePanel.ssldVolume.SetValueSilent(trk.ChannelVolumeValue)
                    trk.ChannelVolumeUpdate = False
                End If

                If trk.PanUpdate = True Then
                    panel.VoicePanel.ssldPan.SetValueSilent(trk.PanValue)
                    trk.PanUpdate = False
                End If

                '--- VU Meter

                If trk.VU_VelocityUpdate = True Then
                    panel.VoicePanel.VU_Meter.Value = trk.VU_Velocity
                    trk.VU_VelocityUpdate = False
                Else
                    'decrease value at each refresh
                    If panel.VoicePanel.VU_Meter.Value > 0 Then
                        If panel.VoicePanel.VU_Meter.Value < 20 Then
                            panel.VoicePanel.VU_Meter.Value = 0
                        Else
                            panel.VoicePanel.VU_Meter.Value = panel.VoicePanel.VU_Meter.Value * 0.8
                        End If
                    End If
                End If

            End If
        Next

        '--- Refresh Play position ---


        If LastPlayPosition <> Fix(TrackPlayerTime) Then

            If MeasureStrip1.MeasureStripAdornerLayer IsNot Nothing Then
                MeasureStrip1.MeasureStripAdornerLayer.Update()

                UpdateMeasureStripAdorner = False
            End If

            'If Sequencer.IsRunning Then
            '    TracksFooter.ScrollIntoView(Sequencer.SequencerTime)
            'End If

            LastPlayPosition = Fix(TrackPlayerTime)

        Else
            If UpdateMeasureStripAdorner = True Then
                If MeasureStrip1.MeasureStripAdornerLayer IsNot Nothing Then
                    MeasureStrip1.MeasureStripAdornerLayer.Update()
                    UpdateMeasureStripAdorner = False
                End If
            End If
        End If

    End Sub

    Friend LastPlayPosition As Double
    Friend UpdateMeasureStripAdorner As Boolean


    Public Sub UpdateVoiceColumnWidth(newwidth As Double)
        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.VoiceColumn.Width = New GridLength(newwidth)
        Next
    End Sub

    Public Sub UpdateTrackHeaderColumnWidth(newwidth As Double)
        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.TrackHeaderColumn.Width = New GridLength(newwidth)
        Next
    End Sub

    Public Sub UpdateKeysColumnColumnWidth(newwidth As Double)
        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.KeysColumn.Width = New GridLength(newwidth)
        Next
    End Sub

    Private Function TimeToPx(Time As Double) As Double
        Return Time * TicksToPixelFactor * sldScaleX.Value
    End Function

#Region "HScroll"
    Private Sub MasterHScroll_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles MasterHScroll.SizeChanged
        SetHScrollValues()
    End Sub

    Private Sub SetHScrollValues()
        If TrackList IsNot Nothing Then
            'TrackList.MaxLength
            Dim CanvasLength As Double
            Dim Maximum As Double
            Dim ActualWidth As Double

            CanvasLength = TrackList.MaxLength * TicksToPixelFactor * sldScaleX.Value
            ActualWidth = MasterHScroll.ActualWidth
            Maximum = CanvasLength - ActualWidth
            If Maximum < 0 Then Maximum = 0

            MasterHScroll.Maximum = CanvasLength - ActualWidth
            MasterHScroll.ViewportSize = ActualWidth

            MasterHScroll.SmallChange = Math.Round(0.02 * Maximum, 0)
            MasterHScroll.LargeChange = Math.Round(0.1 * Maximum, 0)

        End If
    End Sub

    Private Sub sldScaleX_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldScaleX.ValueChanged
        If MasterHScroll IsNot Nothing Then SetHScrollValues()
        MeasureStrip1.InvalidateVisual()

        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.NotePanel.NoteCanvas.InvalidateVisual()
        Next

        UpdateMeasureStripAdorner = True                ' Play Position
    End Sub

    Private Sub MasterHScroll_Scroll(sender As Object, e As Primitives.ScrollEventArgs) Handles MasterHScroll.Scroll
        MeasureStrip1.InvalidateVisual()

        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.NotePanel.NoteCanvas.InvalidateVisual()
        Next

        UpdateMeasureStripAdorner = True                ' Play Position
    End Sub

    Private Sub TrackPanelStack_RequestBringIntoView(sender As Object, e As RequestBringIntoViewEventArgs) Handles TrackPanelStack.RequestBringIntoView
        e.Handled = True            ' prevents scrollbar scrolling when MouseLeftBittonDown on GridSplitter
    End Sub

    ''' <summary>
    ''' Set all Tracks.IsExpanded to False
    ''' </summary>
    Public Sub CollapseAllTracks()
        ' checked = AllCollapsed
        If btnCollapseExpandAll.IsChecked = False Then
            btnCollapseExpandAll.IsChecked = True                   ' Toggle
        Else
            For Each el In TrackPanelStack.Children
                Dim tp As TrackPanel = TryCast(el, TrackPanel)
                If tp IsNot Nothing Then
                    tp.IsExpanded = False
                End If
            Next
        End If
    End Sub
    ''' <summary>
    ''' Set all Tracks.IsExpanded to True
    ''' </summary>
    Public Sub ExpandAllTracks()
        ' unchecked = AllExpanded
        If btnCollapseExpandAll.IsChecked = True Then
            btnCollapseExpandAll.IsChecked = False                  ' Toggle
        Else
            For Each el In TrackPanelStack.Children
                Dim tp As TrackPanel = TryCast(el, TrackPanel)
                If tp IsNot Nothing Then
                    tp.IsExpanded = True
                End If
            Next
        End If
    End Sub

    Private Sub btnCollapseExpandAll_Checked(sender As Object, e As RoutedEventArgs) Handles btnCollapseExpandAll.Checked
        ' Set all Tracks.IsExpanded to False
        For Each el In TrackPanelStack.Children
            Dim tp As TrackPanel = TryCast(el, TrackPanel)
            If tp IsNot Nothing Then
                tp.IsExpanded = False
            End If
        Next
    End Sub

    Private Sub btnCollapseExpandAll_Unchecked(sender As Object, e As RoutedEventArgs) Handles btnCollapseExpandAll.Unchecked
        ' Set all Tracks.IsExpanded to True
        For Each el In TrackPanelStack.Children
            Dim tp As TrackPanel = TryCast(el, TrackPanel)
            If tp IsNot Nothing Then
                tp.IsExpanded = True
            End If
        Next
    End Sub

#End Region



    Private Sub btnPreload_Click(sender As Object, e As RoutedEventArgs) Handles btnPreload.Click

        ' get channel of first MidiEvent
        For Each panel As TrackPanel In TrackPanelStack.Children
            For Each trev In panel.TrackData.EventList
                If trev.Type = EventType.MidiEvent Then
                    panel.VoicePanel.nudMidiChannel.Value = trev.Channel
                    Exit For
                End If
            Next
        Next

        ' get first program change
        For Each panel As TrackPanel In TrackPanelStack.Children
            For Each trev In panel.TrackData.EventList
                If trev.TypeX = EventTypeX.ProgramChange Then
                    panel.VoicePanel.nudGmVoice.Value = trev.Data1
                    Exit For
                End If
            Next
        Next

        ' get first channel volume
        For Each panel As TrackPanel In TrackPanelStack.Children
            For Each trev In panel.TrackData.EventList
                If trev.TypeX = EventTypeX.ControlChange Then
                    If trev.Data1 = 7 Then
                        panel.VoicePanel.ssldVolume.Value = trev.Data2
                        Exit For
                    End If
                End If
            Next
        Next


        ' get first pan
        For Each panel As TrackPanel In TrackPanelStack.Children
            For Each trev In panel.TrackData.EventList
                If trev.TypeX = EventTypeX.ControlChange Then
                    If trev.Data1 = 10 Then
                        panel.VoicePanel.ssldPan.Value = trev.Data2
                        Exit For
                    End If
                End If
            Next
        Next


    End Sub

    Private Sub btnMuteUnMuteAll_Checked(sender As Object, e As RoutedEventArgs) Handles btnMuteUnMuteAll.Checked
        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.VoicePanel.tgbtnMute.IsChecked = True
        Next
    End Sub

    Private Sub btnMuteUnMuteAll_Unchecked(sender As Object, e As RoutedEventArgs) Handles btnMuteUnMuteAll.Unchecked
        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.VoicePanel.tgbtnMute.IsChecked = False
        Next
    End Sub
End Class
