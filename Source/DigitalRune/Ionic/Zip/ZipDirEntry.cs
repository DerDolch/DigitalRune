// ZipDirEntry.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2006-2011 Dino Chiesa .
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Globalization;


namespace DigitalRune.Ionic.Zip
{
    partial class ZipEntry
    {
        /// <summary>
        /// True if the referenced entry is a directory.
        /// </summary>
        internal bool AttributesIndicateDirectory
        {
            get { return ((_InternalFileAttrs == 0) && ((_ExternalFileAttrs & 0x0010) == 0x0010)); }
        }


        internal void ResetDirEntry()
        {
            // __FileDataPosition is the position of the file data for an entry.
            // It is _RelativeOffsetOfLocalHeader + size of local header.

            // We cannot know the __FileDataPosition until we read the local
            // header.

            // The local header is not necessarily the same length as the record
            // in the central directory.

            // Set to -1, to indicate we need to read this later.
            this.__FileDataPosition = -1;

            // set _LengthOfHeader to 0, to indicate we need to read later.
            this._LengthOfHeader = 0;
        }

        // workitem 10330
        private static class CopyHelper
        {
            private static System.Text.RegularExpressions.Regex re =
                new System.Text.RegularExpressions.Regex(" \\(copy (\\d+)\\)$");

            private static int callCount = 0;

            internal static string AppendCopyToFileName(string f)
            {
                callCount++;
                if (callCount > 25)
                    throw new OverflowException("overflow while creating filename");

                int n = 1;
                int r = f.LastIndexOf(".", StringComparison.Ordinal);

                if (r == -1)
                {
                    // there is no extension
                    System.Text.RegularExpressions.Match m = re.Match(f);
                    if (m.Success)
                    {
                        n = Int32.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) + 1;
                        string copy = String.Format(CultureInfo.InvariantCulture, " (copy {0})", n);
                        f = f.Substring(0, m.Index) + copy;
                    }
                    else
                    {
                        string copy = String.Format(CultureInfo.InvariantCulture, " (copy {0})", n);
                        f = f + copy;
                    }
                }
                else
                {
                    //System.Console.WriteLine("HasExtension");
                    System.Text.RegularExpressions.Match m = re.Match(f.Substring(0, r));
                    if (m.Success)
                    {
                        n = Int32.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) + 1;
                        string copy = String.Format(CultureInfo.InvariantCulture, " (copy {0})", n);
                        f = f.Substring(0, m.Index) + copy + f.Substring(r);
                    }
                    else
                    {
                        string copy = String.Format(CultureInfo.InvariantCulture, " (copy {0})", n);
                        f = f.Substring(0, r) + copy + f.Substring(r);
                    }

                    //System.Console.WriteLine("returning f({0})", f);
                }
                return f;
            }
        }



        /// <summary>
        ///   Reads one entry from the zip directory structure in the zip file.
        /// </summary>
        ///
        /// <param name="zf">
        ///   The zipfile for which a directory entry will be read.  From this param, the
        ///   method gets the ReadStream and the expected text encoding
        ///   (ProvisionalAlternateEncoding) which is used if the entry is not marked
        ///   UTF-8.
        /// </param>
        ///
        /// <param name="previouslySeen">
        ///   a list of previously seen entry names; used to prevent duplicates.
        /// </param>
        ///
        /// <returns>the entry read from the archive.</returns>
        internal static ZipEntry ReadDirEntry(ZipFile zf,
                                              Dictionary<String,Object> previouslySeen)
        {
            System.IO.Stream s = zf.ReadStream;
            System.Text.Encoding expectedEncoding = (zf.AlternateEncodingUsage == ZipOption.Always)
                ? zf.AlternateEncoding
                : ZipFile.DefaultEncoding;

            int signature = Ionic.Zip.SharedUtilities.ReadSignature(s);
            // return null if this is not a local file header signature
            if (IsNotValidZipDirEntrySig(signature))
            {
                s.Seek(-4, System.IO.SeekOrigin.Current);

                // Getting "not a ZipDirEntry signature" here is not always wrong or an
                // error.  This can happen when walking through a zipfile.  After the
                // last ZipDirEntry, we expect to read an
                // EndOfCentralDirectorySignature.  When we get this is how we know
                // we've reached the end of the central directory.
                if (signature != ZipConstants.EndOfCentralDirectorySignature &&
                    signature != ZipConstants.Zip64EndOfCentralDirectoryRecordSignature &&
                    signature != ZipConstants.ZipEntrySignature  // workitem 8299
                    )
                {
                    throw new BadReadException(String.Format(CultureInfo.InvariantCulture, "  Bad signature (0x{0:X8}) at position 0x{1:X8}", signature, s.Position));
                }
                return null;
            }

            int bytesRead = 42 + 4;
            byte[] block = new byte[42];
            int n = s.Read(block, 0, block.Length);
            if (n != block.Length) return null;

            int i = 0;
            ZipEntry zde = new ZipEntry();
            zde.AlternateEncoding = expectedEncoding;
            zde._Source = ZipEntrySource.ZipFile;
            zde._container = new ZipContainer(zf);

            unchecked
            {
                zde._VersionMadeBy = (short)(block[i++] + block[i++] * 256);
                zde._VersionNeeded = (short)(block[i++] + block[i++] * 256);
                zde._BitField = (short)(block[i++] + block[i++] * 256);
                zde._CompressionMethod = (Int16)(block[i++] + block[i++] * 256);
                zde._TimeBlob = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                zde._LastModified = Ionic.Zip.SharedUtilities.PackedToDateTime(zde._TimeBlob);
                zde._timestamp |= ZipEntryTimestamp.DOS;

                zde._Crc32 = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                zde._CompressedSize = (uint)(block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256);
                zde._UncompressedSize = (uint)(block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256);
            }

            // preserve
            zde._CompressionMethod_FromZipFile = zde._CompressionMethod;

            zde._filenameLength = (short)(block[i++] + block[i++] * 256);
            zde._extraFieldLength = (short)(block[i++] + block[i++] * 256);
            zde._commentLength = (short)(block[i++] + block[i++] * 256);
            zde._diskNumber = (UInt32)(block[i++] + block[i++] * 256);

            zde._InternalFileAttrs = (short)(block[i++] + block[i++] * 256);
            zde._ExternalFileAttrs = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;

            zde._RelativeOffsetOfLocalHeader = (uint)(block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256);

            // workitem 7801
            zde.IsText = ((zde._InternalFileAttrs & 0x01) == 0x01);

            block = new byte[zde._filenameLength];
            n = s.Read(block, 0, block.Length);
            bytesRead += n;
            if ((zde._BitField & 0x0800) == 0x0800)
            {
                // UTF-8 is in use
                zde._FileNameInArchive = Ionic.Zip.SharedUtilities.Utf8StringFromBuffer(block);
            }
            else
            {
                zde._FileNameInArchive = Ionic.Zip.SharedUtilities.StringFromBuffer(block, expectedEncoding);
            }

            // workitem 10330
            // insure unique entry names
            while (previouslySeen.ContainsKey(zde._FileNameInArchive))
            {
                zde._FileNameInArchive = CopyHelper.AppendCopyToFileName(zde._FileNameInArchive);
                zde._metadataChanged = true;
            }

            if (zde.AttributesIndicateDirectory)
                zde.MarkAsDirectory();  // may append a slash to filename if nec.
            // workitem 6898
            else if (zde._FileNameInArchive.EndsWith("/", StringComparison.Ordinal)) zde.MarkAsDirectory();

            zde._CompressedFileDataSize = zde._CompressedSize;
            if ((zde._BitField & 0x01) == 0x01)
            {
                // this may change after processing the Extra field
                zde._Encryption_FromZipFile = zde._Encryption =
                    EncryptionAlgorithm.PkzipWeak;
                zde._sourceIsEncrypted = true;
            }

            if (zde._extraFieldLength > 0)
            {
                zde._InputUsesZip64 = (zde._CompressedSize == 0xFFFFFFFF ||
                      zde._UncompressedSize == 0xFFFFFFFF ||
                      zde._RelativeOffsetOfLocalHeader == 0xFFFFFFFF);

                // Console.WriteLine("  Input uses Z64?:      {0}", zde._InputUsesZip64);

                bytesRead += zde.ProcessExtraField(s, zde._extraFieldLength);
                zde._CompressedFileDataSize = zde._CompressedSize;
            }

            // we've processed the extra field, so we know the encryption method is set now.
            if (zde._Encryption == EncryptionAlgorithm.PkzipWeak)
            {
                // the "encryption header" of 12 bytes precedes the file data
                zde._CompressedFileDataSize -= 12;
            }
#if AESCRYPTO
            else if (zde.Encryption == EncryptionAlgorithm.WinZipAes128 ||
                        zde.Encryption == EncryptionAlgorithm.WinZipAes256)
            {
                zde._CompressedFileDataSize = zde.CompressedSize -
                    (ZipEntry.GetLengthOfCryptoHeaderBytes(zde.Encryption) + 10);
                zde._LengthOfTrailer = 10;
            }
#endif

            // tally the trailing descriptor
            if ((zde._BitField & 0x0008) == 0x0008)
            {
                // sig, CRC, Comp and Uncomp sizes
                if (zde._InputUsesZip64)
                    zde._LengthOfTrailer += 24;
                else
                    zde._LengthOfTrailer += 16;
            }

            // workitem 12744
            zde.AlternateEncoding = ((zde._BitField & 0x0800) == 0x0800)
                ? System.Text.Encoding.UTF8
                :expectedEncoding;

            zde.AlternateEncodingUsage = ZipOption.Always;

            if (zde._commentLength > 0)
            {
                block = new byte[zde._commentLength];
                n = s.Read(block, 0, block.Length);
                bytesRead += n;
                if ((zde._BitField & 0x0800) == 0x0800)
                {
                    // UTF-8 is in use
                    zde._Comment = Ionic.Zip.SharedUtilities.Utf8StringFromBuffer(block);
                }
                else
                {
                    zde._Comment = Ionic.Zip.SharedUtilities.StringFromBuffer(block, expectedEncoding);
                }
            }
            //zde._LengthOfDirEntry = bytesRead;
            return zde;
        }


        /// <summary>
        /// Returns true if the passed-in value is a valid signature for a ZipDirEntry.
        /// </summary>
        /// <param name="signature">the candidate 4-byte signature value.</param>
        /// <returns>true, if the signature is valid according to the PKWare spec.</returns>
        internal static bool IsNotValidZipDirEntrySig(int signature)
        {
            return (signature != ZipConstants.ZipDirEntrySignature);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private Int16 _VersionMadeBy;
        private Int16 _InternalFileAttrs;
        private Int32 _ExternalFileAttrs;

        //private Int32 _LengthOfDirEntry;
        private Int16 _filenameLength;
        private Int16 _extraFieldLength;
        private Int16 _commentLength;
    }
}
