using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject1
{
    class RegisterFieldEditor
    {
        /// <summary>
        /// Modify a subfield contained in  a specified register
        /// </summary>
        /// <returns></returns>
        /// assumes the sub-field is contained (completely) within the given byte value.
        public static byte SetRegSubField_FS45xx(byte registerValue, byte MSBit, byte width, byte fieldValue)
        {
            byte regValue = 0x00;
            byte mask = 0x00;

            try
            {
                if (fieldValue == 0x00)  // if clearing the sub-field
                {
                    mask = ((byte)~((byte)(Math.Pow(2, width) - 1) << MSBit));
                    regValue &= mask;
                }
                else
                {
                    mask = ((byte)((byte)(Math.Pow(2, width) - 1) << MSBit));
                    regValue |= mask;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Setting Register Sub-field: " + ex.Message);
            }

            return regValue;
        }


        /// <summary>
        /// Extract a sub field from a given byte register value.
        /// </summary>
        /// <param name="registerValue"></param>
        /// <param name="MSBit"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static byte GetRegSubField_FS45xx(byte registerValue, byte MSBit, byte width)
        {
            byte mask = ((byte)((byte)(Math.Pow(2, width) - 1) << MSBit));
            return (registerValue &= mask);
        }
    }
}
