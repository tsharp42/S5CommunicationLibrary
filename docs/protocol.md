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

## Request Full Status
Send
```
0x52 0x00 0x3F
```

Receive
```
 52 00 21 13 00 00 00 00 05 52 32 4b 55 73 65 72 31 37 61
| Header | L|           |ML| FREQ   |      NAME       | CHK
 
 
 Header: Static
 L: Length of message. 0x13 = 19
 ML: Mute Level, 0x01 To 0x0a (1->10)
 FREQ: Current receiver frequency packed as integers?
     0x52 0x32 0x4B = 82,50,75 = 825.075
     
 Name: ASCII, 6 Chars, Current Name
 CHK - Modulo 256
```

## Request User Presets
Send
```
0x52 0x55
```

Receive
```
Full Message:
[2| Header][1| Count][10| Preset Data * 40][1| Checksum]

Header & Count:
 44 00 15 
| HDR | Count, 0x15 = 21

Preset Data:
 55 73 65 72 31 37 52 32 4b 01 
| Name            |  FREQ  | ML

Checksum:
1 Byte - Modulo 256 of all preceeding bytes

Name: 6 Byte ASCII string
FREQ: Current receiver frequency packed as integers?
    0x52 0x32 0x4B = 82,50,75 = 825.075
ML: Mute Level, 0x01 To 0x0a (1->10)
```

## Send User Presets
Send
```
Full Message:
[4| Header][1| Count][10| Preset Data * Count]

Header & Count
 52 53 01 55 15 ...
|           | Count (15 = 21, 21*10 Byte entries following)

Preset Data:
 55 73 65 72 31 37 52 32 4b 01 
| Name            |  FREQ  | ML

Checksum:
1 Byte - Modulo 256 of all preceeding bytes

Name: 6 Byte ASCII string
FREQ: Current receiver frequency packed as integers?
    0x52 0x32 0x4B = 82,50,75 = 825.075
ML: Mute Level, 0x01 To 0x0a (1->10)
```
