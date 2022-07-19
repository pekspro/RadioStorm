# RadioStorm

RadioStorm is an unofficial app the let you listen of live broadcasts and pods
by [Sveriges Radio](https://sverigesradio.se/). If you want to use it, you could
download it from:

* [Windows
  Store](https://pekspro.com/getproduct?n=RadioStorm_Win10_WindowsStore&s=sv)
* Google Play - when it's ready :-)
 
The source code for the Windows Store edition is not publicly available, but the
plan is to replace that edition with a version based on the source code in this
repository. In fact, much of that source code is used in this project.

## About the source

RadioStorm for Android is a MAUI application. It is separated into several
libraries:

### Pekspro.SwedRadio

This contains the REST-client to the API. It contains of an Open
API-specification that is used to auto generate the REST-client.

### Pekspro.RadioStorm

This library contains the essentials parts of RadioStorm. This is:

* Bootstrapping
* Settings management
* Shared settings management
* Database
    * One database is only used for caching
    * A general database. Currently only used to keep track of which podcasts
      that automatically has been downloaded.
* Download management

### Pekspro.RadioStorm.UI

Here you find all models and view models that is used in the user interface.

### Pekspro.RadioStorm.MAUI

This is the actual MAUI-application that is used for the official version of
RadioStorm. Android and Windows (in the future) are supported. 

There is no planned support for iOS. But it is hopefully not too hard to get it
running on that platform. A few services need to be implemented, where the
`AudioManager` is probably the hardest one.

### Sandbox

In the sandbox folder you find other projects that is used to make development
easier. Most importantly is the WPF-project that let you test logic in a classic
Windows application. MAUI is a bit slow, so this very useful during development.

### File providers

In this folder you find file providers. A file provider is what it sounds like,
it gives you access to some kind of file system. But not local ones, instead
this is used to communicate with file providers in the cloud. Currently only
OneDrive is support, but I hope to add support for Google Drive too someday.

This file providers makes is possible to synchronize settings between devices
via Shared settings.

## Shared settings

Some settings are designed to be shared with other devices. This is:

* Favorites
* Listen status of each episode
* Recently listened episode
* Sort order

These settings are stored in the cloud via a file provider if the user has
chosen to do so. Each setting has a time stamp, and therefore settings can
always be properly merged even if a device has been offline for a while.

## Contribute

If you have an idea or have found a bug, please create an issue. Or create a
pull request if you have developed a fix. But please do not make any major work
before it has been discussed.
