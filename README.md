# ner.Cal3D
C# library used to load/parse existing Cal3D files into objects. Supports X* format (ASCII) and C* format (binary).

## Instalation:
Download and add ner.Cal3D.csproj to your solution.
There is currently no nuget package. There are no unit tests.

### To use:
```
    var bytes = File.ReadAllBytes("c:\\somefile.xsf");
    XSF xsf = XSF.Parse(bytes);
    // xsf is now a fully loaded object.
    File.WriteAllText("c:\\updatedFile.xsf", xsf.ToFormattedString());
```

Each main class (XAF, XMF, XSF, XRF, XPF) has a few static methods to support parsing. The main entry point is Parse(byte[]). This will detect if the file is ascii or binary. If you know you have one or the other you can call ParseXml or ParseBinary.

## Purpose
The purpose of this library is 2-fold:
* Read and parse binary/ascii Cal3D files and write them out as IMVU-specific ascii Cal3D files
* Add some useful tools to various formats

## Utilities supported
Some file types support added utilities. For example, stretching an XAF (skeletal animation) to make it longer (slower).

Functions supported per type:
      XAF:
          reverseAndAppend()
          offset(offsetAmount)
          lengthenLastFrame(lengthenTime)
          stretch(stretchMultiplier)
          unTwitch()
      XMF:
          merge(xmf2)
      XPF:
          mergeWithOffset(mergeMorph, newStartTime, newEndTime, newDuration, blendFrames)
          NOTE: See the method in the XFP class for details on what this method does and why it exists.
