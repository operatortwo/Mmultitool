
Imports System.ComponentModel

Public Class MBT_Editor
    Public Sub New()
        ' required for the designer
        InitializeComponent()
    End Sub

    Private Const DefaultUpDownColMinWidth As Double = 25   ' adjustment for Grid Column with UpDown buttons. used together with 'Width="Auto"'
    Private Shared ReadOnly DefaultValueChangedBackground As New SolidColorBrush(Color.FromArgb(&HFF, &HFF, &HE6, &H72))     '#FF FF E6 72

#Region "Appearance"

    Public Shared Shadows ReadOnly BackgroundProperty As DependencyProperty = DependencyProperty.Register("Background",
            GetType(Brush), GetType(MBT_Editor), New UIPropertyMetadata(Brushes.LightGray))
    Public Overloads Property Background As Brush
        Get
            Return CType(GetValue(BackgroundProperty), Brush)
        End Get
        Set(ByVal value As Brush)
            SetValue(BackgroundProperty, value)
        End Set
    End Property

    Public Shared ReadOnly ValueChangedBackgroundProperty As DependencyProperty = DependencyProperty.Register("ValueChangedBackground",
            GetType(Brush), GetType(MBT_Editor), New UIPropertyMetadata(DefaultValueChangedBackground))
    ' appears in code
    ''' <summary>
    ''' Background brush when the value was changed
    ''' </summary>
    <Description("Background brush when the value was changed"), Category("MBT Editor")>   ' appears in VS property
    Public Property ValueChangedBackground As Brush
        Get
            Return CType(GetValue(ValueChangedBackgroundProperty), Brush)
        End Get
        Set(ByVal value As Brush)
            SetValue(ValueChangedBackgroundProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' Design adjustment for the Up/Dow buttons column
    ''' </summary>
    Public Shared ReadOnly UpDownColMinWidthProperty As DependencyProperty = DependencyProperty.Register("UpDownColMinWidth",
            GetType(Double), GetType(MBT_Editor), New FrameworkPropertyMetadata(DefaultUpDownColMinWidth,
            Nothing, New CoerceValueCallback(AddressOf CoerceUpDownColMinWidth)))
    <Description("Design adjustment for the Up/Dow buttons column. Default is 25"), Category("MBT Editor")>
    Public Property UpDownColMinWidth() As Double
        Get
            Return CDbl(GetValue(UpDownColMinWidthProperty))
        End Get
        Set(ByVal value As Double)
            SetValue(UpDownColMinWidthProperty, value)
        End Set
    End Property

    Private Overloads Shared Function CoerceUpDownColMinWidth(ByVal d As DependencyObject, ByVal value As Object) As Object
        Dim newValue As Double = CDbl(value)

        If newValue < 0 Then
            Return 0
        End If

        Return newValue
    End Function

    Private Shared ReadOnly DefaultFocusStrokeBrush As New SolidColorBrush(Color.FromArgb(&HFF, &H56, &H9D, &HE5))
    Public Shared ReadOnly FocusStrokeBrushProperty As DependencyProperty = DependencyProperty.Register("FocusStrokeBrush", GetType(Brush), GetType(MBT_Editor), New UIPropertyMetadata(DefaultFocusStrokeBrush))
    ''' <summary>
    ''' Stroke Brush of the focus rectangle
    ''' </summary>
    <Description("Stroke Brush of the focus rectangle"), Category("MBT Editor")>
    Public Property FocusStrokeBrush As Brush
        Get
            Return CType(GetValue(FocusStrokeBrushProperty), Brush)
        End Get
        Set(ByVal value As Brush)
            SetValue(FocusStrokeBrushProperty, value)
        End Set
    End Property

    Public Shared ReadOnly FocusStrokeThicknessProperty As DependencyProperty = DependencyProperty.Register("FocusStrokeThickness", GetType(Double), GetType(MBT_Editor), New UIPropertyMetadata(1.0))
    ''' <summary>
    ''' Stroke Thickness of the focus rectangle
    ''' </summary>
    <Description("Stroke Thickness of the focus rectangle"), Category("MBT Editor")>
    Public Property FocusStrokeThickness As Double
        Get
            Return CType(GetValue(FocusStrokeThicknessProperty), Double)
        End Get
        Set(ByVal value As Double)
            SetValue(FocusStrokeThicknessProperty, value)
        End Set
    End Property

#End Region

#Region "Value"

    Public Shared ReadOnly TPQProperty As DependencyProperty = DependencyProperty.Register("TPQ", GetType(Integer), GetType(MBT_Editor),
            New FrameworkPropertyMetadata(120, New PropertyChangedCallback(AddressOf OnTPQChanged),
            New CoerceValueCallback(AddressOf CoerceTPQ)))

    <Description("Ticks per quarter note"), Category("MBT Editor")>   ' appears in VS property
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
        Dim control As MBT_Editor = CType(d, MBT_Editor)
        control.nudTick.MaximumValue = control.TPQ - 1
        control.SetNumericUpDownValues()
    End Sub

    Public Shared ReadOnly OriginalValueProperty As DependencyProperty = DependencyProperty.Register("OriginalValue", GetType(UInteger), GetType(MBT_Editor),
           New FrameworkPropertyMetadata(New PropertyChangedCallback(AddressOf OnOriginalValueChanged)))

    <Description("The original unchanged value"), Category("MBT Editor")>   ' appears in VS property
    Public Property OriginalValue() As UInteger
        Get
            Return (GetValue(OriginalValueProperty))
        End Get
        Set(ByVal value As UInteger)
            SetValue(OriginalValueProperty, value)
        End Set
    End Property

    Private Shared Sub OnOriginalValueChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As MBT_Editor = CType(d, MBT_Editor)
        control.SetValue(NewValueKey, control.OriginalValue)
        control.SetNumericUpDownValues()
        control.UpdateAllTextBoxBackgrounds()                   ' reset background in case it was changed before
    End Sub

    <Description("The edited value"), Category("MBT Editor")>
    Friend Shared ReadOnly NewValueKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly("NewValue", GetType(UInteger), GetType(MBT_Editor), New PropertyMetadata(AddressOf OnNewValueChanged))
    Public Shared ReadOnly NewValueProperty As DependencyProperty = NewValueKey.DependencyProperty
    Public ReadOnly Property NewValue() As UInteger
        Get
            Return CDbl(GetValue(NewValueProperty))
        End Get
    End Property

    Private Shared Sub OnNewValueChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As MBT_Editor = CType(d, MBT_Editor)
        control.RaiseValueChangedEvent(control.NewValue)
    End Sub


    ''' <summary>
    ''' Go back to original value
    ''' </summary>
    Public Sub ResetToOriginalValue()
        SetValue(NewValueKey, OriginalValue)
        SetNumericUpDownValues()
    End Sub

    ''' <summary>
    ''' Set current value
    ''' </summary>
    ''' <param name="newvalue"></param>
    Public Sub ChangeCurrentValue(newvalue As UInteger)
        SetValue(NewValueKey, newvalue)
        SetNumericUpDownValues()
    End Sub


    ''' <summary>
    ''' Identifies the ValueChanged routed event.
    ''' </summary>
    Public Shared ReadOnly ValueChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, GetType(RoutedPropertyChangedEventHandler(Of UInteger)), GetType(MBT_Editor))

    ''' <summary>
    ''' UInteger. Occurs when the NewValue property changed.
    ''' </summary>
    <Description("UInteger. Occurs when the NewValue property changed")>
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

    Public Shared ReadOnly IsMBT_Base1Property As DependencyProperty = DependencyProperty.Register("IsMBT_Base1", GetType(Boolean), GetType(MBT_Editor),
           New FrameworkPropertyMetadata(New PropertyChangedCallback(AddressOf OnIsMBT_Base1Changed)))
    ''' <summary>
    ''' Use Base 1 for Measure and Beat. Default is Base 0
    ''' </summary>
    ''' <returns></returns>
    <Description("Use Base 1 for Measure and Beat. Default is Base 0"), Category("MBT Editor")>
    Public Property IsMBT_Base1() As Boolean
        Get
            Return (GetValue(IsMBT_Base1Property))
        End Get
        Set(ByVal value As Boolean)
            SetValue(IsMBT_Base1Property, value)
        End Set
    End Property

    Private Shared Sub OnIsMBT_Base1Changed(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As MBT_Editor = CType(d, MBT_Editor)
        If control.IsMBT_Base1 = True Then
            control.nudBeat.MinimumValue = 1
            control.nudBeat.MaximumValue = 4
            control.nudMeasure.MinimumValue = 1
        Else
            control.nudBeat.MinimumValue = 0
            control.nudBeat.MaximumValue = 3
            control.nudMeasure.MinimumValue = 0
        End If
        control.SetNumericUpDownValues()
    End Sub

#End Region

#Region "Control"

    Private Sub nudBeat_SpinUp(sender As Object, e As RoutedEventArgs) Handles nudBeat.SpinUp
        nudBeat.SetValueSilent(nudBeat.MinimumValue)
        nudMeasure.PushUpButton()
    End Sub

    Private Sub nudBeat_SpinDown(sender As Object, e As RoutedEventArgs) Handles nudBeat.SpinDown
        If nudMeasure.Value > nudMeasure.MinimumValue Then
            nudBeat.SetValueSilent(nudBeat.MaximumValue)
            nudMeasure.PushDownButton()
        End If
    End Sub

    Private Sub nudTick_SpinUp(sender As Object, e As RoutedEventArgs) Handles nudTick.SpinUp
        nudTick.SetValueSilent(nudTick.MinimumValue)
        nudBeat.PushUpButton()
    End Sub

    Private Sub nudTick_SpinDown(sender As Object, e As RoutedEventArgs) Handles nudTick.SpinDown

        If nudBeat.Value > nudBeat.MinimumValue Then
            nudTick.SetValueSilent(nudTick.MaximumValue)
            nudBeat.PushDownButton()
        ElseIf nudMeasure.Value > nudMeasure.MinimumValue Then
            nudTick.SetValueSilent(nudTick.MaximumValue)
            nudBeat.PushDownButton()
        End If

    End Sub

    Private Sub nudTick_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        UpdateNewValue()
    End Sub

    Private Sub nudBeat_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        UpdateNewValue()
    End Sub

    Private Sub nudMeasure_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        UpdateNewValue()
    End Sub

    Private Sub userControl_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseDown
        Focus()
    End Sub

    Private Sub userControl_GotFocus(sender As Object, e As RoutedEventArgs) Handles Me.GotFocus
        'FocusRect.Fill = New SolidColorBrush(Color.FromArgb(&HFF, &H4, &H81, &HFF))

        ' don't set always, can block ctrl+tab function
        If IsKeyboardFocused = True Then
            Keyboard.Focus(nudTick)
        End If
    End Sub

    Private Sub userControl_LostFocus(sender As Object, e As RoutedEventArgs) Handles Me.LostFocus
        'FocusRect.Fill = Nothing
    End Sub

#End Region


End Class
