﻿using Microsoft.Win32;
using System;

namespace TweakUIX.Tweaks.Privacy
{
    internal class DiagnosticData : TweaksBase
    {
        private static readonly ErrorHelper logger = ErrorHelper.Instance;

        private const string keyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy";
        private const int desiredValue = 0;

        public override string ID()
        {
            return "Diagnostic data";
        }

        public override string Info()
        {
            return "This will turn off tailored experiences with relevant tips and recommendations by using your diagnostics data. Many people would call this telemetry, or even spying.";
        }

        public override bool CheckTweak()
        {
            return !(
       RegistryHelper.IntEquals(keyName, "TailoredExperiencesWithDiagnosticDataEnabled", desiredValue)
            );
        }

        public override bool DoTweak()
        {
            try
            {
                Registry.SetValue(keyName, "TailoredExperiencesWithDiagnosticDataEnabled", desiredValue, RegistryValueKind.DWord);

                logger.Log("- Diagnostic data has been successfully disabled.");
                logger.Log(keyName);
                return true;
            }
            catch (Exception ex)
            { logger.Log("Could not disable Diagnostic data {0}", ex.Message); }

            return false;
        }

        public override bool UndoTweak()
        {
            try
            {
                Registry.SetValue(keyName, "TailoredExperiencesWithDiagnosticDataEnabled", 1, RegistryValueKind.DWord);
                logger.Log("- Diagnostic data has been successfully enabled.");
                return true;
            }
            catch
            { }

            return false;
        }
    }
}