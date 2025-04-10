Imports System.Windows.Controls.Primitives

Public Class TrackView

    Public Property TrackList As New Tracklist

    'Public Const TicksToPixelFactor = 0.03125       ' ( 1 / 32 )
    'Public Const PixelToTicksFactor = 32            ' ( 1 / TicksToPixelFactor )

    Friend Const TicksToPixelFactor = 0.0625        ' ( 1 / 16 )
    Friend Const PixelToTicksFactor = 16            ' ( 1 / TicksToPixelFactor )

    Friend Const TicksPerBeat = 480               ' = TPQ 480
    Friend Const TPQ = 480


    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        MeasureStrip.TrackView = Me
    End Sub

    ''' <summary>
    ''' Assign a Tracklist to the TrackView and convert the Event times to TPQ 480 based if necessary.
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

        Dim trkp0 As TrackPanel = TrackPanelStack.Children(0)
        trkp0.IsExpanded = False

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
        MeasureStrip.InvalidateVisual()

        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.NotePanel.NoteCanvas.InvalidateVisual()
        Next
    End Sub

    Private Sub MasterHScroll_Scroll(sender As Object, e As Primitives.ScrollEventArgs) Handles MasterHScroll.Scroll
        MeasureStrip.InvalidateVisual()

        For Each panel As TrackPanel In TrackPanelStack.Children
            panel.NotePanel.NoteCanvas.InvalidateVisual()
        Next
    End Sub

    Private Sub TrackPanelStack_RequestBringIntoView(sender As Object, e As RequestBringIntoViewEventArgs) Handles TrackPanelStack.RequestBringIntoView
        e.Handled = True            ' prevents scrollbar scrolling when MouseLeftBittonDown on GridSplitter
    End Sub

    Private Sub btnCollapseExpandAll_Checked(sender As Object, e As RoutedEventArgs) Handles btnCollapseExpandAll.Checked
        For Each el In TrackPanelStack.Children
            Dim tp As TrackPanel = TryCast(el, TrackPanel)
            If tp IsNot Nothing Then
                tp.IsExpanded = False
            End If
        Next
    End Sub

    Private Sub btnCollapseExpandAll_Unchecked(sender As Object, e As RoutedEventArgs) Handles btnCollapseExpandAll.Unchecked
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


End Class
