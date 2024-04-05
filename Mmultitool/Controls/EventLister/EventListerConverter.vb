Imports System.Globalization

Partial Public Class EventLister

    Private TimeConvert As New TimeToStringConverter(Me)
    Private StatusConvert As New StatusByteConverter(Me)

    Public Class StatusByteConverter
        Implements IValueConverter

        Private ReadOnly Evlister As EventLister
        Public Sub New(evl As EventLister)
            Evlister = evl
        End Sub
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If Evlister.DesiredStatusFormat = HexOrDec.Hex Then
                Return Hex(value)
            Else
                Return value
            End If

        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            If Evlister.DesiredStatusFormat = HexOrDec.Hex Then
                Dim number As Integer = System.Convert.ToInt32(CStr(value), 16)
                Return number
            Else
                Return value
            End If
        End Function
    End Class

    Public Shared Function ConvertStatusByte(value As Object, DesiredFormat As HexOrDec) As String
        If DesiredFormat = HexOrDec.Hex Then
            Return Hex(value)
        Else
            Return value
        End If
    End Function


#Region "Time to String"
    Public Class TimeToStringConverter
        Implements IValueConverter

        Private ReadOnly Evlister As EventLister

        Public Sub New(evl As EventLister)
            Evlister = evl
        End Sub

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return ConvertTimeToString(value, Evlister.EvliTPQ, Evlister.DesiredTimeFormat)
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim number As Integer = System.Convert.ToInt32(CStr(value), 16)
            Return number
        End Function

    End Class

    Public Shared Function ConvertTimeToString(time As UInteger, TPQ As Integer, DesiredFormat As TimeFormat) As String

        If DesiredFormat = TimeFormat.Ticks Then
            Return time.ToString()
        End If

        Dim meas As Long                            ' measure (assume: 4/4)
        Dim beat As Integer                         ' beat inside measure
        Dim ticks As Integer

        meas = time \ (4 * TPQ)                      '  \ = returns an integer result                
        beat = CInt((time \ TPQ) Mod 4)
        ticks = CInt(time Mod TPQ)

        If DesiredFormat = TimeFormat.MBT_0_based Then
            Return meas & " : " & beat & " : " & ticks.ToString("D3")                ' base 0
        Else
            'TimeFormat.MBT_1_based
            Return meas + 1 & " : " & beat + 1 & " : " & ticks.ToString("D3")        ' base 1
        End If

    End Function


#End Region




End Class


