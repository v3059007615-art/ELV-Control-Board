unit WGPacketShort;
interface
    function ShortEncrypt(command:PChar;password:PChar):Integer;Stdcall;
    function ShortDecrypt(command:PChar;password:PChar):Integer;Stdcall;

const
	    WGPacketSize = 64;			    //Length of submission
	    WGPacketType = $17; //2015-04-29 23:26:08 $19;					//Type
	    ControllerPort = 60000;        //controller port
	    SpecialFlag = $55AAAA55;       //Special identification Prevent mishandling
implementation
    function ShortEncrypt;external 'n3kWGCom.dll' name 'ShortEncrypt';    //
    function ShortDecrypt;external 'n3kWGCom.dll' name 'ShortDecrypt';    //


end.
