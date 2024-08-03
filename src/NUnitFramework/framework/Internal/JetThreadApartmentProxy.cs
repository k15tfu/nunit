// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// Thread Apartments APIs wrapper for compatibility with .NET Core on Unix.
    /// On platforms with native thread apartment model support (.NET Framework, .NET Core WindowsDesktop.App) it's just a proxy to platform APIs
    /// and on other platforms it emulates STA threads for our Dispatcher.
    /// </summary>
    internal static class JetThreadApartmentProxy
    {
        private static readonly Logger log = InternalTrace.GetLogger("JetThreadApartmentProxy");
        // private static readonly string JetThreadApartmentImpl = Environment.GetEnvironmentVariable("JetThreadApartmentImpl");
        // private static readonly string JetThreadApartmentImpl = "/home/user/p/dotnet-products/1/dotnet/Bin.RiderBackend/JetBrains.Platform.Core.dll";
        // private static readonly Type JetThreadApartmentType = JetThreadApartmentImpl != null ? Assembly.LoadFrom(JetThreadApartmentImpl).GetType("JetBrains.Util.Concurrency.JetThreadApartment", true) : null;
        private static readonly Type JetThreadApartmentType = Assembly.Load("JetBrains.Platform.Core").GetType("JetBrains.Util.Concurrency.JetThreadApartment", true);
        private static readonly MethodInfo JetThreadApartmentType_STAThread = JetThreadApartmentType?.GetMethod(nameof(STAThread), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo JetThreadApartmentType_SetJetApartmentState = JetThreadApartmentType?.GetMethod(nameof(SetJetApartmentState), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo JetThreadApartmentType_GetJetApartmentState = JetThreadApartmentType?.GetMethod(nameof(GetJetApartmentState), BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// Emulates <see cref="STAThreadAttribute"/> on platforms without native thread apartment model support.
        /// Must be called only from methods marked with <see cref="STAThreadAttribute"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void STAThread()
        {
            if (JetThreadApartmentType_STAThread != null)
                JetThreadApartmentType_STAThread.Invoke(null, null);
        }

        /// <inheritdoc cref="Thread.SetApartmentState"/>
        /// <summary>
        /// Use this method instead of <see cref="Thread.SetApartmentState"/>
        /// </summary>
        public static void SetJetApartmentState(this Thread thread, ApartmentState state)
        {
            // log.Warning($"JetThreadApartmentImpl = {JetThreadApartmentImpl}");
            log.Warning($"JetThreadApartmentType = {JetThreadApartmentType}");
            log.Warning($"JetThreadApartmentType_SetJetApartmentState = {JetThreadApartmentType_SetJetApartmentState}");
            if (JetThreadApartmentType_SetJetApartmentState != null)
                JetThreadApartmentType_SetJetApartmentState.Invoke(null, new object[] { thread, state });
            else
                thread.SetApartmentState(state);
        }

        /// <inheritdoc cref="Thread.GetApartmentState"/>
        /// <summary>
        /// Use this method instead of <see cref="Thread.GetApartmentState"/>
        /// </summary>
        public static ApartmentState GetJetApartmentState(this Thread thread)
        {
            if (JetThreadApartmentType_GetJetApartmentState != null)
                return (ApartmentState)JetThreadApartmentType_GetJetApartmentState.Invoke(null, new object[] { thread });
            else
                return thread.GetApartmentState();
        }
    }
}
