A C# Library for control of Trantec S5.3 and S5.5 Receivers over USB

## Drivers
The drivers supplied for the receivers do not provide a Serial Port device, you will need to force load the FTDI drivers or find some other workaround to have the receiver enumerate as a standard serial device.

### [Windows] Modify the receiver
Use the FTDI configuration tool (FT_PROG) to reconfigure the USB VID and PID to the defaults (0403/6001) so the standard FTDI driver works.
This will break compatibility with the manufacturer software.
This has the potential to completely brick the USB functionality if done incorrectly

### [Windows] Modify the driver
Modify the FTDI drivers with the VID/PID combination of 0304/C719. You will probably also need to disable driver signing among other things.

### [Linux] Force driver load
Force load the ftdi-sio driver for the VID/PID combination of 0304/C719
```sh
modprobe ftdi-sio
echo 0304 c719 > /sys/bus/usb-serial/drivers/ftdi_sio/new_id
```

---

## Disclaimer
**Please note:** This is a third party implentation of the protocol entirely derived from watching protocol data fly across the serial bus. It is in no way connected to, or endorsed by, Trantec or any related parties. Use of the information and software contained within this repository is entirely at your own risk.

