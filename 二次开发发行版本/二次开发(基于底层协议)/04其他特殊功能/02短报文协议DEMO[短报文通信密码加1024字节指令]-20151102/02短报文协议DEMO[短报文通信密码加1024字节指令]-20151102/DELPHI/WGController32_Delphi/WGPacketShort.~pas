unit WGPacketShort;
interface
    function ShortEncrypt(command:PChar;password:PChar):Integer;Stdcall;
    function ShortDecrypt(command:PChar;password:PChar):Integer;Stdcall;

const
	    WGPacketSize = 64;			    //报文长度
	    WGPacketType = $17; //2015-04-29 23:26:08 $19;					//类型
	    ControllerPort = 60000;        //控制器端口
	    SpecialFlag = $55AAAA55;       //特殊标识 防止误操作
implementation
    function ShortEncrypt;external 'n3kWGCom.dll' name 'ShortEncrypt';    //
    function ShortDecrypt;external 'n3kWGCom.dll' name 'ShortDecrypt';    //


end.
