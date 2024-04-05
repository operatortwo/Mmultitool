<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.tbEvListerMessage = New System.Windows.Forms.TextBox()
        Me.tbEvListerFilename = New System.Windows.Forms.TextBox()
        Me.btnOpen = New System.Windows.Forms.Button()
        Me.ElementHost1 = New System.Windows.Forms.Integration.ElementHost()
        Me.EventLister1 = New Mmultitool.EventLister()
        Me.SuspendLayout()
        '
        'btnClear
        '
        Me.btnClear.Location = New System.Drawing.Point(722, 35)
        Me.btnClear.Margin = New System.Windows.Forms.Padding(2)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(56, 30)
        Me.btnClear.TabIndex = 7
        Me.btnClear.Text = "Clear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'tbEvListerMessage
        '
        Me.tbEvListerMessage.Location = New System.Drawing.Point(343, 11)
        Me.tbEvListerMessage.Margin = New System.Windows.Forms.Padding(2)
        Me.tbEvListerMessage.Multiline = True
        Me.tbEvListerMessage.Name = "tbEvListerMessage"
        Me.tbEvListerMessage.Size = New System.Drawing.Size(309, 71)
        Me.tbEvListerMessage.TabIndex = 6
        '
        'tbEvListerFilename
        '
        Me.tbEvListerFilename.Location = New System.Drawing.Point(134, 11)
        Me.tbEvListerFilename.Margin = New System.Windows.Forms.Padding(2)
        Me.tbEvListerFilename.Multiline = True
        Me.tbEvListerFilename.Name = "tbEvListerFilename"
        Me.tbEvListerFilename.Size = New System.Drawing.Size(164, 71)
        Me.tbEvListerFilename.TabIndex = 5
        '
        'btnOpen
        '
        Me.btnOpen.Location = New System.Drawing.Point(24, 35)
        Me.btnOpen.Margin = New System.Windows.Forms.Padding(2)
        Me.btnOpen.Name = "btnOpen"
        Me.btnOpen.Size = New System.Drawing.Size(56, 29)
        Me.btnOpen.TabIndex = 4
        Me.btnOpen.Text = "Open"
        Me.btnOpen.UseVisualStyleBackColor = True
        '
        'ElementHost1
        '
        Me.ElementHost1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ElementHost1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ElementHost1.Location = New System.Drawing.Point(7, 130)
        Me.ElementHost1.Name = "ElementHost1"
        Me.ElementHost1.Size = New System.Drawing.Size(762, 308)
        Me.ElementHost1.TabIndex = 8
        Me.ElementHost1.Text = "ElementHost1"
        Me.ElementHost1.Child = Me.EventLister1
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.ElementHost1)
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.tbEvListerMessage)
        Me.Controls.Add(Me.tbEvListerFilename)
        Me.Controls.Add(Me.btnOpen)
        Me.Name = "Form1"
        Me.Text = "Test Mmultitool WinForms"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnClear As Button
    Friend WithEvents tbEvListerMessage As TextBox
    Friend WithEvents tbEvListerFilename As TextBox
    Friend WithEvents btnOpen As Button
    Friend WithEvents ElementHost1 As Integration.ElementHost
    Friend EventLister1 As Mmultitool.EventLister
End Class
