# ELV-Control-Board

ELV Control Board SDK (WG / AT8000 short-packet protocol).

Chinese folder and document names were translated to English. SDK source files, project files, and libraries (`Form1.cs`, `ace/`, `n3kAdrtC.dll`, and so on) were not renamed.

## Layout

```text
sdk-release/
  Access-Control-Development-Guide-V3.6.doc
  How-to-Find-SDK-Documents-20160926.doc
  SDK-Tools-Usage-20160926.doc
  low-level-protocol/
    01-short-packet-protocol-basic/     # 64-byte UDP protocol
    02-short-packet-protocol-demo/      # C#, VB.NET, Java, C++, Delphi, Android, iOS
    03-short-packet-protocol-extended/  # time zones, alarms, expansion board 0xC6
    04-special-functions/               # encryption, elevator relays, Linux broadcast
  20170912-protocol-additions/          # expansion-board remote control and status
  2017-qrcode-cloud-server-example/
  access-software-integration/          # SQL / OA / N3000 software-side integration
```

Controller transport: UDP port 60000, 64-byte packets, type `0x17`.
