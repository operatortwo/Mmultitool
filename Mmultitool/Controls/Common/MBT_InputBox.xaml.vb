
Imports System.ComponentModel

Public Class MBT_InputBox

    Public Shared ReadOnly TPQProperty As DependencyProperty = DependencyProperty.Register("TPQ", GetType(Integer), GetType(MBT_InputBox),
            New FrameworkPropertyMetadata(120, New PropertyChangedCallback(AddressOf OnTPQChanged),
            New CoerceValueCallback(AddressOf CoerceTPQ)))

    <Description("Ticks per quarter note"), Category("MBT InputBox")>
    Public Property TPQ() As Integer
        Get
            Return (GetValue(TPQProperty))
        End Get
        Set(ByVal value As Integer)
            SetValue(TPQProperty, value)
        End Set
    End Property

    Private Overloads Shared Function CoerceTPQ(ByVal d As DependencyObject, ByVal value As Object) As Object
        Dim newValue As Integer = value
        If newValue < 1 Then
            Return 1
        ElseIf newValue > 2000 Then
            Return 2000
        End If
        Return newValue
    End Function

    Private Shared Sub OnTPQChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As MBT_InputBox = CType(d, MBT_InputBox)


    End Sub

    Public Shared ReadOnly IsMBT_Base1Property As DependencyProperty = DependencyProperty.Register("IsMBT_Base1", GetType(Boolean), GetType(MBT_InputBox),
           New FrameworkPropertyMetadata(New PropertyChangedCallback(AddressOf OnIsMBT_Base1Changed)))
    ''' <summary>
    ''' Use Base 1 for Measure and Beat. Default is Base 0
    ''' </summary>
    ''' <returns></returns>
    <Description("Use Base 1 for Measure and Beat. Default is Base 0"), Category("MBT InputBox")>
    Public Property IsMBT_Base1() As Boolean
        Get
            Return (GetValue(IsMBT_Base1Property))
        End Get
        Set(ByVal value As Boolean)
            SetValue(IsMBT_Base1Property, value)
        End Set
    End Property

    Private Shared Sub OnIsMBT_Base1Changed(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As MBT_InputBox = CType(d, MBT_InputBox)
        If control.IsMBT_Base1 = True Then

        Else

        End If
    End Sub

    Private InputKeyRegex As New Text.RegularExpressions.Regex("[0-9.:]")     ' single key input
    Private Sub TextBox1_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TextBox1.PreviewTextInput
        ' does not receive space (=always accepted)
        If InputKeyRegex.IsMatch(e.Text) = False Then
            e.Handled = True                                ' reject invalid keys       
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TextBox1.TextChanged
        If TextBox1.IsFocused = False Then Exit Sub
        SetValidatorBrush()
    End Sub
    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.Key = Key.Enter Then
            If IsInputValid(TextBox1.Text) = True Then
                RaiseValueChangedEvent(InputValue)
                TextBox1.Text = ""
            Else
                ValidatorRect.Stroke = Brushes.Red
            End If
        End If
    End Sub
    Private Sub TextBox1_GotFocus(sender As Object, e As RoutedEventArgs) Handles TextBox1.GotFocus
        SetValidatorBrush()
    End Sub

    Private Sub TextBox1_LostFocus(sender As Object, e As RoutedEventArgs) Handles TextBox1.LostFocus
        ValidatorRect.Stroke = Nothing
    End Sub

    Private Sub SetValidatorBrush()
        If IsInputValid(TextBox1.Text) = True Then
            ValidatorRect.Stroke = Brushes.Green
        Else
            ValidatorRect.Stroke = ValidatorInputBrush
        End If
    End Sub

    Private Shared ReadOnly DefaultValidatorInputBrush As New SolidColorBrush(Color.FromArgb(&HFF, &H56, &H9D, &HE5))
    Public Shared ReadOnly ValidatorInputBrushProperty As DependencyProperty = DependencyProperty.Register("ValidatorInputBrush", GetType(Brush), GetType(MBT_InputBox), New UIPropertyMetadata(DefaultValidatorInputBrush))
    ''' <summary>
    ''' Stroke Brush of the validator rectangle when in input state
    ''' </summary>
    <Description("Stroke Brush of the validator rectangle when in input state"), Category("MBT InputBox")>
    Public Property ValidatorInputBrush As Brush
        Get
            Return CType(GetValue(ValidatorInputBrushProperty), Brush)
        End Get
        Set(ByVal value As Brush)
            SetValue(ValidatorInputBrushProperty, value)
        End Set
    End Property

    Public Shared ReadOnly ValidatorStrokeThicknessProperty As DependencyProperty = DependencyProperty.Register("ValidatorStrokeThickness", GetType(Double), GetType(MBT_InputBox), New UIPropertyMetadata(1.0))
    ''' <summary>
    ''' Stroke Thickness of the validator rectangle
    ''' </summary>
    <Description("Stroke Thickness of the validator rectangle"), Category("MBT InputBox")>
    Public Property ValidatorStrokeThickness As Double
        Get
            Return CType(GetValue(ValidatorStrokeThicknessProperty), Double)
        End Get
        Set(ByVal value As Double)
            SetValue(ValidatorStrokeThicknessProperty, value)
        End Set
    End Property

    Private InputValue As UInteger

    Private Function IsInputValid(text As String) As Boolean
        If text Is Nothing Then Return False
        If text = "" Then Return False

        Dim measure As UInteger
        Dim beat As UInteger
        Dim tick As UInteger

        Dim txt As String = text.Trim               ' remove leading and trailing spaces

        Dim separators() As Char = {" "c, ":"c, "."c}
        Dim array As String()

        array = txt.Split(separators)
        If array.Count <> 3 Then Return False

        If UInteger.TryParse(array(0), measure) = False Then Return False
        If UInteger.TryParse(array(1), beat) = False Then Return False
        If UInteger.TryParse(array(2), tick) = False Then Return False

        If measure > 99999 Then Return False

        If IsMBT_Base1 = False Then
            If beat > 3 Then Return False
        Else
            If beat > 4 Then Return False
            If beat < 1 Then Return False
        End If

        If tick > (TPQ - 1) Then Return False

        If IsMBT_Base1 = True Then
            If measure > 0 Then measure -= 1
            If beat > 0 Then beat -= 1
        End If

        InputValue = tick
        InputValue += (beat * TPQ)
        InputValue += measure * TPQ * 4

        Return True
    End Function

    ''' <summary>
    ''' Identifies the ValueChanged routed event.
    ''' </summary>
    Public Shared ReadOnly ValueChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, GetType(RoutedPropertyChangedEventHandler(Of UInteger)), GetType(MBT_InputBox))

    ''' <summary>
    ''' UInteger. Occurs when the Value property changed, Input was successful.
    ''' </summary>
    <Description("UInteger. Occurs when the Value property changed, Input was successful.")>
    Public Custom Event ValueChanged As RoutedPropertyChangedEventHandler(Of UInteger)
        AddHandler(ByVal value As RoutedPropertyChangedEventHandler(Of UInteger))
            MyBase.AddHandler(ValueChangedEvent, value)
        End AddHandler
        RemoveHandler(ByVal value As RoutedPropertyChangedEventHandler(Of UInteger))
            MyBase.RemoveHandler(ValueChangedEvent, value)
        End RemoveHandler
        RaiseEvent(ByVal sender As System.Object, ByVal e As RoutedPropertyChangedEventArgs(Of UInteger))
        End RaiseEvent
    End Event

    Private Sub RaiseValueChangedEvent(newvalue As UInteger)
        Dim args As New RoutedPropertyChangedEventArgs(Of UInteger)(0, newvalue, ValueChangedEvent)
        MyBase.RaiseEvent(args)
    End Sub

End Class