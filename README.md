# JavaVersionSwitcher

[![standard-readme compliant][]][standard-readme]
[![Contributor Covenant][contrib-covenantimg]][contrib-covenant]
[![Build][githubimage]][githubbuild]
[![NuGet package][nugetimage]][nuget]

.NET tool to make switching java versions on windows easy.

## Table of Contents

- [Install](#install)
- [Usage](#usage)
  - [Changes to the system](#changes-to-the-system)
- [Discussion](#discussion)
- [Maintainer](#maintainer)
- [Contributing](#contributing)
  - [Contributors](#contributors)
- [License](#license)

## Install

```cmd
dotnet tool install -g JavaVersionSwitcher
```

## Usage

This tool is tested and works on windows. I'm not sure if anything works on non-windows systems.
(In fact I am quite sure that it will probably not work at all on non-windows systems.)

```cmd
REM show all java versions currently installed
dotnet jvs scan

REM check if the java setup is "ok". (Checks %PATH% and %JAVA_HOME%)
dotnet jvs check

REM switch to another java version
dotnet jvs switch
```

### Changes to the system

After running `dotnet jvs switch` the environment variables `JAVA_HOME` and `PATH` will be modified.
It is necessary to close the current terminal and open a new one to refresh the environment.

## Maintainer

[Nils Andresen @nils-a][maintainer]

## Contributing

JavaVersionSwitcher follows the [Contributor Covenant][contrib-covenant] Code of Conduct.

We accept Pull Requests.

Small note: If editing the Readme, please conform to the [standard-readme][] specification.

## License

[MIT License © Nils Andresen][license]

[githubbuild]: https://github.com/nils-org/JavaVersionSwitcher/actions/workflows/build.yaml?query=branch%3Adevelop
[githubimage]: https://github.com/nils-org/JavaVersionSwitcher/actions/workflows/build.yaml/badge.svg?branch=develop
[maintainer]: https://github.com/nils-a
[contrib-covenant]: https://www.contributor-covenant.org/version/2/0/code_of_conduct/
[contrib-covenantimg]: https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg
[nuget]: https://nuget.org/packages/JavaVersionSwitcher
[nugetimage]: https://img.shields.io/nuget/v/JavaVersionSwitcher.svg?logo=nuget&style=flat-square
[license]: LICENSE.txt
[standard-readme]: https://github.com/RichardLitt/standard-readme
[standard-readme compliant]: https://img.shields.io/badge/readme%20style-standard-brightgreen.svg?style=flat-square