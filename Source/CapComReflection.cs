using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Contracts;
using CapCom.Framework;

namespace CapCom
{
    public static class CapComReflection
	{
		internal static void loadMethods()
		{
			ccLoaded = loadCCLimitsMethod();
			ccLoaded = loadCCAcceptMethod();
		}

		private static bool ccLoaded;

		public static bool CCLoaded
		{
			get { return ccLoaded; }
		}

		private const string CCTypeCC = "ContractConfigurator.ContractConfigurator";
		private const string CCLimitsName = "ContractLimit";
		private const string CCAcceptName = "CanAccept";

		private delegate int CCLimits(Contract.ContractPrestige p);
		private delegate bool CCAccept(Contract c);

		private static CCLimits _CCLimits;
		private static CCAccept _CCAccept;

		internal static int prestigeLimits(Contract.ContractPrestige p)
		{
			if (_CCLimits == null)
				return 100;

			return _CCLimits(p);
		}

		internal static bool canAccept(Contract c)
		{
			if (_CCAccept == null)
				return true;

			return _CCAccept(c);
		}

		private static bool loadCCLimitsMethod()
		{
			try
			{
				Type CConfigType = null;
				AssemblyLoader.loadedAssemblies.TypeOperation(t =>
				{
					if (t.FullName == CCTypeCC)
					{
						CConfigType = t;
					}
				});

				if (CConfigType == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Type [{0}] Not Found", CCTypeCC);
					return false;
				}

				MethodInfo CCLimitsMethod = CConfigType.GetMethod(CCLimitsName, new Type[] { typeof(Contract.ContractPrestige) });

				if (CCLimitsMethod == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Method [{0}] Not Loaded", CCLimitsName);
					return false;
				}

				_CCLimits = (CCLimits)Delegate.CreateDelegate(typeof(CCLimits), CCLimitsMethod);

				CC_MBE.LogFormatted("Contract Configurator Contract Limits Method Assigned");

				return _CCLimits != null;
			}
			catch (Exception e)
			{
				CC_MBE.LogFormatted("Error in loading Contract Configurator methods\n{0}", e);
				return false;
			}
		}

		private static bool loadCCAcceptMethod()
		{
			try
			{
				Type CConfigType = null;
				AssemblyLoader.loadedAssemblies.TypeOperation(t =>
				{
					if (t.FullName == CCTypeCC)
					{
						CConfigType = t;
					}
				});

				if (CConfigType == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Type [{0}] Not Found", CCTypeCC);
					return false;
				}

				MethodInfo CCAcceptMethod = CConfigType.GetMethod(CCAcceptName, new Type[] { typeof(Contract) });

				if (CCAcceptMethod == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Method [{0}] Not Loaded", CCAcceptName);
					return false;
				}

				_CCAccept = (CCAccept)Delegate.CreateDelegate(typeof(CCAccept), CCAcceptMethod);

				CC_MBE.LogFormatted("Contract Configurator Contract Can Accept Method Assigned");

				return _CCAccept != null;
			}
			catch (Exception e)
			{
				CC_MBE.LogFormatted("Error in loading Contract Configurator methods\n{0}", e);
				return false;
			}
		}
	}
}
