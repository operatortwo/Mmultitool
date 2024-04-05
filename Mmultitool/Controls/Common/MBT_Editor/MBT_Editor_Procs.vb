Imports DailyUserControls

Partial Public Class MBT_Editor

    Private Sub SetNumericUpDownValues()
        Dim value As UInteger = Me.NewValue
        Dim meas As Long                            ' measure (assume: 4/4)
        Dim beat As Integer                         ' beat inside measure
        Dim ticks As Integer

        meas = value \ (4 * TPQ)                      '  \ = returns an integer result                
        beat = CInt((value \ TPQ) Mod 4)
        ticks = CInt(value Mod TPQ)

        If IsMBT_Base1 = True Then
            meas += 1
            beat += 1
        End If

        nudMeasure.Value = meas
        nudBeat.Value = beat
        nudTick.Value = ticks

    End Sub

    Private Sub UpdateNewValue()
        Dim newval As UInteger

        Dim meas As UInteger = nudMeasure.Value
        Dim beat As UInteger = nudBeat.Value
        Dim tick As UInteger = nudTick.Value

        If IsMBT_Base1 = True Then
            If meas > 0 Then meas -= 1
            If beat > 0 Then beat -= 1
        End If

        newval = tick
        newval += (beat * TPQ)
        newval += meas * TPQ * 4

        SetValue(NewValueKey, newval)


        UpdateAllTextBoxBackgrounds()
    End Sub

    Private Sub UpdateAllTextBoxBackgrounds()
        Dim origMeas As UInteger = GetMeasure(OriginalValue)
        Dim origbeat As UInteger = GetBeat(OriginalValue)
        Dim origTick As UInteger = GetTick(OriginalValue)

        Dim newMeas As UInteger = GetMeasure(NewValue)
        Dim newBeat As UInteger = GetBeat(NewValue)
        Dim newTick As UInteger = GetTick(NewValue)

        If origMeas = newMeas Then
            nudMeasure.ClearValue(NumericUpDown.TextBoxBackgroundProperty)
        Else
            nudMeasure.TextBoxBackground = ValueChangedBackground
        End If

        If origbeat = newBeat Then
            nudBeat.ClearValue(NumericUpDown.TextBoxBackgroundProperty)
        Else
            nudBeat.TextBoxBackground = ValueChangedBackground
        End If

        If origTick = newTick Then
            nudTick.ClearValue(NumericUpDown.TextBoxBackgroundProperty)
        Else
            nudTick.TextBoxBackground = ValueChangedBackground
        End If

    End Sub


    Private Function GetMeasure(value As UInteger) As UInteger
        Dim meas As Long
        meas = value \ (4 * TPQ)
        If IsMBT_Base1 = True Then meas += 1
        Return meas
    End Function

    Private Function GetBeat(value As UInteger) As UInteger
        Dim beat As Integer
        beat = CInt((value \ TPQ) Mod 4)
        If IsMBT_Base1 = True Then beat += 1
        Return beat
    End Function

    Private Function GetTick(value As UInteger) As UInteger
        Dim ticks As Integer
        ticks = CInt(value Mod TPQ)
        If IsMBT_Base1 = True Then ticks += 1
        Return ticks
    End Function

    Private Sub ValueToMBT(value As UInteger)
        Dim meas As Long                            ' measure (assume: 4/4)
        Dim beat As Integer                         ' beat inside measure
        Dim ticks As Integer

        meas = value \ (4 * TPQ)                      '  \ = returns an integer result                
        beat = CInt((value \ TPQ) Mod 4)
        ticks = CInt(value Mod TPQ)

        'if MBT 1 
        ' meas +1
        ' beat +1

    End Sub

End Class
