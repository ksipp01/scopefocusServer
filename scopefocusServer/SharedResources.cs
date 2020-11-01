//
// ================
// Shared Resources
// ================
//
// This class is a container for all shared resources that may be needed
// by the drivers served by the Local Server. 
//
// NOTES:
//
//	* ALL DECLARATIONS MUST BE STATIC HERE!! INSTANCES OF THIS CLASS MUST NEVER BE CREATED!
//
// Written by:	Bob Denny	29-May-2007
// Modified by Chris Rowland and Peter Simpson to hamdle multiple hardware devices March 2011
//
using System;
using System.Collections.Generic;
using System.Text;
using ASCOM;
using ASCOM.Utilities;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;

namespace ASCOM.scopefocusServer
{
    /// <summary>
    /// The resources shared by all drivers and devices, in this example it's a serial port with a shared SendMessage method
    /// an idea for locking the message and handling connecting is given.
    /// In reality extensive changes will probably be needed.
    /// Multiple drivers means that several applications connect to the same hardware device, aka a hub.
    /// Multiple devices means that there are more than one instance of the hardware, such as two focusers.
    /// In this case there needs to be multiple instances of the hardware connector, each with it's own connection count.
    /// </summary>
    public static class SharedResources
    {
        // object used for locking to prevent multiple drivers accessing common code at the same time
        private static readonly object lockObject = new object();

        // Shared serial port. This will allow multiple drivers to use one single serial port.
        private static ASCOM.Utilities.Serial s_sharedSerial = new ASCOM.Utilities.Serial();        // Shared serial port
        private static int s_z = 0;     // counter for the number of connections to the serial port
        //  private static bool connectedState;




        private static TraceLogger traceLogger;
        



        public static TraceLogger tl
        {
            get
            {
                if (traceLogger == null)
                {
                    
                    traceLogger = new TraceLogger("", "scopefocusServer");
                    traceLogger.Enabled = ASCOM.scopefocusServer.Properties.Settings.Default.traceState;
                  //  traceLogger.Enabled = true;
                    //    traceLogger.Enabled = ASCOM.scopefocusServer.Settings
                }
                return traceLogger;
            }
        }

      private static  System.Threading.Mutex mutex = new System.Threading.Mutex();
        private static float lastPos;
        //    double lastTemp = 0;
       private static bool lastMoving = false;
      private static  bool lastLink = false;

      private static  long UPDATETICKS = (long)(1 * 10000000.0); // 10,000,000 ticks in 1 second
      private static  long lastUpdate = 0;

      //  private static int stepsPerDegree;
        private static  long lastL = 0;
        //
        // Public access to shared resources
        //

        #region single serial port connector
        //
        // this region shows a way that a single serial port could be connected to by multiple 
        // drivers.
        //
        // Connected is used to handle the connections to the port.
        //
        // SendMessage is a way that messages could be sent to the hardware without
        // conflicts between different drivers.
        //
        // All this is for a single connection, multiple connections would need multiple ports
        // and a way to handle connecting and disconnection from them - see the
        // multi driver handling section for ideas.
        //

        public static int stepsPerDegree
        {
            get
            {
              //  SharedResources.tl.LogMessage("StepsPerDegree", stepsPerDegree.ToString());
              return ASCOM.scopefocusServer.Properties.Settings.Default.stepsPerDegree;
            }
        }
        public static bool setPos
        {
            get
            {
                //  SharedResources.tl.LogMessage("StepsPerDegree", stepsPerDegree.ToString());
                return ASCOM.scopefocusServer.Properties.Settings.Default.SetPos;
            }
        }
        public static bool contHold
        {
            get
            {
                //  SharedResources.tl.LogMessage("StepsPerDegree", stepsPerDegree.ToString());
                return ASCOM.scopefocusServer.Properties.Settings.Default.ContHold;
            }
        }
        public static bool rev
        {
            get
            {
                //  SharedResources.tl.LogMessage("StepsPerDegree", stepsPerDegree.ToString());
                return ASCOM.scopefocusServer.Properties.Settings.Default.rev;
            }
        }
        public static string COMPort
        {
            get
            {
                //  SharedResources.tl.LogMessage("StepsPerDegree", stepsPerDegree.ToString());
                return ASCOM.scopefocusServer.Properties.Settings.Default.COMPort;
            }
        }
        public static float SetPosValue
        {
            get
            {
                //  SharedResources.tl.LogMessage("StepsPerDegree", stepsPerDegree.ToString());
                return ASCOM.scopefocusServer.Properties.Settings.Default.SetPosValue;
            }
        }

        /// <summary>
        /// Shared serial port
        /// </summary>
        public static ASCOM.Utilities.Serial SharedSerial { get { return s_sharedSerial; } }

        /// <summary>
        /// number of connections to the shared serial port
        /// </summary>
        public static int connections { get { return s_z; } set { s_z = value; } }

        /// <summary>
        /// Example of a shared SendMessage method, the lock
        /// prevents different drivers tripping over one another.
        /// It needs error handling and assumes that the message will be sent unchanged
        /// and that the reply will always be terminated by a "#" character.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// 

        public static string SendMessage(string message)
      //  public static string SendMessage(string function, string message)
        {
            lock (lockObject)
            {
             //   tl.LogMessage("scopefocusServer", "Lockl Object");
       
                string msg = message + "#";  // adds # to all messages, removed #'s from the indivual commands
                if (SharedSerial.Connected && !String.IsNullOrEmpty(msg))
                {
                    tl.LogMessage("Controller", "Send Msg: " + msg);
                    SharedSerial.ClearBuffers();
                    SharedSerial.Transmit(msg);
                    string strRec = SharedSerial.ReceiveTerminated("#");
                    SharedSerial.ClearBuffers();
                    tl.LogMessage("Controller", "Response Msg: " + strRec);
                    return strRec;
                }
                else
                {
                    tl.LogMessage("Controller", "Not Connected or Empty Send Msg: " + message);
                    return "";
                }
                //SharedSerial.Transmit(message);
                //// TODO replace this with your requirements
                //return SharedSerial.ReceiveTerminated("#");
            }
        }

        /// <summary>
        /// Example of handling connecting to and disconnection from the
        /// shared serial port.
        /// Needs error handling
        /// the port name etc. needs to be set up first, this could be done by the driver
        /// checking Connected and if it's false setting up the port before setting connected to true.
        /// It could also be put here.
        /// </summary>
        /// 

      
        public static bool Connected
        {
            //get
            //{
            //    tl.LogMessage("Connected Get", IsConnected.ToString());
            //    return IsConnected;
            //}


            set
            {
                lock (lockObject)
                {
                    if (value)
                    {

                        if (s_z == 0)
                        {
                        
                            try
                            {
                                // SharedSerial.PortName = "COM3";   //set for testing.....  need to figure out how to really get it....
                                SharedSerial.PortName = ASCOM.scopefocusServer.Properties.Settings.Default.COMPort;
                            //                                SharedSerial.ReceiveTimeoutMs = 2000;
                            SharedSerial.Speed = ASCOM.Utilities.SerialSpeed.ps9600;
                            //                                SharedSerial.Handshake = ASCOM.Utilities.SerialHandshake.None;
                            SharedSerial.Connected = true;
                            System.Threading.Thread.Sleep(2500);  
                                                          
                              
                            string answer = SharedResources.rawCommand("G", true);

                     //       tl.LogMessage("Connection", "Trying to Connect");
                            tl.LogMessage("StepsPerDegree", stepsPerDegree.ToString());

                                if (setPos)
                                {
                                    tl.LogMessage("Set Position To", SetPosValue.ToString());
                                    rawCommand("P " + Math.Round(SetPosValue * stepsPerDegree + (360 * stepsPerDegree), 0).ToString(), false);
                                    ASCOM.scopefocusServer.Properties.Settings.Default.SetPos = false;
                                }
                            if (contHold)
                                rawCommand("C 1", false); //continuous hold on
                            else
                                rawCommand("C 0", false);

                             tl.LogMessage("Continuous Hold On", contHold.ToString());
                            // log the arduino firware version
                            string ver = rawCommand("V", false);
                            string verTrim = ver.Replace('#', ' ');
                            string versn = verTrim.Replace('V', ' ').Trim();
                            tl.LogMessage("Firmware Version: ", versn.ToString());
                            tl.LogMessage("StepSize", StepSize.ToString());
                                string version = DriverVersion;

                                   //connectedState = true;
                                //    tl.LogMessage("Connected Set", "Connecting to port " + SharedSerial.PortName);
                                // TODO connect to the device
                                //    string version = DriverVersion;

                                // add

                                //     bool homeSet = false;
                                //    float posValue = 0;
                                //    bool setPos = false;
                                //   bool reverse = true;
                                //   bool contHold = false;

                                // check if we are connected, return if we are


                                // remd 4-27-17
                                //if (SharedSerial.PortName != null && SharedResources.SharedSerial.Connected)  //Portname might not be right.....
                                //return;



                            // get the port name from the profile
                            //    string portName;
                            //using (ASCOM.Utilities.Profile p = new Profile())
                            //{
                            // get the values that are stored in the ASCOM Profile for this driver
                            // these were usually set in the settings dialog
                            //p.DeviceType = "Rotator";
                            //if (!p.IsRegistered("ASCOM.AWR.Telescope"))  // added 2-28-16
                            //{
                            //    p.Register("ASCOM.scopefocus.Rotator", "ASCOM Rotator Driver for scopefocus");
                            //}

                            //        homeSet = p.GetValue(driverID, "HomeSet").ToLower().Equals("true") ? true : false;
                            // portName = p.GetValue(driverID, "ComPort");
                            //     portName = ASCOM.scopefocusServer.Properties.Settings.Default.COMPort;
                            //    portName = "COM4";
                            //   setPos = p.GetValue(driverID, "SetPos").ToLower().Equals("true") ? true : false;

                            // 6-16-16 added 2 lines below
                            //    reverse = p.GetValue(driverID, "Reverse").ToLower().Equals("true") ? true : false;
                            //      contHold = p.GetValue(driverID, "ContHold").ToLower().Equals("true") ? true : false;
                            //     setPos = ASCOM.scopefocusServer.Properties.Settings.Default.SetPos;
                            //if (setPos)
                            //    posValue = ASCOM.scopefocusServer.Properties.Settings.Default.SetPosValue;
                            // posValue = System.Convert.ToSingle(p.GetValue(driverID, "Pos"));



                            //     tempDisplay = p.GetValue(driverID, "TempDisp");
                            //    stepsPerDegree = Convert.ToInt32(p.GetValue(driverID, "StepsPerDegree"));
                            //    tl.LogMessage("Steps per degree:", stepsPerDegree.ToString());
                          

                            //blValue = System.Convert.ToInt32(p.GetValue(driverId, "BackLight"));

                            //*****temp rem until config is finished************


                            //if (string.IsNullOrEmpty(SharedResources.SharedSerial.PortName))
                            //{
                            //    // report a problem with the port name
                            //    throw new ASCOM.NotConnectedException("no Com port selected");
                            //}

                            //*** end temp rem



                            // try to connect using the port
                            //try
                            //{
                            //    log = new StreamWriter("c:\\log.txt");
                            //     tl.LogMessage("Connecting to serial port", "");

                            // setup the serial port.

                            //serialPort = new Serial();
                            //serialPort.PortName = portName;
                            //serialPort.Speed = SerialSpeed.ps9600;
                            //serialPort.StopBits = SerialStopBits.One;
                            //serialPort.Parity = SerialParity.None;
                            //serialPort.DataBits = 8;
                            //serialPort.DTREnable = false;


                            //if (!serialPort.Connected)
                            //    serialPort.Connected = true;


                            //// flush whatever is there.
                            //serialPort.ClearBuffers();


                            // wait for the Serial Port to come online...better way to do this???
                            //    System.Threading.Thread.Sleep(1000);


                            //  **********add setposition*********************

                            //   if the user is setting a position in the Settings dialog set it here.
                            //if (setPos)
                            //    rawCommand("P " + Math.Round(SetPosValue * stepsPerDegree + (360 * stepsPerDegree), 0).ToString(), false);   // was + 36000 not 360*stepsperdegree
                            // CommandString("P " + Math.Round(posValue * stepsPerDegree + 9000, 0).ToString() + "#", false);  //orig was M changed to P 10-18-2015 (want it to set the value not move)
                            //3-7-17 above also need to correct for user defined steps / degree (not just 100); 

                            // added 6-16-16 
                            //if (reverse)
                            //    CommandString("R 1#", false);
                            //else
                            //    CommandString("R 0#", false); // motor sitting shaft up turns clockwise with increasing numbers if NOT reversed

                            //if (contHold)
                            //    rawCommand("C 1", false); //continuous hold on
                            //else
                            //    rawCommand("C 0", false);

                            //// log the arduino firware version
                            //string ver = rawCommand("V", false);
                            //string verTrim = ver.Replace('#', ' ');
                            //string versn = verTrim.Replace('V', ' ').Trim();
                            //tl.LogMessage("Firmware Version: ", versn.ToString());
                            //   }

                            //catch (Exception ex)
                            //{
                            //    // report any error
                            //    throw new ASCOM.NotConnectedException("Serial port connectionerror", ex);
                            //}



                            //    if (!answer.Contains("QuidneArduino"))
                            //    {
                            //        MessageBox.Show("QuidneArduino device not detected at port " + SharedResources.SharedSerial.PortName, "Device not detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //        SharedResources.tl.LogMessage("Connected answer", "Wrong answer " + answer);
                            //    }
                            //}
                            //catch (System.IO.IOException exception)
                            //{
                            //    MessageBox.Show("QuidneArduino Serial port not opened for " + SharedResources.SharedSerial.PortName, "Invalid port state", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //    SharedResources.tl.LogMessage("Serial port not opened", exception.Message);
                            //}
                            //catch (System.UnauthorizedAccessException exception)
                            //{
                            //    MessageBox.Show("QuidneArduino Access denied to serial port " + SharedResources.SharedSerial.PortName, "Access denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //    SharedResources.tl.LogMessage("Access denied to serial port", exception.Message);
                            //}

                        }
                                        catch (ASCOM.DriverAccessCOMException exception)
                                        {
                                            MessageBox.Show("ASCOM driver exception: " + exception.Message, "ASCOM driver exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }

                              //  }




                                //  connectedState = true;
                                //  tl.LogMessage("Connected Set", "Connecting to port " + comPort);
                                // TODO connect to the device
                            }
                       
                            //else
                            //{
                            //    rawCommand("C 0", false); //release the continuous hold
                            //    System.Threading.Thread.Sleep(500);
                            //    //  Dispose();
                            //    connectedState = false;
                            //    tl.LogMessage("Connected Set", "Disconnecting from port " + COMPort);
                            //    if (SharedSerial.Port != null && SharedSerial.Connected)
                            //    {
                            //        //       CommandString("C 0#", false); //release the continuous hold
                            //        //       System.Threading.Thread.Sleep(500);
                            //        SharedSerial.Connected = false;
                            //        SharedSerial.Dispose();
                            //    //    serialPort = null;
                            //    }



                            //}









                            //catch (System.Runtime.InteropServices.COMException exception)
                            //{
                            //    MessageBox.Show("QuidneArduinoS erial port read timeout for port " + SharedResources.SharedSerial.PortName, "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //    SharedResources.tl.LogMessage("QuidneArduino Serial port read timeout", exception.Message);
                            //}
                        
                        s_z++;
                    }
                    else
                    {
                        s_z--;
                        if (s_z <= 0)
                        {
                            tl.LogMessage("Connection", "Disconnecting from port " + COMPort);
                            rawCommand("C 0", false); //release the continuous hold
                            tl.LogMessage("Continuous Hold On", "False");
                            System.Threading.Thread.Sleep(500);
                            SharedSerial.Connected = false;
                            traceLogger.Enabled = false;
                            traceLogger.Dispose();
                            traceLogger = null;
                        }
                    }
                }
            }
            get { return SharedSerial.Connected; }
        }



        public static string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        // added 10-27-2020 
        private static short interfaceVersion = 2;
        private static string driverShortName = "scopefocusServer Rotator";
        public static short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                tl.LogMessage(driverShortName + " InterfaceVersion Get", interfaceVersion.ToString());

                //LogMessage("InterfaceVersion Get", "2");
                return interfaceVersion;
            }
        }







        //set
        //{
        //    lock (lockObject)
        //    {
        //        if (value)
        //        {
        //            if (s_z == 0)
        //                SharedSerial.Connected = true;
        //            s_z++;
        //        }
        //        else
        //        {
        //            s_z--;
        //            if (s_z <= 0)
        //            {
        //                SharedSerial.Connected = false;
        //            }
        //        }
        //    }
        //}
        //get { return SharedSerial.Connected; }
        //  }







        public static void Halt()
        {
            rawCommand("S", false);
            //  tl.LogMessage("Halt", "Not implemented");
         //   throw new ASCOM.MethodNotImplementedException("Halt");
        }


        // added 10-27-2020
        //public static bool CanReverse
        //{
        //    get
        //    {
        //        //tl.LogMessage("CanReverse Get", true.ToString());
        //        return true;
        //    }
        //}
        // added 10-27-2020
        //private static bool _reverseState;
        //private static void Reverstate()
        //{
        //    string rev = rawCommand("R", false);

        //    if (rev == "1")
        //    {
        //        tl.LogMessage("ReverseState ", true.ToString());
        //        _reverseState = true;
        //    }
        //    else
        //    {
        //        tl.LogMessage("ReverseState ", false.ToString());
        //        _reverseState = false;
        //    }
        //}



        //public static bool ReverseState
        //{
            
        //    get
        //    {
        //        return _reverseState;

        //        //  tl.LogMessage("Reverse Get", "Not implemented");
        //        // throw new ASCOM.PropertyNotImplementedException("Reverse", false);
        //    }
        //    set
        //    {
        //        if (value != _reverseState)
        //        {
        //            if (value)
        //                rawCommand("R 1", false);
        //            else
        //                rawCommand("R 0", false);

        //        }

        //        //  tl.LogMessage("Reverse Set", "Not implemented");
        //        //  throw new ASCOM.PropertyNotImplementedException("Reverse", true);
        //    }
        //}



        public static bool IsMoving
        {
            get
            {
                DoUpdate();
                return lastMoving;
                // tl.LogMessage("IsMoving Get", false.ToString()); // This rotator has instantaneous movement
                //  return false;
            }
        }

       public static bool Link
        {
            get
            {
                long now = DateTime.Now.Ticks;
                if (now - lastL > UPDATETICKS)
                {
                    if (SharedSerial.PortName != null)  //was if (serialPort != null) 
                        lastLink = SharedSerial.Connected;  // was serialport.connected

                    lastL = now;
                    return lastLink;
                }

                return lastLink;
            }
            set
            {
                Connected = value;
            }


            /*
            get
            {
                tl.LogMessage("Link Get", this.Connected.ToString());
                return this.Connected; // Direct function to the connected method, the Link method is just here for backwards compatibility
            }
            set
            {
                tl.LogMessage("Link Set", value.ToString());
                this.Connected = value; // Direct function to the connected method, the Link method is just here for backwards compatibility
            }
             */
        }

        public static float StepSize
        {
            get
            {
                if (stepsPerDegree > 100)
                    return .01F;  // minimum of 0.01
                else
                    return 1F / stepsPerDegree; // since carrying out 3 decimla points doesn't work mult by 10

                //tl.LogMessage("StepSize Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("StepSize", false);
            }
        }

        public static float TargetPosition
        {
            get
            {
                return targetPosition;

                //tl.LogMessage("TargetPosition Get", rotatorPosition.ToString()); // This rotator has instantaneous movement
                //return rotatorPosition;
            }
        }


        private static float targetPosition = 0; // Absolute stepper position of the rotator (in steps)  
        public static float StepperPos
        {
            get
            {
                DoUpdate();
                return lastPos;

            }
        }

        public static float Position
        {
            get
            {

                DoUpdate();
                //rotatorPosition = lastPos;
                //return lastPos;
                //   return (lastPos - 9000) / 100 % 360;  // was get{}
                var pos = (lastPos - 360 * stepsPerDegree) / stepsPerDegree;
                if (pos < 0)
                    pos += 360.00F;
                return pos;

                //tl.LogMessage("Position Get", rotatorPosition.ToString()); // This rotator has instantaneous movement
                //return rotatorPosition;
            }
        }


        public static void MoveAbsolute(float pos)
        {


            var stepPosition = PositionAngleToMotorSteps(pos);
            targetPosition = pos;
            //   TargetPosition = pos;
            rawCommand("M " + Math.Round(stepPosition, 0), false);
         //   CommandString("M " + Math.Round(stepPosition, 0) + "#", false);  // Position was 'int value' for focuser  // corrects for 100 steps per degree, need to replace with user defined variable.  
            lastMoving = true;  //remd 1-12-15


            //tl.LogMessage("MoveAbsolute", Position.ToString()); // Move to this position
            //rotatorPosition = Position;
            //rotatorPosition = (float)astroUtilities.Range(rotatorPosition, 0.0, true, 360.0, false); // Ensure value is in the range 0.0..359.9999...
        }


        public static void Move(float pos)
        {
            double moveTo = StepperPos + RelativeAngleToMotorSteps(pos);  // current position in steps + number of steps needed to 
            targetPosition = pos;
            if (moveTo >= 720 * stepsPerDegree) // was 72000
                moveTo -= 360 * stepsPerDegree;
            if (moveTo < 0)
                moveTo += 360 * stepsPerDegree;
            rawCommand("M " + Math.Round(moveTo, 0), false);
         //   CommandString("M " + Math.Round(moveTo, 0) + "#", false);  // Position was 'int value' for focuser
            lastMoving = true;  //remd 1-12-15

            //tl.LogMessage("Move", Position.ToString()); // Move by this amount
            //rotatorPosition += Position;
            //rotatorPosition = (float)astroUtilities.Range(rotatorPosition, 0.0, true, 360.0, false); // Ensure value is in the range 0.0..359.9999...
        }

        private static double RelativeAngleToMotorSteps(float angle)
        {
            var targetSteps1 = angle % 360.00F * stepsPerDegree;
            return targetSteps1;
        }
        private static double PositionAngleToMotorSteps(float targetPositionAngle)
        {
            float targetAngle = 0;
            var absTargetAngle1 = targetPositionAngle + 360;
            var absTargetAngle2 = targetPositionAngle;
            var angleDelta1 = absTargetAngle1 - StepperPos / stepsPerDegree;
            var angleDelta2 = StepperPos / stepsPerDegree - absTargetAngle2;
            if (absTargetAngle1 < 0 || absTargetAngle1 > 720)
            {
                targetAngle = absTargetAngle2;
                if (targetAngle < 0 || targetAngle > 720)
                {
                    tl.LogMessage("Invalid Position value: ", targetAngle.ToString());
                    throw new ASCOM.InvalidValueException();
                }
                else
                    return targetAngle * stepsPerDegree;
            }
            if (absTargetAngle2 < 0 || absTargetAngle2 > 720)
            {
                targetAngle = absTargetAngle1;
                targetAngle = absTargetAngle2;
                if (targetAngle < 0 || targetAngle > 720)
                {
                    tl.LogMessage("Invalid Position value: ", targetAngle.ToString());
                    throw new ASCOM.InvalidValueException();
                }
                else
                    return targetAngle * stepsPerDegree;
            }



            if (angleDelta1 < angleDelta2)
            {
                // if target is close to 0 or 72000 AND the move is < 90 degrees then go there(acceptable if close)...otherwise want to stay close to 36000
                if ((absTargetAngle1 < 90 && angleDelta1 < 90) || (absTargetAngle1 > 630 && angleDelta1 < 90))
                    targetAngle = absTargetAngle1;
                else
                    targetAngle = absTargetAngle2;
                if (absTargetAngle1 >= 90 && absTargetAngle1 <= 630) // if outside the close to 0/72000 zone then use smaller delta
                    targetAngle = absTargetAngle1;


            }
            else // delta 2 is smaller so in general use it unless within 90 of 0/72000 OR if close then ok 
            {
                if ((absTargetAngle2 < 90 && angleDelta2 < 90) || (absTargetAngle2 > 630 && angleDelta2 < 90))
                    targetAngle = absTargetAngle2;
                else
                    targetAngle = absTargetAngle1;
                if (absTargetAngle2 >= 90 && absTargetAngle2 <= 630)
                    targetAngle = absTargetAngle2;

            }
            return targetAngle * stepsPerDegree;
        }


        public static object Properties { get; private set; }


        #endregion

        #region Multi Driver handling
        // this section illustrates how multiple drivers could be handled,
        // it's for drivers where multiple connections to the hardware can be made and ensures that the
        // hardware is only disconnected from when all the connected devices have disconnected.

        // It is NOT a complete solution!  This is to give ideas of what can - or should be done.
        //
        // An alternative would be to move the hardware control here, handle connecting and disconnecting,
        // and provide the device with a suitable connection to the hardware.
        //
        /// <summary>
        /// dictionary carrying device connections.
        /// The Key is the connection number that identifies the device, it could be the COM port name,
        /// USB ID or IP Address, the Value is the DeviceHardware class
        /// </summary>
        private static Dictionary<string, DeviceHardware> connectedDevices = new Dictionary<string, DeviceHardware>();




        /// <summary>
        /// List of connected devices, keyed by a string ID
        /// </summary>
        //public static IDictionary<string, ConnectedDevice> ConnectedDevices
        //{
        //    get { return connectedDevices; }
        //}
        /// <summary>
        /// This is called in the driver Connect(true) property,
        /// it add the device id to the list of devices if it's not there and increments the device count.
        /// </summary>
        /// <param name="deviceId"></param>
        public static void Connect(string deviceId)
        {
            lock (lockObject)
            {
                if (!connectedDevices.ContainsKey(deviceId))
                    connectedDevices.Add(deviceId, new DeviceHardware());
                connectedDevices[deviceId].count++;       // increment the value
            }
        }

        public static void Disconnect(string deviceId)
        {
            lock (lockObject)
            {
                if (connectedDevices.ContainsKey(deviceId))
                {
                    connectedDevices[deviceId].count--;
                    if (connectedDevices[deviceId].count <= 0)
                        connectedDevices.Remove(deviceId);
                }
            }
        }

        ////unremd 4-27-17
        //public static bool IsConnected(string deviceId)
        //{
        //    if (connectedDevices.ContainsKey(deviceId))
        //        return (connectedDevices[deviceId].count > 0);
        //    else
        //        return false;
        //}



     //   remd 4-27-17
        public static bool IsConnected
        {
            get
            {
                if (SharedSerial.Connected)
                    return true;
                else
                    return false;
            }
        }



        /// <summary>
        /// Gives the camera handle and the number of connections
        /// </summary>
        //public class ConnectedDevice
        //{
        //    public ConnectedDevice(IntPtr handle)
        //    {
        //        this.handle = handle;
        //        this.connections = 1;
        //    }
        //    public IntPtr handle;
        //    public int connections;
        //}

        //public class DeviceHardware

        //{
        //   internal int count { set; get; }

        //    internal DeviceHardware()
        //    {
        //        count = 0;
        //    }
        //}





        #endregion



        /// <summary>
        /// Skeleton of a hardware class, all this does is hold a count of the connections,
        /// in reality extra code will be needed to handle the hardware in some way
        /// </summary>

        public class DeviceHardware
        {
            internal int count { set; get; }

            internal DeviceHardware()
            {
                count = 0;
            }
        }


        //public static string rawCommand(string function, string command)
        //{
        //    return rawCommand(function, command, false);
        //}

        //public static string rawCommand(string function, string command, bool raw)
        public static string rawCommand(string command, bool raw)
        {
            try
            {
                string answer = SharedResources.SendMessage(command);
                if (raw)
                {
                    return answer.Trim();
                }
                else
                {
                    return answer.Substring(2).Trim();
                }

            }
            catch (System.TimeoutException e)
            {
                tl.LogMessage("Arduino Timeout exception", e.Message);
            }
            catch (ASCOM.Utilities.Exceptions.SerialPortInUseException e)
            {
                tl.LogMessage("Arduino Serial port in use exception", "Command: " + command + ", " + e.Message);
            }
            catch (ASCOM.NotConnectedException e)
            {
                tl.LogMessage("Arduino Not connected exception", e.Message);
            }
            catch (ASCOM.DriverException e)
            {
                tl.LogMessage("Arduino Driver exception", e.Message);
            }

            return String.Empty;
        }

        private static void DoUpdate()
        {
            // only allow access for "gets" once per second.
            // if inside of 1 second the buffered value will be used.
            if (DateTime.Now.Ticks > UPDATETICKS + lastUpdate)
            {
                lastUpdate = DateTime.Now.Ticks;


                // focuser returns a string like:
                // m:false;s:1000;t:25.20$
                //   m - denotes moving or not
                //   s - denotes the position in steps
                //   t - denotes the temperature, always in C

                
              //  String val = CommandString("G#", false);
              //  String val = SharedResources.rawCommand("G", false);
                String val = rawCommand("G", false);

                // split the values up.  Ideally you should check for null here.  
                // if something goes wrong this will throw an exception...no bueno...


                //focuser sends P 200;M true#  for e.g.

                String[] vals = val.Replace('#', ' ').Trim().Split(';');

                string valTrim = vals[0].Replace('#', ' ');
                string pos = valTrim.Replace('P', ' ').Trim();
                // these values are used in the "Get" calls.  That way the client gets an immediate
                // response.  However it may up to 1 second out of date.
                // Thus "lastMoving" must be set to true when the move is initiated in "Move"

                lastPos = Convert.ToSingle(pos);  // raw stepper position in 'steps' from 0 (which is -90 degrees) 
                //    lastMoving = false;
                lastMoving = vals[1].Substring(2) == "true" ? true : false;  //*** remd 1-12-15
                //   *** 1-12-15  to implement this need to change arduino code to retrun something liek "M:True" 
                //   *** like example above line 640, then slipt ther string into an array and decifer them



                //    lastPos = Convert.ToInt16(vals[1].Substring(2));
                //    lastTemp = Convert.ToDouble(vals[2].Substring(2));
            }
        }

        public static void RunSetupDialog()
        {
          //  tl.LogMessage("Setup", "run setup dialog");
            using (var dialogForm = new ServerSetupDialog())
            {
                var result = dialogForm.ShowDialog();
                switch (result)
                {
                    case DialogResult.OK:
                     //   tl.LogMessage("Setup", "setup complete, sdaving settings");
                        // ****************    to do   save settings

                        break;
                    default:
                     //   tl.LogMessage("Setup", "Setup failed or cancelled");
                        break;
                }

            }

            }

    
      


    }


   





    //#region ServedClassName attribute
    ///// <summary>
    ///// This is only needed if the driver is targeted at  platform 5.5, it is included with Platform 6
    ///// </summary>
    //[global::System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    //public sealed class ServedClassNameAttribute : Attribute
    //{
    //    // See the attribute guidelines at 
    //    //  http://go.microsoft.com/fwlink/?LinkId=85236

    //    /// <summary>
    //    /// Gets or sets the 'friendly name' of the served class, as registered with the ASCOM Chooser.
    //    /// </summary>
    //    /// <value>The 'friendly name' of the served class.</value>
    //    public string DisplayName { get; private set; }
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ServedClassNameAttribute"/> class.
    //    /// </summary>
    //    /// <param name="servedClassName">The 'friendly name' of the served class.</param>
    //    public ServedClassNameAttribute(string servedClassName)
    //    {
    //        DisplayName = servedClassName;
    //    }
    //}
    //#endregion
}
