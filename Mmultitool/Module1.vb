Module Module1
    Public Function FindLogicalParent(base As FrameworkElement, targetType As Type) As Object

        Dim current As DependencyObject = base.Parent

        While current IsNot Nothing
            If current.[GetType]() = targetType Then
                Return current
            End If
            current = LogicalTreeHelper.GetParent(current)
        End While
        Return Nothing
    End Function

    Friend Function CreateGlyphRun(text As String, glyphTypeface As GlyphTypeface, emSize As Double, baselineOrigin As Point) As GlyphRun
        If text Is Nothing Then Return Nothing
        If text.Length = 0 Then Return Nothing
        If glyphTypeface Is Nothing Then Return Nothing

        Dim glyphIndices As UShort() = New UShort(text.Length - 1) {}
        Dim advanceWidths As Double() = New Double(text.Length - 1) {}

        For i = 0 To text.Length - 1
            Dim glyphIndex As UShort
            glyphTypeface.CharacterToGlyphMap.TryGetValue(AscW(text(i)), glyphIndex)
            glyphIndices(i) = glyphIndex
            advanceWidths(i) = glyphTypeface.AdvanceWidths(glyphIndex) * emSize
        Next

        Return New GlyphRun(glyphTypeface, 0, False, emSize, glyphIndices, baselineOrigin,
            advanceWidths, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Get time of next step in stepvalue grid.
    ''' </summary>
    ''' <param name="Time">base value</param>
    ''' <returns>Time 1 rounded up to next step</returns>
    Public Function RoundToNextStep(Time As UInteger, Stepvalue As UInteger) As UInteger
        If Stepvalue = 0 Then Return Time

        Dim Newtime As UInteger
        Dim TicksPerUnit As Integer = Stepvalue         ' gridvalue
        Dim ElapsedTicks As UInteger                    ' in this unit
        Dim RemainingTicks As UInteger                  ' in this unit     

        ElapsedTicks = CUInt(Time Mod TicksPerUnit)
        RemainingTicks = CUInt(TicksPerUnit - ElapsedTicks)
        Newtime = Time + RemainingTicks                     ' round up to end of this unit

        Return Newtime
    End Function

    ''' <summary>
    ''' Get time of previous step in stepvalue grid.
    ''' </summary>
    ''' <param name="Time">base value</param>
    ''' <returns>Time 1 rounded down to previous step</returns>
    Public Function RoundToPreviousStep(Time As UInteger, Stepvalue As UInteger) As UInteger
        If Stepvalue = 0 Then Return Time

        Dim Newtime As UInteger
        Dim TicksPerUnit As UInteger = Stepvalue        ' gridvalue
        Dim ElapsedTicks As UInteger                    ' in this unit

        ElapsedTicks = Time Mod TicksPerUnit
        Newtime = Time - ElapsedTicks                   ' round down to start of this unit

        Return Newtime
    End Function

    Public Function RoundToStep(value As Double, stepvalue As Integer) As Double
        If stepvalue = 0 Then stepvalue = 1
        Dim steps As Double = Fix(value / stepvalue)

        Dim smod As Double = Math.Abs(value Mod stepvalue)  ' remainder
        If smod >= stepvalue / 2 Then       ' round up if necessary
            steps += 1
        End If

        Return steps * stepvalue
    End Function

End Module
