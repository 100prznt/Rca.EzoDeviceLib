## Project under construction :construction:
```diff
! This project is currently  (2019/09/16) under construction.
! Currently only the pH and the ORP EZO™ device are supported, RTD is in progress.
```

---

![EZO Devices on the Whitebox carrier](docu/img/ezo-on-whitebox-carrier.jpg "Atlas Scientific EZO™ devices (pH and ORP) on the Whitebox carrier.")
# EzoDeviceLib
Open source UWP library for communication with Atlas Scientific EZO™ devices. This library covers all I2C commands. You can trigger measurements, set temperature compensation, get device info and state, due the calibration, import/export calibration data, etc.

This library targets __UWP IoT projects__! Download directly from NuGet [Rca.EzoDeviceLib on NuGet](https://nuget.org/packages/Rca.EzoDeviceLib).

* Support for EZO™ pH device
* Support for EZO™ ORP device


[![Bulid](https://img.shields.io/appveyor/ci/100prznt/rca-ezodevicelib.svg?logo=appveyor&style=popout-square)](https://ci.appveyor.com/project/100prznt/rca-ezodevicelib)   [![Current version](https://img.shields.io/nuget/v/Rca.EzoDeviceLib.svg?logo=nuget&logoColor=%23ef8b00&style=popout-square)](https://www.nuget.org/packages/Rca.EzoDeviceLib/)   [![Code size](https://img.shields.io/github/languages/code-size/100prznt/Rca.EzoDeviceLib.svg?logo=github&style=popout-square)](#) 


## How To install?
Download the source from GitHub or get the compiled assembly from NuGet [Rca.EzoDeviceLib on NuGet](https://nuget.org/packages/Rca.EzoDeviceLib).

[![Current version](https://img.shields.io/nuget/v/Rca.EzoDeviceLib.svg?logo=nuget&logoColor=%23ef8b00&style=popout-square)](https://www.nuget.org/packages/Rca.EzoDeviceLib/)   [![NuGet](https://img.shields.io/nuget/dt/Rca.EzoDeviceLib.svg?logo=nuget&logoColor=%23ef8b00&style=popout-square)](https://www.nuget.org/packages/Rca.EzoDeviceLib/)


## How to use?
Some basic usage examples

### Create an sensor instance
In this example is the I2C address of conneted EZO™ device set to default (0x63 for EZO™ pH Circuit):
```cs
var myEzoPhSensor = new PhSensor();
```

Or create an instance with custom parameters:
```cs
var myEzoPhSensor = new PhSensor(0x1A) //set specific I2C address (0x1A) of connected EZO device
{
	BusSpeed = I2cBusSpeed.FastMode //default is StandardMode
};
```

	
### Perform and read measurement
```cs
double ph = myEzoPhSensor.GetMeasValue();
```

With tempreature compensation:

```cs
double temperature = 23.5; //temperature in °C
double phCompensated = myEzoPhSensor.GetMeasValue(temperature);
```


## Hardware
For fast hardware integration there is a cool project from [Whitebox](https://github.com/whitebox-labs). The [Tentacle T3 HAT](https://github.com/whitebox-labs/tentacle-raspi-oshw) accepts 3 EZO devices, two of them are electrically isolated.


## Credits
This library is made possible by contributions from:
* [Elias Rümmler](http://www.100prznt.de) ([@rmmlr](https://github.com/rmmlr)) - core contributor

## License
Rca.EzoDeviceLib is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to [LICENSE.txt](https://github.com/100prznt/EzoDeviceLib/blob/master/LICENSE.txt) for more information.

## Contributions
Contributions are welcome. Fork this repository and send a pull request if you have something useful to add.


[![Bulid](https://img.shields.io/appveyor/ci/100prznt/rca-ezodevicelib.svg?logo=appveyor&style=popout-square)](https://ci.appveyor.com/project/100prznt/rca-ezodevicelib)


## Related Projects
* [Rca.Sht85Lib](https://github.com/100prznt/Rca.Sht85Lib) - Another sensor library for the Sensirion humidity sensor SHT85.
* [OpenPoolControl](https://github.com/100prznt/opc) - My very own pool controller.
