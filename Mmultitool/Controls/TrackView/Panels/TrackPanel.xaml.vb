﻿Imports System.ComponentModel
Public Class TrackPanel
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        SetInitialHeight()
    End Sub


#Region "Expand/Collapse Panel"

    Private ExpandedHeight As Double
    Private Const DefaultExpandedHeight = 64

    ''' <summary>
    ''' Set the initial height depending on the IsExpanded state.
    ''' </summary>
    Private Sub SetInitialHeight()
        If IsExpanded = True Then
            ExpandedHeight = Me.Height
        Else
            ExpandedHeight = DefaultExpandedHeight
            Me.Height = MinHeight
        End If
    End Sub
    Private Sub TrackExpander_Collapsed(sender As Object, e As RoutedEventArgs) Handles TrackExpander.Collapsed
        IsExpanded = False
    End Sub

    Private Sub TrackExpander_Expanded(sender As Object, e As RoutedEventArgs) Handles TrackExpander.Expanded
        IsExpanded = True
    End Sub

    Private Sub HeightSplitter_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles HeightSplitter.PreviewMouseLeftButtonDown
        Dim el As UIElement = CType(sender, UIElement)
        el.CaptureMouse()
    End Sub

    Private Sub HeightSplitter_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles HeightSplitter.PreviewMouseLeftButtonUp
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then
            el.ReleaseMouseCapture()
        End If
    End Sub

    Private Sub HeightSplitter_PreviewMouseMove(sender As Object, e As MouseEventArgs) Handles HeightSplitter.PreviewMouseMove
        Dim el As UIElement = CType(sender, UIElement)
        If el.IsMouseCaptured Then

            If e.LeftButton = MouseButtonState.Pressed Then     ' dragging
                If TrackExpander.IsExpanded = False Then
                    ' is collapsed
                    If Height > MinHeight Then
                        TrackExpander.IsExpanded = True
                        Exit Sub
                    End If
                Else
                    'is expanded
                    If Height <= MinHeight Then
                        TrackExpander.IsExpanded = False            ' collapse
                        ExpandedHeight = DefaultExpandedHeight      ' set ExpandedHeight for next 'expanded'
                        Exit Sub
                    End If
                End If

                Dim pt As Point
                pt = e.GetPosition(Me)

                If pt.Y < HeightSplitter.ActualHeight Then pt.Y = HeightSplitter.ActualHeight

                If pt.Y > MaxHeight Then pt.Y = MaxHeight       ' defined in UserControl property (TrackPanel)
                Me.Height = pt.Y
                If Height > MinHeight Then
                    ExpandedHeight = Height
                End If

            End If
        End If
    End Sub

    Public Shared ReadOnly IsExpandedProperty As DependencyProperty = DependencyProperty.Register("IsExpanded", GetType(Boolean), GetType(TrackPanel), New FrameworkPropertyMetadata(True, New PropertyChangedCallback(AddressOf OnIsExpandedChanged)))
    ' appears in code
    ''' <summary>
    ''' Is TrackPanel expanded
    ''' </summary>
    <Description("Is TrackPanel expanded"), Category("Track Panel")>   ' appears in VS property
    Public Property IsExpanded As Boolean

        Get
            Return CType(GetValue(IsExpandedProperty), Boolean)
        End Get
        Set(ByVal value As Boolean)
            SetValue(IsExpandedProperty, value)
        End Set

    End Property

    Private Shared Sub OnIsExpandedChanged(ByVal d As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        Dim control As TrackPanel = CType(d, TrackPanel)

        If control.IsExpanded = True Then
            control.Height = control.ExpandedHeight
            control.TrackExpander.IsExpanded = True
        Else
            control.Height = control.MinHeight
            control.TrackExpander.IsExpanded = False
        End If
    End Sub

    '--- Allow setting IsExpanded by code. This can also be done by setting the IsExpanded Property.
    ''' <summary>
    ''' Expand the TrackPanel
    ''' </summary>
    Public Sub Expand()
        IsExpanded = True
    End Sub

    ''' <summary>
    ''' Collapse the TrackPanel
    ''' </summary>
    Public Sub Collapse()
        IsExpanded = False
    End Sub

#End Region


End Class