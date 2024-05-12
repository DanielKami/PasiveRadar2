using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SDRdue;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PasiveRadar
{
    public class Window : WindowSupport
    {
        int dongle_nr;

        DrawWave mDrawWave;
        DrawRadar mDrawRadar;

        public Window(Panel _panelViewport, int _dongle_nr)
        {
            panelViewport = _panelViewport;
            dongle_nr = _dongle_nr;
            mDrawWave = new DrawWave();
            mDrawRadar = new DrawRadar();

            service = new GraphicsDeviceService(panelViewport.Handle, panelViewport.Width, panelViewport.Height);
            service.DeviceResetting += mWinForm_DeviceResetting;
            service.DeviceReset += mWinForm_DeviceReset;

            services = new ServiceContainer();
            services.AddService<IGraphicsDeviceService>(service);
            content = new ContentManager(services, "Content");

        }



        void mWinForm_DeviceReset(Object sender, EventArgs e)
        {

            DeviceReset();
            mDrawWave.SizeChanged(panelViewport, service.GraphicsDevice, service, spriteBatch, spriteFont, mSimpleEffect);
            mDrawRadar.SizeChanged(panelViewport, service.GraphicsDevice, service, spriteBatch, spriteFont, mSimpleEffect, texture);

        }

        public void Update(Flags flags)
        {
            mDrawWave.frequency = flags.frequency[dongle_nr];
            mDrawWave.rate = flags.rate[dongle_nr];
            mDrawWave.Gain = flags.Amplification[dongle_nr];
            mDrawWave.ColorThemeNr = flags.ColorTheme;
            mDrawWave.BufferSize = flags.BufferSizeRadio[dongle_nr];
            mDrawWave.ScalePrepare(panelViewport);

            mDrawRadar.rate = flags.rate[dongle_nr];
            mDrawRadar.BufferSize = flags.BufferSize;
            mDrawRadar.ColorThemeNr = flags.ColorTheme;
            mDrawRadar.DrawPrepare(panelViewport, flags);
            mDrawRadar.DeviceName = flags.DeviceName;
            mDrawRadar.dongle_nr = dongle_nr;
        }

        public void Location(float x)
        {
            mDrawWave.FrequencyAtPointedLocation = x;
        }

        //Start rander the scene
        public void RenderWave(double[] data)
        {
            if (data.Length < mDrawWave.BufferSize) return;
            if (resizing) return;

            if (this.service.GraphicsDevice != null)
                mDrawWave.Scene(panelViewport, data, dongle_nr, 0, 0);

            try
            {
                if (service.GraphicsDevice != null)
                    service.GraphicsDevice.Present();
            }
            catch (Exception ex)
            {
                service.ResetDevice(panelViewport.Width, panelViewport.Height);
                String str = "Plot error. " + ex.ToString();
                // MessageBox.Show(str);
                //  System.Windows.Forms.Application.Exit();
            }
        }



        //Start rander the scene
        public void RenderRadar(float[] data, Flags flags, List<Finder.MapPoints> pointFromRadar, bool DrawScale)
        {

            if (resizing) return;

            if (this.service.GraphicsDevice != null)
            {
                mDrawRadar.Scene(data, flags, pointFromRadar, DrawScale);

                try
                {
                    service.GraphicsDevice.Present();
                }
                catch (Exception ex)
                {
                    service.ResetDevice(panelViewport.Width, panelViewport.Height);
                    String str = "Plot error. " + ex.ToString();
                    MessageBox.Show(str);
                    System.Windows.Forms.Application.Exit();
                }
            }
        }

        //Start rander the scene


    }
}
