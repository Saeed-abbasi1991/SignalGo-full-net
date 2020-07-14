﻿using MvvmGo.ViewModels;
using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Extensions;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Views.Extra;
using System.Collections.Generic;

namespace SignalGo.Publisher.Helpers
{
    /// <summary>
    /// Do Authorization before Executing Command on protected Server's, using Interactive Dialog And automatic Method's
    /// </summary>
    public class CommandAuthenticator : BaseViewModel
    {
        public CommandAuthenticator() : base()
        {

        }
        private static int retryAttemp { get; set; } = 0;
        /// <summary>
        /// Interactive Authorization on the specified server
        /// </summary>
        /// <param name="serverInfo"></param>
        /// <returns></returns>
        public static bool Authorize(ref ServerInfo serverInfo)
        {
        //if (serverInfo.HasValue())
        //{
        //    if (serverInfo.ProtectionPassword != null)
        //    {
        GetThePass:
            if (retryAttemp > 2)
            {
                retryAttemp = 0;
                return false;
            }
            InputDialogWindow inputDialog = new InputDialogWindow($"Please enter your secret for server {serverInfo.ServerName}:");
            if (inputDialog.ShowDialog() == true)
            {
                if (serverInfo.ProtectionPassword != PasswordEncoder.ComputeHash(inputDialog.Answer))
                        {
                    if (System.Windows.Forms.MessageBox.Show("password does't match!", "Access Denied", System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    {
                        retryAttemp++;
                        goto GetThePass;
                    }
                    else
                    {
                        serverInfo.IsChecked = false;
                        serverInfo.ServerLastUpdate = "Access Denied!";
                    }
                }
            }
            else return false;
            //    }
            //}
            //else
            //{
            //    return false;
            //}

            return true;
        }

    }
}
