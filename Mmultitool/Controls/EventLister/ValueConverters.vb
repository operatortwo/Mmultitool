Imports System.Globalization
Public Class StatusByte_Converter
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If DesiredStatusFormat = HexOrDec.Hex Then
            Return Hex(value)
        Else
            Return value
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        If DesiredStatusFormat = HexOrDec.Hex Then
            Dim number As Integer = System.Convert.ToInt32(CStr(value), 16)
            Return number
        Else
            Return value
        End If
    End Function

    ''' <summary>
    ''' Make it usable from code behind
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Public Shared Function Convert(value As Object) As String
        If DesiredStatusFormat = HexOrDec.Hex Then
            Return Hex(value)
        Else
            Return value
        End If
    End Function


End Class

Public Class TimeToString_Converter
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value) = False Then Return False

        If DesiredTimeFormat = TimeFormat.Ticks Then
            Return value
        End If


        Dim time As Long = value

        Dim meas As Long                            ' measure (assume: 4/4)
        Dim beat As Integer                         ' beat inside measure
        Dim ticks As Integer

        meas = time \ (4 * TPQ)                      '  \ = returns an integer result                
        beat = CInt((time \ TPQ) Mod 4)
        ticks = CInt(time Mod TPQ)

        If DesiredTimeFormat = TimeFormat.MBT_0_based Then
            Return meas & " : " & beat & " : " & ticks.ToString("D3")                ' base 0
        Else
            'TimeFormat.MBT_1_based
            Return meas + 1 & " : " & beat + 1 & " : " & ticks.ToString("D3")        ' base 1
        End If

        'ticks.ToString("D3")

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Dim number As Integer = System.Convert.ToInt32(CStr(value), 16)
        Return number
    End Function

    ''' <summary>
    ''' Make it usable from code behind
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Public Shared Function Convert(value As Object) As String
        If IsNumeric(value) = False Then Return False

        If DesiredTimeFormat = TimeFormat.Ticks Then
            Return value
        End If


        Dim time As Long = value

        Dim meas As Long                            ' measure (assume: 4/4)
        Dim beat As Integer                         ' beat inside measure
        Dim ticks As Integer

        meas = time \ (4 * TPQ)                      '  \ = returns an integer result                
        beat = CInt((time \ TPQ) Mod 4)
        ticks = CInt(time Mod TPQ)

        If DesiredTimeFormat = TimeFormat.MBT_0_based Then
            Return meas & " : " & beat & " : " & ticks.ToString("D3")                ' base 0
        Else
            'TimeFormat.MBT_1_based
            Return meas + 1 & " : " & beat + 1 & " : " & ticks.ToString("D3")        ' base 1
        End If

        'ticks.ToString("D3")
    End Function


End Class