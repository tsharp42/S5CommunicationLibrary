A C# Library for control of Trantec S5.3 and S5.5 Receivers over USB

## Drivers
The drivers supplied for the receivers do not provide a Serial Port device, you will need to force load the FTDI drivers or find some other workaround to have the receiver enumerate as a standard serial device.

I recommend sticking with Linux as this does not require any modification to the receiver or drivers.

### [Windows] Modify the receiver
Use the FTDI configuration tool (FT_PROG) to reconfigure the USB VID and PID to the defaults (0403/6001) so the standard FTDI driver works.
This will break compatibility with the manufacturer software.
This has the potential to completely brick the USB functionality if done incorrectly

FT_PROG - https://ftdichip.com/utilities/

### [Windows] Modify the driver
Modify the FTDI drivers with the VID/PID combination of 0304/C719. You will probably also need to disable driver signing among other things.

### [Linux] Force driver load (Once) - Tested with Ubuntu 22.04 LTS
Force load the ftdi-sio driver for the VID/PID combination of 0304/C719. This will revert after a reboot
```sh
modprobe ftdi-sio
echo 0304 c719 > /sys/bus/usb-serial/drivers/ftdi_sio/new_id
```

### [Linux] Force driver load (On Boot) - Tested with Ubuntu 22.04 LTS
Create this file:
```sh
sudo nano /etc/udev/rules.d/99-ftdi.rules
```
Enter this into the file:
```
ACTION=="add", ATTRS{idVendor}=="0304", ATTRS{idProduct}=="c719", RUN+="/sbin/modprobe ftdi_sio" RUN+="/bin/sh -c 'echo 0304 c719 > /sys/bus/usb-serial/drivers/ftdi_sio/new_id'"
```
Reboot

---

## Disclaimer
**Please note:** This is a third party implementation of the protocol entirely derived from watching protocol data fly across the serial bus. It is in no way connected to, or endorsed by, Trantec or any related parties. Use of the information and software contained within this repository is entirely at your own risk.


There is a very real risk that some commands may do things to the receiver that can't easily be reversed, some commands write to the EEPROM and it appears as though there's very little error checking on the receiver side. As such, features that _write_ to the receiver have been disabled in release builds until they can be proven safe. You have been warned.
