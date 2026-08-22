object Form1: TForm1
  Left = 905
  Top = 191
  Width = 586
  Height = 716
  Caption = 'Form1'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 296
    Top = 16
    Width = 18
    Height = 13
    Caption = 'SN:'
  end
  object Label2: TLabel
    Left = 296
    Top = 40
    Width = 13
    Height = 13
    Caption = 'IP:'
  end
  object Label3: TLabel
    Left = 232
    Top = 72
    Width = 79
    Height = 13
    Caption = 'Watch Server IP'
  end
  object Label4: TLabel
    Left = 224
    Top = 104
    Width = 88
    Height = 13
    Caption = 'Watch Server Port'
  end
  object Button1: TButton
    Left = 80
    Top = 16
    Width = 137
    Height = 25
    Caption = 'Test Basic Function'
    TabOrder = 0
    OnClick = Button1Click
  end
  object Memo1: TMemo
    Left = 32
    Top = 144
    Width = 513
    Height = 521
    Lines.Strings = (
      'Memo1')
    TabOrder = 1
  end
  object Edit1: TEdit
    Left = 320
    Top = 16
    Width = 121
    Height = 21
    TabOrder = 2
    Text = '229999901'
  end
  object Edit2: TEdit
    Left = 320
    Top = 40
    Width = 121
    Height = 21
    TabOrder = 3
    Text = '192.168.168.123'
  end
  object Edit3: TEdit
    Left = 320
    Top = 72
    Width = 121
    Height = 21
    TabOrder = 4
    Text = '192.168.168.101'
  end
  object Edit4: TEdit
    Left = 320
    Top = 104
    Width = 121
    Height = 21
    TabOrder = 5
    Text = '61005'
  end
  object IdUDPClient1: TIdUDPClient
    Port = 0
    Left = 16
    Top = 8
  end
  object IdUDPServer1: TIdUDPServer
    Bindings = <>
    DefaultPort = 0
    Left = 136
    Top = 56
  end
end
