using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject1
{
    static class LoopOperations
    {
        #region Members
        #endregion // Members

        #region Ctor
        #endregion // Ctor

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods


        /// <summary>
        /// Get the field location in terms of byteID and the MSBit of the field within the byte. 
        /// </summary>
        /// <param name="filterBitID"></param>
        /// <param name="byteID"></param>
        /// <param name="bitID"></param>
        /// <returns></returns>
        /// Assumptions 
        ///                                                                            byte[0]                byte[1]  ...
        ///     OffSet is the bitID starting from the left side and counting up... 7,6,5,4,3,2,1,0  ||  7,6,5,4,3,12,1,0  || ...
        ///                                                                        | | |                | | |
        ///                                                                        | | |                | | |
        ///                                                                        | | offset:2         | | offset:10
        ///                                                                        | offset:1           | offset:9
        ///                                                                        offset:0             offset:8
        static public bool GetFieldLocation(int byteCount, int offset, ref int byteID, ref int bitID)
        {
            bool status = true;
            bitID = 0;

            // this value represents the byte in linear order from the beginning of the array (starting with byte 0, 1, etc)
            float byteBitIDs = ((float)(offset) / (float)8);

            // this value represents the byte in the array i.e. byte[0]  byte[1]  byte[2]...
            byteID = (int)byteBitIDs; 

            // strip off the integer and keep the remainder 
            float bitValue = (float)byteBitIDs - (int)byteBitIDs;

            // identify the bit assoicated with the remainder
            if (bitValue == 0)
                bitID = 7;
            else if (bitValue == 0.125)
                bitID = 6; 
            else if (bitValue == 0.25)
                bitID = 5; 
            else if (bitValue == 0.375)
                bitID = 4; 
            else if (bitValue == 0.50)
                bitID = 3; 
            else if (bitValue == 0.625)
                bitID = 2; 
            else if (bitValue == 0.75)
                bitID = 1; 
            else if (bitValue == 0.875)
                bitID = 0; 
            else
            {
                status = false;
                bitID = -1;
            }

            return status;
        }


        /// <summary>
        /// Extract the number of specified consecutive bits from a given byte array
        /// </summary>
        /// <param name="byteID"></param>
        /// <param name="bitID"></param>
        /// <returns></returns>
        static public long GetFieldValue(int byteID, int bitID, int width, byte[] data)
        {
            long fldValue = 0x00;
            int bitCount = 0;

            // How this works:
            //  'For' loop accumulates the field in chunks of bytes..
            //   The first byte being extracted lops off the MSBits not contained in the field
            //   Thus the bits used in the MSBytes begin with the specified field byte # and Bit #
            //   the loop gets whole bytes for all subsequent bytes being extracted from the data
            //   Once enough bits are accumulated, the loop is exited.  The field value is shifted
            //   to the rights for the number of 'Extra' bits of extracted data.

            // assemble the bytes of data containing the field.
            for (int i = byteID; (width - bitCount) > 0; i++)
            {
                if (i == byteID)
                {
                    // lop off the leading bits...
                    //fldValue = data[i] & (long)(Math.Pow(2, bitID) - 1); //(~((long)(Math.Pow(2, bitID) - 1)));
                    fldValue = data[i] & (long)(Math.Pow(2, bitID + 1) - 1);
                    bitCount += bitID + 1;
                }
                else
                {
                    fldValue = (fldValue << 8) | data[i];
                    bitCount += 8;
                }
            }

            // lop off the trailing bits that are not part of the field.
            if (bitCount > width)
                fldValue = fldValue >> (bitCount - width); 

            return fldValue;
        }


        /// <summary>
        /// Extract the number of specified consecutive bits from a given byte array
        /// </summary>
        /// <param name="byteID"></param>
        /// <param name="bitID"></param>
        /// <returns></returns>
        static public uint GetFieldValueII(int byteID, int bitID, int width, byte[] data)
        {
            uint fldValue = 0x00;
            int bitCount = 0;

            // How this works:
            //  'For' loop accumulates the field in chunks of bytes..
            //   The first byte being extracted lops off the MSBits not contained in the field
            //   Thus the bits used in the MSBytes begin with the specified field byte # and Bit #
            //   the loop gets whole bytes for all subsequent bytes being extracted from the data
            //   Once enough bits are accumulated, the loop is exited.  The field value is shifted
            //   to the rights for the number of 'Extra' bits of extracted data.

            // assemble the bytes of data containing the field.
            for (int i = byteID; (width - bitCount) > 0; i++)
            {
                if (i == byteID)
                {
                    // lop off the leading bits...
                    //fldValue = data[i] & (long)(Math.Pow(2, bitID) - 1); //(~((long)(Math.Pow(2, bitID) - 1)));
                    fldValue = data[i] & (uint)(Math.Pow(2, bitID + 1) - 1);
                    bitCount += bitID + 1;
                }
                else
                {
                    fldValue = (fldValue << 8) | data[i];
                    bitCount += 8;
                }
            }

            // lop off the trailing bits that are not part of the field.
            if (bitCount > width)
                fldValue = fldValue >> (bitCount - width);

            return fldValue;
        }


        /// <summary>
        /// Set a fld value.  
        /// </summary>
        /// Assumptions:  FldValue is left justified... that is LIBit is located at bit 0.  (bits[7:0]
        /// <param name="fldValue"></param>
        /// <param name="loopBytes"></param>
        /// <param name="byteID"></param>
        /// <param name="bitID"></param>
        /// <returns></returns>
        static public bool SetFieldValue(uint fldValue, int fldWidth, byte[] loopBytes, int LoopByteID, int MSBitPos)
        {
            bool status = true;

            int byteNo = LoopByteID;
            int bitPosition = MSBitPos;

            try
            {
                for (int i = fldWidth - 1; i >= 0; i--)
                {
                    if ((fldValue & (0x01 << i)) != 0x00)
                        loopBytes[byteNo] |= (byte)(0x01 << bitPosition);
                    else
                        loopBytes[byteNo] &= (byte)~(0x01 << bitPosition);

                    if (bitPosition == 0)
                    {
                        byteNo += 1;
                        bitPosition = 7;
                    }
                    else
                    {
                        bitPosition -= 1;
                    }
                }
            }
            catch (Exception ex)
            {
                status = false;
            }

            return status;
        }
        #endregion // Public Methods
    }
}
