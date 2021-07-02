/*!
 * Copyright 2021 by ner software, LLC.
 * Licensed under the MIT license.
 * This repo is hosted: https://github.com/nerseus/ner.Cal3D
 */

/*!
 * NOTE: This is NOT a general purpose Cal3D library. This library is used to load
 *       existing ascii/binary Cal3D files, manipulate the data, then export to the
 *		 IMVU-specific version of Cal3D.
 *
 *       This also allows loading generic Cal3D files (such as those exported by WorldViz)
 *       and converting them to IMVU format.
 */

//************************************************************************************************************
//  This library is used to load/parse existing Cal3D files into objects.
//
//  Creating a new/empty object is not supported (though adding support would be relatively trivial).
//  Since this library reads multiple formats of Cal3D files (those in IMVUs specific format as well as the
//  more generic format) - this library can act as a "conversion" - by reading in Cal3D files from
//  one format and then saving in IMVUs format. This allows someone to use a WorldViz exporter (for example)
//  in 3ds Max and export to XMF and XSF, then convert those files to be used in IMVU (since the WorldViz
//  files cannot be used by IMVU as-is).
//
//  General usage - create new instance of the type needed, with "rawData" being the XML string:
//      string rawData = "<SKELETON VERSION=\"1000\" NUMBONES=\"7\"><BONE ID=\"0\"...."; // Cut short for brevity.
//      var xaf = XAF.Parse(rawData);
//      // NOTE: xaf is now a fully parsed object.
//      var bones = xaf.Bones; // Array of bones.
//
//  Writing objects back to files:
//      All objects (and their child objects) support the method toFormattedString() which will return
//      the XML-based string for that type.
//      This can be called on the parent object to return the entire contents.
//
//      For example:
//      string rawData = "<SKELETON VERSION=\"1000\" NUMBONES=\"7\"><BONE ID=\"0\"...."; // Cut short for brevity.
//      var xaf = new XAF(rawData);
//      var newData = xaf.toFormattedString();
//      // NOTE: newData now contains the full string from the xaf and can be written out to file.
//
//  Functions supported per type:
//      XAF:
//          reverseAndAppend()
//          offset(offsetAmount)
//          lengthenLastFrame(lengthenTime)
//          stretch(stretchMultiplier)
//          unTwitch()
//      XMF:
//          merge(xmf2)
//      XPF:
//          mergeWithOffset(mergeMorph, newStartTime, newEndTime, newDuration, blendFrames)
//          NOTE: This is untested. Code originally written in C# and converted/untested.
//      XSF:
//          n/a (object supported for Conversion purposes)
//      XRF:
//          n/a (object supported for Conversion purposes)
//************************************************************************************************************
