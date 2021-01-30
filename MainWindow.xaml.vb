﻿Imports System.ComponentModel
Imports System.Threading
Public Class MainWindow
    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Process.GetCurrentProcess.Kill()
    End Sub

    '初始化
    Private Const WindowMargin = 40
    Public PixelLevel = 1
    Private Sub Init() Handles Me.Loaded
        FrmMain = Me
        AniStartRun()
        StartLevel(1)
        'UI 初始化
        SetText(TextBottomLine, "\DARKGRAY─────────────────────────────────────┴───────────────")
        SetText(TextActionLine, "\DARKGRAY││││││││││││││││┤││││││││││││││││││││")
        SetText(TextChatLine, "\DARKGRAY──────────────────────────────────────────────────────")
        SetText(TextInputResult, "\DARKGRAY等待玩家输入指令。")
        TextInputBox.Tag = ""
        '窗口自适应
        Dim Size As Integer = Math.Floor(Math.Min((ActualHeight - WindowMargin * 2) / PanMain.Height, (ActualWidth - WindowMargin * 2) / PanMain.Width))
        TransScale.ScaleX = Size
        TransScale.ScaleY = Size
        '刷新 UI
        RunInNewThread(Sub()
                           Do While True
                               Dispatcher.Invoke(Sub() RefreshUI())
                               Thread.Sleep(25)
                           Loop
                       End Sub, "UI Loop")
        '抖动特效
        RunInNewThread(Sub()
                           Do While True
                               '获取偏移值
                               Dim Pixelation As Integer '最大值为 1000（对应 1）
                               Select Case PixelLevel
                                   Case 0
                                       Pixelation = 0
                                   Case 1
                                       If RandomInteger(0, 39) = 0 Then
                                           Pixelation = RandomInteger(620, 820)
                                       Else
                                           Pixelation = 0
                                       End If
                                   Case 2
                                       If RandomInteger(0, 19) = 0 Then
                                           Pixelation = RandomInteger(700, 900)
                                       Else
                                           Pixelation = RandomInteger(200, 700)
                                       End If
                               End Select
                               '改变状态
                               RunInUi(Sub()
                                           If Pixelation <= 0 Then
                                               PanBack2.Effect = Nothing
                                           ElseIf PanBack2.Effect IsNot Nothing Then
                                               CType(PanBack2.Effect, Microsoft.Expression.Media.Effects.PixelateEffect).Pixelation = Pixelation / 1000
                                           Else
                                               PanBack2.Effect = New Microsoft.Expression.Media.Effects.PixelateEffect With {.Pixelation = Pixelation / 1000}
                                           End If
                                       End Sub)
                               Thread.Sleep(30)
                           Loop
                       End Sub, "Shake")
    End Sub

    '文本输入
    Private Sub MainWindow_KeyPress(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        '获取真实输入文本
        Dim RealKey As String = e.Key.ToString.ToUpper
        If RealKey.StartsWith("NUMPAD") Then RealKey = RealKey.Substring(6)
        If RealKey.StartsWith("D") AndAlso RealKey.Length = 2 Then RealKey = RealKey.Substring(1)
        '按下任意按键
        If EnterStatus = EnterStatuses.Chat Then
            NextChat()
            Exit Sub
        End If
        '主输入状态
        If e.Key = Key.Escape AndAlso Not (Screen = Screens.Combat OrElse Screen = Screens.Empty) Then
            TextInputBox.Tag = ""
            Enter("ESC")
        ElseIf e.Key = Key.Back Then
            TextInputBox.Tag = ""
            'If TextInputBox.Tag.ToString.Length > 0 Then TextInputBox.Tag = TextInputBox.Tag.ToString.Substring(0, TextInputBox.Tag.ToString.Length - 1)
        ElseIf e.Key = Key.Enter Then
            If TextInputBox.Tag <> "" Then Enter(TextInputBox.Tag)
            TextInputBox.Tag = ""
        ElseIf Not DisabledKey.Contains(RealKey) AndAlso RealKey.Length = 1 Then
            TextInputBox.Tag = (TextInputBox.Tag.ToString & RealKey).Substring(0, Math.Min(TextInputBox.Tag.ToString.Length + 1, 47))
        End If
        RefreshInputBox()
    End Sub
    Public Sub RefreshInputBox() Handles Me.Loaded
        SetText(TextInputBox, ">" & TextInputBox.Tag & "_")
    End Sub

End Class
