ShortPATH
=========

Create environment-wide shortcuts to run commands directly within the context of a chosen directory. Uses the PATH environment variable to add pipe commands that act as shortcuts to a directory of choice.

The executable is portable and requires no administrative privileges. The shortcuts are stored within the AppData directory to make them persistent.



### Screenshot

![ShortPATH Screenshot](https://i.imgur.com/X9evGf7.png)



### Example

`etc` is mapped to `C:\Windows\System32\drivers\etc`

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



### Download

https://github.com/yanikore/ShortPATH/releases



### License

This software is licensed under the MIT license.

