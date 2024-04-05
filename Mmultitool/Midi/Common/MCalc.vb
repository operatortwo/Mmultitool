Module MCalc
    Structure TwoBytes
        Public Byte1 As Byte
        Public Byte2 As Byte
    End Structure

    Structure ThreeBytes
        Public Byte1 As Byte
        Public Byte2 As Byte
        Public Byte3 As Byte
    End Structure

#Region "PitchBend"
    Public Function PitchBendValueToData(value As Integer) As TwoBytes
        ' limit to valid range
        If value < -8192 Then value = -8192
        If value > 8191 Then value = 8191

        Dim tb As TwoBytes
        tb.Byte1 = (value + 8192) Mod 128       ' LSB
        tb.Byte2 = (value + 8192) \ 128         ' MSB

        Return tb
    End Function

    Public Function PitchBendDataToValue(data1 As Byte, data2 As Byte) As Integer
        ' LSB MSB
        Return data1 + (data2 * 128) - 8192
    End Function
#End Region

#Region "Tempo"
    Public Function TempoDataToBPM(data1 As Byte, data2 As Byte, data3 As Byte) As Single
        Dim value As Single
        value = (data1 * 65536) + (data2 * 256) + data3
        Return 60000000 / value
    End Function

    Public Function TempoDataToMicroseconds(data1 As Byte, data2 As Byte, data3 As Byte) As Single
        Return (data1 * 65536) + (data2 * 256) + data3
    End Function

    Public Function BPM_ToTempoData(bpm As Single) As ThreeBytes
        Dim tb As ThreeBytes
        If bpm = 0 Then Return tb
        If bpm > 60000000 Then Return tb
        Dim value As Single
        value = 60000000 / bpm
        tb.Byte1 = value \ 65536
        tb.Byte2 = (value Mod 65536) \ 256
        tb.Byte3 = value Mod 65536 Mod 256
        Return tb
    End Function

    Public Function BPM_ToMicroseconds(bpm As Single) As Single
        If bpm = 0 Then Return 0
        Return 60000000 / bpm
    End Function
#End Region

    ''' <summary>
    ''' Convert a Byte that represents a signed byte-value to SByte.
    ''' </summary>        
    Public Function ByteToSByte(b As Byte) As SByte
        'IIf(b < 128, b, b - 256)
        If b < 128 Then
            Return b
        Else
            Return b - 256
        End If
    End Function



End Module
