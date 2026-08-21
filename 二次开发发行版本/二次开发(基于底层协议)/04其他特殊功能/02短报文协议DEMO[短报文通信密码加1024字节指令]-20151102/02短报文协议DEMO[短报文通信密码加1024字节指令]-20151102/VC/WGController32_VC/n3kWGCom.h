#ifndef N3KWGCOM_H
#define N3KWGCOM_H
extern "C"_declspec(dllimport) int __stdcall  ShortEncrypt(char* command,char* password);  //2015-09-28 12:05:54 加密WGPacketShort 包 短报文
extern "C"_declspec(dllimport) int __stdcall  ShortDecrypt(char* command,char* password);  //2015-09-28 12:05:54 解密WGPacketShort 包 短报文
#endif