﻿Imports System.IO

Public Module Player
    'High-frequency timer should be limited to a single instance to conserve system resources.
    'That's why a module and not a class is used here. This prevents multiple instances.

    Public Event MidiOutShortMsg(status As Byte, data1 As Byte, data2 As Byte)
    Public Event MidiOutLongMsg(SysExData As Byte())

    Public Const PlayerTPQ = 480                            ' Ticks per Quarter Note 
    Private Const TPQdiv60 = 8                              ' auxiliary  480 \  60 = 8

    ' plays one or more tracks simultaneously
    Public ReadOnly Property TrackPlayerErrors As Integer       ' Number of Catches in Play TrackList
    Public ReadOnly Property TrackPlayerTime As Double          ' TrackPlayer Ticks, TrackPlayer position
    Public ReadOnly Property IsTrackPlayerRunning As Boolean     ' True while TrackPlayer is running

    ' plays one or more sequences independently
    Public ReadOnly Property SequencePlayerErrors As Integer    ' Number of Catches in Play SequenceList
    Public ReadOnly Property SequencePlayerTime As Double       ' SequencePlayer Ticks, SequencePlayer position
    Public ReadOnly Property IsSequencePlayerRunning As Boolean ' True while SequencePlayer is running

    Private _TrackPlayerBPM As Single = 120
    ''' <summary>
    ''' Tempo (BeatsPerMinute) Minimum: 10, Maximum: 300. Values above and below will be corrected    
    ''' </summary>
    ''' <returns></returns>
    Public Property TrackPlayerBPM As Single                      ' tempo (Beats per Minute)
        Get
            Return _TrackPlayerBPM
        End Get
        Set(value As Single)
            If value < 10 Then
                _TrackPlayerBPM = 10
            ElseIf value > 300 Then
                _TrackPlayerBPM = 300
            Else
                _TrackPlayerBPM = value
            End If
        End Set
    End Property

    Private _SequencePlayerBPM As Single = 120
    ''' <summary>
    ''' Tempo (BeatsPerMinute) Minimum: 10, Maximum: 300. Values above and below will be corrected    
    ''' </summary>
    ''' <returns></returns>
    Public Property SequencePlayerBPM As Single                      ' tempo (Beats per Minute)
        Get
            Return _SequencePlayerBPM
        End Get
        Set(value As Single)
            If value < 10 Then
                _SequencePlayerBPM = 10
            ElseIf value > 300 Then
                _SequencePlayerBPM = 300
            Else
                _SequencePlayerBPM = value
            End If
        End Set
    End Property

    Private ReadOnly Stopwatch As New Stopwatch         ' to accurately measure elapsed time
    Private LastStopwatchTick As Long                   ' last Stopwatch.elapsedTicks    

    Private Declare Auto Function timeBeginPeriod Lib "winmm.dll" (uPeriod As UInteger) As UInteger
    Private Declare Auto Function timeEndPeriod Lib "winmm.dll" (uPeriod As UInteger) As UInteger

    Private Declare Auto Function timeSetEvent Lib "winmm.dll" (uDelay As UInteger, uResolution As UInteger, lpTimeProc As TimerProc, dwUser As IntPtr, fuEvent As UInteger) As UInteger
    Private Declare Auto Function timeKillEvent Lib "winmm.dll" (uTimerID As UInteger) As UInteger

    Private TimerID As UInteger

    Private Const TIME_PERIODIC = 1

    Delegate Sub TimerProc(uID As UInteger, uMsg As UInteger, dwUser As UInteger, dw1 As UInteger, dw2 As UInteger)
    Private ReadOnly fptrTimeProc As New TimerProc(AddressOf TickCallback)

    Private _TimerInterval As UInteger = 3
    ''' <summary>
    ''' Between 1 and 10 Milliseconds. Default = 3
    ''' </summary>
    ''' <returns></returns>
    Private Property TimerInterval As UInteger
        Get
            Return _TimerInterval
        End Get
        Set(value As UInteger)
            If value > 10 Then
                value = 10
            ElseIf value < 1 Then
                value = 1
            End If
            _TimerInterval = value
        End Set
    End Property

    Private _TimerResolution As UInteger = 3
    ''' <summary>
    ''' Between 0 and 10 , 0 = most accurate. Default = 3
    ''' </summary>
    ''' <returns></returns>
    Private Property TimerResolution As UInteger
        Get
            Return _TimerResolution
        End Get
        Set(value As UInteger)
            If value > 10 Then
                value = 10
            End If
            _TimerResolution = value
        End Set
    End Property

    Sub New()
        If Application.Current IsNot Nothing Then
            If Application.Current.MainWindow IsNot Nothing Then
                AddHandler Application.Current.MainWindow.Closing, AddressOf Dispose_Unmanaged_Resources
            End If
        End If
    End Sub

    Private Sub Dispose_Unmanaged_Resources()
        ' stop Main Timer if still running
        ' else we get Returncode 0xc0020001
        Stop_Timer()
    End Sub

    ''' <summary>
    ''' Only necessary when used from WinForms. 
    ''' </summary>
    Public Sub WinForms_Mmultitool_end()
        Dispose_Unmanaged_Resources()
    End Sub

    Private Sub Start_Timer()
        ' start the main timer
        If TimerID <> 0 Then Exit Sub
        Stopwatch.Start()
        timeBeginPeriod(TimerResolution)
        TimerID = timeSetEvent(TimerInterval, TimerResolution, fptrTimeProc, IntPtr.Zero, TIME_PERIODIC)
    End Sub

    Private Sub Stop_Timer()
        ' stop the main timer
        If TimerID <> 0 Then
            Stopwatch.Stop()
            timeKillEvent(TimerID)
            timeEndPeriod(TimerResolution)
            TimerID = 0
        End If
    End Sub

    Private Sub TickCallback(uID As UInteger, uMsg As UInteger, dwUser As UInteger, dw1 As UInteger, dw2 As UInteger)

        Dim currentTick As Long = Stopwatch.ElapsedTicks
        Dim DeltaTicks As Long                                      'stopwatch ticks

        DeltaTicks = currentTick - LastStopwatchTick
        LastStopwatchTick = currentTick

        'Dim DeltaMilliSeconds As Double = DeltaTicks / Stopwatch.Frequency * 1000

        ' Ticks = time(ms) * BPM * TPQ / 60'000
        ' Ticks = time(sec) * BPM * TPQ / 60                        ' 480 / 60 = 8     (TPQdiv60)
        ' Ticks = time(sec) * BPM * 16


        '--- Play TrackList ---

        If IsTrackPlayerRunning = True Then
            Dim DeltaTrackPlayerTicks As Double
            DeltaTrackPlayerTicks = DeltaTicks / Stopwatch.Frequency * TrackPlayerBPM * TPQdiv60

            _TrackPlayerTime += DeltaTrackPlayerTicks

            ' catch exceptions, make sure the tick ends as quickly as possible
            Try
                TrackPlayer.Do_TimedNoteOff(TrackPlayerTime)         ' NoteOff processing for TrackPlayer
                PlayTrackList()
            Catch
                _TrackPlayerErrors += 1
            End Try

        End If

        '--- Play SequenceList ---

        If IsSequencePlayerRunning = True Then
            Dim DeltaSequencePlayerTicks As Double
            DeltaSequencePlayerTicks = DeltaTicks / Stopwatch.Frequency * SequencePlayerBPM * TPQdiv60

            _SequencePlayerTime += DeltaSequencePlayerTicks

            ' catch exceptions, make sure the tick ends as quickly as possible
            Try
                SequencePlayer.Do_TimedNoteOff(SequencePlayerTime)                     ' NoteOff processing for SequencePlayer
                PlaySequenceList()
            Catch
                _SequencePlayerErrors += 1
            End Try

        End If



    End Sub

    Public Sub StartSequencePlayer()
        If TimerID = 0 Then Start_Timer()
        If IsSequencePlayerRunning = True Then Exit Sub
        _IsSequencePlayerRunning = True
    End Sub

    Public Sub StopSequencePlayer()
        If IsSequencePlayerRunning = False Then Exit Sub
        _IsSequencePlayerRunning = False
        SequencePlayer.AllRunningNotesOff()
        _IsSequencePlayerRunning = False
    End Sub

    Public Sub Set_SequencePlayerTime(newTime As Double)
        If IsSequencePlayerRunning = True Then
            If newTime < SequencePlayerTime Then
                SequencePlayer.AllRunningNotesOff()
            End If
        End If
        _SequencePlayerTime = newTime
    End Sub

    Public Sub StartTrackPlayer()
        If TimerID = 0 Then Start_Timer()
        If IsTrackPlayerRunning = True Then Exit Sub
        If TrackPlayerTime >= Player.Tracklist1.MaxLength Then
            Set_TrackPlayerTime(0)
        End If
        _IsTrackPlayerRunning = True
    End Sub

    Public Sub StopTrackPlayer()
        If IsTrackPlayerRunning = False Then Exit Sub
        TrackPlayer.AllRunningNotesOff()
        _IsTrackPlayerRunning = False
    End Sub



End Module
