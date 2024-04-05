Module EventListerModule
    Private _EvliTPQ As Integer = 120
    Public Property EvliTPQ As Integer
        Get
            Return _EvliTPQ
        End Get
        Set(value As Integer)
            If value < 1 Then
                _EvliTPQ = 1
            Else
                _EvliTPQ = value
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
