# ShortPATH

Create environment-wide shortcuts on **Windows** to run commands directly within the context of a chosen directory. Uses the PATH environment variable to add pipe commands that act as shortcuts to a directory of choice.

The executable is portable and requires no administrative privileges. Shortcuts are stored within the AppData directory to make them persistent.



## Screenshot

![ShortPATH Screenshot](https://i.imgur.com/X9evGf7.png)



## Example

The `etc` shortcut is mapped to `C:\Windows\System32\drivers\etc`

We can now run commands like this: `etc <command> <arguments>`.

Example output for `etc dir`:

```
C:\Users\Yani>etc dir

 Volume in drive C has no label.
 Volume Serial Number is 1234-5678

 Directory of C:\Windows\System32\drivers\etc

23-May-18  05:10    <DIR>          .
23-May-18  05:10    <DIR>          ..
30-Dec-17  02:11             2,464 hosts
12-Apr-18  01:36             3,683 lmhosts.sam
16-Jul-16  13:45               407 networks
16-Jul-16  13:45             1,358 protocol
16-Jul-16  13:45            17,463 services
               5 File(s)         25,375 bytes
               2 Dir(s)  15,289,909,248 bytes free

```



## How it works

The application is just a front-end for managing shortcut .bat files.
When it's first ran, a directory in appdata is created and its path is added to the PATH environment variable.

Now when you run a command in your console, the new directory is checked for executable files.
Because our shortcuts are saved as `<shortcut>.bat`, the shortcut acts as an executable and uses 
all other arguments as a new command in the directory the shortcut points to.

The shortcut file that is created looks like this:
```
@echo off
SET org_dir=%cd%
cd /d "<project-directory>"
%*
cd /d "%org_dir%\"
```



## Download

https://github.com/yani/ShortPATH/releases



## License

This software is licensed under the MIT license.

