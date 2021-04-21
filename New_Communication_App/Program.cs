using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//add GUI for popup box instead of console

namespace New_Communication_App
{
    class Program
    {
        static SerialPort serialPort = null;
        static string[] coms = SerialPort.GetPortNames();
        static int portSelect = -1;

        static int xframe;
        static int yframe;
        static float xofs;
        static float yofs;
        static float xsize;
        static float ysize;
        static float xbin;
        static float ybin;
        static float xsec;
        static float xmsec;

        static int blen;

        static byte[] imgbuf;

        static void Main(string[] args)
        {
            Console.WriteLine("Possible ports:");
            for (int i = 0; i < coms.Length; i++)
            {
                Console.WriteLine(coms.Length);
                Console.WriteLine(coms[i]);
            }

            String user_input = Console.ReadLine();
            try { int.TryParse(user_input, out portSelect); }
            catch (Exception e)
            {
                Console.WriteLine("bad selection format");
                return;
            }
            if(portSelect >= coms.Length && portSelect != -1)
            {
                Console.WriteLine("Port number out of bounds");
                return;
            }
            Console.WriteLine("User selected port " + portSelect + ", " + coms[portSelect]);

            serialPort = new SerialPort((string)coms[portSelect]);
            try { serialPort.Open(); }
            catch (Exception e)
            {
                MessageBox.Show("Unable to open serial port");
                return;
            }

            //Once serial port is open, lets get some info

            xframe = getParam("xframe?"); // get the dimensions of sensor 
            Console.WriteLine("xframe --> " + xframe);
            yframe = getParam("yframe?");
            Console.WriteLine("yframe --> " + yframe);
            xofs = getParam("xoffset?"); // get x,y,w,h of Region of Interest (ROI) 
            Console.WriteLine("xoffset --> " + xofs);
            yofs = getParam("yoffset?");
            Console.WriteLine("yoffset --> " + yofs);
            xsize = getParam("xsize?");
            Console.WriteLine("xsize --> " + xsize);
            ysize = getParam("ysize?");
            Console.WriteLine("ysize --> " + ysize);
            xbin = getParam("xbin?"); // get current bin 
            Console.WriteLine("xbin --> " + xbin);
            ybin = getParam("ybin?");
            Console.WriteLine("ybin --> " + ybin);
            xsec = getParam("xsec?"); // get exposure in seconds 
            Console.WriteLine("xsec --> " + xsec);
            xmsec = getParam("xmsec?");
            Console.WriteLine("xmsec --> " + xmsec);

            blen = 2 * xframe * yframe;

            Console.WriteLine("blen --> " + blen);

            

            //Since we are generating a 16-bit image. Each pixel is represented by 2 bytes with 
            //the most significant byte first

            //Easiest way to ensure you capture all the data is to define a loop where you first check how many
            //bytes have arrived, then read that many bytes into memory, then go back and
            //repeat until you have read all the bytes you expect. Then, read the “ OK\n” at the
            //end

            imgbuf = new byte[blen];
            loadImg();

            Console.WriteLine("Successfully retrieved values... ending applicaiton");
            return;


            //bool accept_commands = true;  
            ////communication
            //while(accept_commands)
            //{
            //    //string inp = Console.ReadLine();
            //    Console.WriteLine("Please enter a command..");
            //    String inp = "";
            //    while (!inp.Contains("\\n"))
            //    {
            //        inp += Console.ReadLine(); //concat all the commands until \n is found
            //    }
            //    inp = inp.Remove(inp.IndexOf("\\n")); //remove the "\n" from the command
            //    Console.WriteLine("Attempting to parse command:" + inp);
            //    //parse input
            //}


        }
        static bool loadImg()
        {
            int nb = 0;
            while (nb < blen)
            {
                int btr = serialPort.BytesToRead;
                int bytes_left = blen - nb;
                // only read image bytes, not delimeter (” OK\n”) bytes 
                int nrd = Math.Min(btr, bytes_left);
                if (btr > 0)
                {
                    nb += serialPort.Read(imgbuf, nb, nrd);
                }
            }
            string isok = serialPort.ReadLine();
            return (isok.Contains("OK"));
        }

        static bool setParam(string cmd, int val)
        {
            serialPort.Write(string.Format("{0} {1}\n", cmd, val));
            string xf = serialPort.ReadLine().Trim();
            return xf.StartsWith("OK");
        }

        static int getParam(string cmd) // retrieves a integer parameter 
        {
            serialPort.Write(cmd + "\n");
            string xf = serialPort.ReadLine().Trim();
            string[] ary = xf.Split(' ');
            return Convert.ToInt32(ary[0]);
        }

        static double getDParam(string cmd) // retrieves a floating point parameter 
        { // for example, temperature 
            serialPort.Write(cmd + "\n");
            string xf = serialPort.ReadLine().Trim();
            string[] ary = xf.Split(' ');
            return Convert.ToDouble(ary[0]);
        }

        static bool sendCommand(string cmd, bool doWait = true)
        {
            serialPort.Write(string.Format("{0}\n", cmd));
            if (doWait)
            {
                string xf = serialPort.ReadLine().Trim();
                return xf.StartsWith("OK");
            }
            else
            {
                return true;
            }
        }
    }
}
