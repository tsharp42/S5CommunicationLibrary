A C# Library and .Net Core Web App for control of Trantec S5.3 and S5.5 Receivers over USB, this has been developed and tested with version 1.9 of the receiver firmware.

⚠️ This implementation is **very** experimental. The receivers do almost no error checking. While the main status commands seem fairly harmless, commands that write data to the receiver have the ability to completely brick the receiver. This may be difficult to recover. All complex write commands are disabled in release builds for this reason. ⚠️

## Documentation
- [Protocol Specification](./docs/protocol.md)
- [Linux Installation - Ubuntu Server 22.04 LTS](./docs/linux-install.md)
- [Windows Installation - Not Recommended](./docs/windows-install.md)

## Disclaimer ⚠️
**Please note:** This is a third party implementation of the protocol entirely derived from watching protocol data fly across the serial bus. It is in no way connected to, or endorsed by, Trantec or any related parties. Use of the information and software contained within this repository is entirely at your own risk.

There is a very real risk that some commands may do things to the receiver that can't easily be reversed, some commands write to the EEPROM and it appears as though there's very little error checking on the receiver side. **As such, features that _write_ to the receiver have been disabled in release builds until they can be proven safe.** You have been warned.
