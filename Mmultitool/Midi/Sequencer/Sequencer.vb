Imports MS.Internal

Public Module Sequencer
    'High-frequency timer should be limited to a single instance to conserve system resources.
    'That's why a module and not a class is used here. This prevents multiple instances.

    Public Event TestEvent()            ' works
    Public Event MidiOutShortMsg(status As Byte, data1 As Byte, data2 As Byte)
    Public Event MidiOutLongMsg(SysExData As Byte())

    Public ReadOnly Property PlaySequenceErrors As Integer  ' Number of Catches in the Play_Sequence Sub
    Public ReadOnly Property SequencerTime As Double        ' Sequencer Ticks, Sequencer position
    Public ReadOnly Property IsSequencerRunning As Boolean           ' Is Sequencer Running ?

    Public Const SequencerTPQ = 960                                  ' Ticks per Quarter Note 
    Private Const TPQdiv60 = SequencerTPQ \ 60                       ' auxiliary

    Private _BPM As Single = 120
    ''' <summary>
    ''' Tempo (BeatsPerMinute) Minimum: 10, Maximum: 300. Values above and below will be corrected    
    ''' </summary>
    ''' <returns></returns>
    Public Property SequencerBPM As Single                      ' tempo (Beats per Minute)
        Get
            Return _BPM
        End Get
        Set(value As Single)
            If value < 10 Then
                _BPM = 10
            ElseIf value > 300 Then
                _BPM = 300
            Else
                _BPM = value
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
        Dim DeltaSongTicks As Double                                ' player ticks

        DeltaTicks = currentTick - LastStopwatchTick
        LastStopwatchTick = currentTick

        'Dim DeltaMilliSeconds As Double = DeltaTicks / Stopwatch.Frequency * 1000

        ' Ticks = time(ms) * BPM * TPQ / 60'000
        ' Ticks = time(sec) * BPM * TPQ / 60                        ' 960 / 60 = 16     (TPQdiv60)
        ' Ticks = time(sec) * BPM * 16
        DeltaSongTicks = DeltaTicks / Stopwatch.Frequency * SequencerBPM * TPQdiv60

        '--- Sequencer ---
        If IsSequencerRunning = True Then
            _SequencerTime += DeltaSongTicks

            ' catch exceptions, make sure the tick ends as quickly as possible
            Try
                Do_TimedNoteOff(SequencerTime)                      ' NoteOff processing
                PlaySequenceList()
            Catch
                _PlaySequenceErrors += 1
            End Try

        End If

    End Sub

    Public Sub StartSequencer()
        If IsSequencerRunning = True Then Exit Sub
        Start_Timer()
        _IsSequencerRunning = True
    End Sub

    Public Sub StopSequencer()
        If IsSequencerRunning = False Then Exit Sub
        AllRunningNotesOff()
        Stop_Timer()
        _IsSequencerRunning = False
    End Sub

    Public Sub Set_SequencerTime(newTime As Double)
        If IsSequencerRunning = True Then
            If newTime < SequencerTime Then
                AllRunningNotesOff()
            End If
        End If

        _SequencerTime = newTime
    End Sub

End Module
