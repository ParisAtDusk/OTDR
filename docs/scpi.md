# H1 This file contains available SCPI commands to control OTDR 

| Query | Response | Description |
| :-----------: | :----------: | :-----------: |
| *IDN?               | ASCII: Manufacturer,Model,SN,Firmware "AGH,OTDR,001,1.0"| Identification        |
| *RST                | -                                                       | Resets instrument     |
| :ACQuire:START      | -                                                       | Starts measurement    |
| :ACQuire:AVERage    | -                                                      | Averaging                    |
| :ACQuire:PULse      | -                                                      | Laser on time in ns                    |
| :STATus:OPERation?  | ASCII: ACQUIRING/COMPLETE/READY/ERROR                   | Status register clears after reading COMPLETE |
| :TRACE:DATA?        | Binary measurement data                                 | Measurement result |


Binary data format options
FORMat
:BORDer NORMal|SWAPped
[:DATA] <type> [,<length>]
:DINTerchange <Boolean>
:SREGister ASCii | BINary | HEXadecimal | OCTal
