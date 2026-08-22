program WGBasicDelphiTestPrj;

uses
  Forms,
  WGBasicDelphiTest in 'WGBasicDelphiTest.pas' {Form1},
  WGPacketShort in 'WGPacketShort.pas';

{$R *.res}

begin
  Application.Initialize;
  Application.CreateForm(TForm1, Form1);
  Application.Run;
end.
