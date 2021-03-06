﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;
using CryptoPriceChecker.GUI_Description;
using CryptoPriceChecker;

namespace LTCPriceChecker
{
    public partial class Form1 : Form
    {

        private String ltcWEB, btcWEB;
        private float LTCValuePLN, userLTCAmount, userLTCValuePLN, BTCValuePLN, userBTCAmount, userBTCValuePLN;
        private bool allwaysOnTopState;
        private WebClient LTCwebClient, BTCwebClient;

        private Thread retakeValue;

        public Form1()
        {
            InitializeComponent();

            labelPutYourLTCAmount.Text = GuiDesc.labelPutYourLTCAmountText;
            labelPutYourBTCAmount.Text = GuiDesc.labelPutYourBTCAmountText;
            buttonChangeOptions.Text = GuiDesc.changeOptionsText;

            allwaysOnTopState = true;

            userLTCAmount = 0;
            LTCValuePLN = 0;
            userLTCValuePLN =0;

            userBTCAmount = 0;
            BTCValuePLN = 0;
            userBTCValuePLN = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            getValuesOfCrypto();
            convertUserCryptoToPLN();
            changeTopMostState();
            //todo by program
        }

        //creating and connecting web client, then values from web
        private void getValuesOfCrypto()
        {
            LTCwebClient = new WebClient();
            BTCwebClient = new WebClient();

            using (LTCwebClient)
            {
                try
                {
                    ltcWEB = LTCwebClient.DownloadString("https://bitmarket24.pl/api/LTC_PLN/transactions.json");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            using (BTCwebClient)
            {                
                try
                {
                    btcWEB = BTCwebClient.DownloadString("https://bitmarket24.pl/api/BTC_PLN/transactions.json");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            //getting decimals from website
            MatchCollection ltc1 = Regex.Matches(ltcWEB, @"\d+((.|,)\d)+", RegexOptions.Singleline);

            int l = 0;
            foreach (Match ltc in ltc1)
            {
                float.TryParse(ltc.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out LTCValuePLN);
                l++;
                if (l == 1) break; //i = 1 is the newest price same in BTC \/
            }

            MatchCollection btc1 = Regex.Matches(btcWEB, @"\d+((.|,)\d)+", RegexOptions.Singleline);
            int b = 0;
            foreach (Match btc in btc1)
            {
                float.TryParse(btc.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out BTCValuePLN);
                b++;
                if (b == 1) break;
            }
        }

        //recalculating user LTC and BTC from value in pln
        private void convertUserCryptoToPLN()
        {
            retakeValue = new Thread(
                new ThreadStart(() =>
                {
                    float oldBTCValue, oldLTCValue;

                    for (; ;)
                    {

                        oldBTCValue = BTCValuePLN;
                        oldLTCValue = LTCValuePLN;

                        Thread.Sleep(1000 * 10); // taking thread to pause for 10 second

                        getValuesOfCrypto();                        

                        try
                        {
                            userLTCAmount = Single.Parse(textBoxuserLTCAmount.Text);
                            userBTCAmount = Single.Parse(textBoxuserBTCAmount.Text);
                        }
                        catch (Exception exception)
                        {

                            MessageBox.Show("Type your value in correct format: \"12,34\"");

                        }

                        userLTCValuePLN = LTCValuePLN * userLTCAmount;
                        userBTCValuePLN = BTCValuePLN * userBTCAmount;

                        Invoke(new Action(() =>
                        {
                            //⮟
                            //⮝
                            richTextBox1.Text = "LTC Price in PLN";
                            if (oldLTCValue > LTCValuePLN) richTextBox1.Text += " ⮟"; //showing user if the crypto cuorse goes up or down
                            else if (oldLTCValue < LTCValuePLN) richTextBox1.Text += " ⮝";
                            else richTextBox1.Text += "";
                            richTextBox1.Text += " = " + LTCValuePLN;

                            richTextBox1.Text += "\nBTC Price in PLN";
                            if (oldBTCValue > BTCValuePLN) richTextBox1.Text += " ⮟";
                            else if (oldBTCValue < BTCValuePLN) richTextBox1.Text += " ⮝";
                            else richTextBox1.Text += "";
                            richTextBox1.Text += " = " + BTCValuePLN;

                            richTextBox1.Text += "\nYour LTC value in PLN = " + userLTCValuePLN.ToString();
                            richTextBox1.Text += "\nYour BTC value in PLN = " + userBTCValuePLN.ToString();
                        }));
                    }
                }
                ));
            retakeValue.Start();
        }


        private void changeTopMostState()
        {
            if (allwaysOnTopState == true)
            {
                TopMost = true;
                checkBoxTest.Checked = true;
            }
            else
            {
                TopMost = false; //allways on top of screen if true
                checkBoxTest.Checked = false;
            }
        }

        private void buttonChangeOptions_Click(object sender, EventArgs e)
        {
            showFormOptions();
        }

        private void showFormOptions()
        {
            FormOptions formOptions = new FormOptions();

            formOptions.SetallwaysOnTopState(allwaysOnTopState);

            if (formOptions.ShowDialog(this) == DialogResult.OK)
            {
                this.allwaysOnTopState = formOptions.GetallwaysOnTopState;
            }
            else
            {
                return;
            }
            formOptions.Dispose();
            changeTopMostState();
        }
    }
}
