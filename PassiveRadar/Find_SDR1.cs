using System;

namespace PasiveRadar
{
    public class FindRSP1
    {
        DLL_RSP1 dll;
        IntPtr dev = IntPtr.Zero;
        public int[] StatusList;
        public string[] NameList;
        public uint[] List; //List stores the index of device which can be opened
        public int NrOfDevices = 0;

        public FindRSP1()
        {
            dll = new DLL_RSP1();
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
            string product = "";
            string serial = "";

            NrOfDevices = dll.get_device_count();
            if (NrOfDevices == -1) return r;

            //int nrdev=dll.;
            //String str = "Can't open rtlsdr dongle. " + nrdev;
            //MessageBox.Show(str);

            for (uint i = 0; i < NrOfDevices; i++)
            {

                int res = dll.open(i);

                r = dll.get_device_usb_strings(i, ref manufact, ref product, ref serial);
                NameList[i + 1] = "(" + i + ") " + manufact + " " + product + " " + serial;
                List[i + 1] = i;

                dll.close();

            }
            NrOfDevices++;
            return r;
        }
    }
}
