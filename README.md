# DTM2SKUA

This library converts SURPAC DTM files to SKUA-GOCAD surface files and vice versa. Addionally SKUA-GOCAD surfaces can be converted to DXF files.

## Usage

This application should be run in a terminal with the following command:

```
DTM2SKUA.exe <DTM file> <output SKUA-GOCAD surface file>
SKUA2DTM.exe <SKUA-GOCAD surface file> <output DTM file>
SKUA2DXF.exe <SKUA-GOCAD surface file> <output DXF file>
```

On macOS/Linux you can run the application using the mono framework:

```
mono DTM2SKUA.exe <DTM file> <output SKUA-GOCAD surface file>
mono SKUA2DTM.exe <SKUA-GOCAD surface file> <output DTM file>
mono SKUA2DXF.exe <SKUA-GOCAD surface file> <output DXF file>
```

Currently the SKUA2DTM function doesn't really work, because a checksum is calculated for DTM files, which cannot be recalculated without information from the SURPAC team. Therefore DTM files can be created, but not loaded into SURPAC.

## Download the latest Release

You can download the latest release of the [DTM2SKUA library here](https://github.com/stdonn/DTM2SKUA/releases/latest)

**!!!ATTENTION!!!** All current versions are still pre-releases and are not build for working in production environments

## TODO

Currently only the mesh is converted. In the future also the style information can be converted.

## Built With

* [VisualStudio](http://www.visualstudio.com/) - C# IDE

## Authors

* **Stephan Donndorf** - [stdonn](https://github.com/stdonn)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
