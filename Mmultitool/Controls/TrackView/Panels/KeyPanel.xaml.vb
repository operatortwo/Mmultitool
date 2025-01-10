Public Class KeyPanel

    Friend TrackPanel As TrackPanel                     ' Parent, set in TrackPanel _Loaded

    Public SelectedView As ViewType = ViewType.FullRangeList
    Public IsDrumView As Boolean

    Public FullRangeFirstNote As Integer = 21
    Public FullRangeNumberOfNotes As Integer = 88

    Public RangeFirstNote As Integer = 36
    Public RangeNumberOfNotes As Integer = 25

    Public UsedNotes As New List(Of Byte)               ' base for RandomList

    Public FullRangeList As New List(Of KeyItem)
    Public RangeList As New List(Of KeyItem)
    Public RandomList As New List(Of KeyItem)

    Public Const KeyItemDefaultHeight = 20

    Public Class KeyItem
        Public KeyType As KeyType = KeyType.Note
        Public NoteNumber As Byte                           ' 0-127
        Public NoteName As String = ""                      ' like "A4"
        Public DrumName As String = ""                  ' like "Acoustic Snare"
        Public IsBlackKey As Boolean
        Public Height As Integer = KeyItemDefaultHeight ' default = 20
        Public StartPosition As Integer
    End Class

    Public Enum KeyType
        Note
        PitchBend
        ControlChange
    End Enum

    Public Enum ViewType
        FullRangeList
        RangeList
        RandomList
    End Enum

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        KeyCanvas.TrackPanel = TrackPanel
        KeyCanvas.KeyPanel = Me

        If TrackPanel IsNot Nothing Then
            If TrackPanel.TrackData IsNot Nothing Then
                Dim nfo As Track.UsedNotesInfo = TrackPanel.TrackData.GetUsedNotesInfo
                RangeFirstNote = nfo.NoteRangeStart
                RangeNumberOfNotes = nfo.NoteRangeEnd - nfo.NoteRangeStart + 1
                nfo.ListOfUsedNotes.Reverse()           ' sort descending
                UsedNotes = nfo.ListOfUsedNotes
            End If
        End If
        FillKeyLists()
        UpdateView()
    End Sub
    ''' <summary>
    ''' Fill the three KeyItemLists depending on the corresponding parameters
    ''' </summary>
    Public Sub FillKeyLists()
        FillFullRangeList()
        FillRangeList()
        FillRandomList()
    End Sub

    Private Sub FillFullRangeList()
        FullRangeList.Clear()
        Dim last As Integer = FullRangeFirstNote + FullRangeNumberOfNotes - 1
        Dim pos As Integer = 0
        For i = last To FullRangeFirstNote Step -1
            InsertKeyItem(i, pos, FullRangeList)
            pos += KeyItemDefaultHeight
        Next
    End Sub

    Private Sub FillRangeList()
        RangeList.Clear()
        Dim last As Integer = RangeFirstNote + RangeNumberOfNotes - 1
        Dim pos As Integer = 0
        For i = last To RangeFirstNote Step -1
            InsertKeyItem(i, pos, RangeList)
            pos += KeyItemDefaultHeight
        Next
    End Sub

    Private Sub FillRandomList()
        RandomList.Clear()
        ' UsedNotes.Sort()
        ' UsedNotes.Reverse()
        Dim pos As Integer
        For Each note In UsedNotes
            InsertKeyItem(note, pos, RandomList)
            pos += KeyItemDefaultHeight
        Next
    End Sub

    Private Sub InsertKeyItem(NoteNumber As Byte, position As Integer, List As List(Of KeyItem))
        Dim item As New KeyItem
        item.NoteNumber = NoteNumber
        item.StartPosition = position
        item.NoteName = GetNoteNameFromNoteNumber(NoteNumber)
        item.DrumName = Get_GM_DrumVoiceName(NoteNumber)
        item.IsBlackKey = IsBlackKey(NoteNumber)
        List.Add(item)
    End Sub


    Private KeyPanelMaxWidth As Double = 50.0
    Private ScaleY As Double = 1.0

    Public Sub UpdateView()
        If TrackPanel Is Nothing Then Exit Sub
        ScaleY = TrackPanel.TrackHeaderPanel.TrackScaleY

        KeyCanvas.Height = GetKeyPanelHeight()

        UpdateMasterVScroll()


        KeyCanvas.InvalidateVisual()


        DrawKeys()

        TrackPanel.NotePanel.UpdateView()

    End Sub

    Public Sub UpdateMasterVScroll()
        TrackPanel.MasterVScroll.Maximum = KeyCanvas.Height - KeyPanelVScroll.ActualHeight
        TrackPanel.MasterVScroll.ViewportSize = KeyPanelVScroll.ViewportHeight
        TrackPanel.MasterVScroll.Value = KeyPanelVScroll.VerticalOffset

        TrackPanel.MasterVScroll.LargeChange = 50 * ScaleY
        TrackPanel.MasterVScroll.SmallChange = 10 * ScaleY
    End Sub


    Private Sub DrawKeys()
        If TrackPanel IsNot Nothing Then
            KeyPanelMaxWidth = TrackPanel.KeysColumn.MaxWidth
            If TrackPanel.TrackHeaderPanel IsNot Nothing Then
                Dim scaleY As Double = TrackPanel.TrackHeaderPanel.sldTrackScaleY.Value
            End If
        End If

        'KeyPanel.Children.Clear()

        If SelectedView = ViewType.FullRangeList Then
            DrawKeys(FullRangeList)
        ElseIf SelectedView = ViewType.RangeList Then
            DrawKeys(RangeList)
        ElseIf SelectedView = ViewType.RandomList Then
            DrawKeys(RandomList)
        End If
    End Sub

    Private Sub DrawKeys(List As List(Of KeyItem))
        Exit Sub


        For Each item In List

            If IsDrumView = False Then
                If item.IsBlackKey = True Then
                    'InsertRectangle(KeyPanel, 1, item.StartPosition * ScaleY, 50, item.Height * ScaleY)
                End If
            End If

            If IsDrumView = False Then
                ' InsertText(KeyPanel, item.NoteName, 5, item.StartPosition * ScaleY)
            Else
                ' InsertText(KeyPanel, item.DrumName, 5, item.StartPosition * ScaleY)
            End If

            ' InsertHorizontalLine(KeyPanel, 5, KeyPanelMaxWidth, (item.StartPosition + item.Height) * ScaleY)

        Next

    End Sub

    Private Sub InsertText(panel As Canvas, text As String, left As Double, top As Double)
        Dim tb As New TextBlock
        tb.Text = text
        Canvas.SetLeft(tb, left)
        Canvas.SetTop(tb, top)
        tb.IsHitTestVisible = False
        panel.Children.Add(tb)
    End Sub

    Private Sub InsertHorizontalLine(panel As Canvas, left As Double, right As Double, top As Double)
        Dim line As New Line
        line.X1 = left
        line.X2 = right
        line.Y1 = top
        line.Y2 = top
        line.Stroke = Brushes.Gray
        line.StrokeThickness = 0.5
        line.IsHitTestVisible = False
        panel.Children.Add(line)
    End Sub

    Private Sub InsertRectangle(panel As Canvas, left As Double, top As Double, width As Double, height As Double)
        Dim rect As New Rectangle
        Canvas.SetLeft(rect, left)
        Canvas.SetTop(rect, top)
        rect.Width = width
        rect.Height = height
        rect.Fill = Brushes.LightGray
        rect.IsHitTestVisible = False
        panel.Children.Add(rect)
    End Sub

    Private Function GetKeyPanelHeight() As Double
        Dim HeightBase As Double
        Dim list As New List(Of KeyItem)

        If SelectedView = ViewType.FullRangeList Then
            list = FullRangeList
        ElseIf SelectedView = ViewType.RangeList Then
            list = RangeList
        ElseIf SelectedView = ViewType.RandomList Then
            list = RandomList
        End If

        If list.Count > 0 Then
            Dim item As KeyItem
            item = list.LastOrDefault
            HeightBase = item.StartPosition + item.Height
        End If

        Return HeightBase * ScaleY
    End Function

    Private Sub UserControl_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        If IsLoaded = True Then
            UpdateMasterVScroll()
        End If
    End Sub

    Private Sub KeyPanelVScroll_ScrollChanged(sender As Object, e As ScrollChangedEventArgs) Handles KeyPanelVScroll.ScrollChanged
        If e.VerticalChange <> 0 Then
            TrackPanel.NotePanel.NotePanelScroll.ScrollToVerticalOffset(KeyPanelVScroll.VerticalOffset)
            TrackPanel.MasterVScroll.Value = KeyPanelVScroll.VerticalOffset
        End If
    End Sub

    'Private Sub KeyPanel_MouseMove(sender As Object, e As MouseEventArgs) Handles KeyPanel.MouseMove
    '    Dim pt As Point
    '    pt = e.GetPosition(KeyPanel)

    '    ' GetKeyItem (pt.y)
    'End Sub



End Class
