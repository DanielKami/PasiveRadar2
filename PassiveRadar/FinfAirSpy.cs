using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasiveRadar
{
    public class FindAirSpy
    {
        DLL_AirSpy dll;
        IntPtr dev = IntPtr.Zero;
        public int[] StatusList;
        public string[] NameList;
        public uint[] List; //List stores the index of device which can be opened
        public int NrOfDevices = 0;

        public FindAirSpy()
        {
            dll = new DLL_AirSpy();
            NrOfDevices = 1;
            List = new uint[256];
            NameList = new string[256];
            NameList[0] = "None";
            StatusList = new int[256];
            StatusList[0] = 1;
        }


        /*param index of device to be opened.
        * \return -1 if no device found at index
        * \return -2 if cannot open device.
        * \return various libusb errors if cannot open device or claim interface
        * \return various tuner init errors.
        * \return -10 if already open.*/
        public int Device()
        {
            int r = -1;
            string manufact = "";
            string product = "AirSpy";
            string serial = "";

            bool res = dll.Open();
            if (res)
            {
                NrOfDevices = 1;// dll.get_device_count();
            }
            else
            {
                return -1;
            }
            // if (NrOfDevices == -1) return r;

            //int nrdev=dll.;
            //String str = "Can't open rtlsdr dongle. " + nrdev;
            //MessageBox.Show(str);

            for (uint i = 0; i < NrOfDevices; i++)
            {

                // bool res = DLL_RX888.Init();
                //string Id = dll.GetIBoardd();
                //UInt64 SN = dll.GetSN();
                // r = dll.get_device_usb_strings(i, ref manufact, ref product, ref serial);
                //string version = dll.GetVersion();
                NameList[i + 1] = "(" + i + ") " + manufact + " " + product ;
                List[i + 1] = i;

               dll.Close();

            }
            NrOfDevices++;
            return r;
        }
    }

}
