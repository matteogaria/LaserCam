# LaserCam
A 2D laser engraving/cutting oriented CAM software

## Overview
After spending some time searching for a CAM software capable of converting standard .DXF drawings into gcode suitable for mylaser cutter, I decided to write my own based on my real needs:

 * **Automatic and simple to use**
	 * Just a short command line to do all operations.
	 * Conversion settings are stored in a .json profile, LaserCam can apply different settings to each layer of the original drawing
 * **GRBL-oriented gcode output**
	 * Simple GCode generated according to Grbl's specifications
	 * Takes advantage of Grbl's Laser Mode
	 * May work with other controllers, untested.
 * **Optimizations for diode-based laser engravers/cutters**
	 * Some useful features, such as line splitting, provides best perfomance when used with typical blue diode laser

## Licensing
LaserCam is free software, released under the [GPLv3 license](https://www.gnu.org/licenses/gpl-3.0.en.html).

For specific licensing of external packages refer to original authors, see references in Technology section.

## Status
**UNDER HEAVY DEVELOPMENT**

Main functionalities are here and LaserCam can be succesfully used to generate proper gcode, but lacks some features and  attention must be paid when using it. It's a good idea to check generated files with a text editor before using it.

**What's currently missing**:
* A way to edit profiles: for the moment, just edit manually the *profiles.json* file (see profiles section below)
* Settings check: every value into *profiles.json* is treated as valid. So, be careful to not insert strange values (for istance, something like 100 decimal places in gcode, or point precision of 0.000001). Default values are a good starting point, typically you just need to optimize feedrate, power, number of passes.
* Support for machines without GRBL's Laser Mode enabled
> For now, it's strictly mandatory to enable laser mode in your machine firmware

**What's work in progress**:
* Travel optimizer: A simple travel path optimizer for rapid moves is implemented but not completely tested. If you get a strange behaviour, disable it with *--no-optimizer* command line option
* Command Line parsing is based on System.CommandLine, actually in beta version from Microsoft. Parameters works well, I've some issues on automatically generated help (see Usage section below for help)

## Usage
Just a short command line to do all operations:

    LaserCam.exe -p "profilename" -i "path_to_input.dxf" -o "path_to_output.gcode"

#### Mandatory options:
* -p, --profile: name of working profile to use, as specified in *profiles.json*
* -i, --input: path to dxf file to process, dxf version must be higher than AutoCad2000
* -o, --output: output path and filename

#### Other options:
* --no-optimizer: to disable travel path optimizer

After the initial work of optimizing profiles for performance and quality, the conversion is completely straighforward.

### IMPORTANT NOTE:
LaserCam assumes that your GRBL controller is configured with **Laser Mode** enabled! Using it without Laser Move causes the laser to remain ON also on G0 rapid moves, and prevents the usage of Dynamic laser control mode.

## Profiles
As said, LaserCam operates according to a profiling .json file, located in the application folder. A typical profile looks like this:

    [
      {
        "Name": "hardply-3mm",
        "DecimalPlaces": 3,
        "PointTolerance": 0.001,
        "Parameters": [
          {
            "Layer": "cut",
            "SplitGeometries": true,
            "SectionLength": 30,
            "SectionTolerance": 5,
            "FeedRate": 300,
            "Power": 2500,
            "Passes": 7,
            "Mode": "Fixed"
          },
          {
            "Layer": "engraving",
            "SplitGeometries": false,
            "SectionLength": 0,
            "SectionTolerance": 0,
            "FeedRate": 600,
            "Power": 2500,
            "Passes": 1,
            "Mode": "Dynamic"
          }
        ]
      },
      ...other profiles
    ]

Profiles are identified by their name, passed as parameter to LaserCam. Profile name can't contain whitespaces and it's case insensitive.
Each profile defines one or more Layer, identified by it's name, the same that you assign in your favourite CAD software. If your drawing contains other layers they'll be ignored.

##### Global profile settings:
* *Name*: short name for the profile, choose easy to remember names
*  *DecimalPlaces*: number of decimal places for the generated GCode, usually 3 is a good value, maybe for imperial units using 4 decimals is better. LaserCam is completely agnostic of measurement units.
* *PointTolerance*: the tolerance used for calculating if two points are the same. Used to correct floating point rounding error.

##### Layer(s) settings:
* *Layer*: layer name, case insensitive
* *SplitGeometries*: boolean, enables long geometry splitting (details below)
*  *SectionLength* : used only if SplitGeometries is true. The maximum allowed vector section length
*  *SectionTolerance*: used only if SplitGeometries is true. Tolerance on *SectionLength* parameter, used to avoid splitting of too short vectors.
> Due to development status, some serious issues (memory leaks) can occur if SectionTolerance is too small. A value of 5 units or above is reccomended
* *FeedRate*: the feedrate for G1/G2/G3 moves in generated GCode. Set it according to your machine performance.
*  *Power*: the laser power (S command). Depends on your machine configuration, some machines goes from 0% to 100%, others specifies the power in mW. 
*  *Passes*: number of passes to do on every vector, useful for cutting.
*  *Mode*: allowed values are:
	* *Fixed*: Laser is driven through M3/M5 commands, laser power is fixed according to Power setting
	* *Dynamic*: Laser is driven through M4/M5 commands, laser power is scaled according to instanteous feedrate
	> See [GRBL](https://github.com/gnea/grbl) documentation for laser operating mode
## Specific optimizations for diode-based laser engravers/cutters
One of the main feature of LaserCAM is a special processing of drawings meant to optimize peromance of diode based laser machines.

Tipically, diode based laser machines are not really meant for cutting. If you want best cutting performance, a wide range of work materials and fast operation, just buy a CO2 based laser machine.

But a CO2 machine is expensive, more dangerous, requires a very precise tune-up to work properly, requires water cooling...

A diode based laser is a more affordable entry point.

With my 2.5W laser I can easily engrave on common materials, and I have a good success rate in cutting thin materials, up to 4mm softwood ply/3mm hardwood ply.
Just give it time to do the work.

After some years using the laser cnc, I found some interesting tweaks that can be done to speed up thre process and get consistent results.
Even a friend of mine, owner of a similar machine, gave me some suggestions, that I've implemented here (thanks Daniele)

So, typical things that people do is running more than one pass to cut completely through a material.
The first optimization is the ratio between number of passes and feedrate.
I've found that some materials cuts best at slow feedrate and a reduced count of passes, but others requires a faster feedrate and more passes. This can reduce the burnt marks on the surface.

But, the main thing that ensures consistency in cutting perfomance is the length of segments.
For istance, a 30mm segment cuts well with "n" passes... but with the same amount of passes, a 100mm segment doesn't cut completely.

By splitting long geometries into smaller sections, as defined by profile, and performing all required passes on every section before moving to the next, cutting perfomance are more repeatable and ensures a better success rate.

LaserCam can split all geometries into smaller sections, automatically and with just a couple of user defined parameters.

## Technology

Built entirely in C#, running on latest .NET 5 framework

Dxf file reading is implemented using [netDxf](https://github.com/haplokuon/netDxf)

Command Line parsing is implemented using [System.CommandLine](https://github.com/dotnet/command-line-api)

This Readme is written with [StackEdit](https://stackedit.io/).

##  Disclaimer

Using laser without proper training and protection can cause severe injuries and blindness, **even with low power**.
Do not leave a laser in operation unattended: most materials can easily catch fire in case of machine malfunction or wrong settings.
Always be sure that that the laser module is rigidly mounted on your cnc, placed on a sturdy table top, and no extraneous objects are in the path of the beam. Don't turn on the laser when it's off the machine.
**ALWAYS WEAR SAFETY GLASSES WHEN EXPERIMENTING WITH LASER**

LaserCam is under continuous development and no warranty is given at any time.
User must always check generated Gcodes, with a text editor and/or with a dry run of the cnc machine (laser OFF).
The author do not accept any liability for damage resulting from usage of this software.

 **NO RESPONSIBILITY IS TAKEN BY THE AUTHOR FOR DAMAGES TO THINGS AND PEOPLE CAUSED BY LASERCAM USAGE.**
