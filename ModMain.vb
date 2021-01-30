﻿Imports System.Text

Public Module ModMain

    '对 TextBlock 设置富文本
    Public Sub SetText(Target As TextBlock, RawText As String)
        RawText = RawText.Replace("\n", vbCrLf)
        '将文本按颜色分段，保证每段开头均为颜色标记
        If Not RawText.StartsWith("\") Then RawText = "WHITE" & RawText
        Dim RawTexts As String() = RawText.Split("\")
        '修改目标显示
        Target.Inlines.Clear()
        For Each Inline In RawTexts
            If Inline = "" Then Continue For
            Dim TargetColor As MyColor
            Dim Delta As Integer = 0
            If Inline.StartsWith("WHITE") Then
                TargetColor = New MyColor(255, 255, 255)
                Delta = 5
            ElseIf Inline.StartsWith("AQUA") Then
                TargetColor = New MyColor(0, 255, 255)
                Delta = 4
            ElseIf Inline.StartsWith("RED") Then
                TargetColor = New MyColor(255, 0, 0)
                Delta = 3
            ElseIf Inline.StartsWith("ORANGE") Then
                TargetColor = New MyColor(255, 128, 0)
                Delta = 6
            ElseIf Inline.StartsWith("DARKRED") Then
                TargetColor = New MyColor(100, 0, 0)
                Delta = 7
            ElseIf Inline.StartsWith("YELLOW") Then
                TargetColor = New MyColor(255, 255, 0)
                Delta = 6
            ElseIf Inline.StartsWith("GREEN") Then
                TargetColor = New MyColor(0, 255, 0)
                Delta = 5
            ElseIf Inline.StartsWith("DARKBLUE") Then
                TargetColor = New MyColor(0, 0, 150)
                Delta = 8
            ElseIf Inline.StartsWith("BLUE") Then
                TargetColor = New MyColor(0, 0, 255)
                Delta = 4
            ElseIf Inline.StartsWith("GRAY") Then
                TargetColor = New MyColor(160, 160, 160)
                Delta = 4
            ElseIf Inline.StartsWith("DARKGRAY") Then
                TargetColor = New MyColor(100, 100, 100)
                Delta = 8
            Else
                TargetColor = New MyColor(255, 255, 255)
                Inline += "未知的颜色"
            End If
            Dim RealString As String = Inline.Substring(Delta)
            '判断是否有禁用的字母
            Dim HasDisabledLetter As Boolean = False
            For Each Letter In RealString
                If DisabledKey.Contains(Letter) Then
                    HasDisabledLetter = True
                    Exit For
                End If
            Next
            If Not HasDisabledLetter Then
                '无禁用字，直接添加
                Target.Inlines.Add(New Run(StrConv(RealString, VbStrConv.Wide)) With {.Foreground = TargetColor})
            Else
                '有禁用字，逐个添加
                For Each Letter In RealString
                    If Letter = vbCr Then Continue For
                    If Letter = vbLf Then Letter = vbCrLf
                    If DisabledKey.Contains(Letter) Then
                        '修改字符与颜色
                        Dim RealTargetColor As MyColor
                        If Inline.StartsWith("YELLOW") Then
                            If RandomInteger(1, 10) > 5 Then Letter = Encoding.Default.GetString({RandomInteger(16 + 160, 87 + 160), RandomInteger(1 + 160, 89 + 160)})
                            RealTargetColor = TargetColor * RandomInteger(40, 70) * 0.01
                        Else
                            If RandomInteger(1, 10) > 9 Then Letter = Encoding.Default.GetString({RandomInteger(16 + 160, 87 + 160), RandomInteger(1 + 160, 89 + 160)})
                            RealTargetColor = TargetColor * ((RandomInteger(90, 100) * 0.01) ^ 2)
                        End If
                        Target.Inlines.Add(New Run(StrConv(Letter, VbStrConv.Wide)) With {.Foreground = RealTargetColor})
                    Else
                        Target.Inlines.Add(New Run(StrConv(Letter, VbStrConv.Wide)) With {.Foreground = TargetColor})
                    End If
                Next
            End If
        Next
    End Sub
    '获取全角处理后的纯文本
    Public Function GetRawText(Text As String) As String
        Return StrConv(Text.Replace("\n", vbCrLf), VbStrConv.Wide)
    End Function

    '获取根据按键处理后的富文本
    Public Function GetKeyText(Key As String) As String
        Return "\YELLOW<" & Key & ">\WHITE"
    End Function
    '获取单个项目的富文本
    Public Function GetItemText(Key As String, Title As String, Desc As String) As String
        Return GetKeyText(Key) & " " & Title & vbCrLf & "    " & Desc
    End Function

    '玩家输入指令
    Public EnterStatus As EnterStatuses = EnterStatuses.Normal
    Public Enum EnterStatuses
        Normal
        Chat
    End Enum
    Public Sub Enter(Input As String)
        Select Case EnterStatus
            Case EnterStatuses.Normal
                SetText(FrmMain.TextInputResult, "\DARKGRAY等待玩家输入指令。")
                If Input = "RST" Then
                    StartLevel(Level)
                    StartChat({"* 本场战斗已重置！"}, False)
                    Exit Sub
                ElseIf Input.StartsWith("TP ") Then
                    Level = Input.Replace("TP ", "")
                    StartLevel(Level)
                    Exit Sub
                End If
                Select Case Screen
                    Case Screens.Empty
                    Case Screens.Combat
                        Select Case Input
                            Case "ATK"
                                Screen = Screens.Select
                                ScreenData = "ATK"
                                ScreenTitle = "攻击"
                                Exit Sub
                            Case "DEF"
                                ExtraDef += 100
                                StartChat({"* 你做好迎接敌人攻击的准备。\n  你在本回合内的防御略微提高了！", "/TURNEND"}, True)
                                Exit Sub
                            Case "MAG"
                                Screen = Screens.Magic
                                Exit Sub
                            Case "EQU"
                                Screen = Screens.Equip
                                Exit Sub
                            Case "ITM"
                                Screen = Screens.Item
                                Exit Sub
                            Case "WIN"
                                PerformLevelWin(Level)
                                Exit Sub
                        End Select
                    Case Screens.Select
                        '选取对象
                        Select Case Input
                            Case "A", "B", "C", "D", "E", "F", "G"
                                Dim Id As Integer = "ABCDEFG".IndexOf(Input)
                                If Id >= 0 AndAlso Id <= MonsterHp.Count - 1 Then
                                    '选中对象：Id
                                    PerformSelect(Id)
                                    Exit Sub
                                End If
                            Case "ESC"
                                Screen = Screens.Combat
                                Exit Sub
                        End Select
                    Case Screens.Equip
                        '装备
                        Select Case Input
                            Case "1", "2", "3", "4", "5", "6", "7"
                                If EquipArmor = Input OrElse EquipWeapon = Input Then
                                    SetText(FrmMain.TextInputResult, "\RED你已装备该物品！")
                                Else
                                    If GetEquipIsWeapon(Input) Then
                                        EquipWeapon = Input
                                    Else
                                        EquipArmor = Input
                                    End If
                                    StartChat({"* 你将装备的" & If(GetEquipIsWeapon(Input), "武器", "护甲") & "切换为了" & GetEquipTitle(Input) & "！", "/TURNEND"}, True)
                                End If
                                Exit Sub
                            Case "ESC"
                                Screen = Screens.Combat
                                Exit Sub
                        End Select
                    Case Screens.Item
                        '道具
                        Select Case Input
                            Case "1", "2", "3", "4", "5", "6", "7"
                                If ItemCount(Input) = 0 Then
                                    SetText(FrmMain.TextInputResult, "\RED该道具槽位为空！")
                                Else
                                    UseItem(Input)
                                End If
                                Exit Sub
                            Case "ESC"
                                Screen = Screens.Combat
                                Exit Sub
                        End Select
                    Case Screens.Magic
                        '法术
                        Select Case Input
                            Case "1", "2", "3", "4", "5", "6", "7"
                                If Mp < GetMagicCost(Input) Then
                                    SetText(FrmMain.TextInputResult, "\RED你的法力值不足！")
                                Else
                                    UseMagic(Input)
                                End If
                                Exit Sub
                            Case "ESC"
                                Screen = Screens.Combat
                                Exit Sub
                        End Select
                End Select
                SetText(FrmMain.TextInputResult, "\RED错误：指令未知或无效，请输入屏幕上以黄色显示的指令！")
            Case Else
                SetText(FrmMain.TextInputResult, "\RED未知的输入状态！")
        End Select
    End Sub

    '对话框
    Private ChatContents As New List(Of String)
    Public Sub StartChat(Contents As String(), RequireEnsure As Boolean)
        ChatContents = New List(Of String)(Contents)
        If RequireEnsure Then
            EnterStatus = EnterStatuses.Chat
            FrmMain.TextInputBox.Tag = ""
        End If
        FrmMain.TextChat.Text = ""
        FrmMain.TextChat.Tag = ""
        NextChat()
    End Sub
    Public Sub StartOrAddChat(Contents As String(), RequireEnsure As Boolean)
        If EnterStatus = EnterStatuses.Chat AndAlso RequireEnsure Then
            '都需要确认，追加
            ChatContents.AddRange(Contents)
        ElseIf EnterStatus = EnterStatuses.Chat AndAlso Not RequireEnsure Then
            '正在进行更重要的对话，忽略
        Else
            '未进行对话
            StartChat(Contents, RequireEnsure)
        End If
    End Sub
    Public Sub NextChat()
        AniStop("Chat Content")
        If FrmMain.TextChat.Text <> FrmMain.TextChat.Tag Then
            '补全当前对话
            FrmMain.TextChat.Text = FrmMain.TextChat.Tag
        ElseIf ChatContents.Count > 0 AndAlso Not ChatContents(0).StartsWith("/") Then
            '下一句对话
            If EnterStatus = EnterStatuses.Chat Then SetText(FrmMain.TextInputResult, "\DARKGRAY请按任意键继续。")
            FrmMain.TextChat.Foreground = If(EnterStatus = EnterStatuses.Chat, New MyColor(255, 255, 255), New MyColor(100, 100, 100))
            '处理文本
            Dim RawText As String = GetRawText(ChatContents.First)
            FrmMain.TextChat.Text = RawText
            FrmMain.TextChat.Tag = FrmMain.TextChat.Text
            '播放动画
            AniStart({
                     AaTextAppear(FrmMain.TextChat, Time:=30)
                }, "Chat Content")
            FrmMain.TextChat.Text = "" '防止动画结束前闪现
            ChatContents.RemoveAt(0)
        ElseIf ChatContents.Count > 0 Then
            '执行命令
            Dim Cmd = ChatContents(0)
            ChatContents.RemoveAt(0)
            If ChatContents.Count = 0 Then EndChat()
            If Cmd.StartsWith("/TURNEND") Then
                TurnEnd()
            ElseIf Cmd.StartsWith("/RESET") Then
                Enter("RST")
            ElseIf Cmd.StartsWith("/LOCK") Then
                DisabledKey &= Cmd.Replace("/LOCK", "")
                NextChat()
            ElseIf Cmd.StartsWith("/UNLOCK") Then
                DisabledKey = DisabledKey.Replace(Cmd.Replace("/UNLOCK", ""), "")
                NextChat()
            ElseIf Cmd.StartsWith("/LEVEL") Then
                Enter(Cmd.Replace("/LEVEL", "TP "))
            End If
        Else
            '结束对话
            EndChat()
        End If
    End Sub
    Public Sub EndChat()
        If EnterStatus = EnterStatuses.Chat Then
            EnterStatus = EnterStatuses.Normal
            SetText(FrmMain.TextInputResult, "\DARKGRAY等待玩家输入指令。")
        End If
        FrmMain.TextChat.Text = ""
        FrmMain.TextChat.Tag = ""
    End Sub

End Module
