Module EventListerModule
    Private _TPQ As Integer = 120
    Public Property TPQ As Integer
        Get
            Return _TPQ
        End Get
        Set(value As Integer)
            If value < 1 Then
                _TPQ = 1
            Else
                _TPQ = value
            End If
        End Set
    End Property

    Public Property DesiredTimeFormat As TimeFormat = TimeFormat.MBT_0_based
    Public Property DesiredStatusFormat As HexOrDec = HexOrDec.Hex

    Public Enum TimeFormat
        Ticks
        MBT_0_based
        MBT_1_based
    End Enum

    Public Enum HexOrDec
        Hex
        Dec
    End Enum
End Module
