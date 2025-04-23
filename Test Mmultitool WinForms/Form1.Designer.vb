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
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.InfoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripLabel1 = New System.Windows.Forms.ToolStripLabel()
        Me.lblSequencePlayerPosition = New System.Windows.Forms.ToolStripLabel()
        Me.btnRestartSequencer = New System.Windows.Forms.ToolStripButton()
        Me.btnStopSequencer = New System.Windows.Forms.ToolStripButton()
        Me.btnStartSequencer = New System.Windows.Forms.ToolStripButton()
        Me.lblMidiOutPort = New System.Windows.Forms.ToolStripLabel()
        Me.btnPlaySelected = New System.Windows.Forms.Button()
        Me.Mi_File_Exit = New System.Windows.Forms.ToolStripMenuItem()
        Me.cbDoLoop = New System.Windows.Forms.CheckBox()
        Me.MenuStrip1.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnClear
        '
        Me.btnClear.Location = New System.Drawing.Point(695, 90)
        Me.btnClear.Margin = New System.Windows.Forms.Padding(2)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(56, 30)
        Me.btnClear.TabIndex = 7
        Me.btnClear.Text = "Clear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'tbEvListerMessage
        '
        Me.tbEvListerMessage.Location = New System.Drawing.Point(345, 65)
        Me.tbEvListerMessage.Margin = New System.Windows.Forms.Padding(2)
        Me.tbEvListerMessage.Multiline = True
        Me.tbEvListerMessage.Name = "tbEvListerMessage"
        Me.tbEvListerMessage.Size = New System.Drawing.Size(309, 71)
        Me.tbEvListerMessage.TabIndex = 6
        '
        'tbEvListerFilename
        '
        Me.tbEvListerFilename.Location = New System.Drawing.Point(135, 65)
        Me.tbEvListerFilename.Margin = New System.Windows.Forms.Padding(2)
        Me.tbEvListerFilename.Multiline = True
        Me.tbEvListerFilename.Name = "tbEvListerFilename"
        Me.tbEvListerFilename.Size = New System.Drawing.Size(164, 35)
        Me.tbEvListerFilename.TabIndex = 5
        '
        'btnOpen
        '
        Me.btnOpen.Location = New System.Drawing.Point(20, 90)
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
        Me.ElementHost1.Location = New System.Drawing.Point(7, 155)
        Me.ElementHost1.Name = "ElementHost1"
        Me.ElementHost1.Size = New System.Drawing.Size(785, 394)
        Me.ElementHost1.TabIndex = 8
        Me.ElementHost1.Text = "ElementHost1"
        Me.ElementHost1.Child = Me.EventLister1
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.InfoToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(804, 24)
        Me.MenuStrip1.TabIndex = 9
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Mi_File_Exit})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'InfoToolStripMenuItem
        '
        Me.InfoToolStripMenuItem.Name = "InfoToolStripMenuItem"
        Me.InfoToolStripMenuItem.Size = New System.Drawing.Size(40, 20)
        Me.InfoToolStripMenuItem.Text = "Info"
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripLabel1, Me.lblSequencePlayerPosition, Me.btnRestartSequencer, Me.btnStopSequencer, Me.btnStartSequencer, Me.lblMidiOutPort})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 24)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(804, 25)
        Me.ToolStrip1.TabIndex = 10
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'ToolStripLabel1
        '
        Me.ToolStripLabel1.Name = "ToolStripLabel1"
        Me.ToolStripLabel1.Size = New System.Drawing.Size(56, 22)
        Me.ToolStripLabel1.Text = "Position: "
        '
        'lblSequencePlayerPosition
        '
        Me.lblSequencePlayerPosition.Name = "lblSequencePlayerPosition"
        Me.lblSequencePlayerPosition.Size = New System.Drawing.Size(50, 22)
        Me.lblSequencePlayerPosition.Text = "xx : x : xx"
        '
        'btnRestartSequencer
        '
        Me.btnRestartSequencer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnRestartSequencer.Image = Global.Test_Mmultitool_WinForms.My.Resources.Resources.PreviousTrack_x22
        Me.btnRestartSequencer.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnRestartSequencer.Margin = New System.Windows.Forms.Padding(10, 1, 0, 2)
        Me.btnRestartSequencer.Name = "btnRestartSequencer"
        Me.btnRestartSequencer.Size = New System.Drawing.Size(23, 22)
        Me.btnRestartSequencer.Text = "ToolStripButton1"
        '
        'btnStopSequencer
        '
        Me.btnStopSequencer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnStopSequencer.Image = Global.Test_Mmultitool_WinForms.My.Resources.Resources.Stop_x22
        Me.btnStopSequencer.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnStopSequencer.Name = "btnStopSequencer"
        Me.btnStopSequencer.Size = New System.Drawing.Size(23, 22)
        Me.btnStopSequencer.Text = "ToolStripButton2"
        '
        'btnStartSequencer
        '
        Me.btnStartSequencer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnStartSequencer.Image = Global.Test_Mmultitool_WinForms.My.Resources.Resources.Play_x22
        Me.btnStartSequencer.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnStartSequencer.Name = "btnStartSequencer"
        Me.btnStartSequencer.Size = New System.Drawing.Size(23, 22)
        Me.btnStartSequencer.Text = "ToolStripButton3"
        '
        'lblMidiOutPort
        '
        Me.lblMidiOutPort.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.lblMidiOutPort.Margin = New System.Windows.Forms.Padding(20, 1, 0, 2)
        Me.lblMidiOutPort.Name = "lblMidiOutPort"
        Me.lblMidiOutPort.Size = New System.Drawing.Size(55, 22)
        Me.lblMidiOutPort.Text = "x m out x"
        '
        'btnPlaySelected
        '
        Me.btnPlaySelected.Image = Global.Test_Mmultitool_WinForms.My.Resources.Resources.Play_x22
        Me.btnPlaySelected.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnPlaySelected.Location = New System.Drawing.Point(135, 113)
        Me.btnPlaySelected.Name = "btnPlaySelected"
        Me.btnPlaySelected.Padding = New System.Windows.Forms.Padding(5, 0, 5, 0)
        Me.btnPlaySelected.Size = New System.Drawing.Size(120, 28)
        Me.btnPlaySelected.TabIndex = 11
        Me.btnPlaySelected.Text = "Play Selected"
        Me.btnPlaySelected.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnPlaySelected.UseVisualStyleBackColor = True
        '
        'Mi_File_Exit
        '
        Me.Mi_File_Exit.Name = "Mi_File_Exit"
        Me.Mi_File_Exit.Size = New System.Drawing.Size(180, 22)
        Me.Mi_File_Exit.Text = "Exit"
        '
        'cbDoLoop
        '
        Me.cbDoLoop.AutoSize = True
        Me.cbDoLoop.Location = New System.Drawing.Point(261, 119)
        Me.cbDoLoop.Name = "cbDoLoop"
        Me.cbDoLoop.Size = New System.Drawing.Size(50, 17)
        Me.cbDoLoop.TabIndex = 12
        Me.cbDoLoop.Text = "Loop"
        Me.cbDoLoop.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(804, 561)
        Me.Controls.Add(Me.cbDoLoop)
        Me.Controls.Add(Me.btnPlaySelected)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.ElementHost1)
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.tbEvListerMessage)
        Me.Controls.Add(Me.tbEvListerFilename)
        Me.Controls.Add(Me.btnOpen)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "Form1"
        Me.Text = "Test Mmultitool WinForms"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnClear As Button
    Friend WithEvents tbEvListerMessage As TextBox
    Friend WithEvents tbEvListerFilename As TextBox
    Friend WithEvents btnOpen As Button
    Friend WithEvents ElementHost1 As Integration.ElementHost
    Friend EventLister1 As Mmultitool.EventLister
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents InfoToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents lblMidiOutPort As ToolStripLabel
    Friend WithEvents ToolStripLabel1 As ToolStripLabel
    Friend WithEvents lblSequencePlayerPosition As ToolStripLabel
    Friend WithEvents btnRestartSequencer As ToolStripButton
    Friend WithEvents btnStopSequencer As ToolStripButton
    Friend WithEvents btnStartSequencer As ToolStripButton
    Friend WithEvents btnPlaySelected As Button
    Friend WithEvents Mi_File_Exit As ToolStripMenuItem
    Friend WithEvents cbDoLoop As CheckBox
End Class
