# Hardware
* Internally, FTDI232L USB UART
  * VID 0304, PID C719
  * 57600, 8N1

# Protocol

## Request Metering Data
Send
```
0x52 0x00 0x44
```

Receive
```
 52 00 44   46    40   77 0d      72       60    52 48 00 13 00
| Header | RFA | RFB | Audio | Battery | Flags1 | ?????????????

Header: Static
RFA: RFA Level
RFB: RFB Level
Audio: Int16 audio Level
Battery: Battery Level

Flags1
-------------
[1]
[2]
[3]
[4]
[5]
[6] - Muted
[7]
[8] - Antenna, 1 = A, 0 = B
```
