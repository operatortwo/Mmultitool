Imports System.ComponentModel

Public Class NoteDurationHelper
    Public Sub New()
        ' required for the designer
        InitializeComponent()
    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs) Handles UserControl.Loaded
        ComboBox1.SelectedIndex = 2
    End Sub

    Public Shared ReadOnly TPQProperty As DependencyProperty = DependencyProperty.Register("TPQ", GetType(Integer), GetType(NoteDurationHelper),
          New FrameworkPropertyMetadata(120, New PropertyChangedCallback(AddressOf OnTPQChanged),
          New CoerceValueCallback(AddressOf CoerceTPQ)))

    <Description("Ticks per quarter note"), Category("NoteDurationHelper")>   ' appears in VS property
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
        Dim control As NoteDurationHelper = CType(d, NoteDurationHelper)
        control.TryValueChanged()
    End Sub

    <Description("The calculated value dependent on TPQ and the selected note"), Category("NoteDurationHelper")>
    Friend Shared ReadOnly ValueKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly("Value", GetType(UInteger), GetType(NoteDurationHelper), New PropertyMetadata(AddressOf OnValueChanged))
    Public Shared ReadOnly ValueProperty As DependencyProperty = ValueKey.DependencyProperty
    Public ReadOnly Property Value() As UInteger
        Get
            Return CDbl(GetValue(ValueProperty))
        End Get
    End Property

    Private Shared Sub OnValueChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As NoteDurationHelper = CType(d, NoteDurationHelper)
        control.RaiseValueChangedEvent(control.Value)
    End Sub


    ''' <summary>
    ''' Identifies the ValueChanged routed event.
    ''' </summary>
    Public Shared ReadOnly ValueChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, GetType(RoutedPropertyChangedEventHandler(Of UInteger)), GetType(NoteDurationHelper))

    ''' <summary>
    ''' UInteger. Occurs when the Value property (Duration) changed.
    ''' </summary>
    <Description("UInteger. Occurs when the Value property (Duration) changed.")>
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

    Private Sub ComboBox1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBox1.SelectionChanged
        TryValueChanged()
    End Sub

    Private Sub TryValueChanged()
        If ComboBox1 IsNot Nothing Then
            If ComboBox1.SelectedItem IsNot Nothing Then
                Dim sel As ComboBoxItem = ComboBox1.SelectedItem
                Dim div As Single = sel.Tag
                Dim val As UInteger
                val = CUInt(TPQ / div)
                SetValue(ValueKey, val)
            End If
        End If
    End Sub


End Class
