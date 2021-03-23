﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetCheck.Models;
using DotNetCheck.Solutions;

namespace DotNetCheck.Checkups
{
	public class AndroidSdkLicensesCheckup : Checkup
	{
		public AndroidSdkLicensesCheckup()
		{
		}

		public override string Id => "androidsdklicenses";

		public override IEnumerable<CheckupDependency> Dependencies
			=> new List<CheckupDependency> { new CheckupDependency("androidsdk") };

		public override string Title => "Android SDK - Licenses Acceptance";

		public FileInfo SdkManagerPath { get; private set; }

		public override Task<DiagnosticResult> Examine(SharedState history)
		{
			var android = new AndroidSdk.AndroidSdkManager(
				history.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? history.GetEnvironmentVariable("ANDROID_HOME"));

			try
			{

				var v = android.SdkManager.RequiresLicenseAcceptance();

				if (!v)
				{
					ReportStatus($"All licenses accepted.", Status.Ok);
					return Task.FromResult(DiagnosticResult.Ok(this));
				}
			}
			catch { }

			ReportStatus("One or more Licenses are not accepted.", Status.Error);

			var ext = Util.IsWindows ? ".bat" : string.Empty;

			var sdkMgrPath = android.SdkManager.FindToolPath(android.Home)?.FullName;

			if (string.IsNullOrEmpty(sdkMgrPath))
				sdkMgrPath = $"sdkmanager{ext}";

			return Task.FromResult(
				new DiagnosticResult(
					Status.Error,
					this,
					new Suggestion(
						"Read and accept Android SDK licenses.",
@$"Your Android SDK has licenses which are unread or unaccepted.
You can use the Android SDK Manager to read and accept them.
For more information see: [underline]https://aka.ms/dotnet-androidsdk-help[/]",
						new ActionSolution(t =>
						{
							android.SdkManager.AcceptLicensesInteractive();
							return Task.CompletedTask;
						}))));

			//$"To read and accept Android SDK licenses, run the following command in a terminal:{Environment.NewLine}    {sdkMgrPath} --licenses")));
		}
	}
}