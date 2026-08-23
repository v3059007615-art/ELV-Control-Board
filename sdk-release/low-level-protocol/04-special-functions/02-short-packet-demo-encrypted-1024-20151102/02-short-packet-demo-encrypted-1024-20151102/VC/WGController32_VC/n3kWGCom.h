#ifndef N3KWGCOM_H
#define N3KWGCOM_H
extern "C"_declspec(dllimport) int __stdcall  ShortEncrypt(char* command,char* password);  //2015-09-28 12:05:54 EncryptionWGPacketShort Package Shortcasts
extern "C"_declspec(dllimport) int __stdcall  ShortDecrypt(char* command,char* password);  //2015-09-28 12:05:54 DecryptWGPacketShort Package Shortcasts
#endif