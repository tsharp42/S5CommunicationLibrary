Tested with Ubuntu Server 23.04 LTS on a cheap "Z83" Atom based mini PC

Install Ubuntu Server, select the SSH server option during install, I used a username of "trantec"

Once booted, connect over SSH or log in locally as the user you setup during install. I suggest SSH as it makes copying these commands easier.

### Driver Configuration
Load the ftdi-sio driver on boot:
```sh
sudo nano /etc/modules-load.d/ftdi.conf
```
Enter `ftdi-sio` and save.

Configure the driver on boot to use the Trantec VID/PID combination:
```sh
sudo nano /etc/udev/rules.d/99-ftdi.rules
```

Enter this and save:
```
ACTION=="add", ATTRS{idVendor}=="0304", ATTRS{idProduct}=="c719", RUN+="/sbin/modprobe ftdi_sio" RUN+="/bin/sh -c 'echo 0304 c719 > /sys/bus/usb-serial/drivers/ftdi_sio/new_id'"
```

Reboot, at this point the receivers should be enumerated as serial devices as /dev/ttyUSB{0}

You can validate this with some simple commands
```sh
sudo dmesg | grep -A7 Trantec
```

Which should return some lines like this, note "now attached to ttyUSB0"
'''
[ 1106.624503] usb 3-1: Product: Trantec Systems receiver
[ 1106.624509] usb 3-1: Manufacturer: BBM
[ 1106.624513] usb 3-1: SerialNumber: 39FQFZ2P
[ 1106.629843] ftdi_sio 3-1:1.0: FTDI USB Serial Device converter detected
[ 1106.629896] usb 3-1: Detected FT232R
[ 1106.630768] usb 3-1: FTDI USB Serial Device converter now attached to ttyUSB0
'''

You can also confirm they exist by checking the device entries:
```sh
ls -lh /dev/ttyU*
```

Which should return a list like this:
```
crw-rw---- 1 root dialout 188, 0 Sep  5 19:25 /dev/ttyUSB0
crw-rw---- 1 root dialout 188, 1 Sep  5 19:24 /dev/ttyUSB1
crw-rw---- 1 root dialout 188, 2 Sep  5 19:24 /dev/ttyUSB2
crw-rw---- 1 root dialout 188, 3 Sep  5 19:24 /dev/ttyUSB3
```

The devices are now configured for use as serial devices, next you need to give your user permissions to use them:
```sh
sudo usermod -a -G dialout $USER
```

Logout, and Log back in. And you are done.


### Install DotNet 6.0
```sh
sudo apt update
sudo apt install dotnet-runtime-6.0 dotnet-sdk-6.0
```

### Launch Manually
Download the latest release package and extract it to your user home directory.
```sh
wget URL_TO_RELEASE
unzip FILE
```

Then, modify the receivers.txt to set up which ports to open:
```
/dev/ttyUSB0
/dev/ttyUSB1
/dev/ttyUSB2
/dev/ttyUSB3
```

Launch the application manually from the command line, and then navigate to the IP address of the server to confirm everything is working. Use Ctrl+C to end the server once tested.
```
dotnet S5CommunicationLibrary.Web.dll --urls http://0.0.0.0:5000
```

### Launch Automatically

Find where you have located the application:
```sh
pwd
```
```
/home/trantec/publish
```

Create a service entry
```sh
sudo nano /etc/systemd/system/trantec.service
```

Contents, note the WorkingDirectory
```
[Unit]
Description=Trantec Server

Wants=network.target
After=syslog.target network-online.target

[Service]
Type=simple
WorkingDirectory=/home/trantec/publish
ExecStart=dotnet S5CommunicationLibrary.Web.dll --urls http://0.0.0.0:5000
Restart=on-failure
RestartSec=10
KillMode=process

[Install]
WantedBy=multi-user.target
```

Then
```sh
sudo systemctl daemon-reload
sudo systemctl enable trantec.service
```

And finally
```sh
sudo systemctl start trantec.service
```

At this point, the applicaiton should start on boot and automatically connect to the specified receivers in receivers.txt