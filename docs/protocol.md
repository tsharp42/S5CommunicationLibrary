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
| Header | RFA | RFB | Audio | Battery | Flags1 |   FREQ |?????

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

FREQ: Current receiver frequency packed as integers?
    0x52 0x32 0x4B = 82,50,75 = 825.075
```

Captured
```
>> 52 00 44
<< 52 00 44 46 44 78 0d cb 60 52 1f 00 13 de [User 1 - 823.100]

>> 52 00 44
<< 52 00 44 5a 5c 78 0c 72 60 53 00 16 13 fc [SINGLE]

>> 52 00 44
<< 52 00 44 52 53 78 0c 72 60 53 04 00 13 de [BK1.01 - 830.400]
```

## Request Full Status ✅
Send
```
0x52 0x00 0x3F
```

Receive
```
 52 00 21 13 00 00 00 00 05 52 32 4b 55 73 65 72 31 37 61
| Header |FW|           |ML| FREQ   |      NAME       | CHK
|-----------------------CHECKSUM----------------------|  ^
 
 
 Header: Static
 FW: Firmware Version?
     0x13 = 19 = V1.9
     0x11 = 17 = V1.7
     0x01... V1.6?! - Maybe this was meant to be 0x10 = 16 = V1.6

 ML: Mute Level
     High 4 Bits, 0x01 To 0x0a (1->10)
     Low 4 Bits: Unknown

 FREQ: Current receiver frequency packed as integers?
     0x52 0x32 0x4B = 82,50,75 = 825.075
     
 Name: ASCII, 6 Chars, Current Name
 CHK - Modulo 256
```

Captured
```
>> 52 00 3f
<< 52 00 21 13 00 00 00 00 85 52 1f 00 55 73 65 72 31 37 83
<< 52 00 21 01 00 00 00 00 05 52 1f 00 55 73 65 72 20 31 da 
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

Captured
```
>> 52 55
<<
0000  44 ca 14 55 73 65 72 20 31 52 1f 00 05 55 73 65 72 20 32 52 22 4b 05 55 73 65 72 20 33 52 27 4b 05 55  D..User 1R...User 2R"K.User 3R'K.U
0022  73 65 72 20 34 52 2e 00 05 55 73 65 72 20 35 52 32 00 05 55 73 65 72 20 36 52 37 19 05 55 73 65 72 20  ser 4R...User 5R2..User 6R7..User 
0044  37 52 3e 19 05 55 73 65 72 20 38 52 42 4b 05 55 73 65 72 20 39 52 48 4b 05 55 73 65 72 31 30 52 50 4b  7R>..User 8RBK.User 9RHK.User10RPK
0066  05 55 73 65 72 31 31 52 56 19 05 55 73 65 72 31 32 52 61 19 05 55 73 65 72 31 33 53 04 32 05 55 73 65  .User11RV..User12Ra..User13S.2.Use
0088  72 31 34 53 13 00 05 55 73 65 72 31 35 56 1f 00 05 55 73 65 72 31 36 56 22 00 05 55 73 65 72 31 37 56  r14S...User15V...User16V"..User17V
00aa  25 32 05 55 73 65 72 31 38 56 2a 19 05 55 73 65 72 31 39 56 2d 32 05 55 73 65 72 32 30 56 31 4b 05 55  %2.User18V*..User19V-2.User20V1K.U
00cc  73 65 72 31 33 53 04 32 05 55 73 65 72 31 34 53 13 00 05 55 73 65 72 31 35 56 1f 00 05 55 73 65 72 31  ser13S.2.User14S...User15V...User1
00ee  36 56 22 00 05 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  6V"...............................
0110  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ..................................
0132  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ..................................
0154  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ..................................
0176  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 74              .............................t     
```

Observations
- Even with the Trantec-Sourced software, running this command seems to select a random(?) user preset once complete. In testing, a receiver set to "User 1" would land on "User 17".
- Having a higher User preset selected when sending this command will result in the receiver trying to load random memory as a preset, corrupting the display? (RX VER 1.9)
- Receiver version 1.6 will crash and reboot after the above

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

Captured
```
>> 
0000  52 53 01 55 14 55 73 65 72 5f 31 52 1f 00 05 55 73 65 72 5f 32 52 22 4b 05 55 73 65 72 5f 33 52 27 4b  RS.U.User_1R...User_2R"K.User_3R'K
0022  05 55 73 65 72 5f 34 52 2e 00 05 55 73 65 72 5f 35 52 32 00 05 55 73 65 72 5f 36 52 37 19 05 55 73 65  .User_4R...User_5R2..User_6R7..Use
0044  72 5f 37 52 3e 19 05 55 73 65 72 5f 38 52 42 4b 05 55 73 65 72 5f 39 52 48 4b 05 55 73 65 72 31 30 52  r_7R>..User_8RBK.User_9RHK.User10R
0066  50 4b 05 55 73 65 72 31 31 52 56 19 05 55 73 65 72 31 32 52 61 19 05 55 73 65 72 31 33 53 04 32 05 55  PK.User11RV..User12Ra..User13S.2.U
0088  73 65 72 31 34 53 13 00 05 55 73 65 72 31 35 56 1f 00 05 55 73 65 72 31 36 56 22 00 05 55 73 65 72 31  ser14S...User15V...User16V"..User1
00aa  37 56 25 32 05 55 73 65 72 31 38 56 2a 19 05 55 73 65 72 31 39 56 2d 32 05 55 73 65 72 32 30 56 31 4b  7V%2.User18V*..User19V-2.User20V1K
00cc  05
```

## Set Mute Level & "PcMute" ?
Send
```
 52 00 6d 00 00 00 06 00 00 00 00 00 00
| Header |   ?    |ML|        ?  |PC| ?

ML: Mute Level, 0x01 To 0x0a (1->10)
PC Mute: 40 = ON, 00 = OFF (Maybe flag byte?)

On V1.9 of the RX Firmware, the display will show "PcMute" when enabled
V1.6 & V1.7 Do not give any indication PcMute is enabled, If it is supported at all.
```

Captured
```
<< 52 00 6d 00 00 00 05 00 00 00 00 00 00 - PcMute Off
<< 52 00 6d 00 00 00 05 00 00 00 00 40 00 - PcMute On
```

Observations
- The receivers have no error checking for input data. Incorrect setting of ML byte nearly bricked a V1.9 Receiver. 

## Set Frequency
Send
```
 52 00 49 53 0c 19 05 00 00 00 01 00 7e
| HEADER | FREQ   |ML|        |     |CHK
         |--------CHECKSUM----------| ^

FREQ: Frequency packed as integers?
    0x52 0x32 0x4B = 82,50,75 = 825.075
ML: Mute Level, 0x01 To 0x0a (1->10)
CHK = Modulo 256
```

Captured
```
<< 52 00 49 53 0c 19 05 00 00 00 01 00 7e - SET 831.225
<< 52 00 49 53 00 00 05 00 00 00 01 00 59 - SET 830.000
<< 52 00 49 53 16 19 06 00 00 00 01 00 89 - SET 832.225 - ML 6
```

## Set Name
Send
```
 52 00 4d 41 42 43 08 44 45 46 00 00 00
| HEADER |C1 C2 C3|ML|C4 C5 C6|

C1-6 - ASCII Chars of new name
ML: Mute Level, 0x01 To 0x0a (1->10)
 
```

Captured
```
<< 52 00 4d 41 42 43 08 44 45 46 00 00 00 - Set to "ABCDEF" - ML 8
<< 52 00 4d 31 32 33 09 34 35 36 00 00 00 - Set to "123456" - ML 9
```
