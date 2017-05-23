using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject1
{
    public class FTDI_StatusString
    {
        #region Members
        #endregion // Members

        #region Ctor
        #endregion // Ctor

        #region Private Methods
        #endregion // Private Methods

        #region Private Methods
        static string FTDI_D3xx_FTStatus_String (uint status)
        {
            string IDString = string.Empty;

            switch (status)
            {
                case 0: 
                    IDString = "OK";
                    break;
                case 1: 
                    IDString = "INVALID_HANDLE";
                    break;
                case 2: 
                    IDString = "DEVICE_NOT_FOUND";
                    break;
                case 3: 
                    IDString = "DEVICE_NOT_OPENED";
                    break;
                case 4: 
                    IDString = "IO_ERROR";
                    break;
                case 5: 
                    IDString = "INSUFFICIENT_RESOURCES";
                    break;
                case 6: 
                    IDString = "INVALID_PARAMETER";
                    break;
                case 7: 
                    IDString = "INVALID_BAUD_RATE";
                    break;
                case 8:
                    IDString = "DEVICE_NOT_OPENED_FOR_ERASE";
                    break;
                case 9: 
                    IDString = "DEVICE_NOT_OPENED_FOR_WRITE";
                    break;
                case 10:
                    IDString = "FAILED_TO_WRITE_DEVICE";
                    break;
                case 11: 
                    IDString = "EEPROM_READ_FAILED";
                    break;
                case 12: 
                    IDString = "EEPROM_WRITE_FAILED";
                    break;
                case 13: 
                    IDString = "EEPROM_ERASE_FAILED";
                    break;
                case 14: 
                    IDString = "EEPROM_NOT_PRESENT";
                    break;
                case 15: 
                    IDString = "EEPROM_NOT_PROGRAMMED";
                    break;
                case 16: 
                    IDString = "INVALID_ARGS";
                    break;
                case 17: 
                    IDString = "NOT_SUPPORTED";
                    break;
                case 18: 
                    IDString = "NO_MORE_ITEMS";
                    break;
                case 19: 
                    IDString = "TIMEOUT";
                    break;
                case 20: 
                    IDString = "OPERATION_ABORTED";
                    break;
                case 21: 
                    IDString = "RESERVED_PIPE";
                    break;
                case 22: 
                    IDString = "INVALID_CONTROL_REQUEST_DIRECTION";
                    break;
                case 23: 
                    IDString = "INVALID_CONTROL_REQUEST_TYPE";
                    break;
                case 24: 
                    IDString = "IO_PENDING";
                    break;
                case 25: 
                    IDString = "IO_INCOMPLETE";
                    break;
                case 26: 
                    IDString = "HANDLE_EOF";
                    break;
                case 27: 
                    IDString = "BUSY";
                    break;
                case 28: 
                    IDString = "NO_SYSTEM_RESOURCES";
                    break;
                case 29:
                    IDString = "DEVICE_LIST_NOT_READY";
                    break;
                case 30: 
                    IDString = "OTHER_ERROR";
                    break;
                default:
                    IDString = "UNKNOWN ERROR";
                    break;
            }

            return IDString;
        }
        #endregion // Private Methods
    }
}
