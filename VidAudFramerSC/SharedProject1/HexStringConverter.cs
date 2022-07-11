using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject1
{
    class HexStringConverter
    {
        #region Members
        #endregion // Members

        #region Ctor
        
        /// <summary>
        /// Ctor
        /// </summary>
        public HexStringConverter()
        {
        }

        #endregion //Ctor

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// convert a string to a hex value
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public static bool ConvertHexString(string inputString, ref uint bValue)
        {
            bool status = false;
            string bString = inputString;

            bValue = 0x00;
            try
            {
                if (bString.StartsWith("0x"))
                    bString = bString.Substring(2, bString.Length - 2);

                //status = byte.TryParse(bString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out bValue);
                status = uint.TryParse(bString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out bValue);
            }
            catch (Exception ex)
            {
                throw new Exception("Error Converting input string to a byte valuye");
            }

            return status;
        }

        /// <summary>
        /// Convert the contents of a hex string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static byte ConvertHexString(string inputString)
        {
            bool status = false;
            string bString = inputString;
            byte bValue = 0x00;
            try
            {
                if (bString.StartsWith("0x"))
                    bString = bString.Substring(2, bString.Length - 2);

                status = byte.TryParse(bString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out bValue);
            }
            catch (Exception ex)
            {
                throw new Exception("Error Converting input string to a byte valuye");
            }

            return bValue;
        }


        /// <summary>
        /// Validates the contents of a given string for hex digits
        /// </summary>
        /// <param name="testString"></param>
        /// <returns></returns>
        public static bool OnlyHexInString(string testString)
        {
            string test = testString;
            if (test.StartsWith("0x"))
                test = test.Substring(2, test.Length - 2);

            // For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }



        /// <summary>
        /// Returns number of digits in a given string.  Strips off preceding "0x" substring
        /// if it exists.
        /// </summary>
        /// <param name="testString"></param>
        /// <returns></returns>
        public static int NumericDigitCount(string formatID = "0x", string digitString = "")
        {
            string digits = digitString;
            if (digits.StartsWith(formatID))
                digits = digits.Substring(2, digits.Length - 2);

            return digits.Length;
        }


        /// <summary>
        /// Verify char is 0-9, a-f, A-F
        /// </summary>
        /// <param name="hexDigit"></param>
        /// <returns></returns>
        private static bool IsHexDigit(char hexDigit)
        {
            bool status = true;

            if (((hexDigit < '0') || (hexDigit > '9')) &&
                ((hexDigit < 'a') || (hexDigit > 'f')) &&
                ((hexDigit < 'A') || (hexDigit > 'F')))
            {
                status = false;
            }

            return status;
        }

        /// <summary>
        /// Verify the char is either x or X
        /// </summary>
        /// <param name="hexDigit"></param>
        /// <returns></returns>
        private static bool IsMaskDigit(char hexDigit)
        {
            bool status = true;

            if ((hexDigit != 'x') && (hexDigit != 'X') && (hexDigit != '$'))
                status = false;

            return status;
        }


        /// <summary>
        /// validate a hex string
        /// 
        /// strlen is the len of the string in a text box control
        /// maxLength is the number of digit after the "0x" hex prefix
        /// </summary>
        /// <param name="curStringCharArray"></param>
        /// <param name="strLen"></param>
        /// <returns></returns>
        public static bool ValidateHexDataField(string hexString, int maxLength)
        {
            bool status = true;
            string inputString = hexString;

            try
            {
                if (inputString.StartsWith("0x"))
                    inputString = inputString.Substring(2, inputString.Length - 2);

                for (int i = 0; i < inputString.Length; i++)
                {
                    if ((!IsHexDigit(inputString[i])) && (!IsMaskDigit(inputString[i])))
                    {
                        status = false;
                        break;
                    }
                }

                if (status)
                {
                    if (inputString.Length > maxLength)
                        status = false;
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
