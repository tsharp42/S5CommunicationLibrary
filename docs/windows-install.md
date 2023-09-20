The drivers supplied for the receivers do not provide a Serial Port device, you will need to force load the FTDI drivers or find some other workaround to have the receiver enumerate as a standard serial device.

Two main options:
 - Modify the driver to accept the VID/PID combination of 0304/C719
 - Modify the receiver to use the default FTDI VID/PID

# Modify the receiver
Use the FTDI configuration tool (FT_PROG) to reconfigure the USB VID and PID to the defaults (0403/6001) so the standard FTDI driver works.
This will break compatibility with the manufacturer software.
This has the potential to completely brick the USB functionality if done incorrectly

FT_PROG - https://ftdichip.com/utilities/

# Modify the driver
Modify the FTDI drivers with the VID/PID combination of 0304/C719. You will probably also need to disable driver signing among other things.
