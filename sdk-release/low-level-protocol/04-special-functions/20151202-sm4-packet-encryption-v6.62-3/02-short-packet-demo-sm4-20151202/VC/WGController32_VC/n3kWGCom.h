#ifndef N3KWGCOM_H
#define N3KWGCOM_H
extern "C"_declspec(dllimport) int __stdcall  ShortEncryptSM4_ECB(char* command,int cmdLen, char* password);  //2015-09-28 12:05:54 SM4 ECB 加密WGPacketShort 包 短报文
extern "C"_declspec(dllimport) int __stdcall  ShortDecryptSM4_ECB(char* command,int cmdLen, char* password);  //2015-09-28 12:05:54 SM4 ECB 解密WGPacketShort 包 短报文
extern "C"_declspec(dllimport) int __stdcall  ShortEncryptSM4_CBC(char* command,int cmdLen, char* password, char* IV);  //2015-09-28 12:05:54 SM4 CBC 加密WGPacketShort 包 短报文
extern "C"_declspec(dllimport) int __stdcall  ShortDecryptSM4_CBC(char* command,int cmdLen, char* password, char* IV);  //2015-09-28 12:05:54 SM4 CBC 解密WGPacketShort 包 短报文
#endif