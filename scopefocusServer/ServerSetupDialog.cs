using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using ASCOM.DeviceInterface;
using ASCOM.Utilities;

namespace ASCOM.scopefocusServer
{
    public partial class ServerSetupDialog : Form
    {
        public ServerSetupDialog()
        {
            InitializeComponent();
            InitUI();
        }


        private static string driverID = "ASCOM.scopefocusServer.Rotator";
        private static bool traceState = false;
        private static string comPort = "";

        //private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        //{
        //    MessageBox.Show("TEST");
        //    if (textBox2.Text == "")
        //    {
        //        MessageBox.Show("You must specify a value for Steps per Degree");

        //        return;
        //    }
        //    else
        //    {

              
        //        using (ASCOM.Utilities.Profile p = new Utilities.Profile())
        //        {


        //            p.DeviceType = "Rotator";
        //            p.WriteValue(driverID, "ComPort", (string)comboBoxComPort.SelectedItem);
        //            p.WriteValue(driverID, "SetPos", checkBox1.Checked.ToString());
        //            // 6-16-16 added 2 lines below
        //            //   p.WriteValue(Rotator.driverID, "Reverse", reverseCheckBox1.Checked.ToString());  // motor sitting shaft up turns clockwise with increasing numbers if NOT reversed
        //            p.WriteValue(driverID, "ContHold", checkBox2.Checked.ToString());


        //            p.WriteValue(driverID, "StepsPerDegree", textBox2.Text.ToString());
        //            //   p.WriteValue(Focuser.driverID, "RPM", textBoxRpm.Text);
        //            if (checkBox1.Checked)
        //            {
        //                p.WriteValue(driverID, "Pos", textBox1.Text.ToString());
        //            }
        //            //    p.WriteValue(Focuser.driverID, "TempDisp", radioCelcius.Checked ? "C" : "F");
        //        }
        //        Dispose();




        //        // Place any validation constraint checks here
        //        // Update the state variables with results from the dialogue



        //        //   Rotator.comPort = (string)comboBoxComPort.SelectedItem;
        //        comPort = (string)comboBoxComPort.SelectedItem;
        //        // Rotator.traceState = chkTrace.Checked;
        //        traceState = chkTrace.Checked;
        //    }

        //    ASCOM.scopefocusServer.Properties.Settings.Default.stepsPerDegree = Convert.ToInt16(textBox2.Text);
        //    ASCOM.scopefocusServer.Properties.Settings.Default.COMPort = comPort;
        //    ASCOM.scopefocusServer.Properties.Settings.Default.traceState = traceState;
        //               ASCOM.scopefocusServer.Properties.Settings.Default.Save();


        //    // Place any validation constraint checks here
        //    // Update the state variables with results from the dialogue
        //    //Rotator.comPort = (string)comboBoxComPort.SelectedItem;
        //    //Rotator.tl.Enabled = chkTrace.Checked;
        //}

        //private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        //{
        //    Close();
        //}

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }
        private void checkTextBox() //don't allow close with steps/dgree blank or set pos checked and blank position
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
                cmdOK.Enabled = false;
            else if ((string.IsNullOrWhiteSpace(textBox1.Text) && checkBox1.Checked))
                cmdOK.Enabled = false;
            else
                cmdOK.Enabled = true;
        }



        public static object Properties { get; private set; }

        private void InitUI()
        {

            // chkTrace.Checked = Rotator.traceState;
            traceState = ASCOM.scopefocusServer.Properties.Settings.Default.traceState;
            chkTrace.Checked = traceState;

            // set the list of com ports to those that are currently available
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());      // use System.IO because it's static


            // select the current port if possible
            //if (comboBoxComPort.Items.Contains(Rotator.comPort))
            //{
            //    comboBoxComPort.SelectedItem = Rotator.comPort;
            //}
            comPort = ASCOM.scopefocusServer.Properties.Settings.Default.COMPort;
            if (comboBoxComPort.Items.Contains(comPort))
            {
                comboBoxComPort.SelectedItem = comPort;
            }

            using (ASCOM.Utilities.Profile p = new Utilities.Profile())
            {
                p.DeviceType = "Rotator";
                textBox2.Text = p.GetValue(driverID, "StepsPerDegree");
                if (p.GetValue(driverID, "ContHold") == "True")
                    checkBox2.Checked = true;
                else
                    checkBox2.Checked = false;
            }
            checkBox1.Checked = false;
            
            if (!checkBox1.Checked)
                textBox1.Enabled = false;
            checkTextBox();
            //chkTrace.Checked = Rotator.tl.Enabled;
            //// set the list of com ports to those that are currently available
            //comboBoxComPort.Items.Clear();
            //comboBoxComPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());      // use System.IO because it's static
            //// select the current port if possible
            //if (comboBoxComPort.Items.Contains(Rotator.comPort))
            //{
            //    comboBoxComPort.SelectedItem = Rotator.comPort;
            //}
        }

        //private void chkTrace_CheckedChanged(object sender, EventArgs e)
        //{
        //    //if (chkTrace.Checked)
        //    //    Rotator.traceState = true;
        //    //else
        //    //    Rotator.traceState = false;

        //    if (chkTrace.Checked)
        //        traceState = true;
        //    else
        //        traceState = false;
        //}

        //private void textBox2_TextChanged(object sender, EventArgs e)
        //{
        //    checkTextBox();
        //}

        //private void textBox1_TextChanged(object sender, EventArgs e)
        //{
        //    checkTextBox();
        //}

        //private void checkBox1_CheckedChanged(object sender, EventArgs e)
        //{
        //    bool enable = false;
        //    if (checkBox1.Checked)
        //        enable = true;


        //    //  label2.Enabled = enable;
        //    textBox1.Enabled = enable;
        //    checkTextBox();
        //}

        private static string traceStateProfileName = "Trace Level";
        private static string comPortProfileName = "COMPort"; // Constants used for Profile persistence
        private static string comPortDefault = "COM4";
        private static string traceStateDefault = "false";
        private static int stepsPerDegree;
        private static float setPosition;
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Rotator";
               SharedResources.tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Rotator";
                driverProfile.WriteValue(driverID, traceStateProfileName, SharedResources.tl.Enabled.ToString());
                driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
            }
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            bool enable = false;
            if (checkBox1.Checked)
                enable = true;


            //  label2.Enabled = enable;
            textBox1.Enabled = enable;
            checkTextBox();
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            checkTextBox();
        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            checkTextBox();
        }

        private void chkTrace_CheckedChanged_1(object sender, EventArgs e)
        {
            if (chkTrace.Checked)
                traceState = true;
            else
                traceState = false;
        }

        private void cmdCancel_Click_1(object sender, EventArgs e)
        {
            Close();
        }

        private void cmdOK_Click_1(object sender, EventArgs e)
        {
         
            if (textBox2.Text == "")
            {
                MessageBox.Show("You must specify a value for Steps per Degree");

                return;
            }
            else
            {


                using (ASCOM.Utilities.Profile p = new Utilities.Profile())
                {


                    p.DeviceType = "Rotator";
                    p.WriteValue(driverID, "ComPort", (string)comboBoxComPort.SelectedItem);
               //     p.WriteValue(driverID, "SetPos", checkBox1.Checked.ToString());


                    //p.WriteValue(driverID, "SetPos", "false");  //leave this false so needs to be check everytime used
                    
                    
                    // 6-16-16 added 2 lines below
                    //   p.WriteValue(Rotator.driverID, "Reverse", reverseCheckBox1.Checked.ToString());  // motor sitting shaft up turns clockwise with increasing numbers if NOT reversed
                    p.WriteValue(driverID, "ContHold", checkBox2.Checked.ToString());


                    p.WriteValue(driverID, "StepsPerDegree", textBox2.Text.ToString());
                    //   p.WriteValue(Focuser.driverID, "RPM", textBoxRpm.Text);
                    if (checkBox1.Checked)
                    {
                        p.WriteValue(driverID, "Pos", textBox1.Text.ToString());
                    }
                    //    p.WriteValue(Focuser.driverID, "TempDisp", radioCelcius.Checked ? "C" : "F");
                }
           //     Dispose();




                // Place any validation constraint checks here
                // Update the state variables with results from the dialogue



                //   Rotator.comPort = (string)comboBoxComPort.SelectedItem;
                comPort = (string)comboBoxComPort.SelectedItem;
                // Rotator.traceState = chkTrace.Checked;
                traceState = chkTrace.Checked;
                stepsPerDegree = Convert.ToInt32(textBox2.Text.ToString());
                if (checkBox1.Checked)
                setPosition = float.Parse(textBox1.Text.ToString());
              //  stepsPerDegree = 150;
            }

            ASCOM.scopefocusServer.Properties.Settings.Default.stepsPerDegree = stepsPerDegree;
            ASCOM.scopefocusServer.Properties.Settings.Default.COMPort = comPort;
            ASCOM.scopefocusServer.Properties.Settings.Default.traceState = traceState;
            ASCOM.scopefocusServer.Properties.Settings.Default.ContHold = checkBox2.Checked;
              ASCOM.scopefocusServer.Properties.Settings.Default.SetPos = checkBox1.Checked;
            ASCOM.scopefocusServer.Properties.Settings.Default.SetPosValue = setPosition;
            ASCOM.scopefocusServer.Properties.Settings.Default.Save();


            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue
            //Rotator.comPort = (string)comboBoxComPort.SelectedItem;
            //Rotator.tl.Enabled = chkTrace.Checked;
        }
    }
}
