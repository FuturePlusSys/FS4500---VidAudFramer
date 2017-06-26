using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject1
{
    class FieldExtractor
    {
        #region Members
        //private int[] LOOP_FLD_WIDTHS = { 3+5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5,                    // P38, P35, R38, R34, P39, P34, R44, R43, P43, P44, R40  
        //                                            5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5,         // P40, P42, R39, R42, R35, R36, P37, P41, R37, R41, P36, P30, R30, P26 
        //                                            5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5,         // P29, R26, R29, P31, R31, P28, P25, R27, R24, P27, P24, R23, P23, R32
        //                                            5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5,         // P32, R28, P33, R25, R33, R18, R22, R15, R21, P15, P22, R14, P20, P14
        //                                            5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5,            // P21, P18, R20, PRICLK, P19, R19, P17, R13, R17, P16, P13, R16, R12
        //                                            5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   // P12, R9,  R11, R4,  R5,  P4,  P5,  P9,  R10, R3,  P11, P8,  R8,  R7, P19, P7
        //                                            5, 5, 5, 5,   5, 5, 5 };                                // RP3, R2, R6, P2, R1, P1, P6  

        private byte[] m_loopFieldWidths = null;
        public byte[] LoopFieldWidths { get { return m_loopFieldWidths; } set { m_loopFieldWidths = value; } }

        #endregion // Members

        #region Ctor
        #endregion // Ctor

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods

        ///// <summary>
        ///// Extract the individual field values from the list of bytes that have been extracted from the HW
        ///// </summary>
        ///// <param name="loopBytes"></param>
        ///// <param name="fldsList"></param>
        ///// <returns></returns>
        //public bool GetloopFields(List<byte> loopBytes, List<long> fldsList)
        //{
        //    bool status = true;

        //    return status;
        //}


        /// <summary>
        /// Extract the individual field values from the list of bytes that have been extracted from the HW
        /// </summary>
        /// Assumes the data is aligned on byte boundries of the stated start and end indices
        /// <param name="loopBytes"></param>
        /// <param name="countsList"></param>
        public bool GetloopFields(byte[] fieldWidths, byte[] loopBytes, List<long> fldsList, int startIndex, int endIndex)
        {
            bool status = true;
            int countID = 0;

            m_loopFieldWidths = fieldWidths;

            // extract the individual counter values.
            int fldID = 0;
            int fldBitCount = 0;
            int totalBitCount = 0;
            long curFldValue = 0x00;
            uint curByte = 0x00;
            int curFldLen = fieldWidths[fldID];

            fldsList.Clear();

            for (int curByteID = startIndex; curByteID <= endIndex; curByteID++)
            {
                curByte = loopBytes[curByteID];

                curFldValue = ((curFldValue << 8) | curByte);
                fldBitCount += 8;
                totalBitCount += 8;

                if (fldBitCount >= curFldLen)
                {
                    while (fldBitCount >= curFldLen)
                    {
                        // store the counter value in a temp variable to be used to begin the next counter value from
                        long tmpCount = curFldValue;

                        // trim the current field value to the correct number of bits
                        curFldValue = curFldValue >> (fldBitCount - curFldLen);

                        // increment the field count
                        countID++;

                        // add the extracted field to the list 
                        fldsList.Add(curFldValue);

                        // check if we've processed to many fields, e.g. something went wrong!
                        if (countID > fieldWidths.Length)
                            break;

                        // keep track of the number of bits used from the loopBytes list.
                        fldBitCount -= curFldLen;  // fldBitCount == 32 - (1, or 16)

                        // create a field mask and  extract the current field.  The fields lsb is located at bit 0
                        curFldValue = tmpCount;
                        long subFldMaskValue = 0x00;
                        for (int i = 0; i < fldBitCount; i++)
                            subFldMaskValue |= ((long)(0x01 << i));
                        curFldValue &= subFldMaskValue;

                        // increment the current fld ID being processed
                        if ((fldID + 1) < fieldWidths.Length)
                            curFldLen = fieldWidths[++fldID];
                    }
                }
            }

            return status;
        }

        #endregion // Public Methods
    }
}
