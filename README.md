# Coffee timer

This is a coffee timer to make it easier to brew the "4:6 method" invented by Tetsu Kasuya.
A video explaining the brewing process can be found [here](https://www.youtube.com/watch?v=wmCW8xSWGZY).

The "app" is designed to be used on your smartphone.
Visit [https://coffee.probsteide.com](https://coffee.probsteide.com) and add it to your homescreen.

## Running locally

To run it locally you need the [pre-requisites listed for SAFE-stack apps](https://safe-stack.github.io/docs/quickstart/):
- The [].NET Core SDK 2.2](https://dotnet.microsoft.com/download)
- FAKE (>= 5.12) installed as global tool (`dotnet tool install -g fake-cli`)
- [Paket](https://fsprojects.github.io/Paket/)
- [node.js](https://nodejs.org/en/) (>= 8.0)
- [yarn](https://yarnpkg.com/en/) (>= 1.10.1) or npm

Then you can run `make web` from the root folder to build and launch the app.

## Screenshot

It looks something like this when being used. Here with 15g of coffee beans:

![img](screenshot.jpeg)
