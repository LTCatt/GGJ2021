﻿Public Module ModGame
    Public FrmMain As MainWindow

    '存档数据
    Public DisabledKey As String = ""
    Public Hp As Integer = 1, HpMax As Integer = 3652
    Public Mp As Integer = 1, MpMax As Integer = 638
    Public BaseAtk As Integer = 505, BaseDef As Integer = 276
    Public ItemCount As Integer() = {0, 999, 999, 999, 0, 99, 9, 999}
    Public EquipWeapon As Integer = 1, EquipArmor As Integer = 2
    Public Level As Integer = 1
    Public MonsterType As New List(Of String), MonsterName As New List(Of String), MonsterHp As New List(Of Integer), MonsterSp As New List(Of Integer)

    '原始存档
    Public ItemCountLast As Integer() = {0, 999, 999, 999, 0, 99, 9, 999}
    Public EquipWeaponLast As Integer = 1, EquipArmorLast As Integer = 2

    '修改存档
    Public Function GetRealAtk() As Integer
        Return BaseAtk + GetEquipData(EquipWeapon)
    End Function
    Public Function GetRealDef() As Integer
        Return BaseDef + GetEquipData(EquipArmor)
    End Function
    Public Function StartLevel(Id As Integer) As Integer
        Screen = Screens.Combat
        StartChat(GetLevelIntro(Id), False)
        '重置存档
        ItemCount = ItemCountLast
        EquipWeapon = EquipWeaponLast
        EquipArmor = EquipArmorLast
        Hp = HpMax
        Mp = MpMax
        '初始化怪物数据
        MonsterType.Clear()
        MonsterName.Clear()
        MonsterHp.Clear()
        MonsterSp.Clear()
        MonsterType.AddRange(GetLevelMonsters(Id))
        MonsterName.AddRange(GetLevelMonstersName(Id))
        For Each Monster In MonsterType
            MonsterHp.Add(GetMonsterHp(Monster))
            MonsterSp.Add(GetMonsterSp(Monster))
        Next
    End Function

    '刷新 UI
    Public Sub RefreshUI()
        '状态栏
        SetText(FrmMain.TextStatus, "勇者   LV 99   \REDHP " & Hp.ToString.PadLeft(4, " ") & "/" & HpMax.ToString.PadLeft(4, " ") & "\WHITE   ATK " & GetRealAtk.ToString.PadLeft(4, " ") & vbCrLf &
                                    "             \BLUEMP " & Mp.ToString.PadLeft(4, " ") & "/" & MpMax.ToString.PadLeft(4, " ") & "\WHITE   DEF " & GetRealDef.ToString.PadLeft(4, " "))
        SetText(FrmMain.TextStatusRight, "\GRAY" & GetLevelName(Level))
        '主要部分
        Select Case Screen
            Case Screens.Empty
                SetText(FrmMain.TextTitle, "")
                SetText(FrmMain.TextAction, "")
            Case Screens.Combat
                SetText(FrmMain.TextTitle, "\ORANGE※ 战斗 ※")
                SetText(FrmMain.TextAction,
                        GetKeyText("ATK") & " 攻击\n" &
                        GetKeyText("DEF") & " 防御\n" &
                        GetKeyText("MAG") & " 法术\n" &
                        GetKeyText("ITM") & " 道具\n" &
                        GetKeyText("EQU") & " 装备\n")
                SetText(FrmMain.TextInfo, CombatInfo)
            Case Screens.Select
                SetText(FrmMain.TextTitle, "\ORANGE※ 选择" & ScreenTitle & "目标 ※")
                SetText(FrmMain.TextAction,
                        GetKeyText("BAC") & " 返回")
                SetText(FrmMain.TextInfo, SelectInfo)
            Case Screens.Magic
                SetText(FrmMain.TextTitle, "\ORANGE※ 法术 ※")
                SetText(FrmMain.TextAction,
                        GetKeyText("BAC") & " 返回")
                SetText(FrmMain.TextInfo, MagicInfo)
            Case Screens.Item
                SetText(FrmMain.TextTitle, "\ORANGE※ 道具 ※")
                SetText(FrmMain.TextAction,
                        GetKeyText("BAC") & " 返回")
                SetText(FrmMain.TextInfo, ItemInfo)
            Case Screens.Equip
                SetText(FrmMain.TextTitle, "\ORANGE※ 装备 ※")
                SetText(FrmMain.TextAction,
                        GetKeyText("BAC") & " 返回")
                SetText(FrmMain.TextInfo, EquipInfo)
        End Select

    End Sub

    '玩家的回合结束
    Public Sub TurnEnd()
        Screen = Screens.Combat
        '检查怪物死亡

    End Sub

    '当前屏幕
    Public Screen As Screens = Screens.Magic
    Public ScreenData As String = "" '用于选取对象
    Public ScreenTitle As String = "" '用于选取对象
    Public Enum Screens
        Empty
        Combat
        Magic
        Item
        Equip
        [Select]
    End Enum

    '战斗
    Public Function CombatInfo() As String
        Dim Info As New List(Of String)
        For i = 0 To MonsterType.Count - 1
            Info.Add(GetItemText("ABCDEFG".ToCharArray()(i),
                                 MonsterName(i).PadRight(9, " ") & "\REDHP " & MonsterHp(i).ToString.PadLeft(4, " "),
                                 "\DARKGRAY" & GetMonsterDesc(MonsterType(i), MonsterSp(i))).Replace("KEY", "WHITE").Replace("YELLOW", "WHITE"))
        Next
        Return Join(Info.ToArray, vbCrLf)
    End Function

    '选取对象
    Public Function SelectInfo() As String
        Dim Info As New List(Of String)
        For i = 0 To MonsterType.Count - 1
            Info.Add(GetItemText("ABCDEFG".ToCharArray()(i),
                                 MonsterName(i).PadRight(9, " ") & "\REDHP " & MonsterHp(i).ToString.PadLeft(4, " "),
                                 "\DARKGRAY" & GetMonsterDesc(MonsterType(i), MonsterSp(i))))
        Next
        Return Join(Info.ToArray, vbCrLf)
    End Function
    Public Sub PerformSelect(Id As Integer)
        Select Case ScreenData
            Case "ATK"
                '攻击怪物
                Screen = Screens.Combat
                Dim Damage As Integer = Math.Max(1, GetRealAtk() - GetMonsterDef(MonsterType(Id)))
                MonsterHp(Id) = Math.Max(0, MonsterHp(Id) - Damage)
                StartChat({"* 你用" & GetEquipTitle(EquipWeapon) & "砍向了" & MonsterName(Id) & "！\n  造成了" & Damage & "点伤害！", "/TURNEND"}, True)
        End Select
    End Sub

    '法术
    Public Function MagicInfo() As String
        Dim Info As New List(Of String)
        For i = 1 To 7
            Info.Add(GetItemText(i,
                                 GetMagicTitle(i).PadRight(9, " ") & If(GetMagicCost(i) > Mp, "\RED", "\BLUE") & GetMagicCost(i).ToString.PadLeft(3, " ") & "MP" & If(GetMagicCost(i) > Mp, " MP不足", ""),
                                 "\DARKGRAY" & GetMagicDesc(i)))
        Next
        Return Join(Info.ToArray, vbCrLf)
    End Function

    '物品
    Public Function ItemInfo() As String
        Dim Info As New List(Of String)
        For i = 1 To 7
            If ItemCount(i) = 0 Then
                Info.Add(GetItemText(i, "\GRAY无物品", ""))
            Else
                Info.Add(GetItemText(i,
                                     GetItemTitle(i).PadRight(9, " ") & "\GRAYx" & ItemCount(i),
                                     "\DARKGRAY" & GetItemDesc(i)))
            End If
        Next
        Return Join(Info.ToArray, vbCrLf)
    End Function

    '装备
    Public Function EquipInfo() As String
        Dim Info As New List(Of String)
        For i = 1 To 7
            Info.Add(GetItemText(i,
                                 GetEquipTitle(i).PadRight(9, " ") & If(EquipArmor = i OrElse EquipWeapon = i, "\DARKBLUE <已装备>", ""),
                                 "\DARKGRAY" & GetEquipDesc(i)))
        Next
        Return Join(Info.ToArray, vbCrLf)
    End Function

End Module
